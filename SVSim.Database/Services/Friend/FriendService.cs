using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SVSim.Database.Models;

namespace SVSim.Database.Services.Friend;

public sealed class FriendService : IFriendService, IPlayedTogetherWriter
{
    internal const int FriendMaxCount = 110;
    internal const int SendApplyMaxCount = 110;
    internal const int PlayedTogetherRetention = 50;

    // Cosmetic field defaults matching the prod capture's "no campaign, normal player" state.
    internal const string DefaultDeviceType = "2";
    internal const string DefaultMaxFriend = "110";
    internal const string DefaultIsReceivedTwoPickMission = "1";
    internal const string DefaultBirth = "0";
    internal const string DefaultMissionChangeTime = "2017-09-15 02:36:09";
    internal const string DefaultMissionReceiveType = "0";
    internal const string DefaultIsOfficial = "0";
    internal const string DefaultIsOfficialMarkDisplayed = "0";

    private readonly SVSimDbContext _db;
    private readonly ILogger<FriendService> _log;

    public FriendService(SVSimDbContext db, ILogger<FriendService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<FriendInfoResult> GetFriendsAsync(long viewerId, CancellationToken ct)
    {
        var friendIds = await _db.ViewerFriends
            .AsNoTracking()
            .Where(f => f.OwnerViewerId == viewerId)
            .OrderBy(f => f.CreatedAt).ThenBy(f => f.FriendViewerId)
            .Select(f => f.FriendViewerId)
            .ToListAsync(ct);

        var friends = new List<FriendEntry>(friendIds.Count);
        foreach (var friendId in friendIds)
        {
            var entry = await BuildFriendEntryAsync(friendId, ct);
            if (entry is not null) friends.Add(entry);
        }

        return new FriendInfoResult(friends, friends.Count, FriendMaxCount);
    }

    public async Task<ReceiveApplyInfoResult> GetReceiveAppliesAsync(long viewerId, CancellationToken ct)
    {
        var rows = await _db.ViewerFriendApplies
            .Where(a => a.ToViewerId == viewerId)
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var applies = new List<FriendApplyEntry>(rows.Count);
        foreach (var row in rows)
            applies.Add(await BuildApplyEntryAsync(row.Id, row.FromViewerId, row.CreatedAt, row.MissionType, ct));

        return new ReceiveApplyInfoResult(applies, ApproveApplyCount: 0);
    }

    public async Task<SendApplyInfoResult> GetSendAppliesAsync(long viewerId, CancellationToken ct)
    {
        var rows = await _db.ViewerFriendApplies
            .Where(a => a.FromViewerId == viewerId)
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var applies = new List<FriendApplyEntry>(rows.Count);
        foreach (var row in rows)
            applies.Add(await BuildApplyEntryAsync(row.Id, row.ToViewerId, row.CreatedAt, row.MissionType, ct));

        int remaining = Math.Max(0, SendApplyMaxCount - rows.Count);
        return new SendApplyInfoResult(applies, remaining, SendApplyMaxCount);
    }

    public async Task<PlayedTogetherResult> GetPlayedTogetherAsync(long viewerId, CancellationToken ct)
    {
        var rows = await _db.ViewerPlayedTogethers
            .Where(p => p.OwnerViewerId == viewerId)
            .OrderByDescending(p => p.PlayedAt)
            .AsNoTracking()
            .ToListAsync(ct);

        var entries = new List<PlayedTogetherEntry>(rows.Count);
        foreach (var row in rows)
        {
            var opp = await LoadViewerProjectionAsync(row.OpponentViewerId, ct);
            if (opp is null) continue; // opponent deleted; skip the dead row

            bool isFriend = await _db.ViewerFriends.AsNoTracking()
                .AnyAsync(f => f.OwnerViewerId == viewerId && f.FriendViewerId == row.OpponentViewerId, ct);

            int friendStatus = 0;
            int friendApplyId = 0;
            if (isFriend)
            {
                friendStatus = 1;
            }
            else
            {
                var sent = await _db.ViewerFriendApplies.AsNoTracking()
                    .Where(a => a.FromViewerId == viewerId && a.ToViewerId == row.OpponentViewerId)
                    .Select(a => (int?)a.Id).FirstOrDefaultAsync(ct);
                if (sent is { } sId) { friendStatus = 2; friendApplyId = sId; }
                else
                {
                    var recv = await _db.ViewerFriendApplies.AsNoTracking()
                        .Where(a => a.FromViewerId == row.OpponentViewerId && a.ToViewerId == viewerId)
                        .Select(a => (int?)a.Id).FirstOrDefaultAsync(ct);
                    if (recv is { } rId) { friendStatus = 3; friendApplyId = rId; }
                }
            }

            entries.Add(new PlayedTogetherEntry(
                (int)opp.Id,
                opp.DisplayName,
                opp.CountryCode,
                ResolveRank(opp.DisplayName),
                opp.EmblemId,
                opp.DegreeId,
                FormatWireTimestamp(opp.LastLogin),
                FormatWireTimestamp(row.PlayedAt),
                friendStatus,
                friendApplyId,
                row.PlayedMode,
                row.BattleType,
                row.DeckFormat,
                row.TwoPickType));
        }

        return new PlayedTogetherResult(entries);
    }

    public async Task<FriendEntry?> SearchAsync(long viewerId, int targetViewerId, CancellationToken ct)
    {
        if (targetViewerId == (int)viewerId) return null;
        return await BuildFriendEntryAsync(targetViewerId, ct);
    }

    public async Task SendApplyAsync(long viewerId, int targetViewerId, CancellationToken ct)
    {
        if (targetViewerId == (int)viewerId)
        {
            _log.LogDebug("SendApply self-target ignored for viewer {ViewerId}", viewerId);
            return;
        }

        bool targetExists = await _db.Viewers.AsNoTracking().AnyAsync(v => v.Id == targetViewerId, ct);
        if (!targetExists)
        {
            _log.LogDebug("SendApply target {Target} not found", targetViewerId);
            return;
        }

        bool alreadyFriends = await _db.ViewerFriends.AsNoTracking()
            .AnyAsync(f => f.OwnerViewerId == viewerId && f.FriendViewerId == targetViewerId, ct);
        if (alreadyFriends)
        {
            _log.LogDebug("SendApply ignored — viewer {ViewerId} already friends with {Target}", viewerId, targetViewerId);
            return;
        }

        bool alreadyPending = await _db.ViewerFriendApplies.AsNoTracking()
            .AnyAsync(a => a.FromViewerId == viewerId && a.ToViewerId == targetViewerId, ct);
        if (alreadyPending) return;

        int outgoingCount = await _db.ViewerFriendApplies.CountAsync(a => a.FromViewerId == viewerId, ct);
        if (outgoingCount >= SendApplyMaxCount)
        {
            _log.LogInformation("SendApply hit cap of {Cap} for viewer {ViewerId}", SendApplyMaxCount, viewerId);
            return;
        }

        _db.ViewerFriendApplies.Add(new ViewerFriendApply
        {
            FromViewerId = viewerId,
            ToViewerId = targetViewerId,
            CreatedAt = DateTime.UtcNow,
            MissionType = 0,
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task ApproveApplyAsync(long viewerId, int applyId, CancellationToken ct)
    {
        var apply = await _db.ViewerFriendApplies
            .FirstOrDefaultAsync(a => a.Id == applyId && a.ToViewerId == viewerId, ct);
        if (apply is null)
        {
            _log.LogDebug("ApproveApply {ApplyId} not addressed to viewer {ViewerId}", applyId, viewerId);
            return;
        }

        long otherViewer = apply.FromViewerId;

        int myFriendCount = await _db.ViewerFriends.CountAsync(f => f.OwnerViewerId == viewerId, ct);
        int otherFriendCount = await _db.ViewerFriends.CountAsync(f => f.OwnerViewerId == otherViewer, ct);
        if (myFriendCount >= FriendMaxCount || otherFriendCount >= FriendMaxCount)
        {
            _log.LogInformation("ApproveApply hit friend cap (me={Me}, other={Other})", myFriendCount, otherFriendCount);
            return;
        }

        var now = DateTime.UtcNow;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        _db.ViewerFriendApplies.Remove(apply);

        // Clean reverse-direction apply if it exists.
        var reverse = await _db.ViewerFriendApplies
            .FirstOrDefaultAsync(a => a.FromViewerId == viewerId && a.ToViewerId == otherViewer, ct);
        if (reverse is not null) _db.ViewerFriendApplies.Remove(reverse);

        _db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = viewerId, FriendViewerId = otherViewer, CreatedAt = now });
        _db.ViewerFriends.Add(new ViewerFriend { OwnerViewerId = otherViewer, FriendViewerId = viewerId, CreatedAt = now });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task RejectApplyAsync(long viewerId, int applyId, CancellationToken ct)
    {
        var apply = await _db.ViewerFriendApplies
            .FirstOrDefaultAsync(a => a.Id == applyId && a.ToViewerId == viewerId, ct);
        if (apply is null) return;
        _db.ViewerFriendApplies.Remove(apply);
        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelApplyAsync(long viewerId, int applyId, CancellationToken ct)
    {
        var apply = await _db.ViewerFriendApplies
            .FirstOrDefaultAsync(a => a.Id == applyId && a.FromViewerId == viewerId, ct);
        if (apply is null) return;
        _db.ViewerFriendApplies.Remove(apply);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RejectAllAppliesAsync(long viewerId, CancellationToken ct)
    {
        await _db.ViewerFriendApplies
            .Where(a => a.ToViewerId == viewerId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task CancelAllAppliesAsync(long viewerId, CancellationToken ct)
    {
        await _db.ViewerFriendApplies
            .Where(a => a.FromViewerId == viewerId)
            .ExecuteDeleteAsync(ct);
    }

    public async Task RejectFriendAsync(long viewerId, int targetViewerId, CancellationToken ct)
    {
        var rows = await _db.ViewerFriends
            .Where(f =>
                (f.OwnerViewerId == viewerId && f.FriendViewerId == targetViewerId) ||
                (f.OwnerViewerId == targetViewerId && f.FriendViewerId == viewerId))
            .ToListAsync(ct);
        if (rows.Count == 0) return;
        _db.ViewerFriends.RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
    }

    public async Task RecordAsync(long ownerViewerId, long opponentViewerId, BattleParticipationContext ctx, CancellationToken ct)
    {
        if (ownerViewerId == opponentViewerId) return;

        var now = DateTime.UtcNow;
        var existing = await _db.ViewerPlayedTogethers
            .FirstOrDefaultAsync(p => p.OwnerViewerId == ownerViewerId && p.OpponentViewerId == opponentViewerId, ct);

        if (existing is null)
        {
            // Enforce per-viewer retention BEFORE insert: if at cap, drop the oldest first.
            int currentCount = await _db.ViewerPlayedTogethers.CountAsync(p => p.OwnerViewerId == ownerViewerId, ct);
            if (currentCount >= PlayedTogetherRetention)
            {
                var toEvict = await _db.ViewerPlayedTogethers
                    .Where(p => p.OwnerViewerId == ownerViewerId)
                    .OrderBy(p => p.PlayedAt).ThenBy(p => p.OpponentViewerId)
                    .FirstAsync(ct);
                _db.ViewerPlayedTogethers.Remove(toEvict);
            }

            _db.ViewerPlayedTogethers.Add(new ViewerPlayedTogether
            {
                OwnerViewerId = ownerViewerId,
                OpponentViewerId = opponentViewerId,
                PlayedAt = now,
                PlayedMode = ctx.PlayedMode,
                BattleType = ctx.BattleType,
                DeckFormat = ctx.DeckFormat,
                TwoPickType = ctx.TwoPickType,
            });
        }
        else
        {
            existing.PlayedAt = now;
            existing.PlayedMode = ctx.PlayedMode;
            existing.BattleType = ctx.BattleType;
            existing.DeckFormat = ctx.DeckFormat;
            existing.TwoPickType = ctx.TwoPickType;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyDictionary<long, FriendRelation>> GetFriendRelationsAsync(
        long viewerId, IReadOnlyList<long> otherViewerIds, CancellationToken ct)
    {
        if (otherViewerIds.Count == 0)
            return new Dictionary<long, FriendRelation>();

        var idSet = otherViewerIds.Where(id => id != viewerId).Distinct().ToList();
        var friendSet = idSet.Count == 0
            ? new HashSet<long>()
            : (await _db.ViewerFriends.AsNoTracking()
                .Where(f => f.OwnerViewerId == viewerId && idSet.Contains(f.FriendViewerId))
                .Select(f => f.FriendViewerId)
                .ToListAsync(ct)).ToHashSet();
        var applySet = idSet.Count == 0
            ? new HashSet<long>()
            : (await _db.ViewerFriendApplies.AsNoTracking()
                .Where(a => a.FromViewerId == viewerId && idSet.Contains(a.ToViewerId))
                .Select(a => a.ToViewerId)
                .ToListAsync(ct)).ToHashSet();

        var result = new Dictionary<long, FriendRelation>(otherViewerIds.Count);
        foreach (var id in otherViewerIds)
        {
            result[id] = id == viewerId
                ? new FriendRelation(false, false)
                : new FriendRelation(friendSet.Contains(id), applySet.Contains(id));
        }
        return result;
    }

    // --- helpers ---

    private sealed record ViewerProjection(
        long Id,
        string DisplayName,
        DateTime LastLogin,
        string CountryCode,
        long EmblemId,
        int DegreeId);

    /// <summary>
    /// Loads a Viewer with Info + cosmetic nav refs, then projects to a slim record.
    /// We materialise the full entity rather than using Select() because EF Core
    /// ignores Include/ThenInclude when a Select projection is present.
    /// </summary>
    private async Task<ViewerProjection?> LoadViewerProjectionAsync(long viewerId, CancellationToken ct)
    {
        var v = await _db.Viewers
            .AsNoTracking()
            .Where(x => x.Id == viewerId)
            .Include(x => x.Info).ThenInclude(i => i.SelectedEmblem)
            .Include(x => x.Info).ThenInclude(i => i.SelectedDegree)
            .FirstOrDefaultAsync(ct);

        if (v is null) return null;

        return new ViewerProjection(
            v.Id,
            v.DisplayName,
            v.LastLogin,
            v.Info.CountryCode,
            v.Info.SelectedEmblem?.Id ?? 0,
            v.Info.SelectedDegree?.Id ?? 0);
    }

    private async Task<FriendEntry?> BuildFriendEntryAsync(long friendViewerId, CancellationToken ct)
    {
        var v = await LoadViewerProjectionAsync(friendViewerId, ct);
        if (v is null) return null;

        return new FriendEntry(
            ViewerId: (int)v.Id,
            Name: v.DisplayName,
            CountryCode: v.CountryCode,
            Rank: ResolveRank(v.DisplayName),
            EmblemId: v.EmblemId,
            DegreeId: v.DegreeId,
            LastPlayTime: FormatWireTimestamp(v.LastLogin),
            DeviceType: DefaultDeviceType,
            MaxFriend: DefaultMaxFriend,
            IsReceivedTwoPickMission: DefaultIsReceivedTwoPickMission,
            Birth: DefaultBirth,
            MissionChangeTime: DefaultMissionChangeTime,
            MissionReceiveType: DefaultMissionReceiveType,
            IsOfficial: DefaultIsOfficial,
            IsOfficialMarkDisplayed: DefaultIsOfficialMarkDisplayed);
    }

    private async Task<FriendApplyEntry> BuildApplyEntryAsync(int applyId, long otherViewerId, DateTime createdAt, int missionType, CancellationToken ct)
    {
        var v = await LoadViewerProjectionAsync(otherViewerId, ct);
        // If viewer was deleted between apply creation and now, emit a placeholder so the wire doesn't break.
        var displayName = v?.DisplayName ?? string.Empty;
        var lastLogin = v?.LastLogin ?? DateTime.UnixEpoch;
        var countryCode = v?.CountryCode ?? string.Empty;
        var emblemId = v?.EmblemId ?? 0;
        var degreeId = v?.DegreeId ?? 0;

        return new FriendApplyEntry(
            Id: applyId,
            ViewerId: (int)otherViewerId,
            Name: displayName,
            CountryCode: countryCode,
            Rank: ResolveRank(displayName),
            EmblemId: emblemId,
            DegreeId: degreeId,
            LastPlayTime: FormatWireTimestamp(lastLogin),
            CreateTime: FormatWireTimestamp(createdAt),
            MissionType: missionType);
    }

    /// <summary>
    /// Rank derivation. We don't track per-viewer rank yet; always 1. Hook here when rank data lands.
    /// </summary>
    private static int ResolveRank(string _) => 1;

    private static string FormatWireTimestamp(DateTime dt) =>
        dt.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
}
