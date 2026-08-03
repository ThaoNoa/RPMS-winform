using System;
using System.Collections.Generic;

namespace RPMS.DTO.Chat
{
    public class ConversationDto
    {
        public int ConversationID { get; set; }
        public int LandlordID { get; set; }
        public int TenantID { get; set; }
        public string LandlordName { get; set; } = "";
        public string TenantName { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ChatMessageDto
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public int SenderID { get; set; }
        public string SenderName { get; set; } = "";
        public string Content { get; set; } = "";
        public string? ImagePath { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsMine { get; set; }
    }

    public class SendMessageDto
    {
        public int ConversationID { get; set; }
        public int SenderID { get; set; }
        public string Content { get; set; } = "";
        public string? ImagePath { get; set; }
    }
}
