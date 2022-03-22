using System;
using System.Text;
using System.Text.Json;

namespace WpfTeslaCamViewer
{
    enum TeslaCamReason { Unknown = -1, UserDashcam, UserHonk, SentryObjectDetection, SentryAccelerometer }

    enum TeslaCamCamera { 
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

    class TeslaCamEventInfo
    {
        DateTime Timestamp;
        string? City;
        double Latitude;
        double Longitude;
        TeslaCamReason Reason;
        TeslaCamCamera Camera;

        private TeslaCamEventInfo() { }

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

        private static TeslaCamReason ConvertStringToReason(string reason)
        {
            //Ew! But how to do it better without checking for the exact reason? I'd like to use .Contains
            if (reason.Contains("user_interaction_dashcam"))
                return TeslaCamReason.UserDashcam;
            else if (reason.Contains("user_interaction_honk"))
                return TeslaCamReason.UserHonk;
            else if (reason.Contains("sentry_aware_object_detection"))
                return TeslaCamReason.SentryObjectDetection;
            else if (reason.Contains("sentry_aware_accel"))
                return TeslaCamReason.SentryAccelerometer;
            else
                return TeslaCamReason.Unknown;
        }

        public static TeslaCamEventInfo? FromJSONString(string json)
        {
            TeslaCamEventInfo info = new();
            try
            {
                JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                info.Timestamp = Convert.ToDateTime(root.GetProperty("timestamp").ToString());
                info.City = root.GetProperty("city").ToString();
                info.Latitude = Convert.ToDouble(root.GetProperty("est_lat").ToString());
                info.Longitude = Convert.ToDouble(root.GetProperty("est_lon").ToString());
                info.Reason = ConvertStringToReason(root.GetProperty("reason").ToString());
                info.Camera = (TeslaCamCamera)Convert.ToInt32(root.GetProperty("camera").ToString());
            }
            catch { /* Still return the object at this point? */ }

            return info;
        }
    }
}
