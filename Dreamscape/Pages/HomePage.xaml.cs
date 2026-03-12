using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Dreamscape.Data;

namespace Dreamscape.Pages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Show admin button only if logged in user is an owner
            if (User.LoggedInUser != null && User.LoggedInUser.Role == User.ROLE_OWNER)
            {
                AdminDashboardButton.Visibility = Visibility.Visible;
            }
            else
            {
                AdminDashboardButton.Visibility = Visibility.Collapsed;
            }
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DashboardPage));
        }

        private void Catalog_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ItemCatalogPage));
        }

        private void Trade_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TradePage));
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(InvetoryPage));
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            User.LoggedInUser = null;
            Frame.Navigate(typeof(LoginPage));
        }
    }
}