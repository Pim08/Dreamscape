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
using Microsoft.EntityFrameworkCore;

namespace Dreamscape.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        using (var db = new AppDbContext())
        {
            // Load items for management
            var items = db.Items.ToList();
            ItemsList.ItemsSource = items;
            AssignItemCombo.ItemsSource = items;
            StatsItemCombo.ItemsSource = items;

            // Load users for assignment
            var users = db.Users.Where(u => u.Role == User.ROLE_USER).ToList();
            AssignUserCombo.ItemsSource = users;
        }
    }

    private void NavButton_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb)
        {
            // Check if elements are loaded (prevents error during InitializeComponent)
            if (CreateUsersSection == null) return;
            
            // Hide all sections
            CreateUsersSection.Visibility = Visibility.Collapsed;
            ManageItemsSection.Visibility = Visibility.Collapsed;
            AssignItemsSection.Visibility = Visibility.Collapsed;
            ItemStatsSection.Visibility = Visibility.Collapsed;

            // Show selected section
            if (rb == NavCreateUsers)
                CreateUsersSection.Visibility = Visibility.Visible;
            else if (rb == NavManageItems)
                ManageItemsSection.Visibility = Visibility.Visible;
            else if (rb == NavAssignItems)
                AssignItemsSection.Visibility = Visibility.Visible;
            else if (rb == NavItemStats)
                ItemStatsSection.Visibility = Visibility.Visible;
        }
    }

    // ============ CREATE USER ============
    private async void CreateUser_Click(object sender, RoutedEventArgs e)
    {
        CreateUserMessage.IsOpen = false;

        string username = NewUserUsername.Text.Trim();
        string email = NewUserEmail.Text.Trim();
        string password = NewUserPassword.Password;
        
        // Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            CreateUserMessage.Title = "Error";
            CreateUserMessage.Message = "All fields are required";
            CreateUserMessage.Severity = InfoBarSeverity.Error;
            CreateUserMessage.IsOpen = true;
            return;
        }

        int role = int.Parse(((ComboBoxItem)NewUserRole.SelectedItem).Tag.ToString());

        using (var db = new AppDbContext())
        {
            // Check if username exists
            if (db.Users.Any(u => u.username.ToLower() == username.ToLower()))
            {
                CreateUserMessage.Title = "Error";
                CreateUserMessage.Message = "Username already exists";
                CreateUserMessage.Severity = InfoBarSeverity.Error;
                CreateUserMessage.IsOpen = true;
                return;
            }

            // Check if email exists
            if (db.Users.Any(u => u.Emailadress.ToLower() == email.ToLower()))
            {
                CreateUserMessage.Title = "Error";
                CreateUserMessage.Message = "Email already exists";
                CreateUserMessage.Severity = InfoBarSeverity.Error;
                CreateUserMessage.IsOpen = true;
                return;
            }

            // Create new user with BCrypt hashed password
            var newUser = new User
            {
                username = username,
                Emailadress = email,
                password_hash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            CreateUserMessage.Title = "Success";
            CreateUserMessage.Message = $"User '{username}' created successfully!";
            CreateUserMessage.Severity = InfoBarSeverity.Success;
            CreateUserMessage.IsOpen = true;

            // Clear form
            NewUserUsername.Text = "";
            NewUserEmail.Text = "";
            NewUserPassword.Password = "";
            NewUserRole.SelectedIndex = 0;

            // Refresh user list for assignment
            LoadData();
        }
    }

    // ============ MANAGE ITEMS ============
    private async void CreateItem_Click(object sender, RoutedEventArgs e)
    {
        ManageItemMessage.IsOpen = false;

        string naam = ItemNaam.Text.Trim();
        
        if (string.IsNullOrWhiteSpace(naam))
        {
            ManageItemMessage.Title = "Error";
            ManageItemMessage.Message = "Item name is required";
            ManageItemMessage.Severity = InfoBarSeverity.Error;
            ManageItemMessage.IsOpen = true;
            return;
        }

        using (var db = new AppDbContext())
        {
            var newItem = new Item
            {
                Naam = naam,
                Beschrijving = ItemBeschrijving.Text.Trim(),
                Type = ((ComboBoxItem)ItemType.SelectedItem).Content.ToString(),
                Zeldzaamheid = ((ComboBoxItem)ItemZeldzaamheid.SelectedItem).Content.ToString(),
                Kracht = (int)ItemKracht.Value,
                Snelheid = (int)ItemSnelheid.Value,
                Duurzaamheid = (int)ItemDuurzaamheid.Value,
                MagischeEigenschap = ItemMagischeEigenschap.Text.Trim()
            };

            db.Items.Add(newItem);
            await db.SaveChangesAsync();

            ManageItemMessage.Title = "Success";
            ManageItemMessage.Message = $"Item '{naam}' created successfully!";
            ManageItemMessage.Severity = InfoBarSeverity.Success;
            ManageItemMessage.IsOpen = true;

            ClearItemForm();
            LoadData();
        }
    }

    private async void UpdateItem_Click(object sender, RoutedEventArgs e)
    {
        ManageItemMessage.IsOpen = false;

        if (string.IsNullOrWhiteSpace(ItemId.Text))
        {
            ManageItemMessage.Title = "Error";
            ManageItemMessage.Message = "Please select an item to update";
            ManageItemMessage.Severity = InfoBarSeverity.Error;
            ManageItemMessage.IsOpen = true;
            return;
        }

        int itemId = int.Parse(ItemId.Text);

        using (var db = new AppDbContext())
        {
            var item = db.Items.FirstOrDefault(i => i.Id == itemId);
            
            if (item == null)
            {
                ManageItemMessage.Title = "Error";
                ManageItemMessage.Message = "Item not found";
                ManageItemMessage.Severity = InfoBarSeverity.Error;
                ManageItemMessage.IsOpen = true;
                return;
            }

            item.Naam = ItemNaam.Text.Trim();
            item.Beschrijving = ItemBeschrijving.Text.Trim();
            item.Type = ((ComboBoxItem)ItemType.SelectedItem).Content.ToString();
            item.Zeldzaamheid = ((ComboBoxItem)ItemZeldzaamheid.SelectedItem).Content.ToString();
            item.Kracht = (int)ItemKracht.Value;
            item.Snelheid = (int)ItemSnelheid.Value;
            item.Duurzaamheid = (int)ItemDuurzaamheid.Value;
            item.MagischeEigenschap = ItemMagischeEigenschap.Text.Trim();

            await db.SaveChangesAsync();

            ManageItemMessage.Title = "Success";
            ManageItemMessage.Message = $"Item '{item.Naam}' updated successfully!";
            ManageItemMessage.Severity = InfoBarSeverity.Success;
            ManageItemMessage.IsOpen = true;

            ClearItemForm();
            LoadData();
        }
    }

    private async void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        ManageItemMessage.IsOpen = false;

        if (string.IsNullOrWhiteSpace(ItemId.Text))
        {
            ManageItemMessage.Title = "Error";
            ManageItemMessage.Message = "Please select an item to delete";
            ManageItemMessage.Severity = InfoBarSeverity.Error;
            ManageItemMessage.IsOpen = true;
            return;
        }

        int itemId = int.Parse(ItemId.Text);

        // Show confirmation dialog
        ContentDialog deleteDialog = new ContentDialog
        {
            Title = "Confirm Deletion",
            Content = "Are you sure you want to delete this item? This action cannot be undone.",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        var result = await deleteDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            using (var db = new AppDbContext())
            {
                var item = db.Items.FirstOrDefault(i => i.Id == itemId);
                
                if (item != null)
                {
                    // Remove from inventories first
                    var inventoryItems = db.InventoryItems.Where(ii => ii.ItemId == itemId).ToList();
                    db.InventoryItems.RemoveRange(inventoryItems);
                    
                    // Remove the item
                    db.Items.Remove(item);
                    await db.SaveChangesAsync();

                    ManageItemMessage.Title = "Success";
                    ManageItemMessage.Message = "Item deleted successfully!";
                    ManageItemMessage.Severity = InfoBarSeverity.Success;
                    ManageItemMessage.IsOpen = true;

                    ClearItemForm();
                    LoadData();
                }
            }
        }
    }

    private void ItemsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsList.SelectedItem is Item selectedItem)
        {
            ItemId.Text = selectedItem.Id.ToString();
            ItemNaam.Text = selectedItem.Naam;
            ItemBeschrijving.Text = selectedItem.Beschrijving;
            ItemKracht.Value = selectedItem.Kracht;
            ItemSnelheid.Value = selectedItem.Snelheid;
            ItemDuurzaamheid.Value = selectedItem.Duurzaamheid;
            ItemMagischeEigenschap.Text = selectedItem.MagischeEigenschap;

            // Set type
            for (int i = 0; i < ItemType.Items.Count; i++)
            {
                if (((ComboBoxItem)ItemType.Items[i]).Content.ToString() == selectedItem.Type)
                {
                    ItemType.SelectedIndex = i;
                    break;
                }
            }

            // Set rarity
            for (int i = 0; i < ItemZeldzaamheid.Items.Count; i++)
            {
                if (((ComboBoxItem)ItemZeldzaamheid.Items[i]).Content.ToString() == selectedItem.Zeldzaamheid)
                {
                    ItemZeldzaamheid.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void ClearItemForm_Click(object sender, RoutedEventArgs e)
    {
        ClearItemForm();
    }

    private void ClearItemForm()
    {
        ItemId.Text = "";
        ItemNaam.Text = "";
        ItemBeschrijving.Text = "";
        ItemType.SelectedIndex = 0;
        ItemZeldzaamheid.SelectedIndex = 0;
        ItemKracht.Value = 0;
        ItemSnelheid.Value = 0;
        ItemDuurzaamheid.Value = 0;
        ItemMagischeEigenschap.Text = "";
        ItemsList.SelectedItem = null;
    }

    // ============ ASSIGN ITEMS ============
    private async void AssignItem_Click(object sender, RoutedEventArgs e)
    {
        AssignItemMessage.IsOpen = false;

        if (AssignUserCombo.SelectedItem == null || AssignItemCombo.SelectedItem == null)
        {
            AssignItemMessage.Title = "Error";
            AssignItemMessage.Message = "Please select both a player and an item";
            AssignItemMessage.Severity = InfoBarSeverity.Error;
            AssignItemMessage.IsOpen = true;
            return;
        }

        var selectedUser = (User)AssignUserCombo.SelectedItem;
        var selectedItem = (Item)AssignItemCombo.SelectedItem;
        int quantity = (int)AssignQuantity.Value;

        using (var db = new AppDbContext())
        {
            // Check if user already has this item
            var existingInventory = db.InventoryItems
                .FirstOrDefault(ii => ii.UserId == selectedUser.Id && ii.ItemId == selectedItem.Id);

            if (existingInventory != null)
            {
                // Update quantity
                existingInventory.Quantity += quantity;
            }
            else
            {
                // Create new inventory item
                var newInventory = new InventoryItem
                {
                    UserId = selectedUser.Id,
                    ItemId = selectedItem.Id,
                    Quantity = quantity
                };
                db.InventoryItems.Add(newInventory);
            }

            await db.SaveChangesAsync();

            AssignItemMessage.Title = "Success";
            AssignItemMessage.Message = $"Assigned {quantity}x '{selectedItem.Naam}' to {selectedUser.username}";
            AssignItemMessage.Severity = InfoBarSeverity.Success;
            AssignItemMessage.IsOpen = true;

            AssignQuantity.Value = 1;
        }
    }

    // ============ ITEM STATISTICS ============
    private void StatsItemCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StatsItemCombo.SelectedItem is Item selectedItem)
        {
            using (var db = new AppDbContext())
            {
                var inventoryData = db.InventoryItems
                    .Include(ii => ii.User)
                    .Where(ii => ii.ItemId == selectedItem.Id)
                    .ToList();

                int playerCount = inventoryData.Count;
                int totalQuantity = inventoryData.Sum(ii => ii.Quantity);

                StatsItemName.Text = selectedItem.Naam;
                StatsPlayerCount.Text = $"Number of players with this item: {playerCount}";
                StatsTotalQuantity.Text = $"Total quantity in circulation: {totalQuantity}";

                // Create display objects for the list
                var playerStats = inventoryData.Select(ii => new
                {
                    Username = ii.User.username,
                    Quantity = ii.Quantity
                }).ToList();

                StatsPlayersList.ItemsSource = playerStats;
            }
        }
    }

    // ============ NAVIGATION ============
    private void GoToPlayerHome_Click(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HomePage));
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        User.LoggedInUser = null;
        Frame.Navigate(typeof(LoginPage));
    }
}
