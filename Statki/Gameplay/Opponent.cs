using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Statki.Board;
using Statki.Class;
using System.Windows;

namespace Statki.Gameplay
{
    public class Opponent : Player
    {
        private List<(int Row, int Col)> AvailableShots;
        private BoardTileClickHandler _clickHandler;


        public Opponent(string name, Grid board) : base(name, board, TurnManager.Instance) // Pass TurnManager to the base class constructor
        {
            InitializeAvailableShots();
        }

        public override void MakeShot(BoardTile targetTile)
        {
            if (targetTile == null)
            {
                Console.WriteLine("Target tile is null!");
                return;
            }

            if (targetTile.HitStatus != HitStatus.None)
            {
                Console.WriteLine($"Tile {targetTile.Name} already hit!");
                return;
            }

            if (targetTile.IsOccupied)
            {
                targetTile.HitStatus = HitStatus.Hit;
                Console.WriteLine($"Hit on {targetTile.Name}");
                targetTile.UpdateTileAppearance();

                int remainingTime = TurnManager.Instance.remainingTime;
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.HandleShot(false, true, remainingTime);

                int points = MainWindow.scoreManager.savedPoints;
                int multiplier = MainWindow.scoreManager.savedMultiplier;

                targetTile.DisplayPointsAnimation(points, multiplier);

                TurnManager.Instance.CheckForWinner();

                if (targetTile.AssignedShip != null)
                {
                    targetTile.AssignedShip.CheckIfSunk();
                    if (targetTile.AssignedShip.IsSunk)
                    {
                        Console.WriteLine($"Ship {targetTile.AssignedShip.Name} is sunk!");
                    }
                }
            }
            else
            {
                targetTile.HitStatus = HitStatus.Miss;
                Console.WriteLine($"Miss on {targetTile.Name}");

                int remainingTime = TurnManager.Instance.remainingTime;
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.HandleShot(false, false, remainingTime);

                targetTile.UpdateTileAppearance();
            }
        }

        // Inicjalizacja dostępnych strzałów
        private void InitializeAvailableShots()
        {
            AvailableShots = new List<(int Row, int Col)>();
            for (int row = 1; row <= 10; row++)
            {
                for (int col = 1; col <= 10; col++)
                {
                    AvailableShots.Add((row, col));
                }
            }
        }

        // Losowy strzał w planszę przeciwnika
        public void MakeRandomShot(Grid opponentBoard)
        {
            if (AvailableShots.Count == 0)
            {
                Console.WriteLine("No available shots left!");
                return;
            }

            var random = new Random();
            int index = random.Next(AvailableShots.Count);
            var (row, col) = AvailableShots[index];
            AvailableShots.RemoveAt(index);

            var targetTile = base.GetTileAtPosition(row, col, opponentBoard);
            if (targetTile != null)
            {
                Console.WriteLine($"Opponent shooting at {targetTile.Name}");
                MakeShot(targetTile);

                if (targetTile.HitStatus == HitStatus.Miss)
                {
                    Console.WriteLine("Opponent missed. Switching to player's turn.");
                }
                else
                {
                    TurnManager.Instance.OpponentShot(); // Oznacz strzał przeciwnika
                    TurnManager.Instance.SwitchTurn();   // Zmień turę na gracza
                    Console.WriteLine("Opponent hit! Opponent continues their turn.");
                }
            }
        }

    }
}