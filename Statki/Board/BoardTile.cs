using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Statki.Board
{
    public class BoardTile : Button
    {
        public bool IsOccupied { get; set; } = false; 
        public bool IsHit { get; private set; } = false;
        public Brush DefaultBackground { get; set; } = Brushes.LightBlue;
        public BoardTile()
        {
            this.Background = DefaultBackground;
            this.Click += BoardTile_Click;
        }

        private void BoardTile_Click(object sender, RoutedEventArgs e)
        {
            if (IsHit) return; 

            IsHit = true;
            if (IsOccupied)
            {
                this.Background = Brushes.Red; 
                // Możesz dodać logikę zniszczenia statku
            }
            else
            {
                this.Background = Brushes.Gray; 
            }
        }

        // Metoda resetująca stan pola
        public void Reset()
        {
            IsOccupied = false;
            IsHit = false;
            this.Background = Brushes.LightBlue;
        }

        // Opcjonalnie: metoda do ustawiania tekstury lub animacji
        public void SetBackgroundImage(ImageSource image)
        {
            this.Background = new ImageBrush(image);
        }

        public void ResetBackground()
        {
            if (!IsOccupied && !IsHit)
                this.Background = DefaultBackground;
        }
    }
}
