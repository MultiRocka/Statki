using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
            Player player2 = new Player("Gracz 2", opponentGrid); // Zmienione, aby przekazać planszę przeciwnika

            // Inicjalizacja menedżera tur
            turnManager = new TurnManager(player1, player2);

            // Rejestracja zdarzenia końca gry
            turnManager.OnGameOver += TurnManager_OnGameOver;

            // Rozpoczęcie gry
            turnManager.Start();
        }

        private void CreateLayout()
        {
            // Tworzenie głównej siatki
            Grid mainGrid = new Grid();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Lewa kolumna na statki
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Plansza gracza
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

            // Tworzenie planszy przeciwnika
            opponentGrid = boardGridCreator.CreateBoardGrid(isOpponent: true);
            Grid.SetColumn(opponentGrid, 2);
            opponentGrid.Margin = new Thickness(10, 0, 0, 0); // Dodanie odstępu od planszy gracza
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
            initializer.CreateShip("Test Ship 1", 3, 2);
            initializer.CreateShip("Test Ship 2", 4, 1);
            initializer.CreateShip("Test Ship 3", 2, 1);
            initializer.CreateShip("Test Ship 4", 1, 1);
            initializer.CreateShip("Test Ship 5", 5, 1);
        }

        private void TurnManager_OnGameOver()
        {
            MessageBox.Show("Gra zakończona!");
            // Możesz dodać kod do pokazania wyniku lub restartu gry
        }
    }
}
