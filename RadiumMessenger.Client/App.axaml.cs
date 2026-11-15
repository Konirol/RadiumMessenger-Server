using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using RadiumMessenger.Client.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RadiumMessenger.Client
{
    public partial class App : Application
    {
        private Process? _serverProcess;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                DisableAvaloniaDataAnnotationValidation();

                desktop.Exit += (_, _) =>
                {
                    try
                    {
                        if (_serverProcess != null &&
                            !_serverProcess.HasExited)
                        {
                            _serverProcess.Kill(true);
                            _serverProcess.Dispose();
                        }
                    }
                    catch
                    {
                    }
                };

                try
                {
                    await StartServerAsync();

                    desktop.MainWindow = new MainWindow();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    desktop.Shutdown();
                    return;
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task StartServerAsync()
        {
            string serverPath = Path.Combine(
                AppContext.BaseDirectory,
                "Server",
                "RadiumMessenger.Server.exe");

            if (!File.Exists(serverPath))
                throw new FileNotFoundException(
                    "Файл сервера не найден.", serverPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                WorkingDirectory = Path.GetDirectoryName(serverPath)!,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _serverProcess = Process.Start(startInfo);

            if (_serverProcess == null)
                throw new Exception("Не удалось запустить сервер.");

            for (int i = 0; i < 40; i++)
            {
                if (_serverProcess.HasExited)
                    throw new Exception("Сервер неожиданно завершился.");

                try
                {
                    using var client = new TcpClient();

                    await client.ConnectAsync("127.0.0.1", 7199);

                    return;
                }
                catch
                {
                    await Task.Delay(500);
                }
            }

            throw new Exception("Сервер не успел запуститься.");
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            var plugins =
                BindingPlugins.DataValidators
                    .OfType<DataAnnotationsValidationPlugin>()
                    .ToArray();

            foreach (var plugin in plugins)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}