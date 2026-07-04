using System.Collections.Concurrent;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.BattleNode.Hosting;

/// <summary>
/// In-process <see cref="IWaitingRoom"/>. Backed by a ConcurrentDictionary of slots
/// keyed by BattleId. Each slot holds the first arriver's RealParticipant and a
/// TaskCompletionSource that gets set when the second arriver Pairs (or cancelled
/// on timeout / abort).
/// </summary>
public sealed class WaitingRoom : IWaitingRoom
{
    private readonly ConcurrentDictionary<string, Slot> _rooms = new();

    public RealParticipant? Pair(string battleId, RealParticipant self)
    {
        if (!_rooms.TryRemove(battleId, out var slot)) return null;
        // Hand `self` (second arriver) to the first arriver's ParkAsync...
        slot.SecondArriverTcs.TrySetResult(self);
        // ...and return the first arriver to the second arriver's handler.
        return slot.FirstArriver;
    }

    public async Task<RealParticipant?> ParkAsync(string battleId, RealParticipant self,
        TimeSpan timeout, CancellationToken ct)
    {
        var slot = new Slot(self);
        if (!_rooms.TryAdd(battleId, slot))
        {
            // Race: a concurrent Park already created a slot for the same BattleId.
            // The bridge mints a fresh BattleId per registration, so this is rare;
            // caller can re-Pair as insurance.
            return null;
        }
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);
        using var reg = timeoutCts.Token.Register(() => slot.SecondArriverTcs.TrySetCanceled());
        try
        {
            return await slot.SecondArriverTcs.Task;
        }
        catch (OperationCanceledException)
        {
            Evict(battleId);
            return null;
        }
    }

    public void Evict(string battleId) => _rooms.TryRemove(battleId, out _);

    private sealed class Slot
    {
        public RealParticipant FirstArriver { get; }
        public TaskCompletionSource<RealParticipant> SecondArriverTcs { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Slot(RealParticipant first) => FirstArriver = first;
    }
}
