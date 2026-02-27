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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
namespace Dreamscape.Pages
{
    public sealed partial class RegistratiePage : Page
    {
        public RegistratiePage()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            await RegisterUser();
        }

        private async Task RegisterUser()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;

            string username = UsernameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // 🔹 Validatie
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Username is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError("Email is required");
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ShowError("Invalid email format");
                return;
            }

            if (password.Length < 6)
            {
                ShowError("Password must be at least 6 characters");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords do not match");
                return;
            }

            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsActive = true;

            using (var db = new AppDbContext())
            {
                bool usernameExists = db.Users
                    .Any(u => u.username.ToLower() == username.ToLower());

                if (usernameExists)
                {
                    ResetUI();
                    ShowError("Username already exists");
                    return;
                }

                bool emailExists = db.Users
                    .Any(u => u.Emailadress.ToLower() == email.ToLower());

                if (emailExists)
                {
                    ResetUI();
                    ShowError("Email already registered");
                    return;
                }

                // 🔐 Hash password
                string hashedPassword =
                    BCrypt.Net.BCrypt.HashPassword(password);

                User newUser = new User
                {
                    username = username,
                    Emailadress = email,
                    password_hash = hashedPassword,
                    Role = User.ROLE_USER
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                ResetUI();

                // Auto login na registratie
                User.LoggedInUser = newUser;

                Frame.Navigate(typeof(LoginPage));
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
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}