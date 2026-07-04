using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

/// <summary>Gungnir keepalive push. <c>scs</c> = self connection status, <c>ocs</c> = opponent
/// connection status; both carry <see cref="WireConstants.OnlineStatus"/> ("ONLINE") in v1.
/// Intentionally has no <c>resultCode</c> — the client treats an absent resultCode on alive
/// frames as "no error" (the lone body without one).</summary>
public sealed record AlivePushBody(
    [property: JsonPropertyName("scs")] string Scs,
    [property: JsonPropertyName("ocs")] string Ocs) : IMsgBody;
