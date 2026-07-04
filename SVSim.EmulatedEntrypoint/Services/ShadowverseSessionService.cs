using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SVSim.EmulatedEntrypoint.Services;

public class ShadowverseSessionService
{
    /// <summary>
    /// Salt the client's <c>Cute/Cryptographer.MakeMd5</c> appends to every input before hashing.
    /// Must match the decompiled client exactly — the server computes SIDs that the client
    /// also computes locally for its outgoing request headers, and any mismatch breaks decrypt.
    /// </summary>
    private const string MakeMd5Salt = "r!I@ws8e5i=";

    /// <summary>
    /// Default cap for the in-memory SID→UDID map. Each entry is roughly 32B SID + 16B Guid
    /// plus dict + queue overhead — 10k entries ≈ 1 MB of process memory. Sized for the
    /// emulator's expected ceiling, not prod scale. Long-running dev hosts that keep
    /// accumulating signups would otherwise grow this dict unboundedly.
    /// </summary>
    public const int DefaultMaxEntries = 10_000;

    private readonly int _maxEntries;
    private readonly ConcurrentDictionary<string, Guid> _sessionIdToUdid;
    private readonly ConcurrentQueue<string> _insertionOrder;

    public ShadowverseSessionService() : this(DefaultMaxEntries) { }

    public ShadowverseSessionService(int maxEntries)
    {
        if (maxEntries <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxEntries), "Cap must be positive.");
        _maxEntries = maxEntries;
        _sessionIdToUdid = new();
        _insertionOrder = new();
    }

    public Guid? GetUdidFromSessionId(string sid)
    {
        if (_sessionIdToUdid.TryGetValue(sid, out var udid))
        {
            return udid;
        }

        return null;
    }

    public void StoreUdidForSessionId(string sid, Guid udid)
    {
        // FIFO eviction: only enqueue on first insertion so the queue doesn't grow when
        // an existing SID is re-stored (the only realistic "update" — same SID always
        // resolves to the same UDID by construction of ComputeClientSessionId, so this
        // path is effectively a no-op semantically).
        if (_sessionIdToUdid.TryAdd(sid, udid))
        {
            _insertionOrder.Enqueue(sid);
            EvictIfOverCap();
        }
        else
        {
            _sessionIdToUdid[sid] = udid;
        }
    }

    private void EvictIfOverCap()
    {
        while (_sessionIdToUdid.Count > _maxEntries && _insertionOrder.TryDequeue(out var oldest))
        {
            _sessionIdToUdid.TryRemove(oldest, out _);
        }
    }

    /// <summary>
    /// Replicates the client's <c>Cute/Certification.SessionId</c> getter:
    /// <c>MakeMd5(viewerId.ToString() + udid.ToString("D"))</c>. Returned as lowercase hex.
    /// The client computes this once after signup and sends it as the SID header on every
    /// subsequent request — the server must produce the same value to map back to the UDID.
    /// </summary>
    public string ComputeClientSessionId(long viewerId, Guid udid)
    {
        string input = viewerId.ToString(CultureInfo.InvariantCulture)
                     + udid.ToString("D")
                     + MakeMd5Salt;
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Pre-stores the SID→UDID mapping the client will use for its first SID-only request
    /// after <c>/tool/signup</c>. Without this, the translation middleware can't decrypt the
    /// next request body (no UDID header, no mapping, falls back to <c>Guid.Empty</c>).
    /// </summary>
    public void StoreSessionForViewer(long viewerId, Guid udid)
    {
        string sid = ComputeClientSessionId(viewerId, udid);
        StoreUdidForSessionId(sid, udid);
    }
}