using System.Windows;
using System.Windows.Controls;

namespace OldGoodAvitoApplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateLoginButton();
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.User == null)
            {
                // Если пользователь не авторизован, перейти на страницу авторизации
                MainFrame.Navigate(new AuthPage());
            }
            else
            {
                // Если пользователь авторизован, перейти на страницу профиля
                MainFrame.Navigate(new ProfilePage()); // Предполагается, что есть страница ProfilePage
            }
        }

        public void UpdateLoginButton()
        {
            if (CurrentUser.User != null)
            {
                LogInButton.Content = "Профиль";
            }
            else
            {
                LogInButton.Content = "Войти";
            }
        }
    }
}

