using System;
using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class RecentSignalStoreTests
{
    [Fact]
    public void Record_AddsNewestSignalFirst()
    {
        var store = new RecentSignalStore();
        var firstSeenAt = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        var secondSeenAt = firstSeenAt.AddSeconds(1);

        store.Record(RabbitEarsSignalType.Tell, "First Player", null, 1, 10f, firstSeenAt, isVisible: true);
        store.Record(RabbitEarsSignalType.Targeting, "Second Player", null, 2, 5f, secondSeenAt, isVisible: true);

        Assert.Equal("Second Player", store.RecentSignals[0].SenderName);
        Assert.Equal("First Player", store.RecentSignals[1].SenderName);
    }

    [Fact]
    public void Record_IncrementsRecentSignalVersion()
    {
        var store = new RecentSignalStore();
        var now = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

        store.Record(RabbitEarsSignalType.Tell, "First Player", null, 1, 10f, now, isVisible: true);
        store.Record(RabbitEarsSignalType.Targeting, "Second Player", null, 2, 5f, now.AddSeconds(1), isVisible: true);

        Assert.Equal(2UL, store.RecentSignalVersion);
    }

    [Fact]
    public void Clear_DoesNotIncrementRecentSignalVersion()
    {
        var store = new RecentSignalStore();
        var now = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

        store.Record(RabbitEarsSignalType.Tell, "First Player", null, 1, 10f, now, isVisible: true);
        store.Clear();

        Assert.Equal(1UL, store.RecentSignalVersion);
    }

    [Fact]
    public void Record_TrimsHistoryToMaximum()
    {
        var store = new RecentSignalStore();
        var now = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < 55; i++)
        {
            store.Record(RabbitEarsSignalType.Tell, $"Player {i}", null, null, null, now.AddSeconds(i), isVisible: false);
        }

        Assert.Equal(50, store.RecentSignals.Count);
        Assert.Equal("Player 54", store.RecentSignals[0].SenderName);
        Assert.Equal("Player 5", store.RecentSignals[^1].SenderName);
    }
}
