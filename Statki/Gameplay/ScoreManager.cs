using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki.Gameplay
{
    public class ScoreManager
    {
        public int PlayerScore { get; private set; }
        public int OpponentScore { get; private set; }

        private int _playerComboCount;
        private int _opponentComboCount;

        public void RegisterShot(bool isPlayer, bool isHit, int remainingTime)
        {
            if (isHit)
            {
                int basePoints = remainingTime * 100;
                int comboMultiplier = isPlayer ? ++_playerComboCount : ++_opponentComboCount; // Zwiększamy combo
                int points = basePoints * comboMultiplier;

                AddPoints(isPlayer, points);
                Console.WriteLine($"REMAINING TIME:{remainingTime}");

                Console.WriteLine($"{(isPlayer ? "Player" : "Opponent")} hit! Combo: {comboMultiplier}, Points: {points}");
            }
            else
            {
                ResetCombo(isPlayer);

                Console.WriteLine($"REMAINING TIME:{remainingTime}");
                Console.WriteLine($"{(isPlayer ? "Player" : "Opponent")} missed. Combo reset.");
            } 
        }

        private void ResetCombo(bool isPlayer)
        {
            if (isPlayer)
                _playerComboCount = 0;
            else
                _opponentComboCount = 0;
        }

        private void AddPoints(bool isPlayer, int points)
        {
            if (isPlayer)
                PlayerScore += points;
            else
                OpponentScore += points;

            Console.WriteLine($"{(isPlayer ? "Player" : "Opponent")} scored {points} points. Total: Player: {PlayerScore}, Opponent: {OpponentScore}");
        }
    }

}
