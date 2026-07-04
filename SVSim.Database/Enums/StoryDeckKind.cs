namespace SVSim.Database.Enums;

/// <summary>
/// Which story deck-select group a prebuilt deck belongs to. Build = the named story decks
/// (build_deck_list); Trial = archetype trial decks (trial_deck_list). Stored as int.
/// </summary>
public enum StoryDeckKind
{
    Build = 0,
    Trial = 1,
}
