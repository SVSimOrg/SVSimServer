namespace SVSim.BattleNode.Sessions.Dispatch;

/// <summary>Handles one (or more) inbound URI(s). Pure: returns the routes to dispatch and may
/// mutate <see cref="FrameDispatchContext.State"/> / advance <see cref="FrameDispatchContext.SenderPhase"/>,
/// but does not touch the wire. Stateless singletons live in BattleSession's registry; a single
/// handler may be registered under multiple URIs (e.g. Retire/Kill).</summary>
internal interface IFrameHandler
{
    IReadOnlyList<DispatchRoute> Handle(FrameDispatchContext ctx);
}
