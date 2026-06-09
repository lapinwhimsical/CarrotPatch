using System;
using System.Text.RegularExpressions;
using Dalamud.Game.Chat;
using Dalamud.Game.Text;

namespace CarrotPatch.Features.RabbitEars;

public readonly record struct TellInfo(string SenderName, string? SenderWorld);

public static partial class TellParser
{
    public static bool TryParseIncomingTell(IChatMessage message, string? localPlayerName, out TellInfo tellInfo)
    {
        tellInfo = default;

        if (message.LogKind != XivChatType.TellIncoming)
            return false;

        var senderText = CleanSender(message.Sender.TextValue);
        if (string.IsNullOrWhiteSpace(senderText))
            return false;

        var (senderName, senderWorld) = SplitNameAndWorld(senderText);
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
        cleaned = cleaned.TrimStart('').Trim();
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
