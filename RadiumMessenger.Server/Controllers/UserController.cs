using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;

namespace RadiumMessenger.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UserController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Ok(new List<object>());

            var users = await _db.Users
                .Where(u => u.Username.Contains(query))
                .Select(u => new
                {
                    u.Id,
                    u.Username
                })
                .Take(20)
                .ToListAsync();

            return Ok(users);
        }
    }
}