using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;
using SVSim.BattleNode.Wire;

namespace SVSim.BattleNode.Hosting;

/// <summary>
/// Validates an incoming WebSocket upgrade request, accepts it, and hands off to a fresh
/// <see cref="BattleSession"/>. Singleton; no per-request state.
/// </summary>
/// <remarks>
/// <para>The validation chain — cheapest checks first, crypto only after both params are
/// present, WS accept only after the store lookup confirms the credentials match an outstanding
/// pending battle:</para>
/// <list type="number">
///   <item>Reject non-WS requests with 400 (someone hit <c>/socket.io/</c> via plain HTTP).</item>
///   <item>Read <c>BattleId</c> and encrypted <c>viewerId</c> from request headers, falling back
///         to query string. The real client puts them on headers despite BestHTTP's
///         <c>AdditionalQueryParams</c> API name — see project README §Wire-format gotchas.</item>
///   <item>Decrypt the viewerId with <see cref="NodeCrypto.DecryptForNode"/>; reject on
///         parse/decrypt failure.</item>
///   <item>Look up the <see cref="PendingBattle"/> in the store and verify the decrypted viewer
///         matches the one the <see cref="Bridge.IMatchingBridge"/> registered.</item>
///   <item>AcceptWebSocketAsync, remove the pending entry (it's now an active session), construct
///         <see cref="BattleSession"/>, await <see cref="BattleSession.RunAsync"/> until the WS
///         closes.</item>
/// </list>
/// </remarks>
public sealed class BattleNodeWebSocketHandler
{
    /// <summary>Header/query key names carrying the upgrade credentials — the auth contract
    /// with the client (and the loader that sets them). Single source of truth for both ends.</summary>
    private const string BattleIdCredential = "BattleId";
    private const string ViewerIdCredential = "viewerId";

    /// <summary>Grace period for the close handshake on a bail-out path. A fresh, short timeout —
    /// <c>ctx.RequestAborted</c> may already be canceled by the path that decided to bail.</summary>
    private static readonly TimeSpan PoliteCloseTimeout = TimeSpan.FromSeconds(5);

    private readonly IBattleSessionStore _store;
    private readonly IWaitingRoom _waitingRoom;
    private readonly BattleNodeOptions _options;
    private readonly ILogger<BattleNodeWebSocketHandler> _log;
    private readonly ILoggerFactory _loggerFactory;

    public BattleNodeWebSocketHandler(
        IBattleSessionStore store,
        IWaitingRoom waitingRoom,
        BattleNodeOptions options,
        ILoggerFactory loggerFactory)
    {
        _store = store;
        _waitingRoom = waitingRoom;
        _options = options;
        _loggerFactory = loggerFactory;
        _log = loggerFactory.CreateLogger<BattleNodeWebSocketHandler>();
    }

    /// <summary>
    /// Endpoint entry point. Sets <see cref="HttpContext.Response"/> to 400 on any validation
    /// failure; otherwise upgrades to a WebSocket and awaits
    /// <see cref="BattleSession.RunAsync"/> until the connection closes.
    /// </summary>
    public async Task HandleAsync(HttpContext ctx)
    {
        // Status code mapping: 400 protocol violations (not WS, missing creds);
        // 401 credential validation failures (decrypt, viewer mismatch); 404 unknown
        // BattleId. Log messages carry the diagnostic detail; the wire code gives the
        // client class of failure.
        if (!ctx.WebSockets.IsWebSocketRequest)
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // BestHTTP's SocketOptions.AdditionalQueryParams puts these on HTTP request HEADERS
        // for the WebSocket-only transport (not on the URL query string). Real clients
        // therefore send BattleId/viewerId as headers; the integration test sends them as
        // query params for convenience. Check headers first, fall back to query.
        var battleId = ReadCredential(ctx, BattleIdCredential);
        var encryptedViewerId = ReadCredential(ctx, ViewerIdCredential);
        if (string.IsNullOrEmpty(battleId) || string.IsNullOrEmpty(encryptedViewerId))
        {
            _log.LogWarning("WS upgrade missing BattleId or viewerId (header or query).");
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        long viewerId;
        try
        {
            var plain = NodeCrypto.DecryptForNode(encryptedViewerId);
            viewerId = long.Parse(plain);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "viewerId failed to decrypt (encryptedLen={Len})", encryptedViewerId.Length);
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var pending = _store.TryGetPending(battleId);
        if (pending is null)
        {
            _log.LogWarning(
                "WS upgrade for unknown BattleId={Bid} (decrypted viewerId={Vid}). " +
                "Bridge may not have minted this battle, or it was already consumed/expired.",
                battleId, viewerId);
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        var isP1 = viewerId == pending.P1.ViewerId;
        var isP2 = pending.P2 is not null && viewerId == pending.P2.ViewerId;
        if (!isP1 && !isP2)
        {
            _log.LogWarning(
                "WS upgrade viewer-id mismatch on BattleId={Bid}: bridge expected={P1}/{P2}, decrypted={Got}.",
                battleId, pending.P1.ViewerId, pending.P2?.ViewerId, viewerId);
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var ws = await ctx.WebSockets.AcceptWebSocketAsync();

        switch (pending.Type)
        {
            case BattleType.Pvp:
            {
                // Pick this connection's MatchContext (P1's if isP1, P2's if isP2).
                var selfCtx = isP1 ? pending.P1.Context : pending.P2!.Context;
                var self = new RealParticipant(ws, viewerId, selfCtx,
                    _loggerFactory.CreateLogger<RealParticipant>(), _options.DiagnosticLogging);

                var firstArriver = _waitingRoom.Pair(battleId, self);

                if (firstArriver is not null)
                {
                    // We are the SECOND arriver. Construct and drive the session.
                    _store.RemovePending(battleId);
                    var session = new BattleSession(
                        battleId, BattleType.Pvp, firstArriver, self,
                        _loggerFactory.CreateLogger<BattleSession>());
                    try
                    {
                        await session.RunAsync(ctx.RequestAborted);
                    }
                    finally
                    {
                        firstArriver.MarkSessionFinished();
                    }
                }
                else
                {
                    // We are the FIRST arriver. Park; ParkAsync returns the second arriver
                    // on pairing, null on timeout / cancellation / TryAdd race.
                    var second = await _waitingRoom.ParkAsync(
                        battleId, self, _options.WaitingRoomTimeout, ctx.RequestAborted);

                    if (second is null)
                    {
                        // Either timeout (most common) or Park/Park race. Retry Pair once.
                        second = _waitingRoom.Pair(battleId, self);
                        if (second is null)
                        {
                            _log.LogWarning(
                                "PvP waiting-room timeout or race on BattleId={Bid}; first arriver disconnected.",
                                battleId);
                            _store.RemovePending(battleId);
                            await TryPoliteCloseAsync(ws, "waiting-room timeout", battleId);
                            return;
                        }
                        // Retry succeeded — we're the de-facto second arriver now. Own the session.
                        _store.RemovePending(battleId);
                        var raceSession = new BattleSession(
                            battleId, BattleType.Pvp, second, self,
                            _loggerFactory.CreateLogger<BattleSession>());
                        try { await raceSession.RunAsync(ctx.RequestAborted); }
                        finally { second.MarkSessionFinished(); }
                        return;
                    }

                    // Normal first-arriver path: session is being constructed/driven by the
                    // second arriver. Hold this HTTP request open until they signal completion.
                    // Do NOT call self.RunAsync — the session already does.
                    await self.AwaitSessionFinishedAsync(ctx.RequestAborted);
                }
                break;
            }

            case BattleType.Bot:
            {
                // Phase 3: real (Real, NoOp) session. Bot's pending always has P2 == null
                // (per IMatchingBridge contract validation), so isP1 must be true here. The
                // earlier isP1/isP2 check has already rejected viewer mismatches.
                _store.RemovePending(battleId);
                var botReal = new RealParticipant(ws, viewerId, pending.P1.Context,
                    _loggerFactory.CreateLogger<RealParticipant>(), _options.DiagnosticLogging);
                var noopBot = new NoOpBotParticipant();
                var botSession = new BattleSession(battleId, BattleType.Bot, botReal, noopBot,
                    _loggerFactory.CreateLogger<BattleSession>());
                await botSession.RunAsync(ctx.RequestAborted);
                break;
            }

            default:
                _log.LogError("Unknown BattleType={Type} for BattleId={Bid}; closing WS", pending.Type, battleId);
                await TryPoliteCloseAsync(ws, $"unknown BattleType={pending.Type}", battleId);
                return;
        }
    }

    private static string ReadCredential(HttpContext ctx, string name)
    {
        var header = ctx.Request.Headers[name].ToString();
        if (!string.IsNullOrEmpty(header)) return header;
        return ctx.Request.Query[name].ToString();
    }

    /// <summary>
    /// Emit an EIO <c>1</c> (Close) text frame, then run the WebSocket close handshake with
    /// <see cref="WebSocketCloseStatus.NormalClosure"/>. Without the EIO frame, BestHTTP /
    /// socket.io-client log the disconnect as an abrupt drop rather than a controlled
    /// disconnect; without the close handshake, the client only sees the TCP teardown after
    /// Kestrel finishes draining. Best-effort: any exception (already-torn-down socket,
    /// canceled token) is swallowed at Debug level since teardown races are routine.
    /// </summary>
    private async Task TryPoliteCloseAsync(WebSocket ws, string reason, string battleId)
    {
        using var cts = new CancellationTokenSource(PoliteCloseTimeout);
        try
        {
            if (ws.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(((int)EngineIoPacketType.Close).ToString());
                await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cts.Token);
            }
            if (ws.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, cts.Token);
            }
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex,
                "polite close failed on BattleId={Bid} (reason={Reason}); socket likely already torn down.",
                battleId, reason);
        }
    }
}
