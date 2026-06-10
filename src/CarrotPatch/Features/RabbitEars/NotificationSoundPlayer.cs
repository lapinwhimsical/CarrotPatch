using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class NotificationSoundPlayer : IDisposable
{
    private const string Alias = "CarrotPatchNotification";

    private readonly IPluginLog pluginLog;
    private readonly string soundPath;

    public NotificationSoundPlayer(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
    {
        this.pluginLog = pluginLog;
        var pluginDirectory = Path.GetDirectoryName(pluginInterface.AssemblyLocation.FullName)
            ?? pluginInterface.AssemblyLocation.DirectoryName
            ?? AppContext.BaseDirectory;
        this.soundPath = Path.Combine(pluginDirectory, "Resources", "notification.mp3");
    }

    public void Play()
    {
        if (!File.Exists(this.soundPath))
        {
            this.pluginLog.Warning("CarrotPatch notification sound not found at {Path}.", this.soundPath);
            return;
        }

        this.Close();
        if (SendCommand($"open \"{this.soundPath}\" type mpegvideo alias {Alias}") != 0)
            return;

        SendCommand($"play {Alias} from 0");
    }

    public void Dispose()
    {
        this.Close();
    }

    private void Close()
    {
        SendCommand($"close {Alias}");
    }

    private int SendCommand(string command)
    {
        var errorBuffer = new StringBuilder(256);
        var result = MciSendString(command, null, 0, IntPtr.Zero);
        if (result == 0)
            return result;

        if (MciGetErrorString(result, errorBuffer, errorBuffer.Capacity))
        {
            this.pluginLog.Debug("CarrotPatch notification sound command failed: {Error}", errorBuffer.ToString());
        }
        else
        {
            this.pluginLog.Debug("CarrotPatch notification sound command failed with MCI error {ErrorCode}.", result);
        }

        return result;
    }

    [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "mciSendStringW")]
    private static extern int MciSendString(string command, StringBuilder? returnValue, int returnLength, IntPtr windowHandle);

    [DllImport("winmm.dll", CharSet = CharSet.Unicode, EntryPoint = "mciGetErrorStringW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool MciGetErrorString(int errorCode, StringBuilder errorText, int errorTextLength);
}
