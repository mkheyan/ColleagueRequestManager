using Microsoft.Extensions.AI;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Business.Services;

public interface IChatHistoryCache
{
    // Fetches the history for a specific user
    List<ChatMessage> GetHistory(string username);

    // Saves or updates the history
    void SaveHistory(string username, List<ChatMessage> history);

    // 🎯 The security mechanism to wipe data
    void ClearUserHistory(string username);
}