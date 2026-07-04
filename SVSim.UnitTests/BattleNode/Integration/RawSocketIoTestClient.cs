using System.Net.WebSockets;
using System.Text;
using MessagePack;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Wire;

namespace SVSim.UnitTests.BattleNode.Integration;

/// <summary>
/// Minimal raw-WS Socket.IO v2 client for integration testing. Knows enough to send msg events
/// with one binary attachment, receive synchronize pushes, and ack-callback echo. Takes a
/// connected <see cref="WebSocket"/> (typically from <c>TestServer.CreateWebSocketClient()</c>).
/// </summary>
internal sealed class RawSocketIoTestClient : IAsyncDisposable
{
    private readonly WebSocket _ws;
    private int _nextAckId = 1;

    public RawSocketIoTestClient(WebSocket connectedWebSocket) => _ws = connectedWebSocket;

    public async Task ConsumeHandshakeAsync(CancellationToken ct = default)
    {
        // Receive and discard the EIO Open frame the server sent on connect.
        await ReceiveTextAsync(ct);
    }

    public async Task<MsgEnvelope> ReceiveSynchronizeAsync(CancellationToken ct = default)
    {
        while (true)
        {
            var text = await ReceiveTextAsync(ct);
            var eio = EngineIoFrame.Parse(text);
            if (eio.Type == EngineIoPacketType.Ping)
            {
                await SendTextAsync("3", ct);
                continue;
            }
            if (eio.Type != EngineIoPacketType.Message) continue;

            var sioHeader = SocketIoFrame.Parse(eio.Payload);
            if (sioHeader.AttachmentCount == 0)
            {
                // Could be an ack — ignore.
                continue;
            }
            var attachments = new List<byte[]>();
            for (var i = 0; i < sioHeader.AttachmentCount; i++)
                attachments.Add(await ReceiveBinaryAsync(ct));

            var assembled = sioHeader.WithAttachments(attachments);
            return MsgPayloadCodec.Decode(assembled.BinaryAttachments[0]);
        }
    }

    public async Task SendMsgAsync(MsgEnvelope env, string key, CancellationToken ct = default)
    {
        var bytes = MsgPayloadCodec.Encode(env, key);
        var sio = SocketIoFrame.BinaryEventWithAttachments("msg", new[] { bytes });
        var (text, bins) = sio.Encode();
        // Insert ack id for ackable emits (those with pubSeq).
        if (env.PubSeq.HasValue)
        {
            var id = _nextAckId++;
            // Re-encode with ackId by hand: type + N + - + id + json
            text = $"5{bins.Count}-{id}{text.Substring(text.IndexOf('['))}";
        }
        await SendTextAsync($"{(int)EngineIoPacketType.Message}{text}", ct);
        foreach (var b in bins)
        {
            // EIO v3 binary frames are prefixed with the packet-type byte (0x04 = Message).
            var prefixed = new byte[b.Length + 1];
            prefixed[0] = (byte)EngineIoPacketType.Message;
            Buffer.BlockCopy(b, 0, prefixed, 1, b.Length);
            await _ws.SendAsync(prefixed, WebSocketMessageType.Binary, true, ct);
        }
    }

    private async Task<string> ReceiveTextAsync(CancellationToken ct)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[8192];
        WebSocketReceiveResult result;
        do
        {
            result = await _ws.ReceiveAsync(buffer, ct);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private async Task<byte[]> ReceiveBinaryAsync(CancellationToken ct)
    {
        using var ms = new MemoryStream();
        var buffer = new byte[8192];
        WebSocketReceiveResult result;
        do
        {
            result = await _ws.ReceiveAsync(buffer, ct);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);
        var raw = ms.ToArray();
        // EIO v3 binary frames are prefixed with the packet-type byte (0x04 = Message). Strip it.
        if (raw.Length > 0 && raw[0] == (byte)EngineIoPacketType.Message)
        {
            return raw.AsSpan(1).ToArray();
        }
        return raw;
    }

    private Task SendTextAsync(string text, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return _ws.SendAsync(bytes, WebSocketMessageType.Text, true, ct);
    }

    public ValueTask DisposeAsync()
    {
        // TestServer's in-process WebSocket doesn't always complete the graceful Close
        // handshake — it can hang the test host shutdown. Abrupt dispose is fine for tests:
        // the server-side ReceiveAsync throws WebSocketException, BattleSession.RunAsync
        // returns, and the handler completes.
        _ws.Dispose();
        return ValueTask.CompletedTask;
    }
}

// Note on attachments: the SocketIO v2 protocol can split binary attachments across multiple WS
// frames, but in practice BestHTTP / our codec emits one attachment per binary WS frame, so the
// receive loop assumes that ordering. If integration tests start to flake on multi-attachment
// pushes, revisit ReceiveBinaryAsync to handle multi-frame attachments.
