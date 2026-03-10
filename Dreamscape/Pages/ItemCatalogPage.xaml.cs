using Dreamscape.Data;
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
using Microsoft.EntityFrameworkCore;
using Windows.System.UserProfile;
using System.Collections.ObjectModel;
using System.Threading.Tasks;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dreamscape.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ItemCatalogPage : Page
{
    private List<Item> _allItems;

    public ItemCatalogPage()
    {
        this.InitializeComponent();
        LoadItems();
    }

    private void LoadItems()
    {
        using var db = new AppDbContext();
        _allItems = db.Items.ToList();
        ItemList.ItemsSource = _allItems;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = SearchBox.Text.ToLower().Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            ItemList.ItemsSource = _allItems;
        }
        else
        {
            var filteredItems = _allItems.Where(item =>
                item.Naam?.ToLower().Contains(searchText) == true ||
                item.Beschrijving?.ToLower().Contains(searchText) == true ||
                item.Type?.ToLower().Contains(searchText) == true ||
                item.MagischeEigenschap?.ToLower().Contains(searchText) == true ||
                item.Zeldzaamheid?.ToLower().Contains(searchText) == true
            ).ToList();

            ItemList.ItemsSource = filteredItems;
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HomePage));
    }
}
