using System;
using System.Linq;
using CarrotPatch.Features.RabbitEars;
using Dalamud.Bindings.ImGui;

namespace CarrotPatch.UI;

public sealed class SettingsWindow
{
    private readonly Configuration configuration;
    private readonly RecentSignalsWindow recentSignalsWindow;

    public SettingsWindow(Configuration configuration, RecentSignalsWindow recentSignalsWindow)
    {
        this.configuration = configuration;
        this.recentSignalsWindow = recentSignalsWindow;
    }

    public bool IsOpen { get; set; }

    public void Draw()
    {
        if (!this.IsOpen)
            return;

        var isOpen = this.IsOpen;
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(360f, 0f), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("CarrotPatch##settings", ref isOpen, ImGuiWindowFlags.AlwaysAutoResize))
        {
            this.IsOpen = isOpen;
            ImGui.End();
            return;
        }

        this.IsOpen = isOpen;

        var enabled = this.configuration.RabbitEarsEnabled;
        if (ImGui.Checkbox("Enable Rabbit Ears", ref enabled))
        {
            this.configuration.RabbitEarsEnabled = enabled;
            this.configuration.Save();
        }

        var targetAlertsEnabled = this.configuration.TargetAlertsEnabled;
        if (ImGui.Checkbox("Alert when players target me", ref targetAlertsEnabled))
        {
            this.configuration.TargetAlertsEnabled = targetAlertsEnabled;
            this.configuration.Save();
        }

        if (ImGui.Button("Open Recent Signals"))
        {
            this.recentSignalsWindow.IsOpen = true;
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
        if (ImGui.SliderFloat("Sound volume", ref notificationVolume, 0f, 1f, "%.2f", ImGuiSliderFlags.None))
        {
            this.configuration.NotificationVolume = RabbitEarsOptions.ClampNotificationVolume(notificationVolume);
            this.configuration.Save();
        }

        var showChatMessage = this.configuration.ShowChatMessage;
        if (ImGui.Checkbox("Show chat confirmation", ref showChatMessage))
        {
            this.configuration.ShowChatMessage = showChatMessage;
            this.configuration.Save();
        }

        ImGui.Separator();
        this.DrawNameFilter("Only alert for names", this.configuration.AllowedPlayerNames, names => this.configuration.AllowedPlayerNames = names);
        this.DrawNameFilter("Never alert for names", this.configuration.BlockedPlayerNames, names => this.configuration.BlockedPlayerNames = names);

        var debugMode = this.configuration.DebugMode;
        if (ImGui.Checkbox("Debug logging", ref debugMode))
        {
            this.configuration.DebugMode = debugMode;
            this.configuration.Save();
        }

        ImGui.End();
    }

    private void DrawNameFilter(string label, System.Collections.Generic.List<string> currentNames, Action<System.Collections.Generic.List<string>> setNames)
    {
        var text = string.Join(Environment.NewLine, currentNames.Order(StringComparer.OrdinalIgnoreCase));
        if (ImGui.InputTextMultiline(label, ref text, 4096, new System.Numerics.Vector2(0f, 72f)))
        {
            setNames(RabbitEarsFilter.ParseNames(text));
            this.configuration.Save();
        }
    }
}
