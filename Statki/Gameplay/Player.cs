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
            Board = Board;
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
                    placed = true;
                }
            }
        }

        // Pomocnicza funkcja do umieszczania statku na planszy
        private void PlaceShipOnBoard(Grid board, int startRow, int startCol, Ship ship, bool isHorizontal)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                // Znajdź kafelek na podstawie wiersza i kolumny
                foreach (var child in board.Children)
                {
                    if (child is BoardTile tile && tile.Row == row && tile.Column == col)
                    {
                        tile.IsOccupied = true;
                        tile.OccupiedByShip = ship; // Zapisujemy, który statek zajmuje pole
                        break;
                    }
                }
            }
        }

        private bool CanPlaceShip(Grid board, int startRow, int startCol, Ship ship, bool isHorizontal)
        {
            for (int i = 0; i < ship.Length; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                // Sprawdzamy, czy w danym miejscu nie ma statku lub nie wykracza poza planszę
                foreach (var child in board.Children)
                {
                    if (child is BoardTile tile && tile.Row == row && tile.Column == col)
                    {
                        if (tile.IsOccupied) return false;
                    }
                }
            }
            return true;
        }

        public bool AllShipsSunk()
        {
            // Sprawdzanie, czy wszystkie statki gracza zostały zatopione
            foreach (var ship in Ships)
            {
                if (!ship.IsSunk())
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
    }
}
