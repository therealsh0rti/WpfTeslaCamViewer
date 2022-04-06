using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using WpfTeslaCamViewer.Annotations;
using WpfTeslaCamViewer.Factories;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace WpfTeslaCamViewer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private const int JumpBackTimeInSeconds = 3;
    private readonly LibVLC libVlc;
    private readonly IMediaPlayerFactory mediaPlayerFactory;
    private readonly Timer playerInfoTimer;
    private string debugInfo;
    private string eventInfo;
    private ObservableCollection<string> fileNames = new();
    private ObservableCollection<string> folderNames = new();
    private float playbackSpeed = 1f;

    private PlayerGroup playerGroup;
    private int selectedFileIndex;
    private int selectedFolderIndex;
    private float sliderProgress;

    public MainViewModel(IMediaPlayerFactory mediaPlayerFactory, ILibVlcFactory libVlcFactory)
    {
        this.mediaPlayerFactory = mediaPlayerFactory;
        libVlc = libVlcFactory.GetLibVlcInstance();

        playerInfoTimer = new Timer(500);
        playerInfoTimer.Elapsed += (_, _) =>
        {
            if (playerGroup == null) return;

            OnPropertyChanged(nameof(SliderProgress));
            DebugInfo = GetDebugInfo();
        };
        playerInfoTimer.Start();
    }

    public Command SlowDownCommand => new(param =>
    {
        switch (param)
        {
            case null:
                PlaybackSpeed -= 0.1f;
                break;
            case string amountStr:
                float.TryParse(amountStr, out var amount);

                if (amount <= 0)
                {
                    PlaybackSpeed -= 0.1f;
                }
                else
                {
                    PlaybackSpeed -= amount;
                }

                break;
        }

        return Task.CompletedTask;
    });

    public Command SpeedUpCommand => new(param =>
    {
        switch (param)
        {
            case null:
                PlaybackSpeed += 0.1f;
                break;
            case string amountStr:
                float.TryParse(amountStr, out var amount);

                if (amount <= 0)
                {
                    PlaybackSpeed += 0.1f;
                }
                else
                {
                    PlaybackSpeed += amount;
                }

                break;
        }

        return Task.CompletedTask;
    });

    public Command JumpToEventCommand => new(async (_) => await JumpToEvent());

    public Command PlayPauseCommand => new(_ =>
    {
        TogglePlayPause();
        return Task.CompletedTask;
    });

    public Command NextClipCommand => new(_ =>
    {
        GoToNextClip();
        return Task.CompletedTask;
    });

    public Command PreviousClipCommand => new(_ =>
    {
        GoToPreviousClip();
        return Task.CompletedTask;
    });

    public Command OpenFolderCommand => new(async _ => await OpenFolder());

    public float PlaybackSpeed
    {
        get => playbackSpeed;
        set
        {
            if (playbackSpeed <= 0.1f) return;

            playbackSpeed = value;
            foreach (var player in playerGroup)
            {
                player.SetRate(playbackSpeed);
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(PlaybackSpeedStr));
        }
    }

    public string PlaybackSpeedStr => $"{PlaybackSpeed:N2}x";

    public string DebugInfo
    {
        get => GetDebugInfo();
        set
        {
            if (debugInfo == value) return;
            debugInfo = value;
            OnPropertyChanged();
        }
    }

    public float SliderProgress
    {
        get => playerGroup?.FirstOrDefault()?.Position ?? 0;
        set
        {
            if (Math.Abs(sliderProgress - value) < 0.01f) return;

            foreach (var player in playerGroup)
            {
                player.Position = value;
            }

            OnPropertyChanged();
        }
    }

    public int SelectedFolderIndex
    {
        get => selectedFolderIndex;
        set
        {
            selectedFolderIndex = value;
            OnFolderListSelectionChanged();
            OnPropertyChanged();
        }
    }

    public int SelectedFileIndex
    {
        get => selectedFileIndex;
        set
        {
            selectedFileIndex = value;
            OnFileListSelectionChanged();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> FolderNames
    {
        get => folderNames;
        set
        {
            folderNames = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<string> FileNames
    {
        get => fileNames;
        set
        {
            fileNames = value;
            OnPropertyChanged();
        }
    }

    public string EventInfo
    {
        get => eventInfo;
        set
        {
            eventInfo = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnLoad(PlayerGroup playerGroup)
    {
        this.playerGroup =
            playerGroup.Initialize(() => mediaPlayerFactory.GetNewMediaPlayer());

        playerGroup.PlayerFront.EndReached += async (_, _) =>
            await Application.Current.Dispatcher.BeginInvoke(() => GoToNextClip());
    }

    private string GetDebugInfo()
    {
        if (playerGroup == null) return string.Empty;

        var lengthInSeconds = TimeSpan.FromMilliseconds(playerGroup.PlayerFront.Length);
        var positionInSeconds = TimeSpan.FromSeconds(Math.Round(
            playerGroup.PlayerFront.Position * playerGroup.PlayerFront.Length / 1000,
            MidpointRounding.AwayFromZero));

        if (lengthInSeconds.Seconds > 0 && positionInSeconds.Seconds > 0)
        {
            return $"{positionInSeconds:mm':'ss} / {lengthInSeconds:mm':'ss}";
        }

        return string.Empty;
    }

    private async Task OpenFolder()
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = "Select the folder containing the clip folders (e.g. SavedClips)";
        dialog.UseDescriptionForTitle = true;
        dialog.ShowNewFolderButton = false;
        var result = dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            StopAllPlayers();

            //Don't include files here
            var entries = Directory.GetDirectories(dialog.SelectedPath).ToList();

            await Application.Current.Dispatcher.BeginInvoke(() =>
            {
                FolderNames.Clear();
                FileNames.Clear();
            });


            if (entries.Count == 0)
            {
                MessageBox.Show(
                    "No subdirectories found!\nPlease select the folder containing the clip-subdirectories, for example the \"SavedClips\" directory.",
                    "Invalid folder selected", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                foreach (var dir in entries.Where(Directory.Exists))
                {
                    if (!FolderNames.Contains(dir))
                    {
                        FolderNames.Add(dir);
                    }
                }

                SelectedFolderIndex = 0;
                SelectedFileIndex = 0;
            }
        }
    }

    private void OnFolderListSelectionChanged()
    {
        if (FolderNames.Count == 0 || SelectedFolderIndex == -1)
        {
            //Selected an invalid folder as TeslaCam folder maybe
            StopAllPlayers();
        }
        else
        {
            var directory = FolderNames[SelectedFolderIndex];

            FileNames.Clear();
            var files = Directory.GetFiles(directory, "*.mp4");
            if (files.Length == 0)
            {
                MessageBox.Show("No video files found in selected folder!\nMake sure you selected the correct folder.",
                    "Nothing to show", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                foreach (var file in files)
                {
                    var nameWithoutSuffix = file.Replace("-front", "")
                        .Replace("-back", "")
                        .Replace("-left_repeater", "")
                        .Replace("-right_repeater", "")
                        .Replace(".mp4", "");

                    if (!FileNames.Contains(nameWithoutSuffix))
                    {
                        FileNames.Add(nameWithoutSuffix);
                    }
                }

                SelectedFileIndex = 0;

                var jsonPath = Path.Combine(directory, "event.json");
                if (File.Exists(jsonPath))
                {
                    ReadEventJson(jsonPath);
                }
            }
        }
    }

    private void OnFileListSelectionChanged()
    {
        PlaySelectedFile();
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

        EventInfo = content;
    }

    private void GoToNextClip()
    {
        if (SelectedFileIndex < FileNames.Count - 1)
        {
            SelectedFileIndex++;
        }
        else if (SelectedFolderIndex < FolderNames.Count - 1)
        {
            SelectedFolderIndex++;
            SelectedFileIndex = 0;
        }
    }

    private void GoToPreviousClip()
    {
        if (SelectedFileIndex > 0)
        {
            SelectedFileIndex--;
        }
        else if (SelectedFolderIndex > 0)
        {
            SelectedFolderIndex--;
            SelectedFileIndex = FileNames.Count - 1;
        }
    }


    private async Task JumpToEvent()
    {
        var counter = 0;

        var jsonPath = Path.Combine(FolderNames[SelectedFolderIndex], "event.json");

        if (!File.Exists(jsonPath))
            return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var eventInfo = json.Deserialize<TeslaCamEventInfo>();

        // Need something to jump to
        if (eventInfo?.Timestamp == null ||
            eventInfo.Timestamp.HasValue && eventInfo.Timestamp.Value.Equals(DateTime.MinValue))
            return;

        var fileToPlay = FileNames.FirstOrDefault(x =>
            x.Split('\\').Last().Contains(eventInfo.Timestamp.Value.ToString("HH-mm")));

        if (string.IsNullOrWhiteSpace(fileToPlay))
        {
            // Search 1 minute back as well
            counter++;
            fileToPlay = FileNames.FirstOrDefault(x =>
                x.Split('\\').Last().Contains(eventInfo.Timestamp.Value.AddMinutes(-counter).ToString("HH-mm")));
        }

        // If it's still empty, give up
        if (string.IsNullOrWhiteSpace(fileToPlay)) return;

        // Figure out where to jump to
        var dateString = fileToPlay.Replace($"{FolderNames[SelectedFolderIndex]}\\", "");
        Debug.WriteLine(dateString);
        DateTimeOffset.TryParseExact(dateString, "yyyy-MM-dd_HH-mm-ss", null, DateTimeStyles.None,
            out var beginningOfClip);

        var startOfVideo = beginningOfClip.TimeOfDay;
        var eventStart = eventInfo.Timestamp.Value.TimeOfDay;

        // If the event is before the clip we selected, go back a clip and try again.
        if (eventStart < startOfVideo)
        {
            counter++;
            fileToPlay = FileNames.FirstOrDefault(x =>
                x.Split('\\').Last().Contains(eventInfo.Timestamp.Value.AddMinutes(-counter).ToString("HH-mm")));

            if (string.IsNullOrWhiteSpace(fileToPlay))
                return;

            dateString = fileToPlay.Replace($"{FolderNames[SelectedFolderIndex]}\\", "");
            Debug.WriteLine(dateString);
            DateTimeOffset.TryParseExact(dateString, "yyyy-MM-dd_HH-mm-ss", null, DateTimeStyles.None,
                out beginningOfClip);

            startOfVideo = beginningOfClip.TimeOfDay;
            eventStart = eventInfo.Timestamp.Value.TimeOfDay;
        }

        // Setting this index plays the file now via the property setter
        SelectedFileIndex = FileNames.IndexOf(fileToPlay);

        // Jump back {JumpBackTimeInSeconds} cuz the event timestamp is usually a little ahead
        var secondsToSkip = Math.Max((eventStart - startOfVideo).TotalSeconds - JumpBackTimeInSeconds, 0);
        Debug.WriteLine($"secondsToSkip: {secondsToSkip}");

        while (!playerGroup.PlayerFront.IsPlaying && playerGroup.PlayerFront.Length <= 0)
        {
            await Task.Delay(100);
        }

        var placeToSkipTo = (float) (secondsToSkip * 1000 / playerGroup.PlayerFront.Length);

        foreach (var player in playerGroup)
        {
            player.Position = placeToSkipTo;
        }
    }

    private void PlaySelectedFile(string? selectedFile = null)
    {
        if (FileNames.Count <= 0) return;

        selectedFile ??= FileNames[SelectedFileIndex];

        if (!string.IsNullOrWhiteSpace(selectedFile))
        {
            playerGroup.PlayerFront.Play(new Media(libVlc, new Uri($"{selectedFile}-front.mp4")));
            playerGroup.PlayerLeft.Play(new Media(libVlc, new Uri($"{selectedFile}-left_repeater.mp4")));
            playerGroup.PlayerRight.Play(new Media(libVlc, new Uri($"{selectedFile}-right_repeater.mp4")));
            playerGroup.PlayerBack.Play(new Media(libVlc, new Uri($"{selectedFile}-back.mp4")));
        }
    }

    private void TogglePlayPause()
    {
        foreach (var player in playerGroup)
        {
            if (player.IsPlaying)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }
    }

    private void StopAllPlayers()
    {
        foreach (var player in playerGroup)
        {
            player.Stop();
        }
    }

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
