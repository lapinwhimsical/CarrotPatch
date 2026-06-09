using System;
using System.Numerics;

namespace CarrotPatch.Features.RabbitEars;

public sealed class ActiveBeacon
{
    public required string SenderName { get; init; }

    public string? SenderWorld { get; init; }

    public ulong GameObjectId { get; set; }

    public Vector3 InitialPosition { get; init; }

    public Vector3 LastKnownPosition { get; set; }

    public DateTime CreatedAt { get; init; }

    public DateTime ExpiresAt { get; init; }

    public bool WasClickedOrTargeted { get; set; }

    public float Distance { get; set; }

    public string DirectionText { get; set; } = string.Empty;

    public int SecondsRemaining => Math.Max(0, (int)Math.Ceiling((this.ExpiresAt - DateTime.UtcNow).TotalSeconds));
}
