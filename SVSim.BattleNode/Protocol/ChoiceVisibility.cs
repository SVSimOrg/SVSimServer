namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wire value of <c>open</c> on a choice/Discover <c>selectCard</c>: whether the pick is revealed.
/// The client emits it as <c>selectCardIsOpen ? 1 : 0</c> (<c>SendKeyActionDataManager.cs</c>);
/// the node uses it to decide whether to strip the pick for the opponent (<c>Hidden</c> = strip).
/// Serializes as the underlying int via
/// <see cref="System.Text.Json.Serialization.JsonNumberEnumConverter{T}"/>.
/// </summary>
public enum ChoiceVisibility
{
    /// <summary>Hidden draw-to-hand pick — the chosen card stays secret until played.</summary>
    Hidden = 0,

    /// <summary>Visible board choice — the pick is revealed immediately.</summary>
    Open = 1,
}
