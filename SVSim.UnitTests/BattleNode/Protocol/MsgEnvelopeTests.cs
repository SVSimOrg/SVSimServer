using NUnit.Framework;
using SVSim.BattleNode.Protocol;

namespace SVSim.UnitTests.BattleNode.Protocol;

[TestFixture]
public class MsgEnvelopeTests
{
    [Test]
    public void FromJson_NumericArray_PreservesLongTypeOnEachElement()
    {
        // Regression for the v1 mulligan bug: idxList:[2] from the wire was decoded as a list
        // containing a boxed double (2.0) instead of a boxed long (2L). The conditional expression
        // `el.TryGetInt64(out var l) ? l : el.GetDouble()` unified its branches to the common
        // implicit type (double) and silently widened the long. Downstream OfType<long> filtered
        // every entry out, so swapIndices arrived empty and the server echoed the unchanged hand.
        const string json = "{\"uri\":\"Swap\",\"viewerId\":1,\"uuid\":\"u\",\"try\":0,\"cat\":1,\"idxList\":[2,3]}";

        var env = MsgEnvelope.FromJson(json);

        var raw = (RawBody)env.Body;
        var idxList = (List<object?>)raw.Entries["idxList"]!;
        Assert.That(idxList.Count, Is.EqualTo(2));
        Assert.That(idxList[0], Is.TypeOf<long>(), "idxList[0] must be boxed long, not double.");
        Assert.That(idxList[0], Is.EqualTo(2L));
        Assert.That(idxList[1], Is.TypeOf<long>());
        Assert.That(idxList[1], Is.EqualTo(3L));
    }

    [Test]
    public void Roundtrip_PreservesEnvelopeAndBody()
    {
        var env = new MsgEnvelope(
            Uri: NetworkBattleUri.InitNetwork,
            ViewerId: 906243102,
            Uuid: "udid-1234",
            Bid: "597830888107",
            RetryAttempt: 0,
            Cat: EmitCategory.General,
            PubSeq: null,
            PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?> { ["foo"] = 42 }));

        var json = MsgEnvelope.ToJson(env);
        var back = MsgEnvelope.FromJson(json);

        Assert.That(back.Uri, Is.EqualTo(NetworkBattleUri.InitNetwork));
        Assert.That(back.ViewerId, Is.EqualTo(906243102));
        Assert.That(back.Uuid, Is.EqualTo("udid-1234"));
        Assert.That(back.Bid, Is.EqualTo("597830888107"));
        Assert.That(back.Cat, Is.EqualTo(EmitCategory.General));
        Assert.That(((RawBody)back.Body).Entries["foo"], Is.EqualTo(42L));
    }

    [Test]
    public void ToJson_OmitsNullEnvelopeFields()
    {
        var env = new MsgEnvelope(
            Uri: NetworkBattleUri.Ready,
            ViewerId: 1,
            Uuid: "u",
            Bid: null,
            RetryAttempt: 0,
            Cat: EmitCategory.Battle,
            PubSeq: null,
            PlaySeq: 5,
            Body: new RawBody(new Dictionary<string, object?>()));

        var json = MsgEnvelope.ToJson(env);

        Assert.That(json, Does.Not.Contain("\"bid\""));
        Assert.That(json, Does.Not.Contain("\"pubSeq\""));
        Assert.That(json, Does.Contain("\"playSeq\":5"));
    }

    [Test]
    public void FromJson_DispatchesUriToEnum()
    {
        const string json = "{\"uri\":\"PlayActions\",\"viewerId\":1,\"uuid\":\"u\",\"try\":0,\"cat\":1}";

        var env = MsgEnvelope.FromJson(json);

        Assert.That(env.Uri, Is.EqualTo(NetworkBattleUri.PlayActions));
        Assert.That(env.Cat, Is.EqualTo(EmitCategory.Battle));
    }

    [Test]
    public void ToJson_RawBodyContainingReservedKey_Throws()
    {
        var env = new MsgEnvelope(
            Uri: NetworkBattleUri.Loaded,
            ViewerId: 1,
            Uuid: "u",
            Bid: null,
            RetryAttempt: 0,
            Cat: EmitCategory.Battle,
            PubSeq: null,
            PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?> { ["uri"] = "Injected" }));

        var ex = Assert.Throws<ArgumentException>(() => MsgEnvelope.ToJson(env));
        Assert.That(ex!.Message, Does.Contain("uri"));
    }

    [Test]
    public void ToJson_UriField_SerializesAsExactPascalCaseMemberName()
    {
        var env = new MsgEnvelope(
            Uri: NetworkBattleUri.PlayActions,
            ViewerId: 1,
            Uuid: "u",
            Bid: null,
            RetryAttempt: 0,
            Cat: EmitCategory.Battle,
            PubSeq: null,
            PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?>()));

        var json = MsgEnvelope.ToJson(env);

        // Wire form must be PascalCase exactly — not "playActions", not "play_actions".
        Assert.That(json, Does.Contain("\"uri\":\"PlayActions\""));
    }
}
