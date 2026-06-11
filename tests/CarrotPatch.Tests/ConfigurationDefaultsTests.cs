using Xunit;

namespace CarrotPatch.Tests;

public sealed class ConfigurationDefaultsTests
{
    [Fact]
    public void RabbitEarsDefaults_UseExpectedValues()
    {
        Assert.Equal(10, ConfigurationDefaults.MaxActiveBeacons);
        Assert.False(ConfigurationDefaults.ShowChatMessage);
        Assert.True(ConfigurationDefaults.ShowOnlyLatestSignalPerPlayer);
        Assert.False(ConfigurationDefaults.OpenSignalLogOnNewEntry);
    }
}
