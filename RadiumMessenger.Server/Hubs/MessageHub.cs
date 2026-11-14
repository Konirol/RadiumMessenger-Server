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

        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"chat_{chatId}"
            );
        }
    }
}