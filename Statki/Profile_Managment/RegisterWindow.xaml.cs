using Statki.Database;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Statki.Profile_Managment
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseManager _databaseManager;

        public RegisterWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessages();

            string email = EmailTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            bool isValid = true;

            if (string.IsNullOrEmpty(email))
            {
                EmailErrorText.Text = "Email is required.";
                EmailErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidEmail(email))
            {
                EmailErrorText.Text = "The provided email is invalid.";
                EmailErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(login))
            {
                LoginErrorText.Text = "Username is required.";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidLogin(login))
            {
                LoginErrorText.Text = "Username can contain only letters and numbers," +
                    " up to 15 characters.";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorText.Text = "Password is required.";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidPassword(password))
            {
                PasswordErrorText.Text = "Password must contain at least one uppercase letter," +
                    " one lowercase letter, and one digit.";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            try
            {
                // Hashowanie hasła
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                bool success = _databaseManager.RegisterUser(email, login, hashedPassword);

                if (success)
                {
                    var user = _databaseManager.GetUserByLoginOrEmail(login);
                    if (user == null)
                    {
                        MessageBox.Show("User creation failed. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    SessionManager.SetLoggedInUser(login);

                    string sessionToken = TokenGenerator.GenerateSessionToken(user.Id); 
                    DateTime expiresAt = DateTime.UtcNow.AddHours(2);

                    _databaseManager.SaveSessionToken(user.Id, sessionToken, expiresAt); 

                    SessionManager.SetSessionToken(sessionToken);

                    var startupWindow = new StartupWindow();
                    startupWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Registration failed. Please check if your username and email are unique.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ClearErrorMessages()
        {
            EmailErrorText.Visibility = Visibility.Collapsed;
            LoginErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");

        private bool IsValidLogin(string login) =>
            Regex.IsMatch(login, @"^[a-zA-Z0-9]{1,15}$");

        private bool IsValidPassword(string password) =>
            Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$");

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Return to the startup screen
            var startupWindow = new StartupWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}
