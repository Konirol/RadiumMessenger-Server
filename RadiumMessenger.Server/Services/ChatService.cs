using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Models;

namespace RadiumMessenger.Server.Services
{
    public class ChatService
    {
        private readonly AppDbContext _db;

        public ChatService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<Chat?> CreateChatAsync(CreateChatDto dto)
        {
            if (dto.FirstUserId == dto.SecondUserId)
                return null;

            bool usersExist = await _db.Users.AnyAsync(u => u.Id == dto.FirstUserId)
                && await _db.Users.AnyAsync(u => u.Id == dto.SecondUserId);

            if (!usersExist)
                return null;

            var existingChat = await _db.Chats.FirstOrDefaultAsync(c =>
                (c.FirstUserId == dto.FirstUserId && c.SecondUserId == dto.SecondUserId) ||
                (c.FirstUserId == dto.SecondUserId && c.SecondUserId == dto.FirstUserId));

            if (existingChat != null)
                return existingChat;

            var chat = new Chat
            {
                FirstUserId = dto.FirstUserId,
                SecondUserId = dto.SecondUserId
            };

            _db.Chats.Add(chat);
            await _db.SaveChangesAsync();

            return chat;
        }

        public async Task<List<Chat>> GetUserChatsAsync(int userId)
        {
            var chats = await _db.Chats
                .Where(c => c.FirstUserId == userId || c.SecondUserId == userId)
                .Include(c => c.Messages)
                .ToListAsync();

            foreach (var chat in chats)
            {
                int otherUserId = chat.FirstUserId == userId
                    ? chat.SecondUserId
                    : chat.FirstUserId;

                var otherUser = await _db.Users.FindAsync(otherUserId);

                chat.OtherUsername = otherUser?.Username ?? "Неизвестный пользователь";
            }

            return chats;
        }
    }
}