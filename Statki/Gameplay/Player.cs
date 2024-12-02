using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using Statki.Board;
using Statki.Class;

namespace Statki.Gameplay
{
    public class Player
    {
        public string Name { get; set; }
        public List<Ship> Ships { get; private set; }
        public int LastShotRow { get; set; }
        public int LastShotCol { get; set; }
        private Random _random;

        private BoardTileDragHandler _dragHandler;
        public Grid Board { get; private set; }

        public Player(string name, Grid board)
        {
            Name = name;
            Board = board;
            Ships = new List<Ship>();
            _random = new Random();
        }

        public void PlaceShipsRandomly()
        {
            // Sprawdzamy, czy statki zostały już przypisane do gracza
            if (Ships.Count == 0)
            {
                throw new InvalidOperationException("Brak statków do rozmieszczenia");
            }

            foreach (var ship in Ships)
            {
                if (!ship.IsPlaced) // Sprawdzamy, czy statek już został ustawiony
                {
                    bool placed = false;

                    while (!placed)
                    {
                        // Losowanie pozycji startowej
                        int startRow = _random.Next(1, 11); // Zakładając planszę 10x10
                        int startCol = _random.Next(1, 11);

                        // Losowanie orientacji (pozioma/pionowa)
                        bool isHorizontal = _random.Next(2) == 0;

                        // Sprawdzamy, czy statek zmieści się na planszy
                        int endRow = isHorizontal ? startRow : startRow + ship.Width - 1;
                        int endCol = isHorizontal ? startCol + ship.Length - 1 : startCol;

                        // Jeśli statek nie zmieści się, próbujemy ponownie
                        if (endRow > 10 || endCol > 10 || !CanPlaceShip(Board, startRow, startCol, ship, isHorizontal))
                        {
                            isHorizontal = !isHorizontal; // Obracamy statek
                            endRow = isHorizontal ? startRow : startRow + ship.Length - 1;
                            endCol = isHorizontal ? startCol + ship.Length - 1 : startCol;

                            // Jeśli po obrocie nadal nie pasuje, wracamy do początku
                            if (endRow > 10 || endCol > 10 || !CanPlaceShip(Board, startRow, startCol, ship, isHorizontal))
                            {
                                continue;
                            }
                        }

                        // Jeśli statek zmieści się, umieszczamy go na planszy
                        PlaceShipOnBoard(Board, startRow, startCol, ship, isHorizontal);
                        ship.IsPlaced = true; // Oznaczamy, że statek został ustawiony
                        placed = true;
                    }
                }
            }
        

            // Po rozmieszczeniu wszystkich statków, zaktualizuj wygląd wszystkich kafelków
            foreach (var child in Board.Children)
            {
                if (child is BoardTile tile && tile.IsOccupied)
                {
                    tile.UpdateTileAppearance(); // Aktualizujemy wygląd kafelka, jeśli jest zajęty
                }
            }
        }


        // Pomocnicza funkcja do umieszczania statku na planszy
        private void PlaceShipOnBoard(Grid board, int startRow, int startCol, Ship ship, bool isHorizontal)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                for (int j = 0; j < ship.Width; j++)
                {
                    int row = isHorizontal ? startRow : startRow + i;
                    int col = isHorizontal ? startCol + i : startCol + j;

                    // Znajdź kafelek na podstawie wiersza i kolumny
                    BoardTile tile = GetTileAtPosition(row, col, board);
                    if (tile != null)
                    {
                        tile.IsOccupied = true;
                        tile.AssignedShip = ship; // Zapisujemy, który statek zajmuje pole
                        tile.UpdateTileAppearance(); // Zaktualizuj wygląd kafelka (np. ustaw kolor)
                    }
                }
            }
        }


        private bool CanPlaceShip(Grid board, int startRow, int startCol, Ship ship, bool isHorizontal)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                for (int j = 0; j < ship.Width; j++)
                {
                    int row = isHorizontal ? startRow : startRow + i;
                    int col = isHorizontal ? startCol + i : startCol + j;

                    // Sprawdzamy, czy w danym miejscu nie ma statku lub nie wykracza poza planszę
                    BoardTile gridTile = GetTileAtPosition(row, col, board); // Funkcja GetTileAtPosition musi zostać zaimplementowana
                    if (gridTile == null || gridTile.IsOccupied)
                    {
                        return false;
                    }

                    // Sprawdzamy sąsiednie kafelki
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
                                BoardTile adjacentTile = GetTileAtPosition(checkRow, checkCol, board);
                                if (adjacentTile != null && adjacentTile.IsOccupied)
                                {
                                    return false; // Jeżeli którykolwiek sąsiedni kafelek jest zajęty, nie możemy postawić statku
                                }
                            }
                        }
                    }
                }
            }
            return true; // Jeżeli wszystkie sprawdzenia przeszły pomyślnie, możemy postawić statek
        }

        private BoardTile GetTileAtPosition(int row, int col, Grid board)
        {
            foreach (var child in board.Children)
            {
                if (child is BoardTile tile && tile.Row == row && tile.Column == col)
                {
                    return tile;
                }
            }
            return null; // Jeśli nie znaleziono kafelka na danej pozycji
        }



        public bool AllShipsSunk()
        {
            // Sprawdzanie, czy wszystkie statki gracza zostały zatopione
            foreach (var ship in Ships)
            {
                if (!ship.CheckIfSunk())
                    return false;
            }
            return true;
        }

        public Ship GetShipAtPosition(int row, int col)
        {
            // Sprawdzamy, czy w danej pozycji znajduje się statek
            foreach (var ship in Ships)
            {
                var tile = ship.OccupiedTiles.Find(t => Grid.GetRow(t) == row && Grid.GetColumn(t) == col);
                if (tile != null)
                {
                    return ship; // Zwracamy statek, jeśli znaleziono pasujący kafelek
                }
            }
            return null; // Jeśli nie znaleziono statku w tej pozycji
        }

         public bool AllShipsPlaced()
          {
                foreach (var ship in Ships)
                {
                    if (ship.OccupiedTiles == null || ship.OccupiedTiles.Count == 0)
                    {
                        // Jeśli któryś statek nie ma przypisanych pól, oznacza to, że nie jest rozstawiony
                        return false;
                    }
                }
                return true; // Wszystkie statki mają przypisane zajęte pola
          }

    }
}
