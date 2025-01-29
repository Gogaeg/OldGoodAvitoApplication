using OldGoodAvitoApplication.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity; // Для метода Include
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace OldGoodAvitoApplication
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public UserProfileViewModel ViewModel { get; set; }

        private ICollectionView _advertsView;

        public ProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
            this.DataContext = ViewModel;

            // Инициализация CollectionView для фильтрации
            if (ViewModel.UserAds != null)
            {
                _advertsView = CollectionViewSource.GetDefaultView(ViewModel.UserAds);
                _advertsView.Filter = FilterAdverts;
            }
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
                        UserAds = new ObservableCollection<Ads>(userAds)
                    };

                    // Обновляем DataContext
                    this.DataContext = ViewModel;

                    // Инициализируем CollectionView для фильтрации
                    _advertsView = CollectionViewSource.GetDefaultView(ViewModel.UserAds);
                    _advertsView.Filter = FilterAdverts;
                }
                else
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.Navigate(new AuthPage());
                }
            }
        }

        /// <summary>
        /// Метод фильтрации объявлений на основе состояния чекбокса
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool FilterAdverts(object item)
        {
            if (item is Ads ad)
            {
                if (ShowOnlyCompletedCheckBox.IsChecked == true)
                {
                    return ad.StatusID == 2;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обработчик события Checked чекбокса
        /// </summary>
        private void ShowOnlyCompleted_Checked(object sender, RoutedEventArgs e)
        {
            _advertsView.Refresh();
        }

        /// <summary>
        /// Обработчик события Unchecked чекбокса
        /// </summary>
        private void ShowOnlyCompleted_Unchecked(object sender, RoutedEventArgs e)
        {
            _advertsView.Refresh();
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

                // Обновляем CollectionView после обновления списка
                if (_advertsView != null)
                {
                    _advertsView = CollectionViewSource.GetDefaultView(ViewModel.UserAds);
                    _advertsView.Filter = FilterAdverts;
                }
            }
            // Если result == false или null, пользователь отменил создание объявления
        }

        /// <summary>
        /// Обработчик нажатия пункта контекстного меню "Удалить"
        /// </summary>
        private void DeleteAdMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получение MenuItem
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            // Получение ContextMenu
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
                return;

            // Получение PlacementTarget и его DataContext
            FrameworkElement placementTarget = contextMenu.PlacementTarget as FrameworkElement;
            if (placementTarget == null)
                return;

            Ads selectedAd = placementTarget.DataContext as Ads;
            if (selectedAd == null)
                return;

            // Подтверждение удаления
            MessageBoxResult confirmation = MessageBox.Show($"Вы уверены, что хотите удалить объявление \"{selectedAd.Title}\"?",
                                                            "Подтверждение удаления",
                                                            MessageBoxButton.YesNo,
                                                            MessageBoxImage.Question);

            if (confirmation == MessageBoxResult.Yes)
            {
                using (var context = new Entities())
                {
                    // Получение объявления из базы данных
                    var adToDelete = context.Ads.FirstOrDefault(ad => ad.AdID == selectedAd.AdID);
                    if (adToDelete != null)
                    {
                        context.Ads.Remove(adToDelete);
                        try
                        {
                            context.SaveChanges();
                            MessageBox.Show("Объявление успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            // Обновление списка объявлений
                            LoadUserProfile();

                            // Обновляем CollectionView после обновления списка
                            if (_advertsView != null)
                            {
                                _advertsView = CollectionViewSource.GetDefaultView(ViewModel.UserAds);
                                _advertsView.Filter = FilterAdverts;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении объявления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Объявление не найдено в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия пункта контекстного меню "Редактировать"
        /// </summary>
        private void EditAdMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Получение MenuItem
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            // Получение ContextMenu
            ContextMenu contextMenu = menuItem.Parent as ContextMenu;
            if (contextMenu == null)
                return;

            // Получение PlacementTarget и его DataContext
            FrameworkElement placementTarget = contextMenu.PlacementTarget as FrameworkElement;
            if (placementTarget == null)
                return;

            Ads selectedAd = placementTarget.DataContext as Ads;
            if (selectedAd == null)
                return;

            // Открытие окна редактирования
            EditAdWindow editAdWindow = new EditAdWindow(selectedAd);
            bool? result = editAdWindow.ShowDialog();

            if (result == true)
            {
                // Объявление успешно отредактировано, обновляем список
                LoadUserProfile();

                // Обновляем CollectionView после обновления списка
                if (_advertsView != null)
                {
                    _advertsView = CollectionViewSource.GetDefaultView(ViewModel.UserAds);
                    _advertsView.Filter = FilterAdverts;
                }
            }
            // Если result == false или null, пользователь отменил редактирование
        }
    }

    // ViewModel для профиля пользователя
    public class UserProfileViewModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public int Profit { get; set; }
        public ObservableCollection<Ads> UserAds { get; set; }
    }
}
