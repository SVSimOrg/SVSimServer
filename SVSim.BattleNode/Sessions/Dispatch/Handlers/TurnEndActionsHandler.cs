using SVSim.BattleNode.Protocol;

namespace SVSim.BattleNode.Sessions.Dispatch.Handlers;

/// <summary>PvP TurnEndActions: the sender's orderList is dropped; the opponent receives an
/// empty body (it only flips _sendEcho + runs the opponent's end-of-turn triggers via the
/// opponent's own engine). Bot drop.</summary>
internal sealed class TurnEndActionsHandler : IFrameHandler
{
    public IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx)
    {
        if (ctx.BothSidesAfterReady())
        {
            var frame = ctx.Env with { Body = new RawBody(new Dictionary<string, object?>()) };
            return new[] { new DispatchRoute(ctx.Other, frame, Stock.Normal) };
        }
        return Array.Empty<DispatchRoute>();
    }
}
