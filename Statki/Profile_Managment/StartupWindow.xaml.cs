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

namespace Statki.Profile_Managment
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        private readonly DatabaseManager _databaseManager;
        private string _loggedInUser;

        public StartupWindow()
        {
            InitializeComponent();
            _databaseManager = new DatabaseManager();
            CheckLoginStatus();
        }

        private void CheckLoginStatus()
        {
            // Sprawdź, czy ktoś jest zalogowany (np. przechowując login w pamięci)
            _loggedInUser = SessionManager.GetLoggedInUser(); // Załóżmy, że jest metoda do zarządzania sesją

            if (string.IsNullOrEmpty(_loggedInUser))
            {
                WelcomeText.Text = "You are not logged in.";
                ActionButton.Content = "Log In";
            }
            else
            {
                WelcomeText.Text = $"Welcome {_loggedInUser} in Statki Game!";
                ActionButton.Content = "Start Game";
                EditProfileButton.Visibility = Visibility.Visible;
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
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
                this.Close();
            }
        }

        private void EditProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Przejdź do okna edycji profilu
            var profileWindow = new EditProfileWindow(_loggedInUser);
            profileWindow.Show();
            this.Close();
        }
    }
}
