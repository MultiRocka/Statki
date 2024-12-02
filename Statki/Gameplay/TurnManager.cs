using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Statki.Class;
using Statki.Board;
using System.Numerics;
using System.Windows.Controls;

namespace Statki.Gameplay
{
    public class TurnManager
    {
        private Random _random = new Random();
        private Player _player1, _player2;
        private bool _isPlayerTurn = true; // Czy tura należy do gracza (true - gracz, false - przeciwnik)
        private DispatcherTimer _turnTimer;
        private int _turnTimeRemaining = 20; // 20 sekund na turę
        private int _shipPlacementTimeRemaining = 30; // 30 sekund na rozłożenie statków
        private bool _isPlacementPhase = true;
        private bool _isGameOver = false;

        public event Action OnGameOver; // Zdarzenie końca gry

        public TurnManager(Player player1, Player player2)
        {
            _player1 = player1;
            _player2 = player2;

            _turnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _turnTimer.Tick += TurnTimer_Tick;
        }

        public void Start()
        {
            // Rozpoczynamy rozkładanie statków (30 sekund)
            StartPlacementPhase();
        }

        private void StartPlacementPhase()
        {
            _turnTimer.Start();
            _shipPlacementTimeRemaining = 30;
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameOver)
                return;

            if (_isPlacementPhase)
            {
                // Rozkładanie statków - countdown
                if (_shipPlacementTimeRemaining > 0)
                {
                    _shipPlacementTimeRemaining--;
                }
                else
                {
                    // Jeśli czas na rozłożenie statków minął, rozkładamy je losowo
                    AutoPlaceShips();
                    _isPlacementPhase = false;
                    _turnTimer.Interval = TimeSpan.FromSeconds(1); // Zmieniamy interwał na 1 sekundy
                    StartTurnPhase();
                }
            }
            else
            {
                // Tura gry - countdown
                if (_turnTimeRemaining > 0)
                {
                    _turnTimeRemaining--;
                }
                else
                {
                    SwitchTurn();
                }
            }
        }

        private void AutoPlaceShips()
        {
            // Automatyczne losowanie pozycji dla statków, jeśli gracz nie rozłożył ich w czasie
            _player1.PlaceShipsRandomly();
            _player2.PlaceShipsRandomly();
        }

        private void StartTurnPhase()
        {
            // Losowanie, kto zaczyna turę
            _isPlayerTurn = _random.Next(2) == 0; // Losowanie między graczami

            // Rozpoczynamy turę
            _turnTimeRemaining = 20; // Resetujemy czas na turę
            _turnTimer.Start();
        }

        private void SwitchTurn()
        {
            if (_isPlayerTurn)
            {
                // Tura gracza - sprawdzamy, czy trafienie
                // Wywołujemy funkcję odpowiedzialną za wykonanie strzału
                if (ShootAtOpponent(_player1, _player2))
                {
                    // Jeśli trafiono, gracz strzela ponownie
                    _turnTimeRemaining = 20;
                }
                else
                {
                    // Jeśli nie trafił, zmiana tury na przeciwnika
                    _isPlayerTurn = false;
                }
            }
            else
            {
                // Tura przeciwnika
                if (ShootAtOpponent(_player2, _player1))
                {
                    _turnTimeRemaining = 20;
                }
                else
                {
                    _isPlayerTurn = true;
                }
            }

            // Sprawdzamy, czy któryś gracz wygrał
            CheckForWinner();
        }

        private bool ShootAtOpponent(Player shooter, Player target)
        {
            // Znajdź statek w miejscu strzału
            var targetShip = target.GetShipAtPosition(shooter.LastShotRow, shooter.LastShotCol);
            if (targetShip != null)
            {
                // Znajdź zajmowany kafelek w miejscu strzału
                var tile = targetShip.OccupiedTiles.Find(t => t.Row == shooter.LastShotRow && t.Column == shooter.LastShotCol);
                if (tile != null)
                {
                    if (tile.HitStatus == HitStatus.Hit)
                    {
                        // Pole już trafione
                        return false;
                    }

                    // Oznacz pole jako trafione
                    tile.HitStatus = HitStatus.Hit;

                    // Sprawdź, czy statek jest zatopiony
                    if (targetShip.IsSunk())
                    {
                        Console.WriteLine($"Ship {targetShip.Name} is sunk.");
                    }

                    return true; // Trafienie
                }
            }
            return false; // Nietrafienie
        }


        private void CheckForWinner()
        {
            if (_player1.AllShipsSunk() || _player2.AllShipsSunk())
            {
                _isGameOver = true;
                _turnTimer.Stop();
                OnGameOver?.Invoke(); // Zdarzenie końca gry
            }
        }
    }
}
