using System;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CarrotPatch;

public sealed class Plugin : IDalamudPlugin
{
    private readonly IPluginLog pluginLog;

    public Plugin(IPluginLog pluginLog)
    {
        this.pluginLog = pluginLog;
        this.pluginLog.Information("CarrotPatch loaded.");
    }

    public void Dispose()
    {
        this.pluginLog.Information("CarrotPatch unloaded.");
    }
}
