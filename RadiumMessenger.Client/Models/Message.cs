using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using System;

namespace RadiumMessenger.Client.Models
{
    public partial class Message : ObservableObject
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        [ObservableProperty]
        private bool isDelivered;
        [ObservableProperty]
        private bool isRead;
        public bool CanDeleteForEveryone { get; set; }
        public string StatusText => IsDelivered || IsRead ? "√√" : "√";
        public IBrush StatusBrush => new SolidColorBrush(
            Color.Parse(IsRead ? "#3999E8" : "#9298A5"));
        partial void OnIsDeliveredChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
        partial void OnIsReadChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusBrush));
        }
        public string LocalTime => SentAt.ToLocalTime().ToString("HH:mm");
    }
}