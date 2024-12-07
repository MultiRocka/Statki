﻿using Statki.Gameplay;
using System;

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
            if (turnManager == null || turnManager.Player1 == null || turnManager.Oponnent == null)
            {
                Console.WriteLine("TurnManager is not initialized or players are null!");
                return;
            }

            if (turnManager._isPlayerTurn)
            {
                if (!turnManager.HasPlayerShot)
                {
                    // Gracz wykonuje strzał
                    HandlePlayerShot(tile);
                    turnManager.PlayerShot();  // Gracz wykonał strzał
                    turnManager.SwitchTurn();  // Zmiana tury na przeciwnika
                }
                else
                {
                    Console.WriteLine("Player has already shot this turn!");
                }
            }
            else
            {
                Console.WriteLine("It is not the player's turn!");
            }

            Console.WriteLine("State of turns in HandleTileClick");
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
        }
    }
}
