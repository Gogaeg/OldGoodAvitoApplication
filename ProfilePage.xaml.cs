using OldGoodAvitoApplication.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity; // Для метода Include
using System;

namespace OldGoodAvitoApplication
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public UserProfileViewModel ViewModel { get; set; }

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
            this.DataContext = ViewModel;
        }

        private void LoadUserProfile()
        {
            if (CurrentUser.User == null)
            {
                // Если пользователь не авторизован, перенаправляем на страницу авторизации
                MessageBox.Show("Вы не авторизованы. Пожалуйста, войдите в систему.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.Navigate(new AuthPage());
                return;
            }

            using (var context = new Entities())
            {
                var user = context.Users
                                  .Include(u => u.Ads.Select(ad => ad.Categories))
                                  .Include(u => u.Ads.Select(ad => ad.Types))
                                  .Include(u => u.Ads.Select(ad => ad.AdStatuses))
                                  .FirstOrDefault(u => u.UserID == CurrentUser.User.UserID);
                if (user != null)
                {
                    // Получаем объявления пользователя
                    var userAds = context.Ads
                                         .Include(a => a.Categories)
                                         .Include(a => a.Types)
                                         .Include(a => a.AdStatuses)
                                         .Where(ad => ad.UserID == user.UserID)
                                         .ToList();

                    // Получаем завершенные объявления (StatusID = 2)
                    var completedAds = userAds.Where(ad => ad.StatusID == 2).ToList();
                    int profit = completedAds.Sum(ad => ad.Price);

                    ViewModel = new UserProfileViewModel
                    {
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        Profit = profit,
                        UserAds = userAds
                    };

                    // Обновляем DataContext
                    this.DataContext = ViewModel;
                }
                else
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.Navigate(new AuthPage());
                }
            }
        }

        // Обработчик нажатия кнопки "Назад"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Навигация обратно к странице объявлений
                NavigationService.Navigate(new Adverts());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при навигации назад: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик нажатия кнопки "Создать объявление"
        private void CreateAdButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAdWindow createAdWindow = new CreateAdWindow();
            bool? result = createAdWindow.ShowDialog();

            if (result == true)
            {
                // Объявление успешно создано, обновляем список
                LoadUserProfile();
            }
            // Если result == false или null, пользователь отменил создание объявления
        }
    }

    // ViewModel для профиля пользователя
    public class UserProfileViewModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int Profit { get; set; }
        public List<Ads> UserAds { get; set; }
    }
}
