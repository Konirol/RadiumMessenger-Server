using Microsoft.AspNetCore.Mvc;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Services;

namespace RadiumMessenger.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = await _authService.RegisterAsync(dto);

            if (user == null)
                return BadRequest("Пользователь с таким email или именем уже существует.");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto);

            if (user == null)
                return Unauthorized("Неверный email или пароль.");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email
            });
        }
    }
}