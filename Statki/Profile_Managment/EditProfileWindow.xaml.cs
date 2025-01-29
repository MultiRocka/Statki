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
            ClearErrorMessages();

            string email = EmailBox.Text.Trim();
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirmPassword = ConfirmPasswordBox.Password.Trim();

            bool isValid = true;

            if (string.IsNullOrEmpty(email))
            {
                EmailErrorLabel.Text = "Email is required.";
                EmailErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidEmail(email))
            {
                EmailErrorLabel.Text = "Please enter a valid email address.";
                EmailErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(login))
            {
                LoginErrorLabel.Text = "Username cannot be empty.";
                LoginErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidLogin(login))
            {
                LoginErrorLabel.Text = "Username can contain only letters and numbers, up to 15 characters.";
                LoginErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorLabel.Text = "Password is required to update the profile.";
                PasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidPassword(password))
            {
                PasswordErrorLabel.Text = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.";
                PasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (password != confirmPassword)
            {
                ConfirmPasswordErrorLabel.Text = "Passwords do not match.";
                ConfirmPasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return;

            string hashedPassword = string.IsNullOrEmpty(password) ? null : BCrypt.Net.BCrypt.HashPassword(password);
            bool updated = _databaseManager.UpdateUser(_loggedInUser, login, hashedPassword, email);

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

        private void ClearErrorMessages()
        {
            EmailErrorLabel.Visibility = Visibility.Collapsed;
            LoginErrorLabel.Visibility = Visibility.Collapsed;
            PasswordErrorLabel.Visibility = Visibility.Collapsed;
            ConfirmPasswordErrorLabel.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        private bool IsValidLogin(string login) =>
            Regex.IsMatch(login, @"^[a-zA-Z0-9]{1,15}$");

        private bool IsValidPassword(string password) =>
            Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var startupWindow = new StartupWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}
