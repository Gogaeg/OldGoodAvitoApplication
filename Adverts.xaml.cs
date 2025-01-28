using OldGoodAvitoApplication.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity; // Для методов Include и AsNoTracking
using System.Collections.ObjectModel;

namespace OldGoodAvitoApplication
{
    public partial class Adverts : Page
    {
        // Свойство ViewModel
        public AdvertsViewModel ViewModel { get; set; }

        public Adverts()
        {
            InitializeComponent();
            LoadAdverts();
            this.DataContext = ViewModel;
        }

        private void LoadAdverts()
        {
            try
            {
                using (var context = new Entities())
                {
                    // Загрузка объявлений с связанными данными
                    var adverts = context.Ads
                                         .AsNoTracking() // Опционально, повышает производительность
                                         .Include(a => a.Categories)
                                         .Include(a => a.Types)
                                         .Include(a => a.AdStatuses)
                                         .ToList();

                    // Инициализация ViewModel
                    ViewModel = new AdvertsViewModel
                    {
                        AdvertsList = new ObservableCollection<Ads>(adverts)
                    };

                    // Установка DataContext
                    this.DataContext = ViewModel;

                    // Пополнение ComboBox фильтров
                    PopulateFilters(context, adverts);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке объявлений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateFilters(Entities context, List<Ads> adverts)
        {
            // Пополнение ComboBox "Город" из объявлений
            var cities = adverts.Select(a => a.City)
                               .Where(c => !string.IsNullOrEmpty(c))
                               .Distinct()
                               .OrderBy(c => c)
                               .ToList();
            CitySearch.ItemsSource = cities;

            // Пополнение ComboBox "Статус" из таблицы AdStatuses
            var statuses = context.AdStatuses
                                  .AsNoTracking()
                                  .OrderBy(s => s.StatusName)
                                  .ToList();
            StatusSearch.ItemsSource = statuses;

            // Пополнение ComboBox "Категория" из таблицы Categories
            var categories = context.Categories
                                    .AsNoTracking()
                                    .OrderBy(c => c.CategoryName)
                                    .ToList();
            CategorySearch.ItemsSource = categories;

            // Пополнение ComboBox "Тип" из таблицы Types
            var types = context.Types
                               .AsNoTracking()
                               .OrderBy(t => t.TypeName)
                               .ToList();
            TypeSearch.ItemsSource = types;
        }


        // Обработчики событий фильтрации
        private void CitySearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void StatusSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void CategorySearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void TypeSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }

        private void NameSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateList();
        }

        // Метод для обновления списка объявлений на основе фильтров
        private void UpdateList()
        {
            if (ViewModel == null)
                return;

            try
            {
                using (var context = new Entities())
                {
                    IQueryable<Ads> query = context.Ads
                                                 .AsNoTracking()
                                                 .Include(a => a.Categories)
                                                 .Include(a => a.Types)
                                                 .Include(a => a.AdStatuses);

                    // Применение фильтра по городу
                    if (CitySearch.SelectedItem != null)
                    {
                        string selectedCity = CitySearch.SelectedItem.ToString();
                        query = query.Where(ad => ad.City == selectedCity);
                    }

                    // Применение фильтра по статусу
                    if (StatusSearch.SelectedItem is AdStatuses selectedStatus)
                    {
                        query = query.Where(ad => ad.StatusID == selectedStatus.StatusID);
                    }

                    // Применение фильтра по категории
                    if (CategorySearch.SelectedItem is Categories selectedCategory)
                    {
                        query = query.Where(ad => ad.CategoryID == selectedCategory.CategoryID);
                    }

                    // Применение фильтра по типу
                    if (TypeSearch.SelectedItem is Types selectedType)
                    {
                        query = query.Where(ad => ad.TypeID == selectedType.TypeID);
                    }

                    // Применение фильтра по названию
                    if (!string.IsNullOrWhiteSpace(NameSearch.Text))
                    {
                        var searchText = NameSearch.Text.Trim().ToLower();
                        query = query.Where(ad => ad.Title != null && ad.Title.ToLower().Contains(searchText));
                    }

                    var filteredAds = query.ToList();

                    // Обновление ObservableCollection
                    ViewModel.AdvertsList.Clear();
                    foreach (var advert in filteredAds)
                    {
                        ViewModel.AdvertsList.Add(advert);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при фильтрации объявлений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обработчик нажатия кнопки "Очистить"
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            CitySearch.SelectedIndex = -1;
            StatusSearch.SelectedIndex = -1;
            CategorySearch.SelectedIndex = -1;
            TypeSearch.SelectedIndex = -1;
            NameSearch.Text = "";

            UpdateList();
        }
    }

    // ViewModel для объявлений
    public class AdvertsViewModel
    {
        public ObservableCollection<Ads> AdvertsList { get; set; }

        public AdvertsViewModel()
        {
            AdvertsList = new ObservableCollection<Ads>();
        }
    }
}
