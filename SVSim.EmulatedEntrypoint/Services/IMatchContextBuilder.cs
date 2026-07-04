using SVSim.BattleNode.Bridge;
using SVSim.Database.Enums;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Per-mode assembler for the battle-node <c>MatchContext</c>. Each multiplayer mode that
/// fronts a <c>do_matching</c> endpoint adds one method here that reads its mode-specific
/// state (TK2 run, current-deck pointer, open-room set_deck, ...) and produces a
/// <c>MatchContext</c> for the bridge.
/// </summary>
public interface IMatchContextBuilder
{
    /// <summary>
    /// Build a context from the viewer's active TK2 run + viewer cosmetics + config.
    /// Throws <see cref="ArenaTwoPickException"/> on missing run / incomplete draft.
    /// </summary>
    Task<MatchContext> BuildForTwoPickAsync(long viewerId);

    /// <summary>
    /// Build a context for a rank-battle viewer + format (rotation / unlimited) + the
    /// caller-selected deck number (from <c>do_matching</c>'s <c>deck_no</c>). Pulls the
    /// viewer's deck for that format/number + viewer cosmetics. Throws if the viewer has
    /// no deck at that slot.
    /// </summary>
    Task<MatchContext> BuildForRankBattleAsync(long viewerId, Format format, int deckNo);

    /// <summary>
    /// Build a context for an Arena Colosseum bracket match — reads the active
    /// <c>ViewerArenaColosseumRun</c>'s registered deck slot (single-deck v1) via
    /// <c>IDeckRepository.GetDeck</c> (NOT viewer-graph traversal per
    /// <c>project_ef_nav_include_pitfall</c>). Throws when the run is missing or no deck
    /// has been registered yet.
    /// </summary>
    Task<MatchContext> BuildForColosseumAsync(long viewerId);
}
