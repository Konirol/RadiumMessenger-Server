namespace RadiumMessenger.Client.DTOs
{
    public class SendMessageDto
    {
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}