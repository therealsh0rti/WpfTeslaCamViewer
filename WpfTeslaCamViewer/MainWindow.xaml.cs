using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace WpfTeslaCamViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly LibVLC libVlc;
        private MediaPlayer? playerFront, playerLeft, playerRight, playerBack;
        private TeslaCamPlayer? player;
        private float playbackSpeed;
        private readonly Timer playerInfoTimer;

        public ObservableCollection<string> FolderNames { get; set; } = new();
        public HashSet<string> FileNames { get; set; } = new();

        public MainWindow(LibVLC libVlc)
        {
            this.libVlc = libVlc;
            InitializeComponent();
            playbackSpeed = 1f;
            playerInfoTimer = new Timer(500);
            playerInfoTimer.Elapsed += async (_, _) =>
            {
                if (player == null) return;

                var info = player.GetDebugInfo();
                var position = player.GetCurrentVideoPosition();

                await Application.Current.Dispatcher.BeginInvoke(() => { 
                    lbl_DebugInfo.Content = info;
                    slider_progress.Value = position;
                });
            };
            playerInfoTimer.Start();
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
            playerInfoTimer.Stop();
        }

        private void slider_progress_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            player?.SetPosition((float)slider_progress.Value);
            playerInfoTimer.Start();
        }

        private void btn_GoBack_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btn_GoForward_Click(object sender, RoutedEventArgs e)
        {
            player?.SkipForward();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            playerFront = new MediaPlayer(libVlc);
            playerLeft = new MediaPlayer(libVlc);
            playerRight = new MediaPlayer(libVlc);
            playerBack = new MediaPlayer(libVlc);

            videoViewFront.MediaPlayer = playerFront;
            videoViewLeftRepeater.MediaPlayer = playerLeft;
            videoViewRightRepeater.MediaPlayer = playerRight;
            videoViewRear.MediaPlayer = playerBack;

            player = new TeslaCamPlayer(libVlc, playerFront, playerLeft, playerRight, playerBack);

            UpdateWindowTitle("No directory opened");
        }

        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select a folder to read TeslaCam videos from.";
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = false;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && player != null)
            {
                var allEntries =
                    Directory.EnumerateFileSystemEntries(dialog.SelectedPath, "*", SearchOption.TopDirectoryOnly);

                var entries = allEntries.ToList();
                
                FolderNames.Clear();
                FileNames.Clear();
                foreach (var dir in entries.Where(Directory.Exists))
                {
                    FolderNames.Add(dir);
                }

                foreach (var file in entries.Where(File.Exists))
                {
                    FileNames.Add(file);
                }

                cmbFolderList.ItemsSource = FolderNames.Select(x => x.Replace($"{dialog.SelectedPath}\\", ""));
                lbFileNames.ItemsSource = FileNames.Select(x => x.Replace($"{dialog.SelectedPath}\\", ""));

                cmbFolderList.SelectedIndex = 0;
            }
        }

        private void UpdateWindowTitle(string Path)
        {
            mainWindow.Title = "WpfTeslaCamViewer - " + Path;
        }

        private void LbFileNames_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlaySelectedFile();
        }

        private void PlaySelectedFile()
        {
            if (FileNames.Count <= 0) return;

            var selectedFile = FileNames.FirstOrDefault(x => x.Contains(lbFileNames.SelectedValue?.ToString() ?? string.Empty));

            if (player != null)
            {
                UpdateWindowTitle(player.Play(selectedFile) ? selectedFile : "Invalid directory");
            }
        }

        private void CmbFolderList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var directory = FolderNames[cmbFolderList.SelectedIndex];

            FileNames.Clear();
            foreach (var file in Directory.GetFiles(directory, "*.mp4"))
            {
                FileNames.Add(file.Replace("-back", "").Replace("-front", "").Replace("-left_repeater", "")
                    .Replace("-right_repeater", "").Replace(".mp4", ""));
            }

            lbFileNames.ItemsSource =
                new ObservableCollection<string>(FileNames.Select(x => x.Replace($"{directory}\\", "")));
            lbFileNames.SelectedIndex = 0;
            lbFileNames.Focus();
        }
    }
}
