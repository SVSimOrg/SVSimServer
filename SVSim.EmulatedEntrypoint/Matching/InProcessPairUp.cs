using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Sessions;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// In-process pair-up service: one slot per mode, FCFS pairing for PvP, plus an
/// AI-fallback branch for modes whose <see cref="ModePolicy"/> is
/// <see cref="PolicyKind.PvpFirstThenAiFallback"/>. The proper matching-queue API
/// is a separate spec; this is the Phase-2 + Phase-3 placeholder.
/// </summary>
/// <remarks>
/// Singleton (process-wide slot state) consuming a scoped <see cref="IGameConfigService"/>
/// via <see cref="IServiceScopeFactory"/>. The config read is cheap — one DB read per
/// pair-up call — and avoids caching policy decisions across config edits.
/// </remarks>
public sealed class InProcessPairUp : IMatchingPairUpService
{
    /// <summary>
    /// Safety backstop: if a waiter has been parked for more than this and a new
    /// arriver shows up, treat the slot as empty (the original waiter has
    /// presumably stopped polling). Well above the AI-fallback threshold so it
    /// only fires for PvpOnly modes.
    /// </summary>
    private static readonly TimeSpan StaleWaiterEvictionAge = TimeSpan.FromMinutes(5);

    private readonly IMatchingBridge _bridge;
    private readonly ModePolicyRegistry _policies;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _clock;
    private readonly ConcurrentDictionary<string, ModeSlot> _slots = new();

    public InProcessPairUp(
        IMatchingBridge bridge,
        ModePolicyRegistry policies,
        IServiceScopeFactory scopeFactory,
        TimeProvider clock)
    {
        _bridge = bridge;
        _policies = policies;
        _scopeFactory = scopeFactory;
        _clock = clock;
    }

    public Task<PairUpResult?> TryPairAsync(string mode, BattlePlayer player, CancellationToken ct)
    {
        var policy = _policies.For(mode);
        var threshold = TimeSpan.FromSeconds(GetThresholdSeconds());
        var slot = _slots.GetOrAdd(mode, _ => new ModeSlot());

        lock (slot.Lock)
        {
            // 1. Already-resolved match cached for this viewer? Consume + return.
            //    The caller is the FIRST arriver picking up their cached pair — owner role.
            if (slot.Resolved.TryGetValue(player.ViewerId, out var cached))
            {
                slot.Resolved.Remove(player.ViewerId);
                return Task.FromResult<PairUpResult?>(
                    new PairUpResult(cached.Match, IsOwner: true, IsAiFallback: cached.IsAiFallback));
            }

            // 2. Stale waiter eviction backstop.
            if (slot.Waiting is not null && slot.WaitingSince is { } since
                && _clock.GetUtcNow() - since > StaleWaiterEvictionAge)
            {
                slot.Waiting = null;
                slot.WaitingSince = null;
            }

            // 3. Different viewer already waiting? Pair them.
            if (slot.Waiting is not null && slot.Waiting.ViewerId != player.ViewerId)
            {
                var p1 = slot.Waiting;
                var p2 = player;
                slot.Waiting = null;
                slot.WaitingSince = null;
                var match = _bridge.RegisterBattle(p1, p2, BattleType.Pvp);
                // Cache for the FIRST arriver's next poll (consume-on-read).
                slot.Resolved[p1.ViewerId] = (match, IsAiFallback: false);
                return Task.FromResult<PairUpResult?>(
                    new PairUpResult(match, IsOwner: false, IsAiFallback: false));
            }

            // 4. Caller IS the waiter AND policy permits AI fallback AND threshold elapsed?
            if (slot.Waiting?.ViewerId == player.ViewerId
                && policy.Kind == PolicyKind.PvpFirstThenAiFallback
                && slot.WaitingSince is { } parkedAt
                && _clock.GetUtcNow() - parkedAt >= threshold)
            {
                slot.Waiting = null;
                slot.WaitingSince = null;
                var match = _bridge.RegisterBattle(player, null, BattleType.Bot);
                return Task.FromResult<PairUpResult?>(
                    new PairUpResult(match, IsOwner: true, IsAiFallback: true));
            }

            // 5. Park (first time only — preserve WaitingSince across sub-threshold re-polls).
            if (slot.Waiting is null)
            {
                slot.Waiting = player;
                slot.WaitingSince = _clock.GetUtcNow();
            }
            return Task.FromResult<PairUpResult?>(null);
        }
    }

    /// <summary>
    /// Resolves the current AI-fallback threshold from the scoped
    /// <see cref="IGameConfigService"/>. Singleton-safe via per-call scope creation.
    /// </summary>
    private int GetThresholdSeconds()
    {
        using var scope = _scopeFactory.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IGameConfigService>();
        return config.Get<MatchingConfig>().RankBattleAiFallbackThresholdSeconds;
    }

    private sealed class ModeSlot
    {
        public BattlePlayer? Waiting { get; set; }
        public DateTimeOffset? WaitingSince { get; set; }
        public Dictionary<long, (PendingMatch Match, bool IsAiFallback)> Resolved { get; } = new();
        public object Lock { get; } = new();
    }
}
