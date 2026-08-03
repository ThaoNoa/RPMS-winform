using RPMS.BLL.Exceptions;
using RPMS.BLL.Interfaces;
using RPMS.DAL.Entities;
using RPMS.DAL.UnitOfWork.Interfaces;
using RPMS.DTO.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPMS.BLL.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(int userId)
        {
            var list = await _unitOfWork.ChatConversations.FindAsync(
                c => c.LandlordID == userId || c.TenantID == userId,
                "Landlord, Tenant, Messages");

            return list
                .OrderByDescending(c => c.LastMessageAt ?? c.UpdatedDate)
                .Select(c => MapConversation(c, userId))
                .ToList();
        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(int landlordId, int tenantId)
        {
            if (landlordId == tenantId)
                throw new BadRequestException("Không thể tạo hội thoại với chính mình.");

            var existing = await _unitOfWork.ChatConversations.FirstOrDefaultAsync(
                c => c.LandlordID == landlordId && c.TenantID == tenantId,
                "Landlord, Tenant, Messages");
            if (existing != null)
                return MapConversation(existing, landlordId);

            var landlord = await _unitOfWork.Users.GetByIdAsync(landlordId);
            var tenant = await _unitOfWork.Users.GetByIdAsync(tenantId);
            if (landlord == null || tenant == null)
                throw new BadRequestException("Người dùng không hợp lệ.");

            var entity = new ChatConversation
            {
                LandlordID = landlordId,
                TenantID = tenantId,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _unitOfWork.ChatConversations.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            existing = await _unitOfWork.ChatConversations.FirstOrDefaultAsync(
                c => c.ConversationID == entity.ConversationID,
                "Landlord, Tenant, Messages");
            return MapConversation(existing!, landlordId);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesAsync(int conversationId, int currentUserId)
        {
            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId);
            if (conversation == null) throw new NotFoundException("Hội thoại", conversationId);
            if (conversation.LandlordID != currentUserId && conversation.TenantID != currentUserId)
                throw new BadRequestException("Bạn không thuộc hội thoại này.");

            var messages = await _unitOfWork.ChatMessages.FindAsync(
                m => m.ConversationID == conversationId,
                "Sender");

            return messages
                .OrderBy(m => m.CreatedDate)
                .Select(m => MapMessage(m, currentUserId))
                .ToList();
        }

        public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Content) && string.IsNullOrWhiteSpace(request.ImagePath))
                throw new BadRequestException("Tin nhắn không được trống.");

            var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(request.ConversationID);
            if (conversation == null) throw new NotFoundException("Hội thoại", request.ConversationID);
            if (conversation.LandlordID != request.SenderID && conversation.TenantID != request.SenderID)
                throw new BadRequestException("Bạn không thuộc hội thoại này.");

            var message = new ChatMessage
            {
                ConversationID = request.ConversationID,
                SenderID = request.SenderID,
                Content = (request.Content ?? "").Trim(),
                ImagePath = string.IsNullOrWhiteSpace(request.ImagePath) ? null : request.ImagePath.Trim(),
                IsRead = false,
                CreatedDate = DateTime.Now
            };
            await _unitOfWork.ChatMessages.AddAsync(message);

            conversation.LastMessageAt = message.CreatedDate;
            conversation.UpdatedDate = message.CreatedDate;
            _unitOfWork.ChatConversations.Update(conversation);

            int receiverId = conversation.LandlordID == request.SenderID
                ? conversation.TenantID
                : conversation.LandlordID;

            string preview = string.IsNullOrWhiteSpace(message.Content) ? "[Hình ảnh]" : message.Content;
            if (preview.Length > 120) preview = preview[..120] + "…";

            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserID = receiverId,
                Title = "Tin nhắn mới",
                Content = preview,
                IsRead = false,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _unitOfWork.SaveChangesAsync();

            var saved = await _unitOfWork.ChatMessages.FirstOrDefaultAsync(m => m.MessageID == message.MessageID, "Sender");
            return MapMessage(saved!, request.SenderID);
        }

        public async Task<bool> MarkConversationReadAsync(int conversationId, int readerUserId)
        {
            var messages = await _unitOfWork.ChatMessages.FindAsync(
                m => m.ConversationID == conversationId && m.SenderID != readerUserId && !m.IsRead);
            foreach (var msg in messages)
            {
                msg.IsRead = true;
                _unitOfWork.ChatMessages.Update(msg);
            }
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            var conversations = await _unitOfWork.ChatConversations.FindAsync(
                c => c.LandlordID == userId || c.TenantID == userId);
            var ids = conversations.Select(c => c.ConversationID).ToList();
            if (ids.Count == 0) return 0;
            return await _unitOfWork.ChatMessages.CountAsync(
                m => ids.Contains(m.ConversationID) && m.SenderID != userId && !m.IsRead);
        }

        private static ConversationDto MapConversation(ChatConversation c, int currentUserId)
        {
            var last = c.Messages?.OrderByDescending(m => m.CreatedDate).FirstOrDefault();
            int unread = c.Messages?.Count(m => m.SenderID != currentUserId && !m.IsRead) ?? 0;
            return new ConversationDto
            {
                ConversationID = c.ConversationID,
                LandlordID = c.LandlordID,
                TenantID = c.TenantID,
                LandlordName = c.Landlord?.FullName ?? "",
                TenantName = c.Tenant?.FullName ?? "",
                LastMessage = last == null ? "" : (string.IsNullOrWhiteSpace(last.Content) ? "[Hình ảnh]" : last.Content),
                LastMessageAt = c.LastMessageAt ?? last?.CreatedDate,
                UnreadCount = unread
            };
        }

        private static ChatMessageDto MapMessage(ChatMessage m, int currentUserId) => new()
        {
            MessageID = m.MessageID,
            ConversationID = m.ConversationID,
            SenderID = m.SenderID,
            SenderName = m.Sender?.FullName ?? "",
            Content = m.Content,
            ImagePath = m.ImagePath,
            IsRead = m.IsRead,
            CreatedDate = m.CreatedDate,
            IsMine = m.SenderID == currentUserId
        };
    }
}
