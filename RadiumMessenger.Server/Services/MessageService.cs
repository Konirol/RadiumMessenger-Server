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

            var message = new Message
            {
                ChatId = dto.ChatId,
                SenderId = dto.SenderId,
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
                    message.Text,
                    message.SentAt
                });

            return message;
        }
    }
}