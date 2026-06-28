using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Models.ChatConversationDtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Business.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly IChatClient _baseChatClient;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IChatHistoryCache _historyCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _ollamaEndpoint;
        private readonly string _ollamaModel;
        private readonly string _ollamaClassifierModel;

        // 🎯 Note: The local _chatHistory list has been removed entirely to stop state tracking leaks!

        public ChatBotService(
            IChatClient chatClient,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IChatHistoryCache historyCache,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            // 1. Keep the base client clean. We will build the function pipeline dynamically inside the method.
            _baseChatClient = chatClient;

            // 2. Inject the thread-safe Factory instead of a sticky, persistent database context.
            _contextFactory = contextFactory;

            _historyCache = historyCache;
            _httpClientFactory = httpClientFactory;
            _ollamaEndpoint = configuration["AI:Ollama:Endpoint"] ?? "http://localhost:11434";
            _ollamaModel = configuration["AI:Ollama:Model"] ?? "qwen3.5:4b";
            _ollamaClassifierModel = configuration["AI:Ollama:ClassifierModel"] ?? _ollamaModel;
        }

        private class IntentResult
        {
            public string Intent { get; set; } = "search_requests";
            public int? Id { get; set; }
            public string? Keywords { get; set; }
        }

        ////needs to be refactored to work with userId
        public Task<List<ChatMessage>> GetChatHistoryAsync(string username)
        {

            var liveHistory = _historyCache.GetHistory(username);

            lock (liveHistory)
            {
                // Return a thread-safe snapshot copy to the Blazor component
                return Task.FromResult(liveHistory.ToList());
            }
        }

        // Keep your GetChatResponseAsync exactly as it was when it worked successfully!

        public async Task<string> GetRequestDetailsAsync(int requestId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var request = await context.ToDoItems.FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return $"System Error: Request ID {requestId} was not found in the database.";

            // Return a complete data dump of EVERY field for this single record
            return $"[LIVE DATABASE RECORD FOR ID {requestId}]\n" +
                   $"Creator: {request.Creator}\n" +
                   $"Is complete: {request.IsComplete}\n" +
                   $"Created: {request.CreationDate:yyyy-MM-dd}\n" +
                   $"Full Description: {request.Description}\n";
        }

        private async Task<string> SearchRequestsAsync(string searchTerm, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                return "No portal requests found. User is unauthenticated.";

            // 1. Classify intent via Ollama
            IntentResult intent = await ClassifyIntentAsync(searchTerm);

            using var context = await _contextFactory.CreateDbContextAsync();

            // 2. Always-scoped base query
            var query = context.ToDoItems
                .Include(r => r.Creator)
                .Include(r => r.Assignee)
                .Include(r => r.Responses)
                    .ThenInclude(r => r.Responder)
                .Where(r => !r.IsDeleted)
                .Where(r => r.CreatorId == currentUserId || r.AssigneeId == currentUserId);

            // 3. Scoped count check
            if (!await query.AnyAsync())
                return "You currently have no portal requests assigned to or created by you.";

            switch (intent.Intent)
            {
                case "small_talk":
                    return "SMALL_TALK";

                case "view_responses":
                    {
                        if (intent.Id == null)
                            return "Please specify a request ID to view its responses.";

                        var item = await query.FirstOrDefaultAsync(r => r.Id == intent.Id);
                        if (item == null)
                            return $"Request ID {intent.Id} was not found or you do not have access to it.";
                        if (!item.Responses.Any())
                            return $"Request ID {intent.Id} has no responses yet.";

                        string thread = string.Join("\n", item.Responses
                            .OrderBy(r => r.CreationDate)
                            .Select(r => $"  - [{r.CreationDate:yyyy-MM-dd}] {r.Responder?.UserName}: {r.ResponseText}"));

                        return $"[RESPONSES FOR REQUEST ID {intent.Id}]:\n{thread}";
                    }

                case "view_item":
                    {
                        if (intent.Id == null)
                            return "Please specify a request ID.";

                        var item = await query.FirstOrDefaultAsync(r => r.Id == intent.Id);
                        if (item == null)
                            return $"Request ID {intent.Id} was not found or you do not have access to it.";

                        return FormatSingleItem(item);
                    }

                case "search_requests":
                default:
                    {
                        if (!string.IsNullOrWhiteSpace(intent.Keywords))
                        {
                            string kw = intent.Keywords.ToLower();
                            query = query.Where(r =>
                                (r.Description != null && r.Description.ToLower().Contains(kw)) ||
                                (r.Creator != null && (r.Creator.FirstName + " " + r.Creator.LastName).ToLower().Contains(kw)));
                        }

                        var results = await query.OrderByDescending(r => r.Id).Take(10).ToListAsync();

                        if (!results.Any())
                            return string.IsNullOrWhiteSpace(intent.Keywords)
                                ? "No portal requests found in your accessible records."
                                : $"No records matching '{intent.Keywords}' were found in your accessible requests.";

                        return $"[DATABASE SNAPSHOT - FOUND {results.Count} MATCHES]:\n" +
                               string.Join("\n", results.Select(i =>
                               {
                                   string creator = i.Creator != null ? $"{i.Creator.FirstName} {i.Creator.LastName}" : "Unknown";
                                   string status = i.IsComplete ? "Completed" : "In Progress";
                                   string created = i.CreationDate != default ? i.CreationDate.ToString("yyyy-MM-dd") : "Not Set";
                                   string due = i.NecessaryCompletionDate != default ? i.NecessaryCompletionDate.ToString("yyyy-MM-dd") : "No Deadline";
                                   string desc = i.Description ?? "No description provided.";
                                   string responses = i.Responses?.Any() == true ? $"{i.Responses.Count} response(s)" : "No responses";

                                   return $"[ID: {i.Id}] From: {creator} | Status: {status} | Created: {created} | Due: {due} | Description: {desc} | Responses: {responses}";
                               }));
                    }
            }
        }

        private async Task<IntentResult> ClassifyIntentAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return new IntentResult { Intent = "search_requests" };

            if (Regex.IsMatch(userMessage.Trim(), @"^\d+$"))
                return new IntentResult { Intent = "view_item", Id = int.Parse(userMessage.Trim()) };

            return await CallOllamaClassifierAsync(userMessage);
        }

        private async Task<IntentResult> CallOllamaClassifierAsync(string userMessage)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var httpClient = _httpClientFactory.CreateClient("OllamaClient");

                var payload = new
                {
                    model = _ollamaClassifierModel,
                    stream = false,
                    keep_alive = "30m",
                    num_predict = 50,
                    messages = new[]
                    {
                new
                {
                    role = "system",
                    content = """
                        Classify the user message and respond with ONLY a raw JSON object. No markdown, no explanation, no backticks.

                        Intents:
                        - "view_item"       — user wants details of a specific request by ID
                        - "view_responses"  — user wants to see replies, comments, feedback, or responses on a specific request
                        - "search_requests" — user wants to list, find, browse, or retrieve requests (including "all", "available", "recent", "everything", "give me", etc.)
                        - "small_talk"      — greeting, casual conversation, or anything unrelated to portal requests

                        Rules:
                        - Extract "id" as an integer if a specific request number is mentioned, otherwise omit it.
                        - Extract "keywords" as a short string ONLY if the user is filtering by a specific topic or description (e.g. "sales", "urgent"). Omit for generic words like "all", "list", "show", "available", "everything", "my", "recent".
                        - When in doubt between search_requests and small_talk, prefer search_requests.
                        - When in doubt between view_item and search_requests and no clear ID exists, prefer search_requests.

                        Examples:
                        "show request 5"                        → {"intent":"view_item","id":5}
                        "what is in request 12"                 → {"intent":"view_item","id":12}
                        "replies on 3"                          → {"intent":"view_responses","id":3}
                        "any feedback on request 7"             → {"intent":"view_responses","id":7}
                        "show responses for ticket 2"           → {"intent":"view_responses","id":2}
                        "list all requests"                     → {"intent":"search_requests"}
                        "give available requests"               → {"intent":"search_requests"}
                        "show everything"                       → {"intent":"search_requests"}
                        "find sales requests"                   → {"intent":"search_requests","keywords":"sales"}
                        "show my urgent items"                  → {"intent":"search_requests","keywords":"urgent"}
                        "list all requests in a structured way" → {"intent":"search_requests"}
                        "hello"                                 → {"intent":"small_talk"}
                        "how are you"                           → {"intent":"small_talk"}
                    """
                },
                new { role = "user", content = userMessage }
            }
                };

                var response = await httpClient.PostAsJsonAsync(
                    $"{_ollamaEndpoint}/api/chat", payload, cts.Token);

                var data = await response.Content.ReadFromJsonAsync<JsonElement>(
                    cancellationToken: cts.Token);

                string raw = data.GetProperty("message").GetProperty("content").GetString() ?? "{}";
                raw = Regex.Replace(raw, @"```json|```", "").Trim();

                return JsonSerializer.Deserialize<IntentResult>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new IntentResult();
            }
            catch
            {
                return new IntentResult { Intent = "search_requests" };
            }
        }

        private static string FormatSingleItem(ToDoItem item)
        {
            string creator = item.Creator != null ? $"{item.Creator.FirstName} {item.Creator.LastName}" : "System/Unknown";
            string status = item.IsComplete ? "Completed" : "In Progress";
            string created = item.CreationDate != default ? item.CreationDate.ToString("yyyy-MM-dd") : "Not Set";
            string due = item.NecessaryCompletionDate != default ? item.NecessaryCompletionDate.ToString("yyyy-MM-dd") : "No specific deadline set";
            string desc = item.Description ?? "No description available.";
            string responses = item.Responses?.Any() == true ? $"{item.Responses.Count} response(s)" : "No responses";

            return $"[ID: {item.Id}] Created By: {creator} | Status: {status} | Created: {created} | Due: {due} | Description: {desc} | Responses: {responses}";
        }

        public async Task<ChatConversationDtos.ChatResponseDto> GetChatResponseAsync(ChatConversationDtos.ChatRequestDto request, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(request.UserPrompt))
            {
                return new ChatConversationDtos.ChatResponseDto { IsSuccess = false, ErrorMessage = "Prompt cannot be empty." };
            }

            try
            {
                // 1. Fetch an isolated, thread-safe snapshot copy from our secure cache
                List<ChatMessage> userHistory = _historyCache.GetHistory(request.CurrentUsername);
                IChatClient executionClient = _baseChatClient.AsBuilder()
                                                     .UseFunctionInvocation()
                                                     .Build();

                // Wipe out any old corrupted text chains if they exist in this local list
                if (userHistory.Any(m => m.Text != null && (m.Text.Contains("{\"name\"") || m.Text.Contains("SearchRequests"))))
                {
                    userHistory.Clear();
                }

                var executionMessages = new List<ChatMessage>();

                var systemInstruction = """
    You are a smart, natural, human-like AI Assistant for the Colleague Request portal.

    CHITCHAT vs DATA RULES:
    1. SMALL TALK & GREETINGS: If the user says hello, asks how you are doing, or chats casually, reply naturally as a friendly coworker. Do NOT run database tools for casual chatter, and do NOT mention portal requests unless specifically asked.
    2. CONTEXTUAL AWARENESS: If the user answers a follow-up question (like "yes" or "sure"), look at the previous message in the history to understand what they are agreeing to before taking any action.
    3. DATA SEARCH: Only call SearchRequests when the user explicitly asks to find, list, look up, or check a portal request, ID, or task.
    4. ABSOLUTE SILENCE ON MECHANICS: Never mention tool names, functions, parameters, JSON, or code. Speak only in fluid, natural prose.
    5. IF NO DATA FOUND: Simply say "I couldn't find any portal requests matching that information." Never guess or invent data.
    6. TOOL INPUT DISCIPLINE: When calling SearchRequests, the searchTerm must only contain a short keyword or ID taken directly from the user's message. Never pass your own reply text or full sentences as a tool parameter.

    DATA PRESENTATION:
    7. When you receive database results, present them in whatever format best matches what the user asked for:
       - "structured", "organized", "formatted" → present each request as a clear block with each field on its own line, separated visually.
       - "summary" or "overview" → write a short prose summary of the results.
       - "table" → present as a markdown table.
       - Casual request (e.g. "show me my requests") → clean readable list, one request per entry.
    8. Never blend multiple records into a single paragraph — always keep each request visually distinct.
    9. Always match your tone and format to what the user asked for, using your own judgement.

    HONESTY & ACCURACY:
    10. You only know what is returned to you from the database or what the user tells you in this conversation. Do not invent, assume, or fill in any details that were not explicitly provided.
    11. If you are unsure about something, say so honestly rather than making something up.
    12. Never reference information from previous conversations or sessions — you only have access to the current conversation and what the database returns.
""";

                executionMessages.Add(new ChatMessage(ChatRole.System, systemInstruction));

                // Rehydrate the conversation history (skipping internal system message placeholders)
                foreach (var existingMsg in userHistory)
                {
                    if (existingMsg.Role != ChatRole.System && !string.IsNullOrWhiteSpace(existingMsg.Text))
                    {
                        executionMessages.Add(new ChatMessage(existingMsg.Role, existingMsg.Text));
                    }
                }

                executionMessages.Add(new ChatMessage(ChatRole.User, request.UserPrompt));

                string capturedUserPrompt = request.UserPrompt; // capture before the lambda

                var searchTool = AIFunctionFactory.Create(
                    ([Description("Pass an empty string. The system handles intent automatically.")] string searchTerm) =>
                        SearchRequestsAsync(capturedUserPrompt, currentUserId), // always use the captured prompt
                    name: "SearchRequests",
                    description: "Queries the portal database for portal requests belonging to the current user."
                );

                var options = new ChatOptions
                {
                    Tools = new[] { searchTool },
                    ToolMode = ChatToolMode.Auto
                };

                // Execution runtime
                ChatResponse response = await executionClient.GetResponseAsync(executionMessages, options);

                string replyText = response.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant && !string.IsNullOrWhiteSpace(m.Text))?.Text
                                   ?? response.Text;

                // Clean out accidental trailing garbage loops if the model stumbled
                if (!string.IsNullOrWhiteSpace(replyText))
                {
                    int jsonIndex = replyText.IndexOf("{");
                    if (jsonIndex >= 0) replyText = replyText.Substring(0, jsonIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(replyText))
                {
                    replyText = "I'm here! What can I help you look up in the portal today?";
                }

                // 2. Append the exchange to our local track list instance
                userHistory.Add(new ChatMessage(ChatRole.User, request.UserPrompt));
                userHistory.Add(new ChatMessage(ChatRole.Assistant, replyText));

                // 3. 🎯 CRITICAL SECURITY FIX: Explicitly save the updated list instance back into the cache dictionary!
                _historyCache.SaveHistory(request.CurrentUsername, userHistory);

                return new ChatConversationDtos.ChatResponseDto { AiReply = replyText, IsSuccess = true };
            }
            catch (Exception ex)
            {
                return new ChatConversationDtos.ChatResponseDto { IsSuccess = false, ErrorMessage = $"Error: {ex.Message}" };
            }
        }
    }
}
