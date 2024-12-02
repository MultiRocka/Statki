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
        private Grid opponentGrid;
        private BoardTileDragHandler _dragHandler;
        private List<Ship> ships = new List<Ship>(); // Lista wszystkich statków
        private KeyAndMouseMonitor shipDragHandler = new KeyAndMouseMonitor();

        public MainWindow()
        {
            InitializeComponent();
            CreateLayout();
            CreateShips(); // Tworzymy statki w lewym panelu
            Width = 1200;
            Height = 600;

            MinHeight = 400;
            MinWidth = 1000;
        }

        private void CreateLayout()
        {
            // Tworzenie głównej siatki
            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Lewa kolumna na statki
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza gracza
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza przeciwnika

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

            // Tworzenie planszy gracza
            gameGrid = CreatePlayerBoard();
            Grid.SetColumn(gameGrid, 1);
            mainGrid.Children.Add(gameGrid);

            // Tworzenie planszy przeciwnika
            opponentGrid = CreateOpponentBoard();
            Grid.SetColumn(opponentGrid, 2);
            opponentGrid.Margin = new Thickness(10, 0, 0, 0); // Dodanie odstępu od planszy gracza
            mainGrid.Children.Add(opponentGrid);

            // Ustawienie głównej zawartości okna
            this.Content = mainGrid;
        }

        private Grid CreatePlayerBoard()
        {
            return CreateBoardGrid(isOpponent: false); // Plansza gracza obsługuje przeciąganie statków
        }

        private Grid CreateOpponentBoard()
        {
            return CreateBoardGrid(isOpponent: true); // Plansza przeciwnika nie obsługuje przeciągania statków
        }

        private Grid CreateBoardGrid(bool isOpponent = false)
        {
            Grid boardGrid = new Grid
            {
                ShowGridLines = false
            };

            for (int i = 0; i < 11; i++)
            {
                boardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                boardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
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
                boardGrid.Children.Add(colHeader);

                TextBlock rowHeader = new TextBlock
                {
                    Text = i.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(rowHeader, i);
                Grid.SetColumn(rowHeader, 0);
                boardGrid.Children.Add(rowHeader);
            }

            _dragHandler = new BoardTileDragHandler(boardGrid);

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
                        AllowDrop = !isOpponent // Tylko plansza gracza obsługuje upuszczanie
                    };

                     tile.Drop += _dragHandler.BoardTile_Drop; // Obsługa upuszczania
                     tile.DragEnter += _dragHandler.BoardTile_DragEnter;
                     tile.DragOver += _dragHandler.BoardTile_DragOver;
                     tile.DragLeave += _dragHandler.BoardTile_DragLeave;

                    Grid.SetRow(tile, row);
                    Grid.SetColumn(tile, col);
                    boardGrid.Children.Add(tile);
                }
            }

            return boardGrid;
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


        }
    }

}