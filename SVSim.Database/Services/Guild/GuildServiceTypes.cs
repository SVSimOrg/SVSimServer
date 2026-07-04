using SVSim.Database.Entities.Guild;

namespace SVSim.Database.Services.Guild;

public enum GuildOpResultCode
{
    Ok = 0,
    NotInGuild,
    AlreadyInGuild,
    GuildNotFound,
    NameTaken,
    NameInvalid,
    MemberCapReached,
    SubLeaderCapReached,
    PermissionDenied,
    InviteNotFound,
    InviteAlreadyResolved,
    JoinRequestNotFound,
    JoinRequestAlreadyResolved,
    TargetNotInGuild,
    LeaderLeaveBlocked,        // leader cannot leave while other members remain
    InvalidRoleTransition,
}

public sealed record GuildOpResult(GuildOpResultCode Code, int? GuildId = null, string? Detail = null, int? GuildStatus = null)
{
    public static readonly GuildOpResult Ok = new(GuildOpResultCode.Ok);
    public bool IsOk => Code == GuildOpResultCode.Ok;
}

public sealed record GuildFullView(
    Entities.Guild.Guild Guild,
    IReadOnlyList<GuildMember> Members,
    int JoinRequestCount,
    int InviteCount);

public sealed record GuildSearchEntry(Entities.Guild.Guild Guild, int MemberNum, string LeaderName);

public sealed record CreateGuildRequest(string Name, int Activity, int JoinCondition);
public sealed record UpdateGuildRequest(int? Activity, int? JoinCondition, string? Name = null);

/// <summary>
/// Enriched outgoing invite entry returned by <see cref="IGuildService.ListOutgoingInvitesAsync"/>.
/// Combines invite metadata with invitee viewer profile for the <c>/guild/invite_user_list</c> response.
/// </summary>
public sealed record GuildOutgoingInviteEntry(
    long InviteId,
    long InviteeViewerId,
    string Name,
    long EmblemId,
    string CountryCode,
    int Rank,
    int DegreeId,
    DateTime CreatedAt);

/// <summary>
/// Enriched received invite entry returned by <see cref="IGuildService.ListInvitedGuildsAsync"/>.
/// Combines invite id with guild detail fields for the <c>/guild/invited_guild_list</c> response.
/// </summary>
public sealed record GuildReceivedInviteEntry(
    long InviteId,
    Entities.Guild.Guild Guild,
    int MemberNum,
    string LeaderName);

/// <summary>
/// Enriched join-request entry returned by <see cref="IGuildService.ListPendingJoinRequestsForMyGuildAsync"/>.
/// Combines applicant profile data with the request timestamp.
/// </summary>
public sealed record GuildJoinRequestEntry(
    long ApplicantViewerId,
    string Name,
    long EmblemId,
    string CountryCode,
    int Rank,
    int DegreeId,
    bool IsOfficialMarkDisplayed,
    DateTime RequestedAt);
