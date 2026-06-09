using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsOverlay
{
    private static readonly Vector4 MarkerColor = new(1f, 0.9f, 0.1f, 1f);
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

        var drewEveryWorldMarker = true;
        if (this.configuration.ShowOverlayMarker)
        {
            foreach (var beacon in this.rabbitEarsService.ActiveBeacons)
            {
                drewEveryWorldMarker &= this.TryDrawWorldMarker(beacon);
            }
        }

        if (!drewEveryWorldMarker || !this.configuration.ShowOverlayMarker)
        {
            this.DrawFallbackWindow();
        }
    }

    private bool TryDrawWorldMarker(ActiveBeacon beacon)
    {
        var gameObject = this.objectTable.SearchById(beacon.GameObjectId);
        if (gameObject is null)
            return false;

        var markerPosition = gameObject.Position + new Vector3(0f, 2.25f, 0f);
        if (!this.gameGui.WorldToScreen(markerPosition, out var screenPosition, out var inView) || !inView)
            return false;

        var drawList = ImGui.GetForegroundDrawList();
        var lines = new[]
        {
            "Tell",
            beacon.SenderName,
            $"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}  {beacon.SecondsRemaining}s",
        };

        var maxWidth = 0f;
        var totalHeight = 0f;
        foreach (var line in lines)
        {
            var size = ImGui.CalcTextSize(line);
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
            var size = ImGui.CalcTextSize(line);
            drawList.AddText(new Vector2(screenPosition.X - (size.X / 2f), cursor.Y), color, line);
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
            ImGui.TextColored(MarkerColor, $"Tell from {beacon.SenderName}");
            ImGui.SameLine();
            ImGui.TextDisabled($"{MathF.Round(beacon.Distance)}y {beacon.DirectionText}  {beacon.SecondsRemaining}s");
        }

        ImGui.End();
    }
}
