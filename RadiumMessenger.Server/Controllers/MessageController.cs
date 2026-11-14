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
                message.Text,
                message.SentAt
            });
        }
    }
}