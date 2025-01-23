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

        public bool IsSunk { get; private set; } = false;

        public List<BoardTile> OccupiedTiles { get; set; } = new List<BoardTile>();
        public List<BoardTile> PreviousOccupiedTiles { get; set; } = new List<BoardTile>();

        public BoardTile BoardTile;


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
                            Margin = new Thickness(3)
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
        public void UpdateOccupiedTiles(int startRow, int startCol, Grid gameGrid)
        {
            OccupiedTiles.Clear();
            Console.WriteLine($"Updating tiles for {Name} starting at ({startRow}, {startCol})");

            for (int i = 0; i < Length; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    int row = IsHorizontal ? startRow : startRow + j;
                    int col = IsHorizontal ? startCol + i : startCol;

                    if (row < 0 || col < 0 || row >= gameGrid.RowDefinitions.Count || col >= gameGrid.ColumnDefinitions.Count)
                    {
                        Console.WriteLine($"Tile ({row}, {col}) is out of bounds for {Name}.");
                        continue;
                    }

                    BoardTile gridTile = GetTileAtPosition(row, col, gameGrid);
                    if (gridTile != null)
                    {
                        OccupiedTiles.Add(gridTile);
                        Console.WriteLine($"Added tile {gridTile.Name} to {Name}");
                    }
                    else
                    {
                        Console.WriteLine($"Tile ({row}, {col}) not found in game grid.");
                    }
                }
            }
        }

        private BoardTile GetTileAtPosition(int row, int col, Grid gameGrid)
        {
            foreach (var child in gameGrid.Children)
            {
                if (child is BoardTile tile && Grid.GetRow(tile) == row && Grid.GetColumn(tile) == col)
                {
                    return tile;
                }
            }
            return null;
        }

        public void Rotate()
        {
            int temp = Length;
            Length = Width;
            Width = temp;
        }

        public bool CheckIfSunk()
        {
            if (IsSunk)
            {
                Console.WriteLine($"{Name} is already sunk!");
                return true;
            }

            foreach (var tile in OccupiedTiles)
            {
                Console.WriteLine($"Checking tile {tile.Name}: HitStatus = {tile.HitStatus}");
                if (tile.HitStatus != HitStatus.Hit)
                {
                    Console.WriteLine($"{Name} is not sunk. Tile {tile.Name} is not hit.");
                    return false;
                }
            }

            IsSunk = true;
            Console.WriteLine($"{Name} is now sunk!");

            foreach (var tile in OccupiedTiles)
            {
                tile.UpdateTileAppearance();
            }

            return true;
        }


        public void PrintState()
        {
            Console.WriteLine($"\n" +
                $"Ship: {Name}");

            Console.WriteLine($" - Length: {Length}");
            Console.WriteLine($" - Width: {Width}");
            Console.WriteLine($" - IsHorizontal: {IsHorizontal}");
            Console.WriteLine($" - IsPlaced: {IsPlaced}");
            Console.WriteLine($" - IsSunk: {IsSunk}");
            Console.WriteLine($" - OccupiedTiles: {OccupiedTiles.Count}");
            foreach (var tile in OccupiedTiles)
            {
                Console.WriteLine($"   - Tile: {tile.Name}, HitStatus: {tile.HitStatus}");
            }
        }

        public void ResetState()
        {
            IsPlaced = false;
            IsSunk = false;
            OccupiedTiles.Clear();
            PreviousOccupiedTiles.Clear();
            Console.WriteLine($"{Name} state has been reset.");
        }

    }
}
