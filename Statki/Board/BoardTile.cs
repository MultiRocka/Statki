﻿using Statki.Class;
using Statki.Gameplay;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Statki.Board
{
    public enum HitStatus
    {
        None,
        Miss,
        Hit
    }

    public class BoardTile : Button
    {
        public bool IsOccupied { get; set; } = false;
        public HitStatus HitStatus { get; set; } = HitStatus.None;
        public Brush DefaultBackground { get; set; } = Brushes.LightBlue;
        private Brush PreviousBackground { get; set; }
        public int Row { get; set; } 
        public int Column { get; set; }
        public Ship AssignedShip { get; set; }
        public bool IsOpponent { get; set; }
        public Grid ParentGrid { get; set; }
        private Border HoverOverlay { get; set; }

        public BoardTile()
        {
            this.Background = DefaultBackground;
            this.Click += BoardTile_Click;
            this.Style = CreateCustomStyle();
            
        }
        private Style CreateCustomStyle()
        {
            // Tworzenie stylu
            var style = new Style(typeof(BoardTile));

            // Domyślne właściwości
            style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightBlue));
            style.Setters.Add(new Setter(BorderBrushProperty, Brushes.Black));
            style.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(1)));

            // Template przycisku
            var template = new ControlTemplate(typeof(BoardTile));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(BorderThicknessProperty));

            var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

            borderFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = borderFactory;

            style.Setters.Add(new Setter(TemplateProperty, template));

            // Trigger dla IsMouseOver
            var isMouseOverTrigger = new Trigger
            {
                Property = IsMouseOverProperty,
                Value = true
            };
            isMouseOverTrigger.Setters.Add(new Setter(BackgroundProperty, Brushes.Transparent));
            isMouseOverTrigger.Setters.Add(new Setter(BorderBrushProperty, Brushes.DarkBlue));
            isMouseOverTrigger.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(5))); 

            style.Triggers.Add(isMouseOverTrigger);

            return style;
        }

        private void BoardTile_Click(object sender, RoutedEventArgs e)
        {
            var tile = sender as BoardTile;

            if (tile != null)
            {
                var handler = new BoardTileClickHandler();
                handler.HandleTileClick(tile);

            }
        }

        // Metoda resetująca stan pola
        public void Reset()
        {
            IsOccupied = false;
            HitStatus = HitStatus.None;
            // Resetowanie przypisanego statku
            AssignedShip = null;
            // Ustawienie domyślnego tła
            this.Background = DefaultBackground;
        }



        // Opcjonalnie: metoda do ustawiania tekstury lub animacji
        public void SetBackgroundImage(ImageSource image)
        {
            this.Background = new ImageBrush(image);
        }

        public void ResetBackground()
        {
            if (HitStatus == HitStatus.None)
                this.Background = DefaultBackground;
            if (IsOccupied==true)
                this.Background = Brushes.Blue;
        }

        public async void UpdateTileAppearance()
        {

            if (IsOccupied && HitStatus == HitStatus.None && IsOpponent ==false)
            {
               this.Background = Brushes.Blue;
            }
            else
            {
                if (AssignedShip != null && AssignedShip.IsSunk)
                {
                    await AnimateSinking(AssignedShip.OccupiedTiles, TimeSpan.FromMilliseconds(75));
                }
                else
                {
                    switch (HitStatus)
                    {
                        case HitStatus.None:
                            this.Background = DefaultBackground;
                            break;
                        case HitStatus.Miss:
                            this.Background = Brushes.Gray;
                            break;
                        case HitStatus.Hit:
                            this.Background = Brushes.Red;
                            break;
                    }
                }
            }
        }

        public async Task AnimateSinking(IEnumerable<BoardTile> tiles, TimeSpan delay)
        {
            foreach (var tile in tiles)
            {
                tile.Background = Brushes.Black; // Zmiana koloru na czarny
                await Task.Delay(delay);        // Oczekiwanie przed zmianą kolejnego kafelka
            }
        }

        public void DisplayPointsAnimation(int points, double multiplier)
        {
            // Tworzenie tekstu
            TextBlock pointsText = new TextBlock
            {
                Text = $"+{points} x{multiplier}",
                Foreground = Brushes.Gold,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Dodanie do nadrzędnego kontenera (ParentGrid)
            ParentGrid.Children.Add(pointsText);

            // Ustawienie pozycji na podstawie pozycji BoardTile w siatce
            Grid.SetRow(pointsText, Row);
            Grid.SetColumn(pointsText, Column);

            // Tworzymy transformację dla przesunięcia
            TranslateTransform translate = new TranslateTransform();
            pointsText.RenderTransform = translate;

            // Tworzenie animacji przesunięcia w górę
            DoubleAnimation moveUp = new DoubleAnimation
            {
                From = 0,
                To = -30,
                Duration = TimeSpan.FromSeconds(1)
            };

            // Tworzenie animacji zanikania
            DoubleAnimation fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromMilliseconds(500)
            };

            // Tworzenie storyboardu
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(moveUp);
            storyboard.Children.Add(fadeOut);

            Storyboard.SetTarget(moveUp, pointsText);
            Storyboard.SetTarget(fadeOut, pointsText);
            Storyboard.SetTargetProperty(moveUp, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));

            // Usunięcie elementu po zakończeniu animacji
            storyboard.Completed += (s, e) =>
            {
                ParentGrid.Children.Remove(pointsText);  // Usuwamy tekst po animacji
            };

            // Uruchomienie animacji
            storyboard.Begin();
        }
    }
}
