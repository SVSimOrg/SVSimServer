using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Mapping;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Gift;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Gift;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Persistent gift inbox. /gift/top + /tutorial/gift_top are pure URL aliases over the
/// same ViewerPresent query; /gift/receive_gift + /tutorial/gift_receive share a single
/// ReceiveImpl whose only divergence is the route-gated tutorial-state bump.
///
/// Tutorial gifts are seeded as real ViewerPresent rows during /tool/signup
/// (see ViewerRepository.RegisterAnonymousViewer) — this controller carries no static
/// gift catalog.
/// </summary>
public class GiftController : SVSimController
{
    private const int PageSize = 30;
    private const int GiftReceiveTutorialStep = 41;

    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;

    public GiftController(SVSimDbContext db, IInventoryService inv)
    {
        _db = db;
        _inv = inv;
    }

    [HttpPost("/gift/top")]
    [HttpPost("/tutorial/gift_top")]
    public async Task<ActionResult<GiftTopResponse>> Top([FromBody] GiftTopRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var (unclaimed, history) = await ReadTopWindowAsync(viewerId, request.Page);

        return new GiftTopResponse
        {
            PresentList          = unclaimed.Select(PresentMapper.ToWire).ToList(),
            PresentHistoryList   = history.Select(PresentMapper.ToWire).ToList(),
            LimitOverPresentList = new(),   // expiration sweep deferred — always [] for now
        };
    }

    [HttpPost("/gift/receive_gift")]
    public Task<ActionResult<GiftReceiveResponse>> Receive([FromBody] GiftReceiveRequest r)
        => ReceiveImpl(r, advanceTutorial: false);

    [HttpPost("/tutorial/gift_receive")]
    public Task<ActionResult<GiftReceiveResponse>> TutorialReceive([FromBody] GiftReceiveRequest r)
        => ReceiveImpl(r, advanceTutorial: true);

    private async Task<ActionResult<GiftReceiveResponse>> ReceiveImpl(
        GiftReceiveRequest request, bool advanceTutorial)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var requested = request.PresentIdArray.ToHashSet();
        var state = request.State;   // 1 = MAIL_READ (claim), 3 = MAIL_DELETE

        // Pull only currently-Unclaimed rows matching the request — already-Claimed /
        // Deleted / Expired rows are silently ignored (idempotent retry semantics).
        var targets = await _db.ViewerPresents
            .Where(p => p.ViewerId == viewerId
                     && p.Status == PresentStatus.Unclaimed
                     && requested.Contains(p.PresentId))
            .ToListAsync();

        await using var tx = await _inv.BeginAsync(viewerId, configure: cfg =>
        {
            cfg.Source = GrantSource.AdminGrant;
            cfg.WithInclude(v => v.MissionData);
        });

        var rewardListEntries = new List<GiftRewardListEntry>();
        var now = DateTime.UtcNow;

        foreach (var p in targets)
        {
            if (state == 1)
            {
                var granted = await tx.GrantAsync(
                    WireRewardTypeToUserGoodsType(p.RewardType),
                    p.RewardDetailId,
                    (int)p.RewardCount);

                // reward_list carries POST-STATE TOTALS (client does direct assignment).
                // See project_wire_reward_list_post_state. GrantAsync already returns post-state.
                if (granted.Count > 0)
                {
                    rewardListEntries.Add(new GiftRewardListEntry
                    {
                        RewardType = p.RewardType.ToString(CultureInfo.InvariantCulture),
                        RewardId   = p.RewardDetailId.ToString(CultureInfo.InvariantCulture),
                        RewardNum  = granted[0].RewardNum.ToString(CultureInfo.InvariantCulture),
                    });
                }

                p.Status = PresentStatus.Claimed;
                p.ClaimedAt = now;
            }
            else if (state == 3)
            {
                // MAIL_DELETE: no grant, no reward_list entry, no history. Tombstone the
                // row so re-deletes are idempotent under the same WHERE-Unclaimed filter.
                p.Status = PresentStatus.Deleted;
                p.ClaimedAt = now;   // overload as "decided-at" — tombstone never reaches wire
            }
        }

        // Tutorial step advance — route-gated, no Source/state checks. Preserve-max so
        // replays don't downgrade viewers already past 41.
        if (advanceTutorial && tx.Viewer.MissionData.TutorialState < GiftReceiveTutorialStep)
            tx.Viewer.MissionData.TutorialState = GiftReceiveTutorialStep;

        await tx.CommitAsync();   // throws DbUpdateConcurrencyException on RowVersion conflict

        // Rebuild the inbox window (page 1) — the client wipes its local lists and rebuilds
        // from these.
        var (unclaimed, history) = await ReadTopWindowAsync(viewerId, page: 1);

        // is_unreceived_present drives the home-screen inbox badge — must be the DB count
        // post-commit, NOT hardcoded false (hiding the badge after partial claims).
        var stillUnclaimed = await _db.ViewerPresents
            .AnyAsync(p => p.ViewerId == viewerId && p.Status == PresentStatus.Unclaimed);

        return new GiftReceiveResponse
        {
            CardList = new(),   // capture is []; reward_list carries the grants

            // Echo only ids actually transitioned by THIS call — NOT requested ids, which
            // would re-fire the "received N gifts" popup on replay.
            ReceivedIds = targets
                .Select(t => t.PresentId)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToList(),

            // Per-gift summary for the "+N received" popup. Empty on state=3.
            TotalReceiveCountList = (state == 1 ? targets : Enumerable.Empty<ViewerPresent>())
                .Select(t => new TotalReceiveCountDto
                {
                    RewardType     = t.RewardType,
                    RewardDetailId = t.RewardDetailId,
                    RewardCount    = t.RewardCount,
                    ItemType       = t.ItemType ?? 0,
                    IsUsable       = true,
                }).ToList(),

            PresentList        = unclaimed.Select(PresentMapper.ToWire).ToList(),
            PresentHistoryList = history.Select(PresentMapper.ToWire).ToList(),
            IsUnreceivedPresent = stillUnclaimed,
            RewardList = rewardListEntries,

            // Echo persisted state, not a hardcoded 41 — preserve-max above keeps it stable.
            TutorialStep = tx.Viewer.MissionData.TutorialState,
        };
    }

    /// <summary>
    /// Gift wire's <c>reward_type</c> is a literal <see cref="UserGoodsType"/> integer — the
    /// client's <c>Wizard/RewardBase.cs:245</c> casts it directly to <c>UserGoods.Type</c>.
    /// Mirror that cast, validated against <see cref="GiftRewardTypes.IsSupported(int)"/>.
    /// </summary>
    private static UserGoodsType WireRewardTypeToUserGoodsType(int wireType)
    {
        if (!GiftRewardTypes.IsSupported(wireType))
            throw new InvalidOperationException($"Unsupported gift reward_type {wireType}");
        return (UserGoodsType)wireType;
    }

    private async Task<(List<ViewerPresent> Unclaimed, List<ViewerPresent> History)> ReadTopWindowAsync(
        long viewerId, int page)
    {
        int pageOneIndexed = Math.Max(1, page);
        int skip = (pageOneIndexed - 1) * PageSize;

        // Unclaimed: chronological (oldest first — capture order matches this).
        var unclaimed = await _db.ViewerPresents
            .Where(p => p.ViewerId == viewerId && p.Status == PresentStatus.Unclaimed)
            .OrderBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .Skip(skip).Take(PageSize)
            .ToListAsync();

        // History: most-recent-first (standard inbox UX).
        var history = await _db.ViewerPresents
            .Where(p => p.ViewerId == viewerId && p.Status == PresentStatus.Claimed)
            .OrderByDescending(p => p.ClaimedAt).ThenByDescending(p => p.Id)
            .Skip(skip).Take(PageSize)
            .ToListAsync();

        return (unclaimed, history);
    }
}
