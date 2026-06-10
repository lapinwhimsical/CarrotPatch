using System;
using CarrotPatch.Features.RabbitEars;
using Dalamud.Bindings.ImGui;
using System.Numerics;

namespace CarrotPatch.UI;

public sealed class SettingsWindow
{
    private readonly Configuration configuration;
    private readonly RabbitEarsService rabbitEarsService;
    private readonly NotificationSoundPlayer notificationSoundPlayer;
    private bool showOverheadPreview;

    public SettingsWindow(
        Configuration configuration,
        RabbitEarsService rabbitEarsService,
        NotificationSoundPlayer notificationSoundPlayer)
    {
        this.configuration = configuration;
        this.rabbitEarsService = rabbitEarsService;
        this.notificationSoundPlayer = notificationSoundPlayer;
    }

    public bool IsOpen { get; set; }

    public void Draw()
    {
        if (!this.IsOpen)
            return;

        var isOpen = this.IsOpen;
        ImGui.SetNextWindowSize(new Vector2(430f, 0f), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("CarrotPatch##settings", ref isOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            this.IsOpen = isOpen;
            ImGui.End();
            return;
        }

        this.IsOpen = isOpen;

        if (ImGui.BeginTabBar("CarrotPatchSettingsTabs"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                this.DrawGeneralTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Sound"))
            {
                this.DrawSoundTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Appearance"))
            {
                this.DrawAppearanceTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Filters"))
            {
                this.DrawFiltersTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    private void DrawGeneralTab()
    {
        var enabled = this.configuration.RabbitEarsEnabled;
        if (ImGui.Checkbox("Alert when players target me", ref enabled))
        {
            this.configuration.RabbitEarsEnabled = enabled;
            this.configuration.Save();
        }

        ImGui.Separator();

        var maxActiveBeacons = RabbitEarsOptions.ClampMaxActiveBeacons(this.configuration.MaxActiveBeacons);
        if (ImGui.InputInt("Max overlay markers", ref maxActiveBeacons))
        {
            this.configuration.MaxActiveBeacons = RabbitEarsOptions.ClampMaxActiveBeacons(maxActiveBeacons);
            this.configuration.Save();
        }

        var duration = RabbitEarsOptions.ClampBeaconDurationSeconds(this.configuration.BeaconDurationSeconds);
        if (ImGui.SliderInt("Beacon duration", ref duration, 1, 120, "%d seconds"))
        {
            this.configuration.BeaconDurationSeconds = duration;
            this.configuration.Save();
        }

        var showChatMessage = this.configuration.ShowChatMessage;
        if (ImGui.Checkbox("Show chat confirmation", ref showChatMessage))
        {
            this.configuration.ShowChatMessage = showChatMessage;
            this.configuration.Save();
        }

        var showOnlyLatestSignalPerPlayer = this.configuration.ShowOnlyLatestSignalPerPlayer;
        if (ImGui.Checkbox("Only show latest entry per player", ref showOnlyLatestSignalPerPlayer))
        {
            this.configuration.ShowOnlyLatestSignalPerPlayer = showOnlyLatestSignalPerPlayer;
            this.configuration.Save();
        }

        if (ImGui.Button("Clear signal log"))
        {
            this.rabbitEarsService.ClearRecentSignals();
        }

        ImGui.Separator();

        var debugMode = this.configuration.DebugMode;
        if (ImGui.Checkbox("Debug logging", ref debugMode))
        {
            this.configuration.DebugMode = debugMode;
            this.configuration.Save();
        }
    }

    private void DrawSoundTab()
    {
        var playSoundOnOverlayMarker = this.configuration.PlaySoundOnOverlayMarker;
        if (ImGui.Checkbox("Enable notification sounds", ref playSoundOnOverlayMarker))
        {
            this.configuration.PlaySoundOnOverlayMarker = playSoundOnOverlayMarker;
            this.configuration.Save();
        }

        var playSoundOnTell = this.configuration.PlaySoundOnTell;
        if (ImGui.Checkbox("Play sound for tells", ref playSoundOnTell))
        {
            this.configuration.PlaySoundOnTell = playSoundOnTell;
            this.configuration.Save();
        }

        var playSoundOnTargeting = this.configuration.PlaySoundOnTargeting;
        if (ImGui.Checkbox("Play sound for targeting", ref playSoundOnTargeting))
        {
            this.configuration.PlaySoundOnTargeting = playSoundOnTargeting;
            this.configuration.Save();
        }

        var notificationVolume = RabbitEarsOptions.ClampNotificationVolume(this.configuration.NotificationVolume);
        ImGui.SetNextItemWidth(300f);
        if (ImGui.SliderFloat("Sound volume", ref notificationVolume, 0f, 1f, "%.2f", ImGuiSliderFlags.None))
        {
            this.configuration.NotificationVolume = RabbitEarsOptions.ClampNotificationVolume(notificationVolume);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGui.Button("Test"))
        {
            this.notificationSoundPlayer.Play(this.configuration.NotificationVolume);
        }
    }

    private void DrawAppearanceTab()
    {
        var showOverlayMarker = this.configuration.ShowOverlayMarker;
        if (ImGui.Checkbox("Show overlay marker", ref showOverlayMarker))
        {
            this.configuration.ShowOverlayMarker = showOverlayMarker;
            this.configuration.Save();
        }

        var markerScale = RabbitEarsOptions.ClampMarkerScale(this.configuration.MarkerScale);
        if (ImGui.SliderFloat("Marker scale", ref markerScale, RabbitEarsOptions.MinimumMarkerScale, RabbitEarsOptions.MaximumMarkerScale, "%.2f"))
        {
            this.configuration.MarkerScale = RabbitEarsOptions.ClampMarkerScale(markerScale);
            this.configuration.Save();
        }

        var overheadBackgroundOpacity = RabbitEarsOptions.ClampOverheadBackgroundOpacity(this.configuration.OverheadBackgroundOpacity);
        if (ImGui.SliderFloat("Overhead background opacity", ref overheadBackgroundOpacity, RabbitEarsOptions.MinimumOverheadBackgroundOpacity, RabbitEarsOptions.MaximumOverheadBackgroundOpacity, "%.2f"))
        {
            this.configuration.OverheadBackgroundOpacity = RabbitEarsOptions.ClampOverheadBackgroundOpacity(overheadBackgroundOpacity);
            this.configuration.Save();
        }

        ImGui.Checkbox("Show overhead preview", ref this.showOverheadPreview);
        if (this.showOverheadPreview)
        {
            this.DrawOverheadPreview();
        }
    }

    private void DrawFiltersTab()
    {
        var suppressParty = this.configuration.SuppressAlertsFromPartyMembers;
        if (ImGui.Checkbox("Suppress alerts from party members", ref suppressParty))
        {
            this.configuration.SuppressAlertsFromPartyMembers = suppressParty;
            this.configuration.Save();
        }

        var suppressAlliance = this.configuration.SuppressAlertsFromAllianceMembers;
        if (ImGui.Checkbox("Suppress alerts from alliance members", ref suppressAlliance))
        {
            this.configuration.SuppressAlertsFromAllianceMembers = suppressAlliance;
            this.configuration.Save();
        }

        var suppressSelf = this.configuration.SuppressAlertsFromSelf;
        if (ImGui.Checkbox("Suppress alerts from yourself", ref suppressSelf))
        {
            this.configuration.SuppressAlertsFromSelf = suppressSelf;
            this.configuration.Save();
        }

    }

    private void DrawOverheadPreview()
    {
        var previewTopLeft = ImGui.GetCursorScreenPos() + new Vector2(0f, 78f);
        var previewSize = new Vector2(MathF.Max(360f, ImGui.GetContentRegionAvail().X), 120f);
        var drawList = ImGui.GetWindowDrawList();
        RabbitEarsMarkerRenderer.Draw(
            drawList,
            previewTopLeft + new Vector2(previewSize.X / 2f, 92f),
            "Preview Player",
            "12y  15s",
            isTargeting: true,
            hasTell: true,
            isManualMarker: false,
            this.configuration.MarkerScale,
            this.configuration.OverheadBackgroundOpacity);

        ImGui.Dummy(previewSize);
    }
}
