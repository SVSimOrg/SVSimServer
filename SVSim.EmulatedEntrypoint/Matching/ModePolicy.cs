namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// Per-mode pairing policy. TK2 is PvP-only; rotation/unlimited rank battles
/// can fall back to an AI battle after a configurable threshold. Future modes
/// add rows in DI registration.
/// </summary>
public enum PolicyKind
{
    /// <summary>Pair PvP; if no partner arrives, keep waiting indefinitely (modulo stale eviction).</summary>
    PvpOnly,
    /// <summary>Pair PvP if a partner arrives within the threshold; otherwise fall back to a Bot battle.</summary>
    PvpFirstThenAiFallback,
}

public sealed record ModePolicy(string Mode, PolicyKind Kind);

/// <summary>
/// DI singleton. Holds the per-mode policy lookup. Unknown modes default to
/// <see cref="PolicyKind.PvpOnly"/> (safest — never accidentally fall through to AI
/// for a mode whose policy hasn't been wired).
/// </summary>
public sealed class ModePolicyRegistry
{
    private readonly Dictionary<string, ModePolicy> _byMode;

    public ModePolicyRegistry(IEnumerable<ModePolicy> policies)
    {
        // Last-wins on duplicate keys — documented in tests.
        _byMode = new Dictionary<string, ModePolicy>();
        foreach (var p in policies) _byMode[p.Mode] = p;
    }

    public ModePolicy For(string mode) =>
        _byMode.TryGetValue(mode, out var p) ? p : new ModePolicy(mode, PolicyKind.PvpOnly);
}
