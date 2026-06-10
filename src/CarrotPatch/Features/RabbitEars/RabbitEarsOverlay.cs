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
        var detailLine = beacon.IsTargeting
            ? $"{MathF.Round(beacon.Distance)}y"
            : $"{MathF.Round(beacon.Distance)}y  {beacon.SecondsRemaining}s";
        RabbitEarsMarkerRenderer.Draw(
            drawList,
            screenPosition,
            beacon.SenderName,
            detailLine,
            beacon.IsTargeting,
            beacon.HasTell,
            beacon.IsManualMarker,
            scale,
            this.configuration.OverheadBackgroundOpacity,
            this.configuration.OverheadForegroundOpacity);

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
