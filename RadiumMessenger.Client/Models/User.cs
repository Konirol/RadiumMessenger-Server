namespace RadiumMessenger.Client.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AvatarLetter =>
            string.IsNullOrWhiteSpace(Username)
                ? "?"
                : Username.Trim().Substring(0, 1).ToUpper();
    }
}