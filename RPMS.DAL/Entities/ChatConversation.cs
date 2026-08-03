using System;
using System.Collections.Generic;

namespace RPMS.DAL.Entities
{
    public class ChatConversation
    {
        public ChatConversation()
        {
            Messages = new HashSet<ChatMessage>();
        }

        public int ConversationID { get; set; }
        public int LandlordID { get; set; }
        public int TenantID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public DateTime? LastMessageAt { get; set; }

        public virtual User Landlord { get; set; } = null!;
        public virtual User Tenant { get; set; } = null!;
        public virtual ICollection<ChatMessage> Messages { get; set; }
    }
}
