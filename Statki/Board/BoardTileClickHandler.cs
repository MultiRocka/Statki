using Statki.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki.Board
{
    public class BoardTileClickHandler
    {
        public void HandleTileClick(BoardTile tile)
        {
            Console.WriteLine("Tile clicked!");

            // Sprawdzamy, czy to plansza przeciwnika
            //if (!tile.IsOpponent)
            //{
            //    Console.WriteLine("Nie możesz strzelać w swoją własną planszę!");
            //    return;
            //}

            // Sprawdzamy, czy kafelek już został kliknięty
            if (tile.HitStatus != HitStatus.None)
            {
                Console.WriteLine($"Tile {tile.Name} already clicked!");
                return;
            }

            // Sprawdzamy, czy jest to trafiony statek
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
            else // Nietrafiony
            {
                tile.HitStatus = HitStatus.Miss;
                tile.UpdateTileAppearance();
                Console.WriteLine($"Miss on {tile.Name}");
            }
        }

    }
}
