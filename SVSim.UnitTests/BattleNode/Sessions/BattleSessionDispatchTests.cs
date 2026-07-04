using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.UnitTests.BattleNode.Sessions;

[TestFixture]
public class BattleSessionDispatchTests
{
    [Test]
    public void Pvp_Loaded_from_A_assigns_turnState_0()
    {
        var (s, a, _) = NewPvpSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));

        var bs = (BattleStartBody)routes[0].Frame.Body;
        Assert.That(bs.TurnState, Is.EqualTo(TurnState.First), "A (first arriver) goes first.");
    }

    [Test]
    public void Pvp_Loaded_from_B_assigns_turnState_1()
    {
        var (s, _, b) = NewPvpSession();
        s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.InitBattle));
        var routes = s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.Loaded));

        var bs = (BattleStartBody)routes[0].Frame.Body;
        Assert.That(bs.TurnState, Is.EqualTo(TurnState.Second), "B (second arriver) goes second.");
    }

    [Test]
    public void Handshake_dispatch_reads_per_participant_Phase_not_session_Phase()
    {
        var a = new FakeRealParticipant(viewerId: 1, FixtureCtx());
        var b = new FakeRealParticipant(viewerId: 2, FixtureCtx());
        var s = new BattleSession("bid-1", BattleType.Pvp, a, b, NullLogger<BattleSession>.Instance);

        // A is AwaitingInitNetwork; B is AwaitingInitBattle (manually set).
        b.Phase = HandshakePhase.AwaitingInitBattle;

        // A's InitNetwork should ack (matches A's phase).
        var routesA = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        Assert.That(routesA.Count, Is.EqualTo(1));
        Assert.That(routesA[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.InitNetwork));
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AwaitingInitBattle));

        // B's InitBattle should produce Matched (matches B's phase, set above).
        var routesB = s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.InitBattle));
        Assert.That(routesB.Count, Is.EqualTo(1));
        Assert.That(routesB[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Matched));
        Assert.That(b.Phase, Is.EqualTo(HandshakePhase.AwaitingLoaded));
    }

    [Test]
    public void OutOfOrder_dispatch_returns_empty_and_does_not_advance_phase()
    {
        var (s, a, _) = NewPvpSession();
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        Assert.That(routes, Is.Empty);
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AwaitingInitNetwork));
    }

    [Test]
    public void Pvp_InitBattle_from_A_pushes_Matched_with_B_oppoInfo_to_A_only()
    {
        var (s, a, b) = NewPvpSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Matched));

        var body = (MatchedBody)routes[0].Frame.Body;
        Assert.That(body.SelfInfo.UserName, Is.EqualTo("PlayerA"),
            "Matched.selfInfo must reflect the sender (A).");
        Assert.That(body.SelfInfo.OppoId, Is.EqualTo(b.ViewerId));
        Assert.That(body.OppoInfo.UserName, Is.EqualTo("PlayerB"),
            "Matched.oppoInfo must reflect the OTHER participant (B).");
        Assert.That(body.OppoInfo.OppoId, Is.EqualTo(a.ViewerId));
        Assert.That(body.SelfInfo.Seed, Is.EqualTo(body.OppoInfo.Seed),
            "Both sides must see the same seed.");
    }

    [Test]
    public void Pvp_Matched_seed_derives_from_master_via_BattleSeeds_Stable()
    {
        var (s, a, _) = NewPvpSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));

        var body = (MatchedBody)routes[0].Frame.Body;
        Assert.That(body.SelfInfo.Seed, Is.EqualTo(BattleSeeds.Stable(s.MasterSeed)));
        Assert.That(body.OppoInfo.Seed, Is.EqualTo(BattleSeeds.Stable(s.MasterSeed)));
    }

    [Test]
    public void Pvp_Ready_idxChangeSeed_derives_from_master_and_recipient_viewer()
    {
        var (s, a, b) = NewPvpSession();
        // Both sides must complete the handshake before either can swap; then a swaps, then b's
        // swap releases Ready to BOTH (mirrors Pvp_Swap_from_both_releases_Ready).
        foreach (var p in new[] { a, b })
        {
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitNetwork));
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitBattle));
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.Loaded));
        }
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));               // a swaps first
        var bRoutes = s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.Swap)); // b releases both Readys

        var readyToA = bRoutes.Single(r => ReferenceEquals(r.Target, a) && r.Frame.Uri == NetworkBattleUri.Ready);
        var readyToB = bRoutes.Single(r => ReferenceEquals(r.Target, b) && r.Frame.Uri == NetworkBattleUri.Ready);
        Assert.That(((ReadyBody)readyToA.Frame.Body).IdxChangeSeed,
            Is.EqualTo(BattleSeeds.IdxChange(s.MasterSeed, a.ViewerId)));
        Assert.That(((ReadyBody)readyToB.Frame.Body).IdxChangeSeed,
            Is.EqualTo(BattleSeeds.IdxChange(s.MasterSeed, b.ViewerId)));
    }

    [Test]
    public void Pvp_InitBattle_from_B_pushes_Matched_with_A_oppoInfo_to_B_only()
    {
        var (s, a, b) = NewPvpSession();
        s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.InitNetwork));
        var routes = s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.InitBattle));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        var body = (MatchedBody)routes[0].Frame.Body;
        Assert.That(body.SelfInfo.UserName, Is.EqualTo("PlayerB"));
        Assert.That(body.OppoInfo.UserName, Is.EqualTo("PlayerA"));
    }

    [Test]
    public void Pvp_Loaded_from_A_pushes_BattleStart_with_B_oppoInfo_plus_Deal_to_A()
    {
        var (s, a, b) = NewPvpSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));

        Assert.That(routes.Count, Is.EqualTo(2));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.BattleStart));
        Assert.That(routes[1].Frame.Uri, Is.EqualTo(NetworkBattleUri.Deal));
        Assert.That(routes.All(r => ReferenceEquals(r.Target, a)), Is.True);

        var bs = (BattleStartBody)routes[0].Frame.Body;
        Assert.That(bs.SelfInfo.ClassId, Is.EqualTo("3"), "A is class 3 per fixture.");
        Assert.That(bs.OppoInfo.ClassId, Is.EqualTo("5"), "B is class 5 per fixture.");
    }

    [Test]
    public void Pvp_Swap_from_A_alone_pushes_SwapResponse_only_Ready_withheld()
    {
        var (s, a, b) = NewPvpSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        Assert.That(routes.Select(r => r.Frame.Uri), Is.EqualTo(new[] { NetworkBattleUri.Swap }),
            "Ready is withheld until BOTH sides have mulliganed.");
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AfterReady),
            "Phase advances on Swap even though Ready is withheld.");
    }

    [Test]
    public void Pvp_Swap_from_both_releases_Ready_to_both_with_opponent_hands()
    {
        var (s, a, b) = NewPvpSession();
        foreach (var p in new[] { a, b })
        {
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitNetwork));
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitBattle));
            s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.Loaded));
        }

        var aRoutes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap)); // first swapper
        Assert.That(aRoutes.Select(r => r.Frame.Uri), Is.EqualTo(new[] { NetworkBattleUri.Swap }));

        var bRoutes = s.ComputeFrames(b, NewEnvelope(NetworkBattleUri.Swap)); // second swapper releases both
        // Expect: B's own SwapResponse, then Ready to B, then Ready to A.
        Assert.That(bRoutes.Count, Is.EqualTo(3));
        Assert.That(bRoutes[0].Target, Is.SameAs(b));
        Assert.That(bRoutes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Swap));

        var readyToB = bRoutes.Single(r => ReferenceEquals(r.Target, b) && r.Frame.Uri == NetworkBattleUri.Ready);
        var readyToA = bRoutes.Single(r => ReferenceEquals(r.Target, a) && r.Frame.Uri == NetworkBattleUri.Ready);
        // Empty mulligans → each hand is the dealt [1,2,3]; oppo mirrors the other side's hand.
        Assert.That(((ReadyBody)readyToB.Frame.Body).Oppo.Select(p => p.Idx), Is.EqualTo(new[] { 1, 2, 3 }));
        Assert.That(((ReadyBody)readyToA.Frame.Body).Oppo.Select(p => p.Idx), Is.EqualTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Pvp_TurnStart_from_A_emits_spin0_to_B()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnStart));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.TurnStart));
        var body = (SVSim.BattleNode.Protocol.Bodies.OpponentTurnStartBody)routes[0].Frame.Body;
        Assert.That(body.Spin, Is.EqualTo(0));
    }

    [Test]
    public void Pvp_Judge_from_A_reflects_spin0_back_to_sender()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Judge));

        // Judge reflects BACK to its sender (the turn taker-over), not to the opponent: receiving
        // Judge{spin} fires the sender's ControlTurnStartPlayer. Routing to the opponent would
        // restart the just-ended player's turn (2026-06-03 two-client capture).
        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Judge));
        var body = (SVSim.BattleNode.Protocol.Bodies.JudgeBody)routes[0].Frame.Body;
        Assert.That(body.Spin, Is.EqualTo(0));
    }

    [Test]
    public void Pvp_PlayActions_synthesizes_knownList_from_sender_deck()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(idx: 3, from: 10, to: 20);
        body["playIdx"] = 3L;
        body["type"] = 30L;

        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.PlayActions));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.PlayIdx, Is.EqualTo(3));
        Assert.That(pb.Type, Is.EqualTo(30));
        Assert.That(pb.KnownList!.Count, Is.EqualTo(1));
        Assert.That(pb.KnownList[0].Idx, Is.EqualTo(3));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(100_011_010L)); // PlayerACtx deck cardId
        Assert.That(pb.KnownList[0].To, Is.EqualTo(20));
        Assert.That(pb.OppoTargetList, Is.Null);
    }

    [Test]
    public void Pvp_PlayActions_renames_targetList_to_oppoTargetList()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(idx: 3, from: 10, to: 20);
        body["playIdx"] = 3L;
        body["type"] = 31L;
        body["targetList"] = new List<object?>
        {
            new Dictionary<string, object?> { ["targetIdx"] = 8L, ["isSelf"] = 0L },
        };

        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;

        Assert.That(pb.OppoTargetList!.Count, Is.EqualTo(1));
        Assert.That(pb.OppoTargetList[0].TargetIdx, Is.EqualTo(8));
        Assert.That(pb.OppoTargetList[0].IsSelf, Is.EqualTo(CardOwner.Opponent));
    }

    [Test]
    public void Pvp_PlayActions_relays_uList_verbatim()
    {
        // A deck-fetch rides the uList (battle-traffic_tk2_regular.ndjson:75); the node forwards it
        // verbatim alongside the synthesized knownList for the played card.
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(idx: 3, from: 10, to: 20);
        body["playIdx"] = 3L;
        body["type"] = 30L;
        body["uList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["idxList"] = new List<object?> { 16L, 22L },
                ["from"] = 0L, ["to"] = 10L, ["isSelf"] = 1L, ["skill"] = "37|36|0",
            },
        };

        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));
        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;

        Assert.That(pb.KnownList!.Count, Is.EqualTo(1), "played card still synthesized in the same frame");
        Assert.That(pb.UList, Is.Not.Null);
        Assert.That(pb.UList!.Count, Is.EqualTo(1));
        Assert.That(pb.UList[0].IdxList, Is.EqualTo(new[] { 16, 22 }));
        Assert.That(pb.UList[0].From, Is.EqualTo(0));
        Assert.That(pb.UList[0].To, Is.EqualTo(10));
        Assert.That(pb.UList[0].IsSelf, Is.EqualTo(CardOwner.Self));
        Assert.That(pb.UList[0].Skill, Is.EqualTo("37|36|0"));
    }

    [Test]
    public void Pvp_PlayActions_without_uList_leaves_it_null()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(idx: 3, from: 10, to: 20);
        body["playIdx"] = 3L;
        body["type"] = 30L;

        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.UList, Is.Null);
    }

    [Test]
    public void Pvp_PlayActions_ungenerated_token_idx_degrades_to_no_knownList()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(idx: 31, from: 10, to: 20); // idx 31 > 30-card deck → token
        body["playIdx"] = 31L;
        body["type"] = 30L;

        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(pb.PlayIdx, Is.EqualTo(31));
        Assert.That(pb.KnownList, Is.Null);
    }

    [Test]
    public void Pvp_PlayActions_reveals_token_generated_in_an_earlier_frame()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // Frame 1: A plays deck card idx 3 (a spell, hand 10 -> cemetery 30) whose fanfare ADDS
        // token idx 31 (cardId 900111010) to A's hand (limbo 50 -> hand 10).
        var gen = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L,
            ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900111010L } } },
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L, ["from"] = 50L, ["to"] = 10L } },
            },
        };
        var genRoutes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gen));
        // The deck card itself reveals from the deck map; the token stays hidden (in hand).
        var genBody = (PlayActionsBroadcastBody)genRoutes[0].Frame.Body;
        Assert.That(genBody.KnownList!.Single().CardId, Is.EqualTo(100_011_010L), "deck card revealed");

        // Frame 2 (later turn): A plays token idx 31 from hand (10) to field (20).
        var play = MoveOrderList(idx: 31, from: 10, to: 20);
        play["playIdx"] = 31L;
        play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.PlayIdx, Is.EqualTo(31));
        Assert.That(pb.KnownList, Is.Not.Null, "the token's identity was remembered from its add op");
        Assert.That(pb.KnownList!.Single().Idx, Is.EqualTo(31));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(900_111_010L), "mined token cardId");
        Assert.That(pb.KnownList[0].To, Is.EqualTo(20));
    }

    [Test]
    public void Pvp_PlayActions_cross_side_gift_is_revealed_when_the_opponent_plays_it()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // A plays a card whose effect GIFTS B a token at idx 31 (isSelf:0 — from A's perspective the
        // card lives in the OPPONENT's index space; RegisterToken.cs:22 sets isSelf = CardObj.IsPlayer).
        // The node must record it into B's map, not A's.
        var gift = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L,
            ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 0L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900111010L } } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gift));

        // Later, B plays the gifted token idx 31 (hand 10 -> field 20). A must see its real identity.
        var play = MoveOrderList(idx: 31, from: 10, to: 20);
        play["playIdx"] = 31L;
        play["type"] = 30L;
        var routes = s.ComputeFrames(b, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(pb.KnownList, Is.Not.Null, "the gifted token's identity was recorded into B's map");
        Assert.That(pb.KnownList!.Single().Idx, Is.EqualTo(31));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(900_111_010L), "mined cross-side gift cardId");
        Assert.That(pb.KnownList[0].To, Is.EqualTo(20));
    }

    [Test]
    public void Pvp_PlayActions_reveals_copy_token_generated_in_an_earlier_frame()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // Frame 1: A plays deck card idx 3; its fanfare ADDS a concrete token idx 31 (cardId 900_111_010)
        // to A's hand (limbo 50 -> hand 10).
        var gen = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L, ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900_111_010L } } },
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L, ["from"] = 50L, ["to"] = 10L } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gen));

        // Frame 2: A plays deck card idx 4; its effect COPIES the token at idx 31 into a new token idx 32
        // (card:{baseIdx:31}) in A's hand.
        var copy = new Dictionary<string, object?>
        {
            ["playIdx"] = 4L, ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 4L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 31L, ["isPremium"] = 0L } } },
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 1L, ["from"] = 50L, ["to"] = 10L } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, copy));

        // Frame 3: A plays the copy token idx 32 from hand (10) to field (20).
        var play = MoveOrderList(idx: 32, from: 10, to: 20);
        play["playIdx"] = 32L; play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.PlayIdx, Is.EqualTo(32));
        Assert.That(pb.KnownList, Is.Not.Null, "the copy's identity was resolved from baseIdx and remembered");
        Assert.That(pb.KnownList!.Single().Idx, Is.EqualTo(32));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(900_111_010L), "copy resolved to its source token's cardId");
        Assert.That(pb.KnownList[0].To, Is.EqualTo(20));
    }

    [Test]
    public void Pvp_PlayActions_copy_of_a_token_added_in_the_same_frame_resolves()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // One frame: A's play ADDS concrete token idx 40 (cardId 900_222_020), then COPIES it to idx 41
        // (card:{baseIdx:40}) — copy op AFTER the concrete add in the same orderList. The copy must
        // resolve against the live map (copy mining runs after plain mining).
        var frame = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L, ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 40L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900_222_020L } } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 41L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 40L, ["isPremium"] = 0L } } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, frame));

        // Later: A plays the copy idx 41 (hand 10 -> field 20). Reveal proves same-frame chaining.
        var play = MoveOrderList(idx: 41, from: 10, to: 20);
        play["playIdx"] = 41L; play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.KnownList, Is.Not.Null, "copy of a same-frame add resolved against the live map");
        Assert.That(pb.KnownList!.Single().CardId, Is.EqualTo(900_222_020L));
    }

    [Test]
    public void Pvp_PlayActions_copy_with_unknown_baseIdx_degrades_to_no_knownList()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // A's play copies a baseIdx (99) that was never recorded → no identity to resolve.
        var frame = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L, ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 99L, ["isPremium"] = 0L } } },
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 1L, ["from"] = 50L, ["to"] = 10L } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, frame));

        var play = MoveOrderList(idx: 32, from: 10, to: 20);
        play["playIdx"] = 32L; play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.KnownList, Is.Null, "unknown baseIdx → no record → degrade to {playIdx,type}");
    }

    [Test]
    public void Pvp_Echo_mines_copy_token_for_a_later_reveal()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // Frame 1: A plays a card adding a concrete token idx 31 (cardId 900_333_030) to A's hand.
        var gen = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L, ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900_333_030L } } },
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gen));

        // Frame 2: B sends an Echo describing a copy of A's idx 31 (isSelf:0 from B = the opponent A's
        // index space) into a new token idx 32. Echo is mined but returns no routes.
        var echo = new Dictionary<string, object?>
        {
            ["playIdx"] = 5L, ["type"] = 31L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 32L }, ["isSelf"] = 0L,
                      ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 31L, ["isPremium"] = 0L } } },
            },
        };
        var echoRoutes = s.ComputeFrames(b, EnvWith(NetworkBattleUri.Echo, echo));
        Assert.That(echoRoutes, Is.Empty, "Echo is mined, never relayed");

        // Frame 3: A plays the copy token idx 32; B must see its real identity.
        var play = MoveOrderList(idx: 32, from: 10, to: 20);
        play["playIdx"] = 32L; play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(pb.KnownList, Is.Not.Null, "copy mined from the Echo into A's map");
        Assert.That(pb.KnownList!.Single().CardId, Is.EqualTo(900_333_030L));
    }

    [Test]
    public void Pvp_PlayActions_choice_token_records_pick_and_strips_selectCard()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // A plays a generating deck card (idx 3) whose fanfare is a hidden draw-to-hand choice: a
        // choiceAdd lands a token at idx 31 (candidates only), the move pulls it limbo->hand, and
        // keyAction.selectCard names the chosen cardId with open:0 (hidden).
        var gen = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L,
            ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?>
                        { ["candidates"] = new List<object?> { 810041260L, 101041020L } },
                      ["isChoice"] = "1" } },
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L, ["from"] = 50L, ["to"] = 10L } },
            },
            ["keyAction"] = new List<object?>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = 1L, ["cardId"] = 100_011_010L,
                    ["selectCard"] = new Dictionary<string, object?>
                        { ["cardId"] = new List<object?> { 810041260L }, ["open"] = 0L },
                }
            },
        };
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gen));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        // The generating deck card reveals from A's deck map (idx 3).
        Assert.That(pb.KnownList!.Single().CardId, Is.EqualTo(100_011_010L), "generating deck card revealed");
        // keyAction forwarded as {type,cardId}; selectCard stripped for the hidden choice.
        Assert.That(pb.KeyAction, Is.Not.Null);
        Assert.That(pb.KeyAction!.Single().Type, Is.EqualTo(KeyActionType.Choice));
        Assert.That(pb.KeyAction.Single().CardId, Is.EqualTo(100_011_010L));
        Assert.That(pb.KeyAction.Single().SelectCard, Is.Null, "the pick stays hidden for open:0");
    }

    [Test]
    public void Pvp_PlayActions_reveals_choice_token_when_chosen_card_is_played()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // Generation frame: records idx 31 -> chosen cardId 810041260 into A's map (from selectCard).
        var gen = new Dictionary<string, object?>
        {
            ["playIdx"] = 3L,
            ["type"] = 30L,
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 3L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?>
                        { ["candidates"] = new List<object?> { 810041260L, 101041020L } },
                      ["isChoice"] = "1" } },
            },
            ["keyAction"] = new List<object?>
            {
                new Dictionary<string, object?>
                {
                    ["type"] = 1L, ["cardId"] = 100_011_010L,
                    ["selectCard"] = new Dictionary<string, object?>
                        { ["cardId"] = new List<object?> { 810041260L }, ["open"] = 0L },
                }
            },
        };
        s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, gen));

        // Later A plays the chosen token idx 31 (hand 10 -> field 20). B must see its real identity.
        var play = MoveOrderList(idx: 31, from: 10, to: 20);
        play["playIdx"] = 31L;
        play["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.PlayIdx, Is.EqualTo(31));
        Assert.That(pb.KnownList, Is.Not.Null, "the choice pick was recorded at generation");
        Assert.That(pb.KnownList!.Single().Idx, Is.EqualTo(31));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(810041260L), "the chosen cardId surfaces on play");
        Assert.That(pb.KnownList[0].To, Is.EqualTo(20));
    }

    [Test]
    public void Pvp_PlayActions_when_B_still_AwaitingSwap_drops()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        // B not AfterReady → not BothSidesAfterReady.
        var body = MoveOrderList(3, 10, 20);
        body["playIdx"] = 3L; body["type"] = 30L;
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.PlayActions, body));
        Assert.That(routes, Is.Empty);
    }

    [Test]
    public void Pvp_Echo_from_A_in_BothSidesAfterReady_is_consumed_not_relayed()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Echo));

        Assert.That(routes, Is.Empty, "Echo has no inbound handler on the client; relaying risks an echo storm.");
    }

    [Test]
    public void Pvp_Echo_mines_token_identity_for_a_later_reveal()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        // B's Echo carries its own (isSelf:1) view of a token it received at idx 31. An Echo's
        // orderList carries the SAME add-op shape as PlayActions (SendCardDataMaker.MakeEchoData ->
        // MakeCommonSendAndEchoCardData), so the node MINES it for the identity — but still never
        // relays the Echo (no inbound client handler). Mining != relaying.
        var echo = new Dictionary<string, object?>
        {
            ["orderList"] = new List<object?>
            {
                new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                    { ["idx"] = new List<object?> { 31L }, ["isSelf"] = 1L,
                      ["card"] = new Dictionary<string, object?> { ["cardId"] = 900111010L } } },
            },
        };
        var echoRoutes = s.ComputeFrames(b, EnvWith(NetworkBattleUri.Echo, echo));
        Assert.That(echoRoutes, Is.Empty, "Echo is mined, not relayed.");

        // B plays the token idx 31 (hand 10 -> field 20); A must now see its real identity.
        var play = MoveOrderList(idx: 31, from: 10, to: 20);
        play["playIdx"] = 31L;
        play["type"] = 30L;
        var routes = s.ComputeFrames(b, EnvWith(NetworkBattleUri.PlayActions, play));

        var pb = (PlayActionsBroadcastBody)routes[0].Frame.Body;
        Assert.That(pb.KnownList, Is.Not.Null, "Echo-mined token identity surfaces on play");
        Assert.That(pb.KnownList!.Single().Idx, Is.EqualTo(31));
        Assert.That(pb.KnownList[0].CardId, Is.EqualTo(900_111_010L), "mined-from-Echo token cardId");
    }

    [Test]
    public void Pvp_TurnEndActions_from_A_emits_empty_body_to_B()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var body = MoveOrderList(3, 20, 30); // a non-empty orderList that must be dropped
        var routes = s.ComputeFrames(a, EnvWith(NetworkBattleUri.TurnEndActions, body));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.TurnEndActions));
        Assert.That(((RawBody)routes[0].Frame.Body).Entries, Is.Empty, "orderList is dropped; body is empty.");
    }

    [Test]
    public void Pvp_JudgeResult_from_A_in_BothSidesAfterReady_forwards_to_B()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.JudgeResult));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
    }

    [Test]
    public void Pvp_TurnEnd_from_A_emits_turnState_to_B_only()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnEnd));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.TurnEnd));
        var body = (SVSim.BattleNode.Protocol.Bodies.TurnEndBody)routes[0].Frame.Body;
        Assert.That(body.TurnState, Is.EqualTo(TurnState.First));
    }

    [Test]
    public void Pvp_TurnEndFinal_from_A_forwards_envelope_to_B_and_pushes_paired_BattleFinish()
    {
        // Unified TurnEndFinal handling — A is the winner, B is the loser.
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnEndFinal));

        Assert.That(routes.Count, Is.EqualTo(3));

        Assert.That(routes[0].Target, Is.SameAs(b));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.TurnEndFinal));

        Assert.That(routes[1].Target, Is.SameAs(a));
        Assert.That(routes[1].Frame.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        Assert.That(((BattleFinishBody)routes[1].Frame.Body).Result, Is.EqualTo(BattleResult.LifeWin));

        Assert.That(routes[2].Target, Is.SameAs(b));
        Assert.That(routes[2].Frame.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        Assert.That(((BattleFinishBody)routes[2].Frame.Body).Result, Is.EqualTo(BattleResult.LifeLose));

        Assert.That(s.Lifecycle, Is.EqualTo(SessionLifecycle.Terminal));
    }

    [Test]
    public void Pvp_TurnEnd_when_B_still_AwaitingSwap_drops()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        // B not at AfterReady.

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnEnd));

        Assert.That(routes, Is.Empty);
    }

    [Test]
    public void Pvp_Retire_from_A_pushes_RetireLose_to_A_and_RetireWin_to_B()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Retire));

        Assert.That(routes.Count, Is.EqualTo(2));
        var aRoute = routes.Single(r => ReferenceEquals(r.Target, a));
        var bRoute = routes.Single(r => ReferenceEquals(r.Target, b));
        Assert.That(aRoute.Frame.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        Assert.That(((BattleFinishBody)aRoute.Frame.Body).Result, Is.EqualTo(BattleResult.RetireLose));
        Assert.That(bRoute.Frame.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        Assert.That(((BattleFinishBody)bRoute.Frame.Body).Result, Is.EqualTo(BattleResult.RetireWin));
        Assert.That(aRoute.Stock, Is.EqualTo(Stock.Bypass));
        Assert.That(bRoute.Stock, Is.EqualTo(Stock.Bypass));
        Assert.That(s.Lifecycle, Is.EqualTo(SessionLifecycle.Terminal));
    }

    [Test]
    public void Pvp_Kill_from_A_same_as_Retire()
    {
        var (s, a, b) = NewPvpSession();
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Kill));

        Assert.That(routes.Count, Is.EqualTo(2));
        Assert.That(s.Lifecycle, Is.EqualTo(SessionLifecycle.Terminal));
    }

    private static (BattleSession, FakeRealParticipant, FakeParticipant) NewBotSession()
    {
        var a = new FakeRealParticipant(viewerId: 1001, PlayerACtx());
        var b = new FakeParticipant(viewerId: ServerBattleFrames.FakeOpponentViewerId, NoOpBotContext());
        var s = new BattleSession("bid-bot-1", BattleType.Bot, a, b, NullLogger<BattleSession>.Instance);
        return (s, a, b);
    }

    private static MatchContext NoOpBotContext() => new(
        SelfDeckCardIds: Array.Empty<long>(),
        ClassId: CardClass.None, CharaId: "0", CardMasterName: "card_master_node_10015",
        CountryCode: "", UserName: "Bot", SleeveId: "0",
        EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0, BattleModeId: 0);

    [Test]
    public void Bot_InitNetwork_acks_to_sender()
    {
        var (s, a, _) = NewBotSession();
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.InitNetwork));
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AwaitingInitBattle));
    }

    [Test]
    public void Bot_InitBattle_acks_to_sender_with_no_Matched_push()
    {
        var (s, a, _) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));

        // Bot InitBattle is ack-only — NO Matched push. Matched would be ignored
        // by the client anyway (gated on status == Connect, which is already
        // past by the time the wire round-trip completes), but the spec is to
        // not send it for clarity.
        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.InitBattle),
            "Expected an ack envelope for InitBattle, NOT a Matched envelope.");
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AwaitingLoaded));
    }

    [Test]
    public void Bot_Loaded_produces_no_routes_but_advances_phase()
    {
        var (s, a, _) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));

        // Bot Loaded is silent — no BattleStart, no Deal. Pushing BattleStart
        // would actively CORRUPT OppoBattleStartInfo on the client (the wire
        // handler at Matching.cs:417 → SetNetworkInfo overwrites it with our
        // placeholder NoOpBotParticipant.Context zeros).
        Assert.That(routes, Is.Empty, "Bot Loaded is silent.");
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AwaitingSwap),
            "Phase still advances even though there are no outbound routes.");
    }

    [Test]
    public void Bot_Swap_per_sender_SwapResponse_plus_Ready()
    {
        // Opponent stub is not IHasHandshakePhase → not a barrier swapper → Ready releases immediately.
        var (s, a, _) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        Assert.That(routes.Select(r => r.Frame.Uri),
            Is.EqualTo(new[] { NetworkBattleUri.Swap, NetworkBattleUri.Ready }));
        Assert.That(routes.All(r => ReferenceEquals(r.Target, a)), Is.True);
        Assert.That(a.Phase, Is.EqualTo(HandshakePhase.AfterReady));
    }

    [Test]
    public void Bot_TurnEnd_pushes_Judge_to_sender_only()
    {
        var (s, a, b) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnEnd));

        Assert.That(routes.Count, Is.EqualTo(1), "Bot TurnEnd → exactly one Judge frame back.");
        Assert.That(routes[0].Target, Is.SameAs(a), "Judge target is the sender, not broadcast.");
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Judge));
    }

    [Test]
    public void Bot_TurnEndFinal_pushes_Judge_to_sender_only()
    {
        var (s, a, _) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.TurnEndFinal));

        Assert.That(routes.Count, Is.EqualTo(1));
        Assert.That(routes[0].Frame.Uri, Is.EqualTo(NetworkBattleUri.Judge));
        Assert.That(routes[0].Target, Is.SameAs(a));
    }

    [Test]
    public void Bot_PlayActions_drops_no_recipient()
    {
        var (s, a, _) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        // Bot's PlayActions falls through the default arm — the Pvp forwarder is gated
        // on Type == Pvp, so Bot's gameplay frames have no routing rule and drop.
        // (The PvP semantics would have been "forward to NoOp which swallows" — same
        // observable result, but cleaner to leave them as default-drops.)
        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.PlayActions));

        Assert.That(routes, Is.Empty);
    }

    [Test]
    public void Bot_Retire_pushes_paired_BattleFinish_RetireLose_to_player_RetireWin_to_bot()
    {
        // Unified Retire/Kill dispatch — same paired push as PvP.
        // NoOpBotParticipant swallows its push.
        var (s, a, b) = NewBotSession();
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Loaded));
        s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Swap));

        var routes = s.ComputeFrames(a, NewEnvelope(NetworkBattleUri.Retire));

        Assert.That(routes.Count, Is.EqualTo(2));
        Assert.That(routes[0].Target, Is.SameAs(a));
        Assert.That(((BattleFinishBody)routes[0].Frame.Body).Result, Is.EqualTo(BattleResult.RetireLose));
        Assert.That(routes[1].Target, Is.SameAs(b));
        Assert.That(((BattleFinishBody)routes[1].Frame.Body).Result, Is.EqualTo(BattleResult.RetireWin));
    }

    private static (BattleSession, FakeRealParticipant, FakeRealParticipant) NewPvpSession()
    {
        var a = new FakeRealParticipant(viewerId: 1001, PlayerACtx());
        var b = new FakeRealParticipant(viewerId: 2002, PlayerBCtx());
        var s = new BattleSession("bid-pvp-1", BattleType.Pvp, a, b, NullLogger<BattleSession>.Instance);
        return (s, a, b);
    }

    private static void DriveToAfterReady(BattleSession s, FakeRealParticipant p)
    {
        s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.InitBattle));
        s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.Loaded));
        s.ComputeFrames(p, NewEnvelope(NetworkBattleUri.Swap));
        // p.Phase should now be AfterReady.
    }

    private static MatchContext PlayerACtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Runecraft, CharaId: "3", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "PlayerA", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    private static MatchContext PlayerBCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 200_011_010L).ToList(),
        ClassId: CardClass.Shadowcraft, CharaId: "5", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Japan, UserName: "PlayerB", SleeveId: "3000022",
        EmblemId: "701441022", DegreeId: "300004", FieldId: 44, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    private static MsgEnvelope NewEnvelope(NetworkBattleUri uri) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?>()));

    private static MsgEnvelope EnvWith(NetworkBattleUri uri, Dictionary<string, object?> body) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: new RawBody(body));

    private static Dictionary<string, object?> MoveOrderList(int idx, int from, int to) => new()
    {
        ["orderList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["move"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { (long)idx },
                    ["isSelf"] = 1L, ["from"] = (long)from, ["to"] = (long)to,
                }
            }
        }
    };

    /// <summary>Data-only IBattleParticipant stub for dispatch tests. PushAsync/RunAsync
    /// are no-ops; FrameEmitted exists but is never invoked by the test.</summary>
    private sealed class FakeParticipant : IBattleParticipant
    {
        public long ViewerId { get; }
        public MatchContext Context { get; }
        public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;
        public FakeParticipant(long viewerId, MatchContext context) { ViewerId = viewerId; Context = context; }
        public Task PushAsync(MsgEnvelope env, Stock stock, CancellationToken ct) => Task.CompletedTask;
        public Task RunAsync(CancellationToken ct) => Task.CompletedTask;
        public Task TerminateAsync(BattleFinishReason reason) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        private void Touch() => FrameEmitted?.Invoke(null!, default);
    }

    /// <summary>Like <see cref="FakeParticipant"/> but additionally implements
    /// <see cref="IHasHandshakePhase"/> so the dispatch tests can drive a participant's
    /// Phase without instantiating a full <c>RealParticipant</c> (which needs a real
    /// WebSocket).</summary>
    private sealed class FakeRealParticipant : IBattleParticipant, IHasHandshakePhase
    {
        public long ViewerId { get; }
        public MatchContext Context { get; }
        public HandshakePhase Phase { get; set; } = HandshakePhase.AwaitingInitNetwork;
        public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;
        public FakeRealParticipant(long viewerId, MatchContext context) { ViewerId = viewerId; Context = context; }
        public Task PushAsync(MsgEnvelope env, Stock stock, CancellationToken ct) => Task.CompletedTask;
        public Task RunAsync(CancellationToken ct) => Task.CompletedTask;
        public Task TerminateAsync(BattleFinishReason reason) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        private void Touch() => FrameEmitted?.Invoke(null!, default);
    }
}
