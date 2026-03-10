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

namespace Dreamscape.Pages;

public class TradeViewModel
{
    public int TradeId { get; set; }
    public string FromUserName { get; set; }
    public string OfferedItemName { get; set; }
    public string RequestedItemName { get; set; }
}

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

        // Load users for sending trades
        UserCombo.ItemsSource = db.Users
            .Where(u => u.Id != User.LoggedInUser.Id)
            .ToList();

        // Load items owned by current user for offering
        var userInventory = db.InventoryItems
            .Include(ii => ii.Item)
            .Where(ii => ii.UserId == User.LoggedInUser.Id)
            .Select(ii => ii.Item)
            .ToList();
        
        ItemCombo.ItemsSource = userInventory;
        
        // Load all items for requesting
        RequestCombo.ItemsSource = db.Items.ToList();

        // Load incoming trades
        LoadIncomingTrades();
    }

    private void LoadIncomingTrades()
    {
        using var db = new AppDbContext();

        var incomingTrades = db.Trades
            .Where(t => t.ToUserId == User.LoggedInUser.Id && t.Status == "Pending")
            .ToList();

        var tradeViewModels = new List<TradeViewModel>();

        foreach (var trade in incomingTrades)
        {
            var fromUser = db.Users.Find(trade.FromUserId);
            var offeredItem = db.Items.Find(trade.ItemOfferedId);
            var requestedItem = db.Items.Find(trade.ItemRequestedId);

            tradeViewModels.Add(new TradeViewModel
            {
                TradeId = trade.Id,
                FromUserName = fromUser?.username ?? "Unknown",
                OfferedItemName = offeredItem?.Naam ?? "Unknown Item",
                RequestedItemName = requestedItem?.Naam ?? "Unknown Item"
            });
        }

        IncomingTradesList.ItemsSource = tradeViewModels;
        NoTradesMessage.Visibility = tradeViewModels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SendTrade_Click(object sender, RoutedEventArgs e)
    {
        var target = UserCombo.SelectedItem as User;
        var offer = ItemCombo.SelectedItem as Item;
        var request = RequestCombo.SelectedItem as Item;

        if (target == null || offer == null || request == null)
        {
            ShowNotification("Please select all fields", InfoBarSeverity.Warning);
            return;
        }

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

        ShowNotification("Trade offer sent successfully!", InfoBarSeverity.Success);

        // Clear selections
        UserCombo.SelectedItem = null;
        ItemCombo.SelectedItem = null;
        RequestCombo.SelectedItem = null;
    }

    private void AcceptTrade_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tradeId = (int)button.Tag;

        using var db = new AppDbContext();

        var trade = db.Trades.Find(tradeId);
        if (trade == null || trade.Status != "Pending")
        {
            ShowNotification("Trade not found or already processed", InfoBarSeverity.Error);
            return;
        }

        // Check if the receiver (current user) has the requested item
        var receiverInventory = db.InventoryItems
            .FirstOrDefault(ii => ii.UserId == trade.ToUserId && ii.ItemId == trade.ItemRequestedId);

        if (receiverInventory == null || receiverInventory.Quantity < 1)
        {
            ShowNotification("You don't have the requested item!", InfoBarSeverity.Error);
            return;
        }

        // Check if the sender has the offered item
        var senderInventory = db.InventoryItems
            .FirstOrDefault(ii => ii.UserId == trade.FromUserId && ii.ItemId == trade.ItemOfferedId);

        if (senderInventory == null || senderInventory.Quantity < 1)
        {
            ShowNotification("The sender no longer has the offered item!", InfoBarSeverity.Error);
            return;
        }

        // Execute the trade
        // Remove item from receiver (current user)
        receiverInventory.Quantity--;
        if (receiverInventory.Quantity == 0)
        {
            db.InventoryItems.Remove(receiverInventory);
        }

        // Remove item from sender
        senderInventory.Quantity--;
        if (senderInventory.Quantity == 0)
        {
            db.InventoryItems.Remove(senderInventory);
        }

        // Add offered item to receiver (current user)
        var receiverNewItem = db.InventoryItems
            .FirstOrDefault(ii => ii.UserId == trade.ToUserId && ii.ItemId == trade.ItemOfferedId);

        if (receiverNewItem == null)
        {
            db.InventoryItems.Add(new InventoryItem
            {
                UserId = trade.ToUserId,
                ItemId = trade.ItemOfferedId,
                Quantity = 1
            });
        }
        else
        {
            receiverNewItem.Quantity++;
        }

        // Add requested item to sender
        var senderNewItem = db.InventoryItems
            .FirstOrDefault(ii => ii.UserId == trade.FromUserId && ii.ItemId == trade.ItemRequestedId);

        if (senderNewItem == null)
        {
            db.InventoryItems.Add(new InventoryItem
            {
                UserId = trade.FromUserId,
                ItemId = trade.ItemRequestedId,
                Quantity = 1
            });
        }
        else
        {
            senderNewItem.Quantity++;
        }

        // Update trade status
        trade.Status = "Accepted";

        db.SaveChanges();

        // Get the item name for the notification
        var offeredItem = db.Items.Find(trade.ItemOfferedId);
        ShowNotification($"Trade accepted! You received {offeredItem?.Naam}!", InfoBarSeverity.Success);

        // Reload the page
        LoadData();
    }

    private void RejectTrade_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tradeId = (int)button.Tag;

        using var db = new AppDbContext();

        var trade = db.Trades.Find(tradeId);
        if (trade == null || trade.Status != "Pending")
        {
            ShowNotification("Trade not found or already processed", InfoBarSeverity.Error);
            return;
        }

        trade.Status = "Rejected";
        db.SaveChanges();

        ShowNotification("Trade rejected", InfoBarSeverity.Informational);

        // Reload incoming trades
        LoadIncomingTrades();
    }

    private void ShowNotification(string message, InfoBarSeverity severity)
    {
        NotificationBar.Message = message;
        NotificationBar.Severity = severity;
        NotificationBar.IsOpen = true;
        NotificationBar.Visibility = Visibility.Visible;
    }

    private void NotificationBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        NotificationBar.Visibility = Visibility.Collapsed;
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HomePage));
    }
}