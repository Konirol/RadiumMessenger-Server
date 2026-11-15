using Microsoft.AspNetCore.Mvc;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Services;

namespace RadiumMessenger.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateChat(CreateChatDto dto)
        {
            var chat = await _chatService.CreateChatAsync(dto);

            if (chat == null)
                return BadRequest("Не удалось создать чат. Проверьте ID пользователей.");

            return Ok(new
            {
                chat.Id,
                chat.FirstUserId,
                chat.SecondUserId,
                chat.CreatedAt
            });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserChats(int userId)
        {
            var chats = await _chatService.GetUserChatsAsync(userId);

            return Ok(chats.Select(c => new
            {
                c.Id,
                c.FirstUserId,
                c.SecondUserId,
                c.OtherUsername,
                c.CreatedAt,
                Messages = c.Messages.Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderUsername,
                    m.Text,
                    m.SentAt,
                    m.IsDelivered,
                    m.IsRead
                })
            }));
        }

        [HttpGet("archived/{userId}")]
        public async Task<IActionResult> GetArchivedUserChats(int userId)
        {
            var chats = await _chatService.GetArchivedUserChatsAsync(userId);

            return Ok(chats.Select(c => new
            {
                c.Id,
                c.FirstUserId,
                c.SecondUserId,
                c.OtherUsername,
                c.CreatedAt,
                Messages = c.Messages.Select(m => new
                {
                    m.Id,
                    m.SenderId,
                    m.SenderUsername,
                    m.Text,
                    m.SentAt,
                    m.IsDelivered,
                    m.IsRead
                })
            }));
        }

        [HttpPost("archive/{chatId}/{userId}")]
        public async Task<IActionResult> ArchiveChat(int chatId, int userId)
        {
            bool result = await _chatService.ArchiveChatAsync(chatId, userId);

            if (!result)
                return NotFound("Чат не найден или у вас нет доступа.");

            return Ok("Чат архивирован.");
        }

        [HttpPost("unarchive/{chatId}/{userId}")]
        public async Task<IActionResult> UnarchiveChat(int chatId, int userId)
        {
            bool result = await _chatService.UnarchiveChatAsync(chatId, userId);

            if (!result)
                return NotFound("Чат не найден или у вас нет доступа.");

            return Ok("Чат восстановлен.");
        }
    }
}