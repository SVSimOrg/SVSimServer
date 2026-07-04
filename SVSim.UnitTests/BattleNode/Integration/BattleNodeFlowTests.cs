using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Wire;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Integration;

[TestFixture]
public class BattleNodeFlowTests
{
    private static string MakeKey()
    {
        var seq = 0;
        return NodeCrypto.GenerateKey(() => (seq++ * 13) % 16);
    }

    internal static MatchContext FixtureCtx(IReadOnlyList<long>? deck = null) => new(
        SelfDeckCardIds: deck ?? Enumerable.Range(1, 30).Select(i => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    /// <summary>
    /// End-to-end: a viewer with a real TK2 run sees their drafted card-ids in the Matched
    /// frame's selfDeck. This is the "visible win" — proves the full plumbing chain works
    /// against an actual seeded viewer.
    /// </summary>
    [Test]
    [Timeout(60000)]
    public async Task Matched_frame_contains_drafted_deck_cards()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        var draftedDeck = Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList();

        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
            {
                ViewerId = vid,
                EntryId = 1,
                ClassId = 1,
                LeaderSkinId = 1,
                SelectedCardIdsJson = JsonSerializer.Serialize(draftedDeck),
                IsSelectCompleted = true,
                MaxBattleCount = 5,
                CandidateClassIdsJson = "[1,2,3]",
                PendingPickSetsJson = "[]",
                ResultListJson = "[]",
                NextCandidateId = 1,
            });
            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctx = await builder.BuildForTwoPickAsync(vid);
        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var ct = cts.Token;
        var vidB = vid + 1;
        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vid, ctx),
            new SVSim.BattleNode.Bridge.BattlePlayer(vidB, FixtureCtx()),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        var key = MakeKey();
        // PvP constructs the BattleSession on the SECOND arriver, so connecting only P1 parks it
        // forever. Connect BOTH clients, then drive P1 (the seeded viewer) through
        // InitNetwork/InitBattle to harvest its own Matched — pushed to the sender before the
        // mulligan barrier, so B's handshake is not needed for P1's Matched to arrive.
        var (client, clientB) = await ConnectBothAsync(factory, pending.BattleId, vid, vidB, key, ct);
        await using var _a = client;
        await using var _b = clientB;
        await Task.WhenAll(client.ConsumeHandshakeAsync(ct), clientB.ConsumeHandshakeAsync(ct));

        // InitNetwork → ack
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitNetwork, pubSeq: 1), key, ct);
        await client.ReceiveSynchronizeAsync(ct);

        // InitBattle → Matched (this is the frame we care about)
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitBattle, pubSeq: 2), key, ct);
        var matched = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(matched.Uri, Is.EqualTo(NetworkBattleUri.Matched));

        // MsgEnvelope.FromJson always inflates Body as a RawBody dictionary — selfDeck is a
        // List<object?> of nested dicts with int "idx" + long "cardId" keys.
        var body = ((RawBody)matched.Body).Entries;
        var selfDeck = (List<object?>)body["selfDeck"]!;
        Assert.That(selfDeck.Count, Is.EqualTo(30));

        // The node shuffles each deck per-battle from the master seed (see BattleSeeds /
        // BattleSessionState.GetShuffledDeck), so cardIds are no longer in drafted order. What must
        // hold: idxs are the contiguous 1..30 positions, and the set of cardIds is exactly the
        // drafted deck (a permutation — same multiset, reordered).
        var idxs = new List<long>(30);
        var cardIds = new List<long>(30);
        foreach (var e in selfDeck)
        {
            var entry = (Dictionary<string, object?>)e!;
            idxs.Add((long)entry["idx"]!);
            cardIds.Add((long)entry["cardId"]!);
        }
        Assert.That(idxs, Is.EqualTo(Enumerable.Range(1, 30).Select(i => (long)i)),
            "idxs are the contiguous 1-based positions 1..30");
        Assert.That(cardIds, Is.EquivalentTo(draftedDeck),
            "selfDeck is a permutation of the drafted deck (shuffled, same multiset)");
    }

    private static MsgEnvelope MakeEnvelopeWith(long vid, NetworkBattleUri uri, long pubSeq,
        Dictionary<string, object?>? body = null) =>
        new(uri, ViewerId: vid, Uuid: "udid-test", Bid: null, RetryAttempt: 0,
            Cat: uri == NetworkBattleUri.InitNetwork ? EmitCategory.General
                 : uri == NetworkBattleUri.InitBattle ? EmitCategory.Matching
                 : EmitCategory.Battle,
            PubSeq: pubSeq, PlaySeq: null, Body: new RawBody(body ?? new Dictionary<string, object?>()));

    // -------------------------------------------------------------------------
    // PvP integration tests (Task 12). Drive two parallel RawSocketIoTestClient
    // instances against the same TestServer to exercise the full PvP wire path:
    // pair-handshake to AfterReady, gameplay-frame broadcasting, Retire flipping,
    // mid-game disconnect cascade, and waiting-room timeout.
    // -------------------------------------------------------------------------

    [Test]
    [Timeout(60000)]
    public async Task PvpHandshakeAndGameplay()
    {
        await using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_021UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_022UL);
        await SeedTwoPickRunAsync(factory, vidA, Enumerable.Range(1, 30).Select(i => 100_000_000L + i).ToList());
        await SeedTwoPickRunAsync(factory, vidB, Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList());

        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctxA = await builder.BuildForTwoPickAsync(vidA);
        var ctxB = await builder.BuildForTwoPickAsync(vidB);

        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vidA, ctxA),
            new SVSim.BattleNode.Bridge.BattlePlayer(vidB, ctxB),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = cts.Token;
        var key = MakeKey();

        var (clientA, clientB) = await ConnectBothAsync(factory, pending.BattleId, vidA, vidB, key, ct);
        await using var _a = clientA;
        await using var _b = clientB;
        await Task.WhenAll(clientA.ConsumeHandshakeAsync(ct), clientB.ConsumeHandshakeAsync(ct));

        await DrivePvpHandshakeAsync(clientA, vidA, clientB, vidB, key, ct);

        // Both are now AfterReady. Deterministic-turn handover, mirroring the real two-client
        // capture (2026-06-03 battle_test). A ends its turn; the OPPONENT (B) receives the
        // translated {turnState:0} TurnEnd. A receives nothing — it already ran the turn locally.
        await clientA.SendMsgAsync(MakeEnvelopeWith(vidA, NetworkBattleUri.TurnEnd, pubSeq: 5), key, ct);
        var bTurnEnd = await clientB.ReceiveSynchronizeAsync(ct);
        Assert.That(bTurnEnd.Uri, Is.EqualTo(NetworkBattleUri.TurnEnd));

        // The client rule is: receive opponent TurnEnd -> SendJudge. So B (the taker-over) sends
        // Judge. The {spin:0} reflects BACK to B (its own ControlTurnStartPlayer gate), NOT to A —
        // routing it to A would restart A's turn and stall the loop (the live-run bug this fixes).
        await clientB.SendMsgAsync(MakeEnvelopeWith(vidB, NetworkBattleUri.Judge, pubSeq: 5), key, ct);
        var bJudge = await clientB.ReceiveSynchronizeAsync(ct);
        Assert.That(bJudge.Uri, Is.EqualTo(NetworkBattleUri.Judge));

        // B opens its turn: TurnStart relays to the opponent A as {spin:0} ("opponent's turn").
        await clientB.SendMsgAsync(MakeEnvelopeWith(vidB, NetworkBattleUri.TurnStart, pubSeq: 6), key, ct);
        var aTurnStart = await clientA.ReceiveSynchronizeAsync(ct);
        Assert.That(aTurnStart.Uri, Is.EqualTo(NetworkBattleUri.TurnStart));

        // PlayActions translation: B plays a card; A receives the opponent-facing PlayActions
        // frame (Uri preserved, body synthesized by PlayActionsHandler).
        await clientB.SendMsgAsync(MakeEnvelopeWith(vidB, NetworkBattleUri.PlayActions, pubSeq: 7), key, ct);
        var aForwarded = await clientA.ReceiveSynchronizeAsync(ct);
        Assert.That(aForwarded.Uri, Is.EqualTo(NetworkBattleUri.PlayActions));
    }

    [Test]
    [Timeout(60000)]
    public async Task PvpRetireFlipsResult()
    {
        await using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_031UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_032UL);
        await SeedTwoPickRunAsync(factory, vidA, Enumerable.Range(1, 30).Select(i => 100_000_000L + i).ToList());
        await SeedTwoPickRunAsync(factory, vidB, Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList());

        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctxA = await builder.BuildForTwoPickAsync(vidA);
        var ctxB = await builder.BuildForTwoPickAsync(vidB);

        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vidA, ctxA),
            new SVSim.BattleNode.Bridge.BattlePlayer(vidB, ctxB),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = cts.Token;
        var key = MakeKey();
        var (clientA, clientB) = await ConnectBothAsync(factory, pending.BattleId, vidA, vidB, key, ct);
        await using var _a = clientA;
        await using var _b = clientB;
        await Task.WhenAll(clientA.ConsumeHandshakeAsync(ct), clientB.ConsumeHandshakeAsync(ct));
        await DrivePvpHandshakeAsync(clientA, vidA, clientB, vidB, key, ct);

        // A retires.
        await clientA.SendMsgAsync(MakeEnvelopeWith(vidA, NetworkBattleUri.Retire, pubSeq: 5), key, ct);

        var aFinish = await clientA.ReceiveSynchronizeAsync(ct);
        var bFinish = await clientB.ReceiveSynchronizeAsync(ct);

        Assert.That(aFinish.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        Assert.That(bFinish.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        var aBody = (RawBody)aFinish.Body;
        var bBody = (RawBody)bFinish.Body;
        // BattleResult.RetireLose = 106 (retirer), RetireWin = 105 (survivor). Player-
        // perspective codes per the FinishBattleEffect trace.
        Assert.That((long)aBody.Entries["result"]!, Is.EqualTo((long)SVSim.BattleNode.Protocol.BattleResult.RetireLose));
        Assert.That((long)bBody.Entries["result"]!, Is.EqualTo((long)SVSim.BattleNode.Protocol.BattleResult.RetireWin));
    }

    [Test]
    [Timeout(60000)]
    public async Task PvpMidGameDisconnect_FullCascade()
    {
        await using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_041UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_042UL);
        await SeedTwoPickRunAsync(factory, vidA, Enumerable.Range(1, 30).Select(i => 100_000_000L + i).ToList());
        await SeedTwoPickRunAsync(factory, vidB, Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList());

        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctxA = await builder.BuildForTwoPickAsync(vidA);
        var ctxB = await builder.BuildForTwoPickAsync(vidB);

        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vidA, ctxA),
            new SVSim.BattleNode.Bridge.BattlePlayer(vidB, ctxB),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var ct = cts.Token;
        var key = MakeKey();
        var (clientA, clientB) = await ConnectBothAsync(factory, pending.BattleId, vidA, vidB, key, ct);
        await using var _b = clientB;
        await Task.WhenAll(clientA.ConsumeHandshakeAsync(ct), clientB.ConsumeHandshakeAsync(ct));
        await DrivePvpHandshakeAsync(clientA, vidA, clientB, vidB, key, ct);

        // Abruptly close A's WS (no Retire).
        await clientA.DisposeAsync();

        // B should receive BattleFinish(DisconnectWin) within a few seconds.
        var bFinish = await clientB.ReceiveSynchronizeAsync(ct);
        Assert.That(bFinish.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));
        var bBody = (RawBody)bFinish.Body;
        Assert.That((long)bBody.Entries["result"]!, Is.EqualTo((long)SVSim.BattleNode.Protocol.BattleResult.DisconnectWin));

        // PendingBattle should be evicted by the second arriver's RemovePending.
        var store = factory.Services.GetRequiredService<SVSim.BattleNode.Sessions.IBattleSessionStore>();
        Assert.That(store.TryGetPending(pending.BattleId), Is.Null);
    }

    [Test]
    [Timeout(75000)]
    public async Task PvpWaitingRoomTimeout()
    {
        // The factory uses BattleNodeOptions.WaitingRoomTimeout default = 60s. We wait the
        // full 60s + grace. The plan permits this fallback if WithWebHostBuilder doesn't
        // play nicely with SVSimTestFactory's SQLite-bound override (the factory's
        // ConfigureWebHost replaces the DbContext per-instance against a private SqliteConnection;
        // composing WithWebHostBuilder on top creates a second host that shares the connection
        // but re-runs CreateHost — risking double EnsureCreated / re-seed against the same DB).
        await using var factory = new SVSimTestFactory();
        var vidA = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_051UL);
        var vidB = await factory.SeedViewerAsync(steamId: 76_561_198_000_000_052UL);
        await SeedTwoPickRunAsync(factory, vidA, Enumerable.Range(1, 30).Select(i => 100_000_000L + i).ToList());
        await SeedTwoPickRunAsync(factory, vidB, Enumerable.Range(1, 30).Select(i => 200_000_000L + i).ToList());

        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctxA = await builder.BuildForTwoPickAsync(vidA);
        var ctxB = await builder.BuildForTwoPickAsync(vidB);

        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vidA, ctxA),
            new SVSim.BattleNode.Bridge.BattlePlayer(vidB, ctxB),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(70));
        var ct = cts.Token;
        var key = MakeKey();
        var encA = NodeCrypto.EncryptForNode(vidA.ToString(), key);
        var wsClient = factory.Server.CreateWebSocketClient();
        var wsA = await wsClient.ConnectAsync(
            new Uri($"ws://localhost/socket.io/?BattleId={pending.BattleId}&viewerId={Uri.EscapeDataString(encA)}&EIO=3&transport=websocket"),
            ct);
        // NOTE: ConsumeHandshakeAsync is NOT called here. The EIO Open frame is sent inside
        // RealParticipant.RunAsync, which only runs once the session is constructed by the
        // SECOND arriver. The first arriver who times out never receives that frame — the
        // handler parks them in AwaitSessionFinishedAsync, the waiting-room timer fires, and
        // the polite-close path emits an EIO "1" Close text frame followed by a clean
        // WebSocket close handshake before the handler returns.
        bool politeFrameObserved = false;
        bool closeObserved = false;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var buf = new byte[1024];
        while (!closeObserved && sw.Elapsed < TimeSpan.FromSeconds(65))
        {
            try
            {
                var rr = await wsA.ReceiveAsync(new ArraySegment<byte>(buf), ct);
                if (rr.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    closeObserved = true;
                    break;
                }
                if (rr.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    var text = System.Text.Encoding.UTF8.GetString(buf, 0, rr.Count);
                    if (text == "1") politeFrameObserved = true;
                }
            }
            catch
            {
                // Aborted / cancelled / WebSocketException — server-side close observed.
                closeObserved = true;
                break;
            }
        }
        Assert.That(politeFrameObserved, Is.True,
            "A's WS should receive an EIO '1' Close text frame before teardown (polite-close contract).");
        Assert.That(closeObserved, Is.True,
            "A's WS should close (or ReceiveAsync should fail) after the waiting-room timeout.");
        wsA.Dispose();

        var store = factory.Services.GetRequiredService<SVSim.BattleNode.Sessions.IBattleSessionStore>();
        Assert.That(store.TryGetPending(pending.BattleId), Is.Null);
    }

    // -------------------------------------------------------------------------
    // Bot integration test (Task 13). Single client, full Bot lifecycle: handshake
    // through Swap (asserting NO Matched / BattleStart / Deal pushes), TurnEnd cycles
    // each producing a single Judge back, Retire → BattleFinish. Reference:
    // docs/api-spec/in-battle/ai-passive.md.
    // -------------------------------------------------------------------------

    [Test]
    [Timeout(30000)]
    public async Task BotBattle_FullLifecycle()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();
        await factory.SeedGlobalsAsync();
        await factory.SeedDeckAsync(vid, SVSim.Database.Enums.Format.Rotation, 1);

        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();
        using var scope = factory.Services.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<SVSim.EmulatedEntrypoint.Services.IMatchContextBuilder>();
        var ctx = await builder.BuildForRankBattleAsync(vid, SVSim.Database.Enums.Format.Rotation, deckNo: 1);
        var pending = bridge.RegisterBattle(
            new SVSim.BattleNode.Bridge.BattlePlayer(vid, ctx),
            p2: null,
            SVSim.BattleNode.Sessions.BattleType.Bot);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var ct = cts.Token;
        var key = MakeKey();
        var encryptedVid = NodeCrypto.EncryptForNode(vid.ToString(), key);
        var wsUri = new Uri($"ws://localhost/socket.io/?BattleId={pending.BattleId}&viewerId={Uri.EscapeDataString(encryptedVid)}&EIO=3&transport=websocket");

        var wsClient = factory.Server.CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(wsUri, ct);
        await using var client = new RawSocketIoTestClient(ws);
        await client.ConsumeHandshakeAsync(ct);

        // InitNetwork → ack.
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitNetwork, pubSeq: 1), key, ct);
        var ack1 = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(ack1.Uri, Is.EqualTo(NetworkBattleUri.InitNetwork));

        // InitBattle → ack (NOT Matched). The client's AI flow doesn't gate on
        // Matched and pushing BattleStart later corrupts OppoBattleStartInfo, so
        // Bot mode keeps the handshake silent (just an ack).
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitBattle, pubSeq: 2), key, ct);
        var ack2 = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(ack2.Uri, Is.EqualTo(NetworkBattleUri.InitBattle),
            "Bot's InitBattle is ack-only — no Matched envelope.");

        // Loaded → silent. Send Swap right after; the next inbound must be SwapResponse
        // (no orphan BattleStart / Deal in the queue).
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.Loaded, pubSeq: 3), key, ct);
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.Swap, pubSeq: 4,
            body: new Dictionary<string, object?> { ["idxList"] = new List<object?>() }), key, ct);
        var swapResp = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(swapResp.Uri, Is.EqualTo(NetworkBattleUri.Swap),
            "Expected Swap response (mulligan ack). Got " + swapResp.Uri + " — Loaded may have leaked a frame.");
        var readyResp = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(readyResp.Uri, Is.EqualTo(NetworkBattleUri.Ready));

        // TurnEnd → Judge back.
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.TurnEnd, pubSeq: 5), key, ct);
        var judge1 = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(judge1.Uri, Is.EqualTo(NetworkBattleUri.Judge));

        // Second TurnEnd → another Judge back.
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.TurnEnd, pubSeq: 6), key, ct);
        var judge2 = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(judge2.Uri, Is.EqualTo(NetworkBattleUri.Judge));

        // Retire → BattleFinish.
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.Retire, pubSeq: 7), key, ct);
        var finish = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(finish.Uri, Is.EqualTo(NetworkBattleUri.BattleFinish));

        // PendingBattle evicted at session start.
        var store = factory.Services.GetRequiredService<SVSim.BattleNode.Sessions.IBattleSessionStore>();
        Assert.That(store.TryGetPending(pending.BattleId), Is.Null,
            "PendingBattle should be evicted at session start.");
    }

    // -- helpers -------------------------------------------------------------

    /// <summary>Drives one PvP client from InitNetwork through Swap, stopping at the
    /// SwapResponse. Ready is NOT received here — the mulligan barrier withholds it until
    /// BOTH sides have swapped, so the caller drains it after driving both sides.</summary>
    private static async Task DriveThroughSwapAsync(
        RawSocketIoTestClient client, long vid, string key, CancellationToken ct)
    {
        long pubSeq = 1;
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitNetwork, pubSeq++), key, ct);
        await client.ReceiveSynchronizeAsync(ct); // InitNetwork ack
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.InitBattle, pubSeq++), key, ct);
        var matched = await client.ReceiveSynchronizeAsync(ct);
        Assert.That(matched.Uri, Is.EqualTo(NetworkBattleUri.Matched));
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.Loaded, pubSeq++), key, ct);
        await client.ReceiveSynchronizeAsync(ct); // BattleStart
        await client.ReceiveSynchronizeAsync(ct); // Deal
        await client.SendMsgAsync(MakeEnvelopeWith(vid, NetworkBattleUri.Swap, pubSeq++,
            body: new Dictionary<string, object?> { ["idxList"] = new List<object?>() }), key, ct);
        await client.ReceiveSynchronizeAsync(ct); // Swap response
    }

    /// <summary>Drives both PvP clients through the full handshake including the mulligan
    /// barrier: each side swaps first (Ready withheld), then the second swap releases Ready
    /// to both. Leaves both at AfterReady with pubSeq up to 4 consumed per client.</summary>
    private static async Task DrivePvpHandshakeAsync(
        RawSocketIoTestClient clientA, long vidA,
        RawSocketIoTestClient clientB, long vidB, string key, CancellationToken ct)
    {
        await DriveThroughSwapAsync(clientA, vidA, key, ct);
        await DriveThroughSwapAsync(clientB, vidB, key, ct);

        // B's Swap (the second) releases Ready to both sides.
        var aReady = await clientA.ReceiveSynchronizeAsync(ct);
        Assert.That(aReady.Uri, Is.EqualTo(NetworkBattleUri.Ready));
        var bReady = await clientB.ReceiveSynchronizeAsync(ct);
        Assert.That(bReady.Uri, Is.EqualTo(NetworkBattleUri.Ready));
    }

    private static async Task<(RawSocketIoTestClient, RawSocketIoTestClient)> ConnectBothAsync(
        SVSimTestFactory factory, string battleId, long vidA, long vidB, string key, CancellationToken ct)
    {
        var encA = NodeCrypto.EncryptForNode(vidA.ToString(), key);
        var encB = NodeCrypto.EncryptForNode(vidB.ToString(), key);
        var uriA = new Uri($"ws://localhost/socket.io/?BattleId={battleId}&viewerId={Uri.EscapeDataString(encA)}&EIO=3&transport=websocket");
        var uriB = new Uri($"ws://localhost/socket.io/?BattleId={battleId}&viewerId={Uri.EscapeDataString(encB)}&EIO=3&transport=websocket");

        var wsClient = factory.Server.CreateWebSocketClient();
        // A's HTTP handler will Park (block in AwaitSessionFinishedAsync) until B connects.
        // TestServer's CreateWebSocketClient returns once the WS upgrade response is flushed,
        // which happens at AcceptWebSocketAsync — well before Park. But to be safe against
        // any in-process buffering, start A's connect and yield briefly so its request thread
        // reaches Park before B's connect arrives.
        var connectATask = wsClient.ConnectAsync(uriA, ct);
        await Task.Delay(50, ct);
        var wsB = await wsClient.ConnectAsync(uriB, ct);
        var wsA = await connectATask;
        return (new RawSocketIoTestClient(wsA), new RawSocketIoTestClient(wsB));
    }

    private static async Task SeedTwoPickRunAsync(SVSimTestFactory factory, long vid, IReadOnlyList<long> deck)
    {
        using var seedScope = factory.Services.CreateScope();
        var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.ViewerArenaTwoPickRuns.Add(new ViewerArenaTwoPickRun
        {
            ViewerId = vid,
            EntryId = 1,
            ClassId = 1,
            LeaderSkinId = 1,
            SelectedCardIdsJson = JsonSerializer.Serialize(deck),
            IsSelectCompleted = true,
            MaxBattleCount = 5,
            CandidateClassIdsJson = "[1,2,3]",
            PendingPickSetsJson = "[]",
            ResultListJson = "[]",
            NextCandidateId = 1,
        });
        await db.SaveChangesAsync();
    }
}
