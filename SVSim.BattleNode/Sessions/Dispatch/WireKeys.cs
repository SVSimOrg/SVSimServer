namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>
/// Single source of truth for the inbound-body (RawBody / orderList) wire-key strings the dispatch
/// path reads off the client's frames. These are the SENDER's JSON keys (mirroring the client's
/// <c>SendCardDataMaker</c> / <c>CardObj</c> serialization); a one-character typo at a read site
/// (<c>"isSelf"</c> vs <c>"IsSelf"</c>) silently degrades token resolution with no error, so every
/// read goes through a constant here instead of a repeated literal. Outbound keys stay on the
/// per-DTO <c>[JsonPropertyName]</c> attributes (already single-sourced there).
/// </summary>
internal static class WireKeys
{
    // Top-level inbound body keys
    public const string OrderList = "orderList";
    public const string KeyAction = "keyAction";
    public const string PlayIdx = "playIdx";
    public const string Type = "type";
    public const string TargetList = "targetList";
    public const string UList = "uList";

    // orderList op keys
    public const string Move = "move";
    public const string Add = "add";
    public const string Idx = "idx";
    public const string To = "to";
    public const string IsSelf = "isSelf";
    public const string Card = "card";
    public const string CardId = "cardId";
    public const string Candidates = "candidates";
    public const string IsChoice = "isChoice";
    public const string BaseIdx = "baseIdx";
    public const string Alter = "alter";
    public const string Spellboost = "spellboost";

    // keyAction.selectCard keys
    public const string SelectCard = "selectCard";
    public const string Open = "open";

    // targetList entry keys
    public const string TargetIdx = "targetIdx";

    // uList entry keys
    public const string IdxList = "idxList";
    public const string From = "from";
    public const string Skill = "skill";
    public const string Clan = "clan";
    public const string Cost = "cost";
    public const string SkillKeyCardIdx = "skillKeyCardIdx";
    public const string RandomTargetIdx = "randomTargetIdx";
    public const string IsInvoke = "isInvoke";
    public const string AttachTarget = "attachTarget";
}
