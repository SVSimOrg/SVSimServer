namespace SVSim.BattleNode.Lifecycle;

/// <summary>
/// Default frame constants templated from TK2 prod captures, shared by the
/// server-authored battle-frame builders. Every value here originated in a real prod
/// frame in <c>data_dumps/captures/battle-traffic_tk2_regular.ndjson</c>; pulling them
/// out of <see cref="ServerBattleFrames"/> makes the magic numerics navigable. The shared effect
/// seed and the deck-shuffle/idxChangeSeed are now derived per-battle from a master seed (see
/// <see cref="BattleSeeds"/>) — only animation/UI constants remain here.
/// </summary>
internal static class BattleFrameDefaults
{
    // From frame[5] (BattleStart). Hardcoded; see spec §Deferred plumbing — sourcing these
    // from real per-viewer state needs a TK2 rank/battle-point tracker.
    public const string PlayerRank = "10";
    public const string PlayerBattlePoint = "6270";

    // Ready-frame spin. Prod shipped 243 (obfuscation base — the spin-rng audit proved ~99% of the
    // magnitude is non-gameplay). Our node is authority for BOTH clients; they each crank this on
    // their own shared _stableRandom, but the shadow engine ingests BOTH sides' Ready frames on ONE
    // stream — so a non-zero value double-cranks the shadow (243×2 = 486 vs each client's 243),
    // desynchronizing every subsequent StableRandom draw. Zero eliminates the offset; both clients
    // and the shadow all start at stream position 0.
    public const int ReadySpin = 0;

    /// <summary>
    /// Server-pushed Judge frame spin value. Prod varies per push (55, 175, 73, ...) — it's
    /// an animation seed, not a stateful value. Fixed at 100 here for test stability;
    /// the client's <c>JudgeOperation</c> doesn't read it.
    /// </summary>
    public const int OpponentJudgeSpin = 100;

    /// <summary>Spin value the PvP relay stamps on the Judge / OpponentTurnStart handover frames
    /// in the deterministic-turn slice. 0 = no animation seed; per-turn spin is deferred
    /// (see the real-spin design). The client self-generates its turn-open and doesn't read it.</summary>
    public const int DeterministicTurnSpin = 0;
}
