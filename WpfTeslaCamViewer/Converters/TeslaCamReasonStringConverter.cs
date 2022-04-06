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
        if (reason.Contains(Constants.UserInteractionDashcamPanelSave))
            return TeslaCamReason.UserInteractionDashcamPanelSave;
        if (reason.Contains(Constants.UserInteractionDashcamIconTapped))
            return TeslaCamReason.UserInteractionDashcamIconTapped;
        if (reason.Contains(Constants.UserInteractionHonk))
            return TeslaCamReason.UserInteractionHonk;
        if (reason.Contains(Constants.SentryAwareObjectDetection))
            return TeslaCamReason.SentryAwareObjectDetection;
        if (reason.Contains(Constants.SentryAwareAccel))
            return TeslaCamReason.SentryAccelerometer;


        return TeslaCamReason.Unknown;
    }

    public override void Write(Utf8JsonWriter writer, TeslaCamReason value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case TeslaCamReason.SentryAccelerometer:
                writer.WriteStringValue(Constants.SentryAwareAccel);
                break;
            case TeslaCamReason.UserInteractionDashcamIconTapped:
                writer.WriteStringValue(Constants.UserInteractionDashcamIconTapped);
                break;
            case TeslaCamReason.UserInteractionHonk:
                writer.WriteStringValue(Constants.UserInteractionHonk);
                break;
            case TeslaCamReason.UserInteractionDashcamPanelSave:
                writer.WriteStringValue(Constants.UserInteractionDashcamPanelSave);
                break;
            case TeslaCamReason.SentryAwareObjectDetection:
                writer.WriteStringValue(Constants.SentryAwareObjectDetection);
                break;
            case TeslaCamReason.Unknown:
                writer.WriteStringValue("unknown");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }
}
