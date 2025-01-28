using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Statki.Board;
using Statki.Class;

namespace Statki.Gameplay
{
    public class Player
    {
        public string Name { get; set; }
        public List<Ship> Ships { get; private set; } = new List<Ship>();
        public int LastShotRow { get; set; }
        public int LastShotCol { get; set; }
        private Random _random;

        private BoardTileDragHandler _dragHandler;
        public Grid Board { get; private set; }
        public List<BoardTile> BoardTiles { get; private set; }
        public TurnManager TurnManager { get; set; }
        private BoardTileClickHandler _clickHandler;


        public Player(string name, Grid board, TurnManager turnManager)
        {
            Name = name;
            Board = board;
            Ships = new List<Ship>();
            _random = new Random();
            BoardTiles = new List<BoardTile>();
            TurnManager = turnManager;

            // Przypisanie clickHandler
            _clickHandler = new BoardTileClickHandler();

            foreach (var child in Board.Children)
            {
                if (child is BoardTile tile)
                {
                    BoardTiles.Add(tile);
                }
            }
        }

        public virtual void MakeShot(BoardTile targetTile)
        {
            if (TurnManager._isPlayerTurn && !TurnManager.HasPlayerShot)
            {
                if (_clickHandler != null)
                {
                    _clickHandler.HandleTileClick(targetTile);
                    TurnManager.PlayerShot();
                }
            }
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

                        // Oblicz końcowe współrzędne
                        int endRow = isHorizontal ? startRow : startRow + ship.Length - 1;
                        int endCol = isHorizontal ? startCol + ship.Length - 1 : startCol;

                        if (endRow <= 10 && endCol <= 10 && CanPlaceShip(Board, startRow, startCol, ship, isHorizontal))
                        {
                            PlaceShipOnBoard(Board, startRow, startCol, ship, isHorizontal);
                            ship.IsPlaced = true;
                            placed = true;
                        }
                        else
                        {
                            // Obracanie w przypadku niepowodzenia
                            isHorizontal = !isHorizontal;
                            endRow = isHorizontal ? startRow : startRow + ship.Length - 1;
                            endCol = isHorizontal ? startCol + ship.Length - 1 : startCol;

                            if (endRow <= 10 && endCol <= 10 && CanPlaceShip(Board, startRow, startCol, ship, isHorizontal))
                            {
                                PlaceShipOnBoard(Board, startRow, startCol, ship, isHorizontal);
                                ship.IsPlaced = true;
                                placed = true;
                            }
                        }
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
            ship.OccupiedTiles.Clear(); // Wyczyszczenie poprzednich zajętych płytek

            for (int i = 0; i < ship.Length; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                BoardTile tile = GetTileAtPosition(row, col, board);
                if (tile != null)
                {
                    tile.IsOccupied = true;
                    tile.AssignedShip = ship;
                    ship.OccupiedTiles.Add(tile); // Dodaj do listy zajętych kafelków
                    tile.UpdateTileAppearance();
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
                    BoardTile gridTile = GetTileAtPosition(row, col, board);
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
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public BoardTile GetTileAtPosition(int row, int col, Grid board)
        {
            foreach (var child in board.Children)
            {
                if (child is BoardTile tile && tile.Row == row && tile.Column == col)
                {
                    return tile;
                }
            }
            return null; 
        }

        public bool AllShipsSunk()
        {
            if (TurnManager == null)
            {
                Console.WriteLine("TurnManager is null in AllShipsSunk.");
                return false;
            }

            if (Ships == null)
            {
                Console.WriteLine($"Player {Name} has no ships assigned.");
                return false;
            }

            return Ships.All(ship => ship != null && ship.IsSunk);
        }

        public Ship GetShipAtPosition(int row, int col)
        {

            foreach (var ship in Ships)
            {
                var tile = ship.OccupiedTiles.Find(t => Grid.GetRow(t) == row && Grid.GetColumn(t) == col);
                if (tile != null)
                {
                    return ship; 
                }
            }
            return null; 
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

        public void ResetShipsState()
        {
            foreach (var ship in Ships)
            {
                ship.ResetState();
            }
        }

    }
}
