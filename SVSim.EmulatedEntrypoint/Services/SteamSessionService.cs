using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Validates Steam session tickets against Steamworks for the auth handler. Owns the
/// lifecycle of per-user Steam auth sessions: when a client reconnects with a fresh ticket
/// (typical on every client restart — Steam tickets are per-process), we end the prior
/// session before opening a new one. Without that, Steam rejects the second
/// <c>BeginAuthSession</c> as a duplicate request and the auth handler returns 401.
///
/// See <c>docs/audits/game-start-steam-ticket-401-on-client-restart-2026-05-23.md</c> for
/// the original symptom + the choice of end-then-begin over reactively retrying on
/// DuplicateRequest (Facepunch 2.3.3 collapses the BeginAuthResult enum to bool, so we
/// can't see the duplicate-request signal directly).
/// </summary>
public class SteamSessionService : IDisposable
{
    private const int ShadowVerseAppId = 453480;

    private readonly ISteamServer _steam;
    private readonly ILogger<SteamSessionService> _logger;

    /// <summary>Ticket-bytes-to-steamid for cache hits on identical re-sends (e.g. retries within one client session).</summary>
    private readonly ConcurrentDictionary<string, ulong> _validatedSessionTickets = new();

    /// <summary>steamId → currently-open ticket. Single entry per user; replaced when a new ticket supersedes it.</summary>
    private readonly ConcurrentDictionary<ulong, string> _activeSessionBySteamId = new();

    /// <summary>Per-steamId mutex so the check-end-begin sequence is atomic for that user without serializing all auth.</summary>
    private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _steamIdLocks = new();

    public SteamSessionService(ISteamServer steam, ILogger<SteamSessionService> logger)
    {
        _steam = steam;
        _logger = logger;
    }

    public bool IsTicketValidForUser(string ticket, ulong steamId)
    {
        if (string.IsNullOrEmpty(ticket))
        {
            // Mishaped request body (wrong casing on the field) used to NRE on the dictionary
            // lookup. Fail cleanly so the auth pipeline returns 401 instead of 500.
            return false;
        }

        // Fast path: identical bytes from a prior validated call for this user. Real clients
        // don't replay tickets across restarts (Steam regenerates per-process), but in-process
        // retries can hit this and avoid both the Steam SDK call and the per-user lock.
        if (_validatedSessionTickets.TryGetValue(ticket, out ulong cachedSteamId))
        {
            return cachedSteamId == steamId;
        }

        _steam.Initialize(ShadowVerseAppId);

        byte[] ticketBytes = HexDecode(ticket);

        SemaphoreSlim gate = _steamIdLocks.GetOrAdd(steamId, _ => new SemaphoreSlim(1, 1));
        gate.Wait();
        try
        {
            // Re-check the cache: another caller for the same steamId may have validated this
            // exact ticket while we were waiting on the semaphore.
            if (_validatedSessionTickets.TryGetValue(ticket, out cachedSteamId))
            {
                return cachedSteamId == steamId;
            }

            // If a different ticket is currently open for this steam id, close it first.
            // Steam's BeginAuthSession returns DuplicateRequest (which Facepunch surfaces as
            // false) when the same user already has an open session on this server — that's
            // the entire bug this whole machinery exists to fix.
            if (_activeSessionBySteamId.TryGetValue(steamId, out string? priorTicket)
                && !string.Equals(priorTicket, ticket, StringComparison.Ordinal))
            {
                _logger.LogInformation(
                    "Retiring stale Steam auth session for steamId {SteamId} before opening a new one (prior ticket bytes differ).",
                    steamId);
                _steam.EndSession(steamId);
                _validatedSessionTickets.TryRemove(priorTicket, out _);
                _activeSessionBySteamId.TryRemove(steamId, out _);
            }

            bool accepted = _steam.BeginAuthSession(ticketBytes, steamId);
            if (!accepted)
            {
                _logger.LogWarning("Steam rejected BeginAuthSession for steamId {SteamId}.", steamId);
                return false;
            }

            _validatedSessionTickets[ticket] = steamId;
            _activeSessionBySteamId[steamId] = ticket;
            return true;
        }
        finally
        {
            gate.Release();
        }
    }

    private static byte[] HexDecode(string hex)
    {
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

    public void Dispose()
    {
        // End every tracked session before shutting down so the Steam SDK doesn't see orphaned
        // sessions on the next process. Shutdown does this implicitly, but being explicit keeps
        // observability honest (and tests can assert it on the fake without needing Shutdown).
        foreach (var (steamId, _) in _activeSessionBySteamId)
        {
            try { _steam.EndSession(steamId); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "EndSession during dispose failed for steamId {SteamId}.", steamId);
            }
        }
        _activeSessionBySteamId.Clear();
        _validatedSessionTickets.Clear();
        foreach (var sem in _steamIdLocks.Values) sem.Dispose();
        _steamIdLocks.Clear();

        try { _steam.Shutdown(); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Steam shutdown failed.");
        }
    }
}
