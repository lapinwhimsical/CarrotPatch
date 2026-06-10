using CarrotPatch.Features.RabbitEars;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class RabbitEarsOptionsTests
{
    [Theory]
    [InlineData(-1, 1)]
    [InlineData(3, 3)]
    [InlineData(100, 25)]
    public void ClampMaxActiveBeacons_ClampsToSupportedRange(int value, int expected)
    {
        Assert.Equal(expected, RabbitEarsOptions.ClampMaxActiveBeacons(value));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(15, 15)]
    [InlineData(999, 120)]
    public void ClampBeaconDurationSeconds_ClampsToSupportedRange(int value, int expected)
    {
        Assert.Equal(expected, RabbitEarsOptions.ClampBeaconDurationSeconds(value));
    }

    [Theory]
    [InlineData(-1f, 0f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(2f, 1f)]
    public void ClampNotificationVolume_ClampsToSupportedRange(float value, float expected)
    {
        Assert.Equal(expected, RabbitEarsOptions.ClampNotificationVolume(value));
    }
}
