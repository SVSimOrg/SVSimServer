using NUnit.Framework;
using SVSim.BattleNode.Sessions.Engine;
using System.Linq;
using SVSim.BattleEngine.Tests;

namespace SVSim.BattleEngine.Tests.SessionEngine;

[TestFixture]
public class SessionEngineSpellboostTests
{

    [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

    [Test]
    public void EngineGlobalInit_makes_a_fresh_engine_ready()
    {
        EngineGlobalInit.EnsureInitialized();
        var cl1 = CaptureReplay.Load("battle_test_cl1.ndjson");
        var cl2 = CaptureReplay.Load("battle_test_cl2.ndjson");
        var deckA = CaptureReplay.SelfDeckFrom(cl1);
        var deckB = CaptureReplay.SelfDeckFrom(cl2);
        // Belt-and-suspenders (matches the sibling tests): load the decks into the harness master so
        // this test never depends on global card-master contents. EnsureInitialized() above still
        // proves EngineGlobalInit's own path works.
        foreach (var id in deckA.Concat(deckB).Distinct()) HeadlessCardMaster.Load((int)id);
        var engine = new SessionBattleEngine();
        Assert.DoesNotThrow(() => engine.Setup(masterSeed: 12345, seatADeck: deckA, seatBDeck: deckB));
        Assert.That(engine.IsReady, Is.True, "engine must be ready after EngineGlobalInit (carried-risk fix)");
    }

}
