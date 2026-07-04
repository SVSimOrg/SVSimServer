using Microsoft.EntityFrameworkCore;
using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public sealed class GuildInviteRepository : IGuildInviteRepository
{
    private readonly SVSimDbContext _db;

    public GuildInviteRepository(SVSimDbContext db) { _db = db; }

    public Task<GuildInvite?> GetAsync(int guildId, long inviteeViewerId, CancellationToken ct = default)
        => _db.GuildInvites.FirstOrDefaultAsync(
            i => i.GuildId == guildId && i.InviteeViewerId == inviteeViewerId, ct);

    public Task<GuildInvite?> GetByIdAsync(long inviteId, CancellationToken ct = default)
        => _db.GuildInvites.FirstOrDefaultAsync(i => i.Id == inviteId, ct);

    public async Task<IReadOnlyList<GuildInvite>> ListPendingForInviteeAsync(long viewerId, CancellationToken ct = default)
        => await _db.GuildInvites
            .Include(i => i.Guild)
            .Where(i => i.InviteeViewerId == viewerId && i.Status == GuildInviteStatus.Pending)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GuildInvite>> ListPendingForGuildAsync(int guildId, CancellationToken ct = default)
        => await _db.GuildInvites
            .Where(i => i.GuildId == guildId && i.Status == GuildInviteStatus.Pending)
            .ToListAsync(ct);

    public Task<int> CountPendingForInviteeAsync(long viewerId, CancellationToken ct = default)
        => _db.GuildInvites.CountAsync(
            i => i.InviteeViewerId == viewerId && i.Status == GuildInviteStatus.Pending, ct);

    public async Task AddAsync(GuildInvite invite, CancellationToken ct = default)
    {
        _db.GuildInvites.Add(invite);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(int guildId, long inviteeViewerId, GuildInviteStatus status,
        DateTime respondedAt, CancellationToken ct = default)
    {
        var invite = await _db.GuildInvites
            .FirstOrDefaultAsync(i => i.GuildId == guildId && i.InviteeViewerId == inviteeViewerId, ct);
        if (invite is null) return;
        invite.Status = status;
        invite.RespondedAt = respondedAt;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ConsumePendingForViewerAsync(long viewerId, DateTime respondedAt, CancellationToken ct = default)
    {
        var pending = await _db.GuildInvites
            .Where(i => i.InviteeViewerId == viewerId && i.Status == GuildInviteStatus.Pending)
            .ToListAsync(ct);
        foreach (var invite in pending)
        {
            invite.Status = GuildInviteStatus.Consumed;
            invite.RespondedAt = respondedAt;
        }
        if (pending.Count > 0)
            await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default)
    {
        var invites = await _db.GuildInvites.Where(i => i.GuildId == guildId).ToListAsync(ct);
        _db.GuildInvites.RemoveRange(invites);
        if (invites.Count > 0)
            await _db.SaveChangesAsync(ct);
    }
}
