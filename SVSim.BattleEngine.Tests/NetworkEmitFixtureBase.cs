namespace SVSim.BattleEngine.Tests
{
    // Shared base for every network-emit test fixture (M13 EmitPathReadOracleTests, the
    // construction-probe's OnEmit seam test, and any M14+ network fixture to come).
    //
    // Post-Phase-5a: still empty. The historical hygiene gap this class closed
    // (HeadlessEngineEnv.NewNetworkEmitBattle leaving IsForecast=false + a stray injected agent
    // visible to a later solo fixture) was a PROCESS-GLOBAL leak via the long-deleted
    // BattleManagerBase._isForecastFallback + ToolboxGame._realTimeNetworkAgentFallback statics.
    // As of Phase 5a IsForecast/RealTimeNetworkAgent live on the mgr instance (authoritative)
    // with the per-test BattleAmbientContext as bridge fallback: scope Dispose drops the ctx and
    // the next fixture's new TestBattleScope starts a fresh ctx with IsForecast=true and a null
    // NetworkAgent — exactly the EnsureInitialized invariant the old TearDown restored.
    //
    // Kept as a marker base class so derived fixtures don't churn; deleteable once every
    // fixture stops touching the ambient (Phase 5b + ambient rip).
    public abstract class NetworkEmitFixtureBase
    {
    }
}
