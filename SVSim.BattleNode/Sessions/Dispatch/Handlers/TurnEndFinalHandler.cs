using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class TurnEndFinalHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // case 4: Bot — Judge to sender only.
        if (ctx.OpponentIsAckOnly && ctx.SenderIsAfterReady)
            return new[] { new DispatchRoute(ctx.From, BattleFrames.BuildJudgeBroadcast(), Stock.Normal) };

        // case 9: general — forward the envelope to other + paired BattleFinish + Terminal.
        if (ctx.SenderIsAfterReady)
        {
            ctx.State.Lifecycle = SessionLifecycle.Terminal;
            // Polarity: the SENDER dealt the lethal, so From WINS / Other LOSES. This is the
            // OPPOSITE of RetireKillHandler (From LOSES there — retire is self-inflicted).
            // Intentional — do NOT "consistency-fix" the two handlers to match; a swap here
            // silently reverses every lethal-turn outcome.
            return new[]
            {
                new DispatchRoute(ctx.Other, ctx.Env, Stock.Normal),
                new DispatchRoute(ctx.From, BattleFrames.BuildBattleFinish(BattleResult.LifeWin), Stock.Bypass),
                new DispatchRoute(ctx.Other, BattleFrames.BuildBattleFinish(BattleResult.LifeLose), Stock.Bypass),
            };
        }

        return Array.Empty<DispatchRoute>();
    }
}
