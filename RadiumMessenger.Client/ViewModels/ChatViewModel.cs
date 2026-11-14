using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadiumMessenger.Client.DTOs;
using RadiumMessenger.Client.Models;
using RadiumMessenger.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace RadiumMessenger.Client.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        private readonly SignalRService _signalRService;

        public ObservableCollection<Chat> Chats { get; } = new();

        [ObservableProperty]
        private Chat? selectedChat;

        [ObservableProperty]
        private string messageText = string.Empty;

        [ObservableProperty]
        private string searchQuery = string.Empty;

        public ObservableCollection<User> SearchResults {  get; } = new();

        partial void OnSelectedChatChanged(Chat? value)
        {
            Messages.Clear();

            if (value != null )
            {
                foreach (var message in value.Messages)
                {
                    Messages.Add(message);
                }
            }
        }

        private async Task JoinChatAsync(int chatId)
        {
            try
            {
                await _signalRService.JoinChatAsync(chatId, UserId);
            }
            catch
            {

            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (SelectedChat == null || string.IsNullOrWhiteSpace(MessageText))
                return;

            try
            {
                var message = await _apiService.PostAsync<SendMessageDto, Message>(
                    "api/Message/send",
                    new SendMessageDto
                    {
                        ChatId = SelectedChat.Id,
                        SenderId = UserId,
                        Text = MessageText
                    });

                if (message != null)
                {
                    Messages.Add(message);
                    MessageText = string.Empty;
                }
            }
            catch
            {

            }
        }

        public ObservableCollection<Message> Messages { get; } = new();

        public int UserId { get; }

        public ChatViewModel(int userId)
        {
            UserId = userId;
            _apiService = new ApiService();
            _signalRService = new SignalRService();

            _signalRService.MessageReceived += message =>
            {
                if (message.SenderId == UserId)
                    return;

                _ = _signalRService.MarkAsDeliveredAsync(message.Id, userId);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var chat = Chats.FirstOrDefault(c => c.Id == message.ChatId);

                    if (chat != null)
                    {
                        chat.Messages.Add(message);

                        if (SelectedChat?.Id == message.ChatId)
                        {
                            Messages.Add(message);
                        }

                        OnPropertyChanged(nameof(Chats));
                    }
                });
            };

            _signalRService.MessageStatusUpdated += (messageId, isDelivered, isRead) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var message = Messages.FirstOrDefault(m => m.Id == messageId);

                    if (message != null)
                    {
                        message.IsDelivered = isDelivered;
                        message.IsRead = isRead;
                    }
                });
            };

            _ = InitializeAsync();

        }

        private async Task InitializeAsync()
        {
            try
            {
                await _signalRService.ConnectAsync();
                await LoadChatsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async Task ConnectSignalRAsync()
        {
            try
            {
                await _signalRService.ConnectAsync();
            }
            catch
            {

            }
        }

        private async Task LoadChatsAsync()
        {
            try
            {
                var chats = await _apiService.GetAsync<List<Chat>>(
                    $"api/Chat/user/{UserId}");

                Chats.Clear();

                if (chats != null)
                {
                    foreach (var chat in chats)
                    {
                        Chats.Add(chat);
                        await JoinChatAsync(chat.Id);
                    }
                }
            }
            catch
            {
                
            }
        }

        [RelayCommand]
        private async Task SearchUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchResults.Clear();
                return;
            }

            try
            {
                var users = await _apiService.GetAsync<List<User>>(
                    $"api/User/search?query={Uri.EscapeDataString(SearchQuery)}");

                SearchResults.Clear();

                if (users != null)
                {
                    foreach (var user in users)
                    {
                        if (user.Id != UserId)
                            SearchResults.Add(user);
                    }
                }
            }
            catch
            {

            }
        }

        [RelayCommand]
        private async Task CreateChat(User user)
        {
            try
            {
                var chat = await _apiService.PostAsync<CreateChatDto, Chat>(
                    "api/Chat/create",
                    new CreateChatDto
                    {
                        FirstUserId = UserId,
                        SecondUserId = user.Id
                    });

                if (chat != null)
                {
                    chat.OtherUsername = user.Username;

                    var existingChat = Chats.FirstOrDefault(c => c.Id == chat.Id);

                    if (existingChat != null)
                    {
                        Chats.Add(chat);
                    }

                    SelectedChat = existingChat ?? chat;
                }

                SearchResults.Clear();
                SearchQuery = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}