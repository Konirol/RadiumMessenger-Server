using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RadiumMessenger.Client.DTOs;
using RadiumMessenger.Client.Services;
using System.Linq;
using System.Threading.Tasks;

namespace RadiumMessenger.Client.ViewModels
{
    public partial class AuthViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public AuthViewModel()
        {
            _apiService = new ApiService();
        }

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                ErrorMessage = string.Empty;

                var result = await _apiService.PostAsync<LoginDto, LoginResponse>(
                    "api/Auth/login",
                    new LoginDto
                    {
                        Email = Email,
                        Password = Password
                    });

                if (result != null)
                {
                    var chatWindow = new Views.ChatWindow(result.Id);
                    chatWindow.Show();

                    App.Current!.ApplicationLifetime
                        .GetType();

                    foreach (var window in Avalonia.Application.Current!
                        .ApplicationLifetime is
                        Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                        ? desktop.Windows.ToList()
                        : Enumerable.Empty<Window>())
                    {
                        if (window is Views.MainWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                }
            }
            catch
            {
                ErrorMessage = "Не удалось войти. Проверьте email и пароль.";
            }
        }

        public class LoginResponse
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }
    }
}