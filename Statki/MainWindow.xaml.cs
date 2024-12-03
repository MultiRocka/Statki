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
            Width = 1300;
            Height = 600;

            MinHeight = 400;
            MinWidth = 1000;

            Player player1 = new Player("Gracz 1", gameGrid); // Zmienione, aby przekazać planszę gracza
            Player player2 = new Player("Oponent", opponentGrid); // Zmienione, aby przekazać planszę przeciwnika

            turnManager = new TurnManager(player1, player2, readyButton);
            turnManager.OnGameOver += TurnManager_OnGameOver;
            turnManager.OnTimerUpdate += UpdateTimerText;

            AssignShipsToPlayers();
            turnManager.Start();
        }

        private void CreateLayout()
        {
            // Tworzenie głównej siatki
            Grid mainGrid = new Grid();

            // Dodanie wiersza na napisy
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) }); // Wiersz na napisy

            // Dodanie wierszy na plansze i inne elementy
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Plansza gracza i przeciwnika
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) }); // Lewa kolumna na statki
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza gracza
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // Kolumna na timer
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza przeciwnika

            // Tworzenie panelu dla statków po lewej stronie
            StackPanel leftPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Width = 200,
                Background = Brushes.LightGray,
            };
            Grid.SetColumn(leftPanel, 0);
            Grid.SetRow(leftPanel, 1); // Ustawiamy panel w drugim wierszu
            mainGrid.Children.Add(leftPanel);

            // Tworzenie planszy gracza
            boardGridCreator = new BoardGridCreator();
            gameGrid = boardGridCreator.CreateBoardGrid(isOpponent: false);
            Grid.SetColumn(gameGrid, 1);
            Grid.SetRow(gameGrid, 1); // Plansza gracza w drugim wierszu
            mainGrid.Children.Add(gameGrid);

            // Tworzenie napisu "Your Board" nad planszą gracza
            TextBlock yourBoardText = new TextBlock
            {
                Text = "Your Board",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            StackPanel yourBoardPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Top
            };
            yourBoardPanel.Children.Add(yourBoardText);
            Grid.SetColumn(yourBoardPanel, 1);
            Grid.SetRow(yourBoardPanel, 0); // Ustawiamy wiersz z napisem nad planszą
            mainGrid.Children.Add(yourBoardPanel);

            // Tworzenie napisu "Opponent Board" nad planszą przeciwnika
            TextBlock opponentBoardText = new TextBlock
            {
                Text = "Opponent Board",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            StackPanel opponentBoardPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Top
            };
            opponentBoardPanel.Children.Add(opponentBoardText);
            Grid.SetColumn(opponentBoardPanel, 3);
            Grid.SetRow(opponentBoardPanel, 0); // Ustawiamy wiersz z napisem nad planszą
            mainGrid.Children.Add(opponentBoardPanel);

            // Tworzenie planszy przeciwnika
            opponentGrid = boardGridCreator.CreateBoardGrid(isOpponent: true);
            Grid.SetColumn(opponentGrid, 3);
            Grid.SetRow(opponentGrid, 1); // Plansza przeciwnika w drugim wierszu
            mainGrid.Children.Add(opponentGrid);

            // Tworzenie panelu timera
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
            Grid.SetRow(timerPanel, 1); // Timer w drugim wierszu
            mainGrid.Children.Add(timerPanel);

            // Przypisanie do zmiennej członkowskiej
            readyButton = new Button
            {
                Content = "Ready",
                Visibility = Visibility.Visible,
                Width = 100,
                Height = 50,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            readyButton.Click += ReadyButton_Click;
            timerPanel.Children.Add(readyButton);

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
        }


        private void TurnManager_OnGameOver()
        {
            MessageBox.Show("Gra zakończona!");
            // Możesz dodać kod do pokazania wyniku lub restartu gry

            Console.WriteLine("Statki przeciwnika:");
            foreach (var ship in turnManager.Player2.Ships)
            {
                ship.PrintState();
            }

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

                    StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
                    leftPanel.Visibility = Visibility.Collapsed;

                    Grid mainGrid = (Grid)this.Content;
                    mainGrid.ColumnDefinitions[0].Width = new GridLength(0); // Pierwsza kolumna - lewy panel
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
            // Ukrywamy przycisk "Ready"
            readyButton.Visibility = Visibility.Hidden;

            // Ukrywamy lewy panel (po kliknięciu "Ready")
            StackPanel leftPanel = (StackPanel)((Grid)this.Content).Children[0];
            leftPanel.Visibility = Visibility.Hidden;

            // Zmieniamy szerokość kolumny na 0, aby przestrzeń po lewym panelu zniknęła
            Grid mainGrid = (Grid)this.Content;
            mainGrid.ColumnDefinitions[0].Width = new GridLength(0); // Pierwsza kolumna - lewy panel


            // Ustawiamy timer na 3 sekundy
            turnManager.SetTimerTo3Seconds();
        }


    }

}
