using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

internal sealed class InitNetworkHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (ctx.SenderPhase != HandshakePhase.AwaitingInitNetwork)
            return Array.Empty<DispatchRoute>();

        var routes = new List<DispatchRoute>
        {
            new(ctx.From, BattleFrames.BuildAck(NetworkBattleUri.InitNetwork), Stock.Bypass),
        };
        ctx.SenderPhase = HandshakePhase.AwaitingInitBattle;
        return routes;
    }
}
