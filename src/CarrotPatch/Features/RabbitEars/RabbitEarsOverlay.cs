using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsOverlay
{
    private static readonly Vector4 MarkerColor = new(1f, 0.9f, 0.1f, 1f);
    private static readonly Vector4 TargetingColor = new(1f, 0.15f, 0.1f, 1f);
    private static readonly Vector4 DisabledColor = new(0.55f, 0.55f, 0.55f, 0.85f);
    private static readonly Vector4 ShadowColor = new(0f, 0f, 0f, 0.85f);

    private readonly RabbitEarsService rabbitEarsService;
    private readonly IObjectTable objectTable;
    private readonly IGameGui gameGui;
    private readonly Configuration configuration;

    public RabbitEarsOverlay(
        RabbitEarsService rabbitEarsService,
        IObjectTable objectTable,
        IGameGui gameGui,
        Configuration configuration)
    {
        this.rabbitEarsService = rabbitEarsService;
        this.objectTable = objectTable;
        this.gameGui = gameGui;
        this.configuration = configuration;
    }

    public void Draw()
    {
        if (!this.configuration.RabbitEarsEnabled || this.rabbitEarsService.ActiveBeacons.Count == 0)
            return;

        if (this.configuration.ShowOverlayMarker)
        {
            foreach (var beacon in this.rabbitEarsService.ActiveBeacons)
            {
                this.TryDrawWorldMarker(beacon);
            }

            return;
        }

        this.DrawFallbackWindow();
    }

    private bool TryDrawWorldMarker(ActiveBeacon beacon)
    {
        var gameObject = this.objectTable.SearchById(beacon.GameObjectId);
        if (gameObject is null)
            return false;

        var scale = RabbitEarsOptions.ClampMarkerScale(this.configuration.MarkerScale);
        var markerPosition = gameObject.Position + new Vector3(0f, 2.25f * scale, 0f);
        if (!this.gameGui.WorldToScreen(markerPosition, out var screenPosition, out var inView) || !inView)
            return false;

        if (!IsSaneScreenPosition(screenPosition))
            return false;

        var drawList = ImGui.GetForegroundDrawList();
        const string TargetingLabel = "TARGETING";
        const string TellLabel = "TELL";
        const string MarkerLabel = "MARKER";
        const string StatusGap = "  ";
        var statusLine = $"{TargetingLabel}{StatusGap}{TellLabel}{StatusGap}{MarkerLabel}";
        var detailLine = beacon.IsTargeting
            ? $"{MathF.Round(beacon.Distance)}y"
            : $"{MathF.Round(beacon.Distance)}y  {beacon.SecondsRemaining}s";
        var lines = new[]
        {
            statusLine,
            beacon.SenderName,
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

        var padding = new Vector2(8f, 5f) * scale;
        var boxMin = new Vector2(screenPosition.X - (maxWidth / 2f) - padding.X, screenPosition.Y - totalHeight - (34f * scale));
        var boxMax = new Vector2(screenPosition.X + (maxWidth / 2f) + padding.X, boxMin.Y + totalHeight + (padding.Y * 2f));
        var center = new Vector2(screenPosition.X, boxMax.Y + (9f * scale));
        var color = ImGui.GetColorU32(MarkerColor);
        var shadow = ImGui.GetColorU32(ShadowColor);
        var rounding = 4f * scale;

        drawList.AddRectFilled(boxMin, boxMax, shadow, rounding);
        drawList.AddRect(boxMin, boxMax, color, rounding, ImDrawFlags.None, 1.5f * scale);
        drawList.AddTriangleFilled(
            new Vector2(center.X, center.Y),
            new Vector2(center.X - (7f * scale), boxMax.Y),
            new Vector2(center.X + (7f * scale), boxMax.Y),
            color);

        var cursor = boxMin + padding;
        var statusSize = ImGui.CalcTextSize(statusLine);
        var targetingSize = ImGui.CalcTextSize(TargetingLabel);
        var gapSize = ImGui.CalcTextSize(StatusGap);
        var tellSize = ImGui.CalcTextSize(TellLabel);
        var markerSize = ImGui.CalcTextSize(MarkerLabel);
        var statusStart = new Vector2(screenPosition.X - (statusSize.X / 2f), cursor.Y);
        drawList.AddText(
            statusStart,
            ImGui.GetColorU32(beacon.IsTargeting ? TargetingColor : DisabledColor),
            TargetingLabel);
        drawList.AddText(
            new Vector2(statusStart.X + targetingSize.X + gapSize.X, cursor.Y),
            ImGui.GetColorU32(beacon.HasTell ? TargetingColor : DisabledColor),
            TellLabel);
        drawList.AddText(
            new Vector2(statusStart.X + targetingSize.X + gapSize.X + tellSize.X + gapSize.X, cursor.Y),
            ImGui.GetColorU32(beacon.IsManualMarker ? TargetingColor : DisabledColor),
            MarkerLabel);
        cursor.Y += statusSize.Y;

        var senderSize = ImGui.CalcTextSize(beacon.SenderName);
        drawList.AddText(
            new Vector2(screenPosition.X - (senderSize.X / 2f), cursor.Y),
            ImGui.GetColorU32(MarkerColor),
            beacon.SenderName);
        cursor.Y += senderSize.Y;

        var detailSize = ImGui.CalcTextSize(detailLine);
        drawList.AddText(
            new Vector2(screenPosition.X - (detailSize.X / 2f), cursor.Y),
            ImGui.GetColorU32(MarkerColor),
            detailLine);

        return true;
    }

    private void DrawFallbackWindow()
    {
        ImGui.SetNextWindowPos(new Vector2(24f, 180f), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(260f, 0f), ImGuiCond.Always);

        var flags = ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoFocusOnAppearing
            | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.AlwaysAutoResize;

        if (!ImGui.Begin("Rabbit Ears##fallback", flags))
        {
            ImGui.End();
            return;
        }

        ImGui.SetWindowFontScale(RabbitEarsOptions.ClampMarkerScale(this.configuration.MarkerScale));
        foreach (var beacon in this.rabbitEarsService.ActiveBeacons)
        {
            ImGui.TextColored(beacon.IsTargeting ? TargetingColor : DisabledColor, "TARGETING");
            ImGui.SameLine();
            ImGui.TextColored(beacon.HasTell ? TargetingColor : DisabledColor, "TELL");
            ImGui.SameLine();
            ImGui.TextColored(beacon.IsManualMarker ? TargetingColor : DisabledColor, "MARKER");
            ImGui.SameLine();
            ImGui.TextColored(MarkerColor, beacon.SenderName);
            ImGui.SameLine();
            ImGui.TextDisabled(beacon.IsTargeting
                ? $"{MathF.Round(beacon.Distance)}y"
                : $"{MathF.Round(beacon.Distance)}y  {beacon.SecondsRemaining}s");
        }

        ImGui.End();
    }

    private static bool IsSaneScreenPosition(Vector2 screenPosition)
    {
        if (!float.IsFinite(screenPosition.X) || !float.IsFinite(screenPosition.Y))
            return false;

        var displaySize = ImGui.GetIO().DisplaySize;
        const float Margin = 64f;
        return screenPosition.X >= -Margin
            && screenPosition.Y >= -Margin
            && screenPosition.X <= displaySize.X + Margin
            && screenPosition.Y <= displaySize.Y + Margin;
    }
}
