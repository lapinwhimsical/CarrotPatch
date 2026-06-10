using System;
using System.Text.RegularExpressions;

namespace CarrotPatch.Features.RabbitEars;

public static partial class TellParserCore
{
    public static bool TryParseIncomingTell(bool isIncomingTell, string senderText, string? localPlayerName, out TellInfo tellInfo)
    {
        tellInfo = default;

        if (!isIncomingTell)
            return false;

        var cleanedSenderText = CleanSender(senderText);
        if (string.IsNullOrWhiteSpace(cleanedSenderText))
            return false;

        var (senderName, senderWorld) = SplitNameAndWorld(cleanedSenderText);
        if (string.IsNullOrWhiteSpace(senderName))
            return false;

        if (!string.IsNullOrWhiteSpace(localPlayerName)
            && string.Equals(senderName, localPlayerName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        tellInfo = new TellInfo(senderName, senderWorld);
        return true;
    }

    public static string NormalizeName(string value)
        => CleanSender(value);

    private static string CleanSender(string value)
    {
        var cleaned = value.Trim();
        cleaned = cleaned.TrimStart('\ue090').Trim();
        cleaned = cleaned.Trim('<', '>', '[', ']', ' ');

        return CollapseWhitespaceRegex().Replace(cleaned, " ");
    }

    private static (string Name, string? World) SplitNameAndWorld(string senderText)
    {
        var match = NameAndWorldRegex().Match(senderText);
        if (match.Success)
        {
            return (match.Groups["name"].Value.Trim(), match.Groups["world"].Value.Trim());
        }

        return (senderText, null);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();

    [GeneratedRegex(@"^(?<name>.+?)\s*@\s*(?<world>[^@]+)$")]
    private static partial Regex NameAndWorldRegex();
}
