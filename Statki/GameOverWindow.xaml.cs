using System;
using System.Collections.Generic;
using System.IO;
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

namespace Statki
{
    public partial class GameOverWindow : Window
    {
        public bool ResetGameRequested { get; private set; } = false;

        public GameOverWindow(bool isWin, string message)
        {
            InitializeComponent();

            // Ustaw obrazek i wiadomość w zależności od wyniku
            if (!isWin)
            {
                Uri imageUri = new Uri("pack://application:,,,/Assets/you_win.png");

                // Wczytujemy obrazek
                ResultImage.Source = new BitmapImage(imageUri);
                ResultMessage.Text = message;
            }
            else
            {
                Uri imageUri1 = new Uri("pack://application:,,,/Assets/you_lose.png");

                ResultImage.Source = new BitmapImage(imageUri1);
                ResultMessage.Text = message;
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            ResetGameRequested = true;
            this.Close();
        }

        private void ExitGame_Click(object sender, RoutedEventArgs e)
        {
            ResetGameRequested = false;
            this.Close();
        }
    }

}
