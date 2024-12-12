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
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }

        public bool _isPlayerTurn { get; private set; } // Czy tura należy do gracza (true - gracz, false - przeciwnik)
        public bool HasPlayerShot {  get; private set; } = false;
        public bool HasOpponentShot { get; private set; } =false;

        private DispatcherTimer _turnTimer;
        private int _turnTimeRemaining = 20; // 20 sekund na turę
        private int _shipPlacementTimeRemaining = 30; // 30 sekund na rozłożenie statków

        private bool _isPlacementPhase = false;
        private bool _isGameOver = false;

        public int _player1Turns {  get; private set; } = 0;
        public int _player2Turns { get; private set; } = 0;

        public event Action<int> OnTimerUpdate;
        public event Action OnGameOver; // Zdarzenie końca gry

        private Button readyButton;
        private int remainingTime;

        public static TurnManager Instance { get; private set; }

        private TurnManager()
        {
            // Provide valid name, grid, and turnManager instance
            Player1 = new Player("Player 1", new Grid(), this);
            Player2 = new Opponent("Opponent", new Grid());

            InitializeTurnTimer();
        }



        public TurnManager(Player player1, Player player2, Button readyButton)
        {
            Instance.Player1 = player1;
            Instance.Player2 = player2;

            this.readyButton = readyButton;

            InitializeTurnTimer();
        }

        public void Start()
        {
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

        private void InitializeTurnTimer()
        {
            _turnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _turnTimer.Tick += TurnTimer_Tick;

            Console.WriteLine("Turn timer initialized.");
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

            Console.WriteLine("Player1 Ships after AutoPlacement:");
            foreach (var ship in Player1.Ships)
            {
                ship.PrintState();
            }

            Console.WriteLine("Player2 Ships after AutoPlacement:");
            foreach (var ship in Player2.Ships)
            {
                ship.PrintState();
            }

        }

        public void StartTurnPhase()
        {
            // Rozpoczynamy fazę tur
            _isPlayerTurn = _random.Next(2) == 0; // Losowanie, kto zaczyna turę
            Console.WriteLine(_isPlayerTurn ? "Player 1 starts!" : "Player 2 starts!");
            _turnTimeRemaining = 20; // Resetujemy czas na turę
            _turnTimer.Start();

            // Nie wykonuj losowego strzału od razu, tylko czekaj na kliknięcie gracza
            if (!_isPlayerTurn)
            {
                // Jeśli to nie gracz, wykonaj strzał przeciwnika, ale nie zmieniaj tury
                Opponent opponent = Player2 as Opponent;
                if (opponent != null)
                {
                    opponent.MakeRandomShot(Player1.Board); // Losowy strzał w planszę gracza
                    OpponentShot(); // Rejestruj strzał
                }
            }

            Stateofturns();

        }

        public void SwitchTurn()
        {
            // Zmieniamy turę
            Console.WriteLine($"Switching turn: {_isPlayerTurn}");

            if (_isPlayerTurn)
            {
                _isPlayerTurn = false; // Player ends their turn
                HasPlayerShot = false;
                Console.WriteLine("***Player turn***");
                
            }
            else
            {
                Console.WriteLine("""---Oponnent turn----""");
                Opponent opponent = Player2 as Opponent;
                if (opponent != null)
                {
                    opponent.MakeRandomShot(Player1.Board); 
                    OpponentShot(); 
                }

                _isPlayerTurn = true; // Opponent ends their turn, player starts next
                HasOpponentShot = false;
 

            }


            // Resetujemy czas na turę
            _turnTimeRemaining = 80;
            OnTimerUpdate?.Invoke(_turnTimeRemaining);

            // Sprawdzamy, czy ktoś wygrał
            Console.WriteLine($"""

                Checking winner
                """);
            CheckForWinner();

            Console.WriteLine("State of turns end of switchturn");
            Stateofturns();

        }

        public void Stateofturns()
        {
            Console.WriteLine("""
                    Stany Tur:
                    _IsPlayerTurn_ = {0}
                    HasPlayerShot = {1}
                    HasOpponentShot = {2}
                    
                    """, _isPlayerTurn, HasPlayerShot, HasOpponentShot
                    );
        }

        public void PlayerShot()
        {
            HasPlayerShot = true;
            _isPlayerTurn = false;
            HasOpponentShot = false;
            _player1Turns++;
        }

        public void OpponentShot()
        {
            HasOpponentShot = true;
            _isPlayerTurn = true;
            HasPlayerShot = false;
            _player2Turns++;
        }

        private void CheckForWinner()
        {
            Console.WriteLine("Checking winner...");

            if ((Player1 != null && Player1.AllShipsSunk()) || (Player2 != null && Player2.AllShipsSunk()))
            {
                _isGameOver = true;
                _turnTimer.Stop();

                Console.WriteLine("Game Over!");
                Console.WriteLine($"Player 1 turns: {_player1Turns}");
                Console.WriteLine($"Player 2 turns: {_player2Turns}");
                Console.WriteLine($"Total turns: {_player1Turns + _player2Turns}");

                OnGameOver?.Invoke();
            }
        }



        // Metoda do ustawiania czasu na 3 sekundy po kliknięciu przycisku
        public void SetTimerTo3Seconds()
        {
            remainingTime = 1;
            OnTimerUpdate?.Invoke(remainingTime);
        }

        public void CheckPhase()
        {
            Console.WriteLine($"Current phase: {(_isPlacementPhase ? "Placement" : "Turn")}");
        }




        public static void Initialize(Player player1, Player player2, Button readyButton)
        {
            if (Instance == null)
            {
                Instance = new TurnManager
                {
                    Player1 = player1,
                    Player2 = player2,
                    readyButton = readyButton
                };
                Instance.InitializeTurnTimer();
                Console.WriteLine("TurnManager initialized successfully.");
            }
        }

    }
}

