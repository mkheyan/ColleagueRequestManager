using Microsoft.Extensions.AI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Services
{
    public class ChatHistoryCache : IChatHistoryCache
    {
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _userHistories = new(StringComparer.OrdinalIgnoreCase);

        public List<ChatMessage> GetHistory(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return new List<ChatMessage>();

            var masterList = _userHistories.GetOrAdd(username, _ =>
            {
                var initialHistory = new List<ChatMessage>();
                var systemContext = new System.Text.StringBuilder();
                systemContext.AppendLine("You are the internal operational assistant for the portal.");
                systemContext.AppendLine("CRITICAL RULES:");
                systemContext.AppendLine("1. STRICT DOMAIN: You must ONLY discuss portal tasks and requests. If the user asks about math, coding, or general trivia, politely decline.");
                systemContext.AppendLine("2. NO GUESSING: NEVER make up or hallucinate data.");
                systemContext.AppendLine("3. GREETINGS: If the user just says hello, respond conversationally without tools.");
                systemContext.AppendLine("4. TOOL USAGE: Use 'SearchRequests' to find items.");
                return initialHistory;
            });

            // 🎯 Return a copy snapshot so external modifications are safely isolated
            lock (masterList)
            {
                return masterList.ToList();
            }
        }

        // 🎯 Added this method to accept the updated data from GetChatResponseAsync
        public void SaveHistory(string username, List<ChatMessage> updatedHistory)
        {
            if (string.IsNullOrWhiteSpace(username) || updatedHistory == null) return;
            _userHistories[username] = updatedHistory.ToList();
        }

        public void ClearUserHistory(string username)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                _userHistories.TryRemove(username, out _);
            }
        }
    }
}
