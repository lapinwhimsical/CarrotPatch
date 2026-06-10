using System;
using Dalamud.Bindings.ImGui;

namespace CarrotPatch.UI;

public sealed class SettingsWindow
{
    private readonly Configuration configuration;

    public SettingsWindow(Configuration configuration)
    {
        this.configuration = configuration;
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

        var maxActiveBeacons = Math.Clamp(this.configuration.MaxActiveBeacons, 1, 25);
        if (ImGui.InputInt("Max overlay markers", ref maxActiveBeacons))
        {
            this.configuration.MaxActiveBeacons = Math.Clamp(maxActiveBeacons, 1, 25);
            this.configuration.Save();
        }

        var duration = Math.Clamp(this.configuration.BeaconDurationSeconds, 1, 120);
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

        var showChatMessage = this.configuration.ShowChatMessage;
        if (ImGui.Checkbox("Show chat confirmation", ref showChatMessage))
        {
            this.configuration.ShowChatMessage = showChatMessage;
            this.configuration.Save();
        }

        var debugMode = this.configuration.DebugMode;
        if (ImGui.Checkbox("Debug logging", ref debugMode))
        {
            this.configuration.DebugMode = debugMode;
            this.configuration.Save();
        }

        ImGui.End();
    }
}
