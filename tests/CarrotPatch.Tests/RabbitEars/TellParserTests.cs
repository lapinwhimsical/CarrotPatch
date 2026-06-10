using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class TellParserTests
{
    [Fact]
    public void TryParseIncomingTell_ParsesNameAndWorld()
    {
        var parsed = TellParserCore.TryParseIncomingTell(
            isIncomingTell: true,
            "Pipkin Patch @ Cactuar",
            localPlayerName: "Carrot Friend",
            out var tellInfo);

        Assert.True(parsed);
        Assert.Equal("Pipkin Patch", tellInfo.SenderName);
        Assert.Equal("Cactuar", tellInfo.SenderWorld);
    }

    [Fact]
    public void TryParseIncomingTell_CleansWhitespaceAndBrackets()
    {
        var parsed = TellParserCore.TryParseIncomingTell(
            isIncomingTell: true,
            "  <  Pipkin   Patch  >  ",
            localPlayerName: null,
            out var tellInfo);

        Assert.True(parsed);
        Assert.Equal("Pipkin Patch", tellInfo.SenderName);
        Assert.Null(tellInfo.SenderWorld);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("[]")]
    public void TryParseIncomingTell_RejectsEmptySender(string sender)
    {
        Assert.False(TellParserCore.TryParseIncomingTell(isIncomingTell: true, sender, null, out _));
    }

    [Fact]
    public void TryParseIncomingTell_RejectsNonIncomingTell()
    {
        Assert.False(TellParserCore.TryParseIncomingTell(isIncomingTell: false, "Pipkin Patch", null, out _));
    }

    [Fact]
    public void TryParseIncomingTell_RejectsLocalPlayer()
    {
        Assert.False(TellParserCore.TryParseIncomingTell(
            isIncomingTell: true,
            "Pipkin Patch",
            localPlayerName: "pipkin patch",
            out _));
    }
}
