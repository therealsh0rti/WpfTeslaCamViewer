using System.Text.Json;
using System.Text.Json.Serialization;
using WpfTeslaCamViewer.Converters;

namespace WpfTeslaCamViewer;

public static class SerializationExtensions
{
    private static readonly JsonSerializerOptions? DefaultOptions = new()
    {
        Converters =
        {
            // Order is important here - the Reason converter needs to go 
            // before any other generic Enum converters
            new TeslaCamReasonStringConverter(),
            new TeslaCamCameraConverter(),
            new JsonStringEnumConverter(),
            new CustomDateTimeConverter()
        },
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static T? Deserialize<T>(this string json) => 
        JsonSerializer.Deserialize<T>(json, DefaultOptions);

    public static T? Deserialize<T>(this string json, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<T>(json, options);

    public static string Serialize<T>(this T? value) =>
        JsonSerializer.Serialize(value, DefaultOptions);

    public static string Serialize<T>(this T? value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(value, options);
}
