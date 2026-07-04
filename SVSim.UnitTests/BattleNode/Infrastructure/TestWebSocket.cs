using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Channels;

namespace SVSim.UnitTests.BattleNode.Infrastructure;

/// <summary>
/// In-memory <see cref="WebSocket"/> for driving <see cref="SVSim.BattleNode.Sessions.BattleSession"/>
/// without booting the EmulatedEntrypoint host. Enqueue inbound frames with
/// <see cref="EnqueueIncoming"/>; outbound sends land in <see cref="Sends"/>. Call
/// <see cref="CompleteIncoming"/> to signal end-of-stream — the next <see cref="ReceiveAsync"/>
/// returns a Close result so <see cref="SVSim.BattleNode.Sessions.BattleSession.RunAsync"/>
/// exits its loop cleanly.
/// </summary>
public sealed class TestWebSocket : WebSocket
{
    private readonly Channel<TestWebSocketFrame> _incoming = Channel.CreateUnbounded<TestWebSocketFrame>();
    private readonly ConcurrentQueue<TestWebSocketFrame> _sends = new();
    private WebSocketState _state = WebSocketState.Open;
    private TaskCompletionSource<bool>? _sendGate;

    public IReadOnlyCollection<TestWebSocketFrame> Sends => _sends.ToArray();

    public override WebSocketCloseStatus? CloseStatus { get; } = null;
    public override string? CloseStatusDescription { get; } = null;
    public override WebSocketState State => _state;
    public override string? SubProtocol { get; } = null;

    public void EnqueueIncoming(byte[] payload, WebSocketMessageType type)
    {
        _incoming.Writer.TryWrite(new TestWebSocketFrame(payload, type));
    }

    public void CompleteIncoming()
    {
        _incoming.Writer.TryComplete();
    }

    /// <summary>
    /// Install a gate that blocks every subsequent <see cref="SendAsync"/> call until
    /// <see cref="ReleaseSends"/>. Used by the cancellation regression test to suspend
    /// a write while the caller cancels the session CT.
    /// </summary>
    public void BlockNextSend()
    {
        _sendGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public void ReleaseSends()
    {
        _sendGate?.TrySetResult(true);
        _sendGate = null;
    }

    public override async Task<WebSocketReceiveResult> ReceiveAsync(
        ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        try
        {
            var frame = await _incoming.Reader.ReadAsync(cancellationToken);
            var count = Math.Min(buffer.Count, frame.Payload.Length);
            Array.Copy(frame.Payload, 0, buffer.Array!, buffer.Offset, count);
            // Single-fragment frames only — the test never enqueues partial frames.
            return new WebSocketReceiveResult(count, frame.Type, endOfMessage: true);
        }
        catch (ChannelClosedException)
        {
            _state = WebSocketState.Closed;
            return new WebSocketReceiveResult(0, WebSocketMessageType.Close, endOfMessage: true);
        }
    }

    public override async Task SendAsync(
        ArraySegment<byte> buffer, WebSocketMessageType messageType,
        bool endOfMessage, CancellationToken cancellationToken)
    {
        if (_sendGate is not null)
        {
            // Race-aware: stash the gate locally because ReleaseSends nulls the field.
            var gate = _sendGate;
            using var reg = cancellationToken.Register(() => gate.TrySetCanceled(cancellationToken));
            await gate.Task;
        }
        var copy = new byte[buffer.Count];
        Array.Copy(buffer.Array!, buffer.Offset, copy, 0, buffer.Count);
        _sends.Enqueue(new TestWebSocketFrame(copy, messageType));
    }

    public override Task CloseAsync(
        WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        _state = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(
        WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        _state = WebSocketState.CloseSent;
        return Task.CompletedTask;
    }

    public override void Abort() => _state = WebSocketState.Aborted;

    public override void Dispose() { /* no-op */ }
}

public sealed record TestWebSocketFrame(byte[] Payload, WebSocketMessageType Type);
