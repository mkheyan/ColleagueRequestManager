using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ChatConversationDtos
    {
        public class ChatRequestDto
        {
            public string UserPrompt { get; set; } = string.Empty;
            public string CurrentUsername { get; set; } = string.Empty;
            public string CurrentUserID { get; set; } = string.Empty;
        }

        // Sent from the Business Layer back to the Blazor View
        public class ChatResponseDto
        {
            public string AiReply { get; set; } = string.Empty;
            public bool IsSuccess { get; set; } = true;
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}
