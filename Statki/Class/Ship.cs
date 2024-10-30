using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Drawing;
using Statki.Board;
namespace Statki.Class
{
    public class Ship
    {
        public int Length { get; set; }
        public int Width { get; set; }
        public string Name { get; set; }
        public bool IsHorizontal { get; set; } = true; 
        public bool IsPlaced { get; set; } = false;
        public List<BoardTile> OccupiedTiles { get; set; } = new List<BoardTile>();

        public Ship(string name, int length, int width)
        {
            Length = length;
            Width = width;
            Name = name;
        }

        public StackPanel CreateVisualRepresentation()
        {
            StackPanel stackPanel = new StackPanel
            {
                Orientation = IsHorizontal ? Orientation.Horizontal : Orientation.Vertical,
                Background = Brushes.Gray,
                Width = IsHorizontal ? Length * 35 : Width * 35, // Dostosowanie szerokości do orientacji
                Height = IsHorizontal ? Width * 35 : Length * 35, // Dostosowanie wysokości do orientacji
                Margin = new Thickness(10),
                Tag = this // Przechowujemy statek w Tag
            };

            return stackPanel;
        }


    }
}

