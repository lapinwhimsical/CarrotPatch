using System;
using System.Numerics;

namespace CarrotPatch.Features.RabbitEars;

public static class DirectionHelper
{
    private static readonly string[] Directions = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

    public static float Distance(Vector3 from, Vector3 to)
        => Vector3.Distance(from, to);

    public static string CardinalDirection(Vector3 from, Vector3 to)
    {
        var dx = to.X - from.X;
        var dz = to.Z - from.Z;
        var angle = Math.Atan2(dx, dz);
        var normalized = angle < 0 ? angle + (Math.PI * 2) : angle;
        var index = (int)Math.Round(normalized / (Math.PI / 4)) % Directions.Length;

        return Directions[index];
    }
}
