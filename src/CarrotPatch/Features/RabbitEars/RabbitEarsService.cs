using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Chat;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsService : IDisposable
{
    private readonly IChatGui chatGui;
    private readonly IObjectTable objectTable;
    private readonly IFramework framework;
    private readonly IPluginLog pluginLog;
    private readonly Configuration configuration;
    private readonly NotificationSoundPlayer notificationSoundPlayer;
    private readonly List<ActiveBeacon> activeBeacons = [];
    private readonly RecentSignalStore recentSignalStore = new();

    public RabbitEarsService(
        IChatGui chatGui,
        IObjectTable objectTable,
        IFramework framework,
        IPluginLog pluginLog,
        Configuration configuration,
        NotificationSoundPlayer notificationSoundPlayer)
    {
        this.chatGui = chatGui;
        this.objectTable = objectTable;
        this.framework = framework;
        this.pluginLog = pluginLog;
        this.configuration = configuration;
        this.notificationSoundPlayer = notificationSoundPlayer;

        this.chatGui.ChatMessageUnhandled += this.OnChatMessage;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IReadOnlyList<ActiveBeacon> ActiveBeacons => this.activeBeacons;

    public IReadOnlyList<RecentSignal> RecentSignals => this.recentSignalStore.RecentSignals;

    public void ClearRecentSignals()
        => this.recentSignalStore.Clear();

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
        {
            this.RecordSignal(RabbitEarsSignalType.Tell, tellInfo.SenderName, tellInfo.SenderWorld, null, null, now: DateTime.UtcNow, isVisible: false);
            return;
        }

        var match = this.FindBestMatch(tellInfo.SenderName, localPlayer);
        if (match is null)
        {
            this.pluginLog.Information("{Sender} not found nearby.", tellInfo.SenderName);
            this.RecordSignal(RabbitEarsSignalType.Tell, tellInfo.SenderName, tellInfo.SenderWorld, null, null, now: DateTime.UtcNow, isVisible: false);
            if (this.configuration.ShowChatMessage)
            {
                this.chatGui.Print($"Rabbit Ears: Tell from {tellInfo.SenderName}, but they are not currently nearby or visible.");
            }

            return;
        }

        this.UpsertBeacon(
            tellInfo.SenderName,
            tellInfo.SenderWorld,
            match,
            localPlayer,
            DateTime.UtcNow,
            moveToFront: true,
            isTargeting: IsTargetingLocalPlayer(match, localPlayer),
            hasTell: true,
            recordTargetingSignal: false);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!this.configuration.RabbitEarsEnabled)
            return;

        var now = DateTime.UtcNow;
        this.activeBeacons.RemoveAll(beacon => beacon.ExpiresAt <= now);

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer is null)
            return;

        this.UpdateRecentSignalVisibility(localPlayer);
        var targeterIds = this.UpdateTargetingBeacons(localPlayer, now);

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

            var isStillTargeting = targeterIds.Contains(gameObject.GameObjectId);
            if (isStillTargeting)
            {
                beacon.IsTargeting = true;
                beacon.LastSeenTargetingAt = now;
                beacon.ExpiresAt = GetExpiresAt(now, this.configuration.BeaconDurationSeconds);
                continue;
            }

            if (beacon.IsTargeting)
            {
                beacon.IsTargeting = false;
                beacon.LastSeenTargetingAt = now;
                beacon.ExpiresAt = GetExpiresAt(now, this.configuration.BeaconDurationSeconds);
            }
        }

        this.TrimActiveBeacons();
    }

    private HashSet<ulong> UpdateTargetingBeacons(IGameObject localPlayer, DateTime now)
    {
        var targeterIds = new HashSet<ulong>();

        foreach (var player in this.objectTable.PlayerObjects)
        {
            if (player.GameObjectId == localPlayer.GameObjectId || !IsTargetingLocalPlayer(player, localPlayer))
                continue;

            targeterIds.Add(player.GameObjectId);
            var existing = this.FindExistingBeacon(player);
            var isNewTargetingSignal = existing is null || !existing.IsTargeting;
            this.UpsertBeacon(
                player.Name.TextValue,
                senderWorld: null,
                player,
                localPlayer,
                now,
                moveToFront: isNewTargetingSignal,
                isTargeting: true,
                hasTell: false,
                recordTargetingSignal: isNewTargetingSignal);
        }

        return targeterIds;
    }

    private ActiveBeacon UpsertBeacon(
        string senderName,
        string? senderWorld,
        IGameObject gameObject,
        IGameObject localPlayer,
        DateTime now,
        bool moveToFront,
        bool isTargeting,
        bool hasTell,
        bool recordTargetingSignal)
    {
        var beacon = this.FindExistingBeacon(gameObject)
            ?? this.activeBeacons.FirstOrDefault(existing =>
                string.Equals(existing.SenderName, senderName, StringComparison.OrdinalIgnoreCase));

        if (beacon is null)
        {
            beacon = new ActiveBeacon
            {
                SenderName = senderName,
                SenderWorld = senderWorld,
                GameObjectId = gameObject.GameObjectId,
                InitialPosition = gameObject.Position,
                LastKnownPosition = gameObject.Position,
                CreatedAt = now,
                ExpiresAt = GetExpiresAt(now, this.configuration.BeaconDurationSeconds),
            };

            this.activeBeacons.Insert(0, beacon);
            if (this.ShouldPlaySound(hasTell, recordTargetingSignal))
            {
                this.notificationSoundPlayer.Play(this.configuration.NotificationVolume);
            }
        }
        else if (moveToFront)
        {
            this.activeBeacons.Remove(beacon);
            this.activeBeacons.Insert(0, beacon);
        }

        beacon.GameObjectId = gameObject.GameObjectId;
        beacon.LastKnownPosition = gameObject.Position;
        beacon.Distance = DirectionHelper.Distance(localPlayer.Position, gameObject.Position);
        beacon.ExpiresAt = GetExpiresAt(now, this.configuration.BeaconDurationSeconds);

        if (isTargeting)
        {
            beacon.IsTargeting = true;
            beacon.LastSeenTargetingAt = now;
        }

        if (hasTell)
        {
            beacon.HasTell = true;
            beacon.LastTellAt = now;
            this.RecordSignal(RabbitEarsSignalType.Tell, senderName, senderWorld, gameObject.GameObjectId, beacon.Distance, now, isVisible: true);
            if (this.ShouldPlaySound(hasTell: true, recordTargetingSignal: false))
            {
                this.notificationSoundPlayer.Play(this.configuration.NotificationVolume);
            }
        }

        if (recordTargetingSignal)
        {
            this.RecordSignal(RabbitEarsSignalType.Targeting, senderName, senderWorld, gameObject.GameObjectId, beacon.Distance, now, isVisible: true);
            if (this.ShouldPlaySound(hasTell: false, recordTargetingSignal: true))
            {
                this.notificationSoundPlayer.Play(this.configuration.NotificationVolume);
            }
        }

        this.TrimActiveBeacons();
        return beacon;
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

    private ActiveBeacon? FindExistingBeacon(IGameObject gameObject)
        => this.activeBeacons.FirstOrDefault(beacon => beacon.GameObjectId == gameObject.GameObjectId);

    private void TrimActiveBeacons()
    {
        var maxActiveBeacons = RabbitEarsOptions.ClampMaxActiveBeacons(this.configuration.MaxActiveBeacons);
        if (this.activeBeacons.Count > maxActiveBeacons)
        {
            this.activeBeacons.RemoveRange(maxActiveBeacons, this.activeBeacons.Count - maxActiveBeacons);
        }
    }

    private static DateTime GetExpiresAt(DateTime now, int durationSeconds)
        => now.AddSeconds(RabbitEarsOptions.ClampBeaconDurationSeconds(durationSeconds));

    private static bool IsTargetingLocalPlayer(IGameObject gameObject, IGameObject localPlayer)
        => gameObject.TargetObjectId == localPlayer.GameObjectId;

    private bool ShouldPlaySound(bool hasTell, bool recordTargetingSignal)
        => this.configuration.PlaySoundOnOverlayMarker
            && ((hasTell && this.configuration.PlaySoundOnTell)
                || (recordTargetingSignal && this.configuration.PlaySoundOnTargeting));

    private void RecordSignal(
        RabbitEarsSignalType type,
        string senderName,
        string? senderWorld,
        ulong? gameObjectId,
        float? distance,
        DateTime now,
        bool isVisible)
        => this.recentSignalStore.Record(type, senderName, senderWorld, gameObjectId, distance, now, isVisible);

    private void UpdateRecentSignalVisibility(IGameObject localPlayer)
    {
        foreach (var signal in this.recentSignalStore.RecentSignals)
        {
            var gameObject = signal.GameObjectId.HasValue
                ? this.objectTable.SearchById(signal.GameObjectId.Value)
                : null;

            gameObject ??= this.FindBestMatch(signal.SenderName, localPlayer);
            signal.IsVisible = gameObject is not null;
            if (gameObject is null)
                continue;

            signal.GameObjectId = gameObject.GameObjectId;
            signal.Distance = DirectionHelper.Distance(localPlayer.Position, gameObject.Position);
        }
    }
}
