using OldGoodAvitoApplication.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;

namespace OldGoodAvitoApplication
{
    /// <summary>
    /// Логика взаимодействия для CreateAdWindow.xaml
    /// </summary>
    public partial class CreateAdWindow : Window
    {
        private List<Categories> categoriesList;
        private List<Types> typesList;

        public CreateAdWindow()
        {
            InitializeComponent();
            LoadCategoriesAndTypes();
        }

        /// <summary>
        /// Загрузка данных для ComboBox категорий и типов
        /// </summary>
        private void LoadCategoriesAndTypes()
        {
            using (var context = new Entities())
            {
                categoriesList = context.Categories.ToList();
                typesList = context.Types.ToList();
            }

            CategoryComboBox.ItemsSource = categoriesList;
            TypeComboBox.ItemsSource = typesList;
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Обзор..." для выбора фото
        /// </summary>
        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                PhotoTextBox.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Валидация ввода только цифр для поля "Цена"
        /// </summary>
        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); // Разрешены только цифры
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Сохранить"
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбор данных из полей ввода
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();
            string photoPath = PhotoTextBox.Text.Trim();
            int? categoryId = CategoryComboBox.SelectedValue as int?;
            int? typeId = TypeComboBox.SelectedValue as int?;

            // Валидация обязательных полей
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) ||
                string.IsNullOrEmpty(city) || string.IsNullOrEmpty(address) ||
                string.IsNullOrEmpty(priceText) || categoryId == null || typeId == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация поля "Цена"
            if (!int.TryParse(priceText, out int price) || price < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Валидация пути к фото (опционально)
            if (!string.IsNullOrEmpty(photoPath) && !System.IO.File.Exists(photoPath))
            {
                MessageBox.Show("Указанный путь к фото не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Создание нового объекта объявления
            Ads newAd = new Ads
            {
                Title = title,
                Description = description,
                City = city,
                Address = address,
                Price = price,
                photo = string.IsNullOrEmpty(photoPath) ? null : photoPath,
                CategoryID = categoryId.Value,
                TypeID = typeId.Value,
                UserID = CurrentUser.User.UserID,
                PublicationDate = DateTime.Now,
                StatusID = 1 // Предполагается, что статус "1" означает "Активное" или аналогичное
            };

            // Сохранение в базу данных
            using (var context = new Entities())
            {
                context.Ads.Add(newAd);
                try
                {
                    context.SaveChanges();
                    MessageBox.Show("Объявление успешно создано.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // Закрывает окно с результатом успешного создания
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении объявления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Отмена"
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Закрывает окно без сохранения
        }
    }
}
