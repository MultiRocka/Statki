using Statki.Class;
using Statki.Gameplay;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public HitStatus HitStatus { get; set; } = HitStatus.None;
        public Brush DefaultBackground { get; set; } = Brushes.LightBlue;
        private Brush PreviousBackground { get; set; }
        public int Row { get; set; } 
        public int Column { get; set; }
        public Ship AssignedShip { get; set; }
        public bool IsOpponent { get; set; }
        public Grid ParentGrid { get; set; }

        public BoardTile()
        {
            this.Background = DefaultBackground;
            this.Click += BoardTile_Click;
            this.MouseEnter += BoardTile_MouseEnter;
            this.MouseLeave += BoardTile_MouseLeave;
        }

        private void BoardTile_Click(object sender, RoutedEventArgs e)
        {
            var tile = sender as BoardTile;

            if (tile != null)
            {
                var handler = new BoardTileClickHandler();
                handler.HandleTileClick(tile);
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
            if (IsOccupied==true)
                this.Background = Brushes.Blue;
        }

        public void UpdateTileAppearance()
        {
            // Jeśli kafelek jest zajęty przez statek i nie był jeszcze trafiony
            if (IsOccupied && HitStatus == HitStatus.None && IsOpponent ==false)
            {
                //this.Background = new ImageBrush(new BitmapImage(new Uri("C:\\Users\\Krzysiek\\Desktop\\PROJEKTY\\Statki\\Assets\\ship.png")));
                this.Background = Brushes.Blue;
            }
            else
            {
                // Jeśli statek jest zatopiony, ustaw kolor na czarny
                if (AssignedShip != null && AssignedShip.IsSunk)
                {
                    this.Background = Brushes.Black;
                }
                else
                {
                    // Ustaw tło na podstawie statusu trafienia
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
            }
        }

    }
}
