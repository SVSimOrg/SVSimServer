namespace SVSim.BattleNode.Bridge;

/// <summary>
/// Per-battle player snapshot captured at do_matching time and replayed into the
/// server-authored frame lifecycle on WS connect. SVSim.BattleNode does not know how to build this — the HTTP-side
/// per-mode controller is the source. Snapshot semantics: cosmetic changes between matching
/// and WS connect have no effect on the in-battle render.
/// <para>FOOTGUN: as a record holding <see cref="SelfDeckCardIds"/> (an IReadOnlyList), the
/// synthesized value-equality is reference-based on that list — see <see cref="BattlePlayer"/>.
/// Don't use as a dict key / <c>Distinct()</c> operand without content equality.</para>
/// </summary>
public sealed record MatchContext(
    // Player's drafted deck — exactly 30 entries, idx 1..30 paired with the chosen cardIds
    // in the order this list provides them. Producer is responsible for the count.
    IReadOnlyList<long> SelfDeckCardIds,

    // Player class + leader (BattleStartSelfInfo)

    /// <summary>The player's class. Rendered onto the wire <c>classId</c> as <c>"1".."8"</c> via
    /// <see cref="CardClassWire.ToWireValue"/>; a closed set, so it's typed, not stringly.</summary>
    CardClass ClassId,

    /// <summary>Leader/skin id on the wire <c>charaId</c>. FREE-FORM, not a class enum: it's the
    /// equipped leader-skin id (e.g. <c>"5000123"</c>) when one is chosen, else the class ordinal
    /// (<c>"1".."8"</c>). Passed through verbatim — the node never interprets it.</summary>
    string CharaId,

    string CardMasterName,  // current card-master, e.g. "card_master_node_10015"

    // Player cosmetics (MatchedSelfInfo)

    /// <summary>Account region code, wire <c>country_code</c>. OPEN-ENDED account data (any value,
    /// possibly empty); the node never branches on it. <see cref="CountryCodes"/> names the values
    /// seen in captures.</summary>
    string CountryCode,

    string UserName,
    string SleeveId,
    string EmblemId,
    string DegreeId,
    int    FieldId,
    int    IsOfficial,      // 0 or 1

    // Battle-mode hint (the prod do_matching mode id). Named BattleModeId, NOT BattleType, to
    // avoid colliding with the <see cref="Sessions.BattleType"/> enum (Pvp/Bot) — a different axis.
    // Known values live in <see cref="BattleModes"/> (currently just TK2 == 11). Future modes add
    // their own constant.
    int    BattleModeId);
