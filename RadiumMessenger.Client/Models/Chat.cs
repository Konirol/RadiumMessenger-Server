using System;
using System.Collections.Generic;
using System.Linq;
using Tmds.DBus.Protocol;

namespace RadiumMessenger.Client.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int FirstUserId { get; set; }
        public int SecondUserId { get; set; }
        public List<Message> Messages { get; set; } = new();
        public string OtherUsername { get; set; } = string.Empty;
        public string AvatarLetter =>
            string.IsNullOrWhiteSpace(OtherUsername)
                ? "?"
                : OtherUsername.Trim().Substring(0, 1).ToUpper();
        public string AvatarColor
        {
            get
            {
                string[] colors =
                {
                    "#5865F2",
                    "#E67E45",
                    "#299D80",
                    "#B45AC7",
                    "#D94F70",
                    "#368BCB",
                    "#A68B32"
                };

                int index = Math.Abs(OtherUsername.GetHashCode()) % colors.Length;

                return colors[index];
            }
        }
        public string LastMessageText =>
            Messages == null || Messages.Count == 0
                ? "Нет сообщений"
                : Messages.OrderByDescending(m => m.SentAt)
                           .First().Text;
    }
}