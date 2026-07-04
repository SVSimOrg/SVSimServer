using System.Security.Cryptography;
using SVSim.BattleNode.Sessions;

namespace SVSim.BattleNode.Bridge;

/// <summary>
/// In-process implementation of <see cref="IMatchingBridge"/>. The HTTP-side
/// matching queue calls <see cref="RegisterBattle"/> once it has decided "these two
/// play each other" or "this viewer is solo (bot)."
/// </summary>
public sealed class MatchingBridge : IMatchingBridge
{
    /// <summary>Battle id is two zero-padded decimal halves concatenated (e.g. "975695" + "075012").
    /// The half-width and the draw bound must stay coupled: bound == 10^digits.</summary>
    private const int BattleIdHalfDigits = 6;
    private const int BattleIdHalfExclusiveMax = 1_000_000; // 10^BattleIdHalfDigits

    private readonly IBattleSessionStore _store;
    private readonly BattleNodeOptions _options;

    public MatchingBridge(IBattleSessionStore store, BattleNodeOptions options)
    {
        _store = store;
        _options = options;
    }

    private const int MaxIdRetries = 5;

    public PendingMatch RegisterBattle(BattlePlayer p1, BattlePlayer? p2, BattleType type)
    {
        ValidateContract(p1, p2, type);
        EvictStaleForViewer(p1.ViewerId);
        if (p2 is not null) EvictStaleForViewer(p2.ViewerId);

        var halfFormat = "D" + BattleIdHalfDigits;

        for (var attempt = 0; attempt < MaxIdRetries; attempt++)
        {
            var hi = RandomNumberGenerator.GetInt32(0, BattleIdHalfExclusiveMax);
            var lo = RandomNumberGenerator.GetInt32(0, BattleIdHalfExclusiveMax);
            var battleId = hi.ToString(halfFormat) + lo.ToString(halfFormat);

            if (_store.TryRegisterPending(new PendingBattle(battleId, type, p1, p2)))
                return new PendingMatch(battleId, _options.NodeServerUrl);
        }

        throw new InvalidOperationException(
            $"Failed to mint a unique battle id after {MaxIdRetries} attempts.");
    }

    private void EvictStaleForViewer(long viewerId)
    {
        var stale = _store.TryFindPendingForViewer(viewerId);
        if (stale is not null)
            _store.RemovePending(stale.BattleId);
    }

    private static void ValidateContract(BattlePlayer p1, BattlePlayer? p2, BattleType type)
    {
        if (p1 is null) throw new ArgumentNullException(nameof(p1));
        switch (type)
        {
            case BattleType.Pvp:
                if (p2 is null) throw new ArgumentException("Pvp requires both p1 and p2.", nameof(p2));
                if (p1.ViewerId == p2.ViewerId)
                    throw new ArgumentException("Pvp requires distinct viewer ids.", nameof(p2));
                break;
            case BattleType.Bot:
                if (p2 is not null) throw new ArgumentException("Bot must have p2==null.", nameof(p2));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown BattleType.");
        }
    }
}
