namespace SVSim.BattleNode.Sessions;

public interface IBattleSessionStore
{
    /// <summary>Register a battle minted by the matching bridge, awaiting a WS connect.
    /// Returns false if a battle with the same id is already pending (caller should retry
    /// with a fresh id).</summary>
    bool TryRegisterPending(PendingBattle battle);

    /// <summary>Look up the pending battle. Returns null if not present.</summary>
    PendingBattle? TryGetPending(string battleId);

    /// <summary>
    /// Find a pending battle this viewer is a participant in (P1 or P2). Used by the
    /// HTTP-side <c>/ai_&lt;fmt&gt;/start</c> endpoint to retrieve the deck/cosmetic
    /// context the viewer registered at <c>do_matching</c> time — the <c>/start</c>
    /// request body carries no <c>deck_no</c> of its own. Returns null if the viewer
    /// has no pending battle (already consumed by WS connect, never registered, or
    /// evicted by timeout).
    /// </summary>
    PendingBattle? TryFindPendingForViewer(long viewerId);

    /// <summary>Mark a battle as no longer pending (e.g. on successful connect or explicit close).</summary>
    bool RemovePending(string battleId);
}
