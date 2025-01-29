using Statki.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using Statki.Database.Ranking;

namespace Statki.Profile_Managment
{

    public partial class StartupWindow : Window
    {
        private readonly DatabaseManager _databaseManager;
        private string _loggedInUser;

        private string _sessionToken;
        private const string SessionFilePath = "user_session.txt"; // Ścieżka do pliku z tokenem

        public StartupWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            _databaseManager.InitializeDatabase();

            CheckSessionToken();
        }

        private void CheckSessionToken()
        {
            _sessionToken = SessionManager.GetSessionToken();

            if (!string.IsNullOrEmpty(_sessionToken))
            {
                var user = _databaseManager.GetUserBySessionToken(_sessionToken);
                if (user != null)
                {
                    _loggedInUser = user.Login;
                    WelcomeText.Text = $"Welcome {_loggedInUser} in Statki Game!";
                    ActionButton.Content = "Start Game";
                    EditProfileButton.Visibility = Visibility.Visible;
                    LogoutButton.Visibility = Visibility.Visible;
                    return;
                }
            }

            WelcomeText.Text = "You are not logged in.";
            ActionButton.Content = "Log in";
            _loggedInUser = null;
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_loggedInUser))
            {
                // Przejdź do okna logowania
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            else
            {
                // Przejdź do gry
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();
                this.Hide();
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Przejdź do okna edycji profilu
            var profileWindow = new EditProfileWindow(_loggedInUser);
            profileWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Usuń token z menedżera sesji i z pliku
            SessionManager.ClearSession();

            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }

            // Przejdź do okna logowania
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
        private void RankingButton_Click(object sender, RoutedEventArgs e)
        {
            // Przejdź do okna rankingu
            var rankingWindow = new RankingWindow(); // Założyłem, że masz okno RankingWindow
            rankingWindow.Show();
            this.Close(); // Zamykamy okno startowe, ale możesz to zmienić, jeśli chcesz, by oba okna były otwarte
        }

    }
}
