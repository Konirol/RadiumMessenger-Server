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

        public async Task ConnectAsync()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7199/messageHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<Message>("ReceiveMessage", message =>
            {
                MessageReceived?.Invoke(message);
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