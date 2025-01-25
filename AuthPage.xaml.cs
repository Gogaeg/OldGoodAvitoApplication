using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OldGoodAvitoApplication.Resources;

namespace OldGoodAvitoApplication
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            string enteredLogin = LoginTextBox.Text.Trim();
            string enteredPassword = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(enteredLogin) || string.IsNullOrEmpty(enteredPassword))
            {
                ErrorMessage.Text = "Пожалуйста, введите логин и пароль.";
                return;
            }

            using (var context = Entities.GetContext()) // Замените YourDbContext на ваш контекст БД
            {
                var user = context.Users
                                  .FirstOrDefault(u => u.Login == enteredLogin && u.Password == enteredPassword);

                if (user != null)
                {
                    // Сохранение информации о текущем пользователе
                    CurrentUser.User = user;

                    // Возврат на страницу объявлений
                    NavigationService.Navigate(new Adverts());

                    // Обновление интерфейса MainWindow
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.UpdateLoginButton();
                    }
                }
                else
                {
                    ErrorMessage.Text = "Неверный логин или пароль.";
                }
            }
        }
    }
}
