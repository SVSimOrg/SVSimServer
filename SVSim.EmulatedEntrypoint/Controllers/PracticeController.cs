using Microsoft.AspNetCore.Mvc;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Practice;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Deck;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Practice;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class PracticeController : SVSimController
{
    private readonly IGlobalsRepository _globalsRepository;
    private readonly IMissionProgressService _missionProgress;
    private readonly IDeckListBuilder _deckListBuilder;
    private readonly IViewerRepository _viewers;
    private readonly IBattleXpService _xp;
    private readonly SVSimDbContext _db;

    public PracticeController(
        IGlobalsRepository globalsRepository,
        IMissionProgressService missionProgress,
        IDeckListBuilder deckListBuilder,
        IViewerRepository viewers,
        IBattleXpService xp,
        SVSimDbContext db)
    {
        _globalsRepository = globalsRepository;
        _missionProgress = missionProgress;
        _deckListBuilder = deckListBuilder;
        _viewers = viewers;
        _xp = xp;
        _db = db;
    }

    /// <summary>
    /// /practice/info — returns the AI opponent catalog. Response data is a JSON array
    /// directly (not wrapped in an object), per spec. Backed by PracticeOpponents table,
    /// seeded by SVSim.Bootstrap from seeds/practice-opponents.json.
    /// </summary>
    [HttpPost("info")]
    public async Task<List<PracticeOpponent>> Info(BaseRequest request)
    {
        var rows = await _globalsRepository.GetPracticeOpponents();
        return rows.Select(e => new PracticeOpponent
        {
            PracticeId = e.PracticeId,
            TextId = e.TextId,
            ClassId = e.ClassId,
            CharaId = e.CharaId,
            DegreeId = e.DegreeId,
            AiDeckLevel = e.AiDeckLevel,
            AiLogicLevel = e.AiLogicLevel,
            AiMaxLife = e.AiMaxLife,
            Battle3dFieldId = e.Battle3dFieldId,
            IsMaintenance = e.IsMaintenance,
            IsCampaignPractice = e.IsCampaignPractice,
        }).ToList();
    }

    /// <summary>
    /// /practice/deck_list — same wire shape as /deck/info (the client parses both via
    /// DeckGroupListData), so it shares <see cref="IDeckListBuilder"/>. Always All-format per spec.
    /// Unlike /deck/info this is a deck *select* screen, so empty "New Deck" slots are NOT padded
    /// (padEmptySlots: false) — prod's practice capture returns the viewer's real decks unpadded,
    /// plus the 8 per-class default decks and per-class leader-skin settings. The builder loads
    /// decks via IDeckRepository (DeckCard.Card Included), so card_id_array carries real ids rather
    /// than the 40 zeros that NRE the client's SBattleLoad.InitPlayer.
    /// </summary>
    [HttpPost("deck_list")]
    public async Task<ActionResult<DeckListResponse>> DeckList(DeckFormatRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        return await _deckListBuilder.BuildAsync(viewerId, Format.All, padEmptySlots: false);
    }

    /// <summary>
    /// /practice/start — server is essentially a no-op for practice. Spec: empty body
    /// response is fine; client tolerates missing mission_parameter.
    /// </summary>
    [HttpPost("start")]
    public Task<PracticeStartResponse> Start(BaseRequest request)
    {
        return Task.FromResult(new PracticeStartResponse());
    }

    /// <summary>
    /// /practice/finish — accept the recovery_data blob without validation; grant class XP
    /// (win or loss) via <see cref="IBattleXpService"/> and return the post-state totals.
    /// </summary>
    [HttpPost("finish")]
    public async Task<PracticeFinishResponse> Finish(PracticeFinishRequest request)
    {
        bool isWin = request.IsWin == 1;

        if (!TryGetViewerId(out long viewerId))
        {
            return new PracticeFinishResponse
            {
                ClassLevel = 1,
                AchievedInfo = new Dictionary<string, object>(),
                RewardList = new List<Models.Dtos.Common.Reward>(),
            };
        }

        // Mission/achievement progress hook. Wire values (difficulty int, enemy_class_id int)
        // are mapped to catalog-facing names by MissionEventKeys.Practice — the catalog
        // authors keys like "practice_win:elite:arisa", so the emitter must match. Wire
        // difficulties outside 4/6/7 (elite/elite2/elite3) only advance the top-level counter.
        if (isWin)
        {
            await _missionProgress.RecordEventAsync(
                viewerId,
                MissionEventKeys.Practice.WinAll(request.Difficulty, request.EnemyClassId));
        }

        int gainXp = 0, totalXp = 0, level = 1;
        bool leveledUp = false;
        var viewer = await _viewers.LoadForBattleXpGrantAsync(viewerId);
        if (viewer is not null)
        {
            var xp = await _xp.GrantAsync(viewer, request.ClassId, isWin, BattleXpMode.Practice);
            await _db.SaveChangesAsync();
            gainXp = xp.GetXp;
            totalXp = xp.TotalXp;
            level = xp.Level == 0 ? 1 : xp.Level;
            leveledUp = xp.LeveledUp;
        }

        if (leveledUp)
        {
            await _missionProgress.RecordEventAsync(
                viewerId, MissionEventKeys.ClassLevel.UpAll(request.ClassId));
        }

        return new PracticeFinishResponse
        {
            GetClassExperience = gainXp,
            ClassExperience = totalXp,
            ClassLevel = level,
            AchievedInfo = new Dictionary<string, object>(),
            RewardList = new List<Models.Dtos.Common.Reward>()
        };
    }
}
