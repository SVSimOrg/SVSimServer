namespace SVSim.BattleNode.Wire;

/// <summary>
/// Engine.IO v3 packet in WebSocket transport mode. Wire form: <c>&lt;digit&gt;&lt;payload&gt;</c>.
/// </summary>
public sealed record EngineIoFrame(EngineIoPacketType Type, string Payload)
{
    public static EngineIoFrame Parse(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            throw new ArgumentException("Empty EIO frame", nameof(raw));
        var typeChar = raw[0];
        if (typeChar < '0' || typeChar > '6')
            throw new ArgumentException($"Invalid EIO type char '{typeChar}'", nameof(raw));
        var type = (EngineIoPacketType)(typeChar - '0');
        var payload = raw.Length > 1 ? raw.Substring(1) : string.Empty;
        return new EngineIoFrame(type, payload);
    }

    public string Encode() => $"{(int)Type}{Payload}";
}
