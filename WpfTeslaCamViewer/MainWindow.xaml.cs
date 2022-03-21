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
        TeslaCamPlayer player;

        public MainWindow()
        {
            InitializeComponent();
            player = null;
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
            player.Play(@"C:\Users\sh0rt\Videos\TeslaCam\2022-03-19_18-37-36");
        }
    }
}
