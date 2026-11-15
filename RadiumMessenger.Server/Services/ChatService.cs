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
                .Where(c =>
                    (c.FirstUserId == userId && !c.IsArchivedForFirstUser) ||
                    (c.SecondUserId == userId && !c.IsArchivedForSecondUser))
                .Include(c => c.Messages)
                .ToListAsync();

            foreach (var chat in chats)
            {
                int otherUserId = chat.FirstUserId == userId
                    ? chat.SecondUserId
                    : chat.FirstUserId;

                var otherUser = await _db.Users.FindAsync(otherUserId);

                chat.OtherUsername = otherUser?.Username ?? "Неизвестный пользователь";

                foreach (var message in chat.Messages)
                {
                    var sender = await _db.Users.FindAsync(message.SenderId);

                    message.SenderUsername = sender?.Username
                        ?? "Неизвестный пользователь";
                }
            }

            return chats;
        }

        public async Task<List<Chat>> GetArchivedUserChatsAsync(int userId)
        {
            var chats = await _db.Chats
                .Where(c =>
                    (c.FirstUserId == userId && c.IsArchivedForFirstUser) ||
                    (c.SecondUserId == userId && c.IsArchivedForSecondUser))
                .Include(c => c.Messages)
                .ToListAsync();

            foreach (var chat in chats)
            {
                int otherUserId = chat.FirstUserId == userId
                    ? chat.SecondUserId
                    : chat.FirstUserId;

                var otherUser = await _db.Users.FindAsync(otherUserId);

                chat.OtherUsername = otherUser?.Username 
                    ?? "Неизвестный пользователь";
            }

            return chats;
        }

        public async Task<bool> ArchiveChatAsync(int chatId, int userId)
        {
            var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return false;

            if (chat.FirstUserId == userId)
                chat.IsArchivedForFirstUser = true;
            else if (chat.SecondUserId == userId)
                chat.IsArchivedForSecondUser = true;
            else
                return false;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnarchiveChatAsync(int chatId, int userId)
        {
            var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return false;

            if (chat.FirstUserId == userId)
                chat.IsArchivedForFirstUser = false;
            else if (chat.SecondUserId == userId)
                chat.IsArchivedForSecondUser = false;
            else
                return false;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}