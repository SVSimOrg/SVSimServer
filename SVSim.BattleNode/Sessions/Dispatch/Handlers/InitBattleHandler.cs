using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class InitBattleHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        // case 2: Bot — ack only, NO Matched (Matched would corrupt client opponent info).
        if (ctx.OpponentIsAckOnly && ctx.SenderPhase == HandshakePhase.AwaitingInitBattle)
        {
            var r = new List<DispatchRoute>
            {
                new(ctx.From, BattleFrames.BuildAck(NetworkBattleUri.InitBattle), Stock.Bypass),
            };
            ctx.SenderPhase = HandshakePhase.AwaitingLoaded;
            return r;
        }

        // case 5: general — push Matched (per-perspective) to the sender only.
        if (ctx.SenderPhase == HandshakePhase.AwaitingInitBattle)
        {
            var r = new List<DispatchRoute>
            {
                new(ctx.From, ServerBattleFrames.BuildMatched(
                    ctx.From.Context, ctx.Other.Context, ctx.From.ViewerId, ctx.Other.ViewerId,
                    ctx.BattleId, BattleSeeds.Stable(ctx.State.MasterSeed),
                    ctx.State.GetShuffledDeck(ctx.From)), Stock.Normal),
            };
            ctx.SenderPhase = HandshakePhase.AwaitingLoaded;
            return r;
        }

        return Array.Empty<DispatchRoute>();
    }
}
