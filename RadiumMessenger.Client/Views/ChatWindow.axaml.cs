using Avalonia.Controls;
using RadiumMessenger.Client.ViewModels;

namespace RadiumMessenger.Client.Views
{
    public partial class ChatWindow : Window
    {
        public int UserId { get; }

        public ChatWindow(int userId)
        {
            InitializeComponent();

            UserId = userId;
            DataContext = new ChatViewModel(userId);
        }
    }
}