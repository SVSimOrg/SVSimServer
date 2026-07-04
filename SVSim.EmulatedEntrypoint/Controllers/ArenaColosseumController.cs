using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Deck;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.EmulatedEntrypoint.Services.ArenaColosseum;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Arena Colosseum (Grand Prix) lobby. Phase 1 covers the three read endpoints (<c>/top</c>,
/// <c>/get_fee_info</c>, <c>/event_info</c>) plus the entry/register-deck pair. Defaults to
/// "no event scheduled" via <see cref="ColosseumSeasonConfig.IsColosseumPeriod"/> — flipping
/// the event on is an admin operation per <c>docs/operations/grand-prix-event-setup.md</c>.
/// </summary>
[Route("arena_colosseum")]
public class ArenaColosseumController : SVSimController
{
    private readonly IGameConfigService _config;
    private readonly IArenaColosseumRunRepository _runs;
    private readonly IInventoryService _inventory;
    private readonly IDeckRepository _decks;
    private readonly IColosseumProgressionService _progression;
    private readonly IArenaTwoPickCardPoolService _pool;
    private readonly IRandom _rng;
    private readonly SVSimDbContext _db;

    public ArenaColosseumController(
        IGameConfigService config,
        IArenaColosseumRunRepository runs,
        IInventoryService inventory,
        IDeckRepository decks,
        IColosseumProgressionService progression,
        IArenaTwoPickCardPoolService pool,
        IRandom rng,
        SVSimDbContext db)
    {
        _config = config;
        _runs = runs;
        _inventory = inventory;
        _decks = decks;
        _progression = progression;
        _pool = pool;
        _rng = rng;
        _db = db;
    }

    [HttpPost("top")]
    public async Task<IActionResult> Top([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var season = _config.Get<ColosseumSeasonConfig>();
        var run = await _runs.GetByViewerIdAsync(vid);

        var response = new TopResponse
        {
            ColosseumInfo = BuildColosseumInfo(season),
            ColosseumStatus = BuildOwnStatus(run),
            LeaderSkinId = run?.LeaderSkinId ?? 0,
        };

        if (run is not null)
        {
            response.EntryInfo = new ColosseumEntryRef { Id = run.EntryId };
            response.NowRoundId = run.RoundId;
            response.MaxBattleCount = run.MaxBattleCountThisRound;
            response.IsFinish = run.IsChampion;
            response.FinalRoundEliminateCount = season.FinalRoundEliminateCount;
            response.EndTime = FormatTime(season.EventEndTime);
            response.BattleResults = new ColosseumBattleResults
            {
                WinCount = run.WinCount,
                ResultList = ParseIntList(run.ResultListJson),
            };
            response.BreakthroughNumber = run.BreakthroughNumberThisRound > 0 ? run.BreakthroughNumberThisRound : null;
        }

        return Ok(response);
    }

    [HttpPost("get_fee_info")]
    public async Task<IActionResult> GetFeeInfo([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var season = _config.Get<ColosseumSeasonConfig>();
        var run = await _runs.GetByViewerIdAsync(vid);

        var response = new GetFeeInfoResponseDto
        {
            ColosseumInfo = BuildColosseumInfo(season),
            ColosseumStatus = BuildOwnStatus(run),
        };

        if (!season.IsColosseumPeriod)
        {
            return Ok(response);
        }

        response.IsUnfinishedEntryExists = run is not null;
        response.IsAllowedFreeEntry = season.IsAllowedFreeEntry;
        response.FeeList = new ColosseumFeeList
        {
            RupyCost = season.RupyCost,
            TicketCost = season.TicketCost,
            CrystalCost = season.CrystalCost,
        };

        if (run is not null)
        {
            response.DeckFormat = (int)run.DeckFormat;
        }

        return Ok(response);
    }

    [HttpPost("event_info")]
    public async Task<IActionResult> EventInfo([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var season = _config.Get<ColosseumSeasonConfig>();
        var rounds = _config.Get<ColosseumRoundsConfig>();
        var run = await _runs.GetByViewerIdAsync(vid);

        return Ok(new EventInfoResponse
        {
            ColosseumInfo = new ColosseumEventInfo
            {
                Format = (int)season.DeckFormat,
                StartTime = FormatTime(season.EventStartTime),
                EndTime = FormatTime(season.EventEndTime),
                AnnounceId = season.AnnounceId,
                FinalRoundEliminateCount = season.FinalRoundEliminateCount,
            },
            Round1 = BuildRoundDetail(rounds, 1),
            Round2 = BuildRoundDetail(rounds, 2),
            Round3 = BuildRoundDetail(rounds, 3),
            ColosseumStatus = BuildOwnStatus(run),
        });
    }

    [HttpPost("entry")]
    public async Task<IActionResult> Entry([FromBody] ArenaColosseumEntryRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var season = _config.Get<ColosseumSeasonConfig>();
        if (!season.IsColosseumPeriod)
        {
            return BadRequest(new { error = "colosseum_period_closed" });
        }

        var serverRoundId = ResolveServerRoundId(season);
        if (req.NowRoundId != serverRoundId)
        {
            return BadRequest(new { error = "now_round_id_mismatch", server_round_id = serverRoundId });
        }

        if (await _runs.GetByViewerIdAsync(vid) is not null)
        {
            return BadRequest(new { error = "arena_colosseum_already_in_progress" });
        }

        await using var tx = await _inventory.BeginAsync(vid);

        RewardEntryDto? feeEntry = req.ConsumeItemType switch
        {
            1 => await DebitCrystalAsync(tx, season.CrystalCost),
            3 => await DebitTicketAsync(tx, season.TicketCost),
            4 => await DebitRupyAsync(tx, season.RupyCost),
            5 when season.IsAllowedFreeEntry => null,
            _ => throw new InvalidOperationException($"invalid consume_item_type {req.ConsumeItemType}"),
        };

        var rounds = _config.Get<ColosseumRoundsConfig>();
        var roundConfig = rounds.Rounds.FirstOrDefault(r => r.RoundId == serverRoundId);
        var group = roundConfig?.Groups.FirstOrDefault();

        var run = new ViewerArenaColosseumRun
        {
            ViewerId = vid,
            EntryId = 0,
            SeasonId = season.SeasonId,
            RoundId = serverRoundId,
            DeckFormat = season.DeckFormat,
            LeaderSkinId = 0,
            ConsumeItemType = req.ConsumeItemType,
            MaxBattleCountThisRound = group?.MaxBattleCount ?? 0,
            BreakthroughNumberThisRound = group?.BreakthroughNumber ?? 0,
            RestEntryNum = 0,
        };
        await _runs.UpsertAsync(run);
        run.EntryId = run.Id;
        await _runs.UpsertAsync(run);
        await tx.CommitAsync();

        return Ok(new EntryResponse
        {
            RewardList = feeEntry is null ? new() : new() { feeEntry },
            EntryInfo = new ColosseumEntryRef
            {
                Id = run.EntryId,
                DeckFormat = (int)season.DeckFormat,
            },
        });
    }

    [HttpPost("register_deck")]
    public async Task<IActionResult> RegisterDeck([FromBody] ArenaColosseumRegisterDeckRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null)
        {
            return BadRequest(new { error = "no_active_run" });
        }

        List<int> deckNos;
        try
        {
            deckNos = JsonSerializer.Deserialize<List<int>>(req.DeckNoList) ?? new();
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "deck_no_list_malformed" });
        }

        if (deckNos.Count == 0)
        {
            return BadRequest(new { error = "deck_no_list_empty" });
        }

        // GetDeck filters by (viewerId, format, deckNo) — a slot that exists under a different
        // format returns null here, which is the format-mismatch case from the spec.
        foreach (var no in deckNos)
        {
            var deck = await _decks.GetDeck(vid, run.DeckFormat, no);
            if (deck is null)
            {
                return BadRequest(new { error = "deck_not_found", deck_no = no });
            }
        }

        run.RegisteredDeckNoListJson = JsonSerializer.Serialize(deckNos);
        run.IsPublished = req.IsPublished;
        await _runs.UpsertAsync(run);

        return Ok(new { });
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });

        var rounds = _config.Get<ColosseumRoundsConfig>();
        var decision = _progression.DecideAdvancement(run, rounds);
        if (!decision.IsBracketEnd)
        {
            return BadRequest(new { error = "bracket_not_finished" });
        }

        run.IsChampion = decision.IsChampion;
        var rewardEntries = _progression.BuildFinishRewards(run, rounds);
        var (wireRewards, wireRewardList) = await GrantRewardsAsync(vid, rewardEntries);

        await _runs.DeleteAsync(vid);

        return Ok(new FinishResponse
        {
            Rewards = wireRewards,
            RewardList = wireRewardList,
            ColosseumStatus = new ColosseumOwnStatus
            {
                NowRoundId = run.RoundId,
                IsChampion = decision.IsChampion ? true : null,
                ColosseumName = decision.IsChampion ? rounds.Rounds.Count > 0
                    ? _config.Get<ColosseumSeasonConfig>().ColosseumName
                    : null : null,
            },
        });
    }

    [HttpPost("get_candidate_classes")]
    public async Task<IActionResult> GetCandidateClasses([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });

        // No persistent slate yet — sample 3 from the configured allow-list per
        // ArenaTwoPickConfig.AllowedClassIds. Idempotent re-call gets a fresh slate; the
        // spec says "logged server-side so re-calling is idempotent" — Phase 3 v1 doesn't
        // yet persist the slate (the slate stamps in on /class_choose).
        var aCfg = _config.Get<ArenaTwoPickConfig>();
        if (aCfg.AllowedClassIds.Count < 3)
        {
            return BadRequest(new { error = "arena_two_pick_allowed_class_ids_misconfigured" });
        }

        var sampled = aCfg.AllowedClassIds
            .OrderBy(_ => _rng.Next(int.MaxValue))
            .Take(3)
            .ToList();

        // Persist onto the run so /class_choose can validate the pick.
        run.CandidateClassIdsJson = JsonSerializer.Serialize(sampled);
        await _runs.UpsertAsync(run);

        // v1 emits Normal-mode shape only (per plan §"Defer Chaos until live capture lands").
        // Server still ACCEPTS chaos_id on /class_choose for forward compatibility.
        return Ok(new GetCandidateClassesResponse
        {
            ClassId1 = sampled[0],
            ClassId2 = sampled[1],
            ClassId3 = sampled[2],
        });
    }

    [HttpPost("class_choose")]
    public async Task<IActionResult> ClassChoose([FromBody] ArenaColosseumClassChooseRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });
        if (run.ClassId != 0) return BadRequest(new { error = "arena_colosseum_invalid_state" });

        // Mutually-exclusive request shape per class-choose.md.
        bool isNormal = req.ClassId != 0 && req.ChaosId == 0;
        bool isChaos = req.ChaosId != 0 && req.ClassId == 0;
        if (!isNormal && !isChaos)
        {
            return BadRequest(new { error = "class_choose_requires_class_id_xor_chaos_id" });
        }

        var candidates = JsonSerializer.Deserialize<List<int>>(run.CandidateClassIdsJson) ?? new();
        int chosenClassId = isNormal ? req.ClassId : ResolveChaosClassId(req.ChaosId);
        if (isNormal && !candidates.Contains(chosenClassId))
        {
            return BadRequest(new { error = "arena_colosseum_class_not_offered" });
        }

        run.ClassId = chosenClassId;
        run.ChaosId = isChaos ? req.ChaosId : 0;
        run.LeaderSkinId = chosenClassId; // class-default skin convention from TwoPick

        var pool = _config.Get<ColosseumSeasonConfig>().PoolCardSetIds;
        var pairs = _pool.GeneratePickSetsForTurn(
            chosenClassId, turn: 1, startingPairId: run.NextCandidateId, _rng, poolCardSetIds: pool);
        run.NextCandidateId += pairs.Count;
        run.SelectTurn = 1;
        run.PendingPickSetsJson = JsonSerializer.Serialize(pairs);
        await _runs.UpsertAsync(run);

        return Ok(new SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick.ClassChooseResponseDto
        {
            ClassInfo = ProjectClassInfo(run),
            DeckInfo = ProjectDeckInfo(run),
            CandidateCardList = pairs.Select(ToDto).ToList(),
        });
    }

    [HttpPost("get_candidate_cards")]
    public async Task<IActionResult> GetCandidateCards([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });

        // Idempotent resume — no state mutation here, just replay the current snapshot.
        var pending = JsonSerializer.Deserialize<List<CandidatePair>>(run.PendingPickSetsJson) ?? new();
        return Ok(new GetCandidateCardsResponse
        {
            DeckInfo = ProjectDeckInfo(run),
            CandidateCardList = pending.Select(ToDto).ToList(),
            LeaderSkinId = run.LeaderSkinId == 0 ? null : run.LeaderSkinId,
            ClassInfo = ProjectClassInfo(run),
        });
    }

    [HttpPost("card_choose")]
    public async Task<IActionResult> CardChoose([FromBody] ArenaColosseumCardChooseRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });
        if (run.ClassId == 0 || run.IsSelectCompleted)
            return BadRequest(new { error = "arena_colosseum_invalid_state" });

        var pending = JsonSerializer.Deserialize<List<CandidatePair>>(run.PendingPickSetsJson) ?? new();
        var pick = pending.FirstOrDefault(p => p.Id == req.SelectedId);
        if (pick is null)
            return BadRequest(new { error = "arena_colosseum_invalid_selection" });

        var selectedCards = JsonSerializer.Deserialize<List<long>>(run.SelectedCardIdsJson) ?? new();
        selectedCards.Add(pick.CardId1);
        selectedCards.Add(pick.CardId2);
        run.SelectedCardIdsJson = JsonSerializer.Serialize(selectedCards);

        List<CandidatePair>? nextPairs = null;
        if (run.SelectTurn < 15)
        {
            run.SelectTurn += 1;
            var pool = _config.Get<ColosseumSeasonConfig>().PoolCardSetIds;
            nextPairs = _pool.GeneratePickSetsForTurn(
                run.ClassId, run.SelectTurn, run.NextCandidateId, _rng, poolCardSetIds: pool);
            run.NextCandidateId += nextPairs.Count;
            run.PendingPickSetsJson = JsonSerializer.Serialize(nextPairs);
        }
        else
        {
            run.IsSelectCompleted = true;
            run.PendingPickSetsJson = "[]";
        }
        await _runs.UpsertAsync(run);

        return Ok(new SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick.CardChooseResponseDto
        {
            DeckInfo = ProjectDeckInfo(run),
            CandidateCardList = nextPairs?.Select(ToDto).ToList(),
        });
    }

    [HttpPost("get_hof_deck_list")]
    public Task<IActionResult> GetHofDeckList([FromBody] BaseRequest _) =>
        GetCuratedListAsync<ColosseumHofDeck>();

    [HttpPost("get_windfall_deck_list")]
    public Task<IActionResult> GetWindFallDeckList([FromBody] BaseRequest _) =>
        GetCuratedListAsync<ColosseumWindFallDeck>();

    [HttpPost("get_avatar_deck_list")]
    public Task<IActionResult> GetAvatarDeckList([FromBody] BaseRequest _) =>
        GetCuratedListAsync<ColosseumAvatarDeck>();

    [HttpPost("register_hof_deck")]
    public Task<IActionResult> RegisterHofDeck([FromBody] RegisterCuratedDeckRequest req) =>
        RegisterCuratedAsync<ColosseumHofDeck>(req);

    [HttpPost("register_windfall_deck")]
    public Task<IActionResult> RegisterWindFallDeck([FromBody] RegisterCuratedDeckRequest req) =>
        RegisterCuratedAsync<ColosseumWindFallDeck>(req);

    [HttpPost("register_avatar_deck")]
    public Task<IActionResult> RegisterAvatarDeck([FromBody] RegisterCuratedDeckRequest req) =>
        RegisterCuratedAsync<ColosseumAvatarDeck>(req);

    /// <summary>
    /// Shared list dispatcher for the three curated-deck pools. Wire shape: bare array at
    /// <c>data</c> per get-curated-deck-list.md (NOT a wrapper object — client iterates
    /// <c>ResponseData["data"]</c> directly).
    /// </summary>
    private async Task<IActionResult> GetCuratedListAsync<TEntity>()
        where TEntity : class, IColosseumCuratedDeck
    {
        if (!TryGetViewerId(out _)) return Unauthorized();

        var rows = await _db.Set<TEntity>().AsNoTracking()
            .OrderBy(d => d.DisplayOrder).ThenBy(d => d.DeckNo)
            .ToListAsync();

        var entries = rows.Select(r => new ColosseumCuratedDeckEntry
        {
            DeckId = r.DeckNo,
            ClassId = r.ClassId,
            CardList = JsonSerializer.Deserialize<List<long>>(r.CardListJson) ?? new(),
            SleeveId = r.SleeveId == 0 ? null : r.SleeveId,
            SkinId = r.LeaderSkinId == 0 ? null : r.LeaderSkinId,
        }).ToList();

        return Ok(entries);
    }

    /// <summary>
    /// Shared register dispatcher — validates each <c>deck_no_list</c> entry exists in the
    /// pool table for <typeparamref name="TEntity"/> (cross-pool register is rejected via
    /// the per-pool lookup). Persists onto the active run, NO <c>is_published</c> flag here
    /// — that's constructed-format-only per register-curated-deck.md.
    /// </summary>
    private async Task<IActionResult> RegisterCuratedAsync<TEntity>(RegisterCuratedDeckRequest req)
        where TEntity : class, IColosseumCuratedDeck
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });

        List<int> deckNos;
        try
        {
            deckNos = JsonSerializer.Deserialize<List<int>>(req.DeckNoList) ?? new();
        }
        catch (JsonException)
        {
            return BadRequest(new { error = "deck_no_list_malformed" });
        }

        if (deckNos.Count == 0)
        {
            return BadRequest(new { error = "deck_no_list_empty" });
        }

        var set = _db.Set<TEntity>();
        foreach (var no in deckNos)
        {
            // Per-pool lookup — registering an HOF deck_no against /register_windfall_deck
            // resolves to null here and rejects (cross-pool isolation).
            var found = await set.AnyAsync(d => d.DeckNo == no);
            if (!found)
            {
                return BadRequest(new { error = "deck_not_found", deck_no = no });
            }
        }

        run.RegisteredDeckNoListJson = JsonSerializer.Serialize(deckNos);
        // Curated-register has no is_published flag; clear any prior value to keep state
        // consistent if the viewer switches from constructed to curated mid-bracket.
        run.IsPublished = false;
        await _runs.UpsertAsync(run);

        return Ok(new { });
    }

    /// <summary>v1 placeholder — Phase 3 §"Defer Chaos until live capture lands". Chaos
    /// chara ids map to a class via prod data (e.g. via a ChaosInfoMap). Until that lands,
    /// fall back to the chaos id mod 8 + 1 to keep the pool service happy. Real impl reads
    /// from <c>ColosseumChaosConfig</c> once captured.</summary>
    private static int ResolveChaosClassId(int chaosId) => ((chaosId - 1) % 8) + 1;

    private static SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick.CandidatePairDto
        ToDto(CandidatePair p) => new()
    {
        Id = p.Id, Turn = p.Turn, SetNum = p.SetNum,
        CardId1 = p.CardId1, CardId2 = p.CardId2,
        IsSelected = p.IsSelected ? 1 : 0,
    };

    private static SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick.ClassInfoDto
        ProjectClassInfo(ViewerArenaColosseumRun run)
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

    private static SVSim.EmulatedEntrypoint.Models.Dtos.Common.ArenaTwoPick.DeckInfoDto
        ProjectDeckInfo(ViewerArenaColosseumRun run)
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

    [HttpPost("retire")]
    public async Task<IActionResult> Retire([FromBody] BaseRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        var run = await _runs.GetByViewerIdAsync(vid);
        if (run is null) return BadRequest(new { error = "no_active_run" });

        var rounds = _config.Get<ColosseumRoundsConfig>();
        var rewardEntries = _progression.BuildRetireRewards(run, rounds);
        var (wireRewards, wireRewardList) = await GrantRewardsAsync(vid, rewardEntries);

        await _runs.DeleteAsync(vid);

        return Ok(new FinishResponse
        {
            Rewards = wireRewards,
            RewardList = wireRewardList,
            ColosseumStatus = new ColosseumOwnStatus
            {
                NowRoundId = run.RoundId,
                RestEntryNum = 0,
            },
        });
    }

    /// <summary>
    /// Grant the bundle through <c>IInventoryTransaction.GrantAsync</c> per
    /// <c>feedback_reward_grant_service</c> — single dispatch table for every UserGoodsType.
    /// Returns the two wire forms (rich <c>rewards</c> receipt + wallet-delta <c>reward_list</c>).
    /// </summary>
    private async Task<(List<ColosseumReceivedReward>, List<RewardEntryDto>)> GrantRewardsAsync(
        long viewerId, IReadOnlyList<ColosseumRoundsConfig.RewardEntry> bundle)
    {
        var receipts = new List<ColosseumReceivedReward>();
        var deltas = new List<RewardEntryDto>();
        if (bundle.Count == 0) return (receipts, deltas);

        await using var tx = await _inventory.BeginAsync(viewerId);
        foreach (var entry in bundle)
        {
            var granted = await tx.GrantAsync(entry.Type, entry.DetailId, entry.Count);
            receipts.Add(new ColosseumReceivedReward
            {
                RewardNumber = entry.Count,
                RewardType = (int)entry.Type,
                RewardDetailId = entry.DetailId,
                Name = entry.Name,
            });
            // GrantAsync returns one or more (RewardType, RewardId, RewardNum) tuples — the
            // post-state-total semantics are owned by the inventory commit, which the wallet-delta
            // reflection inherits via tx.CommitAsync below.
            var first = granted.FirstOrDefault();
            deltas.Add(new RewardEntryDto
            {
                RewardType = first is null ? (int)entry.Type : (int)first.RewardType,
                RewardId = first?.RewardId ?? entry.DetailId,
                RewardNum = first?.RewardNum ?? entry.Count,
            });
        }
        await tx.CommitAsync();
        return (receipts, deltas);
    }

    private static int ResolveServerRoundId(ColosseumSeasonConfig season) => 1;

    private async Task<RewardEntryDto> DebitCrystalAsync(IInventoryTransaction tx, int cost)
    {
        var result = await tx.TrySpendAsync(SpendCurrency.Crystal, cost);
        if (!result.Success)
            throw new InvalidOperationException("insufficient_crystal");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Crystal,
            RewardId = 0,
            RewardNum = (int)result.PostStateTotal,
        };
    }

    private async Task<RewardEntryDto> DebitRupyAsync(IInventoryTransaction tx, int cost)
    {
        var result = await tx.TrySpendAsync(SpendCurrency.Rupee, cost);
        if (!result.Success)
            throw new InvalidOperationException("insufficient_rupy");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Rupy,
            RewardId = 0,
            RewardNum = (int)result.PostStateTotal,
        };
    }

    private async Task<RewardEntryDto> DebitTicketAsync(IInventoryTransaction tx, int cost)
    {
        // Colosseum's ticket id is server-internal — using ArenaTwoPick's TicketItemId convention
        // (item id 1) until a per-season override is captured.
        const int ticketItemId = 1;
        var result = await tx.TryDebitAsync(UserGoodsType.Item, ticketItemId, cost);
        if (!result.Success)
            throw new InvalidOperationException("insufficient_ticket");
        return new RewardEntryDto
        {
            RewardType = (int)UserGoodsType.Item,
            RewardId = ticketItemId,
            RewardNum = (int)result.PostStateTotal,
        };
    }

    // --- helpers ---

    private ColosseumLobbyInfo BuildColosseumInfo(ColosseumSeasonConfig season) =>
        ColosseumLobbyInfoBuilder.Build(season, _config.Get<ColosseumRoundsConfig>(), DateTime.UtcNow);

    /// <summary>Builds the <c>colosseum_status</c> block. When the viewer has no run, every
    /// property is null and global WhenWritingNull renders <c>{}</c> — the client
    /// (<c>SetColosseumOwnStatus</c>) short-circuits on <c>status.Count == 0</c>.</summary>
    private static ColosseumOwnStatus BuildOwnStatus(ViewerArenaColosseumRun? run)
    {
        if (run is null) return new ColosseumOwnStatus();

        return new ColosseumOwnStatus
        {
            RestEntryNum = run.RestEntryNum,
            NowRoundId = run.RoundId,
            IsChampion = run.IsChampion ? true : null,
        };
    }

    private static ColosseumRoundDetail BuildRoundDetail(ColosseumRoundsConfig rounds, int roundId)
    {
        var match = rounds.Rounds.FirstOrDefault(r => r.RoundId == roundId);
        if (match is null) return new ColosseumRoundDetail();

        return new ColosseumRoundDetail
        {
            StartTime = FormatTime(match.StartTime),
            EndTime = FormatTime(match.EndTime),
            IsNowRound = IsNowRound(match),
            RoundDetail = match.Groups.Select(g => new ColosseumGroupRow
            {
                Group = g.Group,
                MaxBattleCount = g.MaxBattleCount,
                BreakthroughNumber = g.BreakthroughNumber,
                EntryNumber = g.EntryNumber,
            }).ToList(),
        };
    }

    private static bool IsNowRound(ColosseumRoundsConfig.RoundEntry round)
    {
        var now = DateTime.UtcNow;
        return now >= round.StartTime && now <= round.EndTime;
    }

    private static string FormatTime(DateTime t) =>
        t == default ? "" : t.ToString("yyyy-MM-dd HH:mm:ss");

    private static List<int> ParseIntList(string json) =>
        string.IsNullOrEmpty(json)
            ? new()
            : System.Text.Json.JsonSerializer.Deserialize<List<int>>(json) ?? new();
}
