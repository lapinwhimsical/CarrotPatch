using System;

namespace CarrotPatch.Features.RabbitEars;

public static class RabbitEarsOptions
{
    public const int MinimumMaxActiveBeacons = 1;
    public const int MaximumMaxActiveBeacons = 25;
    public const int MinimumBeaconDurationSeconds = 1;
    public const int MaximumBeaconDurationSeconds = 120;
    public const float MinimumNotificationVolume = 0f;
    public const float MaximumNotificationVolume = 1f;
    public const float MinimumMarkerScale = 0.65f;
    public const float MaximumMarkerScale = 1.75f;

    public static int ClampMaxActiveBeacons(int value)
        => Math.Clamp(value, MinimumMaxActiveBeacons, MaximumMaxActiveBeacons);

    public static int ClampBeaconDurationSeconds(int value)
        => Math.Clamp(value, MinimumBeaconDurationSeconds, MaximumBeaconDurationSeconds);

    public static float ClampNotificationVolume(float value)
        => float.IsFinite(value) ? Math.Clamp(value, MinimumNotificationVolume, MaximumNotificationVolume) : 1f;

    public static float ClampMarkerScale(float value)
        => float.IsFinite(value) ? Math.Clamp(value, MinimumMarkerScale, MaximumMarkerScale) : 1f;
}
