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

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
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
                EmailErrorText.Text = "Pole email jest wymagane.";
                EmailErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidEmail(email))
            {
                EmailErrorText.Text = "Podany email jest nieprawidłowy.";
                EmailErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(login))
            {
                LoginErrorText.Text = "Pole login jest wymagane.";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidLogin(login))
            {
                LoginErrorText.Text = "Login może zawierać maksymalnie 15 znaków i tylko litery oraz cyfry.";
                LoginErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorText.Text = "Pole hasło jest wymagane.";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!IsValidPassword(password))
            {
                PasswordErrorText.Text = "Hasło musi zawierać przynajmniej jedną dużą literę, jedną małą literę i jedną cyfrę.";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                bool success = _databaseManager.RegisterUser(email, login, hashedPassword);

                if (success)
                {
                    // Automatyczne logowanie po rejestracji
                    SessionManager.SetLoggedInUser(login);

                    // Przejście do StartupWindow
                    var startupWindow = new StartupWindow();
                    startupWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Nie udało się zarejestrować użytkownika. Sprawdź, czy login i email są unikalne.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
