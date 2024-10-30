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

        public BoardTileDragHandler(Grid gameGrid)
        {
            _gameGrid = gameGrid;
        }

        public void BoardTile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Ship)) is Ship ship)
            {
                HighlightTiles(sender as BoardTile, ship, Brushes.LightGreen);
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
                HighlightTiles(sender as BoardTile, ship, Brushes.LightBlue);
            }
        }

        private void HighlightTiles(BoardTile startTile, Ship ship, Brush highlightColor)
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
                    if (gridTile != null)
                    {
                        gridTile.Background = highlightColor;
                    }
                }
            }
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
