using LibVLCSharp.Shared;

namespace WpfTeslaCamViewer.Factories;

public class MediaPlayerFactory : IMediaPlayerFactory
{
    private readonly LibVLC libVlc;

    public MediaPlayerFactory(ILibVlcFactory libVlcFactory)
    {
        libVlc = libVlcFactory.GetLibVlcInstance();
    }

    public MediaPlayer GetNewMediaPlayer() => new(libVlc);
}