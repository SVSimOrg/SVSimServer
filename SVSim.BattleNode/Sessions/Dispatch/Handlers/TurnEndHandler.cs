using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class TurnEndHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // case 4: Bot — Judge to sender only (no real opponent; client flips back to its local AI).
        if (ctx.OpponentIsAckOnly && ctx.SenderIsAfterReady)
            return new[] { new DispatchRoute(ctx.From, BattleFrames.BuildJudgeBroadcast(), Stock.Normal) };

        // case 8: general AfterReady arm — PvP forwards a {turnState} TurnEnd to the opponent
        // (handover gate). Any non-Pvp non-Bot type that reaches AfterReady consumes the frame.
        if (ctx.SenderIsAfterReady)
        {
            if (ctx.BothSidesAfterReady())
            {
                // Opponent sees {turnState}; receiving TurnEnd drives ITS SendJudge (handover gate):
                // the opponent (the turn taker-over) then sends a Judge, which JudgeHandler reflects
                // back to it to start its turn. battleCode/actionSeq/cemetery are dropped.
                var te = ctx.Env with { Body = new TurnEndBody(TurnState: TurnState.First) };
                return new[] { new DispatchRoute(ctx.Other, te, Stock.Normal) };
            }
            return Array.Empty<DispatchRoute>(); // Pvp-not-both-ready → drop (Bot already returned above)
        }

        return Array.Empty<DispatchRoute>();
    }
}
