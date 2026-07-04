namespace SVSim.Database.Services.Guild;

/// <summary>
/// Single dispatch surface for guild business operations. Enforces:
///   - one viewer ≤1 guild
///   - MaxMemberNum / MaxSubLeaderNum caps (from GuildConfig)
///   - role-based permissions per docs/superpowers/specs/2026-06-27-guild-functionality-design.md
///   - leader-leaves-with-members blocked
///   - auto-routing single-member leader leave -> breakup
///   - clear pending invites + cancel pending join_requests on successful join
///   - emit system chat events via IGuildChatService
/// </summary>
public interface IGuildService
{
    Task<GuildFullView?> GetMyGuildAsync(long viewerId, CancellationToken ct = default);
    Task<Entities.Guild.Guild?> GetActiveAsync(int guildId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildSearchEntry>> SearchAsync(string name, int activity, int joinCondition, int memberBucket, CancellationToken ct = default);

    Task<GuildOpResult> CreateAsync(long viewerId, CreateGuildRequest req, CancellationToken ct = default);
    Task<GuildOpResult> UpdateAsync(long viewerId, UpdateGuildRequest req, CancellationToken ct = default);
    Task<GuildOpResult> UpdateDescriptionAsync(long viewerId, string description, CancellationToken ct = default);
    Task<GuildOpResult> UpdateEmblemAsync(long viewerId, long emblemId, CancellationToken ct = default);
    Task<GuildOpResult> BreakupAsync(long viewerId, CancellationToken ct = default);

    Task<GuildOpResult> InviteAsync(long callerViewerId, long targetViewerId, CancellationToken ct = default);
    Task<GuildOpResult> CancelInviteAsync(long callerViewerId, long inviteId, CancellationToken ct = default);
    Task<GuildOpResult> RejectInviteAsync(long callerViewerId, long inviteId, CancellationToken ct = default);

    Task<GuildOpResult> JoinAsync(long viewerId, int guildId, CancellationToken ct = default);
    Task<GuildOpResult> CancelJoinRequestAsync(long viewerId, CancellationToken ct = default);
    Task<GuildOpResult> AcceptJoinRequestAsync(long callerViewerId, long applicantViewerId, CancellationToken ct = default);
    Task<GuildOpResult> RejectJoinRequestAsync(long callerViewerId, long applicantViewerId, CancellationToken ct = default);

    Task<GuildOpResult> LeaveAsync(long viewerId, CancellationToken ct = default);
    Task<GuildOpResult> RemoveAsync(long callerViewerId, long targetViewerId, CancellationToken ct = default);
    Task<GuildOpResult> ChangeRoleAsync(long callerViewerId, long targetViewerId, int newRoleId, CancellationToken ct = default);

    Task<IReadOnlyList<Entities.Guild.GuildInvite>> ListPendingInvitesForMeAsync(long viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildOutgoingInviteEntry>> ListOutgoingInvitesAsync(long callerViewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildReceivedInviteEntry>> ListInvitedGuildsAsync(long viewerId, CancellationToken ct = default);
    Task<IReadOnlyList<GuildJoinRequestEntry>> ListPendingJoinRequestsForMyGuildAsync(long viewerId, CancellationToken ct = default);
}
