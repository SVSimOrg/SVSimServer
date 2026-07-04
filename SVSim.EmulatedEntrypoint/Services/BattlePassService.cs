using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.BattlePass;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.BattlePass;

namespace SVSim.EmulatedEntrypoint.Services;

public sealed class BattlePassService : IBattlePassService
{
    // Default cap mirrors the captured /battle_pass/info.gauge_info.weekly_limit_point.
    public const int WeeklyLimitPointDefault = 3000;

    /// <summary>JST = UTC+9. Capture format ("2026-04-01 02:00:00") is implicit JST.</summary>
    private static readonly TimeSpan JstOffset = TimeSpan.FromHours(9);

    private readonly IBattlePassRepository _bp;
    private readonly IViewerBattlePassRepository _viewerBp;
    private readonly TimeProvider _time;
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;

    public BattlePassService(
        IBattlePassRepository bp,
        IViewerBattlePassRepository viewerBp,
        TimeProvider time,
        SVSimDbContext db,
        IInventoryService inv)
    {
        _bp = bp;
        _viewerBp = viewerBp;
        _time = time;
        _db = db;
        _inv = inv;
    }

    public async Task<IReadOnlyDictionary<string, BattlePassLevel>?> GetLevelCurveAsync(CancellationToken ct)
    {
        var rows = await _bp.GetLevelCurveAsync(ct);
        if (rows.Count == 0) return null;
        return rows.ToDictionary(
            r => Inv(r.Level),
            r => new BattlePassLevel { Level = Inv(r.Level), RequiredPoint = Inv(r.RequiredPoint) });
    }

    public async Task<BattlePassInfoResponse?> GetInfoAsync(long viewerId, CancellationToken ct)
    {
        var now = _time.GetUtcNow();
        var season = await _bp.GetActiveSeasonAsync(now, ct);
        if (season is null) return null;

        var progress = await _viewerBp.GetOrCreateProgressAsync(viewerId, season.Id, ct);

        var rewards = await _bp.GetSeasonRewardsAsync(season.Id, ct);
        var claims = await _viewerBp.GetClaimsAsync(viewerId, season.Id, ct);
        var claimSet = claims.Select(c => (c.Track, c.Level)).ToHashSet();

        var curve = await _bp.GetLevelCurveAsync(ct);
        int currentLevel = ComputeLevel(curve, progress.CurrentPoint);

        return new BattlePassInfoResponse
        {
            SeasonInfo = new BattlePassSeasonInfoDto
            {
                Id = Inv(season.Id),
                SeasonName = season.Name,
                MaxLevel = Inv(season.MaxLevel),
                StartDate = FormatWireDate(season.StartDate),
                EndDate = FormatWireDate(season.EndDate),
                // Client uses can_purchase as the sole "show buy button / use normal-pass icon"
                // signal on the home BP screen (Wizard/BattlePass.cs:56,84 + BattlePassHeader.cs:51);
                // it must flip to false once the viewer owns the pass, or the button persists.
                CanPurchase = season.CanPurchase && !progress.IsPremium,
            },
            RewardInfo = new BattlePassRewardInfoDto
            {
                Normal = new BattlePassRewardListDto
                {
                    Reward = rewards.Where(r => r.Track == BattlePassTrack.Normal)
                                    .Select(r => ToRewardDto(r, claimSet))
                                    .ToList(),
                },
                Premium = new BattlePassRewardListDto
                {
                    Reward = rewards.Where(r => r.Track == BattlePassTrack.Premium)
                                    .Select(r => ToRewardDto(r, claimSet))
                                    .ToList(),
                },
            },
            GaugeInfo = new BattlePassGaugeInfoDto
            {
                CurrentPoint = Inv(progress.CurrentPoint),
                CurrentLevel = Inv(currentLevel),
                WeeklyBattlePassPoint = progress.WeeklyPoints,
                WeeklyLimitPoint = WeeklyLimitPointDefault,
            },
            PremiumAppealLevel = null, // populated when premium_appeal config is wired (future)
        };
    }

    public async Task<BattlePassItemListResponse?> GetItemListAsync(long viewerId, CancellationToken ct)
    {
        var now = _time.GetUtcNow();
        var season = await _bp.GetActiveSeasonAsync(now, ct);
        if (season is null) return null;

        var progress = await _viewerBp.GetOrCreateProgressAsync(viewerId, season.Id, ct);

        var response = new BattlePassItemListResponse
        {
            PremiumPassDescription = season.Description,
            SalesPeriodInfo = new BattlePassSalesPeriodInfoDto
            {
                SalesPeriodTime = FormatWireDate(season.EndDate),
            },
            Products = new List<BattlePassProductDto>(),
        };

        // One product per active season; empty if viewer is already premium.
        if (!progress.IsPremium && season.CanPurchase)
        {
            response.Products.Add(new BattlePassProductDto
            {
                Id = season.Id * 1000,
                SeasonId = season.Id,
                Name = $"{season.Name} Premium Pass",
                PriceCrystal = season.PriceCrystal,
                Description = season.Description,
                SalesPeriodInfo = new BattlePassSalesPeriodInfoDto
                {
                    SalesPeriodTime = FormatWireDate(season.EndDate),
                },
            });
        }

        return response;
    }

    public async Task<BattlePassBuyOutcome> BuyPremiumAsync(
        long viewerId, int seasonId, int productId, CancellationToken ct)
    {
        var now = _time.GetUtcNow();
        var season = await _bp.GetActiveSeasonAsync(now, ct);

        // 24: outside BP period, season mismatch, or season not currently purchasable.
        if (season is null || season.Id != seasonId || !season.CanPurchase)
            return new BattlePassBuyOutcome(24, Array.Empty<GrantedReward>(), Array.Empty<GrantedReward>());

        if (productId != season.Id * 1000)
            return new BattlePassBuyOutcome(0, Array.Empty<GrantedReward>(), Array.Empty<GrantedReward>());

        // Guard: viewer must exist (BeginAsync throws InventoryViewerNotFoundException otherwise).
        var viewerExists = await _db.Viewers.AnyAsync(v => v.Id == viewerId, ct);
        if (!viewerExists)
            return new BattlePassBuyOutcome(0, Array.Empty<GrantedReward>(), Array.Empty<GrantedReward>());

        var progress = await _viewerBp.GetOrCreateProgressAsync(viewerId, season.Id, ct);
        if (progress.IsPremium)
            return new BattlePassBuyOutcome(23, Array.Empty<GrantedReward>(), Array.Empty<GrantedReward>());

        // Open inventory tx — loads viewer + opens DB tx.
        await using var tx = await _inv.BeginAsync(viewerId, ct, cfg => cfg.Source = GrantSource.BattlePassClaim);

        var spendResult = await tx.TrySpendAsync(SpendCurrency.Crystal, season.PriceCrystal, ct);
        if (!spendResult.Success)
            return new BattlePassBuyOutcome(22, Array.Empty<GrantedReward>(), Array.Empty<GrantedReward>());

        progress.IsPremium = true;

        // Retroactive grants: every premium reward at level <= current_level not already claimed.
        var rewards = await _bp.GetSeasonRewardsAsync(season.Id, ct);
        var claims = await _viewerBp.GetClaimsAsync(viewerId, season.Id, ct);
        var claimSet = claims.Select(c => (c.Track, c.Level)).ToHashSet();

        var curve = await _bp.GetLevelCurveAsync(ct);
        int currentLevel = ComputeLevel(curve, progress.CurrentPoint);

        foreach (var r in rewards.Where(r => r.Track == BattlePassTrack.Premium && r.Level <= currentLevel))
        {
            if (claimSet.Contains((r.Track, r.Level))) continue;
            _viewerBp.AddClaim(viewerId, season.Id, r.Track, r.Level, now);
            await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber, ct);
        }

        // CommitAsync handles DB save + currency-collision rule. Crystal spend is the first
        // op, any grants override the post-state. result.RewardList carries the final
        // post-state including the deducted crystal balance. result.Deltas carries the raw
        // grant amounts for achieved_info (no spend entry in Deltas, only GrantOps).
        // CommitAsync's SaveChangesAsync flushes the AddClaim rows + the progress.IsPremium
        // mutation alongside the inventory grants — all tracked on the same scoped DbContext.
        var result = await tx.CommitAsync(ct);

        return new BattlePassBuyOutcome(1, result.Deltas, result.RewardList);
    }

    public async Task<BattlePassPointGrant> AddPointsAsync(
        long viewerId, BattlePassPointSource source, int amount, CancellationToken ct)
    {
        var now = _time.GetUtcNow();
        var season = await _bp.GetActiveSeasonAsync(now, ct);
        if (season is null)
        {
            return new BattlePassPointGrant(0, 0, 0, 0, 0, source,
                Array.Empty<SVSim.Database.Services.GrantedReward>());
        }

        var progress = await _viewerBp.GetOrCreateProgressAsync(viewerId, season.Id, ct);

        int beforePoint = progress.CurrentPoint;
        var curve = await _bp.GetLevelCurveAsync(ct);
        int beforeLevel = ComputeLevel(curve, beforePoint);

        RolloverWeeklyIfNeeded(progress, now);
        int headroom = Math.Max(0, WeeklyLimitPointDefault - progress.WeeklyPoints);
        int capped = Math.Max(0, Math.Min(amount, headroom));

        progress.CurrentPoint += capped;
        progress.WeeklyPoints += capped;

        int afterLevel = ComputeLevel(curve, progress.CurrentPoint);

        IReadOnlyList<SVSim.Database.Services.GrantedReward> newlyClaimed = Array.Empty<SVSim.Database.Services.GrantedReward>();
        if (afterLevel > beforeLevel)
        {
            var rewards = await _bp.GetSeasonRewardsAsync(season.Id, ct);
            var claims = await _viewerBp.GetClaimsAsync(viewerId, season.Id, ct);
            var claimSet = claims.Select(c => (c.Track, c.Level)).ToHashSet();

            await using var tx = await _inv.BeginAsync(viewerId, ct, cfg => cfg.Source = GrantSource.BattlePassClaim);

            for (int level = beforeLevel + 1; level <= afterLevel; level++)
            {
                foreach (var r in rewards.Where(r => r.Level == level))
                {
                    if (r.Track == BattlePassTrack.Premium && !progress.IsPremium) continue;
                    if (claimSet.Contains((r.Track, r.Level))) continue;
                    _viewerBp.AddClaim(viewerId, season.Id, r.Track, r.Level, now);
                    await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber, ct);
                }
            }

            var result = await tx.CommitAsync(ct);
            newlyClaimed = result.Deltas;
        }
        else
        {
            // No level crossed → no tx opened → still need to persist the progress mutation
            // (CurrentPoint/WeeklyPoints/WeeklyPeriodStart) tracked on the scoped DbContext.
            await _db.SaveChangesAsync(ct);
        }

        return new BattlePassPointGrant(
            BeforePoint: beforePoint,
            BeforeLevel: beforeLevel,
            AfterPoint: progress.CurrentPoint,
            AfterLevel: afterLevel,
            PointAdd: capped,
            Source: source,
            NewlyClaimed: newlyClaimed);
    }

    private static void RolloverWeeklyIfNeeded(ViewerBattlePassProgressEntry progress, DateTimeOffset now)
    {
        // Open question (see spec "Open assumptions"): true Cygames boundary likely ties to a fixed
        // weekday/timezone. v1 uses a per-viewer 7-day sliding window from first grant.
        if (progress.WeeklyPeriodStart is null)
        {
            progress.WeeklyPeriodStart = now;
            return;
        }
        if (now - progress.WeeklyPeriodStart.Value >= TimeSpan.FromDays(7))
        {
            progress.WeeklyPeriodStart = now;
            progress.WeeklyPoints = 0;
        }
    }

    internal static int ComputeLevel(IReadOnlyList<BattlePassLevelEntry> curve, int point)
    {
        if (curve.Count == 0) return 1;
        int level = curve[0].Level;
        foreach (var row in curve)
        {
            if (point >= row.RequiredPoint) level = row.Level;
            else break;
        }
        return level;
    }

    private static BattlePassRewardDto ToRewardDto(BattlePassRewardEntry r, HashSet<(BattlePassTrack, int)> claimSet)
    {
        return new BattlePassRewardDto
        {
            RewardLevel = Inv(r.Level),
            RewardType = Inv((int)r.RewardType),
            RewardDetailId = Inv(r.RewardDetailId),
            RewardNumber = Inv(r.RewardNumber),
            IsReceived = claimSet.Contains((r.Track, r.Level)),
            IsAppealExclusion = r.Track == BattlePassTrack.Premium
                ? (r.IsAppealExclusion ? "1" : "0")
                : null,
        };
    }

    private static string FormatWireDate(DateTimeOffset dt) =>
        // Capture format is "2026-04-01 02:00:00" (JST, space-separated). Emit in same shape
        // in JST so the client gets back what it gave.
        dt.ToOffset(JstOffset)
          .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    private static string Inv(long v) => v.ToString(CultureInfo.InvariantCulture);
    private static string Inv(int v) => v.ToString(CultureInfo.InvariantCulture);
}
