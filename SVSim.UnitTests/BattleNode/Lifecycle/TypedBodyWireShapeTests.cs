using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;

namespace SVSim.UnitTests.BattleNode.Lifecycle;

/// <summary>
/// Wire-shape regression tests: compare <see cref="MsgEnvelope.ToJson"/> output against
/// JSON literals derived from captured prod frames in
/// data_dumps/captures/battle-traffic_tk2_regular.ndjson. Per the feedback_wire_shape_tests
/// memory, these are literal-comparison tests — NOT self-symmetric round-trips — so they
/// catch the failure mode where a C# property is renamed and silently breaks the wire
/// contract.
/// </summary>
[TestFixture]
public class TypedBodyWireShapeTests
{
    [Test]
    public void BuildMatched_KeyOrder_PutsUriBeforeSelfDeckAndSelfInfo()
    {
        // Regression: the client's RealTimeNetworkAgent.SetNetworkInfo iterates the
        // synchronize-data dict in insertion order. When it hits "uri" and recognizes
        // "Matched", it calls GameMgr.InitializeSelfInfo() which sets _selfDeck = null.
        // Any "selfDeck" / "selfInfo" key processed BEFORE "uri" is wiped before
        // Matching.StartBattleLoad reads it back, and GetSelfDeck().Select(...) crashes
        // with "Value cannot be null. Parameter name: source". The prod wire format
        // emits envelope keys (uri first) before body keys; we must too.
        var env = ServerBattleFrames.BuildMatched(FixtureCtx(), FakeOpponentCtx(),
            selfViewerId: 1, oppoViewerId: 2, battleId: "b", seed: 17_548_138,
            selfDeckOrder: FixtureCtx().SelfDeckCardIds);
        var json = MsgEnvelope.ToJson(env);

        var uriIdx = json.IndexOf("\"uri\":", StringComparison.Ordinal);
        var selfDeckIdx = json.IndexOf("\"selfDeck\":", StringComparison.Ordinal);
        var selfInfoIdx = json.IndexOf("\"selfInfo\":", StringComparison.Ordinal);

        Assert.That(uriIdx, Is.GreaterThan(-1), "uri must be present");
        Assert.That(selfDeckIdx, Is.GreaterThan(-1), "selfDeck must be present");
        Assert.That(selfInfoIdx, Is.GreaterThan(-1), "selfInfo must be present");
        Assert.That(uriIdx, Is.LessThan(selfDeckIdx), "uri must appear BEFORE selfDeck");
        Assert.That(uriIdx, Is.LessThan(selfInfoIdx), "uri must appear BEFORE selfInfo");
    }

    [Test]
    public void BuildMatched_SerializesAllWireKeysExpectedByTheClient()
    {
        var env = ServerBattleFrames.BuildMatched(FixtureCtx(), FakeOpponentCtx(),
            selfViewerId: 906243102, oppoViewerId: 847666884, battleId: "597830888107",
            seed: 17_548_138, selfDeckOrder: FixtureCtx().SelfDeckCardIds);

        var json = MsgEnvelope.ToJson(env);
        var node = JsonNode.Parse(json)!.AsObject();

        // Top-level envelope fields:
        Assert.That(node["uri"]!.GetValue<string>(), Is.EqualTo("Matched"));
        Assert.That(node["viewerId"]!.GetValue<long>(), Is.EqualTo(999_999_999L));
        Assert.That(node["uuid"]!.GetValue<string>(), Is.EqualTo("node-stub"));
        Assert.That(node["bid"]!.GetValue<string>(), Is.EqualTo("597830888107"));
        Assert.That(node["try"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(node["cat"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));

        // Inner selfInfo block has the wire keys the client's Parse looks up.
        var selfInfo = node["selfInfo"]!.AsObject();
        foreach (var key in new[] {
            "country_code", "userName", "sleeveId", "emblemId", "degreeId",
            "fieldId", "isOfficial", "oppoId", "seed",
        })
        {
            Assert.That(selfInfo.ContainsKey(key), Is.True, $"selfInfo missing wire key '{key}'");
        }
        Assert.That(selfInfo["oppoId"]!.GetValue<long>(), Is.EqualTo(847666884L));
        Assert.That(selfInfo.ContainsKey("oppoDeckCount"), Is.False, "selfInfo must NOT have oppoDeckCount");

        var oppoInfo = node["oppoInfo"]!.AsObject();
        Assert.That(oppoInfo["oppoDeckCount"]!.GetValue<int>(), Is.EqualTo(30));
        Assert.That(oppoInfo["oppoId"]!.GetValue<long>(), Is.EqualTo(906243102L));

        var selfDeck = node["selfDeck"]!.AsArray();
        Assert.That(selfDeck.Count, Is.EqualTo(30));
        Assert.That(selfDeck[0]!.AsObject()["idx"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(selfDeck[0]!.AsObject()["cardId"]!.GetValue<long>(), Is.EqualTo(100_011_010L));
    }

    [Test]
    public void BuildBattleStart_SerializesAllWireKeysAndPreservesBattlePointAsymmetry()
    {
        var env = ServerBattleFrames.BuildBattleStart(FixtureCtx(), FakeOpponentCtx(), selfViewerId: 906243102, turnState: TurnState.First);

        var json = MsgEnvelope.ToJson(env);
        var node = JsonNode.Parse(json)!.AsObject();

        Assert.That(node["turnState"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(node["battleType"]!.GetValue<int>(), Is.EqualTo(11));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));

        var selfInfo = node["selfInfo"]!.AsObject();
        Assert.That(selfInfo["rank"]!.GetValue<string>(), Is.EqualTo("10"));
        Assert.That(selfInfo["battlePoint"]!.GetValue<string>(), Is.EqualTo("6270"));   // string on self
        Assert.That(selfInfo["cardMasterName"]!.GetValue<string>(), Is.EqualTo("card_master_node_10015"));

        var oppoInfo = node["oppoInfo"]!.AsObject();
        Assert.That(oppoInfo["battlePoint"]!.GetValue<int>(), Is.EqualTo(0));            // int on oppo
        Assert.That(oppoInfo["isMasterRank"]!.GetValue<string>(), Is.EqualTo("0"));
        Assert.That(oppoInfo["masterPoint"]!.GetValue<string>(), Is.EqualTo("0"));
    }

    [Test]
    public void BuildDeal_SerializesSelfAndOppoArraysWithPosIdxShape()
    {
        var env = ServerBattleFrames.BuildDeal();
        var json = MsgEnvelope.ToJson(env);
        var node = JsonNode.Parse(json)!.AsObject();

        var self = node["self"]!.AsArray();
        Assert.That(self.Count, Is.EqualTo(3));
        Assert.That(self[0]!.AsObject()["pos"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(self[0]!.AsObject()["idx"]!.GetValue<int>(), Is.EqualTo(1));

        var oppo = node["oppo"]!.AsArray();
        Assert.That(oppo.Count, Is.EqualTo(3));
    }

    [Test]
    public void BuildSwapResponse_SerializesSelfWithoutOppo()
    {
        var env = ServerBattleFrames.BuildSwapResponse(new long[] { 1, 4, 3 });
        var json = MsgEnvelope.ToJson(env);
        var node = JsonNode.Parse(json)!.AsObject();

        Assert.That(node.ContainsKey("self"), Is.True);
        Assert.That(node.ContainsKey("oppo"), Is.False);
        Assert.That(node["self"]!.AsArray()[1]!.AsObject()["idx"]!.GetValue<int>(), Is.EqualTo(4));
    }

    [Test]
    public void BuildReady_SerializesAllFieldsIncludingSeedAndSpin()
    {
        var env = ServerBattleFrames.BuildReady(new long[] { 1, 4, 3 }, idxChangeSeed: 771_335_280);
        var json = MsgEnvelope.ToJson(env);
        var node = JsonNode.Parse(json)!.AsObject();

        Assert.That(node["idxChangeSeed"]!.GetValue<int>(), Is.EqualTo(771_335_280));
        Assert.That(node["spin"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(node["self"]!.AsArray().Count, Is.EqualTo(3));
        Assert.That(node["oppo"]!.AsArray().Count, Is.EqualTo(3));
    }

    /// <summary>
    /// Wire-shape fixture: 30 copies of the legacy DummyCardId (100_011_010L) so the
    /// existing literal assertions on <c>selfDeck[0].cardId</c> (line 81 above) keep working
    /// after the MatchContext migration deletes the const.
    /// </summary>
    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    // Prod-captured opponent fixture — 30-card deck and the prod-captured opponent
    // cosmetics (ClassId/CharaId "8") so the wire bytes asserted below (oppoInfo classId/charaId,
    // oppoDeckCount=30, etc.) remain byte-identical after the BuildMatched/BuildBattleStart
    // signature change.
    private static MatchContext FakeOpponentCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 0L).ToList(),
        ClassId: CardClass.Portalcraft, CharaId: "8", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Japan, UserName: "Opponent", SleeveId: "704141010",
        EmblemId: "400001100", DegreeId: "120027", FieldId: 5, IsOfficial: 0,
        BattleModeId: 0);
}
