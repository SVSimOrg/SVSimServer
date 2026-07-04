using Microsoft.Extensions.Logging;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Globals;

namespace SVSim.Database.Services.BattleXp;

public sealed class BattleXpService : IBattleXpService
{
    private readonly IGlobalsRepository _globals;
    private readonly IGameConfigService _config;
    private readonly ILogger<BattleXpService> _log;

    // Curve is immutable per boot; cache the first fetch.
    private List<ClassExpEntry>? _cachedCurve;

    public BattleXpService(IGlobalsRepository globals, IGameConfigService config, ILogger<BattleXpService> log)
    {
        _globals = globals;
        _config = config;
        _log = log;
    }

    public async Task<BattleXpGrantResult> GrantAsync(
        Viewer viewer, int classId, bool isWin, BattleXpMode mode, CancellationToken ct = default)
    {
        var row = viewer.Classes.FirstOrDefault(c => c.Class.Id == classId);
        if (row is null)
        {
            _log.LogWarning(
                "BattleXpService: viewer {ViewerId} has no ViewerClassData for classId {ClassId}; skipping grant.",
                viewer.Id, classId);
            return new BattleXpGrantResult(0, 0, 1, LeveledUp: false);
        }

        int amount = ResolveAmount(mode, isWin);
        row.Exp += amount;

        var curve = _cachedCurve ??= await _globals.GetClassExpCurve();
        // curve[level] semantics: XP required WHILE AT that level to reach the next
        // (matching classexp.csv seed values and LoadController's client-facing shape).
        var byLevel = curve.ToDictionary(e => e.Id, e => e.NecessaryExp);
        int maxLevel = curve.Count == 0 ? row.Level : curve.Max(e => e.Id);

        int startingLevel = row.Level;
        while (row.Level < maxLevel
               && byLevel.TryGetValue(row.Level, out var needed)
               && row.Exp >= needed)
        {
            row.Exp -= needed;
            row.Level += 1;
        }

        return new BattleXpGrantResult(amount, row.Exp, row.Level, LeveledUp: row.Level > startingLevel);
    }

    private int ResolveAmount(BattleXpMode mode, bool isWin)
    {
        var cfg = _config.Get<BattleXpConfig>();

        if (mode == BattleXpMode.Story)
            return cfg.StoryXpPerClear ?? cfg.XpPerWin;

        int? overrideVal = (mode, isWin) switch
        {
            (BattleXpMode.Practice,     true)  => cfg.PracticeXpPerWin,
            (BattleXpMode.Practice,     false) => cfg.PracticeXpPerLoss,
            (BattleXpMode.Rank,         true)  => cfg.RankXpPerWin,
            (BattleXpMode.Rank,         false) => cfg.RankXpPerLoss,
            (BattleXpMode.Free,         true)  => cfg.FreeXpPerWin,
            (BattleXpMode.Free,         false) => cfg.FreeXpPerLoss,
            (BattleXpMode.ArenaTwoPick, true)  => cfg.ArenaTwoPickXpPerWin,
            (BattleXpMode.ArenaTwoPick, false) => cfg.ArenaTwoPickXpPerLoss,
            (BattleXpMode.Colosseum,    true)  => cfg.ColosseumXpPerWin,
            (BattleXpMode.Colosseum,    false) => cfg.ColosseumXpPerLoss,
            _ => null,
        };
        return overrideVal ?? (isWin ? cfg.XpPerWin : cfg.XpPerLoss);
    }
}
