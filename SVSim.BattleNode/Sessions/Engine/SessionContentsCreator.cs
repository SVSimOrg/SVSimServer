extern alias engine;
using engine::Wizard.BattleMgr;
using engine::Wizard.Battle.Phase;
using engine::Wizard.Battle.Recovery;
using engine::Wizard.Battle.Replay;
using engine::Wizard.Battle.Resource;
using engine::Wizard.Battle.View.Vfx;

namespace SVSim.BattleNode.Sessions.Engine;

/// <summary>The node's production IBattleMgrContentsCreator. Mirrors the test-side
/// HeadlessContentsCreator (HeadlessFixture.cs) but carries the per-battle master seed so the
/// engine's RNG stream is born aligned with the seed the node handed both clients (design F-N-5).
/// The non-RandomSeed members are the no-op recovery/replay/resource/vfx/phase creators the
/// NetworkStandardBattleMgr ctor dereferences — the engine's own Null* implementations, same set the
/// headless test harness uses.</summary>
internal sealed class SessionContentsCreator : IBattleMgrContentsCreator
{
    public SessionContentsCreator(int masterSeed) => RandomSeed = masterSeed;

    public int RandomSeed { get; }

    // No-op managers: the ctor's FirstRecoverySetting/FirstReplaySetting dereference these; recovery/
    // replay recording is irrelevant to a shadow engine, so use the engine's own null implementations.
    public IRecoveryManager RecoveryManager { get; } = new NullRecoveryManager();
    public IRecoveryRecordManager RecoveryRecordManager { get; } = new NullRecoveryRecordManager();
    public IReplayRecordManager ReplayRecordManager { get; } = new NullReplayRecordManager();

    public IBattleResourceMgr CreateResourceMgr() => new BattleResourceMgr();
    // The receive-conductor VfxMgr: runs the InstantVfx the conductor fuses the play mutation into
    // (design Headless-Conductor Candidate B). The shared VfxMgr no-ops registration — correct for the
    // direct ActionProcessor path, wrong for the receive path. See HeadlessConductorVfxMgr.
    public VfxMgr CreateVfxMgr() => new HeadlessConductorVfxMgr();
    public IPhaseCreator CreatePhaseCreator(engine::BattleManagerBase battleMgr) =>
        new SessionPhaseCreator(battleMgr);
}

/// <summary>Node analogue of the test HeadlessPhaseCreator / the engine's SingleBattlePhaseCreator
/// (cut from the M1 copy set as an entry-point ctor): inherits PhaseCreatorBase wholesale.</summary>
internal sealed class SessionPhaseCreator : PhaseCreatorBase
{
    public SessionPhaseCreator(engine::BattleManagerBase battleMgr) : base(battleMgr) { }
}
