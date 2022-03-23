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

        private static string ReasonToPrettyString(TeslaCamReason reason)
        {
            return reason switch
            {
                TeslaCamReason.User_Interaction_Dashcam_Icon_Tapped => "Dashcam (Icon)",
                TeslaCamReason.User_Interaction_Honk => "Honk",
                TeslaCamReason.User_Interaction_Dashcam_Panel_Save => "Dashcam (Panel)",
                TeslaCamReason.Sentry_Aware_Object_Detection => "Sentry Mode (Object Detection)",
                TeslaCamReason.SentryAccelerometer => "Sentry Mode (Accelerometer)",
                _ => "Unknown",
            };
        }

        private static string CameraToPrettyString(TeslaCamCamera camera)
        {
            return camera switch
            {
                TeslaCamCamera.FrontMain => "Front",
                TeslaCamCamera.FrontWide => "Front Wide (Not recorded)",
                TeslaCamCamera.FrontNarrow => "Front Narrow (Not recorded)",
                TeslaCamCamera.LeftRepeater => "Left Repeater",
                TeslaCamCamera.RightRepeater => "Right Repeater",
                TeslaCamCamera.LeftPillar => "Right Pillar (Not recorded)",
                TeslaCamCamera.RightPillar => "Right Pillar (Not recorded)",
                TeslaCamCamera.Rear => "Rear",
                TeslaCamCamera.Cabin => "Cabin (Not recorded)",
                _ => "Unknown",
            };
        }

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
            sb.Append(ReasonToPrettyString(this.Reason));
            sb.AppendLine();

            sb.Append("Camera: ");
            sb.Append(CameraToPrettyString(this.Camera));

            return sb.ToString();
        }
    }
}
