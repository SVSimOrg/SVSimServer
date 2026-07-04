namespace SVSim.BattleNode.Lifecycle;

/// <summary>
/// Deterministic per-battle seed derivation. Given one random master seed (chosen once per battle
/// on <see cref="Sessions.Dispatch.BattleSessionState"/>), derives every RNG value the node hands
/// the clients: the shared effect seed (Matched.seed), each side's deck-shuffle RNG seed, and each
/// side's Ready.idxChangeSeed.
///
/// IMPORTANT: uses a fixed splitmix64-style bit-mix, NOT System.HashCode / string.GetHashCode
/// (those are randomized per process). Stability across process runs is what makes "same master
/// seed reproduces the same battle" — the foundation of replay — actually hold.
/// </summary>
internal static class BattleSeeds
{
    /// <summary>Shared effect-RNG seed; identical for both sides (it seeds the synced stream).</summary>
    public static int Stable(int master) => Derive(master, "stable");

    /// <summary>Per-side Ready.idxChangeSeed (client XorShift for mid-battle card-into-deck).</summary>
    public static int IdxChange(int master, long viewerId) => Derive(master, "idx", viewerId);

    /// <summary>Per-side deck-shuffle RNG seed (node-side Fisher–Yates).</summary>
    public static int DeckShuffle(int master, long viewerId) => Derive(master, "deck", viewerId);

    /// <summary>Derive a stable non-negative int from (master, tag, discriminator). Pure arithmetic
    /// — reproducible across process runs and platforms.</summary>
    public static int Derive(int master, string tag, long disc = 0)
    {
        ulong h = Mix((uint)master);
        foreach (char c in tag) h = Mix(h ^ c);
        h = Mix(h ^ (ulong)disc);
        return (int)(h & 0x7FFFFFFFUL);
    }

    private static ulong Mix(ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        return x ^ (x >> 31);
    }
}
