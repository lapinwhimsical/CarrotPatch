using System;
using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class RecentSignalIdentityTests
{
    [Fact]
    public void GetPlayerKey_IgnoresTransientGameObjectId()
    {
        var now = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        var visibleSignal = new RecentSignal
        {
            Type = RabbitEarsSignalType.Tell,
            SenderName = "Patch Friend",
            SenderWorld = "Cactuar",
            GameObjectId = 123,
            Distance = 10f,
            SeenAt = now,
            IsVisible = true,
        };
        var unavailableSignal = new RecentSignal
        {
            Type = RabbitEarsSignalType.Tell,
            SenderName = "Patch Friend",
            SenderWorld = "Cactuar",
            GameObjectId = null,
            Distance = null,
            SeenAt = now.AddSeconds(1),
            IsVisible = false,
        };

        Assert.Equal(
            RecentSignalIdentity.GetPlayerKey(visibleSignal),
            RecentSignalIdentity.GetPlayerKey(unavailableSignal));
    }

    [Fact]
    public void GetPlayerKey_NormalizesSenderAndWorld()
    {
        var now = new DateTime(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);
        var firstSignal = new RecentSignal
        {
            Type = RabbitEarsSignalType.Tell,
            SenderName = "  Patch   Friend  ",
            SenderWorld = "  Cactuar  ",
            GameObjectId = null,
            Distance = null,
            SeenAt = now,
            IsVisible = false,
        };
        var secondSignal = new RecentSignal
        {
            Type = RabbitEarsSignalType.Tell,
            SenderName = "Patch Friend",
            SenderWorld = "Cactuar",
            GameObjectId = null,
            Distance = null,
            SeenAt = now.AddSeconds(1),
            IsVisible = false,
        };

        Assert.Equal(
            RecentSignalIdentity.GetPlayerKey(firstSignal),
            RecentSignalIdentity.GetPlayerKey(secondSignal));
    }
}
