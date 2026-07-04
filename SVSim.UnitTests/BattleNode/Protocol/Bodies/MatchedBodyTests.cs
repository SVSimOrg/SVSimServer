using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.UnitTests.BattleNode.Protocol.Bodies;

[TestFixture]
public class MatchedBodyTests
{
    [Test]
    public void Serializes_AllSelfInfoFields_WithCorrectWireKeys()
    {
        var body = new MatchedBody(
            SelfInfo: new MatchedSelfInfo(
                CountryCode: "KOR", UserName: "Player", SleeveId: "3000011",
                EmblemId: "701441011", DegreeId: "300003", FieldId: 43,
                IsOfficial: false, OppoId: 847666884, Seed: 17_548_138),
            OppoInfo: new MatchedOppoInfo(
                CountryCode: "JPN", UserName: "Opponent", SleeveId: "704141010",
                EmblemId: "400001100", DegreeId: "120027", FieldId: 5,
                IsOfficial: false, OppoId: 906243102, Seed: 17_548_138, OppoDeckCount: 30),
            SelfDeck: new[] { new DeckCardRef(Idx: 1, CardId: 100011010L) });

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var selfInfo = (JsonObject)node["selfInfo"]!;

        Assert.That(selfInfo["country_code"]!.GetValue<string>(), Is.EqualTo("KOR"));
        Assert.That(selfInfo["userName"]!.GetValue<string>(), Is.EqualTo("Player"));
        Assert.That(selfInfo["sleeveId"]!.GetValue<string>(), Is.EqualTo("3000011"));
        Assert.That(selfInfo["emblemId"]!.GetValue<string>(), Is.EqualTo("701441011"));
        Assert.That(selfInfo["degreeId"]!.GetValue<string>(), Is.EqualTo("300003"));
        Assert.That(selfInfo["fieldId"]!.GetValue<int>(), Is.EqualTo(43));
        Assert.That(selfInfo["isOfficial"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(selfInfo["oppoId"]!.GetValue<int>(), Is.EqualTo(847666884));
        Assert.That(selfInfo["seed"]!.GetValue<int>(), Is.EqualTo(17_548_138));
    }

    [Test]
    public void OppoInfo_HasOppoDeckCount_OnTheWire()
    {
        var body = new MatchedBody(
            SelfInfo: new MatchedSelfInfo("KOR","P","s","e","d",0,false,1,1),
            OppoInfo: new MatchedOppoInfo("JPN","O","s","e","d",0,false,1,1, OppoDeckCount: 30),
            SelfDeck: System.Array.Empty<DeckCardRef>());

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var oppoInfo = (JsonObject)node["oppoInfo"]!;

        Assert.That(oppoInfo["oppoDeckCount"]!.GetValue<int>(), Is.EqualTo(30));
    }

    [Test]
    public void SelfInfo_DoesNotHaveOppoDeckCount_OnTheWire()
    {
        var body = new MatchedBody(
            SelfInfo: new MatchedSelfInfo("KOR","P","s","e","d",0,false,1,1),
            OppoInfo: new MatchedOppoInfo("JPN","O","s","e","d",0,false,1,1,30),
            SelfDeck: System.Array.Empty<DeckCardRef>());

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var selfInfo = (JsonObject)node["selfInfo"]!;

        Assert.That(selfInfo.ContainsKey("oppoDeckCount"), Is.False);
    }

    [Test]
    public void ResultCode_DefaultsToOne_OnConstruction()
    {
        var body = new MatchedBody(
            SelfInfo: new MatchedSelfInfo("KOR","P","s","e","d",0,false,1,1),
            OppoInfo: new MatchedOppoInfo("JPN","O","s","e","d",0,false,1,1,30),
            SelfDeck: System.Array.Empty<DeckCardRef>());

        Assert.That(body.ResultCode, Is.EqualTo(1));
        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));
    }

    [Test]
    public void SelfDeck_SerializesAsArray_WithIdxAndCardIdKeys()
    {
        var body = new MatchedBody(
            SelfInfo: new MatchedSelfInfo("KOR","P","s","e","d",0,false,1,1),
            OppoInfo: new MatchedOppoInfo("JPN","O","s","e","d",0,false,1,1,30),
            SelfDeck: new[]
            {
                new DeckCardRef(Idx: 1, CardId: 100011010L),
                new DeckCardRef(Idx: 2, CardId: 100011010L),
            });

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var deck = (JsonArray)node["selfDeck"]!;

        Assert.That(deck.Count, Is.EqualTo(2));
        Assert.That(((JsonObject)deck[0]!)["idx"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(((JsonObject)deck[0]!)["cardId"]!.GetValue<long>(), Is.EqualTo(100011010L));
    }
}
