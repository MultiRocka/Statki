using Statki.Gameplay;
using System;
using System.Windows;

namespace Statki.Board
{
    public class BoardTileClickHandler
    {


        public void HandleTileClick(BoardTile tile)
        {


            Console.WriteLine("Tile clicked! ", tile.Name);

            if (tile == null)
            {
                Console.WriteLine("Tile is null!");
                return;
            }

            if (!tile.IsOpponent)
            {
                Console.WriteLine("Nie możesz strzelać w swoją własną planszę!");
                Console.WriteLine(tile.HitStatus);
                return;
            }

            if (tile.HitStatus != HitStatus.None)
            {
                Console.WriteLine($"Tile {tile.Name} already clicked!");
                return;
            }

            var turnManager = TurnManager.Instance;
            if (turnManager == null || turnManager.Player1 == null || turnManager.Player2 == null)
            {
                Console.WriteLine("TurnManager is not initialized or players are null!");
                return;
            }

            if (turnManager._isPlayerTurn) // Sprawdzamy, czy jest tura gracza
            {
                HandlePlayerShot(tile);  // Gracz wykonuje strzał
                if (tile.HitStatus == HitStatus.Miss)
                {
                    Console.WriteLine("Miss! Switching to opponent's turn.");
                    turnManager.SwitchTurn();

                    int remainingTime = TurnManager.Instance.remainingTime;
                    MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.HandleShot(true, false, remainingTime);
                }
                else
                {
                    Console.WriteLine("Hit! Player continues their turn.");

                    int remainingTime = TurnManager.Instance.remainingTime;
                    MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow.HandleShot(true, true, remainingTime);

                    int points = MainWindow.scoreManager.savedPoints;
                    int multiplier = MainWindow.scoreManager.savedMultiplier;

                    tile.DisplayPointsAnimation(points, multiplier);

                    turnManager.CheckForWinner();
                }
            }
            else
            {
                Console.WriteLine("It is not the player's turn!");
            }

            Console.WriteLine("State of turns in HandleTileClick:");
            turnManager.Stateofturns();
        }

        private void HandlePlayerShot(BoardTile tile)
        {
            if (tile.IsOccupied)
            {
                tile.HitStatus = HitStatus.Hit;
                tile.UpdateTileAppearance();
                Console.WriteLine($"Hit on {tile.Name}");


                if (tile.AssignedShip != null)
                {
                    tile.AssignedShip.CheckIfSunk();
                    if (tile.AssignedShip.IsSunk)
                    {
                        Console.WriteLine($"Ship {tile.AssignedShip.Name} is sunk!");
                    }
                }
            }
            else
            {
                tile.HitStatus = HitStatus.Miss;
                tile.UpdateTileAppearance();
                Console.WriteLine($"Miss on {tile.Name}");

            }
            TurnManager.Instance.Player1AddTurn();
        }
    }

}
