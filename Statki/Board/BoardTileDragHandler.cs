using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Statki.Class;
using Statki.Board;
using System.Windows.Controls;

namespace Statki.Board
{
    public class BoardTileDragHandler
    {
        private readonly Grid _gameGrid;

        public BoardTileDragHandler(Grid gameGrid)
        {
            _gameGrid = gameGrid;
        }

        public void BoardTile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                HighlightTiles(sender as BoardTile, ship, Brushes.LightGreen, temporary: true);
            }
        }

        public void BoardTile_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        public void BoardTile_DragLeave(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                HighlightTiles(sender as BoardTile, ship, Brushes.LightBlue, temporary: true);
            }
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

                // Sprawdzenie, czy wszystkie pola są wolne
                if (!CanPlaceShip(startRow, startCol, ship))
                {
                    MessageBox.Show("Nie możesz umieścić statku tutaj!");
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

        private void HighlightTiles(BoardTile startTile, Ship ship, Brush highlightColor, bool temporary)
        {
            if (startTile == null) return;

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

        private bool CanPlaceShip(int startRow, int startCol, Ship ship)
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
                }
            }
            return true;
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
            foreach (var child in _gameGrid.Children)
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
