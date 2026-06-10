using System;
using System.Collections.Generic;
using System.Linq;

namespace CarrotPatch.Features.RabbitEars;

public static class RabbitEarsFilter
{
    public static bool IsAllowed(string senderName, IEnumerable<string>? allowedNames, IEnumerable<string>? blockedNames)
    {
        var normalizedSender = TellParserCore.NormalizeName(senderName);
        if (string.IsNullOrWhiteSpace(normalizedSender))
            return false;

        var blocked = BuildSet(blockedNames);
        if (blocked.Contains(normalizedSender))
            return false;

        var allowed = BuildSet(allowedNames);
        return allowed.Count == 0 || allowed.Contains(normalizedSender);
    }

    public static List<string> ParseNames(string value)
        => value.Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TellParserCore.NormalizeName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static HashSet<string> BuildSet(IEnumerable<string>? names)
        => names?
            .Select(TellParserCore.NormalizeName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? [];
}
