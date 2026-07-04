using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class LoadedHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // case 3: Bot — silent (client populates opponent state from AIBattleStart HTTP data).
        if (ctx.OpponentIsAckOnly && ctx.SenderPhase == HandshakePhase.AwaitingLoaded)
        {
            ctx.SenderPhase = HandshakePhase.AwaitingSwap;
            return Array.Empty<DispatchRoute>();
        }

        // case 6: general — BattleStart (per-perspective) + Deal to the sender.
        if (ctx.SenderPhase == HandshakePhase.AwaitingLoaded)
        {
            // A goes first deterministically; B goes second.
            var turnState = ReferenceEquals(ctx.From, ctx.A) ? TurnState.First : TurnState.Second;
            var r = new List<DispatchRoute>
            {
                new(ctx.From, ServerBattleFrames.BuildBattleStart(
                    ctx.From.Context, ctx.Other.Context, ctx.From.ViewerId, turnState), Stock.Normal),
                new(ctx.From, ServerBattleFrames.BuildDeal(), Stock.Normal),
            };
            ctx.SenderPhase = HandshakePhase.AwaitingSwap;
            return r;
        }

        return Array.Empty<DispatchRoute>();
    }
}
