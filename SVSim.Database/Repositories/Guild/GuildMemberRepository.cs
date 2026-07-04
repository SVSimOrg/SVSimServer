using Microsoft.EntityFrameworkCore;
using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public sealed class GuildMemberRepository : IGuildMemberRepository
{
    private readonly SVSimDbContext _db;

    public GuildMemberRepository(SVSimDbContext db) { _db = db; }

    /// <summary>
    /// The unique index on ViewerId guarantees at most one membership row per viewer.
    /// </summary>
    public Task<GuildMember?> GetMembershipAsync(long viewerId, CancellationToken ct = default)
        => _db.GuildMembers.FirstOrDefaultAsync(m => m.ViewerId == viewerId, ct);

    public async Task<IReadOnlyList<GuildMember>> ListByGuildAsync(int guildId, CancellationToken ct = default)
        => await _db.GuildMembers.Where(m => m.GuildId == guildId).ToListAsync(ct);

    public Task<int> CountByGuildAsync(int guildId, CancellationToken ct = default)
        => _db.GuildMembers.CountAsync(m => m.GuildId == guildId, ct);

    public Task<int> CountByGuildAndRoleAsync(int guildId, GuildRole role, CancellationToken ct = default)
        => _db.GuildMembers.CountAsync(m => m.GuildId == guildId && m.Role == role, ct);

    public async Task AddAsync(GuildMember m, CancellationToken ct = default)
    {
        _db.GuildMembers.Add(m);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateRoleAsync(int guildId, long viewerId, GuildRole role, CancellationToken ct = default)
    {
        var member = await _db.GuildMembers
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == viewerId, ct);
        if (member is null) return;
        member.Role = role;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(int guildId, long viewerId, CancellationToken ct = default)
    {
        var member = await _db.GuildMembers
            .FirstOrDefaultAsync(m => m.GuildId == guildId && m.ViewerId == viewerId, ct);
        if (member is null) return;
        _db.GuildMembers.Remove(member);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default)
    {
        var members = await _db.GuildMembers.Where(m => m.GuildId == guildId).ToListAsync(ct);
        _db.GuildMembers.RemoveRange(members);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<int, int>> CountBatchByGuildIdsAsync(
        IReadOnlyList<int> guildIds, CancellationToken ct = default)
    {
        var counts = await _db.GuildMembers
            .Where(m => guildIds.Contains(m.GuildId))
            .GroupBy(m => m.GuildId)
            .Select(g => new { GuildId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Seed all requested guild ids so callers don't need a null-check for guilds with 0 members.
        var result = guildIds.ToDictionary(id => id, _ => 0);
        foreach (var row in counts)
            result[row.GuildId] = row.Count;
        return result;
    }

    public async Task<HashSet<long>> GetViewerIdsInAGuildAsync(
        IReadOnlyList<long> viewerIds, CancellationToken ct = default)
    {
        if (viewerIds.Count == 0) return new HashSet<long>();
        var ids = await _db.GuildMembers
            .Where(m => viewerIds.Contains(m.ViewerId))
            .Select(m => m.ViewerId)
            .ToListAsync(ct);
        return new HashSet<long>(ids);
    }
}
