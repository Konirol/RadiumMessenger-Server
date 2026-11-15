using Microsoft.AspNetCore.SignalR.Client;
using RadiumMessenger.Client.Models;
using System;
using System.Threading.Tasks;

namespace RadiumMessenger.Client.Services
{
    public class SignalRService
    {
        private HubConnection? _connection;

        public event Action<Message>? MessageReceived;

        public event Action<int, bool, bool>? MessageStatusUpdated;

        public event Action<int>? MessageDeleted;

        public async Task ConnectAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://127.0.0.1:7199/messageHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<Message>("ReceiveMessage", message =>
            {
                MessageReceived?.Invoke(message);
            });

            _connection.On<int, bool, bool>("MessageStatusUpdated",
                (messageId, isDelivered, isRead) =>
                {
                    MessageStatusUpdated?.Invoke(messageId, isDelivered, isRead);
                });

            _connection.On<int>("MessageDeleted", messageId =>
            {
                MessageDeleted?.Invoke(messageId);
            });

            await _connection.StartAsync();
        }

        public async Task JoinChatAsync(int chatId, int userId)
        {
            if (_connection == null)
                throw new InvalidOperationException("Нет подключения к серверу.");

            await _connection.InvokeAsync("JoinChat", chatId, userId);
        }

        public async Task LeaveChatAsync(int chatId)
        {
            if (_connection != null)
            {
                await _connection.InvokeAsync("LeaveChat", chatId);
            }
        }

        public async Task MarkAsDeliveredAsync(int messageId, int userId)
        {
            if (_connection == null)
                return;

            await _connection.InvokeAsync("MarkAsDelivered", messageId, userId);
        }

        public async Task MarkAsReadAsync(int messageId, int userId)
        {
            if (_connection == null)
                return;

            await _connection.InvokeAsync("MarkAsRead", messageId, userId);
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}