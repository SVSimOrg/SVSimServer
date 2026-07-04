using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Repositories.Guild;

public sealed class GuildRepository : IGuildRepository
{
    private readonly SVSimDbContext _db;

    public GuildRepository(SVSimDbContext db) { _db = db; }

    public Task<Entities.Guild.Guild?> GetByIdAsync(int guildId, CancellationToken ct = default)
        => _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);

    public Task<Entities.Guild.Guild?> GetActiveByIdAsync(int guildId, CancellationToken ct = default)
        => _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId && g.BreakupAt == null, ct);

    public Task<Entities.Guild.Guild?> GetWithMembersAsync(int guildId, CancellationToken ct = default)
        => _db.Guilds
            .Include(g => g.Members)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.GuildId == guildId, ct);

    public Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
        => _db.Guilds.AnyAsync(g => g.Name == name && g.BreakupAt == null, ct);

    public async Task<Entities.Guild.Guild> AddAsync(Entities.Guild.Guild g, CancellationToken ct = default)
    {
        _db.Guilds.Add(g);
        await _db.SaveChangesAsync(ct);
        return g;
    }

    public async Task MarkBrokenUpAsync(int guildId, DateTime brokenUpAt, CancellationToken ct = default)
    {
        var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);
        if (guild is null) return;
        guild.BreakupAt = brokenUpAt;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Entities.Guild.Guild>> SearchAsync(
        string name, int activity, int joinCondition, int memberBucket,
        int maxMemberCap, int resultCap, CancellationToken ct = default)
    {
        IQueryable<Entities.Guild.Guild> q = _db.Guilds.Where(g => g.BreakupAt == null);

        if (!string.IsNullOrEmpty(name))
            q = q.Where(g => g.Name.Contains(name));

        if (activity != 0)
            q = q.Where(g => (int)g.Activity == activity);

        if (joinCondition != 0)
            q = q.Where(g => (int)g.JoinCondition == joinCondition);

        if (memberBucket is 1 or 2 or 3)
        {
            var (lo, hi) = memberBucket switch
            {
                1 => (1, 10),
                2 => (11, 25),
                _ => (26, maxMemberCap),
            };
            q = q
                .Select(g => new { Guild = g, MemberCount = _db.GuildMembers.Count(m => m.GuildId == g.GuildId) })
                .Where(x => x.MemberCount >= lo && x.MemberCount <= hi)
                .Select(x => x.Guild);
        }

        return await q.Take(resultCap).ToListAsync(ct);
    }

    public async Task UpdateActivityAndJoinConditionAsync(int guildId, int? activity, int? joinCondition, string? name = null, CancellationToken ct = default)
    {
        var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);
        if (guild is null) return;
        if (!string.IsNullOrWhiteSpace(name)) guild.Name = name;
        if (activity.HasValue) guild.Activity = (Entities.Guild.GuildActivity)activity.Value;
        if (joinCondition.HasValue) guild.JoinCondition = (Entities.Guild.GuildJoinCondition)joinCondition.Value;
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateDescriptionAsync(int guildId, string description, CancellationToken ct = default)
    {
        var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);
        if (guild is null) return;
        guild.Description = description;
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateEmblemAsync(int guildId, long emblemId, CancellationToken ct = default)
    {
        var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);
        if (guild is null) return;
        guild.EmblemId = emblemId;
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateLeaderViewerIdAsync(int guildId, long newLeaderViewerId, CancellationToken ct = default)
    {
        var guild = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == guildId, ct);
        if (guild is null) return;
        guild.LeaderViewerId = newLeaderViewerId;
        await _db.SaveChangesAsync(ct);
    }
}
