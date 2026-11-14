using Microsoft.AspNetCore.Mvc;
using RadiumMessenger.Server.Services;

namespace RadiumMessenger.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly VerificationService _verificationService;

        public VerificationController(VerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendCode([FromBody] string email)
        {
            try
            {
                await _verificationService.SendCodeAsync(email);
                return Ok("Код отправлен на электронную почту.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Не удалось отправить код.");
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode(
            [FromQuery] string email,
            [FromQuery] string code)
        {
            bool isValid = await _verificationService.VerifyCodeAsync(email, code);

            if (!isValid)
                return BadRequest("Неверный или просроченный код.");

            return Ok("Электронная почта успешно подтверждена.");
        }
    }
}