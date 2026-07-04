using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public interface IGuildJoinRequestRepository
{
    Task<GuildJoinRequest?> GetAsync(int guildId, long viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildJoinRequest>> ListPendingForGuildAsync(int guildId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildJoinRequest>> ListPendingForViewerAsync(long viewerId, CancellationToken ct = default);
    Task<int> CountPendingForGuildAsync(int guildId, CancellationToken ct = default);
    Task AddAsync(GuildJoinRequest r, CancellationToken ct = default);
    Task UpdateStatusAsync(int guildId, long viewerId, GuildJoinRequestStatus status, DateTime respondedAt, CancellationToken ct = default);
    Task CancelPendingForViewerAsync(long viewerId, DateTime respondedAt, CancellationToken ct = default);
    Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default);
}
