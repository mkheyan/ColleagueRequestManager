using Microsoft.Extensions.AI;
using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public interface IChatBotService
    {
        Task<ChatConversationDtos.ChatResponseDto> GetChatResponseAsync(Models.ChatConversationDtos.ChatRequestDto request, string currentUserId);
        Task<List<ChatMessage>> GetChatHistoryAsync(string username);
    }
}
