using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statki.Class; 
using Statki.Board;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Statki.Board
{
    public class BoardTileDragHandler
    {
        private readonly Grid _gameGrid;
        private List<BoardTile> _currentShipTiles = new List<BoardTile>(); // Śledzi bieżące pola zajęte przez statek

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
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                ClearPreviousShipTiles(); // Zwolnij poprzednie pola statku
                HighlightTiles(sender as BoardTile, ship, Brushes.Blue, temporary: false); // Nowa pozycja

                // Aktualizujemy listę bieżących pól statku
                _currentShipTiles = GetOccupiedTiles(sender as BoardTile, ship);
            }
        }

        private void HighlightTiles(BoardTile startTile, Ship ship, Brush highlightColor, bool temporary)
        {
            int startRow = Grid.GetRow(startTile);
            int startCol = Grid.GetColumn(startTile);

            for (int i = 0; i < ship.Length; i++)
            {
                for (int j = 0; j < ship.Width; j++)
                {
                    int row = ship.IsHorizontal ? startRow + j : startRow + i;
                    int col = ship.IsHorizontal ? startCol + i : startCol + j;

                    BoardTile gridTile = GetTileAtPosition(row, col);

                    // Podświetl tylko te kafelki, które nie są już zajęte
                    if (gridTile != null && (!gridTile.IsOccupied || !temporary))
                    {
                        gridTile.Background = highlightColor;
                        if (!temporary) gridTile.IsOccupied = true;
                    }
                }
            }
        }

        private List<BoardTile> GetOccupiedTiles(BoardTile startTile, Ship ship)
        {
            List<BoardTile> occupiedTiles = new List<BoardTile>();
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
                        occupiedTiles.Add(gridTile);
                    }
                }
            }
            return occupiedTiles;
        }

        private void ClearPreviousShipTiles()
        {
            foreach (var tile in _currentShipTiles)
            {
                tile.IsOccupied = false;
                tile.ResetBackground();
            }
            _currentShipTiles.Clear();
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
