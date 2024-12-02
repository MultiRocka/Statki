using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Statki.Board;
using Statki.Class;
using Statki.Gameplay;

namespace Statki
{
    public partial class MainWindow : Window
    {
        private Grid gameGrid;
        private Grid opponentGrid;
        private BoardTileDragHandler _dragHandler;
        private List<Ship> ships = new List<Ship>(); // Lista wszystkich statków
        private KeyAndMouseMonitor shipDragHandler = new KeyAndMouseMonitor();
        private BoardGridCreator boardGridCreator;

        private TurnManager turnManager;

        private TextBlock timerTextBlock;

        private Button readyButton; // Zmienna członkowska

        public event Action<int> OnTimerUpdate;

        public MainWindow()
        {
            InitializeComponent();
            CreateLayout();
            CreateShips(); // Tworzymy statki w lewym panelu
            Width = 1200;
            Height = 600;

            MinHeight = 400;
            MinWidth = 1000;

            Player player1 = new Player("Gracz 1", gameGrid); // Zmienione, aby przekazać planszę gracza
            Player player2 = new Player("Oponent", opponentGrid); // Zmienione, aby przekazać planszę przeciwnika

            turnManager = new TurnManager(player1, player2, readyButton);
            turnManager.OnGameOver += TurnManager_OnGameOver;
            turnManager.OnTimerUpdate += UpdateTimerText;

            AssignShipsToPlayers();

            // Rozpoczęcie gry
            turnManager.Start();
        }

        private void CreateLayout()
        {
            // Tworzenie głównej siatki
            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) }); // Lewa kolumna na statki
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza gracza
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); //kolumna na timer
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza przeciwnika

            // Tworzenie panelu dla statków po lewej stronie
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 200,
                Background = Brushes.LightGray,
                Margin = new Thickness(10)
            };
            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);

            // Tworzenie planszy gracza
            boardGridCreator = new BoardGridCreator();
            gameGrid = boardGridCreator.CreateBoardGrid(isOpponent: false);
            Grid.SetColumn(gameGrid, 1);
            mainGrid.Children.Add(gameGrid);

            StackPanel timerPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
            };
            timerTextBlock = new TextBlock
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            timerPanel.Children.Add(timerTextBlock);
            Grid.SetColumn(timerPanel, 2);
            mainGrid.Children.Add(timerPanel);

            // Przypisanie do zmiennej członkowskiej
            readyButton = new Button
            {
                Content = "Ready",
                Visibility = Visibility.Hidden,
                Width = 100,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            readyButton.Click += ReadyButton_Click;
            timerPanel.Children.Add(readyButton); // Dodanie przycisku do panelu

            // Tworzenie planszy przeciwnika
            opponentGrid = boardGridCreator.CreateBoardGrid(isOpponent: true);
            Grid.SetColumn(opponentGrid, 3);
            mainGrid.Children.Add(opponentGrid);

            // Ustawienie głównej zawartości okna
            this.Content = mainGrid;
        }

        private void CreateShips()
        {
            // Znalezienie lewego panelu (pierwsza kolumna głównej siatki)
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];

            // Inicjalizator statków
            ShipInitializer initializer = new ShipInitializer(shipDragHandler, ships, leftPanel);

            // Tworzenie statków
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
        }

        private void TurnManager_OnGameOver()
        {
            MessageBox.Show("Gra zakończona!");
            // Możesz dodać kod do pokazania wyniku lub restartu gry
        }

        private void UpdateTimerText(int remainingTime)
        {
            Dispatcher.Invoke(() =>
            {
                timerTextBlock.Text = $"czas: {remainingTime} s";

                // Sprawdzamy, czy przycisk "Ready" powinien zniknąć
                if (remainingTime <= 3)
                {
                    readyButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    readyButton.Visibility = Visibility.Visible;
                }

                if (remainingTime <= 5)
                {
                    timerTextBlock.Foreground = Brushes.Red; // Ostrzeżenie
                }
                else
                {
                    timerTextBlock.Foreground = Brushes.Black; // Domyślny kolor
                }
            });
        }


        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            // Ukrywamy przycisk "Ready" po kliknięciu
            readyButton.Visibility = Visibility.Hidden;

            // Rozpoczynamy timer na 3 sekundy
            TimerForNextPhase(3); // Timer na 3 sekundy, który zacznie fazę układania statków
        }


        private void TimerForNextPhase(int seconds)
        {
            // Ustawiamy timer na 3 sekundy, które trwają w fazie układania statków
            DispatcherTimer phaseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            int remainingTime = seconds;
            phaseTimer.Tick += (sender, e) =>
            {
                remainingTime--;
                OnTimerUpdate?.Invoke(remainingTime); // Wyświetlamy timer w interfejsie
                if (remainingTime <= 0)
                {
                    phaseTimer.Stop();
                    turnManager.AutoPlaceShips(); // Wywołanie metody z TurnManager
                    turnManager.StartTurnPhase(); // Rozpoczynamy turę
                }
            };
            phaseTimer.Start();
        }


    }

}
