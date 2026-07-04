using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Participants; // IHasHandshakePhase

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class SwapHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (ctx.SenderPhase != HandshakePhase.AwaitingSwap)
            return Array.Empty<DispatchRoute>();

        var routes = new List<DispatchRoute>();
        var hand = ServerBattleFrames.ComputeHandAfterSwap(BattleFrames.ExtractIdxList(ctx.Env));

        // SwapResponse is always immediate — completes the sender's own mulligan UI.
        routes.Add(new DispatchRoute(ctx.From, ServerBattleFrames.BuildSwapResponse(hand), Stock.Normal));
        ctx.State.PostSwapHands[ctx.From] = hand;
        ctx.SenderPhase = HandshakePhase.AfterReady;

        // Release Ready to every swapper once all handshake-driving participants have swapped.
        // IHasHandshakePhase membership IS the "participates in mulligan" set.
        var swappers = new[] { ctx.A, ctx.B }.Where(p => p is IHasHandshakePhase).ToList();
        if (swappers.All(ctx.State.PostSwapHands.ContainsKey))
        {
            foreach (var p in swappers)
            {
                var opponent = ReferenceEquals(p, ctx.A) ? ctx.B : ctx.A;
                var idxSeed = BattleSeeds.IdxChange(ctx.State.MasterSeed, p.ViewerId);
                var ready = opponent is IHasHandshakePhase
                            && ctx.State.PostSwapHands.TryGetValue(opponent, out var oppoHand)
                    ? ServerBattleFrames.BuildReady(ctx.State.PostSwapHands[p], oppoHand, idxSeed)
                    : ServerBattleFrames.BuildReady(ctx.State.PostSwapHands[p], idxSeed);
                routes.Add(new DispatchRoute(p, ready, Stock.Normal));
            }
        }
        return routes;
    }
}
