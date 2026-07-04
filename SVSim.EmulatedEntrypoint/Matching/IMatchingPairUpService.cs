using SVSim.BattleNode.Bridge;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// Minimal in-process matching queue stand-in. The proper queue API is a separate
/// spec; this is enough to actually pair two viewers polling /do_matching on the
/// same mode.
/// </summary>
public interface IMatchingPairUpService
{
    /// <summary>
    /// Try to pair the calling viewer with an already-waiting partner.
    /// Returns the resolved <see cref="PairUpResult"/> when a partner was found
    /// (either this call paired with a waiter, or a previous pairing's result is
    /// still cached for this viewer). Returns null if this viewer is the first
    /// arriver and should be parked (caller returns 3002 RETRY).
    /// </summary>
    Task<PairUpResult?> TryPairAsync(string mode, BattlePlayer player, CancellationToken ct);
}

/// <summary>
/// A resolved pair-up for a single caller.
/// <para>
/// <see cref="IsOwner"/> distinguishes the "original waiter" (first arriver, whose
/// cached result is being consumed on a follow-up poll — maps to wire matching_state
/// 3007 = RC_BATTLE_MATCHING_SUCCEEDED_OWNER) from the "joiner" (second arriver,
/// whose poll triggered the pair — maps to 3004 = RC_BATTLE_MATCHING_SUCCEEDED).
/// </para>
/// <para>
/// The split mirrors prod's TK2 wire flow (waiter sees 3007, joiner sees 3004) but
/// is observationally inert in the public-matching code path: the client's
/// <c>Matching</c> class writes <c>isOwner</c> from the response into a field that
/// nothing else in TK2/ranked reads. We send the split anyway for prod fidelity in
/// case a future flow (rematch UI, private rooms grafted on top) starts consuming it.
/// </para>
/// <para>
/// <see cref="IsAiFallback"/> is true when the resolution came from the
/// <c>PvpFirstThenAiFallback</c> policy expiring its threshold — caller is paired
/// with a silent NoOpBotParticipant. Maps to matching_state 3011
/// (AI_BATTLE_MATCHING_SUCCEEDED). Always false for PvpOnly modes (TK2).
/// </para>
/// </summary>
public sealed record PairUpResult(PendingMatch Match, bool IsOwner, bool IsAiFallback);
