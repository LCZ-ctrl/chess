using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ChessUI
{
    public static class WindowManager
    {
        public static MainMenu MainMenu { get; private set; }
        public static MainWindow MainWindow { get; private set; }

        public static Window GameWindow => MainWindow;

        public static void Init()
        {
            MainMenu = new MainMenu
            {
                ShowInTaskbar = true
            };

            MainWindow = new MainWindow
            {
                ShowInTaskbar = true
            };

            Application.Current.MainWindow = MainMenu; 
            MainMenu.Show();
        }

        public static void Show(Window target, Window from = null, int durationMs = 0, Action onOpaque = null)
        {
            if (target == null) return;

            if (from == MainWindow && target == MainMenu)
            {
                onOpaque?.Invoke();

                if (from.IsVisible) from.Hide();

                if (!target.IsVisible)
                {
                    target.Show();
                }
                else
                {
                    target.Activate();
                }

                return;
            }

            if (from == null || !from.IsVisible)
            {
                onOpaque?.Invoke();

                if (!target.IsVisible)
                {
                    target.Show();
                }
                else
                {
                    target.Activate();
                }

                return;
            }

            RunTransition(from, target, Math.Max(0, durationMs), onOpaque);
        }

        private static void RunTransition(Window from, Window target, int totalMs, Action onOpaque)
        {
            if (from == null || target == null) return;

            var halfMs = Math.Max(1, totalMs / 2);

            Window overlay = new Window
            {
                Owner = from,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Black,
                Opacity = 0,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Width = from.ActualWidth > 0 ? from.ActualWidth : from.Width,
                Height = from.ActualHeight > 0 ? from.ActualHeight : from.Height,
                Left = from.Left,
                Top = from.Top,
                Topmost = true
            };

            if (double.IsNaN(overlay.Width) || overlay.Width <= 0)
            {
                overlay.Width = from.RestoreBounds.Width;
                overlay.Height = from.RestoreBounds.Height;
                overlay.Left = from.RestoreBounds.Left;
                overlay.Top = from.RestoreBounds.Top;
            }

            overlay.Show();

            var fadeIn = new DoubleAnimation(0d, 1d, new Duration(TimeSpan.FromMilliseconds(halfMs)))
            {
                FillBehavior = FillBehavior.HoldEnd
            };

            fadeIn.Completed += (s, e) =>
            {
                onOpaque?.Invoke();

                if (from.IsVisible) from.Hide();

                if (!target.IsVisible)
                {
                    target.Show();
                }
                else
                {
                    target.Activate();
                }

                var fadeOut = new DoubleAnimation(1d, 0d, new Duration(TimeSpan.FromMilliseconds(halfMs)))
                {
                    FillBehavior = FillBehavior.Stop
                };

                fadeOut.Completed += (s2, e2) =>
                {
                    overlay.Close();
                };

                overlay.BeginAnimation(Window.OpacityProperty, fadeOut);
            };

            overlay.BeginAnimation(Window.OpacityProperty, fadeIn);
        }
    }
}
