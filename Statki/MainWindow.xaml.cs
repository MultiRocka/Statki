using Statki.Board;
using Statki.Class;
using Statki.Database;
using Statki.Database.Ranking;
using Statki.Gameplay;
using Statki.Profile_Managment;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace Statki
{
    public partial class MainWindow : Window
    {
        private Grid gameGrid;
        private Grid opponentGrid;
        private BoardTileDragHandler _dragHandler;
        private List<Ship> ships = new List<Ship>();
        private KeyAndMouseMonitor shipDragHandler = new KeyAndMouseMonitor();
        private BoardGridCreator boardGridCreator;
        private TurnManager turnManager;
        private TextBlock timerTextBlock;
        private Button readyButton;

        public event Action<int> OnTimerUpdate;

        private TextBlock playerBoardHeader;
        private TextBlock opponentBoardHeader;
        private TextBlock turnIndicatorTextBlock;

        private TextBlock playerScoreTextBlock;
        private TextBlock opponentScoreTextBlock;

        public static ScoreManager scoreManager;

        private DatabaseManager _databaseManager = new DatabaseManager();

        private StatisticsManager _statisticsManager;

        private StackPanel timerPanel;

        private RankingWindow rankingWindow = new RankingWindow();

        public MainWindow()
        {
            InitializeComponent();

            _databaseManager.InitializeDatabase();

            _statisticsManager = new StatisticsManager(_databaseManager);

            CreateLayout();
            CreateShips();
            InitializePlayersAndTurnManager();

            MinHeight = 600;
            MinWidth = 1200;

            Height = 600;
            Width = 1400;
        }

        private void CreateLayout()
        {
            double maxShipWidth = ships.Any() ? ships.Max(ship => ship.Width * 30 + 10) : 230;

            // Create main grid
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(maxShipWidth) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Left Panel for Ships
            StackPanel leftPanel = CreateLeftPanel();
            Grid.SetColumn(leftPanel, 0);
            Grid.SetRow(leftPanel, 1);
            mainGrid.Children.Add(leftPanel);

            playerBoardHeader = new TextBlock
            {
                Text = "Player Board",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Green,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(playerBoardHeader, 1);
            Grid.SetRow(playerBoardHeader, 0);
            mainGrid.Children.Add(playerBoardHeader);

            opponentBoardHeader = new TextBlock
            {
                Text = "Opponent Board",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(opponentBoardHeader, 3);
            Grid.SetRow(opponentBoardHeader, 0);
            mainGrid.Children.Add(opponentBoardHeader);
            // Player and Opponent Boards
            gameGrid = CreateBoardGrid(isOpponent: false);
            opponentGrid = CreateBoardGrid(isOpponent: true);

            Grid.SetColumn(gameGrid, 1);
            Grid.SetRow(gameGrid, 1);
            mainGrid.Children.Add(gameGrid);

            Grid.SetColumn(opponentGrid, 3);
            Grid.SetRow(opponentGrid, 1);
            mainGrid.Children.Add(opponentGrid);

            // Timer Panel
            StackPanel timerPanel = CreateTimerPanel();
            Grid.SetColumn(timerPanel, 2);
            Grid.SetRow(timerPanel, 1);
            mainGrid.Children.Add(timerPanel);

            this.Content = mainGrid;
        }

        private StackPanel CreateLeftPanel()
        {
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = Brushes.LightGray,
                Visibility = Visibility.Visible
            };

            // Oblicz dynamiczną szerokość na podstawie największej szerokości statków
            double maxShipWidth = ships.Any() ? ships.Max(ship => ship.Width * 30 + 10) : 230;
            leftPanel.Width = maxShipWidth;

            return leftPanel;
        }

        private Grid CreateBoardGrid(bool isOpponent)
        {
            boardGridCreator = new BoardGridCreator();
            return boardGridCreator.CreateBoardGrid(isOpponent);
        }

        private StackPanel CreateTimerPanel()
        {
            StackPanel timerPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Tekst odliczania czasu
            timerTextBlock = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            timerPanel.Children.Add(timerTextBlock);

            // Tekst punktacji gracza
            playerScoreTextBlock = new TextBlock
            {
                Text = "Player SCORE: 0",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5),
                Foreground = Brushes.Green
            };
            timerPanel.Children.Add(playerScoreTextBlock);

            // Tekst punktacji przeciwnika
            opponentScoreTextBlock = new TextBlock
            {
                Text = "Opponent SCORE: 0",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5),
                Foreground = Brushes.Red
            };
            timerPanel.Children.Add(opponentScoreTextBlock);

            // Tekst wskaźnika tury
            turnIndicatorTextBlock = new TextBlock
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };
            timerPanel.Children.Add(turnIndicatorTextBlock);

            // Przycisk "Ready"
            readyButton = new Button
            {
                Content = "Ready",
                Width = 100,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            readyButton.Click += ReadyButton_Click;
            timerPanel.Children.Add(readyButton);

            return timerPanel;
        }

        private void UpdateScoreDisplay()
        {
            playerScoreTextBlock.Text = $"Player SCORE: {scoreManager.PlayerScore.ToString("N0")}";
            opponentScoreTextBlock.Text = $"Opponent SCORE: {scoreManager.OpponentScore.ToString("N0")}";
        }

        public void HandleShot(bool isPlayer, bool isHit, int remainingTime)
        {
            scoreManager.RegisterShot(isPlayer, isHit, remainingTime);
            UpdateScoreDisplay();
        }

        private void InitializePlayersAndTurnManager()
        {
            // Create Player 1 and Opponent, but do not assign the TurnManager yet
            Player player1 = new Player("Player", gameGrid, null);
            Opponent opponent = new Opponent("Oponent", opponentGrid);

            // Initialize TurnManager singleton with players and readyButton
            TurnManager.Initialize(player1, opponent, readyButton);

            scoreManager = new ScoreManager();
            // Assign the TurnManager to the players
            player1.TurnManager = TurnManager.Instance;
            opponent.TurnManager = TurnManager.Instance;

            // Retrieve the initialized TurnManager instance
            turnManager = TurnManager.Instance;

            // Subscribe to TurnManager events
            if (turnManager != null)
            {
                turnManager.OnGameOver += TurnManager_OnGameOver;
                turnManager.OnTimerUpdate += UpdateTimerText;
            }
            else
            {
                throw new InvalidOperationException("TurnManager is not properly initialized.");
            }

            // Assign ships to players
            AssignShipsToPlayers();

            // Start the game
            turnManager.Start();
        }

        private void CreateShips()
        {
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];

            // Inicjalizacja statków
            ShipInitializer initializer = new ShipInitializer(shipDragHandler, ships, ((StackPanel)((Grid)this.Content).Children[0]));

            initializer.CreateShip("Carrier", 6, 1);
            initializer.CreateShip("Dreadnought", 6, 1);
            initializer.CreateShip("Battleship", 5, 1);
            initializer.CreateShip("Cruiser", 4, 1);
            initializer.CreateShip("Submarine", 3, 1);
            initializer.CreateShip("Destroyer", 3, 1);
            initializer.CreateShip("Patrol Boat", 2, 1);

            int maxWidth = ships.Any() ? ships.Max(ship => ship.Width * 700) : 200; // 30px + marginesy
            leftPanel.Width = +maxWidth;
        }

        private void AssignShipsToPlayers()
        {
            // Przypisujemy statki graczowi 1
            foreach (var ship in ships)
            {
                if (!turnManager.Player1.Ships.Any(s => s.Name == ship.Name))
                {
                    turnManager.Player1.Ships.Add(ship);
                }
            }

            // Tworzymy kopie statków dla przeciwnika
            foreach (var ship in ships)
            {
                // Sprawdzamy, czy statek o tej samej nazwie już istnieje w liście przeciwnika
                if (!turnManager.Player2.Ships.Any(s => s.Name == ship.Name + " (Opponent)"))
                {
                    var opponentShip = new Ship(ship.Name + " (Opponent)", ship.Length, ship.Width);
                    turnManager.Player2.Ships.Add(opponentShip);
                }
            }

            // Wypisanie stanu statków gracza 1
            Console.WriteLine("Statki gracza 1:");
            foreach (var ship in turnManager.Player1.Ships)
            {
                ship.PrintState();
            }

            // Wypisanie stanu statków przeciwnika
            Console.WriteLine("Statki przeciwnika:");
            foreach (var ship in turnManager.Player2.Ships)
            {
                ship.PrintState();
            }
            Console.WriteLine($"Player1 ship count: {turnManager.Player1.Ships.Count}");
            Console.WriteLine($"Player2 ship count: {turnManager.Player2.Ships.Count}");
        }

        private void UpdateTimerText(int remainingTime)
        {
            Dispatcher.Invoke(() =>
            {
                timerTextBlock.Text = $"Time: {remainingTime} s";

                if (remainingTime <= 3)
                {
                    readyButton.Visibility = Visibility.Hidden;
                    StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
                    leftPanel.Visibility = Visibility.Collapsed;

                    Grid mainGrid = (Grid)this.Content;
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(0);
                }

                timerTextBlock.Foreground = remainingTime <= 5 ? Brushes.Red : Brushes.Black;
            });
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            readyButton.Visibility = Visibility.Hidden;
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
            leftPanel.Visibility = Visibility.Hidden;

            Grid mainGrid = (Grid)this.Content;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0);

            turnManager.SetTimerTo1Seconds();
        }

        public void HighlightBoard(bool isPlayerTurn, bool actionCompleted)
        {
            if (actionCompleted)
            {
                // Reset stylów po akcji
                gameGrid.Opacity = 1.0;
                gameGrid.Effect = null;
                opponentGrid.Opacity = 1.0;
                opponentGrid.Effect = null;

                playerBoardHeader.Opacity = 1.0;
                opponentBoardHeader.Opacity = 1.0;

                turnIndicatorTextBlock.Text = string.Empty; // Brak komunikatu
            }
            else
            {
                // Wyróżnienie aktywnej planszy i odpowiedniego nagłówka
                gameGrid.Opacity = isPlayerTurn ? 0.5 : 1.0;
                gameGrid.Effect = isPlayerTurn ? new System.Windows.Media.Effects.BlurEffect { Radius = 3 } : null;

                opponentGrid.Opacity = isPlayerTurn ? 1.0 : 0.5;
                opponentGrid.Effect = isPlayerTurn ? null : new System.Windows.Media.Effects.BlurEffect { Radius = 3 };

                playerBoardHeader.Opacity = isPlayerTurn ? 0.5 : 1.0;
                opponentBoardHeader.Opacity = isPlayerTurn ? 1.0 : 0.5;

                // Zmieniamy tekst wskaźnika tury
                turnIndicatorTextBlock.Text = isPlayerTurn ? "Your turn, shoot!" : "Wait for your turn.";
                turnIndicatorTextBlock.Foreground = isPlayerTurn ? Brushes.Green : Brushes.Red;
            }
        }

        private void ResetGame()
        {
            gameGrid.Opacity = 1.0;
            opponentGrid.Opacity = 1.0;
            gameGrid.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 0 };
            opponentGrid.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 0 };

            turnIndicatorTextBlock.Text = string.Empty;

            // Wyczyść plansze
            boardGridCreator.ClearBoard(gameGrid);
            boardGridCreator.ClearBoard(opponentGrid);

            // Reset punktów
            scoreManager.ResetScores();
            UpdateScoreDisplay();

            foreach (var ship in turnManager.Player1.Ships)
            {
                ship.ResetState();
            }

            turnManager.Player1.Ships.Clear();
            turnManager.Player2.Ships.Clear();
            turnManager.Reset();

            // Reset panelu lewego
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
            leftPanel.Width = ships.Any() ? ships.Max(ship => ship.Width * 230 + 10) : 230;

            // Przywróć widoczność przycisku "Ready" i lewego panelu
            readyButton.Visibility = Visibility.Visible;
            leftPanel.Visibility = Visibility.Visible;

            Grid mainGrid = (Grid)this.Content;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(leftPanel.Width);

            InitializePlayersAndTurnManager();
        }


        private void TurnManager_OnGameOver()
        {
            // Default values
            string message = "Unexpected game result.";
            string title = "Game Over";

            // Get player and opponent scores
            int playerScore = scoreManager.PlayerScore;
            int opponentScore = scoreManager.OpponentScore;

            // Change opacity of grids
            gameGrid.Opacity = 0.2;
            opponentGrid.Opacity = 0.2;

            _statisticsManager.CreateOrUpdateStatistics();
            _statisticsManager.IncrementGamesPlayed();

            if (turnManager.Player1.AllShipsSunk())
            {
                // Opponent won
                message = "YOU LOSE\n\n";
                message += $"Player 1 turns: {turnManager._player1Turns}\n";
                message += $"Opponent turns: {turnManager._player2Turns}\n";
                message += $"Total turns: {turnManager._player1Turns + turnManager._player2Turns}\n";
                message += $"Your score: {playerScore.ToString("N0")}\n"; // Adding space as thousand separator
                message += $"Opponent score: {opponentScore.ToString("N0")}";
                title = "Game Over";

                _statisticsManager.IncrementGamesLost();
                _statisticsManager.AddPoints(playerScore);
                _statisticsManager.UpdateHighestScore(playerScore);
            }
            else if (turnManager.Player2.AllShipsSunk())
            {
                // Player 1 won
                message = "YOU WIN\n\n";
                message += $"Player 1 turns: {turnManager._player1Turns}\n";
                message += $"Opponent turns: {turnManager._player2Turns}\n";
                message += $"Total turns: {turnManager._player1Turns + turnManager._player2Turns}\n";
                message += $"Your score: {playerScore.ToString("N0")}\n"; // Adding space as thousand separator
                message += $"Opponent score: {opponentScore.ToString("N0")}";
                title = "Game Over";

                _statisticsManager.IncrementGamesWon();
                _statisticsManager.AddPoints(playerScore);
                _statisticsManager.UpdateHighestScore(playerScore);
            }

            // Display the Game Over window with "Play Again" button
            MessageBoxResult result = MessageBox.Show($"{message}\n\nWould you like to play again?", title, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResetGame();
                rankingWindow.Hide();
            }
            else
            {
                this.Hide();
                StartupWindow startupWindow = new StartupWindow();
                startupWindow.Show();
            }
        }
    }
}