using Statki.Class;
using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace Statki.Board
{
    public class KeyAndMouseMonitor
    {
        private Ship currentlyDraggingShip; // Obiekt aktualnie przeciąganego statku
        private Point startPoint; // Punkt początkowy przeciągania
        private DispatcherTimer keyCheckTimer; // Timer do sprawdzania stanu klawiszy

        private readonly Grid _gameGrid;
        public KeyAndMouseMonitor()
        {

            keyCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100) // Częstotliwość sprawdzania klawiszy
            };
            keyCheckTimer.Tick += CheckKeyStates;
        }

        public void ShipPanel_MouseDown(object sender, MouseButtonEventArgs e, Ship ship)
        {
            if (sender is StackPanel && e.LeftButton == MouseButtonState.Pressed)
            {
                startPoint = e.GetPosition(Application.Current.MainWindow);
                currentlyDraggingShip = ship; // Ustawienie przeciąganego statku

            }
        }

        public void ShipPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentlyDraggingShip != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is StackPanel panel)
                {
                    keyCheckTimer.Start(); // Startuje timer przed rozpoczęciem DoDragDrop
                    DragDrop.DoDragDrop(panel, panel.Tag, DragDropEffects.Move);
                    keyCheckTimer.Stop(); // Zatrzymuje timer po zakończeniu DoDragDrop
                }
            }
        }

        private void CheckKeyStates(object sender, EventArgs e)
        {
            // Sprawdzanie stanu klawisza R
            if (Keyboard.IsKeyDown(Key.R) && currentlyDraggingShip != null)
            {
                RotateShip(currentlyDraggingShip); 
            }
        }



        public void ShipPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            currentlyDraggingShip = null; // Resetuje przeciąganie po puszczeniu przycisku myszy
            keyCheckTimer.Stop(); // Zatrzymuje timer, gdy kończymy przeciąganie
        }

        private void RotateShip(Ship ship)
        {
       
            ship.Rotate();
            Console.WriteLine("R Wcisniete");

        }

    }
}
