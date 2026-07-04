namespace SVSim.BattleNode.Bridge;

/// <summary>
/// Known values for <see cref="MatchContext.CountryCode"/>. NOT a closed set — the field is the
/// account's region code copied verbatim from viewer data (any value, possibly empty), and the node
/// never branches on it. These constants just name the values seen in the prod captures so test
/// fixtures and docs aren't sprinkled with bare <c>"KOR"</c>/<c>"JPN"</c> literals.
/// </summary>
public static class CountryCodes
{
    public const string Korea = "KOR";
    public const string Japan = "JPN";
}
