using OldGoodAvitoApplication.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;

namespace OldGoodAvitoApplication
{
    public partial class EditAdWindow : Window
    {
        private Ads AdToEdit;

        public EditAdWindow(Ads ad)
        {
            InitializeComponent();
            AdToEdit = ad;
            LoadCategoriesAndTypes();
            PopulateFields();
        }

        private void LoadCategoriesAndTypes()
        {
            using (var context = new Entities())
            {
                var categoriesList = context.Categories.ToList();
                var typesList = context.Types.ToList();

                CategoryComboBox.ItemsSource = categoriesList;
                TypeComboBox.ItemsSource = typesList;
            }
        }

        private void PopulateFields()
        {
            TitleTextBox.Text = AdToEdit.Title;
            DescriptionTextBox.Text = AdToEdit.Description;
            CityTextBox.Text = AdToEdit.City;
            AddressTextBox.Text = AdToEdit.Address;
            PriceTextBox.Text = AdToEdit.Price.ToString();
            PhotoTextBox.Text = AdToEdit.photo;
            StatusComboBox.SelectedValue = AdToEdit.StatusID;

            CategoryComboBox.SelectedValue = AdToEdit.CategoryID;
            TypeComboBox.SelectedValue = AdToEdit.TypeID;

            if (!string.IsNullOrEmpty(AdToEdit.photo) && File.Exists(AdToEdit.photo))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(AdToEdit.photo, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    PhotoPreview.Source = bitmap;
                }
                catch
                {
                    PhotoPreview.Source = null;
                }
            }
            else
            {
                PhotoPreview.Source = null;
            }
        }

        private void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                PhotoTextBox.Text = openFileDialog.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    PhotoPreview.Source = bitmap;
                }
                catch
                {
                    PhotoPreview.Source = null;
                }
            }
        }

        private void PriceTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+"); 
            e.Handled = regex.IsMatch(e.Text);
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сбор данных из полей ввода
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string city = CityTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();
            string photoPath = PhotoTextBox.Text.Trim();
            int statusId = GetSelectedStatusId();

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
            if (!string.IsNullOrEmpty(photoPath) && !File.Exists(photoPath))
            {
                MessageBox.Show("Указанный путь к фото не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Обновление объекта объявления
            using (var context = new Entities())
            {
                var adToUpdate = context.Ads.FirstOrDefault(ad => ad.AdID == AdToEdit.AdID);
                if (adToUpdate != null)
                {
                    adToUpdate.Title = title;
                    adToUpdate.Description = description;
                    adToUpdate.City = city;
                    adToUpdate.Address = address;
                    adToUpdate.Price = price;
                    adToUpdate.photo = string.IsNullOrEmpty(photoPath) ? null : photoPath;
                    adToUpdate.CategoryID = categoryId.Value;
                    adToUpdate.TypeID = typeId.Value;
                    adToUpdate.StatusID = statusId;

                    try
                    {
                        context.SaveChanges();
                        MessageBox.Show("Объявление успешно обновлено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true; // Закрывает окно с результатом успешного обновления
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при обновлении объявления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Объявление не найдено в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Получение выбранного StatusID из ComboBox
        /// </summary>
        /// <returns></returns>
        private int GetSelectedStatusId()
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag != null && int.TryParse(selectedItem.Tag.ToString(), out int statusId))
                {
                    return statusId;
                }
            }
            // Значение по умолчанию, если ничего не выбрано
            return 1;
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
