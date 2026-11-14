using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace RadiumMessenger.Client.Models
{
    public partial class Message : ObservableObject
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        [ObservableProperty]
        private bool isDelivered;
        [ObservableProperty]
        private bool isRead;
        public string StatusText => IsDelivered || IsRead ? "√√" : "√";
        public string StatusColor => IsRead ? "3999E8" : "#9298A5";
        partial void OnIsDeliveredChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }
        partial void OnIsReadChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(StatusColor));
        }
        public string LocalTime => SentAt.ToLocalTime().ToString("HH:mm");
    }
}