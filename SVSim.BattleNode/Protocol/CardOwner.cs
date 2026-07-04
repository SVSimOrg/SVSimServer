namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wire value of the actor-relative <c>isSelf</c> flag on relayed lists (<c>targetList</c>,
/// <c>uList</c>): whose side a referenced card belongs to, from the SENDER's perspective. The node
/// forwards it verbatim — no perspective flip (bullet-3 audit F2). The client reads it via
/// <c>ConvertToInt(...) == 1</c> (<c>NetworkBattleReceiver.cs</c>), so it serializes as the
/// underlying int via <see cref="System.Text.Json.Serialization.JsonNumberEnumConverter{T}"/>.
/// </summary>
public enum CardOwner
{
    /// <summary>Card belongs to the opponent of the sender.</summary>
    Opponent = 0,

    /// <summary>Card belongs to the sender.</summary>
    Self = 1,
}
