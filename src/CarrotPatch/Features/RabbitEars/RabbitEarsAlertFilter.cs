using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

namespace CarrotPatch.Features.RabbitEars;

public sealed class RabbitEarsAlertFilter
{
    private readonly Configuration configuration;
    private readonly IPartyList partyList;

    public RabbitEarsAlertFilter(
        Configuration configuration,
        IPartyList partyList)
    {
        this.configuration = configuration;
        this.partyList = partyList;
    }

    public bool ShouldSuppressAlert(
        string senderName,
        string? senderWorld,
        IGameObject? gameObject,
        IGameObject? localPlayer)
    {
        if (this.configuration.SuppressAlertsFromSelf && IsSelf(senderName, gameObject, localPlayer))
            return true;

        if (this.configuration.SuppressAlertsFromPartyMembers && this.IsPartyMember(senderName, gameObject))
            return true;

        if (this.configuration.SuppressAlertsFromAllianceMembers && this.IsAllianceMember(senderName, gameObject))
            return true;

        return false;
    }

    public static bool IsSamePlayerName(string firstName, string secondName)
        => string.Equals(
            TellParserCore.NormalizeName(firstName),
            TellParserCore.NormalizeName(secondName),
            StringComparison.OrdinalIgnoreCase);

    private static bool IsSelf(string senderName, IGameObject? gameObject, IGameObject? localPlayer)
    {
        if (localPlayer is null)
            return false;

        if (gameObject is not null && gameObject.GameObjectId == localPlayer.GameObjectId)
            return true;

        return IsSamePlayerName(senderName, localPlayer.Name.TextValue);
    }

    private bool IsPartyMember(string senderName, IGameObject? gameObject)
    {
        if (HasStatusFlag(gameObject, StatusFlags.PartyMember))
            return true;

        return this.IsInPartyList(senderName, gameObject, includeAlliance: false);
    }

    private bool IsAllianceMember(string senderName, IGameObject? gameObject)
    {
        if (HasStatusFlag(gameObject, StatusFlags.AllianceMember))
            return true;

        return this.IsInPartyList(senderName, gameObject, includeAlliance: true);
    }

    private bool IsInPartyList(string senderName, IGameObject? gameObject, bool includeAlliance)
    {
        var length = Math.Max(0, this.partyList.Length);
        for (var index = 0; index < length; index++)
        {
            var member = this.partyList[index];
            if (member is not null && this.IsPartyListMemberMatch(member, senderName, gameObject))
                return true;
        }

        if (!includeAlliance || !this.partyList.IsAlliance)
            return false;

        for (var index = 0; index < 24; index++)
        {
            var address = this.partyList.GetAllianceMemberAddress(index);
            if (address == IntPtr.Zero)
                continue;

            var member = this.partyList.CreateAllianceMemberReference(address);
            if (member is not null && this.IsPartyListMemberMatch(member, senderName, gameObject))
                return true;
        }

        return false;
    }

    private bool IsPartyListMemberMatch(object member, string senderName, IGameObject? gameObject)
    {
        var objectId = GetUInt64Property(member, "ObjectId");
        if (gameObject is not null && objectId is not null && objectId.Value != 0 && objectId.Value == gameObject.GameObjectId)
            return true;

        var memberName = GetStringProperty(member, "Name");
        return !string.IsNullOrWhiteSpace(memberName) && IsSamePlayerName(senderName, memberName);
    }

    private static bool HasStatusFlag(IGameObject? gameObject, StatusFlags statusFlag)
        => gameObject is ICharacter character && character.StatusFlags.HasFlag(statusFlag);

    private static ulong? GetUInt64Property(object value, string propertyName)
    {
        var rawValue = value.GetType().GetProperty(propertyName)?.GetValue(value);
        if (rawValue is null)
            return null;

        try
        {
            return Convert.ToUInt64(rawValue);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetStringProperty(object value, string propertyName)
        => Convert.ToString(value.GetType().GetProperty(propertyName)?.GetValue(value));
}
