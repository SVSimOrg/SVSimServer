namespace SVSim.BattleNode.Bridge;

/// <summary>
/// DI-injected options for the battle node.
/// </summary>
public sealed class BattleNodeOptions
{
    /// <summary>
    /// The Socket.IO v2 endpoint URL echoed back to the client on <c>/*/do_matching</c>
    /// success (matching_state 3004/3007/3011). Matches the prod do_matching wire format:
    /// <c>host[:port]/socket.io/</c> — no scheme prefix, must end with <c>/socket.io/</c>.
    /// The client's BestHTTP <c>SocketManager</c> parses this string directly; a leading
    /// <c>http://</c>/<c>https://</c> or a missing trailing slash will make the client
    /// fail to connect. Host may be an IP, hostname, or FQDN.
    /// <para>
    /// Deployment-time value — no hardcoded fallback. Must be provided via the
    /// <c>"BattleNode:NodeServerUrl"</c> key in <c>appsettings.json</c> (or an
    /// equivalently-named env var); <see cref="Hosting.BattleNodeExtensions.AddBattleNode"/>
    /// validates presence at startup and throws if empty.
    /// </para>
    /// </summary>
    public string NodeServerUrl { get; set; } = "";

    /// <summary>
    /// How long the first arriver's WS waits for a partner before disconnecting.
    /// Matches the architecture spec's 60s default; override (typically lower)
    /// in tests via the factory.
    /// </summary>
    public TimeSpan WaitingRoomTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Cadence of the server→client alive ("Gungnir") keepalive emit. The driving timer/loop
    /// (to live on <see cref="Sessions.BattleSession"/>) is deferred in v1; this is its future
    /// home so the interval isn't a magic literal stranded on the <c>Gungnir</c> body factory.
    /// </summary>
    public TimeSpan AliveEmitInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// When true, <see cref="Sessions.Participants.RealParticipant"/> emits per-frame
    /// diagnostic logs at Information level: <c>[sio-in]</c> on every inbound msg/alive/hand
    /// envelope (URI, pubSeq, ackId, dispatch decision, ack-sent flag, ack arg, inbound
    /// watermark); <c>[sio-out]</c> on every outbound push (URI, pubSeq, playSeq, stock);
    /// <c>[ws-rx-text]</c> / <c>[ws-rx-bin]</c> on every WS frame received at the transport
    /// layer; <c>[ws-recv-exit]</c> / <c>[ws-loop-exit]</c> on read-loop termination
    /// (with WebSocket state + exception type when applicable). Default false — keeps
    /// production logs clean. Flip on per session for live WS debugging, PvP investigation,
    /// or to reproduce the kind of softlock chased in
    /// <c>docs/audits/battle-node-sio-events-2026-06-02.md</c>.
    /// </summary>
    public bool DiagnosticLogging { get; set; } = false;
}
