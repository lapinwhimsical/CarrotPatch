using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dalamud.Plugin.Services;
using NAudio.Wave;

namespace CarrotPatch.Features.RabbitEars;

public sealed class NotificationSoundPlayer
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromSeconds(1);

    private readonly IPluginLog pluginLog;
    private readonly Assembly assembly;
    private DateTime lastPlayedAt = DateTime.MinValue;

    public NotificationSoundPlayer(IPluginLog pluginLog)
    {
        this.pluginLog = pluginLog;
        this.assembly = typeof(NotificationSoundPlayer).Assembly;
    }

    public void Play()
    {
        var now = DateTime.UtcNow;
        if (now - this.lastPlayedAt < MinimumInterval)
            return;

        this.lastPlayedAt = now;
        var thread = new Thread(this.PlayOnBackgroundThread)
        {
            IsBackground = true,
            Name = "CarrotPatch notification sound",
        };
        thread.Start();
    }

    private void PlayOnBackgroundThread()
    {
        try
        {
            using var stream = this.OpenNotificationStream();
            if (stream is null)
            {
                this.pluginLog.Warning("CarrotPatch notification sound resource was not found.");
                return;
            }

            using var reader = new Mp3FileReader(stream);
            using var output = new WaveOutEvent();
            output.Init(reader);
            output.Play();

            while (output.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(50);
            }
        }
        catch (Exception ex)
        {
            this.pluginLog.Warning(ex, "Failed to play CarrotPatch notification sound.");
        }
    }

    private Stream? OpenNotificationStream()
    {
        var resourceName = this.assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("Resources.notification.mp3", StringComparison.Ordinal));

        return resourceName is null
            ? null
            : this.assembly.GetManifestResourceStream(resourceName);
    }
}
