using System.Collections.Immutable;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Lifecycle;

/// <summary>
/// Server-authored battle frames pushed to the client during match setup and teardown
/// (Matched / BattleStart / Deal / Swap response / Ready) plus the post-mulligan hand
/// computation. Used by every battle mode's handshake/mulligan dispatch arms. Hardcoded
/// values are templated from the TK2 prod captures (battle-traffic_tk2_*.ndjson); see
/// <see cref="BattleFrameDefaults"/> for provenance.
/// </summary>
public static class ServerBattleFrames
{
    /// <summary>
    /// Viewer id we present as the opponent on every server-authored opponent push. Out-of-range
    /// vs. real viewer ids so it can't collide with a real account in the auth pipeline.
    /// </summary>
    public const long FakeOpponentViewerId = 999_999_999L;

    public static MsgEnvelope BuildMatched(
        MatchContext selfCtx, MatchContext oppoCtx,
        long selfViewerId, long oppoViewerId,
        string battleId, int seed, IReadOnlyList<long> selfDeckOrder) =>
        EnvelopeForPush(NetworkBattleUri.Matched,
            new MatchedBody(
                SelfInfo: new MatchedSelfInfo(
                    CountryCode: selfCtx.CountryCode,
                    UserName: selfCtx.UserName,
                    SleeveId: selfCtx.SleeveId,
                    EmblemId: selfCtx.EmblemId,
                    DegreeId: selfCtx.DegreeId,
                    FieldId: selfCtx.FieldId,
                    IsOfficial: selfCtx.IsOfficial != 0,
                    OppoId: (int)oppoViewerId,
                    Seed: seed),
                OppoInfo: new MatchedOppoInfo(
                    CountryCode: oppoCtx.CountryCode,
                    UserName: oppoCtx.UserName,
                    SleeveId: oppoCtx.SleeveId,
                    EmblemId: oppoCtx.EmblemId,
                    DegreeId: oppoCtx.DegreeId,
                    FieldId: oppoCtx.FieldId,
                    IsOfficial: oppoCtx.IsOfficial != 0,
                    OppoId: (int)selfViewerId,
                    Seed: seed,
                    OppoDeckCount: oppoCtx.SelfDeckCardIds.Count),
                SelfDeck: BuildPlayerDeck(selfDeckOrder)),
            bid: battleId);

    public static MsgEnvelope BuildBattleStart(
        MatchContext selfCtx, MatchContext oppoCtx, long selfViewerId, TurnState turnState) =>
        EnvelopeForPush(NetworkBattleUri.BattleStart,
            new BattleStartBody(
                TurnState: turnState,    // First = this side goes first, Second = second. Caller decides.
                BattleModeId: selfCtx.BattleModeId,
                SelfInfo: new BattleStartSelfInfo(
                    Rank: BattleFrameDefaults.PlayerRank,
                    BattlePoint: BattleFrameDefaults.PlayerBattlePoint,
                    ClassId: selfCtx.ClassId.ToWireValue(),
                    CharaId: selfCtx.CharaId,
                    CardMasterName: selfCtx.CardMasterName),
                OppoInfo: new BattleStartOppoInfo(
                    // Rank/IsMasterRank/BattlePoint/MasterPoint stay hardcoded —
                    // PvP rank tracking is deferred (per spec § Out of scope).
                    Rank: "1",
                    IsMasterRank: "0",
                    BattlePoint: 0,
                    MasterPoint: "0",
                    ClassId: oppoCtx.ClassId.ToWireValue(),
                    CharaId: oppoCtx.CharaId,
                    CardMasterName: oppoCtx.CardMasterName)));

    public static MsgEnvelope BuildDeal() =>
        EnvelopeForPush(NetworkBattleUri.Deal,
            new DealBody(
                Self: new[] { new PosIdx(0, 1), new PosIdx(1, 2), new PosIdx(2, 3) },
                Oppo: new[] { new PosIdx(0, 1), new PosIdx(1, 2), new PosIdx(2, 3) }));

    /// <summary>
    /// Initial 3-card hand idxs from <see cref="BuildDeal"/>. Each position in this array
    /// is one card; the value is the card's deck idx. <see cref="ImmutableArray{T}"/> enforces
    /// the "read-only constant" contract at the type level — callers cannot mutate it, even
    /// accidentally (the prior <c>long[]</c> allowed in-place modification by anyone with the
    /// field reference).
    /// </summary>
    private static readonly ImmutableArray<long> InitialHand = ImmutableArray.Create<long>(1, 2, 3);

    /// <summary>
    /// Compute the player's hand after a mulligan. For every idx in <paramref name="swapIndices"/>
    /// that is currently in the hand, replace it with the next unused deck idx (the first idx past
    /// the opening hand — <see cref="InitialHand"/> is 1-based and contiguous, so that's
    /// <c>InitialHand.Length + 1</c>). Positions of kept cards are preserved.
    /// </summary>
    public static long[] ComputeHandAfterSwap(IReadOnlyList<long> swapIndices)
    {
        var hand = InitialHand.ToArray();
        var nextDeckIdx = (long)(InitialHand.Length + 1);
        for (var pos = 0; pos < hand.Length; pos++)
        {
            if (swapIndices.Contains(hand[pos]))
            {
                hand[pos] = nextDeckIdx++;
            }
        }
        return hand;
    }

    public static MsgEnvelope BuildSwapResponse(IReadOnlyList<long> hand) =>
        EnvelopeForPush(NetworkBattleUri.Swap,
            new SwapResponseBody(Self: BuildPosIdxList(hand)));

    /// <summary>Non-interactive opponent (Bot/AI): oppo is the placeholder
    /// <see cref="InitialHand"/>.</summary>
    public static MsgEnvelope BuildReady(IReadOnlyList<long> hand, int idxChangeSeed) =>
        BuildReady(hand, InitialHand, idxChangeSeed);

    /// <summary>Both hands known (the mulligan barrier supplies the opponent's
    /// post-mulligan hand).</summary>
    public static MsgEnvelope BuildReady(IReadOnlyList<long> selfHand, IReadOnlyList<long> oppoHand, int idxChangeSeed) =>
        EnvelopeForPush(NetworkBattleUri.Ready,
            new ReadyBody(
                Self: BuildPosIdxList(selfHand),
                Oppo: BuildPosIdxList(oppoHand),
                IdxChangeSeed: idxChangeSeed,
                Spin: BattleFrameDefaults.ReadySpin));

    private static IReadOnlyList<PosIdx> BuildPosIdxList(IReadOnlyList<long> hand)
    {
        var list = new List<PosIdx>(hand.Count);
        for (var pos = 0; pos < hand.Count; pos++)
        {
            list.Add(new PosIdx(Pos: pos, Idx: (int)hand[pos]));
        }
        return list;
    }

    private static IReadOnlyList<DeckCardRef> BuildPlayerDeck(IReadOnlyList<long> cardIds)
    {
        var deck = new List<DeckCardRef>(cardIds.Count);
        for (var i = 0; i < cardIds.Count; i++)
        {
            deck.Add(new DeckCardRef(Idx: i + 1, CardId: cardIds[i]));
        }
        return deck;
    }

    private static MsgEnvelope EnvelopeForPush(NetworkBattleUri uri, IMsgBody body, string? bid = null) =>
        new(uri,
            ViewerId: FakeOpponentViewerId,
            Uuid: WireConstants.ServerUuid,
            Bid: bid,
            RetryAttempt: 0,
            Cat: EmitCategory.Battle,
            PubSeq: null,
            PlaySeq: null,
            Body: body);
}
