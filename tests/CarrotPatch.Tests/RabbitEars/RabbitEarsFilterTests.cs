using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class RabbitEarsFilterTests
{
    [Fact]
    public void IsAllowed_AllowsEveryoneWhenAllowListIsEmpty()
    {
        Assert.True(RabbitEarsFilter.IsAllowed("Pipkin Patch", [], []));
    }

    [Fact]
    public void IsAllowed_BlockListWinsOverAllowList()
    {
        Assert.False(RabbitEarsFilter.IsAllowed(
            "Pipkin Patch",
            ["Pipkin Patch"],
            ["pipkin   patch"]));
    }

    [Fact]
    public void IsAllowed_RequiresAllowListMatchWhenAllowListHasEntries()
    {
        Assert.False(RabbitEarsFilter.IsAllowed("Pipkin Patch", ["Other Player"], []));
        Assert.True(RabbitEarsFilter.IsAllowed("Pipkin Patch", ["pipkin patch"], []));
    }

    [Fact]
    public void ParseNames_NormalizesAndDeduplicatesNames()
    {
        var names = RabbitEarsFilter.ParseNames("Pipkin   Patch, pipkin patch\r\nOther Player");

        Assert.Equal(["Other Player", "Pipkin Patch"], names);
    }
}
