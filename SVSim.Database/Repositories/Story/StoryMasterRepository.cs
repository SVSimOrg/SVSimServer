using Microsoft.EntityFrameworkCore;
using SVSim.Database.Entities.Story;

namespace SVSim.Database.Repositories.Story;

public class StoryMasterRepository : IStoryMasterRepository
{
    private readonly SVSimDbContext _db;
    public StoryMasterRepository(SVSimDbContext db) { _db = db; }

    public Task<List<StorySection>> GetSectionsByFamilyAsync(StoryApiType apiType)
    {
        // /story/section (AllStory) returns BOTH Main and Limited sections — they share world_list
        // entries in prod. The Story Mode UI's "Special" submenu in world 1 is populated by the
        // limited-story sections (section_id >= 9000, story_type_overwrite=2) appearing alongside
        // the Prologue (section 0). Event-story sections never appear here — they live behind their
        // own menu and are fetched via /event_story/section.
        var families = apiType == StoryApiType.AllStory
            ? new[] { StoryApiType.Main, StoryApiType.Limited }
            : new[] { apiType };
        return _db.StorySections.Where(s => families.Contains(s.StoryApiType))
                                .OrderBy(s => s.AllStoryOrderId)
                                .ToListAsync();
    }

    public Task<List<StoryWorld>> GetWorldsForSectionsAsync(IEnumerable<int> worldIds)
        => _db.StoryWorlds.Where(w => worldIds.Contains(w.Id)).ToListAsync();

    public Task<List<StoryChapter>> GetChaptersBySectionCharaAsync(int sectionId, int charaId)
        => _db.StoryChapters
              .AsSplitQuery()
              .Include(c => c.BattleSettings).Include(c => c.Rewards).Include(c => c.SubChapters)
              .Where(c => c.SectionId == sectionId && c.CharaId == charaId)
              .ToListAsync();

    // No Includes — the rollup only reads SectionId/CharaId/StoryId. Including the three owned
    // collections here would cartesian-explode across ~677 chapters and turn a single query into
    // a multi-MB result set.
    public Task<List<StoryChapter>> GetChaptersBySectionsAsync(IEnumerable<int> sectionIds)
    {
        var ids = sectionIds.ToList();
        return _db.StoryChapters
                  .AsNoTracking()
                  .Where(c => ids.Contains(c.SectionId))
                  .ToListAsync();
    }

    public Task<StoryChapter?> GetChapterByIdAsync(int storyId)
        => _db.StoryChapters
              .AsSplitQuery()
              .Include(c => c.BattleSettings).Include(c => c.Rewards).Include(c => c.SubChapters)
              .FirstOrDefaultAsync(c => c.StoryId == storyId);

    public Task<SpecialBattleSetting?> GetSbsByIdAsync(int sbsId)
        => _db.SpecialBattleSettings.FirstOrDefaultAsync(s => s.Id == sbsId);

    public async Task<StorySubChapter?> FindSubChapterByStoryIdAsync(int storyId)
    {
        // StorySubChapter is an owned entity (no DbSet of its own); query through the owning
        // chapter. SelectMany over the owned collection translates to a JOIN in the relational
        // provider — no need to materialize the full chapter row.
        return await _db.StoryChapters
            .AsNoTracking()
            .SelectMany(c => c.SubChapters)
            .FirstOrDefaultAsync(sc => sc.SubChapterStoryId == storyId);
    }
}
