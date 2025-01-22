using OldGoodAvitoApplication.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OldGoodAvitoApplication
{
    public partial class Adverts : Page
    {
        
        private List<Ads> _ads;              
        private List<AdStatuses> _statuses;  
        private List<Categories> _categories;
        private List<Types> _types;          

        public Adverts()
        {
            InitializeComponent();

            _ads = Entities.GetContext().Ads.ToList();
            _statuses = Entities.GetContext().AdStatuses.ToList();
            _categories = Entities.GetContext().Categories.ToList();
            _types = Entities.GetContext().Types.ToList();

            ListAdverts.ItemsSource = _ads;


            var cityList = _ads
                .Select(ad => ad.City)
                .Distinct()
                .OrderBy(city => city)
                .ToList();
            CitySearch.ItemsSource = cityList;

            StatusSearch.ItemsSource = _statuses;
            StatusSearch.DisplayMemberPath = "StatusName";  

            CategorySearch.ItemsSource = _categories;
            CategorySearch.DisplayMemberPath = "CategoryName";

            TypeSearch.ItemsSource = _types;
            TypeSearch.DisplayMemberPath = "TypeName";

        }

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

        private void UpdateList()
        {
            var filtered = _ads;

        
            if (CitySearch.SelectedItem != null)
            {
                string selectedCity = CitySearch.SelectedItem.ToString();
                filtered = filtered
                    .Where(ad => ad.City == selectedCity)
                    .ToList();
            }

            if (StatusSearch.SelectedItem is AdStatuses selectedStatus)
            {
                filtered = filtered
                    .Where(ad => ad.StatusID == selectedStatus.StatusID)
                    .ToList();
            }

            if (CategorySearch.SelectedItem is Categories selectedCategory)
            {
                filtered = filtered
                    .Where(ad => ad.CategoryID == selectedCategory.CategoryID)
                    .ToList();
            }

            if (TypeSearch.SelectedItem is Types selectedType)
            {
                filtered = filtered
                    .Where(ad => ad.TypeID == selectedType.TypeID)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(NameSearch.Text))
            {
                var searchText = NameSearch.Text.Trim().ToLower();
                filtered = filtered
                    .Where(ad => ad.Title != null &&
                                 ad.Title.ToLower().Contains(searchText))
                    .ToList();
            }

            ListAdverts.ItemsSource = filtered;
        }

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
}