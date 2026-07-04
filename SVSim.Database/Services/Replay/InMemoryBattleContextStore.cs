using System.Collections.Concurrent;

namespace SVSim.Database.Services.Replay;

/// <summary>
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>-backed in-memory store.
/// Lives as a singleton in DI. Server restart drops in-flight contexts —
/// acceptable per spec (history is best-effort; finish handlers warn-log
/// and continue when context is missing).
/// </summary>
public sealed class InMemoryBattleContextStore : IBattleContextStore
{
    private readonly ConcurrentDictionary<long, BattleContext> _contexts = new();

    public void Set(long viewerId, BattleContext ctx)
        => _contexts[viewerId] = ctx;

    public BattleContext? TakeFor(long viewerId)
        => _contexts.TryRemove(viewerId, out var ctx) ? ctx : null;
}
