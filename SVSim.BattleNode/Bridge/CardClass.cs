namespace SVSim.BattleNode.Bridge;

/// <summary>
/// A Shadowverse class (craft). The wire carries it as the stringified ordinal (<c>"1".."8"</c> on
/// the <c>classId</c> field); this enum replaces that stringly-typed value on
/// <see cref="MatchContext.ClassId"/> so the legal set lives in the type, not a trailing comment.
/// <see cref="None"/> covers an unset / placeholder context. Use <see cref="CardClassWire.ToWireValue"/>
/// to render the wire string.
/// </summary>
public enum CardClass
{
    None = 0,
    Forestcraft = 1,
    Swordcraft = 2,
    Runecraft = 3,
    Dragoncraft = 4,
    Shadowcraft = 5,
    Bloodcraft = 6,
    Havencraft = 7,
    Portalcraft = 8,
}

/// <summary>Wire rendering for <see cref="CardClass"/>.</summary>
public static class CardClassWire
{
    /// <summary>The <c>classId</c> wire value — the class ordinal as a string (<c>"1".."8"</c>,
    /// <c>"0"</c> for <see cref="CardClass.None"/>), matching what the client sends/expects.</summary>
    public static string ToWireValue(this CardClass cardClass) => ((int)cardClass).ToString();
}
