using Statki.Database;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace Statki.Profile_Managment
{
    public partial class EditProfileWindow : Window
    {
        private readonly string _loggedInUser;
        private readonly DatabaseManager _databaseManager;

        public EditProfileWindow(string loggedInUser)
        {
            InitializeComponent();
            _loggedInUser = loggedInUser;
            _databaseManager = new DatabaseManager();
            LoadUserData();
        }

        private void LoadUserData()
        {
            var user = _databaseManager.GetUserByLoginOrEmail(_loggedInUser);
            if (user != null)
            {
                LoginBox.Text = user.Login;
                EmailBox.Text = user.Email; 
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Resetting error visibility
            LoginErrorLabel.Visibility = Visibility.Collapsed;
            EmailErrorLabel.Visibility = Visibility.Collapsed;
            PasswordErrorLabel.Visibility = Visibility.Collapsed;
            ConfirmPasswordErrorLabel.Visibility = Visibility.Collapsed;

            bool isValid = true;

            // Login validation
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                LoginErrorLabel.Text = "Username cannot be empty.";
                LoginErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(EmailBox.Text) || !EmailBox.Text.Contains("@"))
            {
                EmailErrorLabel.Text = "Please enter a valid email address.";
                EmailErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(PasswordBox.Password) || PasswordBox.Password.Length < 6)
            {
                PasswordErrorLabel.Text = "Password must be at least 6 characters long.";
                PasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Confirm password validation
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ConfirmPasswordErrorLabel.Text = "Passwords do not match.";
                ConfirmPasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return;

            // Save changes
            bool updated = _databaseManager.UpdateUser(_loggedInUser, LoginBox.Text, PasswordBox.Password, EmailBox.Text);
            if (updated)
            {
                MessageBox.Show("Profile has been updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Failed to update the profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Return to the startup screen
            var startupWindow = new StartupWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}
