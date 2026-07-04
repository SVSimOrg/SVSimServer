namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Wraps a parsed-dictionary body for the inbound path. <see cref="MsgEnvelope.FromJson"/>
/// returns this; <see cref="MsgEnvelope.ToJson"/> flattens <see cref="Entries"/> back to
/// top-level keys when echoing.
/// </summary>
public sealed class RawBody : IMsgBody
{
    public Dictionary<string, object?> Entries { get; }

    public RawBody(Dictionary<string, object?> entries)
    {
        Entries = entries;
    }
}
