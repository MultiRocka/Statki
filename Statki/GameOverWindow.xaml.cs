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

            if (isWin)
            {
                // Emoji wygranej
                ResultEmoji.Text = "🎉 🏆 🎉";  // Zwycięstwo
                ResultEmoji.Foreground = new SolidColorBrush(Colors.Gold);  // Złoty kolor dla wygranej
                ResultMessage.Text = message;
            }
            else
            {
                // Emoji przegranej
                ResultEmoji.Text = "☠️ 💀 ☠️";  // Czaszka dla przegranej
                ResultEmoji.Foreground = new SolidColorBrush(Colors.Red);  // Czerwony kolor dla przegranej
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
