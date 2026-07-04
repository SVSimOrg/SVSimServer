using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Entities.Story;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

[ApiController]
[Authorize]
public class StoryController : SVSimController
{
    private readonly IStoryService _service;
    private readonly IMissionProgressService _missionProgress;
    public StoryController(IStoryService service, IMissionProgressService missionProgress)
    {
        _service = service;
        _missionProgress = missionProgress;
    }

    [HttpPost("/story/section")]
    [HttpPost("/main_story/section")]
    [HttpPost("/limited_story/section")]
    [HttpPost("/event_story/section")]
    public async Task<ActionResult<SectionResponse>> Section(SectionRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        return await _service.GetSectionsAsync(ResolveApiType(), vid);
    }

    [HttpPost("/main_story/leader_select")]
    [HttpPost("/limited_story/leader_select")]
    [HttpPost("/event_story/leader_select")]
    public async Task<ActionResult<LeaderSelectResponse>> LeaderSelect(LeaderSelectRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        return await _service.GetLeaderSelectAsync(ResolveApiType(), req.SectionId, vid);
    }

    [HttpPost("/main_story/info")]
    [HttpPost("/limited_story/info")]
    [HttpPost("/event_story/info")]
    public async Task<ActionResult<InfoResponse>> Info(InfoRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        int? chara = req.CharaId == 0 ? null : req.CharaId;
        return await _service.GetInfoAsync(ResolveApiType(), req.SectionId, chara, vid);
    }

    [HttpPost("/main_story/get_deck_list")]
    [HttpPost("/event_story/get_deck_list")]
    public async Task<ActionResult<GetDeckListResponse>> GetDeckList(GetDeckListRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        return await _service.GetDeckListAsync(ResolveApiType(), req.StoryId, vid);
    }

    [HttpPost("/main_story/start")]
    [HttpPost("/limited_story/start")]
    [HttpPost("/event_story/start")]
    public async Task<ActionResult<StartResponse>> Start(StartRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        return await _service.StartAsync(ResolveApiType(), req.StoryIds, vid);
    }

    [HttpPost("/main_story/finish")]
    [HttpPost("/limited_story/finish")]
    [HttpPost("/event_story/finish")]
    public async Task<ActionResult<FinishResponse>> Finish(FinishRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        var outcome = await _service.FinishAsync(ResolveApiType(), req, vid);

        // Emit story-chapter-finish events for mission/achievement progress.
        var apiType = ResolveApiType();
        var prefix = apiType switch
        {
            StoryApiType.Main => "main",
            StoryApiType.Limited => "limited",
            StoryApiType.Event => "event",
            _ => null,
        };
        if (prefix is not null && req.StoryId != 0)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.Story.ChapterFinishAll(prefix, req.StoryId));
        }
        if (outcome.LeveledUp && outcome.ClassId.HasValue)
        {
            await _missionProgress.RecordEventAsync(
                vid, MissionEventKeys.ClassLevel.UpAll(outcome.ClassId.Value));
        }

        return outcome.Response;
    }

    [HttpPost("/main_story/all_finish")]
    [HttpPost("/limited_story/all_finish")]
    [HttpPost("/event_story/all_finish")]
    public async Task<ActionResult<FinishResponse>> AllFinish(AllFinishRequest req)
    {
        if (!TryGetViewerId(out long vid)) return Unauthorized();
        return await _service.AllFinishAsync(ResolveApiType(), req.StoryIds, req.IsFinish == 1, vid);
    }

    private StoryApiType ResolveApiType()
    {
        var path = HttpContext.Request.Path.Value ?? "";
        if (path.StartsWith("/main_story"))    return StoryApiType.Main;
        if (path.StartsWith("/limited_story")) return StoryApiType.Limited;
        if (path.StartsWith("/event_story"))   return StoryApiType.Event;
        return StoryApiType.AllStory;   // /story/section
    }
}
