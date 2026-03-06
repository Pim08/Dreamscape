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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dreamscape.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TradePage : Page
{
    public TradePage()
    {
        this.InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        using var db = new AppDbContext();

        UserCombo.ItemsSource = db.Users
            .Where(u => u.Id != User.LoggedInUser.Id)
            .ToList();

        ItemCombo.ItemsSource = db.Items.ToList();
        RequestCombo.ItemsSource = db.Items.ToList();
    }

    private void SendTrade_Click(object sender, RoutedEventArgs e)
    {
        var target = UserCombo.SelectedItem as User;
        var offer = ItemCombo.SelectedItem as Item;
        var request = RequestCombo.SelectedItem as Item;

        using var db = new AppDbContext();

        var trade = new Trade
        {
            FromUserId = User.LoggedInUser.Id,
            ToUserId = target.Id,
            ItemOfferedId = offer.Id,
            ItemRequestedId = request.Id,
            Status = "Pending"
        };

        db.Trades.Add(trade);
        db.SaveChanges();
    }
}