using Statki.Board;
using Statki.Class;
using Statki.Gameplay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

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

        public MainWindow()
        {
            InitializeComponent();
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
            // Create main grid
            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Left Panel for Ships
            StackPanel leftPanel = CreateLeftPanel();
            Grid.SetColumn(leftPanel, 0);
            Grid.SetRow(leftPanel, 1);
            mainGrid.Children.Add(leftPanel);

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
            return new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 200,
                Background = Brushes.LightGray,
                Visibility = Visibility.Visible
            };
        }

        private Grid CreateBoardGrid(bool isOpponent)
        {
            boardGridCreator = new BoardGridCreator();
            return boardGridCreator.CreateBoardGrid(isOpponent);
        }

        private StackPanel CreateTimerPanel()
        {
            StackPanel timerPanel = new StackPanel { Orientation = Orientation.Vertical };
            timerTextBlock = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            timerPanel.Children.Add(timerTextBlock);

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

        private void InitializePlayersAndTurnManager()
        {
            // Create Player 1 and Opponent, but do not assign the TurnManager yet
            Player player1 = new Player("Gracz 1", gameGrid, null);
            Opponent opponent = new Opponent("Oponent", opponentGrid);

            // Initialize TurnManager singleton with players and readyButton
            TurnManager.Initialize(player1, opponent, readyButton);

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

            initializer.CreateShip("Test Ship 1", 5, 1);
            initializer.CreateShip("Test Ship 2", 4, 1);
            initializer.CreateShip("Test Ship 3", 3, 1);
            initializer.CreateShip("Test Ship 4", 2, 1);
            initializer.CreateShip("Test Ship 5", 1, 1);
        }

        private void AssignShipsToPlayers()
        {
            // Przypisujemy statki graczowi 1
            foreach (var ship in ships)
            {
                turnManager.Player1.Ships.Add(ship);
            }

            // Tworzymy kopie statków dla przeciwnika
            foreach (var ship in ships)
            {
                var opponentShip = new Ship(ship.Name + " (Opponent)", ship.Length, ship.Width);
                turnManager.Player2.Ships.Add(opponentShip);
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


        private void TurnManager_OnGameOver()
        {
            string message;
            string title;

            if (turnManager.Player1.AllShipsSunk())
            {
                // Opponent wygrał
                message = "YOU LOSE\n\n";
                message += $"Liczba tur gracza 1: {turnManager._player1Turns}\n";
                message += $"Liczba tur przeciwnika: {turnManager._player2Turns}\n";
                message += $"Łączna liczba tur: {turnManager._player1Turns + turnManager._player2Turns}";
                title = "Game Over";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (turnManager.Player2.AllShipsSunk())
            {
                // Player1 wygrał
                message = "YOU WIN\n\n";
                message += $"Liczba tur gracza 1: {turnManager._player1Turns}\n";
                message += $"Liczba tur przeciwnika: {turnManager._player2Turns}\n";
                message += $"Łączna liczba tur: {turnManager._player1Turns + turnManager._player2Turns}";
                title = "Game Over";
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



        private void UpdateTimerText(int remainingTime)
        {
            Dispatcher.Invoke(() =>
            {
                timerTextBlock.Text = $"czas: {remainingTime} s";

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

            turnManager.SetTimerTo3Seconds();
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
            }
            else
            {
                // Wyróżnienie aktywnej planszy
                gameGrid.Opacity = isPlayerTurn ? 0.5 : 1.0;
                gameGrid.Effect = isPlayerTurn ? new System.Windows.Media.Effects.BlurEffect { Radius = 3 } : null;
                
                opponentGrid.Opacity = isPlayerTurn ? 1.0 : 0.5;
                opponentGrid.Effect = isPlayerTurn ? null : new System.Windows.Media.Effects.BlurEffect { Radius = 3 };
            }
        }


    }
}
