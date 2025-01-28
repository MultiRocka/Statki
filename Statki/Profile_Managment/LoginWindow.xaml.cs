using Statki.Database;
using System;
using System.Windows;
using System.Windows.Input;

namespace Statki.Profile_Managment
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseManager _databaseManager;
        private const string SessionFilePath = "user_session.txt";
        public LoginWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();

            // Subskrybuj zdarzenie KeyDown dla całego okna
            this.KeyDown += LoginWindow_KeyDown;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Sprawdź, czy wciśnięto klawisz Enter
            if (e.Key == Key.Enter)
            {
                // Wywołaj metodę obsługującą kliknięcie przycisku Login
                LoginButton_Click(null, new RoutedEventArgs());
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Resetowanie komunikatów o błędach
            LoginErrorLabel.Visibility = Visibility.Collapsed;
            PasswordErrorLabel.Visibility = Visibility.Collapsed;

            // Pobranie danych z pól
            string loginOrEmail = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();



            // Walidacja danych wejściowych
            if (string.IsNullOrEmpty(loginOrEmail))
            {
                LoginErrorLabel.Content = "Pole login lub email jest wymagane.";
                LoginErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                PasswordErrorLabel.Content = "Pole hasło jest wymagane.";
                PasswordErrorLabel.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                // Sprawdzanie, czy użytkownik istnieje
                var user = _databaseManager.GetUserByLoginOrEmail(loginOrEmail);

                if (user == null)
                {
                    LoginErrorLabel.Content = $"Niepoprawny login/email lub hasło.";
                    LoginErrorLabel.Visibility = Visibility.Visible;
                    return;
                }

                // Sprawdzanie hasła
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

                if (isPasswordValid)
                {
                    // Generowanie unikalnego tokenu sesji
                    string sessionToken = TokenGenerator.GenerateSessionToken(user.Id);
                    DateTime expiresAt = DateTime.UtcNow.AddHours(2);

                    // Sprawdź, czy istnieje token w bazie danych
                    var existingToken = _databaseManager.GetExistingSessionToken(user.Id);

                    if (existingToken != null)
                    {
                        Console.WriteLine($"Token already exists in database for user {user.Id}: {existingToken}");
                        sessionToken = existingToken;
                    }
                    else
                    {
                        // Zapisanie tokenu w bazie danych
                        _databaseManager.SaveSessionToken(user.Id, sessionToken, expiresAt);
                    }

                    // Zapisanie tokenu w pliku na komputerze użytkownika
                    SessionManager.SetSessionToken(sessionToken);

                    // Zapisanie zalogowanego użytkownika w SessionManager
                    SessionManager.SetLoggedInUser(user.Login);

                    // Przejście do StartupWindow
                    var startupWindow = new StartupWindow();
                    startupWindow.Show();
                    this.Close();
                }
                else
                {
                    PasswordErrorLabel.Content = "Nieprawidłowe hasło.";
                    PasswordErrorLabel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void RegisterRedirect_Click(object sender, RoutedEventArgs e)
        {
            // Otwieranie okna rejestracji
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}