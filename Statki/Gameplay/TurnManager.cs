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
        public bool HasPlayerShot { get; set; } = false;
        public bool HasOpponentShot { get; private set; } = false;

        private DispatcherTimer _turnTimer;
        private int _turnTimeRemaining = 20; // 20 sekund na turę
        private int _shipPlacementTimeRemaining = 30; // 30 sekund na rozłożenie statków

        private bool _isPlacementPhase = false;
        private bool _isGameOver = false;

        public int _player1Turns { get; set; } = 0;
        public int _player2Turns { get; set; } = 0;

        public event Action<int> OnTimerUpdate;
        public event Action OnGameOver; // Zdarzenie końca gry

        private Button readyButton;
        public int remainingTime;

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
                    remainingTime = _turnTimeRemaining;
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
            _isPlayerTurn = false;
            Console.WriteLine(_isPlayerTurn ? "Player 1 starts!" : "Player 2 starts!");
            if (_isPlayerTurn) OpponentShot(); else PlayerShot();
            _turnTimeRemaining = 20; // Resetujemy czas na turę
            _turnTimer.Start();
            SwitchTurn();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow)?.HighlightBoard(_isPlayerTurn, false);
            });

            Stateofturns();

        }

        public async void SwitchTurn()
        {
            // Debugging przed zmianą tury
            Console.WriteLine($"Switching turn. Current turn: {_isPlayerTurn}");

            // Zmiana flagi tury
            _isPlayerTurn = !_isPlayerTurn;

            if (_isPlayerTurn) // Tura gracza
            {
                Console.WriteLine("*** Player's Turn ***");
                HasPlayerShot = false;  // Gracz jeszcze nie strzelał
                HasOpponentShot = false;  // Resetujemy stan przeciwnika
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ((MainWindow)Application.Current.MainWindow)?.HighlightBoard(true, false); // Podświetlamy planszę gracza
                });
            }
            else // Tura przeciwnika
            {
                Console.WriteLine("--- Opponent's Turn ---");
                HasPlayerShot = false;  // Resetujemy stan gracza
                HasOpponentShot = false; // Przeciwnik jeszcze nie strzelał
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ((MainWindow)Application.Current.MainWindow)?.HighlightBoard(false, false); // Podświetlamy planszę przeciwnika
                });

                // Przeciwnik wykonuje strzał
                Opponent opponent = Player2 as Opponent;
                if (opponent != null)
                {
                    await Task.Delay(400);
                    opponent.MakeRandomShot(Player1.Board); // Strzał przeciwnika
                    OpponentShot(); // Aktualizacja stanu po strzale przeciwnika
                    SwitchTurn();
                }
            }

            // Debugging po zmianie tury
            Stateofturns();

            // Reset czasu na turę
            _turnTimeRemaining = 20;
            OnTimerUpdate?.Invoke(_turnTimeRemaining);

            // Sprawdzenie, czy ktoś wygrał
            CheckForWinner();
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
            Console.WriteLine("Player shot executed");
            HasPlayerShot = true;
            HasOpponentShot = false;
            _player1Turns++;

            //updating which board is active
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow)?.HighlightBoard(_isPlayerTurn, true);
            });

            Stateofturns();
        }

        public void OpponentShot()
        {
            Console.WriteLine("Opponent shot executed");
            HasOpponentShot = true;  // Oznaczamy, że przeciwnik wykonał strzał
            HasPlayerShot = false;   // Resetujemy flagę strzału gracza

            _player2Turns++;  // Zwiększamy liczbę tur przeciwnika

            // Uaktualniamy aktywność planszy
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow)?.HighlightBoard(_isPlayerTurn, true);
            });

            // Debugging state after shot
            Stateofturns();
        }

        public void CheckForWinner()
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

        public void Player1AddTurn()
        {
            _player1Turns++;
        }

    }
}