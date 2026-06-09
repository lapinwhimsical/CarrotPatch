using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CarrotPatch;

[System.Serializable]
public sealed class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface? pluginInterface;

    public int Version { get; set; } = 1;

    public bool RabbitEarsEnabled { get; set; } = true;

    public int BeaconDurationSeconds { get; set; } = 15;

    public bool ShowOverlayMarker { get; set; } = true;

    public bool ShowChatMessage { get; set; } = true;

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
