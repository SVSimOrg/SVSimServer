using SVSim.Database.Entities.Story;

namespace SVSim.Database.Repositories.Story;

public interface IStoryMasterRepository
{
    Task<List<StorySection>> GetSectionsByFamilyAsync(StoryApiType apiType);
    Task<List<StoryWorld>> GetWorldsForSectionsAsync(IEnumerable<int> worldIds);
    Task<List<StoryChapter>> GetChaptersBySectionCharaAsync(int sectionId, int charaId);

    /// <summary>
    /// Bulk-load chapter scalars (no owned collections) across multiple sections in one round-trip.
    /// Used by the section rollup to avoid N+1 per (section, chara) lookups.
    /// </summary>
    Task<List<StoryChapter>> GetChaptersBySectionsAsync(IEnumerable<int> sectionIds);

    Task<StoryChapter?> GetChapterByIdAsync(int storyId);
    Task<SpecialBattleSetting?> GetSbsByIdAsync(int sbsId);

    /// <summary>
    /// Resolve a wire story_id to a sub-chapter row when no top-level <see cref="StoryChapter"/>
    /// exists for it. Sub-chapter story_ids have no chapter master data of their own — they're
    /// progress markers hanging off the parent. Used by /finish to record progress at the sub's
    /// story_id when the client sends sub-chapter ids directly.
    /// </summary>
    Task<StorySubChapter?> FindSubChapterByStoryIdAsync(int storyId);
}
