namespace CarrotPatch.Features.RabbitEars;

public static class RecentSignalIdentity
{
    public static string GetPlayerKey(RecentSignal signal)
        => $"name:{TellParserCore.NormalizeName(signal.SenderName)}@{NormalizeWorld(signal.SenderWorld)}";

    private static string NormalizeWorld(string? world)
        => string.IsNullOrWhiteSpace(world)
            ? string.Empty
            : TellParserCore.NormalizeName(world);
}
