using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Statki.Board
{
    public enum HitStatus
    {
        None,
        Miss,
        Hit
    }

    public class BoardTile : Button
    {
        public bool IsOccupied { get; set; } = false;
        public HitStatus HitStatus { get; private set; } = HitStatus.None;
        public Brush DefaultBackground { get; set; } = Brushes.LightBlue;
        private Brush PreviousBackground { get; set; }

        public BoardTile()
        {
            this.Background = DefaultBackground;
            this.Click += BoardTile_Click;
            this.MouseEnter += BoardTile_MouseEnter;
            this.MouseLeave += BoardTile_MouseLeave;
        }

        private void BoardTile_Click(object sender, RoutedEventArgs e)
        {
            if (HitStatus != HitStatus.None) return; // Jeśli już kliknięte, wychodzimy

            if (IsOccupied)
            {
                HitStatus = HitStatus.Hit;
                this.Background = Brushes.Red; // Trafienie w statek
                Console.WriteLine($"Hit on {this.Name}");
                // Możesz dodać logikę zniszczenia statku
            }
            else
            {
                HitStatus = HitStatus.Miss;
                this.Background = Brushes.Gray; // Nietrafienie
                Console.WriteLine($"Miss on {this.Name}");
            }
        }

        private void BoardTile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (HitStatus == HitStatus.None)
            {
                PreviousBackground = this.Background; // Zachowaj aktualny kolor
                this.Background = Brushes.LightGray;   // Kolor najechania myszką
            }
        }

        private void BoardTile_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (HitStatus == HitStatus.None)
            {
                this.Background = PreviousBackground; // Przywraca poprzedni kolor
            }
        }

        // Metoda resetująca stan pola
        public void Reset()
        {
            IsOccupied = false;
            HitStatus = HitStatus.None;
            this.Background = DefaultBackground;
        }

        // Opcjonalnie: metoda do ustawiania tekstury lub animacji
        public void SetBackgroundImage(ImageSource image)
        {
            this.Background = new ImageBrush(image);
        }

        public void ResetBackground()
        {
            if (HitStatus == HitStatus.None)
                this.Background = DefaultBackground;
        }
    }
}
