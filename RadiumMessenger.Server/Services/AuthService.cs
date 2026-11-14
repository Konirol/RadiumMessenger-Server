using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;
using RadiumMessenger.Server.DTOs;
using RadiumMessenger.Server.Models;
using System.Security.Cryptography;

namespace RadiumMessenger.Server.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;

        private readonly VerificationService _verificationService;

        public AuthService(
            AppDbContext db,
            VerificationService verificationService)
        {
            _db = db;
            _verificationService = verificationService;
        }

        public async Task<User?> RegisterAsync(RegisterDto dto)
        {
            bool exists = await _db.Users.AnyAsync(u =>
                u.Email == dto.Email || u.Username == dto.Username);

            if (exists)
                return null;

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                IsEmailVerified = false
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            user.IsEmailVerified = true;
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<User?> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == dto.Email);

            if (user == null)
                return null;

            if (!VerifyPassword(dto.Password, user.PasswordHash))
                return null;

            if (!user.IsEmailVerified)
                return null;

            return user;
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            return Convert.ToBase64String(salt) + "." +
                   Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split('.');

            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] originalHash = Convert.FromBase64String(parts[1]);

            byte[] newHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            return CryptographicOperations.FixedTimeEquals(
                originalHash,
                newHash
            );
        }
    }
}