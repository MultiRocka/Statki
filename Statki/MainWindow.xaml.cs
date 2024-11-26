using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Statki.Board;
using Statki.Class;

namespace Statki
{
    public partial class MainWindow : Window
    {
        private Grid gameGrid;
        private BoardTileDragHandler _dragHandler;
        private List<Ship> ships = new List<Ship>(); // Lista wszystkich statków
        private KeyAndMouseMonitor shipDragHandler = new KeyAndMouseMonitor();



        public MainWindow()
        {
            InitializeComponent();
            CreateLayout();
            CreateShips(); // Tworzymy statki w lewym panelu
            this.Width = 800;  
            this.Height = 600;

            this.MinWidth = 600;  // Minimalna szerokość okna
            this.MinHeight = 500;

        }

        public List<Ship> AllShipsList => ships;

        private void CreateLayout()
        {
            // Tworzenie głównej siatki
            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Lewa kolumna na statki
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Prawa kolumna na planszę

            // Tworzenie panelu dla statków po lewej stronie
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 200,
                Background = Brushes.LightGray,
                Margin = new Thickness(10)
            };
            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);

            // Tworzenie planszy po prawej stronie
            gameGrid = new Grid
            {
                ShowGridLines = false
            };

            _dragHandler = new BoardTileDragHandler(gameGrid);

            for (int i = 0; i < 11; i++)
            {
                gameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                gameGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            // Dodanie etykiet wierszy i kolumn
            for (int i = 1; i <= 10; i++)
            {
                TextBlock colHeader = new TextBlock
                {
                    Text = ((char)(i + 64)).ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(colHeader, 0);
                Grid.SetColumn(colHeader, i);
                gameGrid.Children.Add(colHeader);

                TextBlock rowHeader = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(rowHeader, i);
                Grid.SetColumn(rowHeader, 0);
                gameGrid.Children.Add(rowHeader);
            }

            // Dodanie pól planszy jako BoardTile
            for (int row = 1; row <= 10; row++)
            {
                for (int col = 1; col <= 10; col++)
                {
                    BoardTile tile = new BoardTile
                    {
                        Name = $"{(char)(col + 64)}{row}",
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        AllowDrop = true
                    };
                    tile.Drop += _dragHandler.BoardTile_Drop; // Obsługa zdarzenia Drop
                    Grid.SetRow(tile, row);
                    Grid.SetColumn(tile, col);
                    gameGrid.Children.Add(tile);
                    tile.DragEnter += _dragHandler.BoardTile_DragEnter;
                    tile.DragOver += _dragHandler.BoardTile_DragOver;
                    tile.DragLeave += _dragHandler.BoardTile_DragLeave;
                }
            }

            // Ustawienie planszy w prawej kolumnie
            Grid.SetColumn(gameGrid, 1);
            mainGrid.Children.Add(gameGrid);

            // Ustawienie głównej zawartości okna
            this.Content = mainGrid;
        }

        private void CreateShips()
        {
            // Znalezienie lewego panelu (pierwsza kolumna głównej siatki)
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];

            // Inicjalizator statków
            ShipInitializer initializer = new ShipInitializer(shipDragHandler, ships, leftPanel);

            // Tworzenie statków
            initializer.CreateShip("Test Ship 1", 3, 2);
            initializer.CreateShip("Test Ship 2", 4, 1);
            initializer.CreateShip("Test Ship 3", 2, 1);
            initializer.CreateShip("Test Ship 4", 3, 1);
            initializer.CreateShip("Test Ship 3", 2, 1);
            initializer.CreateShip("Test Ship 3", 2, 1);
            initializer.CreateShip("Test Ship 3", 2, 1);

            PrintAllShips();
        }


        private void PrintAllShips()
        {
            foreach (var ship in ships)
            {
                Console.WriteLine($"Ship Name: {ship.Name}, Length: {ship.Length}, Width: {ship.Width}");
            }
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void ClearPreviousTiles(Ship ship)
        {
            foreach (var tile in ship.OccupiedTiles)
            {
                tile.IsOccupied = false;
                tile.ResetBackground();
            }
            ship.OccupiedTiles.Clear(); // Czyścimy listę
        }

        private BoardTile GetTileAtPosition(int row, int col)
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
    }
}
