using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Reliability;
using SVSim.BattleNode.Wire;

namespace SVSim.BattleNode.Sessions.Participants;

/// <summary>
/// Marker interface implemented by participants that own a handshake-phase cursor.
/// <see cref="BattleSession.ComputeFrames"/> reads the sender's <see cref="Phase"/>
/// when gating the handshake-phase arms (InitNetwork / InitBattle / Loaded / Swap)
/// and the TurnEnd-AfterReady forwarder. Bots don't implement this — they never
/// send the gating URIs.
/// </summary>
internal interface IHasHandshakePhase
{
    HandshakePhase Phase { get; set; }
}

/// <summary>
/// WS-backed participant. Owns the WS read loop, SIO encoding/decoding, per-WS
/// <see cref="OutboundSequencer"/> + <see cref="InboundTracker"/>. Fires
/// <see cref="FrameEmitted"/> on each deduplicated inbound <see cref="MsgEnvelope"/>.
/// PushAsync encodes + sends; ordered pushes get a playSeq from the sequencer,
/// no-stock control pushes bypass it.
/// </summary>
public sealed class RealParticipant : IBattleParticipant, IHasHandshakePhase
{
    /// <summary>WS read-loop receive buffer, in bytes. Messages larger than this are
    /// reassembled across multiple ReceiveAsync calls (see <see cref="ReadCompleteMessageAsync"/>).</summary>
    private const int ReceiveBufferBytes = 8192;

    /// <summary>Engine.IO heartbeat parameters advertised in the open handshake — the
    /// pingInterval/pingTimeout (ms) the BestHTTP client honors. Not related to
    /// <see cref="Bridge.BattleNodeOptions.WaitingRoomTimeout"/> despite the 60s coincidence.</summary>
    private const int EngineIoPingIntervalMs = 25000;
    private const int EngineIoPingTimeoutMs = 60000;

    /// <summary>Length (hex chars) of the Engine.IO session id we mint in the open handshake.</summary>
    private const int EngineIoSidLength = 16;

    /// <summary>Exclusive upper bound for one random hex nibble (0x0..0xF) fed to
    /// <see cref="NodeCrypto.GenerateKey"/>. Distinct concept from <see cref="EngineIoSidLength"/>
    /// despite the shared value 16.</summary>
    private const int KeyHexDigitExclusiveMax = 16;

    private readonly WebSocket _ws;
    private readonly ILogger<RealParticipant> _log;
    private readonly bool _diagnosticLogging;
    private CancellationToken _sessionCt;

    public long ViewerId { get; }
    public MatchContext Context { get; }
    public InboundTracker Inbound { get; } = new();
    public OutboundSequencer Outbound { get; } = new();

    /// <summary>Per-side handshake progression. Session reads this when gating
    /// handshake-phase synthesis (Matched / BattleStart / Deal / Swap response /
    /// Ready). Session transitions via the setter after dispatch. Defaults to
    /// AwaitingInitNetwork; only RealParticipant tracks this — bots have no phase
    /// because they never send the gating URIs. Also satisfies
    /// <see cref="IHasHandshakePhase"/> (the interface BattleSession uses to gate
    /// handshake dispatch without depending on the concrete RealParticipant type).</summary>
    internal HandshakePhase Phase { get; set; } = HandshakePhase.AwaitingInitNetwork;

    HandshakePhase IHasHandshakePhase.Phase
    {
        get => Phase;
        set => Phase = value;
    }

    public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;

    private readonly TaskCompletionSource<bool> _sessionFinished
        = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>Called by the second arriver's handler (in a finally block) after
    /// session.RunAsync completes. Signals the first arriver's handler that it can
    /// return and let the HTTP request complete (which closes the WS).</summary>
    internal void MarkSessionFinished() => _sessionFinished.TrySetResult(true);

    /// <summary>Awaited by the first arriver's handler instead of calling RunAsync
    /// (the session already calls RunAsync on this instance from the second arriver's
    /// handler context — calling it twice would race the WS read loop). Returns when
    /// either MarkSessionFinished fires or the passed CT cancels.</summary>
    internal Task AwaitSessionFinishedAsync(CancellationToken ct)
    {
        if (_sessionFinished.Task.IsCompleted) return _sessionFinished.Task;
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reg = ct.Register(() => tcs.TrySetCanceled(ct));
        _sessionFinished.Task.ContinueWith(t =>
        {
            reg.Dispose();
            if (t.IsCompletedSuccessfully) tcs.TrySetResult(true);
            else if (t.IsFaulted) tcs.TrySetException(t.Exception!.InnerExceptions);
            else tcs.TrySetCanceled();
        }, TaskContinuationOptions.ExecuteSynchronously);
        return tcs.Task;
    }

    public RealParticipant(WebSocket ws, long viewerId, MatchContext context,
        ILogger<RealParticipant> log, bool diagnosticLogging = false)
    {
        _ws = ws;
        _log = log;
        _diagnosticLogging = diagnosticLogging;
        ViewerId = viewerId;
        Context = context;
    }

    public async Task RunAsync(CancellationToken cancellation)
    {
        _sessionCt = cancellation;
        await SendEioOpenAsync(cancellation);

        var buffer = new byte[ReceiveBufferBytes];
        var pendingAttachments = new List<byte[]>();
        SocketIoFrame? pendingFrame = null;
        string exitReason = "loop-condition-false";

        try
        {
            while (_ws.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
            {
                var msg = await ReadCompleteMessageAsync(buffer, cancellation);
                if (msg is null) { exitReason = "read-returned-null"; break; }

                if (msg.Value.IsText)
                {
                    var text = Encoding.UTF8.GetString(msg.Value.Bytes);
                    if (text.Length == 0) continue;

                    EngineIoFrame eio;
                    try { eio = EngineIoFrame.Parse(text); }
                    catch (ArgumentException ex)
                    {
                        _log.LogWarning(ex, "Dropping unparseable EIO frame from viewer {Vid}", ViewerId);
                        continue;
                    }

                    if (_diagnosticLogging)
                    {
                        _log.LogInformation(
                            "[ws-rx-text] viewer={Vid} eioType={Eio} len={Len} preview={Preview}",
                            ViewerId, eio.Type, text.Length,
                            text.Length > 60 ? text.Substring(0, 60) + "..." : text);
                    }
                    if (eio.Type == EngineIoPacketType.Ping)
                    {
                        await SendTextAsync(((int)EngineIoPacketType.Pong).ToString(), cancellation);
                        continue;
                    }
                    if (eio.Type != EngineIoPacketType.Message) continue;

                    SocketIoFrame sio;
                    try { sio = SocketIoFrame.Parse(eio.Payload); }
                    catch (ArgumentException ex)
                    {
                        _log.LogWarning(ex, "Dropping unparseable SIO frame from viewer {Vid}", ViewerId);
                        continue;
                    }
                    if (sio.AttachmentCount > 0)
                    {
                        pendingFrame = sio;
                        pendingAttachments.Clear();
                        continue;
                    }
                    await DispatchSocketIo(sio);
                }
                else
                {
                    var bin = msg.Value.Bytes;
                    if (bin.Length > 0 && bin[0] == (byte)EngineIoPacketType.Message)
                    {
                        bin = bin.AsSpan(1).ToArray();
                    }
                    pendingAttachments.Add(bin);
                    if (_diagnosticLogging)
                    {
                        _log.LogInformation(
                            "[ws-rx-bin] viewer={Vid} binLen={Len} pendingFrame={Pending} attachCount={AttachCount}",
                            ViewerId, bin.Length, pendingFrame?.EventName ?? "(null)", pendingAttachments.Count);
                    }
                    if (pendingFrame is not null && pendingAttachments.Count == pendingFrame.AttachmentCount)
                    {
                        var assembled = pendingFrame.WithAttachments(pendingAttachments.ToArray());
                        pendingFrame = null;
                        await DispatchSocketIo(assembled);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            exitReason = $"throw:{ex.GetType().Name}:{ex.Message}";
            throw;
        }
        finally
        {
            if (_diagnosticLogging)
            {
                // Clean-teardown terminators (normal client close, or a null read after
                // cancellation was requested) log at Info; anything else stays Warn so
                // genuine crashes stay visible even with diagnostics on.
                bool cleanExit = exitReason == "read-returned-null" &&
                    (cancellation.IsCancellationRequested || _ws.State is WebSocketState.CloseReceived or WebSocketState.Closed);
                if (cleanExit)
                {
                    _log.LogInformation(
                        "[ws-loop-exit] viewer={Vid} reason={Reason} wsState={State} cancelled={Cancelled}",
                        ViewerId, exitReason, _ws.State, cancellation.IsCancellationRequested);
                }
                else
                {
                    _log.LogWarning(
                        "[ws-loop-exit] viewer={Vid} reason={Reason} wsState={State} cancelled={Cancelled}",
                        ViewerId, exitReason, _ws.State, cancellation.IsCancellationRequested);
                }
            }
        }
    }

    public async Task PushAsync(MsgEnvelope envelope, Stock stock, CancellationToken ct)
    {
        var stamped = stock == Stock.Bypass ? Outbound.WrapNoStock(envelope) : Outbound.AssignAndArchive(envelope);
        if (_diagnosticLogging)
        {
            _log.LogInformation(
                "[sio-out] viewer={Vid} uri={Uri} pubSeq={Pseq} playSeq={Plseq} stock={Stock}",
                ViewerId, stamped.Uri, stamped.PubSeq, stamped.PlaySeq, stock);
        }
        await EncodeAndSendAsync(stamped, WireConstants.SynchronizeEvent, ct);
    }

    public Task TerminateAsync(BattleFinishReason reason)
    {
        // WS will close via the read loop exiting; nothing to do here.
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseReceived)
        {
            try { _ws.Abort(); } catch { /* best effort */ }
        }
        _ws.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task DispatchSocketIo(SocketIoFrame frame)
    {
        if (frame.Type is SocketIoPacketType.Event or SocketIoPacketType.BinaryEvent)
        {
            switch (frame.EventName)
            {
                case WireConstants.MsgEvent when frame.BinaryAttachments.Count == 1:
                    await HandleMsgEventAsync(frame);
                    return;
                case WireConstants.AliveEvent when frame.BinaryAttachments.Count == 1:
                    await HandleAliveEventAsync(frame);
                    return;
                case WireConstants.HandEvent when frame.BinaryAttachments.Count == 1:
                    await HandleHandEventAsync(frame);
                    return;
            }
        }
        _log.LogDebug("RealParticipant viewer={Vid}: dropping SIO event={Event}", ViewerId, frame.EventName);
    }

    private async Task HandleMsgEventAsync(SocketIoFrame frame)
    {
        try
        {
            MsgEnvelope env;
            try { env = MsgPayloadCodec.Decode(frame.BinaryAttachments[0]); }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "RealParticipant viewer={Vid}: failed to decode msg envelope", ViewerId);
                return;
            }

            bool shouldDispatch = true;
            bool ackSent = false;
            long? ackArg = null;
            if (env.PubSeq.HasValue)
            {
                shouldDispatch = Inbound.Observe(env.PubSeq.Value);
                if (frame.AckId.HasValue)
                {
                    await SendSioAckAsync(frame.AckId.Value, env.PubSeq.Value);
                    ackSent = true;
                    ackArg = env.PubSeq.Value;
                }
            }
            if (_diagnosticLogging)
            {
                _log.LogInformation(
                    "[sio-in] viewer={Vid} uri={Uri} pubSeq={Pseq} ackId={AckId} dispatch={Dispatch} ackSent={AckSent} ackArg={AckArg} highWaterMark={Hwm}",
                    ViewerId, env.Uri, env.PubSeq, frame.AckId, shouldDispatch, ackSent, ackArg, Inbound.HighWaterMark);

                // Full inbound body (orderList/targetList/uList/rand/...). Each client's re-simulation
                // of the opponent's play arrives here as its own inbound frame, so this lets a PvP log
                // show the caster's play and BOTH clients' re-rolled responses side by side — the
                // ground truth for "did the opponent reproduce the RNG draw?" that frame metadata alone
                // can't answer. Serialize defensively: a logging failure must never kill the read loop.
                string body;
                try { body = MsgEnvelope.ToJson(env); }
                catch (Exception ex) { body = $"<serialize-failed: {ex.GetType().Name}: {ex.Message}>"; }
                if (body.Length > 4000) body = body.Substring(0, 4000) + "...(truncated)";
                _log.LogInformation(
                    "[sio-in-body] viewer={Vid} uri={Uri} pubSeq={Pseq} body={Body}",
                    ViewerId, env.Uri, env.PubSeq, body);
            }

            if (!shouldDispatch) return;

            if (FrameEmitted is not null)
            {
                await FrameEmitted.Invoke(env, _sessionCt);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "RealParticipant viewer={Vid}: unhandled in HandleMsgEventAsync", ViewerId);
        }
    }

    /// <summary>
    /// Ack <c>hand</c> events from the client so the client's <c>stockEmitMessageMgr</c>
    /// drains and subsequent emits transmit.
    /// <para>
    /// Wire shape: hand events are <b>not encrypted</b> on the wire — the client's
    /// <c>RealTimeNetworkAgent.CreatePackEmitHandData:815-817</c> calls only
    /// <c>MessagePackSerializer.Serialize(JsonMapper.ToJson(list))</c>, skipping the
    /// <c>CryptAES.encryptForNode</c> wrap that <c>CreatePackEmitData</c> applies to <c>msg</c>
    /// events. The msgpack-wrapped string is a JSON array of the form
    /// <c>[uri_int, viewerId, udid, pubSeq, ...emit_params]</c> — see
    /// <c>EmitMsgUriPack:1456-1458</c> which inserts <c>pubSeq</c> at index 3 of the list
    /// for <c>isHandData</c> emits. The dict's top-level <c>pubSeq</c> stays client-local
    /// (used by its stockEmitMessageMgr.GetSelectData lookup); it's NOT on the wire.
    /// </para>
    /// <para>
    /// In Bot mode the server has no opponent to forward touches to; ack-only is
    /// correct. PvP-side forwarding semantics are unverified — see
    /// <c>docs/audits/battle-node-sio-events-2026-06-02.md</c>.
    /// </para>
    /// <para>
    /// Fire-and-forget hand frames (TOUCH_URI / SELECT_OBJECT_URI / TURN_END_READY_URI) arrive
    /// with no ack-id; we swallow without decoding. Stocked variants (SELECT_SKILL_URI /
    /// SLIDE_OBJECT_URI) arrive with an ack-id and must be acked with the body's <c>pubSeq</c>
    /// or the client's emit queue softlocks behind them.
    /// </para>
    /// </summary>
    private async Task HandleHandEventAsync(SocketIoFrame frame)
    {
        if (!frame.AckId.HasValue)
        {
            // Fire-and-forget; no queue-blocking risk. Swallow without decoding.
            return;
        }
        try
        {
            // No NodeCrypto.DecryptForNode here — hand events are unencrypted on the wire.
            var json = MessagePack.MessagePackSerializer.Deserialize<string>(frame.BinaryAttachments[0]);
            if (_diagnosticLogging)
            {
                _log.LogInformation(
                    "[hand-rx] viewer={Vid} ackId={AckId} bodyLen={Len} body={Body}",
                    ViewerId, frame.AckId, json.Length,
                    json.Length > 200 ? json.Substring(0, 200) + "..." : json);
            }

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            long? pubSeq = null;
            var rootKind = doc.RootElement.ValueKind;
            if (rootKind == System.Text.Json.JsonValueKind.Array)
            {
                // Prod shape: [uri_int, viewerId, udid, pubSeq, ...emit_params].
                var arr = doc.RootElement;
                if (arr.GetArrayLength() > 3
                    && arr[3].ValueKind == System.Text.Json.JsonValueKind.Number)
                {
                    pubSeq = arr[3].GetInt64();
                }
            }
            else if (rootKind == System.Text.Json.JsonValueKind.Object
                     && doc.RootElement.TryGetProperty("pubSeq", out var psEl)
                     && psEl.ValueKind == System.Text.Json.JsonValueKind.Number)
            {
                // Defensive: dict root with top-level pubSeq isn't what the client sends today,
                // but the StockHandData dict shape exists on the client side and a future
                // wire-format change could expose it. Cheap to handle.
                pubSeq = psEl.GetInt64();
            }

            if (pubSeq is null)
            {
                _log.LogWarning(
                    "RealParticipant viewer={Vid}: 'hand' event ackId={AckId} body has no extractable pubSeq " +
                    "(rootKind={Kind}, bodyLen={Len}); acking with 0 as fallback.",
                    ViewerId, frame.AckId, rootKind, json.Length);
                await SendSioAckAsync(frame.AckId.Value, 0);
                return;
            }
            await SendSioAckAsync(frame.AckId.Value, pubSeq.Value);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex,
                "RealParticipant viewer={Vid}: failed to decode 'hand' event body; not acking. ackId={AckId}",
                ViewerId, frame.AckId);
        }
    }

    private async Task HandleAliveEventAsync(SocketIoFrame frame)
    {
        try
        {
            if (frame.AckId.HasValue)
            {
                await SendSioAckAsync(frame.AckId.Value, 0);
            }
            var aliveEnv = new MsgEnvelope(
                Uri: NetworkBattleUri.Gungnir,
                ViewerId: SVSim.BattleNode.Lifecycle.ServerBattleFrames.FakeOpponentViewerId,
                Uuid: WireConstants.ServerUuid,
                Bid: null,
                RetryAttempt: 0,
                Cat: EmitCategory.General,
                PubSeq: null,
                PlaySeq: null,
                Body: new AlivePushBody(Scs: WireConstants.OnlineStatus, Ocs: WireConstants.OnlineStatus));
            var stamped = Outbound.WrapNoStock(aliveEnv);
            await EncodeAndSendAsync(stamped, WireConstants.AliveEvent, _sessionCt);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "RealParticipant viewer={Vid}: unhandled in HandleAliveEventAsync", ViewerId);
        }
    }

    private async Task EncodeAndSendAsync(MsgEnvelope env, string eventName, CancellationToken ct)
    {
        var key = NodeCrypto.GenerateKey(() => RandomNumberGenerator.GetInt32(0, KeyHexDigitExclusiveMax));
        var bytes = MsgPayloadCodec.Encode(env, key);
        var sio = SocketIoFrame.BinaryEventWithAttachments(eventName, new[] { bytes });
        var (text, bins) = sio.Encode();
        var eioText = $"{(int)EngineIoPacketType.Message}{text}";
        await SendTextAsync(eioText, ct);
        foreach (var bin in bins)
        {
            var prefixed = new byte[bin.Length + 1];
            prefixed[0] = (byte)EngineIoPacketType.Message;
            Buffer.BlockCopy(bin, 0, prefixed, 1, bin.Length);
            await _ws.SendAsync(prefixed, WebSocketMessageType.Binary, endOfMessage: true, ct);
        }
    }

    internal static int ClipAckArg(long arg, ILogger log, long viewerId)
    {
        if (arg > int.MaxValue)
        {
            log.LogWarning("RealParticipant viewer={Vid}: pubSeq {Seq} exceeds int.MaxValue; clipping.", viewerId, arg);
            return int.MaxValue;
        }
        if (arg < int.MinValue)
        {
            log.LogWarning("RealParticipant viewer={Vid}: pubSeq {Seq} below int.MinValue; clipping.", viewerId, arg);
            return int.MinValue;
        }
        return (int)arg;
    }

    private async Task SendSioAckAsync(int ackId, long arg)
    {
        var ack = SocketIoFrame.AckResponse(ackId, ClipAckArg(arg, _log, ViewerId));
        var (text, _) = ack.Encode();
        var eioText = $"{(int)EngineIoPacketType.Message}{text}";
        await SendTextAsync(eioText, _sessionCt);
    }

    private async Task SendEioOpenAsync(CancellationToken ct)
    {
        var sid = Guid.NewGuid().ToString("N").Substring(0, EngineIoSidLength);
        var handshake = new EngineIoHandshake(
            sid, Array.Empty<string>(), EngineIoPingIntervalMs, EngineIoPingTimeoutMs).ToJson();
        await SendTextAsync($"0{handshake}", ct);
    }

    private Task SendTextAsync(string text, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return _ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
    }

    private async Task<(byte[] Bytes, bool IsText)?> ReadCompleteMessageAsync(byte[] buffer, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            try { result = await _ws.ReceiveAsync(buffer, ct); }
            catch (OperationCanceledException)
            {
                if (_diagnosticLogging)
                {
                    // A cancellation-requested cancel is a normal session teardown; a spontaneous
                    // OperationCanceled without cancellation being requested is a real anomaly.
                    if (ct.IsCancellationRequested)
                        _log.LogInformation("[ws-recv-exit] viewer={Vid} reason=OperationCanceled wsState={State}", ViewerId, _ws.State);
                    else
                        _log.LogWarning("[ws-recv-exit] viewer={Vid} reason=OperationCanceled wsState={State}", ViewerId, _ws.State);
                }
                return null;
            }
            catch (WebSocketException wsex)
            {
                if (_diagnosticLogging)
                    _log.LogWarning(wsex, "[ws-recv-exit] viewer={Vid} reason=WebSocketException wsState={State} errCode={ErrCode}",
                        ViewerId, _ws.State, wsex.WebSocketErrorCode);
                return null;
            }
            if (result.MessageType == WebSocketMessageType.Close)
            {
                if (_diagnosticLogging)
                {
                    // NormalClosure ("Bye!") at battle-end is expected; abnormal close statuses stay Warn.
                    if (result.CloseStatus == WebSocketCloseStatus.NormalClosure)
                        _log.LogInformation("[ws-recv-exit] viewer={Vid} reason=ClientCloseFrame wsState={State} closeStatus={Status} desc={Desc}",
                            ViewerId, _ws.State, result.CloseStatus, result.CloseStatusDescription);
                    else
                        _log.LogWarning("[ws-recv-exit] viewer={Vid} reason=ClientCloseFrame wsState={State} closeStatus={Status} desc={Desc}",
                            ViewerId, _ws.State, result.CloseStatus, result.CloseStatusDescription);
                }
                return null;
            }
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);
        return (ms.ToArray(), result.MessageType == WebSocketMessageType.Text);
    }
}
