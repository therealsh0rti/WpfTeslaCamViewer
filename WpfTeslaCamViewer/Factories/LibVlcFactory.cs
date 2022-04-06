using LibVLCSharp.Shared;

namespace WpfTeslaCamViewer.Factories;

public class LibVlcFactory : ILibVlcFactory
{
    private readonly LibVLC instance;

    public LibVlcFactory()
    {
        Core.Initialize();
        instance = new LibVLC();
    }

    public LibVLC GetLibVlcInstance()
    {
        return instance;
    }
}
