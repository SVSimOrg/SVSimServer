using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions;

namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>Mutable per-session state shared across frame handlers. The mulligan barrier's
/// post-swap hands, plus the per-side idx->cardId map used as the FALLBACK identity source for the
/// opponent-facing <c>knownList</c>. As of M-HC-4f the played card's identity is ENGINE-first — the handler
/// reads <c>SessionBattleEngine.PlayedCardId</c> and uses this map only as the fallback (non-engine session,
/// or a token case the engine doesn't resolve at a wire idx). The map still holds deck cards (seeded from the
/// shuffled deck) and the wire-mined generated/choice/copy/cross-side token identities (recorded via
/// <see cref="RecordToken"/>) — those token cases remain wire-mined pending an engine-read proof
/// (TODO(M-HC-4f) in <c>PlayActionsHandler</c>).</summary>
internal sealed class BattleSessionState
{
    /// <summary>The one random value chosen per battle. Every per-battle RNG (shared effect seed,
    /// each side's deck shuffle + idxChangeSeed) derives from it via <see cref="BattleSeeds"/>.
    /// Logged at session start so a battle's randomness is reproducible (future replay).</summary>
    public int MasterSeed { get; }

    /// <param name="masterSeed">Test hook — production uses the random default.</param>
    public BattleSessionState(int? masterSeed = null) =>
        MasterSeed = masterSeed ?? Random.Shared.Next();

    private readonly Dictionary<IBattleParticipant, IReadOnlyList<long>> _shuffledDecks = new();

    /// <summary>This side's deck, shuffled deterministically from <see cref="MasterSeed"/>
    /// (Fisher–Yates). Cached per side. Both the wire selfDeck (Matched) and the reveal map
    /// (<see cref="GetOrSeedDeckMap"/>) read this, so they share one shuffled order.</summary>
    public IReadOnlyList<long> GetShuffledDeck(IBattleParticipant side)
    {
        if (_shuffledDecks.TryGetValue(side, out var cached)) return cached;
        var deck = side.Context.SelfDeckCardIds.ToArray();
        var rng = new Random(BattleSeeds.DeckShuffle(MasterSeed, side.ViewerId));
        for (var i = deck.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
        _shuffledDecks[side] = deck;
        return deck;
    }

    public SessionLifecycle Lifecycle { get; set; } = SessionLifecycle.Active;
    public Dictionary<IBattleParticipant, long[]> PostSwapHands { get; } = new();

    /// <summary>Per-side idx->cardId, seeded lazily from <see cref="MatchContext.SelfDeckCardIds"/>.
    /// Holds deck cards (idx 1..deckCount, seeded) and generated tokens (idx>deckCount, recorded
    /// from add ops via <see cref="RecordToken"/>).</summary>
    public Dictionary<IBattleParticipant, Dictionary<int, long>> IdxToCardId { get; } = new();

    /// <summary>The sender's idx->cardId map, seeding it from its <see cref="GetShuffledDeck"/> order on
    /// first use. Deck idx = position+1 in the shuffled order, so entry (i+1) -> shuffledDeck[i]. The
    /// wire selfDeck (Matched) is built from the same shuffled order, so the two agree.</summary>
    public IReadOnlyDictionary<int, long> GetOrSeedDeckMap(IBattleParticipant side)
    {
        if (!IdxToCardId.TryGetValue(side, out var map))
        {
            map = new Dictionary<int, long>();
            var deck = GetShuffledDeck(side);
            for (var i = 0; i < deck.Count; i++) map[i + 1] = deck[i];
            IdxToCardId[side] = map;
        }
        return map;
    }

    /// <summary>Record a generated token's identity into the side's idx->cardId map (the same map
    /// that holds deck cards). Mined from the sender's <c>orderList</c> <c>add</c> ops by
    /// <see cref="KnownListBuilder.MineAddOps"/>; surfaced later by <c>BuildPlayedCard</c> when the
    /// token is the played card. Deck idxs (1..deckCount) and token idxs (&gt;deckCount) don't
    /// collide — the client allocates token idxs after the deck.</summary>
    public void RecordToken(IBattleParticipant side, int idx, long cardId)
    {
        GetOrSeedDeckMap(side);            // ensure the per-side map exists (deck-seeded)
        IdxToCardId[side][idx] = cardId;   // overwrite-on-conflict: latest identity wins
    }

    /// <summary>Mine generated-token identities from a sender's <c>orderList</c> <c>add</c> ops and
    /// record each into the correct side's map. <c>isSelf:1</c> → the sender's own token (<paramref
    /// name="from"/>); <c>isSelf:0</c> → a cross-side gift living at that idx in the OPPONENT's index
    /// space (<paramref name="other"/>) — <c>isSelf</c> is the sender's perspective tag on
    /// <c>CardObj.IsPlayer</c> (RegisterToken.cs:22), and a card has a single <c>CardObj.Index</c>, so
    /// the gifted idx is the same slot in the recipient's own map (the one consulted when the recipient
    /// later plays it). Shared by <c>PlayActionsHandler</c> and <c>EchoHandler</c> — an Echo's orderList
    /// carries the same add-op shape (<c>SendCardDataMaker.MakeEchoData</c>), so both mine identically;
    /// Echo is mined but never relayed.</summary>
    public void RecordTokensFrom(IBattleParticipant from, IBattleParticipant other, object? orderList)
    {
        // TRUST: isSelf is the SENDER's own perspective flag and idx is unbounded, while RecordToken
        // overwrites-on-conflict. A buggy/malicious sender could pass isSelf:0 with a deck-range idx to
        // rewrite the OPPONENT's card identity at a seeded slot. Acceptable for the current trusted-LAN
        // relay; if peers ever become untrusted, gate on `idx > deckCount` here (generated tokens always
        // allocate past the deck) so a sender can't forge over seeded deck cards.
        foreach (var (idx, cardId, isSelf) in KnownListBuilder.MineAddOps(orderList))
            RecordToken(isSelf == CardOwner.Self ? from : other, idx, cardId);
    }

    /// <summary>Mine + record choice/Discover-token picks (<see cref="KnownListBuilder.MineChoicePicks"/>)
    /// into the correct side's map, by the same <c>isSelf</c> routing as <see cref="RecordTokensFrom"/>.
    /// The chosen cardId rides the generating send's <c>keyAction.selectCard</c> (not the orderList add
    /// op, which carries candidates only); recorded regardless of the choice's <c>open</c> visibility —
    /// an unplayed idx is never queried, so a stray record is harmless.</summary>
    public void RecordChoicePicksFrom(IBattleParticipant from, IBattleParticipant other, object? orderList, object? keyAction)
    {
        foreach (var (idx, cardId, isSelf) in KnownListBuilder.MineChoicePicks(orderList, keyAction))
            RecordToken(isSelf == CardOwner.Self ? from : other, idx, cardId);
    }

    /// <summary>Mine + record copy/clone-token identities (<see cref="KnownListBuilder.MineCopyTokens"/>)
    /// into the correct side's map. A copy's source lives at <c>baseIdx</c> in the actor's own index
    /// space, so the resolution side == the record side, both selected by the same <c>isSelf</c> routing
    /// as <see cref="RecordTokensFrom"/>. Passing the LIVE per-side maps (via
    /// <see cref="GetOrSeedDeckMap"/>, not snapshots) lets a copy that references a plain/choice token
    /// added earlier THIS frame resolve — provided this runs AFTER
    /// <see cref="RecordTokensFrom"/>/<see cref="RecordChoicePicksFrom"/> (the handler orders it last).
    /// Seeding both maps up front matters because a copy-only frame (no concrete/choice add) would never
    /// have hit <see cref="RecordToken"/> yet, leaving the maps unseeded.</summary>
    public void RecordCopyTokensFrom(IBattleParticipant from, IBattleParticipant other, object? orderList)
    {
        var selfMap = GetOrSeedDeckMap(from);
        var otherMap = GetOrSeedDeckMap(other);
        foreach (var (idx, cardId, isSelf) in KnownListBuilder.MineCopyTokens(orderList, selfMap, otherMap))
            RecordToken(isSelf == CardOwner.Self ? from : other, idx, cardId);
    }
}
