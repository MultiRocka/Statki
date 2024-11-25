using Statki.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Statki.Class
{
    public class ShipInitializer
    {
        private KeyAndMouseMonitor shipDragHandler = new KeyAndMouseMonitor();
        private readonly List<Ship> ships;
        private readonly StackPanel leftPanel;

        public ShipInitializer(KeyAndMouseMonitor shipDragHandler, List<Ship> ships, StackPanel leftPanel)
        {
            this.shipDragHandler = shipDragHandler;
            this.ships = ships;
            this.leftPanel = leftPanel;
        }

        public void CreateShip(string name, int length, int width)
        {
            // Tworzymy statek
            Ship ship = new Ship(name, length, width);
            StackPanel shipPanel = ship.CreateVisualRepresentation();

            // Obsługa przeciągania
            shipPanel.MouseDown += (sender, e) => shipDragHandler.ShipPanel_MouseDown(sender, e, ship);
            shipPanel.MouseMove += shipDragHandler.ShipPanel_MouseMove;
            shipPanel.MouseUp += shipDragHandler.ShipPanel_MouseUp;

            // Dodajemy statek do listy
            ships.Add(ship);

            // Dodajemy panel statku do lewego panelu
            leftPanel.Children.Add(shipPanel);
        }
    }

}
