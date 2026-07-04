using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Repositories.Guild;

public interface IGuildRepository
{
    Task<Entities.Guild.Guild?> GetByIdAsync(int guildId, CancellationToken ct = default);

    /// <summary>Returns guild only if not soft-deleted (BreakupAt is null).</summary>
    Task<Entities.Guild.Guild?> GetActiveByIdAsync(int guildId, CancellationToken ct = default);

    /// <summary>Returns guild + populated Members list (split-query).</summary>
    Task<Entities.Guild.Guild?> GetWithMembersAsync(int guildId, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    Task<Entities.Guild.Guild> AddAsync(Entities.Guild.Guild g, CancellationToken ct = default);

    /// <summary>Soft-delete: sets BreakupAt to UtcNow. Caller is responsible for cascading
    /// hard-deletes on Members / Invites / JoinRequests / ChatMessages.</summary>
    Task MarkBrokenUpAsync(int guildId, DateTime brokenUpAt, CancellationToken ct = default);

    /// <summary>Filtered + bucketed search. Empty name = match-all; activity/joinCondition/bucket = 0 = any.</summary>
    Task<IReadOnlyList<Entities.Guild.Guild>> SearchAsync(
        string name, int activity, int joinCondition, int memberBucket,
        int maxMemberCap, int resultCap, CancellationToken ct = default);

    /// <summary>Updates name, activity, and/or join_condition. Only non-null fields are written.</summary>
    Task UpdateActivityAndJoinConditionAsync(int guildId, int? activity, int? joinCondition, string? name = null, CancellationToken ct = default);

    /// <summary>Overwrites the description field.</summary>
    Task UpdateDescriptionAsync(int guildId, string description, CancellationToken ct = default);

    /// <summary>Overwrites the emblem_id field.</summary>
    Task UpdateEmblemAsync(int guildId, long emblemId, CancellationToken ct = default);

    /// <summary>Updates LeaderViewerId on the Guild row — used by ChangeRoleAsync atomic leader transfer.</summary>
    Task UpdateLeaderViewerIdAsync(int guildId, long newLeaderViewerId, CancellationToken ct = default);
}
