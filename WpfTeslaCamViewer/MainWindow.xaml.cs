using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using LibVLCSharp.Shared;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace WpfTeslaCamViewer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly LibVLC libVlc;
    private readonly Timer playerInfoTimer;
    private float playbackSpeed;
    private TeslaCamPlayer? player;
    private MediaPlayer? playerFront, playerLeft, playerRight, playerBack;

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

            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                lbl_DebugInfo.Content = info;
                slider_progress.Value = position;
            });
        };
        playerInfoTimer.Start();
    }

    public ObservableCollection<string> FolderNames { get; set; } = new();
    public HashSet<string> FileNames { get; set; } = new();

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

        player = new TeslaCamPlayer(libVlc, playerFront, playerLeft, playerRight, playerBack,
            () => Application.Current.Dispatcher.BeginInvoke(GoToNextClip).GetAwaiter().GetResult());

        UpdateWindowTitle("No directory opened");
    }

    private void btn_SlowDown_Click(object sender, RoutedEventArgs e)
    {
        if (playbackSpeed <= 0.1f) return;

        playbackSpeed -= 0.1f;
        SetPlaybackRate();
    }

    private void btn_SlowDownMore_Click(object sender, RoutedEventArgs e)
    {
        if (playbackSpeed <= 0.1f) return;

        playbackSpeed -= 0.5f;
        SetPlaybackRate();
    }

    private void btn_SpeedUp_Click(object sender, RoutedEventArgs e)
    {
        playbackSpeed += 0.1f;
        SetPlaybackRate();
    }

    private void btn_SpeedUpMore_Click(object sender, RoutedEventArgs e)
    {
        playbackSpeed += 0.5f;
        SetPlaybackRate();
    }

    private void slider_progress_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        playerInfoTimer.Stop();
    }

    private void slider_progress_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        player?.SetPosition((float) slider_progress.Value);
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

    private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
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
                MessageBox.Show(
                    "No subdirectories found!\nPlease select the folder containing the clip-subdirectories, for example the \"SavedClips\" directory.",
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

    private void SetPlaybackRate()
    {
        lbl_playbackspeed.Content = $"{Math.Round(playbackSpeed, 1)}x";
        playerFront?.SetRate(playbackSpeed);
        playerLeft?.SetRate(playbackSpeed);
        playerRight?.SetRate(playbackSpeed);
        playerBack?.SetRate(playbackSpeed);
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

        var selectedFile =
            FileNames.FirstOrDefault(x => x.Contains(lbFileNames.SelectedValue?.ToString() ?? string.Empty));

        if (player != null && !string.IsNullOrWhiteSpace(selectedFile))
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

                var jsonPath = Path.Combine(directory, "event.json");
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
        {
            lbFileNames.SelectedIndex--;
        }
        else if (cmbFolderList.SelectedIndex > 0)
        {
            cmbFolderList.SelectedIndex--;
            lbFileNames.SelectedIndex = lbFileNames.Items.Count - 1;
        }
    }

    private void GoToNextClip()
    {
        if (lbFileNames.SelectedIndex < lbFileNames.Items.Count - 1)
        {
            lbFileNames.SelectedIndex++;
        }
        else if (cmbFolderList.SelectedIndex < cmbFolderList.Items.Count - 1)
        {
            cmbFolderList.SelectedIndex++;
            lbFileNames.SelectedIndex = 0;
        }
    }

    private void ReadEventJson(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var content = "";
        if (!string.IsNullOrWhiteSpace(json))
        {
            var info = json.Deserialize<TeslaCamEventInfo>();
            if (info != null)
            {
                content = info.ToString();
            }
        }

        lbl_eventinfo.Content = content;
    }
}