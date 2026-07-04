namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wire value of <c>turnState</c> on BattleStart / TurnEnd frames: which side acts first.
/// The client reads it via <c>Convert.ToInt32</c> (<c>RealTimeNetworkAgent.cs</c> "turnState"
/// case) into <c>NetworkUserInfoData.TurnState</c>, so it serializes as the underlying int via
/// <see cref="System.Text.Json.Serialization.JsonNumberEnumConverter{T}"/>.
/// </summary>
public enum TurnState
{
    /// <summary>This side takes the first turn.</summary>
    First = 0,

    /// <summary>This side takes the second turn.</summary>
    Second = 1,
}
