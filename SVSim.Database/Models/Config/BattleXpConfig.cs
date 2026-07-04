namespace SVSim.Database.Models.Config;

/// <summary>
/// Class XP awarded on battle finish. <see cref="XpPerWin"/> / <see cref="XpPerLoss"/>
/// are the global defaults; each per-mode nullable slot overrides them for that mode
/// when set. Story is clear-only (no loss variant) — <see cref="StoryXpPerClear"/>
/// falls back to <see cref="XpPerWin"/> when null.
/// </summary>
[ConfigSection("BattleXp")]
public class BattleXpConfig
{
    public int XpPerWin  { get; set; } = 200;
    public int XpPerLoss { get; set; } = 50;

    public int? PracticeXpPerWin      { get; set; }
    public int? PracticeXpPerLoss     { get; set; }
    public int? RankXpPerWin          { get; set; }
    public int? RankXpPerLoss         { get; set; }
    public int? FreeXpPerWin          { get; set; }
    public int? FreeXpPerLoss         { get; set; }
    public int? ArenaTwoPickXpPerWin  { get; set; }
    public int? ArenaTwoPickXpPerLoss { get; set; }
    public int? ColosseumXpPerWin     { get; set; }
    public int? ColosseumXpPerLoss    { get; set; }
    public int? StoryXpPerClear       { get; set; }

    public static BattleXpConfig ShippedDefaults() => new();
}
