using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Statki.Board;
using Statki.Class;

namespace Statki.Gameplay
{
    public class Opponent : Player
    {
        private List<(int Row, int Col)> AvailableShots;

        public Opponent(string name, Grid board) : base(name, board) // Zgodnie z definicją konstruktora Player
        {
            InitializeAvailableShots();
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
        public (int Row, int Col) MakeRandomShot(Grid opponentBoard)
        {
            if (AvailableShots.Count == 0)
            {
                throw new InvalidOperationException("Brak dostępnych strzałów.");
            }

            // Wybierz losowy strzał z listy dostępnych
            Random random = new Random();
            int index = random.Next(AvailableShots.Count);
            var (row, col) = AvailableShots[index];

            // Usuń strzał z listy, aby nie powtarzać
            AvailableShots.RemoveAt(index);

            // Zaznacz wynik strzału na planszy przeciwnika
            foreach (var child in opponentBoard.Children)
            {
                if (child is BoardTile tile && tile.Row == row && tile.Column == col)
                {
                    // Jeśli pole jest zajęte przez statek
                    if (tile.AssignedShip != null)
                    {
                        tile.HitStatus = HitStatus.Hit; // Jeśli trafiono
                        tile.Background = Brushes.Red; // Zmiana koloru na czerwony (trafienie)

                        // Sprawdzenie, czy statek jest zatopiony
                        if (tile.AssignedShip.CheckIfSunk())
                        {
                            Console.WriteLine($"Statek przeciwnika {tile.AssignedShip.Name} został zatopiony!");
                        }
                    }
                    else
                    {
                        tile.HitStatus = HitStatus.Miss; // Jeśli pole jest puste, status to "miss"
                        tile.Background = Brushes.Gray; // Zmiana koloru na szary (nietrafiony)
                        Console.WriteLine($"Miss on {tile.Name}");
                    }
                    break;
                }
            }

            return (row, col);
        }


    }
}