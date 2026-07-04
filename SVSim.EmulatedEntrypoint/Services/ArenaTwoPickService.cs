using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Services;

public class ArenaTwoPickService : IArenaTwoPickService
{
    private readonly IArenaTwoPickRunRepository _runs;
    private readonly IArenaTwoPickRewardRepository _rewards;
    private readonly IArenaTwoPickCardPoolService _pool;
    private readonly IGameConfigService _config;
    private readonly IViewerRepository _viewers;
    private readonly IInventoryService _inv;
    private readonly IBattleXpService _xp;
    private readonly IRandom _rng;
    private readonly SVSimDbContext _db;

    public ArenaTwoPickService(
        IArenaTwoPickRunRepository runs,
        IArenaTwoPickRewardRepository rewards,
        IArenaTwoPickCardPoolService pool,
        IGameConfigService config,
        IViewerRepository viewers,
        IInventoryService inv,
        IBattleXpService xp,
        IRandom rng,
        SVSimDbContext db)
    {
        _runs = runs; _rewards = rewards; _pool = pool; _config = config;
        _viewers = viewers; _inv = inv; _xp = xp; _rng = rng; _db = db;
    }

    public async Task<TopResponseDto> GetTopAsync(long viewerId)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId);
        if (run is null) return new TopResponseDto { EntryInfo = null };

        var dto = new TopResponseDto
        {
            EntryInfo = ProjectEntryInfo(run, viewerId),
            BattleResults = ProjectBattleResults(run),
        };
        if (run.ClassId != 0)
        {
            dto.ClassInfo = ProjectClassInfo(run);
            dto.DeckInfo = ProjectDeckInfo(run);
            if (run.WinCount > 0 || run.LossCount > 0)
                dto.LeaderSkinId = run.LeaderSkinId;
        }
        return dto;
    }

    public async Task<EntryResponseDto> EntryAsync(long viewerId, int consumeItemType)
    {
        if (await _runs.GetByViewerIdAsync(viewerId) is not null)
            throw new ArenaTwoPickException("arena_two_pick_already_in_progress");

        var aCfg = _config.Get<SVSim.Database.Models.Config.ArenaTwoPickConfig>();

        // Open inventory tx for currency/item debit.
        await using var tx = await _inv.BeginAsync(viewerId);

        // Dispatch on the client's chosen payment method (ArenaData.eARENA_PAY).
        RewardEntryDto? feeEntry = consumeItemType switch
        {
            1 => await DebitCrystalsAsync(tx, aCfg.CrystalCost),
            3 => await DebitTicketAsync(tx, aCfg.TicketItemId, aCfg.TicketCost),
            4 => await DebitRupiesAsync(tx, aCfg.RupyCost),
            5 => null, // Free entry — no fee.
            _ => throw new ArenaTwoPickException("invalid_consume_item_type"),
        };

        var maxWins = await ResolveMaxBattleCountAsync();
        var candidates = SampleCandidateClasses(aCfg.AllowedClassIds, _rng);

        var run = new ViewerArenaTwoPickRun
        {
            ViewerId = viewerId,
            EntryId = 0,
            RewardScheduleId = aCfg.RewardScheduleId,
            ChallengeId = aCfg.ChallengeId,
            MaxBattleCount = maxWins,
            ClassId = 0,
            LeaderSkinId = 0,
            CandidateClassIdsJson = JsonSerializer.Serialize(candidates),
            SelectTurn = 0,
            IsSelectCompleted = false,
            SelectedCardIdsJson = "[]",
            PendingPickSetsJson = "[]",
            NextCandidateId = 1,
            ResultListJson = "[]",
            WinCount = 0,
            LossCount = 0,
            IsRetire = false,
        };
        await _runs.UpsertAsync(run);
        // Save to get auto-generated Id before CommitAsync.
        await _db.SaveChangesAsync();
        run.EntryId = run.Id;
        await _runs.UpsertAsync(run);
        // CommitAsync saves all pending changes (including run update) and commits the db tx.
        await tx.CommitAsync();

        var rewardList = feeEntry is null ? new List<RewardEntryDto>() : new List<RewardEntryDto> { feeEntry };

        return new EntryResponseDto
        {
            EntryInfo = ProjectEntryInfo(run, viewerId),
            RewardList = rewardList,
            CandidateClassIds = candidates,
            BattleResults = new BattleResultsDto { WinCount = 0, ResultList = new List<int>() },
        };
    }

    private async Task<RewardEntryDto> DebitTicketAsync(IInventoryTransaction tx, int ticketItemId, int ticketCost)
    {
        if (tx.IsFreeplay)
        {
            var ticket = tx.Viewer.Items.FirstOrDefault(i => i.Item.Id == ticketItemId);
            return new RewardEntryDto
            {
                RewardType = (int)UserGoodsType.Item,
                RewardId = ticketItemId,
                RewardNum = ticket?.Count ?? 0,
            };
        }
        var debitResult = await tx.TryDebitAsync(UserGoodsType.Item, ticketItemId, ticketCost);
        if (!debitResult.Success)
            throw new ArenaTwoPickException("insufficient_ticket");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Item,
            RewardId = ticketItemId,
            RewardNum = (int)debitResult.PostStateTotal,
        };
    }

    private async Task<RewardEntryDto> DebitCrystalsAsync(IInventoryTransaction tx, int cost)
    {
        var result = await tx.TrySpendAsync(SpendCurrency.Crystal, cost);
        if (!result.Success)
            throw new ArenaTwoPickException("insufficient_crystal");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Crystal,
            RewardId = 0,
            RewardNum = (int)result.PostStateTotal,
        };
    }

    private async Task<RewardEntryDto> DebitRupiesAsync(IInventoryTransaction tx, int cost)
    {
        var result = await tx.TrySpendAsync(SpendCurrency.Rupee, cost);
        if (!result.Success)
            throw new ArenaTwoPickException("insufficient_rupy");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Rupy,
            RewardId = 0,
            RewardNum = (int)result.PostStateTotal,
        };
    }

    private async Task<int> ResolveMaxBattleCountAsync()
    {
        var rawMaxWins = await _rewards.GetMaxWinCountAsync();
        if (rawMaxWins == 0)
        {
            Console.Error.WriteLine("[ArenaTwoPickService] ArenaTwoPickRewards catalog empty; defaulting MaxBattleCount=5. Run SVSim.Bootstrap to seed.");
            return 5;
        }
        return rawMaxWins;
    }

    private static List<int> SampleCandidateClasses(List<int> allowed, IRandom rng)
    {
        if (allowed.Count < 3)
            throw new InvalidOperationException("ArenaTwoPickConfig.AllowedClassIds needs ≥3 entries");
        var shuffled = allowed.OrderBy(_ => rng.Next(int.MaxValue)).ToList();
        return shuffled.Take(3).ToList();
    }

    private async Task<SVSim.Database.Models.Viewer> LoadViewerForGrantsAsync(long viewerId)
    {
        return await _db.Viewers
            .Include(v => v.Currency)
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .Include(v => v.Cards)
            .Include(v => v.Sleeves)
            .Include(v => v.Emblems)
            .Include(v => v.Degrees)
            .Include(v => v.LeaderSkins)
            .Include(v => v.MyPageBackgrounds)
            .Include(v => v.Classes).ThenInclude(c => c.Class)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId);
    }
    public async Task<ClassChooseResponseDto> ChooseClassAsync(long viewerId, int classId)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");
        if (run.ClassId != 0)
            throw new ArenaTwoPickException("arena_two_pick_invalid_state");
        var candidates = JsonSerializer.Deserialize<List<int>>(run.CandidateClassIdsJson) ?? new();
        if (!candidates.Contains(classId))
            throw new ArenaTwoPickException("arena_two_pick_class_not_offered");

        run.ClassId = classId;
        run.LeaderSkinId = ResolveClassDefaultLeaderSkin(classId);
        var pairs = _pool.GeneratePickSetsForTurn(classId, turn: 1, startingPairId: run.NextCandidateId, _rng);
        run.NextCandidateId += pairs.Count;
        run.SelectTurn = 1;
        run.PendingPickSetsJson = JsonSerializer.Serialize(pairs);
        await _runs.UpsertAsync(run);

        return new ClassChooseResponseDto
        {
            ClassInfo = ProjectClassInfo(run),
            DeckInfo = ProjectDeckInfo(run),
            CandidateCardList = pairs.Select(p => new CandidatePairDto
            {
                Id = p.Id, Turn = p.Turn, SetNum = p.SetNum,
                CardId1 = p.CardId1, CardId2 = p.CardId2,
                IsSelected = p.IsSelected ? 1 : 0,
            }).ToList(),
        };
    }

    // Placeholder: class default skin = class id. Matches the capture's "leader_skin_id":"1" when class_id=1.
    private static long ResolveClassDefaultLeaderSkin(int classId) => classId;

    public async Task<CardChooseResponseDto> ChooseCardAsync(long viewerId, long selectedId)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");
        if (run.ClassId == 0 || run.IsSelectCompleted)
            throw new ArenaTwoPickException("arena_two_pick_invalid_state");

        var pending = JsonSerializer.Deserialize<List<CandidatePair>>(run.PendingPickSetsJson) ?? new();
        var pick = pending.FirstOrDefault(p => p.Id == selectedId)
            ?? throw new ArenaTwoPickException("arena_two_pick_invalid_selection");

        var selectedCards = JsonSerializer.Deserialize<List<long>>(run.SelectedCardIdsJson) ?? new();
        selectedCards.Add(pick.CardId1);
        selectedCards.Add(pick.CardId2);
        run.SelectedCardIdsJson = JsonSerializer.Serialize(selectedCards);

        List<CandidatePair>? nextPairs = null;
        if (run.SelectTurn < 15)
        {
            run.SelectTurn += 1;
            nextPairs = _pool.GeneratePickSetsForTurn(run.ClassId, run.SelectTurn, run.NextCandidateId, _rng);
            run.NextCandidateId += nextPairs.Count;
            run.PendingPickSetsJson = JsonSerializer.Serialize(nextPairs);
        }
        else
        {
            run.IsSelectCompleted = true;
            run.PendingPickSetsJson = "[]";
        }
        await _runs.UpsertAsync(run);

        return new CardChooseResponseDto
        {
            DeckInfo = ProjectDeckInfo(run),
            CandidateCardList = nextPairs?.Select(p => new CandidatePairDto
            {
                Id = p.Id, Turn = p.Turn, SetNum = p.SetNum,
                CardId1 = p.CardId1, CardId2 = p.CardId2,
                IsSelected = p.IsSelected ? 1 : 0,
            }).ToList(),
        };
    }
    public async Task<FinishResponseDto> RetireAsync(long viewerId)
    {
        var (response, _) = await GrantRunRewardsAndDeleteAsync(viewerId, requireComplete: false);
        return response;
    }

    public async Task<RunFinishOutcome> FinishAsync(long viewerId)
    {
        var (response, winCount) = await GrantRunRewardsAndDeleteAsync(viewerId, requireComplete: true);
        // TK2 is 5 battles per run; full clear == 5 wins == 0 losses. Reference the reward
        // catalog's max WinCount rather than a magic 5 so a future rules-config change tracks.
        int maxBattles = await ResolveMaxBattleCountAsync();
        return new RunFinishOutcome(response, WasFullClear: winCount >= maxBattles);
    }

    /// <summary>
    /// Grants rewards for the given run tier and deletes the run row. Returns the wire
    /// response DTO plus the run's final WinCount — the latter feeds
    /// <see cref="FinishAsync"/>'s full-clear detection but isn't exposed on the wire.
    /// </summary>
    private async Task<(FinishResponseDto Response, int WinCount)> GrantRunRewardsAndDeleteAsync(long viewerId, bool requireComplete)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");

        // Classic SV Take Two: run ends after MaxBattles total games played, regardless of the
        // win/loss split. No separate loss cap (Worlds Beyond's 2-loss rule does not apply here).
        // MaxBattles is derived from MAX(reward.WinCount), which is 5 for the live TK2 catalog.
        var maxBattles = await ResolveMaxBattleCountAsync();
        int battlesPlayed = run.WinCount + run.LossCount;
        bool runOver = battlesPlayed >= maxBattles;
        if (requireComplete && !runOver)
            throw new ArenaTwoPickException("arena_two_pick_run_not_complete");

        var rewardRows = await _rewards.GetRewardsByWinCountAsync(run.WinCount);

        // Pre-load item_type for any Item-typed reward so we can populate it on the
        // per-grant delta entries. Currencies don't need a lookup (item_type stays 0).
        var itemRewardIds = rewardRows
            .Where(r => r.RewardType == UserGoodsType.Item)
            .Select(r => (int)r.RewardId)
            .Distinct()
            .ToList();
        var itemTypeById = itemRewardIds.Count == 0
            ? new Dictionary<int, int>()
            : await _db.Items.Where(i => itemRewardIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i.Type);

        var deltas = new List<TwoPickRewardReceivedDto>();

        // Open inventory tx for grants.
        await using var tx = await _inv.BeginAsync(viewerId, configure: cfg => cfg.Source = GrantSource.ArenaTwoPickFinish);

        // Group by RewardGroup, weighted-pick one row per group (Weight==0 excluded).
        foreach (var group in rewardRows.GroupBy(r => r.RewardGroup))
        {
            var pickable = group.Where(r => r.Weight > 0).ToList();
            if (pickable.Count == 0) continue;
            var pick = WeightedPick(pickable, _rng);

            // Skip when the rolled outcome is "nothing" (RewardNum == 0).
            if (pick.RewardNum <= 0) continue;

            await tx.GrantAsync(pick.RewardType, pick.RewardId, pick.RewardNum);
            deltas.Add(new TwoPickRewardReceivedDto
            {
                RewardType     = (int)pick.RewardType,
                RewardDetailId = pick.RewardId,
                RewardCount    = pick.RewardNum,
                ItemType       = itemTypeById.TryGetValue((int)pick.RewardId, out var t) ? t : 0,
                IsUsable       = true,
            });
        }

        var result = await tx.CommitAsync();

        var postStates = result.RewardList
            .Select(g => new RewardEntryDto { RewardType = (int)g.RewardType, RewardId = g.RewardId, RewardNum = g.RewardNum })
            .ToList();

        int winCountAtFinish = run.WinCount;
        await _runs.DeleteAsync(viewerId);
        return (new FinishResponseDto { Rewards = deltas, RewardList = postStates }, winCountAtFinish);
    }

    private static SVSim.Database.Models.ArenaTwoPickReward WeightedPick(
        List<SVSim.Database.Models.ArenaTwoPickReward> rows, IRandom rng)
    {
        long total = rows.Sum(r => (long)r.Weight);
        long roll = rng.Next((int)Math.Min(total, int.MaxValue));
        long cum = 0;
        foreach (var r in rows)
        {
            cum += r.Weight;
            if (roll < cum) return r;
        }
        return rows[^1];
    }

    public async Task<BattleFinishResultDto> RecordBattleResultAsync(long viewerId, bool isWin)
    {
        var run = await _runs.GetByViewerIdAsync(viewerId)
            ?? throw new ArenaTwoPickException("arena_two_pick_no_active_run");

        var aCfg = _config.Get<SVSim.Database.Models.Config.ArenaTwoPickConfig>();
        var results = JsonSerializer.Deserialize<List<bool>>(run.ResultListJson) ?? new();
        results.Add(isWin);
        run.ResultListJson = JsonSerializer.Serialize(results);
        if (isWin) run.WinCount += 1; else run.LossCount += 1;
        await _runs.UpsertAsync(run);

        var viewer = await LoadViewerForGrantsAsync(viewerId);
        int before = (int)(viewer.Currency?.SpotPoints ?? 0);

        var xp = await _xp.GrantAsync(viewer, run.ClassId, isWin, BattleXpMode.ArenaTwoPick);

        viewer.Currency!.SpotPoints += (ulong)aCfg.SpotPointsPerBattle;
        int after = (int)viewer.Currency.SpotPoints;
        await _db.SaveChangesAsync();

        return new BattleFinishResultDto
        {
            BattleResult = isWin ? 1 : 0,
            GetClassExperience = xp.GetXp,
            ClassExperience = xp.TotalXp,
            ClassLevel = xp.Level,
            BeforeSpotPoint = before,
            AddSpotPoint = aCfg.SpotPointsPerBattle,
            AfterSpotPoint = after,
            LeveledUp = xp.LeveledUp,
            ClassId = run.ClassId,
        };
    }

    // --- projection helpers (kept internal so test subclasses could exercise if needed) ---

    internal static EntryInfoDto ProjectEntryInfo(ViewerArenaTwoPickRun run, long viewerId) => new()
    {
        Id = run.EntryId,
        ViewerId = viewerId,
        RewardScheduleId = run.RewardScheduleId,
        ChallengeId = run.ChallengeId,
        MaxBattleCount = run.MaxBattleCount,
        LeaderSkinId = run.LeaderSkinId,
        IsRetire = run.IsRetire ? 1 : 0,
    };

    internal static BattleResultsDto ProjectBattleResults(ViewerArenaTwoPickRun run)
    {
        var bools = JsonSerializer.Deserialize<List<bool>>(run.ResultListJson) ?? new();
        return new()
        {
            ResultList = bools.Select(b => b ? 1 : 0).ToList(),
            WinCount = run.WinCount,
        };
    }

    internal static ClassInfoDto ProjectClassInfo(ViewerArenaTwoPickRun run)
    {
        var ids = JsonSerializer.Deserialize<List<int>>(run.CandidateClassIdsJson) ?? new();
        return new()
        {
            ClassId1 = ids.ElementAtOrDefault(0),
            ClassId2 = ids.ElementAtOrDefault(1),
            ClassId3 = ids.ElementAtOrDefault(2),
            SelectedClassId = run.ClassId,
        };
    }

    internal static DeckInfoDto ProjectDeckInfo(ViewerArenaTwoPickRun run)
    {
        var cards = JsonSerializer.Deserialize<List<long>>(run.SelectedCardIdsJson) ?? new();
        return new()
        {
            TwoPickEntryId = run.EntryId,
            ClassId = run.ClassId,
            IsSelectCompleted = run.IsSelectCompleted,
            SelectedCardIds = cards,
            SelectTurn = run.SelectTurn == 0 ? 1 : run.SelectTurn,
        };
    }
}
