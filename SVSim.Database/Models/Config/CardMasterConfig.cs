namespace SVSim.Database.Models.Config;

/// <summary>
/// Tunables for serving the card-master payload at <c>POST /immutable_data/card_master</c>.
/// V1 ships the captured prod blob verbatim; the hash here pins to that capture's prod hash
/// so any client whose <c>cardmaster/card_master_1</c> was seeded from a prod dump skips
/// re-download.
/// </summary>
[ConfigSection("CardMasterConfig")]
public class CardMasterConfig
{
    /// <summary>
    /// Emitted on <c>/load/index</c> response's inner <c>data.card_master_hash</c> when the
    /// request's <c>card_master_hash</c> body field doesn't match this value. Client treats
    /// the string as opaque (presence-only check, value is echoed back unchanged on the next
    /// <c>/immutable_data/card_master</c> request). Format <c>&lt;sha1 hex&gt;:&lt;CardMasterId&gt;</c>.
    /// <para>
    /// Default value is the prod-captured hash paired with the blob at
    /// <c>SVSim.EmulatedEntrypoint/Data/card_master_2026-06-03.txt</c>. When swapping the
    /// blob, update this string in lockstep — the hash is otherwise free-form (the client
    /// never validates its structure).
    /// </para>
    /// </summary>
    public string CurrentHash { get; set; } = "94b5c44edc51ff76c0af8fcc894af12f979dd38c:1";

    /// <summary>
    /// Kill switch. When false, <c>/immutable_data/card_master</c> returns 503 and
    /// <c>/load/index</c> never emits <c>card_master_hash</c>. Useful for shaving the 1.27 MB
    /// download out of test-only deployments where the test viewers already have a seeded
    /// local cache.
    /// </summary>
    public bool EnableServing { get; set; } = true;

    public static CardMasterConfig ShippedDefaults() => new();
}
