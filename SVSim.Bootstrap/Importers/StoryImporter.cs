using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Reads worlds.json, sections.json, chapters.json, special-battle-settings.json from a story
/// data directory and upserts the corresponding entities. Idempotent. FK ordering: SBS → Worlds
/// → Sections → Chapters (with owned collections cascading).
/// </summary>
public class StoryImporter
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public async Task ImportAsync(SVSimDbContext context, string storyDataDir)
    {
        string worldsPath   = Path.Combine(storyDataDir, "importer-worlds.json");
        string sectionsPath = Path.Combine(storyDataDir, "importer-sections.json");
        string chaptersPath = Path.Combine(storyDataDir, "importer-chapters.json");
        string sbsPath      = Path.Combine(storyDataDir, "importer-sbs.json");

        // Fallback to production filenames when fixture-prefixed names aren't present.
        if (!File.Exists(worldsPath))   worldsPath   = Path.Combine(storyDataDir, "worlds.json");
        if (!File.Exists(sectionsPath)) sectionsPath = Path.Combine(storyDataDir, "sections.json");
        if (!File.Exists(chaptersPath)) chaptersPath = Path.Combine(storyDataDir, "chapters.json");
        if (!File.Exists(sbsPath))      sbsPath      = Path.Combine(storyDataDir, "special-battle-settings.json");

        if (!File.Exists(chaptersPath))
        {
            Console.Error.WriteLine($"[Story] chapters.json not found at {chaptersPath}; skipping story import.");
            return;
        }

        var inputSbs      = await ReadOrEmptyAsync<List<SbsInput>>(sbsPath);
        var inputWorlds   = await ReadOrEmptyAsync<List<WorldInput>>(worldsPath);
        var inputSections = await ReadOrEmptyAsync<List<SectionInput>>(sectionsPath);
        var inputChapters = await ReadOrEmptyAsync<List<ChapterInput>>(chaptersPath);

        Console.WriteLine($"[Story] Parsed {inputWorlds.Count} worlds, {inputSections.Count} sections, " +
                          $"{inputChapters.Count} chapters, {inputSbs.Count} sbs payloads.");

        int sbsCreated = 0, sbsUpdated = 0;
        var existingSbs = await context.SpecialBattleSettings.ToDictionaryAsync(x => x.Id);
        foreach (var s in inputSbs)
        {
            if (existingSbs.TryGetValue(s.Id, out var row))
            {
                Apply(row, s); sbsUpdated++;
            }
            else
            {
                context.SpecialBattleSettings.Add(ToEntity(s)); sbsCreated++;
            }
        }

        int wCreated = 0, wUpdated = 0;
        var existingWorlds = await context.StoryWorlds.ToDictionaryAsync(x => x.Id);
        foreach (var w in inputWorlds)
        {
            if (existingWorlds.TryGetValue(w.Id, out var row))
            {
                row.TitleTextKey = w.TitleTextKey; row.PanelImageName = w.PanelImageName; row.RibbonText = w.RibbonText;
                wUpdated++;
            }
            else
            {
                context.StoryWorlds.Add(new StoryWorld {
                    Id = w.Id, TitleTextKey = w.TitleTextKey,
                    PanelImageName = w.PanelImageName, RibbonText = w.RibbonText });
                wCreated++;
            }
        }

        int secCreated = 0, secUpdated = 0;
        var existingSections = await context.StorySections.ToDictionaryAsync(x => x.Id);
        foreach (var s in inputSections)
        {
            if (existingSections.TryGetValue(s.Id, out var row)) { Apply(row, s); secUpdated++; }
            else { context.StorySections.Add(ToEntity(s)); secCreated++; }
        }

        int chCreated = 0, chUpdated = 0;
        var existingChapters = await context.StoryChapters
            .Include(c => c.BattleSettings).Include(c => c.Rewards).Include(c => c.SubChapters)
            .ToDictionaryAsync(x => x.StoryId);
        foreach (var c in inputChapters)
        {
            if (existingChapters.TryGetValue(c.StoryId, out var row)) { Apply(row, c); chUpdated++; }
            else { context.StoryChapters.Add(ToEntity(c)); chCreated++; }
        }

        Console.WriteLine($"[Story] Saving: worlds +{wCreated}/~{wUpdated}, sections +{secCreated}/~{secUpdated}, " +
                          $"chapters +{chCreated}/~{chUpdated}, sbs +{sbsCreated}/~{sbsUpdated}...");
        await context.SaveChangesAsync();
        Console.WriteLine("[Story] Done.");
    }

    private static async Task<T> ReadOrEmptyAsync<T>(string path) where T : new()
    {
        if (!File.Exists(path)) return new T();
        await using var fs = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(fs, JsonOpts) ?? new T();
    }

    // --- mapping helpers ---

    private static SpecialBattleSetting ToEntity(SbsInput s) => Apply(new SpecialBattleSetting { Id = s.Id }, s);
    private static SpecialBattleSetting Apply(SpecialBattleSetting row, SbsInput s)
    {
        row.PlayerFirstTurn = s.PlayerFirstTurn;
        row.PlayerStartPp = s.PlayerStartPp; row.EnemyStartPp = s.EnemyStartPp;
        row.PlayerStartLife = s.PlayerStartLife; row.EnemyStartLife = s.EnemyStartLife;
        row.PlayerAttachSkill = s.PlayerAttachSkill ?? ""; row.EnemyAttachSkill = s.EnemyAttachSkill ?? "";
        row.IdOverrideInBattleLog = s.IdOverrideInBattleLog ?? "";
        row.BanishEffectOverride = s.BanishEffectOverride ?? "";
        row.TokenDrawEffectOverride = s.TokenDrawEffectOverride ?? "";
        row.SpecialTokenDrawEffectOverride = s.SpecialTokenDrawEffectOverride ?? "";
        row.ResultSkip = s.ResultSkip;
        row.VsEffectOverride = s.VsEffectOverride;
        row.ClassDestroyEffectOverride = s.ClassDestroyEffectOverride;
        row.Note = s.Note;
        return row;
    }

    private static StorySection ToEntity(SectionInput s) => Apply(new StorySection { Id = s.Id }, s);
    private static StorySection Apply(StorySection row, SectionInput s)
    {
        row.WorldId = s.WorldId;
        row.StoryApiType = Enum.Parse<StoryApiType>(s.StoryApiType ?? "Main");
        row.OrderId = s.OrderId; row.AllStoryOrderId = s.AllStoryOrderId;
        row.NameTextKey = s.NameTextKey ?? ""; row.ImageName = s.ImageName ?? "";
        row.IsLeaderSelect = s.IsLeaderSelect; row.BackGroundId = s.BackGroundId;
        row.ChapterSelectType = s.ChapterSelectType; row.StoryTypeOverwrite = s.StoryTypeOverwrite;
        row.IsUnderMaintenance = s.IsUnderMaintenance;
        row.IsPlayAnotherEndAppearanceAnimation = s.IsPlayAnotherEndAppearanceAnimation;
        row.IsSpoiler = s.IsSpoiler;
        row.SpoilerMessage = s.SpoilerMessage ?? string.Empty;
        return row;
    }

    private static StoryChapter ToEntity(ChapterInput c) => Apply(new StoryChapter { StoryId = c.StoryId }, c);
    private static StoryChapter Apply(StoryChapter row, ChapterInput c)
    {
        row.SectionId = c.SectionId; row.CharaId = c.CharaId;
        row.ChapterId = c.ChapterId ?? ""; row.NextChapterId = c.NextChapterId ?? "";
        row.RequiredChapterId = c.RequiredChapterId;
        row.SelectionDisplayPosition = c.SelectionDisplayPosition;
        row.SelectionTextId = c.SelectionTextId;
        row.ShowCoordinate = c.ShowCoordinate;
        row.XCoordinate = (decimal)c.XCoordinate; row.YCoordinate = (decimal)c.YCoordinate;
        row.IsCameraMovable = c.IsCameraMovable; row.ShowSubtitles = c.ShowSubtitles;
        row.BattleExists = c.BattleExists;
        row.EnemyCharaId = c.EnemyCharaId; row.EnemyClass = c.EnemyClass; row.EnemyAiId = c.EnemyAiId;
        row.BgFileName = c.BgFileName ?? "";
        row.ChapterEffectPath = c.ChapterEffectPath; row.ChapterClearTextId = c.ChapterClearTextId;
        row.Battle3dFieldId = c.Battle3dFieldId; row.BgmId = c.BgmId ?? "";
        row.SpecialBattleSettingId = c.SpecialBattleSettingId;
        row.ReleasePoint = c.ReleasePoint; row.UnlockText = c.UnlockText; row.IsMaintenanceChapter = c.IsMaintenanceChapter;
        row.IsPlayAnotherEndAppearanceAnimation = c.IsPlayAnotherEndAppearanceAnimation;
        row.IsReleasedAnotherEnd = c.IsReleasedAnotherEnd;
        row.IsSkipEnabled = c.IsSkipEnabled;

        // Owned collections: clear + replace, EF tracks the deletes.
        row.BattleSettings.Clear();
        foreach (var b in c.BattleSettings ?? new())
            row.BattleSettings.Add(new StoryChapterBattleSetting
            {
                DeckClassId = b.DeckClassId,
                PlayerEmotionOverride = b.PlayerEmotionOverride,
                EnemyEmotionOverride = b.EnemyEmotionOverride,
                SkinIdOverride = b.SkinIdOverride,
                Battle3dFieldIdOverride = b.Battle3dFieldIdOverride,
                BgmIdOverride = b.BgmIdOverride,
                DeckSkinIdOverride = b.DeckSkinIdOverride,
            });

        row.Rewards.Clear();
        foreach (var r in c.StoryReward ?? new())
            row.Rewards.Add(new StoryChapterReward
            {
                RewardType = (UserGoodsType)r.RewardType,
                RewardDetailId = r.RewardDetailId,
                RewardNumber = r.RewardNumber,
            });

        row.SubChapters.Clear();
        foreach (var sc in c.SubChapters ?? new())
            row.SubChapters.Add(new StorySubChapter
            {
                SubChapterId = sc.SubChapterId,
                SubChapterStoryId = sc.SubChapterStoryId,
                IsMaintenanceChapter = sc.IsMaintenanceChapter,
            });
        return row;
    }

    // --- input shapes (snake_case via JsonOpts) ---

    private class SbsInput
    {
        public int Id { get; set; }
        public int PlayerFirstTurn { get; set; }
        public int PlayerStartPp { get; set; } public int EnemyStartPp { get; set; }
        public int PlayerStartLife { get; set; } public int EnemyStartLife { get; set; }
        public string? PlayerAttachSkill { get; set; } public string? EnemyAttachSkill { get; set; }
        public string? IdOverrideInBattleLog { get; set; }
        public string? BanishEffectOverride { get; set; }
        public string? TokenDrawEffectOverride { get; set; }
        public string? SpecialTokenDrawEffectOverride { get; set; }
        public int ResultSkip { get; set; } public int VsEffectOverride { get; set; }
        public int ClassDestroyEffectOverride { get; set; }
        public string? Note { get; set; }
    }
    private class WorldInput
    {
        public int Id { get; set; }
        public string TitleTextKey { get; set; } = "";
        public string PanelImageName { get; set; } = "";
        public string RibbonText { get; set; } = "";
    }
    private class SectionInput
    {
        public int Id { get; set; } public int? WorldId { get; set; }
        public string? StoryApiType { get; set; }
        public int OrderId { get; set; } public int AllStoryOrderId { get; set; }
        public string? NameTextKey { get; set; } public string? ImageName { get; set; }
        public bool IsLeaderSelect { get; set; } public int BackGroundId { get; set; }
        public int ChapterSelectType { get; set; } public int StoryTypeOverwrite { get; set; }
        public bool IsUnderMaintenance { get; set; }
        public bool IsPlayAnotherEndAppearanceAnimation { get; set; }
        public int IsSpoiler { get; set; }
        public string? SpoilerMessage { get; set; }
    }
    private class ChapterInput
    {
        public int StoryId { get; set; } public int SectionId { get; set; } public int CharaId { get; set; }
        public string? ChapterId { get; set; } public string? NextChapterId { get; set; }
        public string? RequiredChapterId { get; set; }
        public string? SelectionDisplayPosition { get; set; } public string? SelectionTextId { get; set; }
        public int ShowCoordinate { get; set; }
        public double XCoordinate { get; set; } public double YCoordinate { get; set; }
        public int IsCameraMovable { get; set; } public int ShowSubtitles { get; set; }
        public bool BattleExists { get; set; } public int EnemyCharaId { get; set; }
        public int EnemyClass { get; set; } public int EnemyAiId { get; set; }
        public string? BgFileName { get; set; } public string? ChapterEffectPath { get; set; }
        public string? ChapterClearTextId { get; set; }
        [JsonPropertyName("battle3dfield_id")]
        public int Battle3dFieldId { get; set; }
        public string? BgmId { get; set; }
        public int? SpecialBattleSettingId { get; set; }
        public int ReleasePoint { get; set; } public string? UnlockText { get; set; }
        public bool IsMaintenanceChapter { get; set; }
        public bool IsPlayAnotherEndAppearanceAnimation { get; set; }
        public bool IsReleasedAnotherEnd { get; set; } public bool IsSkipEnabled { get; set; }
        public List<BattleSettingInput>? BattleSettings { get; set; }
        public List<RewardInput>? StoryReward { get; set; }
        public List<SubChapterInput>? SubChapters { get; set; }
    }
    private class BattleSettingInput {
        public int DeckClassId { get; set; }
        public int PlayerEmotionOverride { get; set; } public int EnemyEmotionOverride { get; set; }
        public int SkinIdOverride { get; set; }
        [JsonPropertyName("battle3dfield_id_override")]
        public int Battle3dFieldIdOverride { get; set; }
        public int BgmIdOverride { get; set; } public int DeckSkinIdOverride { get; set; }
    }
    private class RewardInput {
        public int RewardType { get; set; } public long RewardDetailId { get; set; } public int RewardNumber { get; set; }
    }
    private class SubChapterInput {
        public int SubChapterId { get; set; } public int SubChapterStoryId { get; set; }
        public bool IsMaintenanceChapter { get; set; }
    }
}
