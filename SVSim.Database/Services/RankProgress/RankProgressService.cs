using Microsoft.Extensions.Logging;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;

namespace SVSim.Database.Services.RankProgress;

public sealed class RankProgressService : IRankProgressService
{
    // Simple constants for v1. Promote to a [ConfigSection("RankProgress")] if per-mode
    // tuning is needed later (mirror BattleXpConfig's shape).
    public const int PointsPerWin  = 100;
    public const int PointsPerLoss = 50;

    private const int MasterRankId = 25;
    private const int FirstGrandMasterRankId = 26;

    private readonly IGlobalsRepository _globals;
    private readonly ILogger<RankProgressService> _log;

    private Dictionary<int, RankInfoEntry>? _byId;

    public RankProgressService(IGlobalsRepository globals, ILogger<RankProgressService> log)
    {
        _globals = globals;
        _log = log;
    }

    public async Task<RankProgressResult> GrantAsync(
        Viewer viewer, Format format, bool isWin, CancellationToken ct = default)
    {
        EnsureSupportedFormat(format);
        var byId = await LoadRanksAsync();

        var row = viewer.RankProgress.FirstOrDefault(p => p.Format == format);
        if (row is null)
        {
            row = new ViewerRankProgress { Format = format, Point = 0, MasterPoint = 0 };
            viewer.RankProgress.Add(row);
        }

        // Snapshot pre-tier for the TierAdvanced signal — compared to post-tier below.
        string? preTier = RankTier.Name(CurrentRankId(row, byId));

        int deltaPoint = 0;
        int deltaMp = 0;
        int masterFloor = byId[MasterRankId].LowerLimitPoint; // 50000

        if (isWin)
        {
            if (row.Point < masterFloor)
            {
                deltaPoint = PointsPerWin;
                row.Point += PointsPerWin;
            }
            else
            {
                deltaMp = PointsPerWin;
                row.MasterPoint += PointsPerWin;
            }
        }
        else
        {
            if (row.MasterPoint > 0)
            {
                // Once you're at Grand Master (rank_id >= 26), MP can never drop back to
                // Master. Floor = the Master→GM0 threshold from ranks.csv (5000).
                int rankBeforeDemotion = CurrentRankId(row, byId);
                int mpFloor = rankBeforeDemotion >= FirstGrandMasterRankId
                    ? byId[MasterRankId].AccumulateMasterPoint
                    : 0;
                int newMp = Math.Max(row.MasterPoint - PointsPerLoss, mpFloor);
                deltaMp = newMp - row.MasterPoint;
                row.MasterPoint = newMp;
            }
            else
            {
                int currentRank = CurrentRankId(row, byId);
                int floor = byId[currentRank].LowerLimitPoint;
                int newPoint = Math.Max(row.Point - PointsPerLoss, floor);
                deltaPoint = newPoint - row.Point;
                row.Point = newPoint;
            }
        }

        int finalRank = CurrentRankId(row, byId);
        string? postTier = RankTier.Name(finalRank);
        // Tier "advanced" only on a promotion (pre != post AND both resolve). Demotion by
        // point loss would technically change the tier string too, but rank_achieved is
        // an achievement — it doesn't fire on going backward.
        bool tierAdvanced = isWin && preTier != postTier && postTier is not null && preTier is not null;

        return new RankProgressResult(
            Rank:              finalRank,
            AfterBattlePoint:  row.Point,
            AfterMasterPoint:  row.MasterPoint,
            BattlePoint:       deltaPoint,
            MasterPoint:       deltaMp,
            IsMasterRank:      finalRank == MasterRankId,
            IsGrandMasterRank: finalRank >= FirstGrandMasterRankId,
            TierAdvanced:      tierAdvanced);
    }

    public async Task<RankProgressResult> GetAsync(
        Viewer viewer, Format format, CancellationToken ct = default)
    {
        var byId = await LoadRanksAsync();
        var row = viewer.RankProgress.FirstOrDefault(p => p.Format == format)
                  ?? new ViewerRankProgress { Format = format };
        int rank = CurrentRankId(row, byId);
        return new RankProgressResult(
            Rank: rank,
            AfterBattlePoint: row.Point,
            AfterMasterPoint: row.MasterPoint,
            BattlePoint: 0,
            MasterPoint: 0,
            IsMasterRank: rank == MasterRankId,
            IsGrandMasterRank: rank >= FirstGrandMasterRankId);
    }

    private static void EnsureSupportedFormat(Format format)
    {
        if (format != Format.Rotation && format != Format.Unlimited)
            throw new ArgumentOutOfRangeException(nameof(format),
                $"RankProgressService supports only Rotation/Unlimited, got {format}.");
    }

    /// <summary>
    /// Highest rank_id whose entry conditions the viewer meets.
    /// - MP &gt;= previous rank's AccumulateMasterPoint threshold → GM tier (26..29)
    /// - Point &gt;= 50000 (MasterRankId.LowerLimitPoint) → 25 (Master)
    /// - Otherwise: smallest rank_id in 1..24 where Point &lt; AccumulatePoint.
    /// </summary>
    private static int CurrentRankId(ViewerRankProgress row, Dictionary<int, RankInfoEntry> byId)
    {
        for (int gm = 29; gm >= FirstGrandMasterRankId; gm--)
        {
            int threshold = byId[gm - 1].AccumulateMasterPoint;
            if (threshold > 0 && row.MasterPoint >= threshold)
                return gm;
        }

        if (row.Point >= byId[MasterRankId].LowerLimitPoint)
            return MasterRankId;

        for (int r = 1; r <= 24; r++)
        {
            if (row.Point < byId[r].AccumulatePoint) return r;
        }
        return 24; // guardrail; shouldn't be reachable.
    }

    private async Task<Dictionary<int, RankInfoEntry>> LoadRanksAsync()
    {
        if (_byId is not null) return _byId;
        var rows = await _globals.GetRankInfo();
        if (rows.Count == 0)
        {
            _log.LogWarning("RankProgressService: RankInfo table is empty; grant will no-op.");
        }
        _byId = rows.ToDictionary(r => r.Id);
        return _byId;
    }
}
