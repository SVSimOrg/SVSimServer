using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public interface IGuildInviteRepository
{
    Task<GuildInvite?> GetAsync(int guildId, long inviteeViewerId, CancellationToken ct = default);
    Task<GuildInvite?> GetByIdAsync(long inviteId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildInvite>> ListPendingForInviteeAsync(long viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildInvite>> ListPendingForGuildAsync(int guildId, CancellationToken ct = default);
    Task<int> CountPendingForInviteeAsync(long viewerId, CancellationToken ct = default);
    Task AddAsync(GuildInvite invite, CancellationToken ct = default);
    Task UpdateStatusAsync(int guildId, long inviteeViewerId, GuildInviteStatus status, DateTime respondedAt, CancellationToken ct = default);
    Task ConsumePendingForViewerAsync(long viewerId, DateTime respondedAt, CancellationToken ct = default);
    Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default);
}
