using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfTeslaCamViewer.Converters;

public class TeslaCamReasonStringConverter : JsonConverter<TeslaCamReason>
{
    public override TeslaCamReason Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var reason = reader.GetString()!;

        // ol' trusty
        if (reason.Contains("user_interaction_dashcam_panel_save"))
            return TeslaCamReason.UserInteractionDashcamPanelSave;
        if (reason.Contains("user_interaction_dashcam_icon_tapped"))
            return TeslaCamReason.UserInteractionDashcamIconTapped;
        if (reason.Contains("user_interaction_honk"))
            return TeslaCamReason.UserInteractionHonk;
        if (reason.Contains("sentry_aware_object_detection"))
            return TeslaCamReason.SentryAwareObjectDetection;
        if (reason.Contains("sentry_aware_accel"))
            return TeslaCamReason.SentryAccelerometer;


        return TeslaCamReason.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, TeslaCamReason value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case TeslaCamReason.SentryAccelerometer:
                writer.WriteStringValue("sentry_aware_accel");
                break;
            case TeslaCamReason.UserInteractionDashcamIconTapped:
                writer.WriteStringValue("user_interaction_dashcam_icon_tapped");
                break;
            case TeslaCamReason.UserInteractionHonk:
                writer.WriteStringValue("user_interaction_honk");
                break;
            case TeslaCamReason.UserInteractionDashcamPanelSave:
                writer.WriteStringValue("user_interaction_dashcam_panel_save");
                break;
            case TeslaCamReason.SentryAwareObjectDetection:
                writer.WriteStringValue("sentry_aware_object_detection");
                break;
            case TeslaCamReason.Unknown:
                writer.WriteStringValue("unknown");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}