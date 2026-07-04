namespace SVSim.BattleNode.Reliability;

/// <summary>
/// Body builders for the alive channel ("Gungnir" is the client's codename for the
/// keepalive/connection-status channel — see <see cref="Protocol.Bodies.AlivePushBody"/>).
/// The timer/loop that would drive the emit cadence
/// (<see cref="Bridge.BattleNodeOptions.AliveEmitInterval"/>) is to live on BattleSession;
/// this class is just the pure body-shape factory.
/// v1 always reports scs/ocs=ONLINE — real disconnect detection is deferred. The push
/// body itself is constructed inline in BattleSession.HandleAliveEventAsync using
/// AlivePushBody; only the emit body (sent by us TO the client on the alive channel,
/// currently unused in v1) remains here.
/// </summary>
public static class Gungnir
{
    public static Dictionary<string, object?> BuildAliveEmitBody(InboundTracker tracker) => new()
    {
        ["currentSeq"] = tracker.HighWaterMark,
        // actionSeq omitted in v1 — no turn-transition flag yet.
    };
}
