using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class RabbitEarsAlertFilterTests
{
    [Theory]
    [InlineData("Patch Friend", "patch friend")]
    [InlineData("  Patch   Friend  ", "Patch Friend")]
    [InlineData("\ue05d Patch Friend", "Patch Friend")]
    public void IsSamePlayerName_NormalizesNames(string firstName, string secondName)
    {
        Assert.True(RabbitEarsAlertFilter.IsSamePlayerName(firstName, secondName));
    }

    [Fact]
    public void IsSamePlayerName_DistinguishesDifferentNames()
    {
        Assert.False(RabbitEarsAlertFilter.IsSamePlayerName("Patch Friend", "Other Friend"));
    }
}
