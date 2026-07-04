using MessagePack;
using SVSim.BattleNode.Wire;

namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Full chain between an envelope and the bytes that ride as a SocketIO binary attachment.
/// Inbound:  bytes → msgpack-string → NodeCrypto.Decrypt → JSON → MsgEnvelope
/// Outbound: MsgEnvelope → JSON → NodeCrypto.Encrypt → msgpack-bytes
/// </summary>
public static class MsgPayloadCodec
{
    public static MsgEnvelope Decode(byte[] msgpackBytes)
    {
        var encryptedString = MessagePackSerializer.Deserialize<string>(msgpackBytes);
        var json = NodeCrypto.DecryptForNode(encryptedString);
        return MsgEnvelope.FromJson(json);
    }

    public static byte[] Encode(MsgEnvelope envelope, string key)
    {
        var json = MsgEnvelope.ToJson(envelope);
        var encryptedString = NodeCrypto.EncryptForNode(json, key);
        return MessagePackSerializer.Serialize(encryptedString);
    }
}
