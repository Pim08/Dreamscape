using Dreamscape.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Dreamscape.Pages
{
    public sealed partial class ProfilePage : Page
    {
        private User _currentUser;

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            _currentUser = User.LoggedInUser;

            if (_currentUser == null)
            {
                // If no user is logged in, redirect to login page
                Frame.Navigate(typeof(LoginPage));
                return;
            }

            // Populate fields
            UsernameText.Text = _currentUser.username;
            EmailTextBox.Text = _currentUser.Emailadress;
            RoleText.Text = _currentUser.Role == User.ROLE_OWNER ? "Owner" : "User";
        }

        private async void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            await SaveProfileChanges();
        }

        private async Task SaveProfileChanges()
        {
            // Hide any previous messages
            MessageBorder.Visibility = Visibility.Collapsed;

            // Validate email
            string newEmail = EmailTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newEmail))
            {
                ShowMessage("Email address cannot be empty", false);
                return;
            }

            // Validate email format
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(newEmail))
            {
                ShowMessage("Please enter a valid email address", false);
                return;
            }

            // Check if password change is requested
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            bool passwordChangeRequested = !string.IsNullOrEmpty(currentPassword) || 
                                          !string.IsNullOrEmpty(newPassword) || 
                                          !string.IsNullOrEmpty(confirmPassword);

            if (passwordChangeRequested)
            {
                // Validate password change
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ShowMessage("Please enter your current password", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ShowMessage("Please enter a new password", false);
                    return;
                }

                if (newPassword.Length < 6)
                {
                    ShowMessage("New password must be at least 6 characters long", false);
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    ShowMessage("New passwords do not match", false);
                    return;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, _currentUser.password_hash))
                {
                    ShowMessage("Current password is incorrect", false);
                    return;
                }
            }

            // Show loading
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;

            await Task.Delay(300); // Brief delay for UX

            try
            {
                using (var db = new AppDbContext())
                {
                    // Get fresh user from database
                    var userToUpdate = db.Users.FirstOrDefault(u => u.Id == _currentUser.Id);
                    
                    if (userToUpdate == null)
                    {
                        ShowMessage("User not found in database", false);
                        return;
                    }

                    // Check if email is already in use by another user
                    if (newEmail.ToLower() != userToUpdate.Emailadress.ToLower())
                    {
                        var emailExists = db.Users.Any(u => 
                            u.Emailadress.ToLower() == newEmail.ToLower() && 
                            u.Id != _currentUser.Id);
                        
                        if (emailExists)
                        {
                            ShowMessage("Email address is already in use", false);
                            return;
                        }
                    }

                    // Update email
                    userToUpdate.Emailadress = newEmail;

                    // Update password if requested
                    if (passwordChangeRequested)
                    {
                        userToUpdate.password_hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                        
                        // Clear password fields
                        CurrentPasswordBox.Password = "";
                        NewPasswordBox.Password = "";
                        ConfirmPasswordBox.Password = "";
                    }

                    // Save changes
                    db.SaveChanges();

                    // Update the logged-in user reference
                    _currentUser.Emailadress = newEmail;
                    if (passwordChangeRequested)
                    {
                        _currentUser.password_hash = userToUpdate.password_hash;
                    }

                    ShowMessage("Profile updated successfully!", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error updating profile: {ex.Message}", false);
            }
            finally
            {
                LoadingRing.IsActive = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            MessageText.Text = message;
            MessageBorder.Background = isSuccess 
                ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightGreen)
                : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.LightCoral);
            MessageBorder.Visibility = Visibility.Visible;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(HomePage));
        }
    }
}
