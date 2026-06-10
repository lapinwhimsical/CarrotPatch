using System;

namespace CarrotPatch.Features.RabbitEars;

public enum RabbitEarsSignalType
{
    Tell,
    Targeting,
}

public sealed class RecentSignal
{
    public required RabbitEarsSignalType Type { get; init; }

    public required string SenderName { get; init; }

    public string? SenderWorld { get; init; }

    public ulong? GameObjectId { get; set; }

    public float? Distance { get; set; }

    public required DateTime SeenAt { get; init; }

    public bool IsVisible { get; set; }
}
