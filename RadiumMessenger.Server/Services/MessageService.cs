using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Hubs;
using RadiumMessenger.Server.Models;

namespace RadiumMessenger.Server.Services
{
    public class MessageService
    {
        private readonly AppDbContext _db;

        private readonly IHubContext<MessageHub> _hub;

        public MessageService(AppDbContext db, IHubContext<MessageHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task<Message?> SendMessageAsync(SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Text))
                return null;

            var chat = await _db.Chats.FirstOrDefaultAsync(c =>
                c.Id == dto.ChatId);

            if (chat == null)
                return null;

            if (dto.SenderId != chat.FirstUserId &&
                dto.SenderId != chat.SecondUserId)
                return null;

            var sender = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == dto.SenderId);

            if (sender == null)
                return null;

            var message = new Message
            {
                ChatId = dto.ChatId,
                SenderId = dto.SenderId,
                SenderUsername = sender.Username,
                Text = dto.Text.Trim()
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await _hub.Clients
                .Group($"chat_{dto.ChatId}")
                .SendAsync("ReceiveMessage", new
                {
                    message.Id,
                    message.ChatId,
                    message.SenderId,
                    message.SenderUsername,
                    message.Text,
                    message.SentAt,
                    message.IsDelivered,
                    message.IsRead
                });

            return message;
        }

        public async Task<bool> DeleteForMeAsync(int messageId, int userId)
        {
            var message = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return false;

            var chat = await _db.Chats.FirstOrDefaultAsync(c =>
                c.Id == message.ChatId);

            if (chat == null)
                return false;

            if (chat.FirstUserId == userId)
                message.IsDeletedForFirstUser = true;
            else if (chat.SecondUserId == userId)
                message.IsDeletedForSecondUser = true;
            else
                return false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteForEveryoneAsync(int messageId, int userId)
        {
            var message = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null || message.SenderId != userId)
                return false;

            var chatId = message.ChatId;

            _db.Messages.Remove(message);
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"chat_{chatId}")
                .SendAsync("MessageDeleted", messageId);

            return true;
        }

        public async Task MarkAsDeliveredAsync(int messageId)
        {
            var message = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return;

            message.IsDelivered = true;

            await _db.SaveChangesAsync();
        }
    }
}