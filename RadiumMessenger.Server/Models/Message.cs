using System.ComponentModel.DataAnnotations.Schema;

namespace RadiumMessenger.Server.Models
{
    public partial class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }

        public int SenderId { get; set; }

        [NotMapped]
        public string SenderUsername { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsDelivered { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public bool IsDeletedForFirstUser { get; set; } = false;

        public bool IsDeletedForSecondUser { get; set; } = false;
    }
}
