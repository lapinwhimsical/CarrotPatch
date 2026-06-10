using Dalamud.Game.Chat;
using Dalamud.Game.Text;

namespace CarrotPatch.Features.RabbitEars;

public static class TellParser
{
    public static bool TryParseIncomingTell(IChatMessage message, string? localPlayerName, out TellInfo tellInfo)
        => TryParseIncomingTell(message.LogKind, message.Sender.TextValue, localPlayerName, out tellInfo);

    public static bool TryParseIncomingTell(XivChatType logKind, string senderText, string? localPlayerName, out TellInfo tellInfo)
        => TellParserCore.TryParseIncomingTell(logKind == XivChatType.TellIncoming, senderText, localPlayerName, out tellInfo);

    public static string NormalizeName(string value)
        => TellParserCore.NormalizeName(value);
}
