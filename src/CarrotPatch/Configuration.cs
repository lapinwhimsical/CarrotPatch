using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CarrotPatch;

[System.Serializable]
public sealed class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    public bool RabbitEarsEnabled { get; set; } = true;

    public int MaxActiveBeacons { get; set; } = ConfigurationDefaults.MaxActiveBeacons;

    public int BeaconDurationSeconds { get; set; } = 15;

    public bool ShowOverlayMarker { get; set; } = true;

    public bool PlaySoundOnOverlayMarker { get; set; } = true;

    public bool PlaySoundOnTell { get; set; } = true;

    public bool PlaySoundOnTargeting { get; set; } = true;

    public float NotificationVolume { get; set; } = 1f;

    public float MarkerScale { get; set; } = 1f;

    public float OverheadBackgroundOpacity { get; set; } = 0.85f;

    public float OverheadForegroundOpacity { get; set; } = 1f;

    public bool ShowChatMessage { get; set; } = ConfigurationDefaults.ShowChatMessage;

    public bool ShowOnlyLatestSignalPerPlayer { get; set; } = ConfigurationDefaults.ShowOnlyLatestSignalPerPlayer;

    public bool OpenSignalLogOnNewEntry { get; set; } = ConfigurationDefaults.OpenSignalLogOnNewEntry;

    public bool SuppressAlertsFromPartyMembers { get; set; }

    public bool SuppressAlertsFromAllianceMembers { get; set; }

    public bool DebugMode { get; set; }

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface?.SavePluginConfig(this);
    }
}

public static class ConfigurationDefaults
{
    public const int MaxActiveBeacons = 10;
    public const bool ShowChatMessage = false;
    public const bool ShowOnlyLatestSignalPerPlayer = true;
    public const bool OpenSignalLogOnNewEntry = false;
}
