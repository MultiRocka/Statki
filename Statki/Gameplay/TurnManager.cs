using System;
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

        public bool _isPlayerTurn = true; // Czy tura należy do gracza (true - gracz, false - przeciwnik)
        private DispatcherTimer _turnTimer;
        private int _turnTimeRemaining = 20; // 20 sekund na turę
        private int _shipPlacementTimeRemaining = 30; // 30 sekund na rozłożenie statków

        private bool _isPlacementPhase = false;
        private bool _isGameOver = false;

        private int _player1Turns = 0;
        private int _player2Turns = 0;

        public event Action<int> OnTimerUpdate;
        public event Action OnGameOver; // Zdarzenie końca gry

        private Button readyButton;

        // Zmienna do trzymania pozostałego czasu
        private int remainingTime;

        public bool _hasShotThisTurn = false;
        public static TurnManager Instance { get; private set; }

        public TurnManager(Player player1, Player player2, Button readyButton)
        {
            Instance = this;
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
            _shipPlacementTimeRemaining = 30;
            remainingTime = _shipPlacementTimeRemaining;
            OnTimerUpdate?.Invoke(remainingTime);
            Console.WriteLine("Placement Phase Started");
            _isPlacementPhase = true;  // Upewnij się, że faza rozmieszczania jest ustawiona na true
        }


        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameOver)
                return;

            if (_isPlacementPhase)
            {
                if (remainingTime > 0)
                {
                    remainingTime--;
                    OnTimerUpdate?.Invoke(remainingTime);
                }

                if (_isPlacementPhase && remainingTime <= 0)
                {
                    AutoPlaceShips();  // Automatyczne rozmieszczenie statków
                    _isPlacementPhase = false;
                    Console.WriteLine("Placement Phase ended.");
                    StartTurnPhase();  // Przechodzimy do fazy tur
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
                    SwitchTurn();  // Zmieniamy turę po upływie czasu
                }
            }
        }


        public void AutoPlaceShips()
        {
            Player1.PlaceShipsRandomly();
            Player2.PlaceShipsRandomly();

        }

        public void StartTurnPhase()
        {
            // Rozpoczynamy fazę tur
            _isPlayerTurn = _random.Next(2) == 0; // Losowanie, kto zaczyna turę
            Console.WriteLine(_isPlayerTurn ? "Player 1 starts!" : "Player 2 starts!");
            _turnTimeRemaining = 20; // Resetujemy czas na turę
            _turnTimer.Start();
        }


        public void SwitchTurn()
        {
            // Zmieniamy turę
            Console.WriteLine($"Switching turn: {_isPlayerTurn}");

            // Zmieniamy, kto ma teraz tura
            if (_isPlayerTurn)
            {
                _player1Turns++;
            }
            else
            {
                _player2Turns++;
            }

            // Resetujemy flagę strzału
            _hasShotThisTurn = false;

            // Zmieniamy tura
            _isPlayerTurn = !_isPlayerTurn;
            Console.WriteLine($"It's now {_isPlayerTurn} turn.");

            // Sprawdzamy zwycięzcę
            CheckForWinner();
            _turnTimeRemaining = 20;
            OnTimerUpdate?.Invoke(_turnTimeRemaining);
        }

        

        private void CheckForWinner()
        {
            if (Player1.AllShipsSunk() || Player2.AllShipsSunk())
            {
                _isGameOver = true;
                _turnTimer.Stop();
                OnGameOver?.Invoke();

                Console.WriteLine("Game Over!");
                Console.WriteLine($"Player 1 turns: {_player1Turns}");
                Console.WriteLine($"Player 2 turns: {_player2Turns}");
                Console.WriteLine($"Total turns: {_player1Turns + _player2Turns}");

            }
        }

        // Metoda do ustawiania czasu na 3 sekundy po kliknięciu przycisku
        public void SetTimerTo3Seconds()
        {
            remainingTime = 3;
            OnTimerUpdate?.Invoke(remainingTime);
        }

        public void CheckPhase()
        {
            Console.WriteLine($"Current phase: {(_isPlacementPhase ? "Placement" : "Turn")}");
        }

        public void EndTurn()
        {
            _hasShotThisTurn = false; // Resetujemy flagę
            SwitchTurn(); // Zmieniamy turę
        }
    }
}

