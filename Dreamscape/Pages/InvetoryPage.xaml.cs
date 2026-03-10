using Dreamscape.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dreamscape.Pages
{
    public sealed partial class InvetoryPage : Page
    {
        private List<InventoryItem> _allInventoryItems;

        public InvetoryPage()
        {
            this.InitializeComponent();
            LoadInventory();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // Refresh inventory when navigating to this page
            LoadInventory();
        }

        private void LoadInventory()
        {
            using var db = new AppDbContext();

            var items = db.InventoryItems
                .Include(i => i.Item)
                .Where(i => i.UserId == User.LoggedInUser.Id)
                .ToList();

            _allInventoryItems = items;
            InventoryList.ItemsSource = _allInventoryItems;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                InventoryList.ItemsSource = _allInventoryItems;
            }
            else
            {
                var filteredItems = _allInventoryItems.Where(invItem =>
                    invItem.Item?.Naam?.ToLower().Contains(searchText) == true ||
                    invItem.Item?.Beschrijving?.ToLower().Contains(searchText) == true ||
                    invItem.Item?.Type?.ToLower().Contains(searchText) == true
                ).ToList();

                InventoryList.ItemsSource = filteredItems;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }
    }
}