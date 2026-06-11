using System;
using System.Collections.Generic;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RecentSignalStore
{
    private const int MaximumSignals = 50;
    private readonly List<RecentSignal> recentSignals = [];

    public IReadOnlyList<RecentSignal> RecentSignals => this.recentSignals;

    public ulong RecentSignalVersion { get; private set; }

    public void Record(
        RabbitEarsSignalType type,
        string senderName,
        string? senderWorld,
        ulong? gameObjectId,
        float? distance,
        DateTime seenAt,
        bool isVisible)
    {
        this.recentSignals.Insert(0, new RecentSignal
        {
            Type = type,
            SenderName = senderName,
            SenderWorld = senderWorld,
            GameObjectId = gameObjectId,
            Distance = distance,
            SeenAt = seenAt,
            IsVisible = isVisible,
        });
        this.RecentSignalVersion++;

        if (this.recentSignals.Count > MaximumSignals)
        {
            this.recentSignals.RemoveRange(MaximumSignals, this.recentSignals.Count - MaximumSignals);
        }
    }

    public void Clear()
        => this.recentSignals.Clear();
}
