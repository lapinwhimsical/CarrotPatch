using System;
using System.Globalization;
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
        cleaned = TrimLeadingNameMarkers(cleaned).Trim();
        cleaned = cleaned.Trim('<', '>', '[', ']', ' ');
        cleaned = TrimLeadingNameMarkers(cleaned).Trim();

        return CollapseWhitespaceRegex().Replace(cleaned, " ");
    }

    private static string TrimLeadingNameMarkers(string value)
    {
        var trimStart = 0;
        while (trimStart < value.Length)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(value, trimStart);
            if (!IsNameMarkerCategory(category))
                break;

            trimStart += char.IsSurrogatePair(value, trimStart) ? 2 : 1;
        }

        return trimStart == 0
            ? value
            : value[trimStart..];
    }

    private static bool IsNameMarkerCategory(UnicodeCategory category)
        => category is UnicodeCategory.PrivateUse
            or UnicodeCategory.MathSymbol
            or UnicodeCategory.CurrencySymbol
            or UnicodeCategory.ModifierSymbol
            or UnicodeCategory.OtherSymbol;

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
