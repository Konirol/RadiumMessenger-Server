using Avalonia.Controls;
using Avalonia.Threading;
using System.Collections.Specialized;
using RadiumMessenger.Client.ViewModels;

namespace RadiumMessenger.Client.Views
{
    public partial class ChatWindow : Window
    {
        public int UserId { get; }

        private readonly ChatViewModel _viewModel;

        public ChatWindow(int userId)
        {
            InitializeComponent();

            UserId = userId;

            _viewModel = new ChatViewModel(userId);
            DataContext = _viewModel;

            _viewModel.Messages.CollectionChanged += OnMessagesChanged;

            Loaded += (_, _) => ScrollToBottom();
        }

        private void OnMessagesChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.UIThread.Post(ScrollToBottom);
        }

        private void ScrollToBottom()
        {
            MessagesScrollViewer.ScrollToEnd();
        }
    }
}