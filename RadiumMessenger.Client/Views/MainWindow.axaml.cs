using Avalonia.Controls;
using RadiumMessenger.Client.ViewModels;

namespace RadiumMessenger.Client.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new AuthViewModel();
        }
    }
}