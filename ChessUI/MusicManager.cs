using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace ChessUI
{
    public static class MusicManager
    {
        private static MediaPlayer menuPlayer = null!;
        private const string MenuResourceName = "ChessUI.Assets.menu1.mp3";
        private static readonly string MenuFilePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChessUI", "menu1.mp3");

        private static readonly Dictionary<string, string> soundResources = new()
        {
            { "check", "ChessUI.Assets.check.wav" },
            { "capture", "ChessUI.Assets.capture.wav" },
            { "move", "ChessUI.Assets.move.wav" },
            { "castle", "ChessUI.Assets.castle.wav" },
            { "promote", "ChessUI.Assets.promote.wav" },
            { "gameover", "ChessUI.Assets.gameover.wav" }
        };

        private static readonly string SoundsFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ChessUI", "Sounds");

        private static bool initialized = false;
        private static double currentVolume = 1.0;
        private static bool isPlayingMenu = false;

        public static void Initialize()
        {
            if (initialized) return;

            try
            {
                Directory.CreateDirectory(SoundsFolder);
                Directory.CreateDirectory(Path.GetDirectoryName(MenuFilePath) ?? Path.GetTempPath());

                TryExtractResource(MenuResourceName, MenuFilePath);

                foreach (var pair in soundResources)
                {
                    string resName = pair.Value;
                    string fileName = Path.GetFileName(resName);
                    string targetPath = Path.Combine(SoundsFolder, fileName);
                    TryExtractResource(resName, targetPath);
                }

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    menuPlayer = new MediaPlayer();
                    menuPlayer.Volume = ClampVolume(currentVolume);
                    menuPlayer.MediaEnded += (s, e) =>
                    {
                        try
                        {
                            menuPlayer.Position = TimeSpan.Zero;
                            menuPlayer.Play();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("MenuPlayer loop failed: " + ex);
                        }
                    };
                });

                initialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MusicManager.Initialize failed: " + ex);
                initialized = false;
            }
        }

        private static double ClampVolume(double v)
        {
            if (double.IsNaN(v)) return 0.0;
            return Math.Max(0.0, Math.Min(1.0, v));
        }

        private static void TryExtractResource(string resourceName, string targetPath)
        {
            try
            {
                if (string.IsNullOrEmpty(resourceName)) return;
                if (File.Exists(targetPath) && new FileInfo(targetPath).Length > 0) return;

                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (stream == null)
                {
                    Debug.WriteLine("Resource not found: " + resourceName);
                    return;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? Path.GetTempPath());
                using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                stream.CopyTo(fs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TryExtractResource failed for {resourceName} -> {targetPath}: {ex}");
            }
        }

        public static void PlayMenuMusic()
        {
            Initialize();
            if (!initialized) return;

            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    StopMenuMusicInternal();

                    if (!File.Exists(MenuFilePath))
                    {
                        Debug.WriteLine("Menu file not found: " + MenuFilePath);
                        return;
                    }

                    menuPlayer.Open(new Uri(MenuFilePath, UriKind.Absolute));
                    menuPlayer.Volume = ClampVolume(currentVolume);
                    menuPlayer.Position = TimeSpan.Zero;
                    menuPlayer.Play();
                    isPlayingMenu = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PlayMenuMusic failed: " + ex);
                }
            }));
        }

        private static void StopMenuMusicInternal()
        {
            try
            {
                if (menuPlayer != null)
                {
                    menuPlayer.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Stop menu music failed: " + ex);
            }
            isPlayingMenu = false;
        }

        public static void Stop()
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                StopMenuMusicInternal();
            }));
        }

        public static void StopMusic() => Stop();

        public static void PlaySound(string type)
        {
            try
            {
                Initialize();
            }
            catch { }

            if (!initialized) return;

            if (!soundResources.TryGetValue(type, out string resName))
            {
                Debug.WriteLine("Unknown sound type: " + type);
                return;
            }

            string fileName = Path.GetFileName(resName);
            string filePath = Path.Combine(SoundsFolder, fileName);

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("Sound file not found: " + filePath);
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var player = new MediaPlayer();
                    player.Volume = ClampVolume(currentVolume);
                    player.Open(new Uri(filePath, UriKind.Absolute));

                    player.MediaEnded += (s, e) =>
                    {
                        try { player.Close(); } catch { }
                    };

                    player.MediaFailed += (s, e) =>
                    {
                        Debug.WriteLine($"MediaFailed: {filePath} - {e.ErrorException?.Message}");
                        try { player.Close(); } catch { }
                    };

                    player.Play();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("PlaySound failed: " + ex.Message);
                }
            }));
        }

        public static void SetVolume(double volume)
        {
            currentVolume = ClampVolume(volume);

            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (menuPlayer != null)
                        menuPlayer.Volume = currentVolume;
                }
                catch { }
            }));
        }

        public static double GetVolume() => currentVolume;
    }
}