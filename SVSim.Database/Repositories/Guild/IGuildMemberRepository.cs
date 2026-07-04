using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public interface IGuildMemberRepository
{
    Task<GuildMember?> GetMembershipAsync(long viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildMember>> ListByGuildAsync(int guildId, CancellationToken ct = default);
    Task<int> CountByGuildAsync(int guildId, CancellationToken ct = default);
    Task<int> CountByGuildAndRoleAsync(int guildId, GuildRole role, CancellationToken ct = default);
    Task AddAsync(GuildMember m, CancellationToken ct = default);
    Task UpdateRoleAsync(int guildId, long viewerId, GuildRole role, CancellationToken ct = default);
    Task RemoveAsync(int guildId, long viewerId, CancellationToken ct = default);
    Task DeleteAllForGuildAsync(int guildId, CancellationToken ct = default);

    /// <summary>Returns a count of members per guild for the given guild ids. Used by Task 9 search.</summary>
    Task<Dictionary<int, int>> CountBatchByGuildIdsAsync(IReadOnlyList<int> guildIds, CancellationToken ct = default);

    /// <summary>
    /// Batch-checks whether each viewer id in <paramref name="viewerIds"/> is currently in any guild.
    /// Returns a set of viewer ids that ARE in a guild. Used by <c>/guild/friend_list</c> to set
    /// <c>is_join_guild</c>.
    /// </summary>
    Task<HashSet<long>> GetViewerIdsInAGuildAsync(IReadOnlyList<long> viewerIds, CancellationToken ct = default);
}
