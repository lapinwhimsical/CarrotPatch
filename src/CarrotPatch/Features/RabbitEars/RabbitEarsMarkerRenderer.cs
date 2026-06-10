using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;

namespace CarrotPatch.Features.RabbitEars;

public static class RabbitEarsMarkerRenderer
{
    private static readonly Vector4 MarkerColor = new(1f, 0.9f, 0.1f, 1f);
    private static readonly Vector4 TargetingColor = new(1f, 0.15f, 0.1f, 1f);
    private static readonly Vector4 DisabledColor = new(0.55f, 0.55f, 0.55f, 0.85f);
    private static readonly Vector4 ShadowColor = new(0f, 0f, 0f, 1f);

    public static void Draw(
        ImDrawListPtr drawList,
        Vector2 screenPosition,
        string senderName,
        string detailLine,
        bool isTargeting,
        bool hasTell,
        bool isManualMarker,
        float scale,
        float backgroundOpacity,
        float foregroundOpacity)
    {
        const string TargetingLabel = "TARGETING";
        const string TellLabel = "TELL";
        const string MarkerLabel = "MARKER";
        const string StatusGap = "  ";
        var statusLine = $"{TargetingLabel}{StatusGap}{TellLabel}{StatusGap}{MarkerLabel}";
        var lines = new[]
        {
            statusLine,
            senderName,
            detailLine,
        };

        var maxWidth = 0f;
        var totalHeight = 0f;
        foreach (var line in lines)
        {
            var size = ImGui.CalcTextSize(line);
            maxWidth = MathF.Max(maxWidth, size.X);
            totalHeight += size.Y;
        }

        var clampedScale = RabbitEarsOptions.ClampMarkerScale(scale);
        var padding = new Vector2(8f, 5f) * clampedScale;
        var boxMin = new Vector2(screenPosition.X - (maxWidth / 2f) - padding.X, screenPosition.Y - totalHeight - (34f * clampedScale));
        var boxMax = new Vector2(screenPosition.X + (maxWidth / 2f) + padding.X, boxMin.Y + totalHeight + (padding.Y * 2f));
        var center = new Vector2(screenPosition.X, boxMax.Y + (9f * clampedScale));
        var clampedForegroundOpacity = RabbitEarsOptions.ClampOverheadForegroundOpacity(foregroundOpacity);
        var color = ImGui.GetColorU32(WithAlpha(MarkerColor, clampedForegroundOpacity));
        var targetingColor = ImGui.GetColorU32(WithAlpha(TargetingColor, clampedForegroundOpacity));
        var disabledColor = ImGui.GetColorU32(WithAlpha(DisabledColor, DisabledColor.W * clampedForegroundOpacity));
        var shadow = ImGui.GetColorU32(WithAlpha(ShadowColor, RabbitEarsOptions.ClampOverheadBackgroundOpacity(backgroundOpacity)));
        var rounding = 4f * clampedScale;

        drawList.AddRectFilled(boxMin, boxMax, shadow, rounding);
        drawList.AddRect(boxMin, boxMax, color, rounding, ImDrawFlags.None, 1.5f * clampedScale);
        drawList.AddTriangleFilled(
            new Vector2(center.X, center.Y),
            new Vector2(center.X - (7f * clampedScale), boxMax.Y),
            new Vector2(center.X + (7f * clampedScale), boxMax.Y),
            color);

        var cursor = boxMin + padding;
        var statusSize = ImGui.CalcTextSize(statusLine);
        var targetingSize = ImGui.CalcTextSize(TargetingLabel);
        var gapSize = ImGui.CalcTextSize(StatusGap);
        var tellSize = ImGui.CalcTextSize(TellLabel);
        var statusStart = new Vector2(screenPosition.X - (statusSize.X / 2f), cursor.Y);
        drawList.AddText(
            statusStart,
            isTargeting ? targetingColor : disabledColor,
            TargetingLabel);
        drawList.AddText(
            new Vector2(statusStart.X + targetingSize.X + gapSize.X, cursor.Y),
            hasTell ? targetingColor : disabledColor,
            TellLabel);
        drawList.AddText(
            new Vector2(statusStart.X + targetingSize.X + gapSize.X + tellSize.X + gapSize.X, cursor.Y),
            isManualMarker ? targetingColor : disabledColor,
            MarkerLabel);
        cursor.Y += statusSize.Y;

        var senderSize = ImGui.CalcTextSize(senderName);
        drawList.AddText(
            new Vector2(screenPosition.X - (senderSize.X / 2f), cursor.Y),
            color,
            senderName);
        cursor.Y += senderSize.Y;

        var detailSize = ImGui.CalcTextSize(detailLine);
        drawList.AddText(
            new Vector2(screenPosition.X - (detailSize.X / 2f), cursor.Y),
            color,
            detailLine);
    }

    private static Vector4 WithAlpha(Vector4 color, float alpha)
        => new(color.X, color.Y, color.Z, alpha);
}
