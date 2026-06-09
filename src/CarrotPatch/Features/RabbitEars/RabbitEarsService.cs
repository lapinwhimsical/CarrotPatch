using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsService : IDisposable
{
    private const int MaxActiveBeacons = 3;

    private readonly IChatGui chatGui;
    private readonly IObjectTable objectTable;
    private readonly IFramework framework;
    private readonly IPluginLog pluginLog;
    private readonly Configuration configuration;
    private readonly List<ActiveBeacon> activeBeacons = [];

    public RabbitEarsService(
        IChatGui chatGui,
        IObjectTable objectTable,
        IFramework framework,
        IPluginLog pluginLog,
        Configuration configuration)
    {
        this.chatGui = chatGui;
        this.objectTable = objectTable;
        this.framework = framework;
        this.pluginLog = pluginLog;
        this.configuration = configuration;

        this.chatGui.ChatMessageUnhandled += this.OnChatMessage;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IReadOnlyList<ActiveBeacon> ActiveBeacons => this.activeBeacons;

    public void Dispose()
    {
        this.framework.Update -= this.OnFrameworkUpdate;
        this.chatGui.ChatMessageUnhandled -= this.OnChatMessage;
    }

    private void OnChatMessage(IChatMessage message)
    {
        if (!this.configuration.RabbitEarsEnabled)
            return;

        var localPlayer = this.objectTable.LocalPlayer;
        var localPlayerName = localPlayer?.Name.TextValue;

        if (this.configuration.DebugMode)
        {
            this.pluginLog.Debug(
                "Rabbit Ears chat message: {Type} from '{Sender}' message '{Message}'",
                message.LogKind,
                message.Sender.TextValue,
                message.Message.TextValue);
        }

        if (!TellParser.TryParseIncomingTell(message, localPlayerName, out var tellInfo))
            return;

        this.pluginLog.Information("Rabbit Ears detected tell from {Sender}.", tellInfo.SenderName);

        if (localPlayer is null)
            return;

        var match = this.FindBestMatch(tellInfo.SenderName, localPlayer);
        if (match is null)
        {
            this.pluginLog.Information("{Sender} not found nearby.", tellInfo.SenderName);
            if (this.configuration.ShowChatMessage)
            {
                this.chatGui.Print($"Rabbit Ears: Tell from {tellInfo.SenderName}, but they are not currently nearby or visible.");
            }

            return;
        }

        var now = DateTime.UtcNow;
        var distance = DirectionHelper.Distance(localPlayer.Position, match.Position);
        var direction = DirectionHelper.CardinalDirection(localPlayer.Position, match.Position);

        this.activeBeacons.RemoveAll(beacon =>
            string.Equals(beacon.SenderName, tellInfo.SenderName, StringComparison.OrdinalIgnoreCase));

        this.activeBeacons.Insert(0, new ActiveBeacon
        {
            SenderName = tellInfo.SenderName,
            SenderWorld = tellInfo.SenderWorld,
            GameObjectId = match.GameObjectId,
            InitialPosition = match.Position,
            LastKnownPosition = match.Position,
            CreatedAt = now,
            ExpiresAt = now.AddSeconds(Math.Clamp(this.configuration.BeaconDurationSeconds, 1, 120)),
            Distance = distance,
            DirectionText = direction,
        });

        if (this.activeBeacons.Count > MaxActiveBeacons)
        {
            this.activeBeacons.RemoveRange(MaxActiveBeacons, this.activeBeacons.Count - MaxActiveBeacons);
        }

        if (this.configuration.ShowChatMessage)
        {
            this.chatGui.Print($"Rabbit Ears: Tell from {tellInfo.SenderName} - {MathF.Round(distance)}y {direction}");
        }
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        var now = DateTime.UtcNow;
        this.activeBeacons.RemoveAll(beacon => beacon.ExpiresAt <= now);

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer is null)
            return;

        for (var i = this.activeBeacons.Count - 1; i >= 0; i--)
        {
            var beacon = this.activeBeacons[i];
            var gameObject = this.objectTable.SearchById(beacon.GameObjectId)
                ?? this.FindBestMatch(beacon.SenderName, localPlayer);

            if (gameObject is null)
            {
                this.activeBeacons.RemoveAt(i);
                continue;
            }

            beacon.GameObjectId = gameObject.GameObjectId;
            beacon.LastKnownPosition = gameObject.Position;
            beacon.Distance = DirectionHelper.Distance(localPlayer.Position, gameObject.Position);
            beacon.DirectionText = DirectionHelper.CardinalDirection(localPlayer.Position, gameObject.Position);
        }
    }

    private IGameObject? FindBestMatch(string senderName, IGameObject localPlayer)
    {
        var normalizedSender = TellParser.NormalizeName(senderName);
        var matches = this.objectTable.PlayerObjects
            .Where(player => string.Equals(
                TellParser.NormalizeName(player.Name.TextValue),
                normalizedSender,
                StringComparison.OrdinalIgnoreCase))
            .OrderBy(player => DirectionHelper.Distance(localPlayer.Position, player.Position))
            .ToList();

        if (matches.Count > 1 && this.configuration.DebugMode)
        {
            this.pluginLog.Debug("Rabbit Ears found {Count} nearby matches for {Sender}; choosing closest.", matches.Count, senderName);
        }

        return matches.FirstOrDefault();
    }
}
