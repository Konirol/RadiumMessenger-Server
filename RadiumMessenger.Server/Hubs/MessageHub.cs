using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;

namespace RadiumMessenger.Server.Hubs
{
    public class MessageHub : Hub
    {
        private readonly AppDbContext _db;

        public MessageHub(AppDbContext db)
        {
            _db = db;
        }

        public async Task JoinChat(int chatId, int userId)
        {
            bool isMember = await _db.Chats.AnyAsync(c =>
                c.Id == chatId &&
                (c.FirstUserId == userId || c.SecondUserId == userId));

            if (!isMember)
                throw new HubException("У вас нет доступа к этому чату.");

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"chat_{chatId}"
            );
        }

        public async Task MarkAsDelivered(int messageId, int userId)
        {
            var message = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return;

            var chat = await _db.Chats.FirstOrDefaultAsync(c =>
                c.Id == message.ChatId);

            if (chat == null)
                return;

            bool isRecipient =
                (chat.FirstUserId == userId || chat.SecondUserId == userId)
                && message.SenderId != userId;

            if (!isRecipient)
                return;

            message.IsDelivered = true;
            await _db.SaveChangesAsync();

            await Clients.Group($"chat_{message.ChatId}")
                .SendAsync("MessageStatusUpdated", message.Id, true, message.IsRead);
        }

        public async Task MarkAsRead(int messageId, int userId)
        {
            var message = await _db.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                return;

            var chat = await _db.Chats.FirstOrDefaultAsync(c => 
                c.Id == message.ChatId);

            if (chat == null) 
                return;

            bool isRecipient =
                (chat.FirstUserId == userId || chat.SecondUserId == userId)
                && message.SenderId != userId;

            if (!isRecipient)
                return;

            message.IsDelivered = true;
            message.IsRead = true;

            await _db.SaveChangesAsync();

            await Clients.Group($"chat_{message.ChatId}")
                .SendAsync("MessageStatusUpdated", message.Id, true, true);
        }

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"chat_{chatId}"
            );
        }
    }
}