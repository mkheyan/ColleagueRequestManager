using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.VisualBasic;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
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

        // 🎯 Note: The local _chatHistory list has been removed entirely to stop state tracking leaks!

        public ChatBotService(
            IChatClient chatClient,
            IDbContextFactory<ApplicationDbContext> contextFactory,
            IChatHistoryCache historyCache)
        {
            // 1. Keep the base client clean. We will build the function pipeline dynamically inside the method.
            _baseChatClient = chatClient;

            // 2. Inject the thread-safe Factory instead of a sticky, persistent database context.
            _contextFactory = contextFactory;

            _historyCache = historyCache;
        }

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

        private async Task<string> SearchRequestsAsync(string searchTerm, string currentUsername)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentUsername) || currentUsername.StartsWith("Anonymous", StringComparison.OrdinalIgnoreCase))
                {
                    return "No portal requests found. User is unauthenticated.";
                }

                using var context = await _contextFactory.CreateDbContextAsync();
                // -----------------------------------------------------------------
                // 1. DIAGNOSTIC SAFETY CHECK
                // -----------------------------------------------------------------
                int totalCount = await context.ToDoItems.CountAsync();
                if (totalCount == 0)
                {
                    return "SYSTEM NOTIFICATION: The database table 'ToDoItems' is entirely empty (0 rows). " +
                           "Inform the user that no records exist in the system yet, so no results can be displayed.";
                }

                // -----------------------------------------------------------------
                // 2. CHAT PASS-THROUGH GUARD
                // -----------------------------------------------------------------
                string cleanTerm = (searchTerm ?? string.Empty).Trim().ToLower();
                string[] casualWords = { "hi", "hello", "hey", "good morning", "how are you", "yes", "no", "good", "thanks", "ok" };

                if (casualWords.Contains(cleanTerm))
                {
                    return "The user is engaging in conversational small talk or a basic acknowledgement. " +
                           "Ignore database processing and respond warmly in natural language.";
                }

                // -----------------------------------------------------------------
                // 3. BASE QUERY INITIALIZATION (Eager Loading)
                // -----------------------------------------------------------------
                // We explicitly .Include(r => r.Creator) to prevent NullReferenceExceptions when reading user profiles
                var query = context.ToDoItems.Include(r => r.Creator).AsQueryable();

                // -----------------------------------------------------------------
                // 4. INTELLIGENT ID EXTRACTION (Regex)
                // -----------------------------------------------------------------
                // Pulls numbers cleanly out of prompts like "request id 1", "show item 12", or just "1"
                var numericMatch = Regex.Match(cleanTerm, @"\d+");
                if (numericMatch.Success && int.TryParse(numericMatch.Value, out int extractedId))
                {
                    var item = await query.FirstOrDefaultAsync(r => r.Id == extractedId);
                    if (item != null)
                    {
                        string creatorName = item.Creator != null ? $"{item.Creator.FirstName} {item.Creator.LastName}" : "System/Unknown";
                        string status = item.IsComplete ? "Completed" : "In Progress";
                        string dateStr = item.CreationDate != default ? item.CreationDate.ToString("yyyy-MM-dd") : "Not Set";

                        // 🎯 Added clear visibility for the Due/Completion date to prevent AI hallucination
                        string necessaryCompletionDateStr = item.NecessaryCompletionDate != default ? item.NecessaryCompletionDate.ToString("yyyy-MM-dd") : "No specific deadline set";
                        string description = item.Description ?? "No description available.";

                        return $"[FOUND MATCH FOR REQUEST ID {extractedId}]:\n" +
                               $"[ID: {item.Id}] Created By: {creatorName} | Status: {status} | Created Date: {dateStr} | Necessary Completion Date: {necessaryCompletionDateStr} | Description: {description}";
                    }
                    return $"Database lookup executed: Request ID {extractedId} does not exist in our records.";
                }

                // -----------------------------------------------------------------
                // 5. OWNERSHIP CONTEXT FILTER
                // -----------------------------------------------------------------
                // Detects if the user wants to narrow data down to their own person
                bool isUserSpecific = cleanTerm.Contains("my") || cleanTerm.Contains("me") ||
                                     cleanTerm.Contains("creator") || cleanTerm.Contains("assignee");

                if (isUserSpecific)
                {
                    // Fallback strategy: cross-checks username or first name strings safely
                    query = query.Where(r => r.Creator != null &&
                        (r.Creator.UserName.ToLower() == currentUsername.ToLower() ||
                         r.Creator.FirstName.ToLower().Contains(currentUsername.ToLower())));
                }

                // -----------------------------------------------------------------
                // 6. OPERATIONAL NOISE STRIPPING
                // -----------------------------------------------------------------
                // Strips out generic structural sentences so we extract the actual keyword intent (e.g., "sales")
                string trueKeywords = cleanTerm
                    .Replace("list all", "").Replace("list available", "").Replace("available requests", "")
                    .Replace("list", "").Replace("show", "").Replace("find", "").Replace("all", "")
                    .Replace("requests", "").Replace("request", "").Replace("where i am the creator", "")
                    .Replace("my", "").Replace("me", "")
                    .Trim();

                // If there are true keywords left after cleaning, apply wild-card phrase matching
                if (!string.IsNullOrWhiteSpace(trueKeywords))
                {
                    query = query.Where(r =>
                        (r.Description != null && r.Description.ToLower().Contains(trueKeywords)) ||
                        (r.Creator != null && (r.Creator.FirstName + " " + r.Creator.LastName).ToLower().Contains(trueKeywords))
                    );
                }

                // -----------------------------------------------------------------
                // 7. RECORD MATERIALIZATION & NULL-SAFE OUTPUT
                // -----------------------------------------------------------------
                // Pull the top 10 most recent records to prevent payload bloating
                var results = await query.OrderByDescending(r => r.Id).Take(10).ToListAsync();

                if (!results.Any())
                {
                    return isUserSpecific
                        ? $"No active portal requests were found under your user identity profile ('{currentUsername}')."
                        : $"No database logs found matching the keyword filters: '{trueKeywords}'.";
                }

                // Format data into a clean, deterministic textual summary string that the LLM can cleanly process
                return $"[DATABASE SNAPSHOT - FOUND {results.Count} MATCHES]:\n" +
                       string.Join("\n", results.Select(i =>
                       {
                           string firstName = i.Creator?.FirstName ?? "Unknown";
                           string lastName = i.Creator?.LastName ?? "User";
                           string status = i.IsComplete ? "Completed" : "In Progress";
                           string createdDate = i.CreationDate != default ? i.CreationDate.ToString("yyyy-MM-dd") : "Not Set";

                           // 🎯 Maintained here inside the collection mapping for open lists
                           string necessaryCompletionDate = i.NecessaryCompletionDate != default ? i.NecessaryCompletionDate.ToString("yyyy-MM-dd") : "No Deadline";
                           string description = i.Description ?? "No description provided.";

                           return $"[ID: {i.Id}] From: {firstName} {lastName} | Status: {status} | Created: {createdDate} | Due: {necessaryCompletionDate} | Description: {description}";
                       }));
            }
            catch (Exception ex)
            {
                // Forwards the detailed runtime issue directly to the LLM context rather than throwing silently
                return $"INTERNAL DATABASE TOOL ERROR: {ex.Message}. Check inner stack trace configuration loops.";
            }
        }

        public async Task<ChatConversationDtos.ChatResponseDto> GetChatResponseAsync(ChatConversationDtos.ChatRequestDto request)
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

                // 🎯 THE BALANCED SYSTEM PROMPT: Teaches the AI common sense
                var systemInstruction = """You are a smart, natural, human-like AI Assistant for the Colleague Request portal. CHITCHAT vs DATA RULES:1. SMALL TALK & GREETINGS: If the user says hello, asks how you are doing, says "good", "no", or chats casually, reply naturally as a friendly coworker. DO NOT run database tools for casual chatter, and DO NOT mention portal requests or "sales enablement plans" unless specifically asked.2. CONTEXTUAL AWARENESS: If the user answers a follow-up question (like saying "yes" or "sure"), look at the previous message in the history to understand what they are agreeing to. 3. DATA SEARCH: Only rely on your SearchRequests tool when the user explicitly asks to find, list, look up, or check a portal request, ID, or task.4. ABSOLUTE SILENCE ON MECHANICS: Never mention the name of your tools, functions, parameters, JSON, or code. Speak only in fluid, natural prose sentences.5. IF NO DATA FOUND: If a database search yields no records, simply say: "I couldn't find any portal requests matching that information." Do not guess or make up data.""";

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

                // Define your data access tool mapping
                var searchTool = AIFunctionFactory.Create(
                    ([Description("The keyword, ID, or phrase to filter requests by. Pass an empty string if the user wants to list all, everything, or recent requests.")] string searchTerm) =>
                        SearchRequestsAsync(searchTerm, request.CurrentUsername),
                    name: "SearchRequests",
                    description: "Queries the portal database for requests. Automatically handles search keywords, specific request IDs, or requests belonging to the current user."
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
