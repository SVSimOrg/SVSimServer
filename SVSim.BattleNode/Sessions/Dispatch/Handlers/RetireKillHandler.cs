using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class RetireKillHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        ctx.State.Lifecycle = SessionLifecycle.Terminal;
        // Polarity: the SENDER retired, so From LOSES / Other WINS. This is the OPPOSITE of
        // TurnEndFinalHandler (From WINS there — sender dealt the lethal). Intentional — do NOT
        // "consistency-fix" the two handlers to match; a swap here silently reverses every retire.
        return new[]
        {
            new DispatchRoute(ctx.From, BattleFrames.BuildBattleFinish(BattleResult.RetireLose), Stock.Bypass),
            new DispatchRoute(ctx.Other, BattleFrames.BuildBattleFinish(BattleResult.RetireWin), Stock.Bypass),
        };
    }
}
