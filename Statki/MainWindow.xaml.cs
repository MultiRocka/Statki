using System.Text;
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
        private List<(Ship ship, int startRow, int startCol)> placedShips = new List<(Ship, int, int)>();

        public MainWindow()
        {
            InitializeComponent();
            CreateLayout();
            CreateShip(); // Tworzymy statek w lewym panelu
        }

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
                    tile.Drop += BoardTile_Drop; // Obsługa zdarzenia Drop
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

        private void CreateShip()
        {
            // Tworzymy testowy statek 2x1
            Ship testShip = new Ship("Test Ship", 3, 2); // Długość 2, szerokość 1
            StackPanel shipPanel = testShip.CreateVisualRepresentation();
            Ship testShip2 = new Ship("Test Ship", 4, 1); // Długość 2, szerokość 1
            StackPanel shipPanel2 = testShip2.CreateVisualRepresentation();

            // Dodanie obsługi przeciągania statku
            shipPanel.MouseMove += ShipPanel_MouseMove;
            shipPanel2.MouseMove += ShipPanel_MouseMove;

            // Znalezienie lewego panelu (pierwsza kolumna głównej siatki)
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
            leftPanel.Children.Add(shipPanel);
            leftPanel.Children.Add(shipPanel2);
        }

        private void ShipPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is StackPanel panel)
            {
                DragDrop.DoDragDrop(panel, panel.Tag, DragDropEffects.Move);

                // Dodajemy efekt przeciągania
                DragDrop.AddGiveFeedbackHandler(this, OnGiveFeedback);
            }
        }

        private void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // Ustawiamy kursor jako półprzezroczysty podczas przeciągania
            Mouse.SetCursor(Cursors.None); // Ukrycie domyślnego kursora
            e.Handled = true;
        }


        public void BoardTile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship && sender is BoardTile tile)
            {
                // Jeśli statek jest już umieszczony, wyczyść poprzednie pola
                if (ship.IsPlaced)
                {
                    ClearPreviousTiles(ship);
                }

                // Ustawienia dla nowej pozycji
                int startRow = Grid.GetRow(tile);
                int startCol = Grid.GetColumn(tile);
                int endRow = ship.IsHorizontal ? startRow : startRow + ship.Width - 1;
                int endCol = ship.IsHorizontal ? startCol + ship.Length - 1 : startCol;

                if (endRow > 10 || endCol > 10)
                {
                    MessageBox.Show("Statek nie zmieści się na planszy!");
                    return;
                }

                // Aktualizujemy pola jako zajęte i dodajemy je do listy statku
                ship.OccupiedTiles.Clear();
                for (int i = 0; i < ship.Length; i++)
                {
                    for (int j = 0; j < ship.Width; j++)
                    {
                        int row = ship.IsHorizontal ? startRow + j : startRow + i;
                        int col = ship.IsHorizontal ? startCol + i : startCol + j;
                        BoardTile gridTile = GetTileAtPosition(row, col);

                        if (gridTile != null)
                        {
                            gridTile.Background = Brushes.Blue;
                            gridTile.IsOccupied = true;
                            ship.OccupiedTiles.Add(gridTile); // Dodajemy pole do zajętych
                        }
                    }
                }
                ship.IsPlaced = true; // Ustawiamy jako umieszczony
            }
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
