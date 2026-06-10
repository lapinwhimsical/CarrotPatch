using System;
using CarrotPatch.Features.RabbitEars;
using CarrotPatch.UI;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CarrotPatch;

public sealed class Plugin : IDalamudPlugin
{
    private const string MainCommand = "/carrotpatch";
    private const string SettingsCommand = "/carrotpatchsettings";

    private readonly IDalamudPluginInterface pluginInterface;
    private readonly ICommandManager commandManager;
    private readonly IPluginLog pluginLog;
    private readonly Configuration configuration;
    private readonly NotificationSoundPlayer notificationSoundPlayer;
    private readonly RabbitEarsService rabbitEarsService;
    private readonly RabbitEarsOverlay rabbitEarsOverlay;
    private readonly RecentSignalsWindow recentSignalsWindow;
    private readonly SettingsWindow settingsWindow;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui,
        IObjectTable objectTable,
        IFramework framework,
        IGameGui gameGui,
        ICommandManager commandManager,
        IPluginLog pluginLog)
    {
        this.pluginInterface = pluginInterface;
        this.commandManager = commandManager;
        this.pluginLog = pluginLog;

        this.configuration = this.pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Initialize(this.pluginInterface);

        this.notificationSoundPlayer = new NotificationSoundPlayer(pluginLog);
        this.rabbitEarsService = new RabbitEarsService(chatGui, objectTable, framework, pluginLog, this.configuration, this.notificationSoundPlayer);
        this.rabbitEarsOverlay = new RabbitEarsOverlay(this.rabbitEarsService, objectTable, gameGui, this.configuration);
        this.recentSignalsWindow = new RecentSignalsWindow(this.rabbitEarsService);
        this.settingsWindow = new SettingsWindow(this.configuration);

        this.pluginInterface.UiBuilder.Draw += this.DrawUi;
        this.pluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;
        this.pluginInterface.UiBuilder.OpenMainUi += this.OpenMainUi;

        this.commandManager.AddHandler(MainCommand, new CommandInfo(this.OnMainCommand)
        {
            HelpMessage = "Open CarrotPatch.",
        });
        this.commandManager.AddHandler(SettingsCommand, new CommandInfo(this.OnSettingsCommand)
        {
            HelpMessage = "Open CarrotPatch settings.",
        });
        this.pluginLog.Information("CarrotPatch loaded.");
    }

    public void Dispose()
    {
        this.commandManager.RemoveHandler(SettingsCommand);
        this.commandManager.RemoveHandler(MainCommand);

        this.pluginInterface.UiBuilder.OpenMainUi -= this.OpenMainUi;
        this.pluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;
        this.pluginInterface.UiBuilder.Draw -= this.DrawUi;

        this.rabbitEarsService.Dispose();
        this.pluginLog.Information("CarrotPatch unloaded.");
    }

    private void OnMainCommand(string command, string arguments)
    {
        this.recentSignalsWindow.IsOpen = true;
    }

    private void OnSettingsCommand(string command, string arguments)
    {
        this.settingsWindow.IsOpen = true;
    }

    private void OpenConfigUi()
    {
        this.settingsWindow.IsOpen = true;
    }

    private void OpenMainUi()
    {
        this.recentSignalsWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        this.rabbitEarsOverlay.Draw();
        this.recentSignalsWindow.Draw();
        this.settingsWindow.Draw();
    }
}
