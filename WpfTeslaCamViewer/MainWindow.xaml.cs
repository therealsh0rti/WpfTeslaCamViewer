using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainWindow()
        {
            InitializeComponent();
            playbackSpeed = 1f;
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
            lbl_playbackspeed.Content = playbackSpeed.ToString();
            playerFront?.SetRate(playbackSpeed);
            playerLeft?.SetRate(playbackSpeed);
            playerRight?.SetRate(playbackSpeed);
            playerBack?.SetRate(playbackSpeed);

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
            player.SetDebugInfoAction(info => lbl_DebugInfo.Content = info);

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
