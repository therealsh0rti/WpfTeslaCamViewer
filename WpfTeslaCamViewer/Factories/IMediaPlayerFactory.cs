using LibVLCSharp.Shared;

namespace WpfTeslaCamViewer.Factories;

public interface IMediaPlayerFactory
{
    MediaPlayer GetNewMediaPlayer();
}
