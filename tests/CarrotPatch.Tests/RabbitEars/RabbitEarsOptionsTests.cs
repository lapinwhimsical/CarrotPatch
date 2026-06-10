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

    [Theory]
    [InlineData(-1f, 0.10f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(2f, 1f)]
    public void ClampOverheadBackgroundOpacity_ClampsToSupportedRange(float value, float expected)
    {
        Assert.Equal(expected, RabbitEarsOptions.ClampOverheadBackgroundOpacity(value));
    }

    [Fact]
    public void ClampOverheadBackgroundOpacity_UsesDefaultForNonFiniteValues()
    {
        Assert.Equal(0.85f, RabbitEarsOptions.ClampOverheadBackgroundOpacity(float.NaN));
        Assert.Equal(0.85f, RabbitEarsOptions.ClampOverheadBackgroundOpacity(float.PositiveInfinity));
    }

    [Theory]
    [InlineData(-1f, 0.10f)]
    [InlineData(0.5f, 0.5f)]
    [InlineData(2f, 1f)]
    public void ClampOverheadForegroundOpacity_ClampsToSupportedRange(float value, float expected)
    {
        Assert.Equal(expected, RabbitEarsOptions.ClampOverheadForegroundOpacity(value));
    }

    [Fact]
    public void ClampOverheadForegroundOpacity_UsesDefaultForNonFiniteValues()
    {
        Assert.Equal(1f, RabbitEarsOptions.ClampOverheadForegroundOpacity(float.NaN));
        Assert.Equal(1f, RabbitEarsOptions.ClampOverheadForegroundOpacity(float.PositiveInfinity));
    }
}
