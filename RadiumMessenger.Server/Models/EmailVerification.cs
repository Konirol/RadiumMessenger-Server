namespace RadiumMessenger.Server.Models
{
    public class EmailVerification
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string CodeHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}