namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wire value of <c>type</c> on a keyAction entry — what kind of card-generating choice the play
/// is. Mirrors the client's <c>SendKeyActionDataManager.KeyActionType</c> exactly (same ordinals);
/// the client reads it back via <c>ConvertToInt(...)</c>, so it serializes as the underlying int
/// via <see cref="System.Text.Json.Serialization.JsonNumberEnumConverter{T}"/>. The node currently
/// relays only <see cref="Choice"/> and <see cref="HaveBeforeSkillChoice"/>
/// (<see cref="Bodies.KeyActionEntry"/> / <c>KnownListBuilder.StripKeyActionForOpponent</c>); the
/// rest are defined so the guard compares against named values instead of bare ints.
/// </summary>
public enum KeyActionType
{
    None = 0,
    Choice = 1,
    Accelerated = 2,
    Crystallize = 3,
    Fusion = 4,
    HaveBeforeSkillChoice = 5,
    BurialRate = 6,
    ChoiceEvolution = 7,
    ChoiceBrave = 8,
}
