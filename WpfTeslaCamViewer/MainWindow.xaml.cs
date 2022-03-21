using LibVLCSharp.Shared;
using System;
using System.Timers;
using System.Windows;

namespace WpfTeslaCamViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LibVLC? _libVLC;
        LibVLCSharp.Shared.MediaPlayer? playerFront, playerLeft, playerRight, playerBack;
        TeslaCamPlayer? player;
        float playbackSpeed;
        Timer PlayerInfoTimer;

        public MainWindow()
        {
            InitializeComponent();
            playbackSpeed = 1f;
            PlayerInfoTimer = new(500);
            PlayerInfoTimer.Elapsed += async (object? sender, ElapsedEventArgs e) =>
            {
                if (player != null)
                {
                    string info = player.GetDebugInfo();

                    float Position = player.GetCurrentVideoPosition();

                    await Application.Current.Dispatcher.BeginInvoke(() => { 
                        lbl_DebugInfo.Content = info;
                        slider_progress.Value = Position;
                    });
                }
            };
            PlayerInfoTimer.Start();
        }

        private void btn_SlowDown_Click(object sender, RoutedEventArgs e)
        {
            playbackSpeed -= 0.1f;
            SetPlaybackRate();
        }

        private void btn_SpeedUp_Click(object sender, RoutedEventArgs e)
        {
            playbackSpeed += 0.1f;
            SetPlaybackRate();
        }

        private void SetPlaybackRate()
        {
            lbl_playbackspeed.Content = Math.Round(playbackSpeed, 1).ToString();
            playerFront?.SetRate(playbackSpeed);
            playerLeft?.SetRate(playbackSpeed);
            playerRight?.SetRate(playbackSpeed);
            playerBack?.SetRate(playbackSpeed);
        }

        private void slider_progress_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlayerInfoTimer.Stop();
        }

        private void slider_progress_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            player?.SetPosition((float)slider_progress.Value);
            PlayerInfoTimer.Start();
        }

        private void btn_GoBack_Click(object sender, RoutedEventArgs e)
        {
            player?.SkipBack();
        }

        private void btn_GoForward_Click(object sender, RoutedEventArgs e)
        {
            player?.SkipForward();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            playerFront = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            playerLeft = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            playerRight = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            playerBack = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

            videoViewFront.MediaPlayer = playerFront;
            videoViewLeftRepeater.MediaPlayer = playerLeft;
            videoViewRightRepeater.MediaPlayer = playerRight;
            videoViewRear.MediaPlayer = playerBack;

            player = new(_libVLC, playerFront, playerLeft, playerRight, playerBack);

            UpdateWindowTitle("No directory opened");
        }

        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to read TeslaCam videos from.";
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = false;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && player != null)
                {
                    UpdateWindowTitle(player.Play(dialog.SelectedPath) ? dialog.SelectedPath : "Invalid directory");
                }
            }
        }

        private void UpdateWindowTitle(string Path)
        {
            mainWindow.Title = "WpfTeslaCamViewer - " + Path;
        }
    }
}
