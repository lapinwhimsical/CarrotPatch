using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsOverlay
{
    private static readonly Vector4 MarkerColor = new(1f, 0.9f, 0.1f, 1f);
    private static readonly Vector4 TargetingColor = new(1f, 0.15f, 0.1f, 1f);
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

        var markerPosition = gameObject.Position + new Vector3(0f, 2.25f, 0f);
        if (!this.gameGui.WorldToScreen(markerPosition, out var screenPosition, out var inView) || !inView)
            return false;

        if (!IsSaneScreenPosition(screenPosition))
            return false;

        var drawList = ImGui.GetForegroundDrawList();
        var statusLine = beacon.IsTargeting
            ? "TARGETING"
            : "Tell";
        var detailLine = beacon.IsTargeting
            ? $"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}"
            : $"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}  {beacon.SecondsRemaining}s";
        var lines = new (string Text, Vector4 Color)[]
        {
            (statusLine, beacon.IsTargeting ? TargetingColor : MarkerColor),
            (beacon.SenderName, MarkerColor),
            (detailLine, MarkerColor),
        };

        var maxWidth = 0f;
        var totalHeight = 0f;
        foreach (var line in lines)
        {
            var size = ImGui.CalcTextSize(line.Text);
            maxWidth = MathF.Max(maxWidth, size.X);
            totalHeight += size.Y;
        }

        var padding = new Vector2(8f, 5f);
        var boxMin = new Vector2(screenPosition.X - (maxWidth / 2f) - padding.X, screenPosition.Y - totalHeight - 34f);
        var boxMax = new Vector2(screenPosition.X + (maxWidth / 2f) + padding.X, boxMin.Y + totalHeight + (padding.Y * 2f));
        var center = new Vector2(screenPosition.X, boxMax.Y + 9f);
        var color = ImGui.GetColorU32(MarkerColor);
        var shadow = ImGui.GetColorU32(ShadowColor);

        drawList.AddRectFilled(boxMin, boxMax, shadow, 4f);
        drawList.AddRect(boxMin, boxMax, color, 4f, ImDrawFlags.None, 1.5f);
        drawList.AddTriangleFilled(
            new Vector2(center.X, center.Y),
            new Vector2(center.X - 7f, boxMax.Y),
            new Vector2(center.X + 7f, boxMax.Y),
            color);

        var cursor = boxMin + padding;
        foreach (var line in lines)
        {
            var size = ImGui.CalcTextSize(line.Text);
            drawList.AddText(
                new Vector2(screenPosition.X - (size.X / 2f), cursor.Y),
                ImGui.GetColorU32(line.Color),
                line.Text);
            cursor.Y += size.Y;
        }

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

        foreach (var beacon in this.rabbitEarsService.ActiveBeacons)
        {
            ImGui.TextColored(beacon.IsTargeting ? TargetingColor : MarkerColor, beacon.IsTargeting ? "TARGETING" : "Tell");
            ImGui.SameLine();
            ImGui.TextColored(MarkerColor, beacon.SenderName);
            ImGui.SameLine();
            ImGui.TextDisabled(beacon.IsTargeting
                ? $"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}"
                : $"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}  {beacon.SecondsRemaining}s");
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
