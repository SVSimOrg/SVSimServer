using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class TurnStartHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // PvP: the active player's TurnStart{orderList} is dropped; the opponent receives {spin}
        // (spin=0 for the deterministic-turn slice) and self-generates its turn-open.
        if (ctx.BothSidesAfterReady())
        {
            var frame = ctx.Env with { Body = new OpponentTurnStartBody(Spin: BattleFrameDefaults.DeterministicTurnSpin) };
            return new[] { new DispatchRoute(ctx.Other, frame, Stock.Normal) };
        }

        return Array.Empty<DispatchRoute>();
    }
}
