namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Marker for every type that can appear as <see cref="MsgEnvelope.Body"/>.
/// Implementers fall into two camps: typed records used on the outbound path
/// (one per server-authored frame shape) and <see cref="RawBody"/> used on the inbound
/// path. The marker exists so the envelope can carry either without falling
/// back to <c>object</c>.
/// </summary>
public interface IMsgBody { }
