using LibVLCSharp.Shared;

namespace WpfTeslaCamViewer.Factories;

public interface ILibVlcFactory
{
    LibVLC GetLibVlcInstance();
}
