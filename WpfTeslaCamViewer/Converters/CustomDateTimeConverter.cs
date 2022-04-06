using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfTeslaCamViewer.Converters
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTimeStr = reader.GetString();
            return string.IsNullOrWhiteSpace(dateTimeStr) ? DateTime.MinValue : DateTime.Parse(dateTimeStr);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
