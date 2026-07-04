namespace SVSim.BattleNode.Reliability;

/// <summary>
/// Per-session inbound-emit ledger. Dedupes the client's pubSeq so we never dispatch
/// a retransmitted emit twice; ack-echo (via SIO callback) is the caller's job.
/// </summary>
/// <remarks>
/// State is bounded: the ledger keeps the most recent <see cref="WindowSize"/>
/// pubSeqs in an LRU ring. Seqs below <c>HighWaterMark - WindowSize</c> are
/// treated as stale-below-window and rejected without recording — this is what
/// prevents window eviction from re-admitting an old seq as novel. The pubSeq is
/// client-assigned monotonically; the bound is sized well above the realistic
/// Socket.IO retransmit horizon, so legitimate retransmits always fall inside.
/// </remarks>
public sealed class InboundTracker
{
    /// <summary>Sliding-window size. Anything below <c>HighWaterMark - WindowSize</c> is dropped.</summary>
    public const int WindowSize = 256;

    private readonly HashSet<long> _seen = new(WindowSize);
    private readonly Queue<long> _order = new(WindowSize);

    /// <summary>Highest pubSeq observed so far. Reported via Gungnir for diagnostics.</summary>
    public long HighWaterMark { get; private set; }

    /// <summary>Record an incoming pubSeq. Returns true if the caller should dispatch the envelope, false on duplicate or stale-below-window.</summary>
    public bool Observe(long pubSeq)
    {
        // Stale-below-window guard. Required AFTER HighWaterMark is past the window,
        // otherwise an evicted ring entry would re-admit as novel.
        if (HighWaterMark > 0 && pubSeq <= HighWaterMark - WindowSize)
            return false;

        if (pubSeq > HighWaterMark)
        {
            HighWaterMark = pubSeq;
            Record(pubSeq);
            return true;
        }

        if (_seen.Contains(pubSeq))
            return false;
        Record(pubSeq);
        return true;
    }

    private void Record(long pubSeq)
    {
        if (_order.Count >= WindowSize)
        {
            var evicted = _order.Dequeue();
            _seen.Remove(evicted);
        }
        _order.Enqueue(pubSeq);
        _seen.Add(pubSeq);
    }
}
