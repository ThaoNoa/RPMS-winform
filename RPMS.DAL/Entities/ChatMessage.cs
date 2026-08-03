using System;

namespace RPMS.DAL.Entities
{
    public class ChatMessage
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public int SenderID { get; set; }
        public string Content { get; set; } = "";
        public string? ImagePath { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ChatConversation Conversation { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }
}
