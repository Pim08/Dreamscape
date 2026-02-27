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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Dreamscape.Pages
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptLogin();
        }
        private async void RegistratieButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegistratiePage));
        }
        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await AttemptLogin();
            }
        }

        private async Task AttemptLogin()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;

            string enteredUsername = UsernameTextBox.Text.Trim();
            string enteredPassword = PasswordBox.Password;

            // 🔹 Validatie
            if (string.IsNullOrWhiteSpace(enteredUsername))
            {
                ShowError("Username is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(enteredPassword))
            {
                ShowError("Password is required");
                return;
            }

            // 🔹 Loading UI
            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsActive = true;
            LoginButton.IsEnabled = false;

            await Task.Delay(300);

            using (var db = new AppDbContext())
            {
                var user = db.Users
                             .FirstOrDefault(u =>
                             u.username.ToLower() == enteredUsername.ToLower());

                if (user == null)
                {
                    ResetUI();
                    ShowError("User does not exist");
                    return;
                }

                // 🔐 Password hash check
                bool passwordCorrect =
                    BCrypt.Net.BCrypt.Verify(enteredPassword, user.password_hash);

                if (!passwordCorrect)
                {
                    ResetUI();
                    ShowError("Incorrect password");
                    PasswordBox.Password = "";
                    return;
                }

                // ✅ Login success
                User.LoggedInUser = user;

                ResetUI();

                // 🔹 Role check
                if (user.Role == User.ROLE_OWNER)
                {
                    Frame.Navigate(typeof(DashboardPage));
                }
                else
                {
                    Frame.Navigate(typeof(HomePage));
                }
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }

        private void ResetUI()
        {
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            LoginButton.IsEnabled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UsernameTextBox.Focus(FocusState.Programmatic);
        }
    }
}