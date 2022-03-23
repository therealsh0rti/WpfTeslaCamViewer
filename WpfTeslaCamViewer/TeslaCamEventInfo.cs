using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfTeslaCamViewer
{
    public enum TeslaCamReason
    {
        Unknown = -1,
        User_Interaction_Dashcam_Icon_Tapped,
        User_Interaction_Honk,
        User_Interaction_Dashcam_Panel_Save,
        Sentry_Aware_Object_Detection,
        SentryAccelerometer
    }

    public enum TeslaCamCamera { 
        //Source: teslamotorsclub.com - Feel free to correct
        FrontMain = 0, 
        FrontWide = 1, 
        FrontNarrow = 2, 
        LeftRepeater = 3, 
        RightRepeater = 4, 
        LeftPillar = 5, 
        RightPillar = 6,
        Rear = 7,
        Cabin = 8,
    }

    public class TeslaCamEventInfo
    {
        public DateTime Timestamp { get; set; }
        public string? City { get; set; }
        [JsonPropertyName("est_lat")]
        public float Latitude { get; set; }
        [JsonPropertyName("est_lon")]
        public float Longitude { get; set; }
        public TeslaCamReason Reason { get; set; }
        public TeslaCamCamera Camera { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.Append("Timestamp: ");
            sb.Append(this.Timestamp);
            sb.AppendLine();

            sb.Append("City: ");
            sb.AppendLine(this.City);

            sb.Append("Position: ");
            sb.Append(this.Latitude);
            sb.Append('/');
            sb.Append(this.Longitude);
            sb.AppendLine();

            sb.Append("Reason: ");
            sb.Append(this.Reason);
            sb.AppendLine();

            sb.Append("Camera: ");
            sb.Append(this.Camera);

            return sb.ToString();
        }

        public static TeslaCamEventInfo? FromJSONString(string json)
        {
            TeslaCamEventInfo? info = new();
            try
            {
                info = JsonSerializer.Deserialize<TeslaCamEventInfo>(json, new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
            }
            catch { /* Still return the object at this point? */ }

            return info;
        }
    }
}
