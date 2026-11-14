using Microsoft.EntityFrameworkCore;
using RadiumMessenger.Server.Data;
using RadiumMessenger.Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace RadiumMessenger.Server.Services
{
    public class VerificationService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public VerificationService(
            AppDbContext db,
            EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task SendCodeAsync(string email)
        {
            var code = RandomNumberGenerator.GetInt32(100000, 1000000)
                .ToString();

            var verification = new EmailVerification
            {
                Email = email,
                CodeHash = HashCode(code),
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            var oldCodes = await _db.EmailVerifications
                .Where(v => v.Email == email && !v.IsUsed)
                .ToListAsync();

            _db.EmailVerifications.RemoveRange(oldCodes);
            _db.EmailVerifications.Add(verification);

            await _db.SaveChangesAsync();

            await _emailService.SendVerificationCodeAsync(email, code);
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var verification = await _db.EmailVerifications
                .Where(v => v.Email == email && !v.IsUsed)
                .OrderByDescending(v => v.Id)
                .FirstOrDefaultAsync();

            if (verification == null)
                return false;

            if (verification.ExpiresAt < DateTime.UtcNow)
                return false;

            if (verification.CodeHash != HashCode(code))
                return false;

            verification.IsUsed = true;

            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Email == email);

            if (user != null)
            {
                user.IsEmailVerified = true;
            }

            await _db.SaveChangesAsync();

            return true;
        }

        private string HashCode(string code)
        {
            byte[] hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(code));

            return Convert.ToBase64String(hash);
        }
    }
}