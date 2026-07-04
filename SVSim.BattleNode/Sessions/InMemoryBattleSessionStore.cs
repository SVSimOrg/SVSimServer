using System.Collections.Concurrent;

namespace SVSim.BattleNode.Sessions;

public sealed class InMemoryBattleSessionStore : IBattleSessionStore
{
    private readonly ConcurrentDictionary<string, PendingBattle> _pending = new();

    public bool TryRegisterPending(PendingBattle battle) =>
        _pending.TryAdd(battle.BattleId, battle);

    public PendingBattle? TryGetPending(string battleId) =>
        _pending.TryGetValue(battleId, out var b) ? b : null;

    public PendingBattle? TryFindPendingForViewer(long viewerId)
    {
        // Linear scan — _pending is bounded by concurrent in-flight matches (low
        // double digits at most), so this stays cheap. Returns whichever match the
        // dictionary's enumerator yields first; in practice a viewer has at most one
        // pending battle since each /do_matching either pairs/falls-back the existing
        // slot or parks without registering.
        foreach (var b in _pending.Values)
        {
            if (b.P1.ViewerId == viewerId) return b;
            if (b.P2 is not null && b.P2.ViewerId == viewerId) return b;
        }
        return null;
    }

    public bool RemovePending(string battleId) =>
        _pending.TryRemove(battleId, out _);
}
