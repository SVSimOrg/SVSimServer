using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class ForwardWhenBothReadyHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (ctx.BothSidesAfterReady())
            return new[] { new DispatchRoute(ctx.Other, ctx.Env, Stock.Normal) };
        return Array.Empty<DispatchRoute>();
    }
}
