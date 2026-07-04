using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotently upserts the 9 reference-data tables (classes, leader skins, sleeves, emblems,
/// degrees, battlefields, my-page backgrounds, ranks, class-XP) from CSVs under
/// <c>{AppContext.BaseDirectory}/Data/</c>. Order within ImportAllAsync respects FK
/// dependencies (Classes before LeaderSkins).
/// </summary>
public class ReferenceDataImporter
{
    private readonly TextWriter _out;
    private readonly TextWriter _err;

    public ReferenceDataImporter() : this(Console.Out, Console.Error) { }

    /// <summary>
    /// Pass <see cref="TextWriter.Null"/> for both to silence progress banners (tests
    /// instantiate this importer ~500 times per run; the captured stdout otherwise OOMs
    /// the NUnit trx serializer).
    /// </summary>
    public ReferenceDataImporter(TextWriter output, TextWriter error)
    {
        _out = output;
        _err = error;
    }

    public async Task ImportAllAsync(SVSimDbContext context, string dataDir)
    {
        if (!Directory.Exists(dataDir))
        {
            _err.WriteLine($"[ReferenceDataImporter] Data dir missing: {dataDir}");
            return;
        }
        _out.WriteLine($"[ReferenceDataImporter] Reading CSVs from {dataDir}...");

        await ImportClasses(context, dataDir);
        await ImportLeaderSkins(context, dataDir);
        await ImportSleeves(context, dataDir);
        await ImportEmblems(context, dataDir);
        await ImportDegrees(context, dataDir);
        await ImportBattlefields(context, dataDir);
        await ImportMyPageBackgrounds(context, dataDir);
        await ImportRankInfo(context, dataDir);
        await ImportClassExp(context, dataDir);

        _out.WriteLine("[ReferenceDataImporter] Done.");
    }

    private async Task ImportClasses(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<ClassEntry, ClassEntryMap>(dir, "classes.csv");
        var existing = await ctx.Classes.ToDictionaryAsync(c => c.Id);
        int created = 0, updated = 0;
        foreach (var r in rows)
        {
            if (existing.TryGetValue(r.Id, out var e))
            {
                if (e.Name != r.Name) { e.Name = r.Name; updated++; }
            }
            else { ctx.Classes.Add(r); created++; }
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] Classes: +{created} / ~{updated}");
    }

    private async Task ImportLeaderSkins(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<LeaderSkinEntry, LeaderSkinEntryMap>(dir, "leaderskins.csv");
        // CSV writes class_chara_id=0 for neutral/unassigned; the FK column is nullable.
        foreach (var r in rows) if (r.ClassId == 0) r.ClassId = null;

        var existing = await ctx.LeaderSkins.ToDictionaryAsync(s => s.Id);
        int created = 0, updated = 0;
        foreach (var r in rows)
        {
            if (existing.TryGetValue(r.Id, out var e))
            {
                bool changed = false;
                if (e.Name != r.Name) { e.Name = r.Name; changed = true; }
                if (e.ClassId != r.ClassId) { e.ClassId = r.ClassId; changed = true; }
                if (changed) updated++;
            }
            else { ctx.LeaderSkins.Add(r); created++; }
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] LeaderSkins: +{created} / ~{updated}");
    }

    private async Task ImportSleeves(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<SleeveEntry, SleeveEntryMap>(dir, "sleeves.csv");
        var existing = (await ctx.Sleeves.ToListAsync()).ToHashSet();
        int created = 0;
        foreach (var r in rows)
        {
            if (existing.Any(e => e.Id == r.Id)) continue;
            ctx.Sleeves.Add(r); created++;
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] Sleeves: +{created}");
    }

    private async Task ImportEmblems(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<EmblemEntry, EmblemEntryMap>(dir, "emblems.csv");
        var existing = (await ctx.Emblems.Select(e => e.Id).ToListAsync()).ToHashSet();
        int created = 0;
        foreach (var r in rows)
        {
            if (existing.Contains(r.Id)) continue;
            ctx.Emblems.Add(r); created++;
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] Emblems: +{created}");
    }

    private async Task ImportDegrees(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<DegreeEntry, DegreeEntryMap>(dir, "degrees.csv");
        var existing = (await ctx.Degrees.Select(e => e.Id).ToListAsync()).ToHashSet();
        int created = 0;
        foreach (var r in rows)
        {
            if (existing.Contains(r.Id)) continue;
            ctx.Degrees.Add(r); created++;
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] Degrees: +{created}");
    }

    private async Task ImportBattlefields(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<BattlefieldEntry, BattlefieldEntryMap>(dir, "battlefields.csv");
        var existing = await ctx.Battlefields.ToDictionaryAsync(b => b.Id);
        int created = 0, updated = 0;
        foreach (var r in rows)
        {
            if (existing.TryGetValue(r.Id, out var e))
            {
                if (e.IsOpen != r.IsOpen) { e.IsOpen = r.IsOpen; updated++; }
            }
            else { ctx.Battlefields.Add(r); created++; }
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] Battlefields: +{created} / ~{updated}");
    }

    private async Task ImportMyPageBackgrounds(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<MyPageBackgroundEntry, MyPageBackgroundEntryMap>(dir, "mypagebackgrounds.csv");
        var existing = (await ctx.MyPageBackgrounds.Select(e => e.Id).ToListAsync()).ToHashSet();
        int created = 0;
        foreach (var r in rows)
        {
            if (existing.Contains(r.Id)) continue;
            ctx.MyPageBackgrounds.Add(r); created++;
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] MyPageBackgrounds: +{created}");
    }

    private async Task ImportRankInfo(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<RankInfoEntry, RankInfoEntryMap>(dir, "ranks.csv");
        var existing = await ctx.RankInfo.ToDictionaryAsync(r => r.Id);
        int created = 0, updated = 0;
        foreach (var r in rows)
        {
            if (existing.TryGetValue(r.Id, out var e))
            {
                if (ApplyRankUpdates(e, r)) updated++;
            }
            else { ctx.RankInfo.Add(r); created++; }
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] RankInfo: +{created} / ~{updated}");
    }

    private static bool ApplyRankUpdates(RankInfoEntry e, RankInfoEntry r)
    {
        bool changed = false;
        if (e.Name != r.Name) { e.Name = r.Name; changed = true; }
        if (e.NecessaryPoint != r.NecessaryPoint) { e.NecessaryPoint = r.NecessaryPoint; changed = true; }
        if (e.AccumulatePoint != r.AccumulatePoint) { e.AccumulatePoint = r.AccumulatePoint; changed = true; }
        if (e.LowerLimitPoint != r.LowerLimitPoint) { e.LowerLimitPoint = r.LowerLimitPoint; changed = true; }
        if (e.BaseAddBp != r.BaseAddBp) { e.BaseAddBp = r.BaseAddBp; changed = true; }
        if (e.BaseDropBp != r.BaseDropBp) { e.BaseDropBp = r.BaseDropBp; changed = true; }
        if (e.StreakBonusPt != r.StreakBonusPt) { e.StreakBonusPt = r.StreakBonusPt; changed = true; }
        if (e.WinBonus != r.WinBonus) { e.WinBonus = r.WinBonus; changed = true; }
        if (e.LoseBonus != r.LoseBonus) { e.LoseBonus = r.LoseBonus; changed = true; }
        if (e.MaxWinBonus != r.MaxWinBonus) { e.MaxWinBonus = r.MaxWinBonus; changed = true; }
        if (e.MaxLoseBonus != r.MaxLoseBonus) { e.MaxLoseBonus = r.MaxLoseBonus; changed = true; }
        if (e.IsPromotionWar != r.IsPromotionWar) { e.IsPromotionWar = r.IsPromotionWar; changed = true; }
        if (e.MatchCount != r.MatchCount) { e.MatchCount = r.MatchCount; changed = true; }
        if (e.NecessaryWin != r.NecessaryWin) { e.NecessaryWin = r.NecessaryWin; changed = true; }
        if (e.ResetLose != r.ResetLose) { e.ResetLose = r.ResetLose; changed = true; }
        if (e.AccumulateMasterPoint != r.AccumulateMasterPoint) { e.AccumulateMasterPoint = r.AccumulateMasterPoint; changed = true; }
        return changed;
    }

    private async Task ImportClassExp(SVSimDbContext ctx, string dir)
    {
        var rows = ReadCsv<ClassExpEntry, ClassExpEntryMap>(dir, "classexp.csv");
        var existing = await ctx.ClassExpCurve.ToDictionaryAsync(c => c.Id);
        int created = 0, updated = 0;
        foreach (var r in rows)
        {
            if (existing.TryGetValue(r.Id, out var e))
            {
                if (e.NecessaryExp != r.NecessaryExp) { e.NecessaryExp = r.NecessaryExp; updated++; }
            }
            else { ctx.ClassExpCurve.Add(r); created++; }
        }
        await ctx.SaveChangesAsync();
        _out.WriteLine($"[ReferenceDataImporter] ClassExp: +{created} / ~{updated}");
    }

    private List<T> ReadCsv<T, TMap>(string dir, string fileName) where TMap : ClassMap<T>, new()
    {
        string path = Path.Combine(dir, fileName);
        if (!File.Exists(path))
        {
            _err.WriteLine($"[ReferenceDataImporter] Missing CSV: {path}");
            return new List<T>();
        }
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TMap>();
        return csv.GetRecords<T>().ToList();
    }

    private sealed class ClassEntryMap : ClassMap<ClassEntry>
    {
        public ClassEntryMap()
        {
            Map(m => m.Id).Name("id");
            Map(m => m.Name).Name("name");
            Map(m => m.DefaultLeaderSkin).Ignore();
        }
    }

    private sealed class LeaderSkinEntryMap : ClassMap<LeaderSkinEntry>
    {
        public LeaderSkinEntryMap()
        {
            Map(m => m.Id).Name("class_chara_id");
            Map(m => m.Name).Name("class_chara_name");
            Map(m => m.ClassId).Name("clan");
            Map(m => m.Class).Ignore();
            Map(m => m.Viewers).Ignore();
            Map(m => m.EmoteId).Ignore();
        }
    }

    private sealed class EmblemEntryMap : ClassMap<EmblemEntry>
    {
        public EmblemEntryMap() { Map(m => m.Id).Name("emblem_id"); }
    }

    private sealed class SleeveEntryMap : ClassMap<SleeveEntry>
    {
        public SleeveEntryMap() { Map(m => m.Id).Name("sleeve_id"); }
    }

    private sealed class DegreeEntryMap : ClassMap<DegreeEntry>
    {
        public DegreeEntryMap() { Map(m => m.Id).Name("degree_id"); }
    }

    private sealed class BattlefieldEntryMap : ClassMap<BattlefieldEntry>
    {
        public BattlefieldEntryMap()
        {
            Map(m => m.Id).Name("value");
            Map(m => m.IsOpen).Name("is_open");
        }
    }

    private sealed class MyPageBackgroundEntryMap : ClassMap<MyPageBackgroundEntry>
    {
        public MyPageBackgroundEntryMap() { Map(m => m.Id).Name("id"); }
    }

    private sealed class ClassExpEntryMap : ClassMap<ClassExpEntry>
    {
        public ClassExpEntryMap()
        {
            Map(m => m.Id).Name("level");
            Map(m => m.NecessaryExp).Name("necessary_exp");
        }
    }

    private sealed class RankInfoEntryMap : ClassMap<RankInfoEntry>
    {
        public RankInfoEntryMap()
        {
            Map(m => m.Id).Name("rank_id");
            Map(m => m.Name).Name("rank_name");
            Map(m => m.NecessaryPoint).Name("necessary_point");
            Map(m => m.AccumulatePoint).Name("accumulate_point");
            Map(m => m.LowerLimitPoint).Name("lower_limit_point");
            Map(m => m.BaseAddBp).Name("base_add_bp");
            Map(m => m.BaseDropBp).Name("base_drop_bp");
            Map(m => m.StreakBonusPt).Name("streak_bonus_pt");
            Map(m => m.WinBonus).Name("win_bonus");
            Map(m => m.LoseBonus).Name("lose_bonus");
            Map(m => m.MaxWinBonus).Name("max_win_bonus");
            Map(m => m.MaxLoseBonus).Name("max_lose_bonus");
            Map(m => m.IsPromotionWar).Name("is_promotion_war");
            Map(m => m.MatchCount).Name("match_count");
            Map(m => m.NecessaryWin).Name("necessary_win");
            Map(m => m.ResetLose).Name("reset_lose");
            Map(m => m.AccumulateMasterPoint).Name("accumulate_master_point");
        }
    }
}
