using RPMS.DTO.Chat;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RPMS.BLL.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ConversationDto>> GetConversationsAsync(int userId);
        Task<ConversationDto> GetOrCreateConversationAsync(int landlordId, int tenantId);
        Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int conversationId, int currentUserId);
        Task<ChatMessageDto> SendMessageAsync(SendMessageDto request);
        Task<bool> MarkConversationReadAsync(int conversationId, int readerUserId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
