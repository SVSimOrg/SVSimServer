using SVSim.BattleNode.Bridge;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;

namespace SVSim.EmulatedEntrypoint.Matching;

/// <summary>
/// DB-backed bot roster. Reads <c>BotRoster</c> rows (seeded from
/// <c>seeds/bot-roster.json</c>) and picks one deterministically per
/// <see cref="MatchContext"/>. See <see cref="IBotRoster"/> for the contract.
/// </summary>
public sealed class BotRoster : IBotRoster
{
    private readonly IGlobalsRepository _globals;

    public BotRoster(IGlobalsRepository globals)
    {
        _globals = globals;
    }

    public async Task<AIBotProfile> PickAsync(MatchContext selfCtx, string battleId, CancellationToken ct = default)
    {
        var roster = await _globals.GetBotRoster();
        if (roster.Count == 0)
        {
            throw new InvalidOperationException(
                "BotRoster is empty. Run SVSim.Bootstrap to import seeds/bot-roster.json.");
        }

        // Deterministic per battle ID: same pending battle → same bot on retry,
        // but different battles get different opponents.
        var hash = StringComparer.Ordinal.GetHashCode(battleId);
        var index = (int)((uint)hash % roster.Count);
        var row = roster[index];
        return ToProfile(row);
    }

    private static AIBotProfile ToProfile(BotRosterEntry row) => new(
        AiId: row.AiId,
        CountryCode: row.CountryCode,
        UserName: row.UserName,
        SleeveId: row.SleeveId,
        EmblemId: row.EmblemId,
        DegreeId: row.DegreeId,
        FieldId: row.FieldId,
        IsOfficial: row.IsOfficial,
        ClassId: row.ClassId,
        CharaId: row.CharaId,
        Rank: row.Rank,
        BattlePoint: row.BattlePoint,
        IsMasterRank: row.IsMasterRank,
        MasterPoint: row.MasterPoint);
}
