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
                EmailBox.Text = user.Email; // Załóżmy, że masz pole "Email" w bazie
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Resetowanie widoczności błędów
            LoginErrorLabel.Visibility = Visibility.Collapsed;
            EmailErrorLabel.Visibility = Visibility.Collapsed;
            PasswordErrorLabel.Visibility = Visibility.Collapsed;
            ConfirmPasswordErrorLabel.Visibility = Visibility.Collapsed;

            bool isValid = true;

            // Walidacja loginu
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                LoginErrorLabel.Text = "Login nie może być pusty.";
                LoginErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Walidacja e-maila
            if (string.IsNullOrWhiteSpace(EmailBox.Text) || !EmailBox.Text.Contains("@"))
            {
                EmailErrorLabel.Text = "Wprowadź poprawny adres e-mail.";
                EmailErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Walidacja hasła
            if (string.IsNullOrWhiteSpace(PasswordBox.Password) || PasswordBox.Password.Length < 6)
            {
                PasswordErrorLabel.Text = "Hasło musi zawierać co najmniej 6 znaków.";
                PasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            // Walidacja potwierdzenia hasła
            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ConfirmPasswordErrorLabel.Text = "Hasła nie są zgodne.";
                ConfirmPasswordErrorLabel.Visibility = Visibility.Visible;
                isValid = false;
            }

            if (!isValid)
                return;

            // Zapis zmian
            bool updated = _databaseManager.UpdateUser(_loggedInUser, LoginBox.Text, PasswordBox.Password, EmailBox.Text);
            if (updated)
            {
                MessageBox.Show("Profil został zaktualizowany.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Nie udało się zaktualizować profilu.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Powrót na ekran startowy
            var startupWindow = new StartupWindow();
            startupWindow.Show();
            this.Close();
        }
    }
}
