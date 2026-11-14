namespace RadiumMessenger.Server.Models
{
    public partial class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public int SenderId { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDelivered { get; set; } = false;

        public bool IsRead { get; set; } = false;
    }
}
