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
        LibVLCSharp.Shared.MediaPlayer? _mediaPlayer, _mediaPlayer2, _mediaPlayer3, _mediaPlayer4;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _mediaPlayer2 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _mediaPlayer3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _mediaPlayer4 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

            videoViewFront.MediaPlayer = _mediaPlayer;
            videoViewLeftRepeater.MediaPlayer = _mediaPlayer2;
            videoViewRightRepeater.MediaPlayer = _mediaPlayer3;
            videoViewRear.MediaPlayer = _mediaPlayer4;

            _mediaPlayer.Play(new Media(_libVLC, new Uri(@"C:\Users\sh0rt\Videos\TeslaCam\2022-03-19_17-56-03\2022-03-19_17-44-54-front.mp4")));
            _mediaPlayer2.Play(new Media(_libVLC, new Uri(@"C:\Users\sh0rt\Videos\TeslaCam\2022-03-19_17-56-03\2022-03-19_17-44-54-left_repeater.mp4")));
            _mediaPlayer3.Play(new Media(_libVLC, new Uri(@"C:\Users\sh0rt\Videos\TeslaCam\2022-03-19_17-56-03\2022-03-19_17-44-54-right_repeater.mp4")));
            _mediaPlayer4.Play(new Media(_libVLC, new Uri(@"C:\Users\sh0rt\Videos\TeslaCam\2022-03-19_17-56-03\2022-03-19_17-44-54-back.mp4")));
        }
    }
}
