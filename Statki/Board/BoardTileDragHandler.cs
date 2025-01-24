using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Statki.Class;
using Statki.Board;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace Statki.Board
{
    public class BoardTileDragHandler
    {
        private readonly Grid _gameGrid;
        private Ship _heldShip;
        private readonly KeyAndMouseMonitor _keyAndMouseMonitor;
        public BoardTileDragHandler(Grid gameGrid)
        {
            _gameGrid = gameGrid;
            _keyAndMouseMonitor = new KeyAndMouseMonitor();
        }

        public void BoardTile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                _heldShip = ship;
                HighlightTiles(sender as BoardTile, ship, Brushes.LightGreen, temporary: true);
                
                Console.WriteLine("Działa to: " + _heldShip.Name);
            }
        }

        public void BoardTile_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;

            if (_heldShip != null && sender is BoardTile currentTile)
            {
                int startRow = Grid.GetRow(currentTile);
                int startCol = Grid.GetColumn(currentTile);

                // Sprawdź, czy statek można postawić w tej lokalizacji
                bool canPlace = CanPlaceShip(startRow, startCol, _heldShip);

                // Ustaw kolor w zależności od możliwości postawienia statku
                Brush highlightColor = canPlace ? Brushes.LightGreen : Brushes.IndianRed;

                // Wywołaj podświetlenie z odpowiednim kolorem
                HighlightTiles(currentTile, _heldShip, highlightColor, true);
            }
        }


        public void BoardTile_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                HighlightTiles(sender as BoardTile, ship, Brushes.LightBlue, temporary: true);
                ClearAllHighlights();
            }
        }

        public void BoardTile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship && sender is BoardTile tile)
            {
                List<BoardTile> previousTiles = new List<BoardTile>(ship.OccupiedTiles);
                ship.PreviousOccupiedTiles = previousTiles;
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

                // Sprawdzenie, czy statek mieści się na planszy
                if (endRow > 10 || endCol > 10 || !CanPlaceShip(startRow, startCol, ship))
                {
                    MessageBox.Show("Nie możesz umieścić statku tutaj!");
                    ClearAllHighlights(); // Resetowanie wszystkich podświetleń
                    RestorePreviousPosition(ship);
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
                            ship.OccupiedTiles.Add(gridTile);
                        }
                    }
                }
                ship.IsPlaced = true;

                Console.WriteLine("Zajęte pola dla statku: " + ship.Name);
                foreach (var occupiedTile in ship.OccupiedTiles)
                {
                    Console.WriteLine(occupiedTile.Name);
                }
            }
        }

        public void HighlightTiles(BoardTile startTile, Ship ship, Brush highlightColor, bool temporary)
        {
            if (startTile == null) return;

            ClearAllHighlights();

            int startRow = Grid.GetRow(startTile);
            int startCol = Grid.GetColumn(startTile);

            for (int i = 0; i < ship.Length; i++)
            {
                for (int j = 0; j < ship.Width; j++)
                {
                    int row = ship.IsHorizontal ? startRow + j : startRow + i;
                    int col = ship.IsHorizontal ? startCol + i : startCol + j;

                    BoardTile gridTile = GetTileAtPosition(row, col);
                    if (gridTile != null)
                    {
                        if (temporary)
                        {
                            if (!gridTile.IsOccupied)
                            {
                                gridTile.Background = highlightColor;
                            }
                        }
                        else
                        {
                            gridTile.Background = highlightColor;
                            gridTile.IsOccupied = true;
                        }
                    }
                }
            }
        }
        public void ClearAllHighlights()
        {
            foreach (var child in _gameGrid.Children)
            {
                if (child is BoardTile tile && !tile.IsOccupied)
                {
                    tile.ResetBackground(); // Resetowanie tła do wartości domyślnej
                }
            }
        }


        public bool CanPlaceShip(int startRow, int startCol, Ship ship)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                for (int j = 0; j < ship.Width; j++)
                {
                    int row = ship.IsHorizontal ? startRow + j : startRow + i;
                    int col = ship.IsHorizontal ? startCol + i : startCol + j;

                    BoardTile gridTile = GetTileAtPosition(row, col);
                    if (gridTile == null || gridTile.IsOccupied)
                    {
                        return false;
                    }

                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            // Pomijamy sprawdzanie samego statku (obecnego pola)
                            if (x == 0 && y == 0) continue;

                            int checkRow = row + x;
                            int checkCol = col + y;

                            if (checkRow >= 1 && checkRow <= 10 && checkCol >= 1 && checkCol <= 10)
                            {
                                BoardTile adjacentTile = GetTileAtPosition(checkRow, checkCol);
                                if (adjacentTile != null && adjacentTile.IsOccupied)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
            }
            return true;
        }

        public void ClearPreviousTiles(Ship ship)
        {
            foreach (var tile in ship.OccupiedTiles)
            {
                tile.IsOccupied = false;
                tile.ResetBackground();
            }
            ship.OccupiedTiles.Clear(); // Czyścimy listę
        }

        public BoardTile GetTileAtPosition(int row, int col)
        {
            foreach (var child in _gameGrid.Children)
            {
                if (child is BoardTile tile && Grid.GetRow(tile) == row && Grid.GetColumn(tile) == col)
                {
                    return tile;
                }
            }
            return null;
        }

        private void RestorePreviousPosition(Ship ship)
        {
            foreach (var tile in ship.PreviousOccupiedTiles)
            {
                if (tile != null)
                {
                    tile.IsOccupied = true;
                    tile.ResetBackground(); // Przywróć tło poprzednich pól
                }
            }
            ship.OccupiedTiles.Clear(); // Czyścimy listę aktualnych zajętych pól
            ship.OccupiedTiles.AddRange(ship.PreviousOccupiedTiles);
        }
    }
}
