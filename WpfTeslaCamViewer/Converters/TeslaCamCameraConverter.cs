using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfTeslaCamViewer.Converters;

public class TeslaCamCameraConverter : JsonConverter<TeslaCamCamera>
{
    public override TeslaCamCamera Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var camera = Convert.ToInt64(reader.GetString());

        switch (camera)
        {
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
                return (TeslaCamCamera) camera;
            default:
                return TeslaCamCamera.Unknown;
        }
    }

    public override void Write(Utf8JsonWriter writer, TeslaCamCamera value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((int) value).ToString());
    }
}