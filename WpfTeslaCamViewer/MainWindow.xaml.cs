using System.Windows;
using WpfTeslaCamViewer.ViewModels;

namespace WpfTeslaCamViewer;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel mainViewModel;

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();

        this.mainViewModel = mainViewModel;
        DataContext = mainViewModel;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var playerGroup = new PlayerGroup(videoViewFront, videoViewLeftRepeater, videoViewRightRepeater, videoViewRear);

        mainViewModel.OnLoad(playerGroup);

        UpdateWindowTitle("No directory opened");
    }

    private void UpdateWindowTitle(string Path)
    {
        mainWindow.Title = "WpfTeslaCamViewer - " + Path;
    }
}
