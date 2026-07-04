using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Pack;
using SVSim.Database.Repositories.PackDrawTables;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Pack;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /pack/* — card-pack shop catalog and pack opening. /tutorial/pack_info and
/// /tutorial/pack_open are aliased here.
/// </summary>
[Route("pack")]
public class PackController : SVSimController
{
    private const string WireDateFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IPackRepository _packs;
    private readonly PackOpenService _opener;
    private readonly IPackDrawTableRepository _drawTables;
    private readonly ICardFoilLookup _foils;
    private readonly IRandom _rng;
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly IGachaPointService _gachaPoint;
    private readonly IGameCalendarService _calendar;

    public PackController(
        IPackRepository packs,
        PackOpenService opener,
        IPackDrawTableRepository drawTables,
        ICardFoilLookup foils,
        IRandom rng,
        SVSimDbContext db,
        IInventoryService inv,
        IGachaPointService gachaPoint,
        IGameCalendarService calendar)
    {
        _packs = packs;
        _opener = opener;
        _drawTables = drawTables;
        _foils = foils;
        _rng = rng;
        _db = db;
        _inv = inv;
        _gachaPoint = gachaPoint;
        _calendar = calendar;
    }

    [HttpPost("info")]
    [HttpPost("/tutorial/pack_info")]
    public async Task<ActionResult<PackInfoResponse>> Info(BaseRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var packs = await _packs.GetActivePacks(DateTime.UtcNow);
        var openCounts = await _packs.GetOpenCountsForViewer(viewerId);

        // Load owned-item counts so child_gacha_info.item_number reflects the viewer's actual
        // ticket inventory (see ToDto). The client filters tutorial packs by item_number > 0
        // — without this the legendary starter pack (99047, requires 1× item 90001) and the
        // throwback pack (80047, requires 1× item 80001) are hidden even when the tutorial
        // gift just granted those tickets, blocking the END transition.
        //
        // OwnedItemEntry is [Owned] by Viewer, and EF refuses to track owned entities without
        // their owner in the result. Project to primitive pairs in the database query before
        // materialising into the dictionary — no entity tracking, single round-trip.
        //
        // Use EF.Property<int>(i, "ItemId") to read the shadow FK directly instead of going
        // through the OwnedItemEntry.Item nav. The nav route works today (EF translates
        // `i.Item.Id` to the FK column), but a future model change that renames the FK or
        // breaks the nav→column mapping would silently fall back to client eval — where
        // `i.Item.Id` returns 0 for every row (the default-initialised ItemEntry) and the
        // dictionary collapses every ticket to item_number=0. Shadow-FK access bypasses
        // that hazard entirely.
        var ownedItemsByItemId = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Items)
            .Select(i => new { ItemId = (long)EF.Property<int>(i, "ItemId"), i.Count })
            .ToDictionaryAsync(x => x.ItemId, x => x.Count);

        var gachaPointBalancesByPackId = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.GachaPointBalances)
            .Select(b => new { b.PackId, b.Points })
            .ToDictionaryAsync(x => x.PackId, x => x.Points);

        // Per-viewer free-pack claim records, keyed by campaign id. Drives the
        // "drop the type_detail=10 child once today's quota is spent" filter in ToDto.
        // Plain projection — no owned-entity tracking needed (mirrors the items query above).
        var freeClaimsByCampaignId = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.FreePackClaims)
            .Select(c => new { c.FreeGachaCampaignId, c.LastClaimedAt, c.ClaimCount })
            .ToDictionaryAsync(x => x.FreeGachaCampaignId, x => (x.LastClaimedAt, x.ClaimCount));

        // Per-viewer RotationStarter class choices keyed by pack id — populates
        // selected_class_id on the parent PackConfigDto so the client's
        // StarterClassSelectDialog can pre-select on revisit (PackInfoTask.cs:86 reads via
        // TryGetValue, so absent-key for non-RS packs is intentional/safe).
        var starterClassesByPackId = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.PackStarterClasses)
            .Select(s => new { s.PackId, s.ClassId })
            .ToDictionaryAsync(x => x.PackId, x => x.ClassId);

        return new PackInfoResponse
        {
            PackConfigList = packs
                .Select(p => ToDto(p, openCounts, ownedItemsByItemId, gachaPointBalancesByPackId, freeClaimsByCampaignId, starterClassesByPackId))
                .ToList(),
        };
    }

    private PackConfigDto ToDto(
        PackConfigEntry p,
        IReadOnlyDictionary<int, ViewerPackOpenCount> openCounts,
        IReadOnlyDictionary<long, int> ownedItemsByItemId,
        IReadOnlyDictionary<int, int> gachaPointBalancesByPackId,
        IReadOnlyDictionary<int, (DateTime LastClaimedAt, int ClaimCount)> freeClaimsByCampaignId,
        IReadOnlyDictionary<int, int> starterClassesByPackId)
    {
        int openCount = openCounts.TryGetValue(p.Id, out var oc) ? oc.OpenCount : 0;

        // Suppress the daily-single half-off flag once the viewer has claimed this parent
        // pack's DAILY child today. The client's UI is entirely wire-driven — it renders the
        // half-off button iff `is_daily_single: true` (GachaPackAreaLayout.cs:420,
        // PackInfoTask.cs:143 → PackChildGachaInfo.IsDailySingle). Without this gate the button
        // stays visible after the first successful open and a second click 400s with
        // `daily_free_already_claimed`. Keep the child entry itself so the CRYSTAL_MULTI
        // full-price button still activates.
        bool dailyClaimedToday = !_calendar.ResetReady(oc?.LastDailyFreeAt);

        // Drop type_detail=10 (FREE_PACKS) children whose daily quota for THIS viewer is spent.
        // Mirrors prod behavior: post-claim /pack/info simply omits the free child from
        // child_gacha_info (verified in traffic_event_crate_free_pack.ndjson lines 28→32).
        // Reset happens at the daily boundary — new day resets ClaimCount effectively.
        bool ChildAvailable(PackChildGachaEntry c)
        {
            if (c.TypeDetail != CardPackType.FreePacks) return true;
            if (c.FreeGachaCampaignId is not int campaignId) return true;
            if (!freeClaimsByCampaignId.TryGetValue(campaignId, out var claim)) return true;
            if (_calendar.ResetReady(claim.LastClaimedAt)) return true;
            int dailyCap = c.DailyFreeGachaCount > 0 ? c.DailyFreeGachaCount : 1;
            return claim.ClaimCount < dailyCap;
        }
        var visibleChildren = p.ChildGachas.Where(ChildAvailable).ToList();

        // Ticket-only pack: every child is TICKET (4) or TICKET_MULTI (5). These are
        // gifted-currency packs (tutorial starter, throwback) that don't participate in
        // gacha-point accrual or exchange, even if GachaPointConfig is set in seed.
        bool isTicketOnly = visibleChildren.All(c =>
            c.TypeDetail == CardPackType.Ticket || c.TypeDetail == CardPackType.TicketMulti);

        PackGachaPointDto? gachaPointDto = null;
        if (p.GachaPointConfig is not null && !isTicketOnly)
        {
            int balance = gachaPointBalancesByPackId.TryGetValue(p.Id, out var b) ? b : 0;
            int threshold = p.GachaPointConfig.ExchangeablePoint;
            gachaPointDto = new PackGachaPointDto
            {
                PackId = p.BasePackId.ToString(CultureInfo.InvariantCulture),
                GachaPoint = balance,
                IncreaseGachaPoint = p.GachaPointConfig.IncreaseGachaPoint.ToString(CultureInfo.InvariantCulture),
                ExchangeableGachaPoint = threshold,
                IsExchangeableGachaPoint = balance >= threshold,
            };
        }

        return new PackConfigDto
        {
            ParentGachaId = p.Id,
            BasePackId = p.BasePackId,
            OverrideDrawEffectPackId = p.OverrideDrawEffectPackId,
            OverrideUiEffectPackId = p.OverrideUiEffectPackId,
            GachaType = p.GachaType,
            SleeveId = p.SleeveId,
            SpecialSleeveId = p.SpecialSleeveId,
            CommenceDate = p.CommenceDate.ToString(WireDateFormat, CultureInfo.InvariantCulture),
            CompleteDate = p.CompleteDate.ToString(WireDateFormat, CultureInfo.InvariantCulture),
            CardpackBannerList = p.Banners.Select(b => new PackBannerDto
            {
                BannerName = b.BannerName,
                DialogTitle = b.DialogTitle,
            }).ToList(),
            GachaDetail = p.GachaDetail,
            ChildGachaInfo = visibleChildren.Select(c => new PackChildGachaDto
            {
                GachaId = c.GachaId,
                TypeDetail = (int)c.TypeDetail,
                Cost = c.Cost,
                Count = c.CardCount,
                ItemId = c.ItemId?.ToString(CultureInfo.InvariantCulture),
                // item_number is viewer-specific — the count of item_id this viewer currently
                // owns, NOT a per-pack-catalog value. Verified against the prod tutorial
                // capture: legendary pack 99047 reports item_number=1 right after the gift
                // granted 1× ticket id=90001; throwback 80047 reports 40 right after the gift
                // granted 40× ticket id=80001. Client filters the tutorial pack list to
                // packs with non-zero item_number (free packs like 92001 are special-cased
                // separately), so this lookup is what makes the tutorial-final pack show up.
                ItemNumber = c.ItemId is long iid && ownedItemsByItemId.TryGetValue(iid, out var ownedCount)
                    ? ownedCount
                    : 0,
                IsDailySingle = c.IsDailySingle && !(c.TypeDetail == CardPackType.Daily && dailyClaimedToday),
                OverrideIncreaseGachaPoint = c.OverrideIncreaseGachaPoint.ToString(CultureInfo.InvariantCulture),
                CampaignName = c.CampaignName,
                PurchaseLimitCount = c.PurchaseLimitCount > 0
                    ? c.PurchaseLimitCount.ToString(CultureInfo.InvariantCulture)
                    : null,
                DailyFreeGachaCount = c.DailyFreeGachaCount > 0
                    ? c.DailyFreeGachaCount.ToString(CultureInfo.InvariantCulture)
                    : null,
                FreeGachaCampaignId = c.FreeGachaCampaignId,
            }).ToList(),
            OpenCount = openCount,
            OpenCountLimit = p.OpenCountLimit,
            IsHide = p.IsHide ? 1 : 0,
            PackCategory = (int)p.PackCategory,
            SelectedClassId = starterClassesByPackId.TryGetValue(p.Id, out var chosenClass) ? chosenClass : null,
            GachaPoint = gachaPointDto,
            IsPreRelease = p.IsPreRelease,
            ExistsPurchaseReward = false,
            IsNew = p.IsNew,
            PosterType = p.PosterType,
            SalesPeriodInfo = new(),  // emit `{}` per the DTO docstring
        };
    }

    [HttpPost("get_gacha_point_rewards")]
    public async Task<ActionResult<GetGachaPointRewardsResponse>> GetGachaPointRewards(
        GetGachaPointRewardsRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // odds_gacha_id is the active seasonal pack id (the one with GachaPointConfig +
        // balance). parent_gacha_id is the base_pack_id of the family — not the lookup key.
        // See GetGachaPointRewardsRequest docstring; verified against
        // traffic_prod_all_gacha_exchange.ndjson.
        var rewards = await _gachaPoint.GetRewardsAsync(request.OddsGachaId, viewerId);

        return new GetGachaPointRewardsResponse
        {
            GachaPointRewards = rewards.ToList(),
        };
    }

    [HttpPost("exchange_gacha_point")]
    public async Task<ActionResult<ExchangeGachaPointResponse>> ExchangeGachaPoint(
        ExchangeGachaPointRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // Open inventory tx with extra includes for GachaPointBalances + GachaPointReceived
        // (needed by TryExchangeAsync to validate balance and already-received guard).
        await using var tx = await _inv.BeginAsync(viewerId, configure: cfg =>
        {
            cfg.Source = GrantSource.GachaPointExchange;
            cfg.WithInclude(v => v.GachaPointBalances);
            cfg.WithInclude(v => v.GachaPointReceived);
        });

        // Use odds_gacha_id (the seasonal pack id) — that's where the balance / received marker
        // live. Mirrors the GetGachaPointRewards fix.
        var outcome = await _gachaPoint.TryExchangeAsync(tx, request.OddsGachaId, request.CardId);
        if (!outcome.Success) return BadRequest(new { error = outcome.Error });

        await tx.CommitAsync();

        return new ExchangeGachaPointResponse
        {
            RewardList = outcome.RewardList.ToList(),
        };
    }

    /// <summary>
    /// /pack/set_rotation_starter_class — lock in the class for a RotationStarterCardPack.
    /// One-shot per (viewer, pack); rejects the second attempt with 400. The client opens
    /// the pack via /pack/open afterwards (see Wizard/StarterClassSelectDialog.cs which
    /// chains this call into StarterPurchaseConfirmationDialog on 200).
    /// </summary>
    [HttpPost("set_rotation_starter_class")]
    public async Task<ActionResult<EmptyResponse>> SetRotationStarterClass(
        PackSetRotationStarterClassRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (request.ClassId is < 1 or > 8)
            return BadRequest(new { error = "invalid_class" });

        var pack = await _packs.GetPack(request.PackId);
        if (pack is null) return NotFound(new { error = "unknown_pack" });
        if (pack.PackCategory != PackCategory.RotationStarterCardPack)
            return BadRequest(new { error = "not_a_rotation_starter_pack" });

        var viewer = await _db.Viewers
            .Include(v => v.PackStarterClasses)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        if (viewer.PackStarterClasses.Any(s => s.PackId == request.PackId))
            return BadRequest(new { error = "class_already_chosen" });

        viewer.PackStarterClasses.Add(new ViewerPackStarterClass
        {
            PackId = request.PackId,
            ClassId = request.ClassId,
        });
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }

    [HttpPost("get_leader_skin_owned_status")]
    public ActionResult<Dictionary<string, Dictionary<string, object>>> GetLeaderSkinOwnedStatus(
        [FromBody] PackGetLeaderSkinOwnedStatusRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();

        // ClanType range MIN..MAX per spec (CardBasePrm.ClanType). 0..8 inclusive.
        // Each bucket is `{}` — no curated leader-skin catalog per pack yet.
        var result = new Dictionary<string, Dictionary<string, object>>(9);
        for (int classId = 0; classId <= 8; classId++)
            result[classId.ToString()] = new Dictionary<string, object>();
        return result;
    }

    [HttpPost("open")]
    [HttpPost("/tutorial/pack_open")]
    public async Task<ActionResult<PackOpenResponse>> Open(PackOpenRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // /tutorial/pack_open is a plain alias for /pack/open — the client uses it during the
        // tutorial's final "open the starter legendary pack" step. The only wire-level
        // difference is that the response carries tutorial_step=100 so the client transitions
        // out of the tutorial. Currency/ticket debits, open-count tracking, and pack identity
        // all flow through the normal path — the tutorial gift's tickets naturally constrain
        // which packs are openable via this alias to the current throwback starter pair.
        bool isTutorialPath = HttpContext.Request.Path.StartsWithSegments("/tutorial/pack_open");

        // Skin-card overload not implemented; rotation-starter (class_id) IS supported below.
        if (request.TargetCardId.HasValue)
            return StatusCode(StatusCodes.Status501NotImplemented, new { error = "skin_overload_not_implemented" });

        var pack = await _packs.GetPack(request.ParentGachaId);
        if (pack is null) return NotFound(new { error = "unknown_pack" });

        // Rotation-starter packs require a class_id (1..8) to select the per-class card pool.
        // Reject if missing/out-of-range; reject class_id on non-RS packs so it can't sneak
        // through and pick up nothing (a class-filtered draw against a non-RS pack would
        // return an empty pool and 500). For RS packs the client is expected to have already
        // committed the choice via /pack/set_rotation_starter_class — cross-check that the
        // request class matches the persisted one so a tampered request can't swap pools
        // mid-flight.
        bool isRotationStarter = pack.PackCategory == PackCategory.RotationStarterCardPack;
        if (isRotationStarter)
        {
            if (request.ClassId is not int cid || cid < 1 || cid > 8)
                return BadRequest(new { error = "class_id_required_for_rotation_starter" });
            var persisted = await _db.Viewers
                .Where(v => v.Id == viewerId)
                .SelectMany(v => v.PackStarterClasses)
                .Where(s => s.PackId == request.ParentGachaId)
                .Select(s => (int?)s.ClassId)
                .FirstOrDefaultAsync();
            if (persisted is null)
                return BadRequest(new { error = "rotation_starter_class_not_chosen" });
            if (persisted != cid)
                return BadRequest(new { error = "rotation_starter_class_mismatch" });
        }
        else if (request.ClassId.HasValue)
        {
            return BadRequest(new { error = "class_id_not_valid_for_pack_category" });
        }

        // Skin / leader-skin packs aren't drawn in v1 regardless of child type.
        if (pack.PackCategory is PackCategory.LeaderSkinPack
                              or PackCategory.FreePackLeaderSkin
                              or PackCategory.LegendAndLeaderSkinSinglePack)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { error = "skin_starter_category_not_implemented" });
        }

        var child = pack.ChildGachas.FirstOrDefault(c => c.GachaId == request.GachaId);
        if (child is null)
            return BadRequest(new { error = "unknown_child_gacha" });

        // Note: request.GachaType is the PARENT pack's gacha_type (a routing/disambiguation field),
        // NOT the child's type_detail. Prod traffic confirms the client sends gacha_type=1 even
        // when buying a RUPY_MULTI (type_detail=7) child. The gacha_id alone disambiguates the
        // child; gacha_type validation against child.TypeDetail would falsely reject every buy.

        // Supported: Crystal / CrystalMulti -> spend crystals; Rupy / RupyMulti -> spend rupees;
        // Daily -> spend rupees, once per UTC day; Ticket / TicketMulti -> consume child.ItemId
        // from OwnedItemEntry; FreePacks -> no debit, gated by per-campaign daily quota.
        // CrystalSpecial / CrystalSelectSkin / CrystalAcquireSkinCardPack and the
        // FreePackWithSkin / RotationStarterPack overlays need extra selection / banner
        // plumbing — kept 501 until the relevant flows land.
        if (child.TypeDetail is not (
            CardPackType.Crystal or CardPackType.CrystalMulti or CardPackType.Daily or
            CardPackType.Ticket or CardPackType.TicketMulti or CardPackType.Rupy or
            CardPackType.RupyMulti or CardPackType.FreePacks))
            return StatusCode(StatusCodes.Status501NotImplemented, new { error = "currency_path_not_implemented" });

        // Load viewer via InventoryService transaction with extra includes for pack-open needs.
        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg =>
        {
            cfg.Source = GrantSource.PackOpen;
            cfg.WithInclude(v => v.PackOpenCounts);
            cfg.WithInclude(v => v.GachaPointBalances);
            cfg.WithInclude(v => v.MissionData);
            cfg.WithInclude(v => v.FreePackClaims);
        });
        var viewer = tx.Viewer;

        const int TutorialEndStep = 100;

        int packNumber = Math.Max(1, request.PackNumber);

        // Currency check + deduction. TICKET_MULTI is the mechanism the tutorial alias rides
        // on: the tutorial gift grants the starter ticket, and this branch consumes it, so no
        // separate tutorial-path bypass is needed.
        switch (child.TypeDetail)
        {
            case CardPackType.Crystal:
            case CardPackType.CrystalMulti:
            {
                long cost = (long)child.Cost * packNumber;
                var r = await tx.TrySpendAsync(SpendCurrency.Crystal, cost);
                if (!r.Success) return BadRequest(new { error = "insufficient_crystals" });
                break;
            }
            case CardPackType.Rupy:
            case CardPackType.RupyMulti:
            {
                long cost = (long)child.Cost * packNumber;
                var r = await tx.TrySpendAsync(SpendCurrency.Rupee, cost);
                if (!r.Success) return BadRequest(new { error = "insufficient_rupees" });
                break;
            }
            case CardPackType.Daily:
            {
                // DAILY is the once-per-day half-off crystal single-pack (GachaUI.cs:1046 →
                // CheckBuyPackWithCrystal, decompile-confirmed). Currency is Crystal, NOT Rupee.
                var existing = viewer.PackOpenCounts.FirstOrDefault(p => p.PackId == pack.Id);
                if (!_calendar.ResetReady(existing?.LastDailyFreeAt))
                    return BadRequest(new { error = "daily_free_already_claimed" });

                long cost = (long)child.Cost * packNumber;
                var r = await tx.TrySpendAsync(SpendCurrency.Crystal, cost);
                if (!r.Success) return BadRequest(new { error = "insufficient_crystals" });
                break;
            }
            case CardPackType.Ticket:
            case CardPackType.TicketMulti:
            {
                if (child.ItemId is not long ticketItemId)
                    return StatusCode(StatusCodes.Status501NotImplemented, new { error = "ticket_pack_missing_item_id" });

                int ticketsNeeded = child.Cost * packNumber;
                var debit = await tx.TryDebitAsync(UserGoodsType.Item, ticketItemId, ticketsNeeded);
                if (!debit.Success) return BadRequest(new { error = "insufficient_tickets" });
                break;
            }
            case CardPackType.FreePacks:
            {
                if (child.FreeGachaCampaignId is not int campaignId)
                    return StatusCode(StatusCodes.Status501NotImplemented, new { error = "free_pack_missing_campaign_id" });

                int dailyCap = child.DailyFreeGachaCount > 0 ? child.DailyFreeGachaCount : 1;
                var existing = viewer.FreePackClaims.FirstOrDefault(c => c.FreeGachaCampaignId == campaignId);
                bool resetSinceLastClaim = existing is null || _calendar.ResetReady(existing.LastClaimedAt);
                if (existing is not null && !resetSinceLastClaim && existing.ClaimCount >= dailyCap)
                    return BadRequest(new { error = "free_pack_already_claimed_today" });

                // pack_number is forced to 1 — free-pack metadata never authorizes multi-opens.
                // The capture shows pack_number=1 even when daily_free_gacha_count=1 == daily quota.
                packNumber = 1;

                if (existing is null)
                {
                    viewer.FreePackClaims.Add(new ViewerFreePackClaim
                    {
                        FreeGachaCampaignId = campaignId,
                        ClaimCount = 1,
                        LastClaimedAt = DateTime.UtcNow,
                    });
                }
                else if (resetSinceLastClaim)
                {
                    existing.ClaimCount = 1;
                    existing.LastClaimedAt = DateTime.UtcNow;
                }
                else
                {
                    existing.ClaimCount++;
                    existing.LastClaimedAt = DateTime.UtcNow;
                }
                break;
            }
        }

        await _packs.IncrementOpenCount(viewerId, pack.Id, packNumber);
        if (child.TypeDetail == CardPackType.Daily)
        {
            await _packs.MarkDailyFreeUsed(viewerId, pack.Id, DateTime.UtcNow);
        }

        // Draw + persist. DAILY single overrides packNumber to 1 (it's a one-card open).
        int drawCount = child.IsDailySingle ? 1 : packNumber;

        var drawTable = await _drawTables.GetAsync(pack.Id);
        if (drawTable is null)
            return StatusCode(StatusCodes.Status501NotImplemented, new { error = "pack_draw_table_missing" });

        // Owned card_ids for the rate-less Guaranteed-Leader-Card branch. Project to longs to
        // avoid pulling viewer.Cards entities into memory. Shadow-FK access (EF.Property) per
        // the project_ef_nav_include_pitfall memory.
        var ownedCardIds = await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Cards)
            .Select(c => (long)EF.Property<int>(c, "CardId"))
            .ToListAsync();

        var draw = _opener.Draw(
            drawTable,
            pack,
            drawCount,
            request.ExcludeCardIds ?? Array.Empty<long>(),
            ownedCardIds,
            _foils,
            _rng,
            classId: request.ClassId);

        // Grant drawn cards through the transaction — cosmetic cascade fires on first-time owners.
        foreach (var grp in draw.Cards.GroupBy(c => c.CardId))
            await tx.GrantAsync(UserGoodsType.Card, grp.Key, grp.Count());

        _gachaPoint.Accrue(viewer, pack, child, drawCount);

        // Tutorial alias epilogue: advance TutorialState to END (max-preserve so a higher
        // sentinel is never regressed) and emit tutorial_step=100 on the wire. The client's
        // PackOpenTask.Parse runs _userTutorial.Update on the response — this is the END
        // signal that transitions the client out of the tutorial.
        int? responseTutorialStep = null;
        if (isTutorialPath)
        {
            if (viewer.MissionData.TutorialState < TutorialEndStep)
                viewer.MissionData.TutorialState = TutorialEndStep;
            responseTutorialStep = TutorialEndStep;
        }

        // CommitAsync saves all mutations and produces reward_list with currency-collision resolved.
        var result = await tx.CommitAsync(HttpContext.RequestAborted);
        var rewardList = result.RewardList.ToRewardList();

        return new PackOpenResponse
        {
            PackList = draw.Cards.Select(c => new CardPackEntryDto
            {
                CardId = c.CardId,
                Rarity = (int)c.Rarity,
                Number = 1,
            }).ToList(),
            RewardList = rewardList,
            TutorialStep = responseTutorialStep,
        };
    }
}
