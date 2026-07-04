using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Wire;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Integration;

/// <summary>
/// Wire-shape conformance of our server-authored synchronize frames against real prod TK2
/// captures (<c>data_dumps/captures/battle-traffic_tk2_regular.ndjson</c> +
/// <c>…_tk2_second.ndjson</c>, captured 2026-05-31 from a real client mid-PvP).
///
/// <para><b>What this guards:</b> for every frame our server *authors* (as opposed to forwarding a
/// client's bytes), the payload it emits must carry every key prod sent, with a matching value
/// *category* (object / array / string / number / bool). This is the bug class that has bitten the
/// node repeatedly — wrong casing (<c>card_id</c> vs <c>cardID</c>), a missing field the client
/// reads without a guard, or a string where the client expects a number. The existing
/// <see cref="BattleNodeFlowTests"/> assert frame *ordering and routing*; they never inspect the
/// body. This closes that gap and turns the prod captures into a permanent regression oracle that
/// survives the June-2026 server shutdown.</para>
///
/// <para><b>Direction of the check is capture ⊆ ours</b> — we must emit at least what prod emits
/// (missing/miscased/mistyped = fail), but we may emit extra envelope fields (we send
/// <c>viewerId/uuid/try/cat</c> on pushes; prod's receive frames omit them). Pure
/// envelope/sequencing keys (<c>viewerId, uuid, try, cat, bid, pubSeq, playSeq</c>) are excluded
/// from the comparison: they're transport concerns assigned by the sequencer, covered by the
/// reliability layer + integration tests, and legitimately vary per frame (e.g. the no-stock
/// <c>BattleFinish</c> frame is played immediately whether or not it carries a <c>playSeq</c>).
/// The check is on *body shape*.</para>
///
/// <para><b>Coverage:</b> a two-client PvP session emits all ten server-authored URIs
/// (<c>InitNetwork, Matched, BattleStart, Deal, Swap, Ready, TurnStart, TurnEnd, Judge,
/// BattleFinish</c>). PvP authors the handshake/mulligan frames through the same shared
/// <see cref="SVSim.BattleNode.Lifecycle.ServerBattleFrames"/> builders, and the turn cycle
/// (<c>TurnStart/TurnEnd/Judge</c>) falls out of the real two-client handover. Forwarded frames
/// (<c>PlayActions / TurnEndActions / ChatStamp / TurnEndFinal</c>) relay the
/// client's own bytes verbatim, so their shape is the client's contract, not ours — out of scope
/// here.</para>
/// </summary>
[TestFixture]
public class CaptureConformanceTests
{
    // Top-level keys that are envelope/transport, not body shape. Excluded from the comparison
    // at the root level only (nested objects never contain these).
    private static readonly HashSet<string> IgnoredEnvelopeKeys = new()
    {
        "viewerId", "uuid", "try", "cat", "bid", "pubSeq", "playSeq",
    };

    [Test]
    [Timeout(60000)]
    public async Task ServerAuthoredFrames_MatchProdCaptureShapes()
    {
        await using var factory = new SVSimTestFactory();
        var bridge = factory.Services.GetRequiredService<IMatchingBridge>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var ct = cts.Token;

        // Two-client PvP drive. PvP authors the same handshake/mulligan frames the old Scripted
        // path did (via the shared server-frame builders) PLUS the turn-cycle frames
        // (TurnStart/TurnEnd/Judge) the scripted bot used to fake — so a two-client session
        // harvests all ten server-authored URIs. The shape check is category-based, so PvP's
        // spin:0 still matches prod's spin:189.
        const long vidA = 906243102L;
        const long vidB = 847666884L;
        var pending = bridge.RegisterBattle(
            new BattlePlayer(vidA, BattleNodeFlowTests.FixtureCtx()),
            new BattlePlayer(vidB, BattleNodeFlowTests.FixtureCtx()),
            SVSim.BattleNode.Sessions.BattleType.Pvp);

        var key = MakeKey();
        var (clientA, clientB) = await ConnectBothAsync(factory, pending.BattleId, vidA, vidB, key, ct);
        await using var _a = clientA;
        await using var _b = clientB;
        await Task.WhenAll(clientA.ConsumeHandshakeAsync(ct), clientB.ConsumeHandshakeAsync(ct));

        var harvested = new Dictionary<NetworkBattleUri, MsgEnvelope>();
        void Harvest(MsgEnvelope env) => harvested[env.Uri] = env;

        long seqA = 1, seqB = 1;

        // A walks the handshake; Ready is withheld by the mulligan barrier until B also swaps.
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.InitNetwork, seqA++), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // InitNetwork ack
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.InitBattle, seqA++), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // Matched
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.Loaded, seqA++), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // BattleStart
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // Deal
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.Swap, seqA++,
            new Dictionary<string, object?> { ["idxList"] = new List<object?>() }), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // Swap response

        // B walks the handshake; B's Swap (the second) releases Ready to both sides.
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.InitNetwork, seqB++), key, ct);
        await clientB.ReceiveSynchronizeAsync(ct);                         // ack
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.InitBattle, seqB++), key, ct);
        await clientB.ReceiveSynchronizeAsync(ct);                         // Matched
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.Loaded, seqB++), key, ct);
        await clientB.ReceiveSynchronizeAsync(ct);                         // BattleStart
        await clientB.ReceiveSynchronizeAsync(ct);                         // Deal
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.Swap, seqB++,
            new Dictionary<string, object?> { ["idxList"] = new List<object?>() }), key, ct);
        await clientB.ReceiveSynchronizeAsync(ct);                         // B Swap response
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // Ready (released to A)
        await clientB.ReceiveSynchronizeAsync(ct);                         // Ready to B

        // Turn cycle: A ends turn -> B receives TurnEnd{turnState}. B sends Judge -> Judge{spin}
        // reflects to B. B sends TurnStart -> A receives TurnStart{spin}.
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.TurnEnd, seqA++), key, ct);
        Harvest(await clientB.ReceiveSynchronizeAsync(ct));                 // TurnEnd
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.Judge, seqB++), key, ct);
        Harvest(await clientB.ReceiveSynchronizeAsync(ct));                 // Judge
        await clientB.SendMsgAsync(MakeEnvelope(vidB, NetworkBattleUri.TurnStart, seqB++), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // TurnStart

        // BattleFinish: A retires.
        await clientA.SendMsgAsync(MakeEnvelope(vidA, NetworkBattleUri.Retire, seqA++), key, ct);
        Harvest(await clientA.ReceiveSynchronizeAsync(ct));                 // BattleFinish

        // Compare each harvested frame's wire JSON against the prod capture fixture.
        using var fixtures = JsonDocument.Parse(ProdCaptureFixture.Json);
        var failures = new List<string>();

        foreach (var uriName in ExpectedUris)
        {
            var uri = Enum.Parse<NetworkBattleUri>(uriName);
            if (!harvested.TryGetValue(uri, out var env))
            {
                failures.Add($"[{uriName}] our server never pushed this frame during the PvP lifecycle.");
                continue;
            }

            var expected = fixtures.RootElement.GetProperty(uriName);
            using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
            CompareSubset(expected, ourDoc.RootElement, uriName, isRoot: true, failures);
        }

        if (failures.Count > 0)
        {
            Assert.Fail(
                "Server-authored frames diverge from the prod TK2 capture shapes:\n  - " +
                string.Join("\n  - ", failures));
        }
    }

    private static async Task<(RawSocketIoTestClient, RawSocketIoTestClient)> ConnectBothAsync(
        SVSimTestFactory factory, string battleId, long vidA, long vidB, string key, CancellationToken ct)
    {
        var encA = NodeCrypto.EncryptForNode(vidA.ToString(), key);
        var encB = NodeCrypto.EncryptForNode(vidB.ToString(), key);
        var uriA = new Uri($"ws://localhost/socket.io/?BattleId={battleId}&viewerId={Uri.EscapeDataString(encA)}&EIO=3&transport=websocket");
        var uriB = new Uri($"ws://localhost/socket.io/?BattleId={battleId}&viewerId={Uri.EscapeDataString(encB)}&EIO=3&transport=websocket");

        var wsClient = factory.Server.CreateWebSocketClient();
        var connectATask = wsClient.ConnectAsync(uriA, ct);
        await Task.Delay(50, ct);
        var wsB = await wsClient.ConnectAsync(uriB, ct);
        var wsA = await connectATask;
        return (new RawSocketIoTestClient(wsA), new RawSocketIoTestClient(wsB));
    }

    private static readonly string[] ExpectedUris =
    {
        "InitNetwork", "Matched", "BattleStart", "Deal", "Swap", "Ready",
        "TurnStart", "TurnEnd", "Judge", "BattleFinish",
    };

    /// <summary>
    /// Recursively assert every key/element in <paramref name="expected"/> (the prod capture)
    /// exists in <paramref name="actual"/> (our wire JSON) with a matching value category.
    /// </summary>
    private static void CompareSubset(JsonElement expected, JsonElement actual, string path,
        bool isRoot, List<string> failures)
    {
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                if (actual.ValueKind != JsonValueKind.Object)
                {
                    failures.Add($"{path}: prod is an object, ours is {actual.ValueKind}");
                    return;
                }
                foreach (var prop in expected.EnumerateObject())
                {
                    if (isRoot && IgnoredEnvelopeKeys.Contains(prop.Name)) continue;
                    if (!actual.TryGetProperty(prop.Name, out var av))
                    {
                        failures.Add($"{path}.{prop.Name}: MISSING — prod sends this key, we don't");
                        continue;
                    }
                    CompareSubset(prop.Value, av, $"{path}.{prop.Name}", isRoot: false, failures);
                }
                break;

            case JsonValueKind.Array:
                if (actual.ValueKind != JsonValueKind.Array)
                {
                    failures.Add($"{path}: prod is an array, ours is {actual.ValueKind}");
                    return;
                }
                if (expected.GetArrayLength() > 0)
                {
                    if (actual.GetArrayLength() == 0)
                    {
                        failures.Add($"{path}: prod array is non-empty, ours is empty");
                        return;
                    }
                    // Arrays here are uniform (decks, pos/idx lists) — element 0 defines the shape.
                    CompareSubset(expected[0], actual[0], $"{path}[0]", isRoot: false, failures);
                }
                break;

            case JsonValueKind.Null:
                // Can't infer an expected type from a null; accept whatever we emit.
                break;

            default:
                var ec = Category(expected.ValueKind);
                var ac = Category(actual.ValueKind);
                if (ec != ac)
                {
                    failures.Add(
                        $"{path}: type mismatch — prod is {ec} ({Trunc(expected)}), ours is {ac} ({Trunc(actual)})");
                }
                break;
        }
    }

    private static string Category(JsonValueKind k) => k switch
    {
        JsonValueKind.String => "string",
        JsonValueKind.Number => "number",
        JsonValueKind.True or JsonValueKind.False => "bool",
        JsonValueKind.Null => "null",
        JsonValueKind.Object => "object",
        JsonValueKind.Array => "array",
        _ => k.ToString(),
    };

    private static string Trunc(JsonElement el)
    {
        var s = el.GetRawText();
        return s.Length > 40 ? s[..40] + "…" : s;
    }

    private static MsgEnvelope MakeEnvelope(long vid, NetworkBattleUri uri, long pubSeq,
        Dictionary<string, object?>? body = null) =>
        new(uri, ViewerId: vid, Uuid: "udid-test", Bid: null, RetryAttempt: 0,
            Cat: uri == NetworkBattleUri.InitNetwork ? EmitCategory.General
                 : uri == NetworkBattleUri.InitBattle ? EmitCategory.Matching
                 : EmitCategory.Battle,
            PubSeq: pubSeq, PlaySeq: null, Body: new RawBody(body ?? new Dictionary<string, object?>()));

    private static string MakeKey()
    {
        var seq = 0;
        return NodeCrypto.GenerateKey(() => (seq++ * 13) % 16);
    }

    [Test]
    public void SynthesizedKnownList_matches_prod_recv_PlayActions_entry_shape()
    {
        // Prod recv PlayActions knownList entry (battle-traffic_tk2_regular.ndjson:27).
        const string prodEntry = """
        { "idx": 17, "cardId": 128821011, "to": 20, "cost": 2, "clan": 8, "tribe": "7,16", "spellboost": 0, "attachTarget": "" }
        """;

        // Build the same entry through our synthesizer.
        var deckMap = new Dictionary<int, long> { [17] = 128821011L };
        var orderList = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["move"] = new Dictionary<string, object?>
                {
                    ["idx"] = new List<object?> { 17L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 20L,
                }
            }
        };
        var entry = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.BuildPlayedCard(17, deckMap[17], orderList);
        Assert.That(entry, Is.Not.Null);

        var body = new SVSim.BattleNode.Protocol.Bodies.PlayActionsBroadcastBody(
            PlayIdx: 17, Type: 30, KnownList: new[] { entry! }, OppoTargetList: null);
        var env = new MsgEnvelope(NetworkBattleUri.PlayActions, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: body);

        using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
        var ourEntry = ourDoc.RootElement.GetProperty("knownList")[0];
        using var prodDoc = JsonDocument.Parse(prodEntry);

        // This is a pure-BUILDER shape check: idx/cardId/to are synthesized here; cost/clan/tribe are
        // ENGINE-sourced at the handler (PlayActionsHandler reads them off the resolved engine and passes them
        // to BuildPlayedCard — covered by HeadlessConductorTests), so this builder-only call leaves them at
        // their defaults and we assert only the structural keys.
        foreach (var key in new[] { "idx", "cardId", "to" })
        {
            Assert.That(ourEntry.TryGetProperty(key, out var ours), Is.True, $"knownList entry missing '{key}'");
            var prodVal = prodDoc.RootElement.GetProperty(key);
            Assert.That(ours.ValueKind, Is.EqualTo(prodVal.ValueKind), $"'{key}' type category mismatch");
        }
        Assert.That(ourEntry.GetProperty("cardId").GetInt64(), Is.EqualTo(128821011L));
    }

    [Test]
    public void SynthesizedKnownList_for_a_generated_token_matches_prod_recv_shape()
    {
        // Prod recv PlayActions for a PLAYED token (battle-traffic_tk2_regular.ndjson:96):
        // the token's cardId was generated by an earlier add op, not present in any deck.
        const string prodEntry = """
        { "idx": 38, "cardId": 900811111, "to": 20, "cost": 1, "clan": 8, "tribe": "0", "spellboost": 0, "attachTarget": "" }
        """;

        // Compose the two new pure pieces: mine the token from a generating frame's add op,
        // then build the played-card entry from the resulting map.
        var generatingOrderList = new List<object?>
        {
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 38L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?> { ["cardId"] = 900811111L } } },
        };
        var map = new Dictionary<int, long>();
        foreach (var (idx, cardId, _) in SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.MineAddOps(generatingOrderList))
            map[idx] = cardId;

        var playOrderList = new List<object?>
        {
            new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 38L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 20L } },
        };
        var entry = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.BuildPlayedCard(38, map[38], playOrderList);
        Assert.That(entry, Is.Not.Null, "the mined token resolves to a knownList entry");

        var body = new SVSim.BattleNode.Protocol.Bodies.PlayActionsBroadcastBody(
            PlayIdx: 38, Type: 30, KnownList: new[] { entry! }, OppoTargetList: null);
        var env = new MsgEnvelope(NetworkBattleUri.PlayActions, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: body);

        using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
        var ourEntry = ourDoc.RootElement.GetProperty("knownList")[0];
        using var prodDoc = JsonDocument.Parse(prodEntry);

        // Pure-BUILDER shape check: idx/cardId/to are synthesized here; cost/clan/tribe are ENGINE-sourced at
        // the handler (covered by HeadlessConductorTests), so this builder-only call leaves them at defaults.
        foreach (var key in new[] { "idx", "cardId", "to" })
        {
            Assert.That(ourEntry.TryGetProperty(key, out var ours), Is.True, $"knownList entry missing '{key}'");
            var prodVal = prodDoc.RootElement.GetProperty(key);
            Assert.That(ours.ValueKind, Is.EqualTo(prodVal.ValueKind), $"'{key}' type category mismatch");
        }
        Assert.That(ourEntry.GetProperty("cardId").GetInt64(), Is.EqualTo(900811111L));
        Assert.That(ourEntry.GetProperty("to").GetInt32(), Is.EqualTo(20));
    }

    [Test]
    public void RelayedUList_matches_prod_recv_uList_shape()
    {
        // Prod recv PlayActions uList entry (battle-traffic_tk2_regular.ndjson:75) — a deck-fetch the
        // official node relayed to the opponent. We forward it verbatim; assert the always-present keys
        // round-trip with matching value-kinds (capture ⊆ ours).
        const string prodEntry = """
        { "idxList": [16, 22], "from": 0, "to": 10, "isSelf": 1, "skill": "37|36|0" }
        """;

        var uListRaw = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["idxList"] = new List<object?> { 16L, 22L },
                ["from"] = 0L, ["to"] = 10L, ["isSelf"] = 1L, ["skill"] = "37|36|0",
            },
        };
        var relayed = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.RelayUList(uListRaw);
        Assert.That(relayed, Is.Not.Null);

        var body = new SVSim.BattleNode.Protocol.Bodies.PlayActionsBroadcastBody(
            PlayIdx: 37, Type: 30, KnownList: null, OppoTargetList: null, UList: relayed);
        var env = new MsgEnvelope(NetworkBattleUri.PlayActions, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: body);

        using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
        var ourEntry = ourDoc.RootElement.GetProperty("uList")[0];
        using var prodDoc = JsonDocument.Parse(prodEntry);

        foreach (var key in new[] { "idxList", "from", "to", "isSelf", "skill" })
        {
            Assert.That(ourEntry.TryGetProperty(key, out var ours), Is.True, $"uList entry missing '{key}'");
            var prodVal = prodDoc.RootElement.GetProperty(key);
            Assert.That(ours.ValueKind, Is.EqualTo(prodVal.ValueKind), $"'{key}' type category mismatch");
        }
        Assert.That(ourEntry.GetProperty("idxList").GetArrayLength(), Is.EqualTo(2));
        Assert.That(ourEntry.GetProperty("skill").GetString(), Is.EqualTo("37|36|0"));
        // No cardId on a hidden fetch — confirm we didn't invent one.
        Assert.That(ourEntry.TryGetProperty("cardId", out _), Is.False, "hidden fetch carries no cardId");
    }

    [Test]
    public void MineCopyTokens_extracts_the_prod_capture_copy_op_shape()
    {
        // Prod copy op (battle-traffic_tk2_regular.ndjson:196) — the ONLY copy op in any capture, an
        // isSelf:0 Echo: {add:{idx:[49], isSelf:0, card:{baseIdx:21, isPremium:0}}}. No later play-reveal
        // of idx 49 exists, so this locks the copy-op PARSE + map-resolution only (spec §5), not a reveal.
        var orderList = new List<object?>
        {
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 49L }, ["isSelf"] = 0L,
                  ["card"] = new Dictionary<string, object?> { ["baseIdx"] = 21L, ["isPremium"] = 0L } } },
        };
        // baseIdx 21 lives in the opponent's (isSelf:0 -> otherMap) index space. The capture never reveals
        // idx 21's cardId, so seed a sentinel; this verifies parse + resolution of the op shape, not an id.
        var otherMap = new Dictionary<int, long> { [21] = 123_456_789L };
        var mined = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder
            .MineCopyTokens(orderList, new Dictionary<int, long>(), otherMap)
            .ToList();

        Assert.That(mined, Is.EquivalentTo(new[] { new SVSim.BattleNode.Sessions.Dispatch.MinedToken(49, 123_456_789L, CardOwner.Opponent) }));
    }

    [Test]
    public void SynthesizedChoiceGeneration_matches_prod_recv_keyAction_and_knownList_shape()
    {
        // Prod recv PlayActions for the generating card play (battle-traffic_tk2_regular.ndjson:151):
        // keyAction is {type,cardId} only (selectCard stripped for the hidden open:0 choice); knownList
        // reveals the generating DECK card. The choiceAdd lands a hidden token at idx 46 (candidates).
        // Subset check covers playIdx/type/keyAction — the parts we own; knownList idx/cardId/to are
        // asserted explicitly below (cost/clan/tribe are ENGINE-sourced at the handler, covered by
        // HeadlessConductorTests, so this builder-only call leaves them at defaults — not checked here).
        const string prodFrame = """
        { "playIdx": 18, "type": 30,
          "keyAction": [ { "type": 1, "cardId": 810014030 } ] }
        """;

        var deckMap = new Dictionary<int, long> { [18] = 810014030L };
        var orderList = new List<object?>
        {
            new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 18L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 30L } },
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 46L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?>
                    { ["candidates"] = new List<object?> { 810041260L, 101041020L } },
                  ["isChoice"] = "1" } },
        };
        var keyActionIn = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = 1L, ["cardId"] = 810014030L,
                ["selectCard"] = new Dictionary<string, object?>
                    { ["cardId"] = new List<object?> { 810041260L }, ["open"] = 0L },
            }
        };

        var played = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.BuildPlayedCard(18, deckMap[18], orderList);
        var keyActionOut = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.StripKeyActionForOpponent(keyActionIn);
        var body = new SVSim.BattleNode.Protocol.Bodies.PlayActionsBroadcastBody(
            PlayIdx: 18, Type: 30, KnownList: new[] { played! }, OppoTargetList: null, KeyAction: keyActionOut);
        var env = new MsgEnvelope(NetworkBattleUri.PlayActions, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: body);

        using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
        using var prodDoc = JsonDocument.Parse(prodFrame);
        var failures = new List<string>();
        CompareSubset(prodDoc.RootElement, ourDoc.RootElement, "PlayActions", isRoot: true, failures);
        Assert.That(failures, Is.Empty, string.Join("\n", failures));

        // The hidden pick must NOT leak: keyAction[0] carries no selectCard.
        var ourKa = ourDoc.RootElement.GetProperty("keyAction")[0];
        Assert.That(ourKa.TryGetProperty("selectCard", out _), Is.False, "selectCard must be stripped for open:0");
        Assert.That(ourKa.GetProperty("type").GetInt32(), Is.EqualTo(1));
        Assert.That(ourKa.GetProperty("cardId").GetInt64(), Is.EqualTo(810014030L));

        // The generating deck card reveals on its own play (idx 18 -> 810014030, to 30). cost/clan/tribe
        // are ENGINE-sourced at the handler (covered by HeadlessConductorTests); this builder-only call
        // leaves them at defaults, so only idx/cardId/to are checked — as in the sibling SynthesizedKnownList_* tests.
        var ourKnown = ourDoc.RootElement.GetProperty("knownList")[0];
        Assert.That(ourKnown.GetProperty("idx").GetInt32(), Is.EqualTo(18));
        Assert.That(ourKnown.GetProperty("cardId").GetInt64(), Is.EqualTo(810014030L));
        Assert.That(ourKnown.GetProperty("to").GetInt32(), Is.EqualTo(30));
    }

    [Test]
    public void SynthesizedChoiceReveal_matches_prod_recv_knownList_shape()
    {
        // Prod recv PlayActions for the chosen token play (battle-traffic_tk2_regular.ndjson:193):
        // knownList:[{idx:46, cardId:810041260,...}] — the pick recorded at generation, revealed on play.
        const string prodEntry = """
        { "idx": 46, "cardId": 810041260, "to": 20, "cost": 5, "clan": 0, "tribe": "0", "spellboost": 0, "attachTarget": "" }
        """;

        // Mine the pick from the generating frame (choiceAdd ∩ selectCard), then build the played entry.
        var generatingOrderList = new List<object?>
        {
            new Dictionary<string, object?> { ["add"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 46L }, ["isSelf"] = 1L,
                  ["card"] = new Dictionary<string, object?>
                    { ["candidates"] = new List<object?> { 810041260L, 101041020L } },
                  ["isChoice"] = "1" } },
        };
        var keyAction = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = 1L, ["cardId"] = 810014030L,
                ["selectCard"] = new Dictionary<string, object?>
                    { ["cardId"] = new List<object?> { 810041260L }, ["open"] = 0L },
            }
        };
        var map = new Dictionary<int, long>();
        foreach (var (idx, cardId, _) in SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.MineChoicePicks(generatingOrderList, keyAction))
            map[idx] = cardId;

        var playOrderList = new List<object?>
        {
            new Dictionary<string, object?> { ["move"] = new Dictionary<string, object?>
                { ["idx"] = new List<object?> { 46L }, ["isSelf"] = 1L, ["from"] = 10L, ["to"] = 20L } },
        };
        var entry = SVSim.BattleNode.Sessions.Dispatch.KnownListBuilder.BuildPlayedCard(46, map[46], playOrderList);
        Assert.That(entry, Is.Not.Null, "the mined choice pick resolves to a knownList entry");

        var body = new SVSim.BattleNode.Protocol.Bodies.PlayActionsBroadcastBody(
            PlayIdx: 46, Type: 30, KnownList: new[] { entry! }, OppoTargetList: null);
        var env = new MsgEnvelope(NetworkBattleUri.PlayActions, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null, Body: body);

        using var ourDoc = JsonDocument.Parse(MsgEnvelope.ToJson(env));
        var ourEntry = ourDoc.RootElement.GetProperty("knownList")[0];
        using var prodDoc = JsonDocument.Parse(prodEntry);

        foreach (var key in new[] { "idx", "cardId", "to" })
        {
            Assert.That(ourEntry.TryGetProperty(key, out var ours), Is.True, $"knownList entry missing '{key}'");
            var prodVal = prodDoc.RootElement.GetProperty(key);
            Assert.That(ours.ValueKind, Is.EqualTo(prodVal.ValueKind), $"'{key}' type category mismatch");
        }
        Assert.That(ourEntry.GetProperty("cardId").GetInt64(), Is.EqualTo(810041260L));
        Assert.That(ourEntry.GetProperty("to").GetInt32(), Is.EqualTo(20));
    }
}

/// <summary>
/// Representative server→client (<c>receive</c>) frames lifted verbatim from the prod TK2 captures.
/// One frame per server-authored URI, picked as the richest observed instance. The
/// <c>selfDeck</c> in <c>Matched</c> is trimmed to three cards (the array is uniform — three
/// entries are enough to lock the element shape). Numbers and string/number typing are preserved
/// exactly as captured, including the deliberate prod quirk that <c>BattleStart.selfInfo.battlePoint</c>
/// is a string while <c>oppoInfo.battlePoint</c> is a number.
///
/// Provenance (line numbers in the capture files):
///   InitNetwork  regular:1   | Matched     regular:2  | BattleStart regular:3
///   Deal         regular:4   | Swap        regular:7  | Ready       regular:9
///   TurnStart    regular:14  | TurnEnd     regular:18 | Judge       regular:20
///   BattleFinish regular:274 (result=102, a real loss capture)
/// </summary>
internal static class ProdCaptureFixture
{
    public const string Json = """
    {
      "InitNetwork": { "uri": "InitNetwork", "resultCode": 1 },
      "Matched": {
        "uri": "Matched",
        "selfInfo": {
          "country_code": "KOR", "userName": "combusty7", "sleeveId": "3000011",
          "emblemId": "701441011", "degreeId": "300003", "fieldId": 43,
          "isOfficial": 0, "oppoId": 847666884, "seed": 17548138
        },
        "oppoInfo": {
          "country_code": "JPN", "userName": "AtagoSuki", "sleeveId": "704141010",
          "emblemId": "400001100", "degreeId": "120027", "fieldId": 5,
          "isOfficial": 0, "oppoId": 906243102, "seed": 17548138, "oppoDeckCount": 30
        },
        "selfDeck": [
          { "idx": 1, "cardId": 128111020 },
          { "idx": 2, "cardId": 128121010 },
          { "idx": 3, "cardId": 127134010 }
        ],
        "bid": "975695075012", "playSeq": 1, "resultCode": 1
      },
      "BattleStart": {
        "uri": "BattleStart",
        "turnState": 0,
        "selfInfo": {
          "rank": "10", "battlePoint": "6270", "classId": "1", "charaId": "1",
          "cardMasterName": "card_master_node_10015"
        },
        "oppoInfo": {
          "rank": "25", "isMasterRank": "1", "battlePoint": 50000, "masterPoint": "2144",
          "classId": "8", "charaId": "4608", "cardMasterName": "card_master_node_10015"
        },
        "battleType": 11, "resultCode": 1, "playSeq": 2
      },
      "Deal": {
        "uri": "Deal",
        "self": [ { "pos": 0, "idx": 2 }, { "pos": 1, "idx": 16 }, { "pos": 2, "idx": 25 } ],
        "oppo": [ { "pos": 0, "idx": 28 }, { "pos": 1, "idx": 20 }, { "pos": 2, "idx": 18 } ],
        "playSeq": 3, "resultCode": 1
      },
      "Swap": {
        "uri": "Swap",
        "self": [ { "pos": 0, "idx": 2 }, { "pos": 1, "idx": 16 }, { "pos": 2, "idx": 25 } ],
        "playSeq": 4, "resultCode": 1
      },
      "Ready": {
        "uri": "Ready",
        "self": [ { "pos": 0, "idx": 2 }, { "pos": 1, "idx": 16 }, { "pos": 2, "idx": 25 } ],
        "oppo": [ { "pos": 0, "idx": 28 }, { "pos": 1, "idx": 24 }, { "pos": 2, "idx": 18 } ],
        "idxChangeSeed": 771335280, "spin": 243, "playSeq": 5, "resultCode": 1
      },
      "TurnStart": { "uri": "TurnStart", "spin": 189, "resultCode": 1, "playSeq": 6 },
      "TurnEnd": { "uri": "TurnEnd", "turnState": 0, "resultCode": 1, "playSeq": 8 },
      "Judge": { "uri": "Judge", "spin": 55, "playSeq": 9, "resultCode": 1 },
      "BattleFinish": { "uri": "BattleFinish", "result": 102, "playSeq": 99, "resultCode": 1 }
    }
    """;
}
