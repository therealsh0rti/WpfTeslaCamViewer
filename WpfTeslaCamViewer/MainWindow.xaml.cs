using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            if (playbackSpeed > 0.1f)
            {
                playbackSpeed -= 0.1f;
                SetPlaybackRate();
            }
        }

        private void btn_SpeedUp_Click(object sender, RoutedEventArgs e)
        {
            playbackSpeed += 0.1f;
            SetPlaybackRate();
        }

        private void SetPlaybackRate()
        {
            lbl_playbackspeed.Content = $"{Math.Round(playbackSpeed, 1)}x";
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
            GoToLastClip();
        }

        private void btn_GoForward_Click(object sender, RoutedEventArgs e)
        {
            GoToNextClip();
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

            player = new TeslaCamPlayer(libVlc, playerFront, playerLeft, playerRight, playerBack, async () => await Application.Current.Dispatcher.BeginInvoke(GoToNextClip));

            UpdateWindowTitle("No directory opened");
        }

        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select the folder containing the clip folders (e.g. SavedClips)";
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = false;
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                player?.Stop();

                //Don't include files here
                var entries = Directory.GetDirectories(dialog.SelectedPath).ToList();

                FolderNames.Clear();
                FileNames.Clear();

                if (entries.Count == 0)
                {
                    cmbFolderList.ItemsSource = null;
                    lbFileNames.ItemsSource = null;
                    MessageBox.Show("No subdirectories found!\nPlease select the folder containing the clip-subdirectories, for example the \"SavedClips\" directory.",
                        "Invalid folder selected", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    foreach (var dir in entries.Where(Directory.Exists))
                    {
                        FolderNames.Add(dir);
                    }

                    cmbFolderList.ItemsSource = FolderNames.Select(x => x.Replace($"{dialog.SelectedPath}\\", ""));
                    lbFileNames.ItemsSource = FileNames.Select(x => x.Replace($"{dialog.SelectedPath}\\", ""));

                    cmbFolderList.SelectedIndex = 0;
                }
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

            if (player != null && !String.IsNullOrWhiteSpace(selectedFile))
            {
                UpdateWindowTitle(player.Play(selectedFile) ? selectedFile : "Invalid directory");
            }
        }

        private void CmbFolderList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFolderList.Items.Count == 0 || cmbFolderList.SelectedIndex == -1)
            {
                //Selected an invalid folder as TeslaCam folder maybe
                player?.Stop();
            }
            else
            {
                var directory = FolderNames[cmbFolderList.SelectedIndex];

                FileNames.Clear();
                var files = Directory.GetFiles(directory, "*.mp4");
                if (files.Length == 0)
                {
                    lbFileNames.ItemsSource = null;
                    MessageBox.Show("No video files found in selected folder!\nMake sure you selected the correct folder.",
                        "Nothing to show", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cmbFolderList.Focus();
                }
                else
                {
                    foreach (var file in files)
                    {
                        FileNames.Add(file.Replace("-back", "").Replace("-front", "").Replace("-left_repeater", "")
                            .Replace("-right_repeater", "").Replace(".mp4", ""));
                    }

                    lbFileNames.ItemsSource =
                        new ObservableCollection<string>(FileNames.Select(x => x.Replace($"{directory}\\", "")));
                    lbFileNames.SelectedIndex = 0;
                    lbFileNames.Focus();

                    string jsonPath = Path.Combine(directory, "event.json");
                    if (File.Exists(jsonPath))
                    {
                        ReadEventJson(jsonPath);
                    }
                }
            }
        }

        private void GoToLastClip()
        {
            if (lbFileNames.SelectedIndex > 0)
                lbFileNames.SelectedIndex--;
            else if (cmbFolderList.SelectedIndex > 0)
            {
                cmbFolderList.SelectedIndex--;
                lbFileNames.SelectedIndex = lbFileNames.Items.Count - 1;
            }
        }

        private void GoToNextClip()
        {
            if (lbFileNames.SelectedIndex < lbFileNames.Items.Count - 1)
                lbFileNames.SelectedIndex++;
            else if (cmbFolderList.SelectedIndex < cmbFolderList.Items.Count - 1)
            {
                cmbFolderList.SelectedIndex++;
                lbFileNames.SelectedIndex = 0;
            }
        }

        private void ReadEventJson(string JsonPath)
        {
            string json = File.ReadAllText(JsonPath);
            string content = "";
            if (!string.IsNullOrWhiteSpace(json))
            {
                TeslaCamEventInfo? info = JsonSerializer.Deserialize<TeslaCamEventInfo>(json, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                if (info != null)
                    content = info.ToString();
            }
            lbl_eventinfo.Content = content;
        }
    }
}
