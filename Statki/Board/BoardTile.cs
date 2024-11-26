using Statki.Class;
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

        public Ship AssignedShip { get; set; }

        public BoardTile()
        {
            this.Background = DefaultBackground;
            this.Click += BoardTile_Click;
            this.MouseEnter += BoardTile_MouseEnter;
            this.MouseLeave += BoardTile_MouseLeave;
        }

        private void UpdateTileAppearance()
        {
            switch (HitStatus)
            {
                case HitStatus.None:
                    this.Background = DefaultBackground;
                    break;
                case HitStatus.Miss:
                    this.Background = Brushes.Gray;
                    break;
                case HitStatus.Hit:
                    this.Background = Brushes.Red;
                    break;
            }
        }

        private void BoardTile_Click(object sender, RoutedEventArgs e)
        {
            if (HitStatus != HitStatus.None)
            {
                Console.WriteLine($"Tile {this.Name} already clicked!");
                return; // Jeśli pole było już kliknięte, kończymy
            }

            if (IsOccupied) // Trafiony statek
            {
                HitStatus = HitStatus.Hit;
                UpdateTileAppearance();
                Console.WriteLine($"Hit on {this.Name}");

                if (AssignedShip != null)
                {
                    AssignedShip.CheckIfSunk();
                    if (AssignedShip.IsSunk)
                    {
                        Console.WriteLine($"Ship {AssignedShip.Name} is sunk!");
                    }
                }
            }
            else // Nietrafiony
            {
                HitStatus = HitStatus.Miss;
                UpdateTileAppearance();
                Console.WriteLine($"Miss on {this.Name}");
            }
        }

        private void BoardTile_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (HitStatus == HitStatus.None)
            {
                PreviousBackground = this.Background; // Zachowaj aktualny kolor
                this.Background = Brushes.Transparent;   // Kolor najechania myszką
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
            if (IsOccupied == true)
                this.Background = Brushes.Blue;
        }
    }
}
