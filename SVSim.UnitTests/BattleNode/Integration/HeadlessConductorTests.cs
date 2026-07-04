using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Dispatch;
using SVSim.BattleNode.Sessions.Dispatch.Handlers;

namespace SVSim.UnitTests.BattleNode.Integration;

/// <summary>
/// Headless-Conductor milestone tests (M-HC-*). The oracle is a node-native battle:
/// a FIXED master seed + FIXED decks drive the engine's receive path headless, and we
/// assert on engine board-state. By construction the node assigns idx = position in the
/// shuffled order, so the engine's headless draw reproduces the node's draw order.
///
/// Task 1 (M-HC-0a) exit criterion: the engine seats headless (IsReady) in the
/// SVSim.UnitTests process.
///
/// Task 2 (M-HC-0b) exit criterion: a node-generated <c>Deal</c> seats the 3-card hand and a
/// vanilla hand-card <c>Play</c> resolves on ENGINE board state (card left hand, PP dropped
/// by cost, board reflects the play) — driven through the receive CONDUCTOR, not the
/// direct ActionProcessor path the M2-M12 oracles use.
///
/// Task 3 (M-HC-1) exit criterion: the mulligan ops (<c>Swap</c> seats the post-mulligan hand —
/// idx-3 swapped for the next unused deck idx-4) and turn ops (<c>Ready</c>/<c>TurnStart</c>/
/// <c>TurnEnd</c>) resolve headless, so two full turns of a node-native battle track on engine
/// state (hand/board/PP/deck/turn/leader-life on both seats match the deterministic progression
/// at each boundary). All driven through the same receive conductor.
///
/// Task 5 (M-HC-3a) exit criterion: the opponent-facing <c>knownList[].cost</c> carries the
/// engine-RESOLVED play-time cost (the discounted cost the engine actually charged), closing the
/// spellboost cost-desync by construction. Proven both at the engine read (PlayedCardCost off a
/// charge-seeded reducer) and the handler emit (PlayActionsHandler -> PlayActionsBroadcastBody).
/// NOTE: a BOARD-DEPENDENT cost reducer (e.g. <c>when_evolve_other</c>) is DEFERRED to M-HC-4 —
/// evolve does not yet resolve headless. Because cost is read straight off the resolved engine,
/// board modifiers are captured by construction once their ops resolve, so no separate emit-site
/// change is needed when M-HC-4 lands; only a board-dependent validation case is owed there.
/// </summary>
[TestFixture]
[NonParallelizable]
public class HeadlessConductorTests
{
    [Test]
    public void Harness_seats_engine_headless_and_is_ready()
    {
        using var harness = NodeNativeBattleHarness.Create();

        Assert.That(harness.IsReady, Is.True,
            "Engine must seat headless: EngineGlobalInit ran + both decks seeded. " +
            "If false, the most likely cause is a missing cards.json content link in " +
            "SVSim.UnitTests.csproj (EngineGlobalInit reads AppContext.BaseDirectory/Data/cards.json).");

        // Non-vacuous: a seated engine has live board state for BOTH seats. Reading these off a
        // not-really-set-up engine would throw (Seat() guards on _mgr). Leader life is the headless
        // default (20) before any frame is ingested.
        Assert.That(harness.LeaderLife(playerSeat: true), Is.EqualTo(20), "seat A leader life");
        Assert.That(harness.LeaderLife(playerSeat: false), Is.EqualTo(20), "seat B leader life");
    }

    // The node's BuildDeal opening hand: pos->idx (0,1),(1,2),(2,3). hand == deck idx 1,2,3, i.e.
    // the top 3 of the node-native shuffled deck. Both seats deal the same idx triple.
    private static Dictionary<string, object?> DealBody() => new()
    {
        ["self"] = PosIdxList((0, 1), (1, 2), (2, 3)),
        ["oppo"] = PosIdxList((0, 1), (1, 2), (2, 3)),
    };

    // A minimal vanilla hand-card play: type 30 == PLAY_HAND; playIdx is the played card's index.
    // No targetList/orderList — a vanilla follower auto-resolves with no selection.
    private static Dictionary<string, object?> PlayBody(int playIdx) => new()
    {
        ["playIdx"] = playIdx,
        ["type"] = 30,
    };

    // A pos->idx list (the wire shape NetworkParameter.self/oppo carry: an ordered list of
    // {pos, idx} dicts). The receiver re-sorts by pos into the seat's idx list.
    private static List<object?> PosIdxList(params (int pos, int idx)[] entries)
    {
        var list = new List<object?>(entries.Length);
        foreach (var (pos, idx) in entries)
            list.Add(new Dictionary<string, object?> { ["pos"] = pos, ["idx"] = idx });
        return list;
    }

    // Server-authored Swap RESPONSE frame (the shadow ingests this, NOT the client's {idxList}
    // Submit). It carries the POST-mulligan self hand as pos->idx. Swapping the pos-2 card (deck
    // idx 3) pulls the next unused deck idx (4) — exactly battle_test_cl1's Swap receive frame.
    private static Dictionary<string, object?> SwapBody() => new()
    {
        ["self"] = PosIdxList((0, 1), (1, 2), (2, 4)),
    };

    // Server-authored Ready frame: both hands known + the idxChangeSeed/spin the receiver
    // consumes to seal the mulligan and start turn 1. Mirrors battle_test_cl1's Ready receive.
    private static Dictionary<string, object?> ReadyBody() => new()
    {
        ["self"] = PosIdxList((0, 1), (1, 2), (2, 4)), // same post-mulligan self hand as SwapBody — Ready re-echoes it
        ["oppo"] = PosIdxList((0, 1), (1, 2), (2, 3)),
        ["idxChangeSeed"] = 857671914,
        ["spin"] = 0,
    };

    private static Dictionary<string, object?> TurnStartBody() => new() { ["spin"] = 0 };
    private static Dictionary<string, object?> TurnEndBody() => new() { ["turnState"] = 0 };

    // An opponent play that REVEALS the played card. The wire shape is taken verbatim from
    // battle_test_cl2.ndjson's first opponent PlayActions frame:
    //   { playIdx, type:30, knownList:[{idx, cardId, to:30, spellboost:0, attachTarget:""}] }
    // type 30 == PLAY_HAND; knownList[].idx == the hidden dummy's engine Index; cardId == the real
    // identity to substitute; to 30 == NetworkCardPlaceState.Field (the card lands in play).
    private static Dictionary<string, object?> RevealPlayBody(int idx, long cardId) => new()
    {
        ["playIdx"] = idx,
        ["type"] = 30,
        ["knownList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["idx"] = idx,
                ["cardId"] = cardId,
                ["to"] = 30,
                ["spellboost"] = 0,
                ["attachTarget"] = "",
            },
        },
    };

    [Test]
    public void Swap_seats_post_mulligan_hand_headless()
    {
        using var harness = NodeNativeBattleHarness.Create();

        var deal = harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true);
        Assert.That(deal.Accepted, Is.True, $"Deal rejected: {deal.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(3), "post-Deal hand");

        var swap = harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true);
        Assert.That(swap.Accepted, Is.True, $"Swap rejected: {swap.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(3),
            "the swapped slot is replaced, not removed — hand stays at 3");

        // The pos-2 card was the deck-idx-3 card; the swap replaces it with the deck-idx-4 card.
        // The kept cards (idx 1, 2) stay put. Assert the engine hand holds idx {1,2,4}.
        var handIdxs = new[]
        {
            harness.PlayerHandCardIndex(0),
            harness.PlayerHandCardIndex(1),
            harness.PlayerHandCardIndex(2),
        };
        Assert.That(handIdxs, Is.EquivalentTo(new[] { 1, 2, 4 }),
            "post-mulligan hand must hold deck idx 1,2,4 (idx-3 swapped for the next unused idx-4)");
    }

    [Test]
    public void Two_turns_track_on_engine_state_headless()
    {
        // The oracle is the engine's OWN deterministic node-native progression off the fixed seed:
        // every value below is the engine-resolved state, reproducible by construction. The shadow
        // ingests the same server-authored frame stream the live node emits (Deal/Swap/Ready then
        // per-turn TurnStart/TurnEnd — the exact receive frames captured in battle_test_cl1.ndjson).
        using var harness = NodeNativeBattleHarness.Create();

        // --- mulligan barrier: Deal, Swap, Ready -------------------------------------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted,
            Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted,
            Is.True, "Swap");
        var ready = harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false);
        Assert.That(ready.Accepted, Is.True, $"Ready rejected: {ready.RejectReason}");

        // After Ready the mulligan is sealed and the main phase is entered, but no turn has been
        // opened yet (TurnStart does the ramp + draw). Seat A holds its post-mulligan 3-card hand;
        // the opponent's hand stays hidden until its reveal frames land (Task 4) — node-native, the
        // opponent's opening hand is never disclosed to the relay before its own turn.
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(3), "seat A hand after Ready");
        Assert.That(harness.Turn(playerSeat: true), Is.EqualTo(0), "no turn opened yet after Ready");

        // --- turn 1 (seat A active) -------------------------------------------------------------
        // Seat A is game-first (doesPlayerGoFirst: true), so turn-1 draws ONE card. PP ramps to 1.
        var t1 = harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true);
        Assert.That(t1.Accepted, Is.True, $"turn1 TurnStart rejected: {t1.RejectReason}");
        Assert.That(harness.Turn(playerSeat: true), Is.EqualTo(1), "seat A turn counter");
        Assert.That(harness.Pp(playerSeat: true), Is.EqualTo(1), "turn 1 ramps seat A max PP to 1");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(4),
            "turn-1 first-player draw is 1 card (3 mulligan + 1 draw)");
        Assert.That(harness.DeckCount(playerSeat: true), Is.EqualTo(26), "seat A deck after draw");

        // End seat A's turn.
        var t1End = harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true);
        Assert.That(t1End.Accepted, Is.True, $"turn1 TurnEnd rejected: {t1End.RejectReason}");

        // --- turn 2 (seat B active) -------------------------------------------------------------
        // Seat B is second player (doesPlayerGoFirst: true → enemy goes second). Ready's
        // isPlayerSeat=false triggers OperateOppoMulligan → DrawFirstMulliganCard, moving 3 dealt
        // cards from deck to hand. Turn-1 draws 2 (second player draws 2 on turn 1).
        var t2 = harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false);
        Assert.That(t2.Accepted, Is.True, $"turn2 TurnStart rejected: {t2.RejectReason}");
        Assert.That(harness.Turn(playerSeat: false), Is.EqualTo(1), "seat B turn counter");
        Assert.That(harness.Pp(playerSeat: false), Is.EqualTo(1), "turn 2 ramps seat B max PP to 1");
        Assert.That(harness.HandCount(playerSeat: false), Is.EqualTo(5), "seat B hand: 3 mulligan + 2 turn-1 draws");
        Assert.That(harness.DeckCount(playerSeat: false), Is.EqualTo(25), "seat B deck: 30 - 3 mulligan - 2 draws");

        // Both leaders untouched (no damage dealt across the two opening turns) — state tracks
        // cleanly on BOTH seats at the turn boundary.
        Assert.That(harness.LeaderLife(playerSeat: true), Is.EqualTo(20), "seat A leader life");
        Assert.That(harness.LeaderLife(playerSeat: false), Is.EqualTo(20), "seat B leader life");
    }

    [Test]
    public void Seat_A_play_after_partial_mulligan_finds_kept_card()
    {
        // Regression: a partial mulligan (swap 1 of 3) must leave the kept cards in hand.
        // Matches live battle 175320039619: A (cl2, Forestcraft) swaps idx 1,2 (keeps 3).
        // Includes BOTH client Swaps + server Swap responses (the full live frame stream).
        var aDeck = new List<long> { 101121080,102131020,100111010,102121030,101121020,101121110,101114010,100111010,102141010,102121010,101121020,102131030,701141011,100111020,101131050,100111020,100111070,101121010,100111070,101121080,100114010,101121110,101114050,101114050,100114010,100114010,102111060,113011010,102121030,102131010,100111020,101114050,101121080,101121010,101131020,113011010,113011010,101114010,102111060,102121010 };
        var bDeck = Enumerable.Repeat(NodeNativeBattleHarness.VanillaFollowerId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(
            seatADeck: aDeck, seatBDeck: bDeck,
            seatAClass: CardClass.Forestcraft, seatBClass: CardClass.Runecraft);

        harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true);

        // Client Swap from A (idxList only — no "self")
        harness.Push(NetworkBattleUri.Swap, new Dictionary<string, object?> { ["idxList"] = new List<object?> { 2, 1 } }, isPlayerSeat: true);
        // Server Swap response to A
        harness.Push(NetworkBattleUri.Swap, new Dictionary<string, object?> { ["self"] = PosIdxList((0, 4), (1, 5), (2, 3)) }, isPlayerSeat: true);
        // Client Swap from B (no mulligan)
        harness.Push(NetworkBattleUri.Swap, new Dictionary<string, object?> { ["idxList"] = new List<object?>() }, isPlayerSeat: false);
        // Server Swap response to B
        harness.Push(NetworkBattleUri.Swap, new Dictionary<string, object?> { ["self"] = PosIdxList((0, 1), (1, 2), (2, 3)) }, isPlayerSeat: false);

        // Ready (from A's perspective)
        harness.Push(NetworkBattleUri.Ready, new Dictionary<string, object?>
        {
            ["self"] = PosIdxList((0, 4), (1, 5), (2, 3)),
            ["oppo"] = PosIdxList((0, 1), (1, 2), (2, 4)),
            ["idxChangeSeed"] = 1463392880, ["spin"] = 0,
        }, isPlayerSeat: false);

        harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true);

        var handIdxs = Enumerable.Range(0, harness.HandCount(playerSeat: true))
            .Select(i => harness.HandCardIndex(playerSeat: true, i)).ToList();
        TestContext.WriteLine($"A hand after T1: [{string.Join(",", handIdxs)}]");
        Assert.That(handIdxs, Does.Contain(3), "kept card idx 3 must be in A hand");

        var play = harness.Push(NetworkBattleUri.PlayActions, PlayBody(3), isPlayerSeat: true);
        Assert.That(play.Accepted, Is.True, $"A play idx 3 rejected: {play.RejectReason}");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "A board after play");
    }

    [Test]
    public void Seed_deck_advances_cardTotalNum_so_tokens_dont_collide_with_deck_indices()
    {
        // Regression for the engine-divergence diagnosed 2026-06-07 (bid 806245601092).
        //
        // The real client's SBattleLoad.InitPlayer (SBattleLoad.cs:1292) loads the 40-card deck at
        // indices 1..40 and THEN sets `cardTotalNum = deck.Count + 1` (== 41), so the first
        // skill-generated token (via BattleManagerBase.SetupCardIndex with addIndex=-1) gets Index
        // 41 — exactly what the wire `add.idx` carries (e.g. `{"add":{"idx":[41,42],...}}`).
        //
        // The headless SessionBattleEngine.SeedDeck used to omit that tail, leaving `cardTotalNum`
        // at the property default (0). The first generated token then got Index 0, the second got
        // Index 1, and they COLLIDED with deck-loaded cards at the same indices. The collision was
        // silent until something addressed the deck card with the colliding Index: Hoverboarder at
        // deck idx 1 made GetBattleCardIdx's SingleOrDefault find TWO Index-1 cards and throw
        // "Sequence contains more than one matching element".
        //
        // The contract verified here: after Setup, `cardTotalNum` MUST equal `deck.Count + 1` on
        // both seats. This pins SBattleLoad's tail behavior in the headless engine.
        const int deckSize = 30; // NodeNativeBattleHarness.DefaultDeck is 30 cards
        using var harness = NodeNativeBattleHarness.Create();
        Assert.That(harness.IsReady, Is.True, "engine seats headless");

        Assert.Multiple(() =>
        {
            Assert.That(harness.DebugCardTotalNum(playerSeat: true), Is.EqualTo(deckSize + 1),
                "seat A cardTotalNum must be deck.Count+1 after Setup (= next token Index >= deck.Count+1)");
            Assert.That(harness.DebugCardTotalNum(playerSeat: false), Is.EqualTo(deckSize + 1),
                "seat B cardTotalNum must be deck.Count+1 after Setup");
        });
    }

    [Test]
    public void Engine_stableRandom_seed_aligns_with_wire_seed_clients_receive()
    {
        // Regression for the shadow-engine desync diagnosed 2026-06-07 (bid 654473755566).
        //
        // CLIENTS seed System.Random with Matched.seed (BattleManagerBase.cs:721), which the node
        // sends as BattleSeeds.Stable(MasterSeed) (InitBattleHandler.cs:28). The engine must seed its
        // _stableRandom with the SAME value; otherwise the very first NextDouble() returns a different
        // number, every turn-1+ StableRandom-driven draw picks a different deck position, and the
        // opponent's first non-mulligan play addresses a card the engine never drew → HandCardToField
        // throws.
        //
        // Before the fix, engine.Setup received the raw MasterSeed (1184631275 in the live battle),
        // while clients received BattleSeeds.Stable(MasterSeed) (=1543475792). After the fix,
        // BattleSession.EnsureEngineSetup + NodeNativeBattleHarness.Create both pass the Stable-derived
        // value, so both streams produce the same NextDouble sequence.
        const int masterSeed = 1184631275; // the bid 654473755566 master seed
        int wireSeed = BattleSeeds.Stable(masterSeed);

        // The first NextDouble a fresh client would consume (turn-1 first-player draw is the very
        // first _stableRandom consumer — Deal/Swap/Ready don't touch _stableRandom).
        double expectedFirstDouble = new System.Random(wireSeed).NextDouble();

        using var harness = NodeNativeBattleHarness.Create(masterSeed: masterSeed);
        Assert.That(harness.IsReady, Is.True, "engine seats headless");

        double engineFirstDouble = harness.DebugStableRandomDouble();
        Assert.That(engineFirstDouble, Is.EqualTo(expectedFirstDouble),
            $"engine _stableRandom must be seeded with BattleSeeds.Stable({masterSeed})={wireSeed} " +
            "(the value Matched.seed ships clients); otherwise turn-1+ draws desync from the clients.");
    }

    [Test]
    public void Seat_B_vanilla_play_resolves_on_engine_state()
    {
        // Seat B (opponent/enemy) plays a vanilla follower on its first turn. Uses an all-vanilla
        // deck so no spell-path interference. Verifies the doesPlayerGoFirst:true seat mapping
        // lets B's play resolve through the engine (hand→board mutation).
        var allVanilla = Enumerable.Repeat(NodeNativeBattleHarness.VanillaFollowerId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: allVanilla, seatBDeck: allVanilla);

        harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true);
        harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true);
        harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false);

        // A's turn
        harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true);
        harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true);

        // B's turn (second player, draws 2)
        harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false);
        Assert.That(harness.HandCount(playerSeat: false), Is.EqualTo(5), "B hand: 3 mulligan + 2 draws");

        var bPlay = harness.Push(NetworkBattleUri.PlayActions, PlayBody(3), isPlayerSeat: false);
        Assert.That(bPlay.Accepted, Is.True, $"B play rejected: {bPlay.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: false), Is.EqualTo(4), "B hand after play");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(1), "B board after play");
    }

    [Test]
    public void Opponent_reveal_seats_card_on_seat_B_headless()
    {
        // Seat B's deck idx 1 is a known vanilla follower, so the reveal's wire cardId maps to a real
        // card the opponent can play to the board. (Seat A's deck is left at default — irrelevant here.)
        var seatBDeck = new List<long> { NodeNativeBattleHarness.VanillaFollowerId };
        seatBDeck.AddRange(NodeNativeBattleHarness.DefaultDeck());
        seatBDeck = seatBDeck.GetRange(0, 30);

        using var harness = NodeNativeBattleHarness.Create(seatBDeck: seatBDeck);

        // --- drive to seat B's turn (reuse Task 3's two-turn sequence) ---------------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted,
            Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted,
            Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted,
            Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted,
            Is.True, "turn2 TurnStart (seat B active)");

        // Seat B's opening hand is hidden (deck reads full minus its single turn-1 draw); its cards
        // have NOT been disclosed to the relay yet. The dummy at engine Index 1 is whatever card the
        // shuffle seated at that index (shuffledDeck[0]), parked in a hidden zone — NOT on the board.
        // Confirm seat B's board is empty BEFORE the reveal, so the post-reveal +1 is decisively the
        // reveal seating the card. (Node-native, the harness seeds each side's cards with their real id
        // — it knows both decks — so this test's reveal substitution is identity-preserving by choice;
        // CreateActualCard builds the card purely from the wire cardId regardless of which card the
        // shuffle parked at Index 1. The board delta is what proves ReplaceReceivedCard.ReplaceCard ->
        // CreateActualCard resolved the card onto the board headless. The companion test
        // Opponent_reveal_overrides_seeded_identity_headless stresses a MISMATCHED cardId to prove the
        // wire id — not the seeded identity — is what gets seated.)
        var boardBefore = harness.BoardCount(playerSeat: false);
        Assert.That(boardBefore, Is.EqualTo(0), "seat B has no board followers before the reveal");

        // --- the reveal: an opponent PlayActions frame carrying a knownList that discloses idx 1 ---
        const long revealedCardId = NodeNativeBattleHarness.VanillaFollowerId;
        var reveal = harness.Push(
            NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: revealedCardId),
            isPlayerSeat: false);

        Assert.That(reveal.Accepted, Is.True, $"opponent reveal rejected: {reveal.RejectReason}");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(boardBefore + 1),
            "the revealed follower must seat on seat B's board");
        Assert.That(harness.InPlayCardId(playerSeat: false, boardPos: 0), Is.EqualTo((int)revealedCardId),
            "the seated card's identity must equal the wire cardId from the reveal");
    }

    [Test]
    public void Opponent_reveal_overrides_seeded_identity_headless()
    {
        // This is the substitution half of M-HC-2: prove the seated card's POST-reveal identity is the
        // WIRE cardId even when it DIFFERS from whatever the shuffle parked at that engine Index.
        // ReplaceReceivedCard.CreateActualCard builds the card purely from cardData.CardId, independent
        // of the seated dummy's id — so a reveal whose cardId mismatches the seed must OVERRIDE it.
        //
        // Z (seeded) vs W (revealed) are DIFFERENT cost-1 vanilla followers, both present + creatable in
        // cards.json:
        //   Z = 100011010 — the proven vanilla follower (char_type 1, cost 1). Seat B's deck is made
        //       UNIFORMLY of Z, so whichever idx the shuffle parked at Index 1 is unambiguously Z.
        //   W = 101211120 — a different cost-1 vanilla follower (char_type 1, cost 1, no skill). Cost 1
        //       seats at seat B's first-turn PP (1). The id is NOT in seat B's deck, so the only way it
        //       can appear on the board is the reveal substituting it in.
        const long Z = NodeNativeBattleHarness.VanillaFollowerId; // 100011010
        const long W = NodeNativeBattleHarness.AltVanillaFollowerId; // 101211120
        Assert.That(W, Is.Not.EqualTo(Z), "Z and W must differ for the substitution to be observable");

        // Uniform Z deck for seat B (every dummy is Z regardless of shuffle). Seat A left at default.
        var seatBDeck = Enumerable.Repeat(Z, 30).ToList();

        using var harness = NodeNativeBattleHarness.Create(seatBDeck: seatBDeck);

        // --- drive to seat B's turn (same two-turn sequence as the sibling reveal test) -------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted,
            Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted,
            Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted,
            Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted,
            Is.True, "turn2 TurnStart (seat B active)");

        var boardBefore = harness.BoardCount(playerSeat: false);
        Assert.That(boardBefore, Is.EqualTo(0), "seat B has no board followers before the reveal");

        // The reveal discloses idx 1 (seeded as Z) with the MISMATCHED wire cardId W.
        var reveal = harness.Push(
            NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: W), isPlayerSeat: false);

        Assert.That(reveal.Accepted, Is.True, $"opponent reveal rejected: {reveal.RejectReason}");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(boardBefore + 1),
            "the revealed follower must seat on seat B's board");
        // The decisive assertion: the seated identity is W (the wire cardId), NOT Z (the seeded id).
        // Because the deck is uniformly Z, this can only pass if the reveal OVERRODE the seeded identity.
        Assert.That(harness.InPlayCardId(playerSeat: false, boardPos: 0), Is.EqualTo((int)W),
            "the seated card must be the wire cardId W, overriding the seeded Z identity at that idx");
    }

    // === M-HC-4a: attack resolves headless =======================================================

    [Test]
    public void Attack_on_enemy_leader_resolves_on_engine_state_headless()
    {
        // Seat A plays a vanilla follower on turn 1, then on its NEXT turn (past summoning sickness)
        // attacks seat B's leader. Assert seat B's leader life drops by the follower's attack (1) and the
        // attacker is spent. Driven entirely through the receive conductor (Push -> engine.Receive).
        //
        // Uniform vanilla deck so the card dealt at engine Index 1 is unambiguously the 1/2 vanilla.
        var deck = Enumerable.Repeat(NodeNativeBattleHarness.VanillaFollowerId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: deck);

        // --- mulligan + open seat A turn 1 ------------------------------------------------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        // Play the vanilla (engine Index 1, cost 1) onto seat A's board.
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true).Accepted,
            Is.True, "turn1 vanilla play");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "seat A follower on board after play");

        // The just-played follower has summoning sickness this turn (can't attack yet).
        Assert.That(harness.InPlayCardAttackable(playerSeat: true, boardPos: 0), Is.False,
            "a follower has summoning sickness the turn it is played");

        int attackerIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);
        int attackerAtk = harness.InPlayCardAtk(playerSeat: true, boardPos: 0);
        Assert.That(attackerAtk, Is.EqualTo(1), "the vanilla follower's attack stat is 1");

        // --- advance to seat A's NEXT turn (turn 3) so the follower is past summoning sickness ---------
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnStart (A)");

        Assert.That(harness.InPlayCardAttackable(playerSeat: true, boardPos: 0), Is.True,
            "the follower can attack on seat A's next turn (summoning sickness cleared)");

        int leaderLifeBefore = harness.LeaderLife(playerSeat: false);
        Assert.That(leaderLifeBefore, Is.EqualTo(20), "seat B leader untouched before the attack");

        // --- the attack: seat A follower -> seat B leader (Index 0, on the enemy seat) ----------------
        var attack = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.AttackBody(attackerIdx, targetIdx: 0, targetOnEnemySeat: true),
            isPlayerSeat: true);

        Assert.That(attack.Accepted, Is.True, $"attack rejected: {attack.RejectReason}");
        Assert.That(harness.LeaderLife(playerSeat: false), Is.EqualTo(leaderLifeBefore - attackerAtk),
            "seat B leader life must drop by the attacker's attack stat");
        Assert.That(harness.InPlayCardAttackable(playerSeat: true, boardPos: 0), Is.False,
            "the attacker is spent after attacking (can't attack again this turn)");
    }

    [Test]
    public void Follower_vs_follower_attack_is_a_lethal_trade_headless()
    {
        // Seat A plays a 1/1 vanilla; seat B reveals a 1/1 vanilla (M-HC-2 reveal pattern). On seat A's
        // next turn the follower attacks seat B's follower. Each deals 1 to a 1-life body -> a lethal
        // trade: both followers' life drops and both leave the board.
        var oneOne = NodeNativeBattleHarness.VanillaOneOneFollowerId;
        var seatADeck = Enumerable.Repeat(oneOne, 30).ToList();
        var seatBDeck = Enumerable.Repeat(oneOne, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck, seatBDeck: seatBDeck);

        // --- mulligan + seat A turn 1: play the 1/1 -------------------------------------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true).Accepted, Is.True, "turn1 play 1/1");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "seat A 1/1 on board");
        int attackerIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);

        // --- seat B turn 2: reveal a 1/1 onto seat B's board ------------------------------------------
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(0), "seat B board empty before reveal");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: oneOne), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play 1/1");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(1), "seat B 1/1 on board after reveal");
        int targetIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 0);

        // --- back to seat A (turn 3): the 1/1 is past summoning sickness ------------------------------
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnStart (A)");
        Assert.That(harness.InPlayCardAttackable(playerSeat: true, boardPos: 0), Is.True, "attacker past summoning sickness");

        Assert.That(harness.InPlayCardLife(playerSeat: true, boardPos: 0), Is.EqualTo(1), "attacker 1/1 full life before trade");
        Assert.That(harness.InPlayCardLife(playerSeat: false, boardPos: 0), Is.EqualTo(1), "target 1/1 full life before trade");

        // --- attack follower -> follower (target on enemy seat B) ------------------------------------
        var attack = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.AttackBody(attackerIdx, targetIdx, targetOnEnemySeat: true),
            isPlayerSeat: true);

        Assert.That(attack.Accepted, Is.True, $"follower trade rejected: {attack.RejectReason}");
        // 1/1 vs 1/1: each takes 1 -> both at 0 life -> both die and leave the board (lethal trade).
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(0), "attacker 1/1 died in the trade");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(0), "target 1/1 died in the trade");
        Assert.That(harness.LeaderLife(playerSeat: false), Is.EqualTo(20),
            "neither leader takes damage in a follower-vs-follower trade");
    }

    // === M-HC-4b: evolve resolves headless =======================================================

    [Test]
    public void Evolve_resolves_on_engine_state_headless()
    {
        // Seat A plays a vanilla follower (base 1/2, evo 3/4 — a +2/+2 plain evolve, no target), then ramps
        // to the turn its EP unlocks and EVOLVES it. Assert the engine-state mutation: the follower is marked
        // evolved, its atk/life rise by the card's evolve deltas, and seat A's EP drops by 1. Driven entirely
        // through the receive conductor (Push -> engine.Receive).
        //
        // Uniform vanilla deck so the card dealt at engine Index 1 is unambiguously the 1/2 vanilla. Card
        // 100011010: base atk 1 / life 2, evo_atk 3 / evo_life 4 -> evolve delta +2/+2 (read from cards.json).
        const int evolvedAtk = 3;
        const int evolvedLife = 4;
        var deck = Enumerable.Repeat(NodeNativeBattleHarness.VanillaFollowerId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: deck);

        // --- mulligan + open seat A turn 1, play the vanilla onto seat A's board --------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true).Accepted,
            Is.True, "turn1 vanilla play");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "seat A follower on board after play");

        // The follower can't evolve yet — seat A's EvolveWaitTurnCount has not counted down to 0.
        Assert.That(harness.EvolveWaitTurnCount(playerSeat: true), Is.GreaterThan(0),
            "evolve is locked on seat A's first turn (wait-turn counter not yet 0)");
        int attackerIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);

        // --- ramp seat A to the turn its evolve unlocks (EvolveWaitTurnCount counts down per seat-A turn) ---
        // End turn 1 first (TurnEnd sets NowTurnEvol = true, the other CanEvolution precondition), then ramp.
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        RampSeatAToEvolveTurn(harness);

        // EP precondition: seat A holds at least 1 evolve point and evolve is now unlocked.
        Assert.That(harness.EvolveWaitTurnCount(playerSeat: true), Is.EqualTo(0), "evolve unlocked on seat A's turn");
        int epBefore = harness.EpCount(playerSeat: true);
        Assert.That(epBefore, Is.GreaterThanOrEqualTo(1), "seat A must hold >= 1 EP before evolving");

        // Pre-evolve stats: the un-evolved vanilla is 1/2 and not yet flagged evolved.
        Assert.That(harness.IsEvolved(playerSeat: true, boardPos: 0), Is.False, "follower not evolved before the evolve");
        int atkBefore = harness.InPlayCardAtk(playerSeat: true, boardPos: 0);
        int lifeBefore = harness.InPlayCardLife(playerSeat: true, boardPos: 0);
        Assert.That(atkBefore, Is.EqualTo(1), "vanilla base atk is 1 before evolve");
        Assert.That(lifeBefore, Is.EqualTo(2), "vanilla base life is 2 before evolve");

        // --- the evolve: a plain EVOLUTION frame addressing the follower by its in-play Index -------
        var evolve = harness.Push(
            NetworkBattleUri.PlayActions, NodeNativeBattleHarness.EvolveBody(attackerIdx), isPlayerSeat: true);

        Assert.That(evolve.Accepted, Is.True, $"evolve rejected: {evolve.RejectReason}");
        Assert.That(harness.IsEvolved(playerSeat: true, boardPos: 0), Is.True,
            "the follower must be flagged evolved after the EVOLUTION frame resolves");
        Assert.That(harness.InPlayCardAtk(playerSeat: true, boardPos: 0), Is.EqualTo(evolvedAtk),
            "evolved atk must equal the card's evo_atk (3) — base 1 + evolve delta +2");
        Assert.That(harness.InPlayCardLife(playerSeat: true, boardPos: 0), Is.EqualTo(evolvedLife),
            "evolved life must equal the card's evo_life (4) — base 2 + evolve delta +2");
        Assert.That(harness.EpCount(playerSeat: true), Is.EqualTo(epBefore - 1),
            "an evolve must spend exactly one evolve point");
    }

    // TODO(M-HC-exit): EVOLUTION_SELECT target path uncovered — needs an evolve-target fixture card.
    // The EVOLUTION_SELECT driver (NodeNativeBattleHarness.EvolveSelectBody, opcode 21) is in place; what's
    // missing is a fixture: a follower whose evo_skill + evo_skill_target are populated so the evolve drives
    // a real target/select. Such cards DO exist in cards.json — skill MECHANICS (skill/skill_timing/
    // skill_condition/skill_target/skill_option, plus evo_skill/evo_skill_target) are fully dumped and
    // engine-executed (M-HC-4c..f exercise real skills incl. cost_change/when_evolve_other/token_draw). The
    // CLAUDE.md "placeholder" note refers ONLY to card NAMES/TEXT, not mechanics. Driving an EVOLUTION_SELECT
    // against a non-targeting evolve degenerates to the plain-evolve path (empty select list), so it would
    // not exercise GetOpposingCardObjTarget / the select view leaves. Wire one of the existing evo-target
    // followers into the harness to cover this — that's the only remaining step.

    // === M-HC-4c: targeted play resolves headless ================================================

    [Test]
    public void Targeted_damage_spell_resolves_on_engine_state_headless()
    {
        // Seat A plays a single-target when_play DAMAGE spell (deal 2 to a selected enemy follower) at ONE of
        // TWO enemy followers seat B revealed onto its board. Assert the engine applied the damage headless to
        // the WIRE-SPECIFIED target and ONLY it: the targeted follower's life drops by exactly the skill's
        // damage amount (2) AND the other follower is untouched (full life). Two targets makes the assertion
        // itself prove the resolution honored the wire-specified target idx (with a single follower, "auto-pick
        // the only legal target" would be indistinguishable from honoring the wire target). Driven entirely
        // through the receive conductor (Push -> engine.Receive -> RecoveryOperationCollection.PlaySkillSelectHandCardOperation
        // -> PlayHandCardReflection.PlayAction, target resolved via LookForActionDataToTargetCard).
        //
        // Seat A deck: uniformly the cost-1 damage spell so whatever idx the shuffle parked at engine Index 1
        // (the first dealt card) is unambiguously the spell. Seat A's class is the spell's clan (Dragoncraft=4)
        // so the leader/clan is consistent. Seat B: uniformly the 1/4 high-life vanilla (both reveal targets are
        // 1/4, so both SURVIVE 2 damage — the non-targeted one reads a clean "untouched" life of 4).
        var seatADeck = Enumerable.Repeat(NodeNativeBattleHarness.SingleTargetDamageSpellId, 30).ToList();
        var seatBDeck = Enumerable.Repeat(NodeNativeBattleHarness.HighLifeVanillaFollowerId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(
            seatADeck: seatADeck, seatBDeck: seatBDeck,
            seatAClass: SVSim.BattleNode.Bridge.CardClass.Dragoncraft);

        // --- mulligan + open seat A turn 1, end it (no enemy target yet) -----------------------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");

        // --- reveal TWO high-life followers onto seat B's board (one per seat-B turn) ----------------
        // A reveal substitutes identity onto a card seat B holds IN HAND (BattlePlayerBase.HandCardToField),
        // and seat B's opening hand is dealt into hidden zones — only its per-turn DRAW is a revealable hand
        // card. So each seat-B turn yields exactly one revealable card: reveal follower #1 on seat B turn 2,
        // then advance to seat B turn 4 and reveal follower #2. (The reveal frame is server-authored, so it
        // seats regardless of seat B's PP — turn-2 PP 1 vs the 1/4's cost 2 just drives PP negative, which is
        // immaterial to seat A's later spell.) Each reveal addresses seat B's current hand card by its live
        // engine Index so we don't hard-code a shuffle-dependent idx.
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(0), "seat B board empty before reveals");
        int revealIdx1 = harness.HandCardIndex(playerSeat: false, handPos: 0);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions,
                RevealPlayBody(idx: revealIdx1, cardId: NodeNativeBattleHarness.HighLifeVanillaFollowerId), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play follower #1");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(1), "one seat B follower after reveal #1");

        // seat A turn 3 (no play) -> seat B turn 4 (draws a second revealable card).
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnStart (A)");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnEnd (A)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn4 TurnStart (B)");
        int revealIdx2 = harness.HandCardIndex(playerSeat: false, handPos: 0);
        Assert.That(revealIdx2, Is.Not.EqualTo(revealIdx1), "the two revealed hand cards must be distinct engine Indices");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions,
                RevealPlayBody(idx: revealIdx2, cardId: NodeNativeBattleHarness.HighLifeVanillaFollowerId), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play follower #2");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(2), "two seat B followers on board after reveals");

        // The spell will target board position 0; assert board position 1 (the OTHER follower) is left whole.
        int targetIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 0);
        int otherIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 1);
        Assert.That(otherIdx, Is.Not.EqualTo(targetIdx), "the two revealed followers must have distinct engine Indices");
        int targetLifeBefore = harness.InPlayCardLife(playerSeat: false, boardPos: 0);
        int otherLifeBefore = harness.InPlayCardLife(playerSeat: false, boardPos: 1);
        Assert.That(targetLifeBefore, Is.EqualTo(NodeNativeBattleHarness.HighLifeVanillaFollowerLife),
            "target's base life (4) before the spell");
        Assert.That(otherLifeBefore, Is.EqualTo(NodeNativeBattleHarness.HighLifeVanillaFollowerLife),
            "non-target's base life (4) before the spell");

        // --- back to seat A (turn 5): play the damage spell at the FIRST enemy follower -------------
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn4 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn5 TurnStart (A)");

        // Locate the cost-1 damage spell in seat A's hand (uniform deck -> first hand card is the spell).
        int spellIdx = harness.HandCardIndex(playerSeat: true, handPos: 0);
        Assert.That(harness.HandCardId(playerSeat: true, handPos: 0),
            Is.EqualTo((int)NodeNativeBattleHarness.SingleTargetDamageSpellId), "seat A hand card is the damage spell");
        int handBefore = harness.HandCount(playerSeat: true);
        int ppBefore = harness.Pp(playerSeat: true);

        var play = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.TargetedPlayBody(spellIdx, targetIdx, targetOnEnemySeat: true),
            isPlayerSeat: true);

        Assert.That(play.Accepted, Is.True, $"targeted spell play rejected: {play.RejectReason}");
        // The spell actually resolved: it left the hand and charged its cost (guards against an accepted-
        // but-silently-no-op resolution that would make the damage assertion vacuous).
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(handBefore - 1),
            "the played spell must leave seat A's hand");
        Assert.That(harness.Pp(playerSeat: true), Is.LessThan(ppBefore),
            "the spell's cost must be charged to seat A's PP");
        // Both 1/4 targets survive 2 damage, so the board still holds two followers (the test reads life, not
        // removal). Board positions are stable across this play: pos 0 is the targeted follower, pos 1 the other.
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(2),
            "both 1/4 followers survive (the spell hits only one, for 2 < 4)");
        // THE target-discriminating assertion: the WIRE-targeted follower took exactly the skill's damage (2)
        // -> 1/4 drops to life 2, while the OTHER (non-targeted) follower is UNTOUCHED at full life (4). This
        // pair proves resolution honored the wire-specified target idx, not "auto-pick the only legal target".
        Assert.That(harness.InPlayCardLife(playerSeat: false, boardPos: 0),
            Is.EqualTo(targetLifeBefore - NodeNativeBattleHarness.SingleTargetDamageAmount),
            "the WIRE-TARGETED follower's life must drop by the spell's damage amount (2)");
        Assert.That(harness.InPlayCardLife(playerSeat: false, boardPos: 1),
            Is.EqualTo(otherLifeBefore),
            "the NON-targeted follower must be UNTOUCHED (full life) — proves the wire target was honored");
    }

    // === isSelf->vid owner-mapping is DIRECTIONAL across BOTH sender perspectives =================

    [Test]
    public void Attack_from_seat_B_on_seat_A_follower_resolves_isSelf_reversed()
    {
        // The reversed-perspective half of the live isSelf->vid translation: a frame sent BY SEAT B
        // (isPlayerSeat:false) targeting a SEAT A follower carries isSelf:0 (the target is NOT on the
        // sender's seat). TranslateTargetOwners must map (isPlayerSeat:false, isSelf:0) -> the seat-A
        // engine vid (ThisViewerId), so the attack resolves on seat A's follower — NOT seat B's own. The
        // seat-A-sender M-HC-4c test proves the forward direction; this proves the mapping isn't
        // accidentally symmetric (a translation that ignored isPlayerSeat would mis-route a seat-B frame).
        //
        // Driven via an ATTACK (no hand-identity dependency): seat A plays a 1/2 vanilla turn 1; seat B
        // reveals a 1/2 vanilla turn 2 and on turn 4 attacks seat A's follower. Both are 1/2 so each
        // survives the single trade and the life DROP (2 -> 1) is readable on the seat-A target.
        var vanilla = NodeNativeBattleHarness.VanillaFollowerId; // 1/2
        var seatADeck = Enumerable.Repeat(vanilla, 30).ToList();
        var seatBDeck = Enumerable.Repeat(vanilla, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck, seatBDeck: seatBDeck);

        // seat A turn 1: play a 1/2 onto seat A's board.
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart (A)");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true).Accepted, Is.True, "turn1 play 1/2 (A)");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "one seat A follower on board");
        int targetIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);
        int targetLifeBefore = harness.InPlayCardLife(playerSeat: true, boardPos: 0);
        Assert.That(targetLifeBefore, Is.EqualTo(2), "seat A 1/2 at full life before the attack");

        // seat B turn 2: reveal a 1/2 onto seat B's board (so it exists; it gains summoning sickness).
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd (A)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: vanilla), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play 1/2");
        Assert.That(harness.BoardCount(playerSeat: false), Is.EqualTo(1), "one seat B follower on board");
        int attackerIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 0);

        // advance to seat B's NEXT turn (turn 4) so seat B's follower is past summoning sickness.
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnStart (A)");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnEnd (A)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn4 TurnStart (B)");
        Assert.That(harness.InPlayCardAttackable(playerSeat: false, boardPos: 0), Is.True, "seat B attacker past summoning sickness");

        // isPlayerSeat:false (seat B sends), targetOnEnemySeat:true -> isSelf:0 -> the SEAT-A engine vid.
        var attack = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.AttackBody(attackerIdx, targetIdx, targetOnEnemySeat: true),
            isPlayerSeat: false);

        Assert.That(attack.Accepted, Is.True, $"seat-B attack rejected: {attack.RejectReason}");
        // The attack resolved onto the SEAT A follower (the reversed-perspective owner mapping worked):
        // the 1/2 target took 1 -> life 1.
        Assert.That(harness.InPlayCardLife(playerSeat: true, boardPos: 0), Is.EqualTo(targetLifeBefore - 1),
            "the seat-A target took the attack's damage (isPlayerSeat:false, isSelf:0 -> seat A vid)");
    }

    [Test]
    public void Attack_with_wrong_owner_flag_does_not_hit_the_enemy_follower()
    {
        // Negative / wrong-owner discriminator: seat A attacks but the targetList flags the target as the
        // SENDER's OWN (targetOnEnemySeat:false -> isSelf:1 -> the seat-A engine vid), while pointing at the
        // index where the ENEMY (seat B) follower sits. The translation must route that to seat A, so seat
        // B's follower is NOT hit — proving the owner mapping is directional, not "hit whatever sits at the
        // idx". (Mirrors the M-HC-4c target-discriminating pattern, on the OWNER axis.)
        var oneOne = NodeNativeBattleHarness.VanillaOneOneFollowerId;
        var seatADeck = Enumerable.Repeat(oneOne, 30).ToList();
        var seatBDeck = Enumerable.Repeat(oneOne, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck, seatBDeck: seatBDeck);

        // seat A turn 1: play a 1/1.
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true).Accepted, Is.True, "turn1 play 1/1");
        int attackerIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);

        // seat B turn 2: reveal a 1/1 onto seat B's board.
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: oneOne), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play 1/1");
        int enemyIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 0);
        int enemyLifeBefore = harness.InPlayCardLife(playerSeat: false, boardPos: 0);

        // back to seat A (turn 3): attacker past summoning sickness.
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnEnd (B)");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn3 TurnStart (A)");
        Assert.That(harness.InPlayCardAttackable(playerSeat: true, boardPos: 0), Is.True, "attacker past summoning sickness");

        // WRONG owner: targetOnEnemySeat:false (isSelf:1) but pointing at the enemy follower's idx. The
        // attack resolves against seat A's own space, so seat B's follower is NOT damaged.
        harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.AttackBody(attackerIdx, targetIdx: enemyIdx, targetOnEnemySeat: false),
            isPlayerSeat: true);

        Assert.That(harness.InPlayCardLife(playerSeat: false, boardPos: 0), Is.EqualTo(enemyLifeBefore),
            "the enemy follower must be UNTOUCHED when the attack flags the target as the sender's own (wrong owner)");
    }

    [Test]
    public void Choice_play_resolves_chosen_branch_on_engine_state_headless()
    {
        // Seat A plays a CHOICE card (id 127011010: "choose ONE of two tokens to add to hand") and selects
        // token B. Assert the engine resolved the CHOSEN branch headless: seat A's hand gains the chosen
        // token (the choice card itself leaves the hand; the chosen token is drawn in). Driven through the
        // receive conductor (Push -> engine.Receive). The wire keyAction shape is taken verbatim from a real
        // capture of THIS card: data_dumps/captures/battle_test/rng/battle-traffic_cl1.ndjson carries
        //   keyAction:[{"type":1,"cardId":127011010,"selectCard":{"cardId":[121011010],"open":0}}].
        var seatADeck = Enumerable.Repeat(NodeNativeBattleHarness.ChoiceCardId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");

        int choiceIdx = harness.HandCardIndex(playerSeat: true, handPos: 0);
        Assert.That(harness.HandCardId(playerSeat: true, handPos: 0),
            Is.EqualTo((int)NodeNativeBattleHarness.ChoiceCardId), "seat A hand card is the choice card");

        // PP-charged no-op guard (symmetry with the targeted test): the choice card is cost 1 and turn-1 PP
        // is 1, so a real resolution must drop PP. An accept-but-no-op resolution (chosen branch never ran)
        // would leave PP unchanged — this catches it.
        int ppBefore = harness.Pp(playerSeat: true);

        // Choose token B (distinct from token A so the assertion is decisive about WHICH branch resolved).
        const long chosen = NodeNativeBattleHarness.ChoiceTokenB;
        var play = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.ChoicePlayBody(choiceIdx, NodeNativeBattleHarness.ChoiceCardId, chosen),
            isPlayerSeat: true);

        Assert.That(play.Accepted, Is.True, $"choice play rejected: {play.RejectReason}");
        Assert.That(harness.Pp(playerSeat: true), Is.LessThan(ppBefore),
            "the choice card's cost (1) must be charged to seat A's PP");
        // The chosen token landed in seat A's hand (token_draw of the CHOSEN id) -> the chosen branch resolved.
        bool chosenInHand = false;
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)chosen) { chosenInHand = true; break; }
        Assert.That(chosenInHand, Is.True,
            "the chosen token (B) must be added to seat A's hand — proving the chosen choice branch resolved");

        // Non-vacuity / decisiveness: the OTHER branch's token (A) must NOT be in hand — i.e. the engine
        // resolved the SPECIFIC chosen branch, not "any token" or "both". (Token A != token B by construction.)
        bool otherInHand = false;
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)NodeNativeBattleHarness.ChoiceTokenA) { otherInHand = true; break; }
        Assert.That(otherInHand, Is.False,
            "the UN-chosen token (A) must NOT be added — the engine resolved the specific chosen branch");
    }

    [Test]
    public void Choice_play_resolves_under_wrapped_selectCard_wire_shape()
    {
        // Regression for the engine silently-dropped Choice play diagnosed 2026-06-07
        // (bid 131549100204): the SENDER's live wire wraps selectCard as
        //   selectCard:{cardId:[<tokenId>], open:0}
        // (verified in data_dumps/captures/battle_test/cl1/battle-traffic.ndjson at the Resonance
        // play of idx 20). The engine's receive parser reads selectCard via ConvertToListInt
        // (NetworkBattleReceiver.cs:1202), which does `value as List<object>` — a Dictionary value
        // casts to null and the inner foreach NREs. The surrounding ConvertReceiveDataToMakeData has
        // a swallow-all catch (NetworkBattleReceiver.cs:1255-1260) that logs to Debug.LogError +
        // LocalLog — both shimmed/no-op'd headlessly — and returns false; SessionBattleEngine.Receive
        // calls ReceivedMessage with checkBreakData:false, so the false isn't propagated. The play
        // continues with choiceIdList=[], never moves the card from hand to board, and any LATER
        // targeted play that addresses the un-resolved card by Index (e.g. a bounce spell) crashes
        // with a null target.
        //
        // Fix: SessionBattleEngine.TranslateChoiceKeyAction unwraps the wrapped selectCard on the
        // engine's own dict copy before the receiver sees it (sibling to TranslateTargetOwners). The
        // unwrap is purely a shadow-ingest shape transformation — production engine code is
        // unchanged, and the opponent-facing relay (which never carries selectCard at all) is
        // untouched. After the unwrap, the same resolution path that the existing flat-list test
        // (Choice_play_resolves_chosen_branch_on_engine_state_headless) exercises must produce the
        // same outcome.
        var seatADeck = Enumerable.Repeat(NodeNativeBattleHarness.ChoiceCardId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");

        int choiceIdx = harness.HandCardIndex(playerSeat: true, handPos: 0);
        int ppBefore = harness.Pp(playerSeat: true);
        const long chosen = NodeNativeBattleHarness.ChoiceTokenB;

        // Drive the play using the WRAPPED wire shape — the exact form a live client emits.
        var play = harness.Push(
            NetworkBattleUri.PlayActions,
            NodeNativeBattleHarness.ChoicePlayBodyWrapped(choiceIdx, NodeNativeBattleHarness.ChoiceCardId, chosen),
            isPlayerSeat: true);

        Assert.That(play.Accepted, Is.True, $"wrapped-selectCard choice play rejected: {play.RejectReason}");
        Assert.That(harness.Pp(playerSeat: true), Is.LessThan(ppBefore),
            "the choice card's cost must charge PP — confirms the play actually resolved, not silently dropped");

        bool chosenInHand = false;
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)chosen) { chosenInHand = true; break; }
        Assert.That(chosenInHand, Is.True,
            "the chosen token (B) must land in seat A's hand — proves the CHOSEN branch resolved through the wrapped wire shape");

        bool otherInHand = false;
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)NodeNativeBattleHarness.ChoiceTokenA) { otherInHand = true; break; }
        Assert.That(otherInHand, Is.False,
            "the UN-chosen token (A) must NOT be added — decisive that the unwrap forwarded the SPECIFIC chosen id, not a default or both");
    }

    [Test]
    public void Deal_seats_three_card_hand_headless()
    {
        using var harness = NodeNativeBattleHarness.Create();

        var result = harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true);

        Assert.That(result.Accepted, Is.True, $"Deal rejected: {result.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(3),
            "Deal must seat the 3-card opening hand on the player seat.");
    }

    [Test]
    public void Vanilla_play_resolves_on_engine_state_headless()
    {
        // Deck idx 1/2/3 are the top three of the shuffled deck; arrange idx-1 to be a known vanilla
        // follower so the Play assertion is decisive. Put the vanilla follower first; the rest of the
        // default deck (spellboost + vanillas) follows.
        var deck = new List<long> { NodeNativeBattleHarness.VanillaFollowerId };
        deck.AddRange(NodeNativeBattleHarness.DefaultDeck());
        deck = deck.GetRange(0, 30);

        using var harness = NodeNativeBattleHarness.Create(seatADeck: deck);

        var deal = harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true);
        Assert.That(deal.Accepted, Is.True, $"Deal rejected: {deal.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(3), "post-Deal hand");

        var ppBefore = harness.Pp(playerSeat: true);
        var handBefore = harness.HandCount(playerSeat: true);
        var boardBefore = harness.BoardCount(playerSeat: true);

        // The played card is at hand index 1 (deck idx 1 -> the first dealt card; engine card Index
        // mirrors deck position+1). The shuffle determines which deck idx-1 maps to; we only need a
        // vanilla follower in the opening hand. Use the first dealt idx.
        var playIdx = harness.PlayerHandCardIndex(0);
        var play = harness.Push(NetworkBattleUri.PlayActions, PlayBody(playIdx), isPlayerSeat: true);

        Assert.That(play.Accepted, Is.True, $"Play rejected: {play.RejectReason}");
        Assert.That(harness.HandCount(playerSeat: true), Is.EqualTo(handBefore - 1),
            "the played card must leave the hand");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(boardBefore + 1),
            "a follower play must add one to the board");
        Assert.That(harness.Pp(playerSeat: true), Is.LessThan(ppBefore),
            "PP must drop by the played card's cost");
    }

    // === M-HC-3a: engine-resolved cost on the knownList ==========================================

    // The spellboost cost-reducer 101314020 (base cost 5). Its when_spell_charge cost_change skill
    // (skill_option add=ADD_CHARGE_COUNT*-1) reduces its OWN cost by 1 per accumulated spellboost
    // charge — so resolved cost == max(0, 5 - charge). The harness seeds the charge directly
    // (SeedHandCardSpellboostCost registers the same CostAddModifier(-1)/charge the engine's own
    // Skill_cost_change builds) because pumping real charge needs the VFX-coupled spell-charge chain.
    private const long SpellboostReducerId = NodeNativeBattleHarness.SpellboostCardId; // 101314020
    private const int SpellboostReducerBaseCost = 5;

    // A deck made UNIFORMLY of the spellboost reducer, so whatever idx the shuffle parks at engine
    // Index 1 (the first dealt card) is unambiguously the reducer — no need to chase the shuffled
    // position. (A non-uniform deck would shuffle the reducer off idx 1; the cost read would then be a
    // vanilla's base 1, masking the discount — that is exactly the first RED this surfaced.)
    private static IReadOnlyList<long> ReducerDeck() => Enumerable.Repeat(SpellboostReducerId, 30).ToList();

    [TestCase(0, SpellboostReducerBaseCost)] // no charge -> base cost (5)
    [TestCase(4, 1)]                         // 4 charges -> 5 - 4 = 1
    [TestCase(5, 0)]                         // 5 charges -> max(0, 5 - 5) = 0
    public void PlayedCardCost_reads_engine_resolved_discounted_cost(int charge, int expectedCost)
    {
        // ENGINE-READ proof (the count->cost resolution off the real Cost getter). Drive a node-native
        // battle to seat A's turn 1, seed the reducer's spellboost charge, play it, and read the cost the
        // engine actually charged. expectedCost is base(5) - charge, the engine's authentic resolution —
        // and the differing values across charge levels are the non-vacuity (a wrong charge -> wrong cost).
        using var harness = NodeNativeBattleHarness.Create(seatADeck: ReducerDeck());

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted,
            Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted,
            Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted,
            Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        // The reducer dealt at engine Index 1 (deck position 0). Seed the charge on it WHILE it is in hand,
        // then confirm the engine's Cost getter resolved the discount BEFORE the play (pre-play pin).
        int seededHandCost = harness.Engine.SeedHandCardSpellboostCost(playerSeat: true, idx: 1, charge);
        Assert.That(seededHandCost, Is.EqualTo(expectedCost),
            $"engine hand-card Cost must resolve base({SpellboostReducerBaseCost}) - charge({charge})");

        // Play it. With max(0,5-charge) <= 1 for charge 4/5, and charge 0 keeping cost 5 (PP 1 can't pay
        // 5), we only need the cost READ to be correct — but assert acceptance where affordable.
        var play = harness.Push(NetworkBattleUri.PlayActions, PlayBody(1), isPlayerSeat: true);
        if (expectedCost <= 1)
            Assert.That(play.Accepted, Is.True, $"affordable reducer play rejected: {play.RejectReason}");

        // The PAYOFF read: PlayedCardCost returns the engine-resolved play-time cost. For an affordable
        // play this is the captured PlayedCost (post-resolution, card now in cemetery — it is a spell);
        // for the unaffordable charge-0 case the card stays in hand and the live Cost (5) is read. Either
        // way the value equals the engine's resolved discounted cost.
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, idx: 1),
            Is.EqualTo(expectedCost),
            $"PlayedCardCost must equal the engine-resolved cost {expectedCost} at charge {charge}");
    }

    [Test]
    public void Vanilla_play_PlayedCardCost_is_base_cost()
    {
        // A vanilla follower has no cost modifier, so the engine resolves its base cost (1) by
        // construction — the cost the knownList will carry for a non-boosted play.
        var deck = new List<long> { NodeNativeBattleHarness.VanillaFollowerId };
        deck.AddRange(NodeNativeBattleHarness.DefaultDeck());
        deck = deck.GetRange(0, 30);
        using var harness = NodeNativeBattleHarness.Create(seatADeck: deck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True);
        var playIdx = harness.PlayerHandCardIndex(0);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(playIdx), isPlayerSeat: true).Accepted,
            Is.True, "vanilla play");

        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, idx: playIdx), Is.EqualTo(1),
            "a cost-1 vanilla follower resolves to base cost 1");
    }

    [Test]
    public void PlayedCardCost_degrades_to_fallback_for_unknown_idx()
    {
        // Graceful degradation: an idx with no resolved card returns the fallback (non-engine sessions
        // and unmapped idxs never crash the handler).
        using var harness = NodeNativeBattleHarness.Create();
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, idx: 9999, fallback: 7), Is.EqualTo(7));
    }

    // --- HANDLER-EMIT proof: the cost reaches the opponent-facing knownList[].cost ----------------

    // A PlayActions wire frame the HANDLER consumes: it needs an orderList move op for the played idx so
    // BuildPlayedCard can synthesize the entry (the engine resolves the play from playIdx/type alone, but
    // the opponent-facing synthesis is driven by the wire orderList). to:30 == Field.
    private static Dictionary<string, object?> HandlerPlayBody(int playIdx) => new()
    {
        ["playIdx"] = playIdx,
        ["type"] = 30,
        ["orderList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["move"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { (long)playIdx },
                    ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L,
                },
            },
        },
    };

    [Test]
    public void Handler_emits_engine_resolved_cost_on_knownList()
    {
        // The end-to-end payoff: build a FrameDispatchContext over the harness (engine + state +
        // participants), drive to seat A's turn, seed the reducer's charge, INGEST the play (so the engine
        // resolves + captures PlayedCost), then run PlayActionsHandler.Handle and inspect the emitted
        // knownList[0].cost. It must equal the engine-resolved discounted cost (NOT the base cost) —
        // proving the cost-desync is closed by construction at the emit site.
        const int charge = 4;
        const int expectedCost = SpellboostReducerBaseCost - charge; // 1
        using var harness = NodeNativeBattleHarness.Create(seatADeck: ReducerDeck());

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        // Seed the charge so the engine resolves the reducer at cost 1 (affordable on PP 1).
        Assert.That(harness.Engine.SeedHandCardSpellboostCost(playerSeat: true, idx: 1, charge),
            Is.EqualTo(expectedCost), "pre-play resolved hand cost");

        // Ingest the play into the engine (seat A == player) so PlayedCost is captured at resolution.
        var playBody = HandlerPlayBody(1);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, playBody, isPlayerSeat: true).Accepted,
            Is.True, "reducer play ingest");

        // Build the dispatch context the way BattleSession.BuildContext does, with both stubs advanced to
        // AfterReady so the PvP relay gate (BothSidesAfterReady) passes. From == seat A (the sender).
        harness.SeatA.Phase = HandshakePhase.AfterReady;
        harness.SeatB.Phase = HandshakePhase.AfterReady;
        var env = new MsgEnvelope(
            NetworkBattleUri.PlayActions, ViewerId: harness.SeatA.ViewerId, Uuid: "udid-test", Bid: null,
            RetryAttempt: 0, Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(playBody));
        var ctx = new FrameDispatchContext
        {
            A = harness.SeatA, B = harness.SeatB, From = harness.SeatA, Other = harness.SeatB,
            Env = env, BattleId = "test-battle", State = harness.State, Engine = harness.Engine,
        };

        var routes = new PlayActionsHandler().Handle(ctx);

        Assert.That(routes, Has.Count.EqualTo(1), "one route to the opponent");
        var body = routes[0].Frame.Body as PlayActionsBroadcastBody;
        Assert.That(body, Is.Not.Null, "frame body is a PlayActionsBroadcastBody");
        Assert.That(body!.KnownList, Is.Not.Null.And.Count.EqualTo(1), "one knownList entry (the played card)");
        Assert.That(body.KnownList![0].CardId, Is.EqualTo(SpellboostReducerId), "the reducer's identity");
        // THE assertion: the emitted cost is the engine-resolved DISCOUNTED cost (1), not the base (5).
        Assert.That(body.KnownList[0].Cost, Is.EqualTo(expectedCost),
            "knownList[].cost must be the engine-resolved discounted cost, not the base cost");
        Assert.That(body.KnownList[0].Cost, Is.Not.EqualTo(SpellboostReducerBaseCost),
            "non-vacuity: the emitted cost must NOT be the un-discounted base cost");
    }

    // === M-HC-3b: REAL spell-charge accumulation (no seam) =======================================

    // The spellboost GRANTOR 118311030: a cost-3 follower whose when_play spell_charge skill
    // (add_charge=1, target character=me&target=hand&card_type=all) adds +1 spell-charge to EVERY card in
    // the caster's hand on each play. Drives the reducer's charge for real headless — no SeedHandCardSpellboostCost
    // seam. (Its authored SECOND charge skill, add_charge=5, does NOT fire headless — only +1 lands per play;
    // recorded as a known fidelity follow-up, irrelevant to this regression which needs only the +1.)
    private const long SpellboostGrantorId = 118311030;

    // A deck of alternating reducers + grantors so both reliably populate the opening hand and early draws
    // (a single front-loaded reducer would shuffle out of reach). 15 of each = 30.
    private static IReadOnlyList<long> ReducerAndGrantorDeck()
    {
        var deck = new List<long>(30);
        for (int i = 0; i < 15; i++) { deck.Add(SpellboostReducerId); deck.Add(SpellboostGrantorId); }
        return deck;
    }

    // Find the engine Index of the first hand card on seat A with the given wire cardId (the hand is
    // shuffled, so we locate by identity, not position). -1 if not present.
    private static int FindHandIdxByCardId(NodeNativeBattleHarness harness, long cardId)
    {
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)cardId)
                return harness.HandCardIndex(playerSeat: true, i);
        return -1;
    }

    // Ramp seat A to its turn `targetTurn` by alternating TurnStart/TurnEnd A/B; leaves seat A's turn OPEN.
    private void RampToSeatATurn(NodeNativeBattleHarness harness, int targetTurn)
    {
        bool seatA = true;
        while (true)
        {
            Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: seatA).Accepted,
                Is.True, "TurnStart");
            if (seatA && harness.Turn(playerSeat: true) == targetTurn) return;
            Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: seatA).Accepted,
                Is.True, "TurnEnd");
            seatA = !seatA;
        }
    }

    // Ramp seat A to the turn its evolve unlocks (seat A's EvolveWaitTurnCount counts down per seat-A turn),
    // leaving seat A's turn OPEN. Caller must have already ended seat A's first turn (TurnEnd sets
    // NowTurnEvol = true, the other CanEvolution precondition) so the next TurnStart is seat B's. A guard
    // bounds the loop so a never-unlocking bug fails loud instead of hanging.
    private static void RampSeatAToEvolveTurn(NodeNativeBattleHarness harness)
    {
        bool seatA = false; // next TurnStart is seat B's
        int guard = 0;
        while (harness.EvolveWaitTurnCount(playerSeat: true) > 0)
        {
            Assert.That(++guard, Is.LessThan(20), "evolve never unlocked — EvolveWaitTurnCount stuck > 0");
            Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: seatA).Accepted, Is.True, "ramp TurnStart");
            if (seatA && harness.EvolveWaitTurnCount(playerSeat: true) == 0) break; // leave seat A's turn open
            Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: seatA).Accepted, Is.True, "ramp TurnEnd");
            seatA = !seatA;
        }
    }

    [Test]
    public void Real_spell_charge_drops_engine_cost_and_count_no_seam()
    {
        // The committed M-HC-3b closure guard: drive a REAL spell-charge sequence headless (NO
        // SeedHandCardSpellboostCost seam) and assert the engine-sourced COST and SPELLBOOST COUNT the node
        // now emits are both correct by construction. Proves the retired wire-derived bookkeeping is
        // redundant: the engine accumulates the charge itself (each grantor play runs the reducer's own
        // AddSpellChargeCount) and resolves the discount.
        using var harness = NodeNativeBattleHarness.Create(seatADeck: ReducerAndGrantorDeck());

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");

        // Ramp to seat A turn 3 (PP 3) so the cost-3 grantor is affordable.
        RampToSeatATurn(harness, targetTurn: 3);
        Assert.That(harness.Pp(playerSeat: true), Is.EqualTo(3), "seat A PP at turn 3");

        // Locate a reducer + a grantor in the (shuffled) hand by identity.
        int reducerIdx = FindHandIdxByCardId(harness, SpellboostReducerId);
        int grantorIdx = FindHandIdxByCardId(harness, SpellboostGrantorId);
        Assert.That(reducerIdx, Is.GreaterThan(0), "a reducer must be in seat A's opening hand");
        Assert.That(grantorIdx, Is.GreaterThan(0), "a grantor must be in seat A's opening hand");

        // PRE-CHARGE non-vacuity: the reducer resolves to its BASE cost (5) and 0 charge BEFORE any grant.
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(SpellboostReducerBaseCost), "reducer cost is base (5) before any charge");
        Assert.That(harness.Engine.PlayedCardSpellboost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(0), "reducer spell-charge is 0 before any grant");

        // Play the grantor (cost 3). Its when_play spell_charge adds +1 to every hand card — REAL engine
        // resolution, no seam. This runs through the receive conductor (Push -> engine.Receive).
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(grantorIdx), isPlayerSeat: true).Accepted,
            Is.True, "grantor play");

        // THE engine-read assertions: the reducer (still in hand) now reads charge 1 and cost 4 (5 - 1) —
        // accumulated for real by the engine, not seeded.
        Assert.That(harness.Engine.PlayedCardSpellboost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(1), "one grantor play accumulates +1 real spell-charge on the reducer");
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(SpellboostReducerBaseCost - 1),
            "the engine resolves the reducer's cost down to 4 (base 5 - 1 charge), no seam");

        // PERSIST-POST-PLAY proof (the read-moment this milestone chose): advance to seat A's next turn
        // (fresh PP 4, affording the cost-4 reducer), play the reducer (a spell -> cemetery), and confirm
        // PlayedCardSpellboost/PlayedCardCost STILL read 1/4 AFTER the card left the hand — i.e. the zone
        // search reads the persisted count off the resolved card, no receive-capture needed.
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True);
        Assert.That(harness.Pp(playerSeat: true), Is.GreaterThanOrEqualTo(4), "seat A fresh PP affords cost-4 reducer");

        // The reducer's engine Index is stable across turns; play it now.
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(reducerIdx), isPlayerSeat: true).Accepted,
            Is.True, "charged reducer play");
        Assert.That(harness.Engine.PlayedCardSpellboost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(1), "spell-charge persists on the played reducer (now in cemetery)");
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, reducerIdx, fallback: -1),
            Is.EqualTo(SpellboostReducerBaseCost - 1),
            "PlayedCost captured the discounted cost (4) at play time and persists post-play");
    }

    [Test]
    public void Handler_emits_real_engine_spellboost_and_cost_on_knownList()
    {
        // The end-to-end emit payoff for M-HC-3b: a REAL-charged reducer played through the conductor, then
        // PlayActionsHandler.Handle, with BOTH knownList[].cost AND knownList[].spellboost read straight off
        // the engine (no wire-derived bookkeeping). Cost 4 (discounted) + count 1 (real charge).
        using var harness = NodeNativeBattleHarness.Create(seatADeck: ReducerAndGrantorDeck());

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        RampToSeatATurn(harness, targetTurn: 3);

        int reducerIdx = FindHandIdxByCardId(harness, SpellboostReducerId);
        int grantorIdx = FindHandIdxByCardId(harness, SpellboostGrantorId);
        Assert.That(reducerIdx, Is.GreaterThan(0), "reducer in hand");
        Assert.That(grantorIdx, Is.GreaterThan(0), "grantor in hand");

        // Charge the reducer for real (one grantor play -> +1), then advance to a fresh seat A turn that
        // affords the discounted reducer.
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(grantorIdx), isPlayerSeat: true).Accepted,
            Is.True, "grantor play");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: false).Accepted, Is.True);
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True);

        // Ingest the reducer play into the engine (so PlayedCost/SpellChargeCount are captured at resolution).
        var playBody = HandlerPlayBody(reducerIdx);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, playBody, isPlayerSeat: true).Accepted,
            Is.True, "charged reducer play ingest");

        // Build the dispatch context the way BattleSession.BuildContext does; From == seat A (the sender).
        harness.SeatA.Phase = HandshakePhase.AfterReady;
        harness.SeatB.Phase = HandshakePhase.AfterReady;
        var env = new MsgEnvelope(
            NetworkBattleUri.PlayActions, ViewerId: harness.SeatA.ViewerId, Uuid: "udid-test", Bid: null,
            RetryAttempt: 0, Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(playBody));
        var ctx = new FrameDispatchContext
        {
            A = harness.SeatA, B = harness.SeatB, From = harness.SeatA, Other = harness.SeatB,
            Env = env, BattleId = "test-battle", State = harness.State, Engine = harness.Engine,
        };

        var routes = new PlayActionsHandler().Handle(ctx);

        Assert.That(routes, Has.Count.EqualTo(1), "one route to the opponent");
        var body = routes[0].Frame.Body as PlayActionsBroadcastBody;
        Assert.That(body, Is.Not.Null, "frame body is a PlayActionsBroadcastBody");
        Assert.That(body!.KnownList, Is.Not.Null.And.Count.EqualTo(1), "one knownList entry (the played reducer)");
        Assert.That(body.KnownList![0].CardId, Is.EqualTo(SpellboostReducerId), "the reducer's identity");
        // THE assertions: cost is the engine-resolved DISCOUNTED cost (4), spellboost is the REAL count (1).
        Assert.That(body.KnownList[0].Cost, Is.EqualTo(SpellboostReducerBaseCost - 1),
            "knownList[].cost must be the engine-resolved discounted cost (4), not base (5)");
        Assert.That(body.KnownList[0].Spellboost, Is.EqualTo(1),
            "knownList[].spellboost must be the REAL engine-accumulated charge count (1), engine-sourced");
        // Non-vacuity: neither field is the un-charged default.
        Assert.That(body.KnownList[0].Cost, Is.Not.EqualTo(SpellboostReducerBaseCost),
            "non-vacuity: emitted cost is NOT the un-discounted base cost");
        Assert.That(body.KnownList[0].Spellboost, Is.Not.EqualTo(0),
            "non-vacuity: emitted spellboost is NOT 0");
    }

    // === M-HC-4d: BOARD-DEPENDENT (when_evolve_other) cost validated headless =====================

    // The board-dependent cost-reducer 127011020 (base cost 6, neutral 3/3). Its when_evolve_other
    // cost_change skill (skill_option set=1, condition turn=self & {me.hand_self.unit.count}>0 &
    // target=evolution_card & card_type=unit) SETS this card's cost to a flat 1 once ANOTHER of the
    // controller's followers evolves on the controller's turn (with >=1 other unit in hand). Unlike the
    // M-HC-3 spellboost reducer (a SELF when_spell_charge modifier), this reduction is driven by a BOARD
    // EVENT (an evolve) on a DIFFERENT card — so it could only ever be captured once evolve resolves
    // headless (M-HC-4b). Because the node reads opponent-facing cost straight off the resolved engine
    // (PlayedCardCost, M-HC-3), the reduction is captured BY CONSTRUCTION — this test proves it.
    private const long BoardDependentCostCardId = NodeNativeBattleHarness.BoardDependentCostCardId; // 127011020
    private const int BoardDependentCostBase = NodeNativeBattleHarness.BoardDependentCostBase;       // 6
    private const int BoardDependentCostReduced = NodeNativeBattleHarness.BoardDependentCostReduced; // 1

    [Test]
    public void Board_dependent_when_evolve_other_cost_validated_headless()
    {
        // The M-HC-4d payoff. Drive a node-native battle: seat A plays the vanilla follower turn 1, ramps
        // to its evolve turn, and EVOLVES it while a board-dependent cost-reducer (127011020) sits in hand.
        // The reducer's when_evolve_other cost_change (set=1) fires on the evolve, dropping its resolved cost
        // 6 -> 1. We pin the reducer's resolved cost BEFORE the evolve (== base 6) and AFTER (== reduced 1)
        // to prove the EVOLVE caused the reduction (causation, not coincidence), then drive the reducer's play
        // through PlayActionsHandler and assert the emitted knownList[].cost == the reduced cost. This proves
        // the desync the retired wire-derived count->cost calculator would have corrupted is closed by
        // construction: a board event modifies a DIFFERENT card's cost, and the engine read carries it.
        using var harness = NodeNativeBattleHarness.Create(seatADeck: NodeNativeBattleHarness.BoardDependentCostDeck());

        // --- mulligan + open seat A turn 1, play a vanilla follower onto seat A's board -------------
        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        // Locate a vanilla follower in the (shuffled) opening hand and play it (cost 1, affordable on PP 1).
        int vanillaHandIdx = FindHandIdxByCardId(harness, NodeNativeBattleHarness.VanillaFollowerId);
        Assert.That(vanillaHandIdx, Is.GreaterThan(0), "a vanilla follower must be in seat A's opening hand");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(vanillaHandIdx), isPlayerSeat: true).Accepted,
            Is.True, "turn1 vanilla play");
        Assert.That(harness.BoardCount(playerSeat: true), Is.EqualTo(1), "the vanilla is on seat A's board");
        int evolverIdx = harness.InPlayCardIndex(playerSeat: true, boardPos: 0);

        // --- ramp seat A to the turn its evolve unlocks (mirrors Evolve_resolves_on_engine_state_headless) ---
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        RampSeatAToEvolveTurn(harness);
        Assert.That(harness.EvolveWaitTurnCount(playerSeat: true), Is.EqualTo(0), "evolve unlocked on seat A's turn");
        Assert.That(harness.EpCount(playerSeat: true), Is.GreaterThanOrEqualTo(1), "seat A must hold >= 1 EP before evolving");

        // The reducer must be IN HAND across the evolve (its when_evolve_other skill is scanned off the hand).
        int reducerHandIdx = FindHandIdxByCardId(harness, BoardDependentCostCardId);
        Assert.That(reducerHandIdx, Is.GreaterThan(0), "the board-dependent cost-reducer must be in seat A's hand at the evolve");

        // PRE-EVOLVE pin (non-vacuity + causation baseline): the reducer resolves to its BASE cost (6) while
        // no follower has evolved yet. Read it WHILE in hand by its engine Index.
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, reducerHandIdx, fallback: -1),
            Is.EqualTo(BoardDependentCostBase),
            "reducer resolves to its BASE cost (6) BEFORE any follower evolves");
        Assert.That(harness.IsEvolved(playerSeat: true, boardPos: 0), Is.False, "vanilla not yet evolved");

        // --- THE evolve: a plain EVOLUTION frame on the vanilla (boardPos 0) — fires when_evolve_other ----
        var evolve = harness.Push(
            NetworkBattleUri.PlayActions, NodeNativeBattleHarness.EvolveBody(evolverIdx), isPlayerSeat: true);
        Assert.That(evolve.Accepted, Is.True, $"evolve rejected: {evolve.RejectReason}");
        Assert.That(harness.IsEvolved(playerSeat: true, boardPos: 0), Is.True, "the vanilla must be flagged evolved");

        // POST-EVOLVE pin (THE engine-state assertion): the evolve fired the reducer's when_evolve_other
        // cost_change (set=1), so the reducer's resolved cost is now the flat REDUCED cost (1) — 6 -> 1 caused
        // by the evolve. (Engine-derived: the value is the engine's CostSetModifier, not test-set.)
        Assert.That(harness.Engine.PlayedCardCost(playerSeat: true, reducerHandIdx, fallback: -1),
            Is.EqualTo(BoardDependentCostReduced),
            "the evolve must drop the reducer's resolved cost to the SET value (1) via when_evolve_other");

        // --- HANDLER-EMIT proof: the board-reduced cost reaches the opponent-facing knownList[].cost --------
        // Ingest the reducer's play into the engine (cost 1 is affordable on seat A's fresh PP), then run
        // PlayActionsHandler and assert the emitted cost is the board-reduced 1, not the base 6.
        var playBody = HandlerPlayBody(reducerHandIdx);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, playBody, isPlayerSeat: true).Accepted,
            Is.True, "board-reduced reducer play ingest");

        harness.SeatA.Phase = HandshakePhase.AfterReady;
        harness.SeatB.Phase = HandshakePhase.AfterReady;
        var env = new MsgEnvelope(
            NetworkBattleUri.PlayActions, ViewerId: harness.SeatA.ViewerId, Uuid: "udid-test", Bid: null,
            RetryAttempt: 0, Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(playBody));
        var ctx = new FrameDispatchContext
        {
            A = harness.SeatA, B = harness.SeatB, From = harness.SeatA, Other = harness.SeatB,
            Env = env, BattleId = "test-battle", State = harness.State, Engine = harness.Engine,
        };

        var routes = new PlayActionsHandler().Handle(ctx);

        Assert.That(routes, Has.Count.EqualTo(1), "one route to the opponent");
        var body = routes[0].Frame.Body as PlayActionsBroadcastBody;
        Assert.That(body, Is.Not.Null, "frame body is a PlayActionsBroadcastBody");
        Assert.That(body!.KnownList, Is.Not.Null.And.Count.EqualTo(1), "one knownList entry (the played reducer)");
        Assert.That(body.KnownList![0].CardId, Is.EqualTo(BoardDependentCostCardId), "the reducer's identity");
        // THE emit assertion: the opponent-facing cost is the BOARD-REDUCED cost (1), engine-sourced.
        Assert.That(body.KnownList[0].Cost, Is.EqualTo(BoardDependentCostReduced),
            "knownList[].cost must be the engine-resolved BOARD-reduced cost (1), not the base cost (6)");
        // Non-vacuity: the emitted cost must NOT be the un-reduced base — a wire-derived count->cost
        // calculator (the retired path) had no signal for a when_evolve_other event and would have shipped 6.
        Assert.That(body.KnownList[0].Cost, Is.Not.EqualTo(BoardDependentCostBase),
            "non-vacuity: the emitted cost must NOT be the un-reduced base cost (6)");
    }

    // === M-HC-4e: engine-resolved clan/tribe on the knownList =====================================
    //
    // Prod ALWAYS emits clan (int ClanType ordinal) + tribe (comma-joined int TribeType string, "0" for
    // none) on every knownList entry (battle-traffic_tk2_regular.ndjson, e.g.
    // {idx:17,cardId:128821011,...,clan:8,tribe:"7,16",...}). The node now sources both off the resolved
    // engine (SessionBattleEngine.PlayedCardClan/PlayedCardTribe → BattleCardBase.Clan/Tribe), so a
    // skill-applied clan/tribe change rides the wire. The fixture 900231030 (ROYAL/clan 2, LEGION/tribe "2",
    // cost 0) gives concrete non-zero values so the assertion is non-vacuous (NOT the 0/"0" no-tribe default).
    private const long ClanTribeFollowerId = NodeNativeBattleHarness.ClanTribeFollowerId;     // 900231030
    private const int ClanTribeFollowerClan = NodeNativeBattleHarness.ClanTribeFollowerClan;  // 2 (ROYAL)
    private const string ClanTribeFollowerTribe = NodeNativeBattleHarness.ClanTribeFollowerTribe; // "2" (LEGION)

    [Test]
    public void PlayedCardClan_and_Tribe_read_engine_resolved_values()
    {
        // Engine-read proof (mirrors PlayedCardCost_*): drive a node-native battle under a Swordcraft (clan 2)
        // seat A — so the ROYAL fixture is legal — play the cost-0 fixture turn 1, then read clan/tribe off the
        // resolved engine by the played card's engine Index. They must be the engine's LIVE values (clan 2,
        // tribe "2"), in the exact prod wire form.
        using var harness = NodeNativeBattleHarness.Create(
            seatADeck: NodeNativeBattleHarness.ClanTribeDeck(), seatAClass: CardClass.Swordcraft);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        int handIdx = FindHandIdxByCardId(harness, ClanTribeFollowerId);
        Assert.That(handIdx, Is.GreaterThan(0), "the clan/tribe fixture must be in seat A's opening hand");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(handIdx), isPlayerSeat: true).Accepted,
            Is.True, "cost-0 fixture play ingest");

        // The PAYOFF reads: clan/tribe off the resolved engine, in the prod wire form.
        Assert.That(harness.Engine.PlayedCardClan(playerSeat: true, handIdx, fallback: -1),
            Is.EqualTo(ClanTribeFollowerClan),
            "PlayedCardClan must equal the engine-resolved clan (ROYAL == 2)");
        Assert.That(harness.Engine.PlayedCardTribe(playerSeat: true, handIdx),
            Is.EqualTo(ClanTribeFollowerTribe),
            "PlayedCardTribe must equal the engine-resolved tribe in prod wire form (LEGION == \"2\")");
    }

    [Test]
    public void PlayedCardTribe_is_zero_string_for_a_no_tribe_card()
    {
        // The no-tribe form: prod sends tribe "0" (== TribeType.ALL == 0), never empty/omitted. The default
        // vanilla follower (VanillaFollowerId) carries no tribe, so its engine-resolved tribe must render "0".
        using var harness = NodeNativeBattleHarness.Create();

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        int vanillaHandIdx = FindHandIdxByCardId(harness, NodeNativeBattleHarness.VanillaFollowerId);
        Assert.That(vanillaHandIdx, Is.GreaterThan(0), "a vanilla follower must be in seat A's opening hand");
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(vanillaHandIdx), isPlayerSeat: true).Accepted,
            Is.True, "vanilla play ingest");

        Assert.That(harness.Engine.PlayedCardTribe(playerSeat: true, vanillaHandIdx), Is.EqualTo("0"),
            "a no-tribe card's wire tribe is the single \"0\" (TribeType.ALL), never empty");
    }

    [Test]
    public void Handler_emits_engine_resolved_clan_and_tribe_on_knownList()
    {
        // The end-to-end payoff (mirrors Handler_emits_engine_resolved_cost_on_knownList): play the clan/tribe
        // fixture, INGEST it (engine resolves the play), then run PlayActionsHandler.Handle and inspect the
        // emitted knownList[0].clan/.tribe. They must equal the engine-resolved values (clan 2, tribe "2") —
        // proving clan/tribe reach the opponent-facing wire engine-sourced, matching the prod form.
        using var harness = NodeNativeBattleHarness.Create(
            seatADeck: NodeNativeBattleHarness.ClanTribeDeck(), seatAClass: CardClass.Swordcraft);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted,
            Is.True, "turn1 TurnStart");

        int handIdx = FindHandIdxByCardId(harness, ClanTribeFollowerId);
        Assert.That(handIdx, Is.GreaterThan(0), "the clan/tribe fixture must be in seat A's opening hand");

        var playBody = HandlerPlayBody(handIdx);
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, playBody, isPlayerSeat: true).Accepted,
            Is.True, "fixture play ingest");

        harness.SeatA.Phase = HandshakePhase.AfterReady;
        harness.SeatB.Phase = HandshakePhase.AfterReady;
        var env = new MsgEnvelope(
            NetworkBattleUri.PlayActions, ViewerId: harness.SeatA.ViewerId, Uuid: "udid-test", Bid: null,
            RetryAttempt: 0, Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(playBody));
        var ctx = new FrameDispatchContext
        {
            A = harness.SeatA, B = harness.SeatB, From = harness.SeatA, Other = harness.SeatB,
            Env = env, BattleId = "test-battle", State = harness.State, Engine = harness.Engine,
        };

        var routes = new PlayActionsHandler().Handle(ctx);

        Assert.That(routes, Has.Count.EqualTo(1), "one route to the opponent");
        var body = routes[0].Frame.Body as PlayActionsBroadcastBody;
        Assert.That(body, Is.Not.Null, "frame body is a PlayActionsBroadcastBody");
        Assert.That(body!.KnownList, Is.Not.Null.And.Count.EqualTo(1), "one knownList entry (the played fixture)");
        Assert.That(body.KnownList![0].CardId, Is.EqualTo(ClanTribeFollowerId), "the fixture's identity");
        // THE assertions: clan/tribe are the engine-resolved values, in the prod wire form.
        Assert.That(body.KnownList[0].Clan, Is.EqualTo(ClanTribeFollowerClan),
            "knownList[].clan must be the engine-resolved clan ordinal (ROYAL == 2)");
        Assert.That(body.KnownList[0].Tribe, Is.EqualTo(ClanTribeFollowerTribe),
            "knownList[].tribe must be the engine-resolved tribe in prod wire form (LEGION == \"2\")");
        // Non-vacuity: the emitted values must NOT be the 0/"0" no-clan/no-tribe default.
        Assert.That(body.KnownList[0].Clan, Is.Not.EqualTo(0), "non-vacuity: clan must not be the 0 default");
        Assert.That(body.KnownList[0].Tribe, Is.Not.EqualTo("0"), "non-vacuity: tribe must not be the \"0\" default");
    }

    // === M-HC-4f: engine-resolved token identity (cardId) on the knownList =========================
    //
    // The opponent-facing knownList[].cardId is now ENGINE-sourced (PlayActionsHandler reads
    // SessionBattleEngine.PlayedCardId off the resolved card). These tests prove the engine carries the TRUE
    // id for each token case the retired wire-mining used to handle — deck card, generated/substituted token,
    // and choice/Discover token — so the wire-mined idx→cardId bookkeeping for the PLAYED card is redundant.
    // (The deck-map remains as the non-engine-session fallback.)

    [Test]
    public void PlayedCardId_reads_engine_resolved_deck_card_id()
    {
        // BASELINE (proves the read mechanism): a plain DECK card. Play it and assert PlayedCardId returns the
        // seeded deck id — the same value the deck-map fallback would have supplied, read off the engine.
        var deck = new List<long> { NodeNativeBattleHarness.VanillaFollowerId };
        deck.AddRange(NodeNativeBattleHarness.DefaultDeck());
        deck = deck.GetRange(0, 30);
        using var harness = NodeNativeBattleHarness.Create(seatADeck: deck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        int playIdx = harness.PlayerHandCardIndex(0);
        long dealtId = harness.HandCardId(playerSeat: true, handPos: 0); // the engine-seated deck identity at this idx
        Assert.That(harness.Push(NetworkBattleUri.PlayActions, PlayBody(playIdx), isPlayerSeat: true).Accepted,
            Is.True, "deck-card play");

        Assert.That(harness.PlayedCardId(playerSeat: true, idx: playIdx), Is.EqualTo(dealtId),
            "PlayedCardId must return the engine-resolved deck card identity");
        // Cross-check against the deck-map (the retired path's source) so we KNOW the engine read is equivalent
        // for a deck card — the behavior-preserving guarantee for the common case.
        Assert.That(dealtId, Is.EqualTo(harness.SeatADeck[playIdx - 1]),
            "the dealt id equals the node's shuffled deck-map id at this idx (engine read == deck-map fallback)");
    }

    [Test]
    public void PlayedCardId_degrades_to_fallback_for_unknown_idx()
    {
        // Graceful degradation (mirrors PlayedCardCost_degrades_*): a non-engine session / unmapped idx returns
        // the caller's fallback — the deck-map id the handler hands in — never crashing.
        using var harness = NodeNativeBattleHarness.Create();
        Assert.That(harness.PlayedCardId(playerSeat: true, idx: 9999, fallback: 424242L), Is.EqualTo(424242L));
    }

    [Test]
    public void PlayedCardId_reads_substituted_token_identity_off_the_board()
    {
        // GENERATED/SUBSTITUTED TOKEN: M-HC-2 proved a reveal seats the WIRE cardId (overriding the seeded id)
        // via CreateActualCard. This proves PlayedCardId then reads that TRUE id off the resolved card — so a
        // later play of a generated token reveals its real identity engine-sourced, NOT the wire-mined map.
        // Reuse the substitution fixture: seat B's deck is uniformly Z; the reveal substitutes W at idx 1.
        const long Z = NodeNativeBattleHarness.VanillaFollowerId;       // seeded identity
        const long W = NodeNativeBattleHarness.AltVanillaFollowerId;    // the wire (revealed) identity
        var seatBDeck = Enumerable.Repeat(Z, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatBDeck: seatBDeck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");
        Assert.That(harness.Push(NetworkBattleUri.TurnEnd, TurnEndBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnEnd");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: false).Accepted, Is.True, "turn2 TurnStart (B)");

        Assert.That(harness.Push(NetworkBattleUri.PlayActions, RevealPlayBody(idx: 1, cardId: W), isPlayerSeat: false).Accepted,
            Is.True, "seat B reveal-play");
        int boardIdx = harness.InPlayCardIndex(playerSeat: false, boardPos: 0);

        // THE assertion: PlayedCardId reads the SUBSTITUTED wire id W off the resolved board card, not the seeded Z.
        Assert.That(harness.PlayedCardId(playerSeat: false, idx: boardIdx), Is.EqualTo(W),
            "PlayedCardId must read the engine-seated (substituted) wire cardId, not the seeded deck id");
        Assert.That(harness.PlayedCardId(playerSeat: false, idx: boardIdx), Is.Not.EqualTo(Z),
            "non-vacuity: the read is the wire id, NOT the seeded identity at that idx");
    }

    [Test]
    [Explicit("M-HC-4f UNPROVEN case: the engine's autonomous token_draw seats the chosen token at engine " +
              "Index 0 headless (NOT a wire idx past the deck), so PlayedCardId cannot address it by idx — it " +
              "would collide with the leader (also Index 0). This test DOCUMENTS the gap that keeps " +
              "MineChoicePicks wire-mining alive (see TODO(M-HC-4f) in PlayActionsHandler). Run explicitly to " +
              "re-verify the gap; it asserts the FINDING (Index 0), not a passing engine read.")]
    public void PlayedCardId_choice_token_seats_at_index_zero_headless_GAP()
    {
        // CHOICE/Discover TOKEN — the case the engine does NOT resolve at a wire idx headless. Playing the choice
        // card (127011010) and choosing token B (120011010) lands it in hand with the correct IDENTITY, but at
        // engine Index 0 (the autonomous token_draw skill path, not the wire add-op/replace path the relay uses).
        // PlayedCardId(seat, 0) would therefore read the LEADER (Index 0), not the token — so the engine cannot
        // replace MineChoicePicks for this case. (In a real relay the token rides a wire add op that seats a dummy
        // at a non-zero idx, then a replace substitutes the real id there — a path not reproducible cheaply here.)
        var seatADeck = Enumerable.Repeat(NodeNativeBattleHarness.ChoiceCardId, 30).ToList();
        using var harness = NodeNativeBattleHarness.Create(seatADeck: seatADeck);

        Assert.That(harness.Push(NetworkBattleUri.Deal, DealBody(), isPlayerSeat: true).Accepted, Is.True, "Deal");
        Assert.That(harness.Push(NetworkBattleUri.Swap, SwapBody(), isPlayerSeat: true).Accepted, Is.True, "Swap");
        Assert.That(harness.Push(NetworkBattleUri.Ready, ReadyBody(), isPlayerSeat: false).Accepted, Is.True, "Ready");
        Assert.That(harness.Push(NetworkBattleUri.TurnStart, TurnStartBody(), isPlayerSeat: true).Accepted, Is.True, "turn1 TurnStart");

        int choiceIdx = harness.HandCardIndex(playerSeat: true, handPos: 0);
        const long chosen = NodeNativeBattleHarness.ChoiceTokenB; // 120011010
        Assert.That(harness.Push(NetworkBattleUri.PlayActions,
                NodeNativeBattleHarness.ChoicePlayBody(choiceIdx, NodeNativeBattleHarness.ChoiceCardId, chosen),
                isPlayerSeat: true).Accepted,
            Is.True, "choice play (chose token B)");

        // The chosen token IS in hand with the right identity (M-HC-4c proved this) ...
        int tokenIdx = -1;
        for (int i = 0; i < harness.HandCount(playerSeat: true); i++)
            if (harness.HandCardId(playerSeat: true, i) == (int)chosen) { tokenIdx = harness.HandCardIndex(playerSeat: true, i); break; }
        Assert.That(tokenIdx, Is.GreaterThanOrEqualTo(0), "the chosen token (B) must be in seat A's hand");

        // ... but its engine Index is 0 — the documented gap. PlayedCardId(seat, 0) reads the leader, not the token.
        Assert.That(tokenIdx, Is.EqualTo(0),
            "FINDING: the autonomous token_draw seats the chosen token at engine Index 0 headless — not addressable by a wire idx");
    }
}
