using SVSim.Database.Entities.Story;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IStoryService
{
    Task<SectionResponse> GetSectionsAsync(StoryApiType apiType, long viewerId);
    Task<LeaderSelectResponse> GetLeaderSelectAsync(StoryApiType apiType, int sectionId, long viewerId);
    Task<InfoResponse> GetInfoAsync(StoryApiType apiType, int sectionId, int? charaId, long viewerId);
    Task<GetDeckListResponse> GetDeckListAsync(StoryApiType apiType, int storyId, long viewerId);
    Task<StartResponse> StartAsync(StoryApiType apiType, int[] storyIds, long viewerId);
    Task<StoryFinishOutcome> FinishAsync(StoryApiType apiType, FinishRequest req, long viewerId);
    Task<FinishResponse> AllFinishAsync(StoryApiType apiType, int[] storyIds, bool isFinish, long viewerId);
}
