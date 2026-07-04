using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class JudgeHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // PvP: Judge is the handover gate. The player who sends Judge is the one TAKING OVER the
        // turn (the client rule is: receive opponent TurnEnd -> SendJudge). Receiving Judge{spin}
        // fires ControlTurnStartPlayer ("start MY turn"), so the {spin} must REFLECT BACK to the
        // sender — NOT go to the opponent (that would make the player who just ended their turn
        // start another one, stalling the loop; confirmed by the 2026-06-03 two-client capture).
        // The sender then emits TurnStart, which TurnStartHandler relays to the opponent as {spin}.
        // battleCode is dropped; spin=0 for the deterministic-turn slice.
        if (ctx.BothSidesAfterReady())
        {
            var frame = ctx.Env with { Body = new JudgeBody(Spin: BattleFrameDefaults.DeterministicTurnSpin) };
            return new[] { new DispatchRoute(ctx.From, frame, Stock.Normal) };
        }

        return Array.Empty<DispatchRoute>();
    }
}
