using System.ComponentModel.DataAnnotations.Schema;

namespace RadiumMessenger.Server.Models
{
    public class Chat
    {
        public int Id { get; set; }

        public int FirstUserId { get; set; }

        public int SecondUserId { get; set; }

        public bool IsArchivedForFirstUser { get; set; } = false;

        public bool IsArchivedForSecondUser { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string OtherUsername { get; set; } = string.Empty;

        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
