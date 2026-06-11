using CarrotPatch.Features.RabbitEars;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Xunit;

namespace CarrotPatch.Tests.RabbitEars;

public sealed class NotificationSoundPlayerTests
{
    [Theory]
    [InlineData(0.10f, 0.10f)]
    [InlineData(-1f, 0f)]
    [InlineData(2f, 1f)]
    public void CreateVolumeAdjustedSampleProvider_AppliesClampedVolumeToSampleProvider(float volume, float expected)
    {
        var source = new TestSampleProvider();

        var provider = NotificationSoundPlayer.CreateVolumeAdjustedSampleProvider(source, volume);

        Assert.Same(source.WaveFormat, provider.WaveFormat);
        Assert.Equal(expected, provider.Volume);
    }

    private sealed class TestSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        public int Read(float[] buffer, int offset, int count)
            => 0;
    }
}
