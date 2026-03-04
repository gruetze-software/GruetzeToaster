using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Linq;

namespace GruetzeToaster
{
    public class FlyingObject
    {
        public Point Position { get; set; }
        public double Speed { get; set; }
        public bool IsLogo { get; set; }
        public bool IsToast { get; set; }
        public int Frame { get; set; } = 0;
        public Image DisplayImage { get; set; } = new Image();

        private double _animTick = 0;

        public void Update(double canvasWidth, double canvasHeight, double deltaTime, IImage[] frames)
        {
            // 1. Bewegung: Nah am C64-Original (diagonal nach links unten)
            // Wir nutzen Speed direkt als Multiplikator für die Zeit
            double moveX = Speed * 120 * deltaTime;
            double moveY = Speed * 60 * deltaTime;

            Position = new Point(Position.X - moveX, Position.Y + moveY);

            // 2. Reset-Logik: Verhindert den "Perlenschnur-Effekt"
            if (Position.Y > canvasHeight + 150 || Position.X < -200)
            {
                // Random.Shared ist threadsicher und erzeugt keine neue Instanz pro Frame
            var r = Random.Shared;
                // Wechselndes Spawning: Mal von oben, mal von rechts
                if (r.Next(0, 2) == 0)
                {
                    // Startet oben, über die Breite verteilt (füllt auch rechts)
                    Position = new Point(r.Next(0, (int)canvasWidth + 600), -200);
                }
                else
                {
                    // Startet rechts, über die Höhe verteilt (füllt gezielt unten rechts)
                    Position = new Point(canvasWidth + 200, r.Next(-200, (int)canvasHeight));
                }
            }

            // 3. Animation: Flügel schlagen (nur für Toaster)
            if (!IsLogo && !IsToast)
            {
                _animTick += deltaTime;
                // 0.12s Intervall für ein angenehmes Tempo (nicht zu hektisch)
                if (_animTick > 0.12)
                {
                    Frame = (Frame + 1) % frames.Length;
                    DisplayImage.Source = frames[Frame];
                    _animTick = 0;
                }
            }

            // 4. UI-Position an den Canvas übermitteln
            Canvas.SetLeft(DisplayImage, Position.X);
            Canvas.SetTop(DisplayImage, Position.Y);
        }
    }
}