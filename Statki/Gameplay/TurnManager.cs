﻿using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Statki.Class;
using Statki.Board;
using System.Numerics;
using System.Windows.Controls;
using System.Windows;

namespace Statki.Gameplay
{
    public class TurnManager
    {
        private Random _random = new Random();
        public Player Player1 { get; private set; }
        public Player Player2 { get; private set; }

        private bool _isPlayerTurn = true; // Czy tura należy do gracza (true - gracz, false - przeciwnik)
        private DispatcherTimer _turnTimer;
        private int _turnTimeRemaining = 20; // 20 sekund na turę
        private int _shipPlacementTimeRemaining = 30; // 30 sekund na rozłożenie statków
        private bool _isPlacementPhase = true;
        private bool _isGameOver = false;

        public event Action<int> OnTimerUpdate;
        public event Action OnGameOver; // Zdarzenie końca gry

        private Button readyButton;
        public TurnManager(Player player1, Player player2, Button readyButton)
        {
            Player1 = player1;
            Player2 = player2;
            this.readyButton = readyButton;

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

        public void StartPlacementPhase()
        {
            _turnTimer.Start();
            _shipPlacementTimeRemaining = 30; // Czas układania statków to 30 sekund
            readyButton.Visibility = Visibility.Visible; // Pokazujemy przycisk "Ready" na początku fazy układania
            Console.WriteLine("Placement Phase");
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameOver)
                return;

            if (_isPlacementPhase)
            {
                if (_shipPlacementTimeRemaining > 0)
                {
                    _shipPlacementTimeRemaining--;
                    OnTimerUpdate?.Invoke(_shipPlacementTimeRemaining);
                }
                else
                {
                    AutoPlaceShips();
                    _isPlacementPhase = false;
                    StartTurnPhase(); // Po rozłożeniu statków przechodzimy do fazy tur
                }
            }
            else
            {
                if (_turnTimeRemaining > 0)
                {
                    _turnTimeRemaining--;
                    OnTimerUpdate?.Invoke(_turnTimeRemaining);
                }
                else
                {
                    SwitchTurn();
                }
            }
        }


        public void AutoPlaceShips()
        {
            // Sprawdzamy, czy wszystkie statki są poprawnie ustawione
            if (!AreAllShipsPlaced())
            {
                MessageBox.Show("Wszystkie statki muszą być ustawione!");
                return;
            }

            // Automatyczne losowanie pozycji dla statków, jeśli gracz nie rozłożył ich w czasie
            Player1.PlaceShipsRandomly();
            Player2.PlaceShipsRandomly();
        }

        private bool AreAllShipsPlaced()
        {
            foreach (var ship in Player1.Ships)
            {
                // Sprawdzamy, czy statek nie jest ustawiony na już zajętym polu
                foreach (var tile in ship.OccupiedTiles)
                {
                    if (tile.IsOccupied)
                    {
                        return false; // Zwracamy false, jeśli jakieś pole jest już zajęte
                    }
                }
            }
            return true; // Zwracamy true, jeśli wszystkie statki są poprawnie ustawione
        }


        public void StartTurnPhase()
        {
            // Rozpoczynamy fazę tur
            _isPlayerTurn = _random.Next(2) == 0; // Losowanie, kto zaczyna turę
            _turnTimeRemaining = 20; // Resetujemy czas na turę
            _turnTimer.Start();
        }

        private void SwitchTurn()
        {
            // Przełączamy turę
            _isPlayerTurn = !_isPlayerTurn;

            // Sprawdzamy, czy gra się zakończyła
            CheckForWinner();

            // Resetujemy czas tury
            _turnTimeRemaining = 20;

            // Aktualizujemy timer na wątku UI
            OnTimerUpdate?.Invoke(_turnTimeRemaining);
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
                    if (targetShip.CheckIfSunk())
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
            if (Player1.AllShipsSunk() || Player2.AllShipsSunk())
            {
                _isGameOver = true;
                _turnTimer.Stop();
                OnGameOver?.Invoke(); // Zdarzenie końca gry
            }
        }



    }
}