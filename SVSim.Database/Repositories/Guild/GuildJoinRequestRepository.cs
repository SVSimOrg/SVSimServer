using Microsoft.EntityFrameworkCore;
using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public sealed class GuildJoinRequestRepository : IGuildJoinRequestRepository
{
    private readonly SVSimDbContext _db;

    public GuildJoinRequestRepository(SVSimDbContext db) { _db = db; }

    public Task<GuildJoinRequest?> GetAsync(int guildId, long viewerId, CancellationToken ct = default)
        => _db.GuildJoinRequests.FirstOrDefaultAsync(
            r => r.GuildId == guildId && r.ViewerId == viewerId, ct);

    public async Task<IReadOnlyList<GuildJoinRequest>> ListPendingForGuildAsync(int guildId, CancellationToken ct = default)
        => await _db.GuildJoinRequests
            .Where(r => r.GuildId == guildId && r.Status == GuildJoinRequestStatus.Pending)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GuildJoinRequest>> ListPendingForViewerAsync(long viewerId, CancellationToken ct = default)
        => await _db.GuildJoinRequests
            .Where(r => r.ViewerId == viewerId && r.Status == GuildJoinRequestStatus.Pending)
            .ToListAsync(ct);

    public Task<int> CountPendingForGuildAsync(int guildId, CancellationToken ct = default)
        => _db.GuildJoinRequests.CountAsync(
            r => r.GuildId == guildId && r.Status == GuildJoinRequestStatus.Pending, ct);

    public async Task AddAsync(GuildJoinRequest r, CancellationToken ct = default)
    {
        _db.GuildJoinRequests.Add(r);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(int guildId, long viewerId, GuildJoinRequestStatus status,
        DateTime respondedAt, CancellationToken ct = default)
    {
        var req = await _db.GuildJoinRequests
            .FirstOrDefaultAsync(r => r.GuildId == guildId && r.ViewerId == viewerId, ct);
        if (req is null) return;
        req.Status = status;
        req.RespondedAt = respondedAt;
        await _db.SaveChangesAsync(ct);
    }

    public async Task CancelPendingForViewerAsync(long viewerId, DateTime respondedAt, CancellationToken ct = default)
    {
        var pending = await _db.GuildJoinRequests
            .Where(r => r.ViewerId == viewerId && r.Status == GuildJoinRequestStatus.Pending)
            .ToListAsync(ct);
        foreach (var req in pending)
        {
            req.Status = GuildJoinRequestStatus.Canceled;
            req.RespondedAt = respondedAt;
        }
        if (pending.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default)
    {
        var requests = await _db.GuildJoinRequests.Where(r => r.GuildId == guildId).ToListAsync(ct);
        _db.GuildJoinRequests.RemoveRange(requests);
        if (requests.Count > 0)
            await _db.SaveChangesAsync(ct);
    }
}
