using Microsoft.AspNetCore.Mvc;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Services;

namespace RadiumMessenger.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageService _messageService;

        public MessageController(MessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            var message = await _messageService.SendMessageAsync(dto);

            if (message == null)
                return BadRequest("Не удалось отправить сообщение.");

            return Ok(new
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
        }

        [HttpDelete("delete-for-me/{messageId}/{userId}")]
        public async Task<IActionResult> DeleteForMe(int messageId, int userId)
        {
            bool result = await _messageService.DeleteForMeAsync(messageId, userId);

            if (!result)
                return NotFound("Сообщение не найдено или нет доступа.");

            return Ok();
        }

        [HttpDelete("delete-for-everyone/{messageId}/{userId}")]
        public async Task<IActionResult> DeleteForEveryone(int messageId, int userId)
        {
            bool result = await _messageService.DeleteForEveryoneAsync(messageId, userId);

            if (!result)
                return NotFound("Сообщение не найдено или вы не его автор.");

            return Ok();
        }

    }
}