using Statki.Board;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Statki.Class
{
    public class Ship
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public string Name { get; set; }
        public bool IsHorizontal { get; set; } = true;
        public bool IsPlaced { get; set; } = false;

        // Lista przechowująca zajęte pola przez statek
        public List<BoardTile> OccupiedTiles { get; set; } = new List<BoardTile>();

        public Ship(string name, int length, int width)
        {
            Length = length;
            Width = width;
            Name = name;
        }

        public StackPanel CreateVisualRepresentation()
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = IsHorizontal ? Orientation.Vertical : Orientation.Horizontal,
                Background = Brushes.Gray,
                Margin = new Thickness(10),
                Tag = this
            };

            // Tworzenie reprezentacji statku z uwzględnieniem orientacji i wymiarów
            if (IsHorizontal)
            {
                for (int row = 0; row < Width; row++) // liczba wierszy zależna od szerokości
                {
                    StackPanel rowPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal
                    };

                    for (int col = 0; col < Length; col++) // liczba kolumn zależna od długości
                    {
                        Border segment = new Border
                        {
                            Width = 30,
                            Height = 30,
                            Background = Brushes.DarkGray,
                            Margin = new Thickness(1)
                        };
                        rowPanel.Children.Add(segment);
                    }

                    stackPanel.Children.Add(rowPanel);
                }
            }
            else
            {
                for (int col = 0; col < Length; col++) // liczba kolumn zależna od długości
                {
                    StackPanel colPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };

                    for (int row = 0; row < Width; row++) // liczba wierszy zależna od szerokości
                    {
                        Border segment = new Border
                        {
                            Width = 30,
                            Height = 30,
                            Background = Brushes.DarkGray,
                            Margin = new Thickness(1)
                        };
                        colPanel.Children.Add(segment);
                    }

                    stackPanel.Children.Add(colPanel);
                }
            }

            return stackPanel;
        }


    }
}
