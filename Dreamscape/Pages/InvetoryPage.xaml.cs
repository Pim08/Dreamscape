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

            InventoryList.ItemsSource = items;

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }
    }
}