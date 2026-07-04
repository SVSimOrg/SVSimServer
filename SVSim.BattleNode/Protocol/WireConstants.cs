namespace SVSim.BattleNode.Protocol;

/// <summary>
/// String constants that show up on the wire as opaque tags. Lifting them out of
/// inline string literals gives each one a single source of truth and a name that
/// reads at the use site.
/// </summary>
internal static class WireConstants
{
    /// <summary>SIO event name for ordered server-pushed frames (the lifecycle channel).</summary>
    public const string SynchronizeEvent = "synchronize";

    /// <summary>SIO event name for client-emitted msg frames + their ack-responses.</summary>
    public const string MsgEvent = "msg";

    /// <summary>SIO event name for Gungnir keepalive frames (both directions).</summary>
    public const string AliveEvent = "alive";

    /// <summary>
    /// SIO event name for client-emitted hand frames (touches + skill/object selection).
    /// Stocked variants (<c>SELECT_SKILL_URI</c>, <c>SLIDE_OBJECT_URI</c>) carry an ack-id;
    /// fire-and-forget variants (<c>TOUCH_URI</c>, <c>SELECT_OBJECT_URI</c>,
    /// <c>TURN_END_READY_URI</c>) do not. The body wire shape differs from <c>msg</c>
    /// frames — see <c>HandleHandEventAsync</c>.
    /// </summary>
    public const string HandEvent = "hand";

    /// <summary>
    /// Placeholder UUID we stamp on every server-originated envelope. Prod servers stamp a
    /// real per-request UUID; the client doesn't validate it.
    /// </summary>
    public const string ServerUuid = "node-stub";

    /// <summary>Gungnir scs/ocs value the v1 server reports unconditionally.</summary>
    public const string OnlineStatus = "ONLINE";
}
