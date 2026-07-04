using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.UnitTests.BattleNode.Lifecycle;

[TestFixture]
public class ServerBattleFramesTests
{
    [Test]
    public void BuildMatched_PutsOppoIdInSelfInfoEqualToTheRealOpponentVid()
    {
        var env = ServerBattleFrames.BuildMatched(FixtureCtx(), FakeOpponentCtx(),
            selfViewerId: 906243102, oppoViewerId: 847666884,
            battleId: "b", seed: 17_548_138, selfDeckOrder: FixtureCtx().SelfDeckCardIds);

        Assert.That(env.Uri, Is.EqualTo(NetworkBattleUri.Matched));
        var body = (MatchedBody)env.Body;
        Assert.That(body.SelfInfo.OppoId, Is.EqualTo(847666884));
        Assert.That(body.OppoInfo.OppoId, Is.EqualTo(906243102));
        Assert.That(env.Bid, Is.EqualTo("b"));
    }

    [Test]
    public void BuildMatched_ContainsThirtyCardSelfDeck()
    {
        var env = ServerBattleFrames.BuildMatched(FixtureCtx(), FakeOpponentCtx(), 1, 2, "b", 17_548_138, FixtureCtx().SelfDeckCardIds);
        var body = (MatchedBody)env.Body;
        Assert.That(body.SelfDeck.Count, Is.EqualTo(30));
    }

    [Test]
    public void BuildMatched_deck_idxs_pair_1to30_with_context_card_ids()
    {
        var draftedDeck = Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList();
        var env = ServerBattleFrames.BuildMatched(FixtureCtx(draftedDeck), FakeOpponentCtx(), 1, 2, "b", 17_548_138, draftedDeck);
        var body = (MatchedBody)env.Body;

        for (int i = 0; i < 30; i++)
        {
            Assert.That(body.SelfDeck[i].Idx, Is.EqualTo(i + 1),
                $"slot {i}: idx should be 1-based position");
            Assert.That(body.SelfDeck[i].CardId, Is.EqualTo(200_000_000L + i + 1),
                $"slot {i}: cardId should be the drafted card");
        }
    }

    [Test]
    public void BuildMatched_selfInfo_cosmetics_flow_from_context()
    {
        var ctx = FixtureCtx() with
        {
            CountryCode = "JPN", UserName = "Drafter", SleeveId = "999",
            EmblemId = "888", DegreeId = "777", FieldId = 42, IsOfficial = 1,
        };

        var env = ServerBattleFrames.BuildMatched(ctx, FakeOpponentCtx(), 1, 2, "b", 17_548_138, ctx.SelfDeckCardIds);
        var body = (MatchedBody)env.Body;

        Assert.That(body.SelfInfo.CountryCode, Is.EqualTo("JPN"));
        Assert.That(body.SelfInfo.UserName, Is.EqualTo("Drafter"));
        Assert.That(body.SelfInfo.SleeveId, Is.EqualTo("999"));
        Assert.That(body.SelfInfo.EmblemId, Is.EqualTo("888"));
        Assert.That(body.SelfInfo.DegreeId, Is.EqualTo("777"));
        Assert.That(body.SelfInfo.FieldId, Is.EqualTo(42));
        Assert.That(body.SelfInfo.IsOfficial, Is.True);
    }

    [Test]
    public void BuildBattleStart_HasTurnStateZero_AndUsesContextBattleModeId()
    {
        var env = ServerBattleFrames.BuildBattleStart(FixtureCtx(), FakeOpponentCtx(), selfViewerId: 1, turnState: TurnState.First);
        var body = (BattleStartBody)env.Body;
        Assert.That(body.TurnState, Is.EqualTo(TurnState.First));
        Assert.That(body.BattleModeId, Is.EqualTo(BattleModes.TakeTwo));
    }

    [Test]
    public void BuildBattleStart_class_chara_cardMaster_battleModeId_flow_from_context()
    {
        var ctx = FixtureCtx() with
        {
            ClassId = CardClass.Havencraft, CharaId = "5000123",
            CardMasterName = "card_master_test_v2",
            BattleModeId = 42,
        };

        var env = ServerBattleFrames.BuildBattleStart(ctx, FakeOpponentCtx(), selfViewerId: 1, turnState: TurnState.First);
        var body = (BattleStartBody)env.Body;

        Assert.That(body.SelfInfo.ClassId, Is.EqualTo("7"));
        Assert.That(body.SelfInfo.CharaId, Is.EqualTo("5000123"));
        Assert.That(body.SelfInfo.CardMasterName, Is.EqualTo("card_master_test_v2"));
        Assert.That(body.BattleModeId, Is.EqualTo(42));
    }

    [Test]
    public void BuildDeal_HasThreeSelfAndThreeOppoEntries()
    {
        var env = ServerBattleFrames.BuildDeal();
        var body = (DealBody)env.Body;
        Assert.That(body.Self.Count, Is.EqualTo(3));
        Assert.That(body.Oppo.Count, Is.EqualTo(3));
    }

    [Test]
    public void ComputeHandAfterSwap_NoSwap_ReturnsInitialHand()
    {
        var hand = ServerBattleFrames.ComputeHandAfterSwap(Array.Empty<long>());
        Assert.That(hand, Is.EqualTo(new long[] { 1, 2, 3 }));
    }

    [Test]
    public void ComputeHandAfterSwap_SwapMiddleCard_ReplacesWithFreshDeckIdx()
    {
        var hand = ServerBattleFrames.ComputeHandAfterSwap(new long[] { 2 });
        Assert.That(hand, Is.EqualTo(new long[] { 1, 4, 3 }));
    }

    [Test]
    public void ComputeHandAfterSwap_SwapAll_ReplacesAllWithFreshDeckIdxs()
    {
        var hand = ServerBattleFrames.ComputeHandAfterSwap(new long[] { 1, 2, 3 });
        Assert.That(hand, Is.EqualTo(new long[] { 4, 5, 6 }));
    }

    [Test]
    public void BuildSwapResponse_RendersGivenHandAsPositions()
    {
        var env = ServerBattleFrames.BuildSwapResponse(new long[] { 1, 4, 3 });
        var body = (SwapResponseBody)env.Body;
        Assert.That(body.Self.Count, Is.EqualTo(3));
        Assert.That(body.Self[1].Idx, Is.EqualTo(4));
    }

    [Test]
    public void BuildReady_IncludesGivenIdxChangeSeedAndSpin_AndUsesGivenHand()
    {
        var env = ServerBattleFrames.BuildReady(new long[] { 1, 4, 3 }, idxChangeSeed: 555_000);
        var body = (ReadyBody)env.Body;
        Assert.That(body.IdxChangeSeed, Is.EqualTo(555_000));
        Assert.That(body.Spin, Is.EqualTo(0));
        Assert.That(body.Self[1].Idx, Is.EqualTo(4));
    }

    [Test]
    public void BuildReady_two_arg_sets_oppo_to_supplied_hand()
    {
        var env = ServerBattleFrames.BuildReady(new long[] { 1, 4, 3 }, new long[] { 1, 2, 6 }, idxChangeSeed: 555_000);
        var body = (ReadyBody)env.Body;

        Assert.That(body.Self.Select(p => p.Idx), Is.EqualTo(new[] { 1, 4, 3 }));
        Assert.That(body.Oppo.Select(p => p.Idx), Is.EqualTo(new[] { 1, 2, 6 }),
            "oppo must reflect the opponent's post-mulligan hand, not the placeholder InitialHand.");
    }

    [Test]
    public void BuildReady_one_arg_defaults_oppo_to_InitialHand()
    {
        var env = ServerBattleFrames.BuildReady(new long[] { 1, 4, 3 }, idxChangeSeed: 555_000);
        var body = (ReadyBody)env.Body;

        Assert.That(body.Oppo.Select(p => p.Idx), Is.EqualTo(new[] { 1, 2, 3 }),
            "single-arg overload (non-interactive opponent) keeps the placeholder hand.");
    }

    private static MatchContext FixtureCtx(IReadOnlyList<long>? deck = null) => new(
        SelfDeckCardIds: deck ?? Enumerable.Range(1, 30).Select(i => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    // A prod-captured opponent MatchContext fixture that the BuildMatched/BuildBattleStart
    // helpers read from for the oppo half.
    private static MatchContext FakeOpponentCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 0L).ToList(),
        ClassId: CardClass.Portalcraft, CharaId: "8", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Japan, UserName: "Opponent", SleeveId: "704141010",
        EmblemId: "400001100", DegreeId: "120027", FieldId: 5, IsOfficial: 0,
        BattleModeId: 0);
}
