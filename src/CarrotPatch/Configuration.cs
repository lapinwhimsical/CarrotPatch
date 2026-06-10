using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CarrotPatch;

[System.Serializable]
public sealed class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    public bool RabbitEarsEnabled { get; set; } = true;

    public int MaxActiveBeacons { get; set; } = 3;

    public int BeaconDurationSeconds { get; set; } = 15;

    public bool ShowOverlayMarker { get; set; } = true;

    public bool PlaySoundOnOverlayMarker { get; set; } = true;

    public bool PlaySoundOnTell { get; set; } = true;

    public bool PlaySoundOnTargeting { get; set; } = true;

    public float NotificationVolume { get; set; } = 1f;

    public float MarkerScale { get; set; } = 1f;

    public float OverheadBackgroundOpacity { get; set; } = 0.85f;

    public bool ShowChatMessage { get; set; } = true;

    public bool ShowOnlyLatestSignalPerPlayer { get; set; } = true;

    public bool SuppressAlertsFromPartyMembers { get; set; }

    public bool SuppressAlertsFromAllianceMembers { get; set; }

    public bool SuppressAlertsFromSelf { get; set; }

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
