using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Statki.Board;
using Statki.Class;

namespace Statki.Board
{
    public class BoardGridCreator
    {
        private BoardTileDragHandler _dragHandler;

        public Grid CreateBoardGrid(bool isOpponent = false)
        {
            Grid boardGrid = new Grid
            {
                ShowGridLines = false
            };

            // Tworzenie wierszy i kolumn
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

            //BitmapImage waterGif = new BitmapImage(new Uri("C:\\Users\\Krzysiek\\Desktop\\PROJEKTY\\Statki\\Assets\\water.png"));

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
                        AllowDrop = !isOpponent, // Tylko plansza gracza obsługuje upuszczanie

                        Row = row, // Przypisanie wiersza
                        Column = col, // Przypisanie kolumny
                        IsOpponent = isOpponent, // Ustawiamy IsOpponent

                        ParentGrid = boardGrid
                    };

                    //tile.SetBackgroundImage(waterGif);

                    // Zdarzenia przeciągania
                    tile.Drop += _dragHandler.BoardTile_Drop;
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
    }
}
