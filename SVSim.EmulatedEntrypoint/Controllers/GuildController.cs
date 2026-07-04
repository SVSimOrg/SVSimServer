using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Guild;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Friend;
using SVSim.Database.Services.Guild;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;
using SVSim.EmulatedEntrypoint.Models.Dtos.Guild;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>/guild/* — 22 endpoints. See docs/api-spec/endpoints/post-login/guild-*.md.</summary>
[Route("guild")]
public sealed class GuildController : SVSimController
{
    private readonly IGuildService _guild;
    private readonly IGameConfigService _configs;
    private readonly IViewerRepository _viewers;
    private readonly IGuildMemberRepository _members;
    private readonly IFriendService _friends;

    // Wire error code returned when guild operations fail (non-1 result_code).
    private const int GuildErrorResultCode = 2;

    public GuildController(
        IGuildService guild,
        IGameConfigService configs,
        IViewerRepository viewers,
        IGuildMemberRepository members,
        IFriendService friends)
    {
        _guild = guild;
        _configs = configs;
        _viewers = viewers;
        _members = members;
        _friends = friends;
    }

    [HttpPost("info")]
    public async Task<ActionResult<GuildInfoResponse>> Info([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var cfg = _configs.Get<GuildConfig>();
        var view = await _guild.GetMyGuildAsync(viewerId, ct);
        var resp = new GuildInfoResponse
        {
            MaxMemberNum = cfg.MaxMemberNum,
            MaxSubLeaderNum = cfg.MaxSubLeaderNum,
            UsableStampList = cfg.UsableStampList.ConvertAll(i => i.ToString()),
        };
        if (view is null)
        {
            resp.GuildStatus = 0;                         // NOT_JOINING
        }
        else
        {
            resp.GuildStatus = 2;                         // JOINING
            resp.JoinRequestCount = view.JoinRequestCount;
            resp.InviteCount = view.InviteCount;
            var memberDtos = await ToMemberDtoListAsync(view.Members, viewerId, ct);
            var leaderName = memberDtos.FirstOrDefault(m => m.ViewerId == (view.Guild.LeaderViewerId ?? 0L))?.Name ?? "";
            resp.Guild = new GuildBundle
            {
                Detail = ToDetailDto(view.Guild, view.Members.Count, leaderName),
                Members = memberDtos,
            };
        }
        return resp;
    }

    // ===== 21 remaining stubs — each returns the response DTO with defaults =====

    [HttpPost("create")]
    public async Task<ActionResult<EmptyResponse>> Create([FromBody] GuildCreateRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var res = await _guild.CreateAsync(viewerId, new(req.GuildName, req.Activity, req.JoinCondition), ct);
        if (res.IsOk) return new EmptyResponse();
        return MapErrorToWire(res);
    }

    [HttpPost("breakup")]
    public async Task<ActionResult<EmptyResponse>> Breakup([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var v)) return Unauthorized();
        var r = await _guild.BreakupAsync(v, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("update")]
    public async Task<ActionResult<GuildUpdateResponse>> Update([FromBody] GuildUpdateRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        int? activity = req.Activity != 0 ? req.Activity : null;
        int? joinCondition = req.JoinCondition != 0 ? req.JoinCondition : null;
        string? name = string.IsNullOrWhiteSpace(req.GuildName) ? null : req.GuildName;
        var r = await _guild.UpdateAsync(viewerId, new(activity, joinCondition, name), ct);
        if (!r.IsOk) return WireError();

        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return WireError();
        var guild = await _guild.GetActiveAsync(m.GuildId, ct);
        if (guild is null) return WireError();
        var memberCount = await _members.CountByGuildAsync(guild.GuildId, ct);
        var leaderNames = guild.LeaderViewerId.HasValue
            ? await _viewers.LoadDisplayNamesAsync(new[] { guild.LeaderViewerId.Value }, ct)
            : new Dictionary<long, string>();
        var leaderName = leaderNames.GetValueOrDefault(guild.LeaderViewerId ?? 0L, "");
        // GuildUpdateTask.Parse() reads data["guild"] directly as GuildDetailInfo — flat, no "detail" wrapper.
        return new GuildUpdateResponse { Guild = ToDetailDto(guild, memberCount, leaderName) };
    }

    [HttpPost("update_description")]
    public async Task<ActionResult<EmptyResponse>> UpdateDescription([FromBody] GuildUpdateDescriptionRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.UpdateDescriptionAsync(viewerId, req.Description, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("update_emblem")]
    public async Task<ActionResult<GuildUpdateEmblemResponse>> UpdateEmblem([FromBody] GuildUpdateEmblemRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.UpdateEmblemAsync(viewerId, req.EmblemId, ct);
        if (!r.IsOk) return WireError();

        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return WireError();
        var guild = await _guild.GetActiveAsync(m.GuildId, ct);
        if (guild is null) return WireError();
        var memberCount = await _members.CountByGuildAsync(guild.GuildId, ct);
        var leaderNames2 = guild.LeaderViewerId.HasValue
            ? await _viewers.LoadDisplayNamesAsync(new[] { guild.LeaderViewerId.Value }, ct)
            : new Dictionary<long, string>();
        var leaderName2 = leaderNames2.GetValueOrDefault(guild.LeaderViewerId ?? 0L, "");
        // GuildEmblemUpdateTask.Parse() reads data["guild"]["detail"] — nested wrapper required.
        return new GuildUpdateEmblemResponse
        {
            Guild = new GuildDetailSubTree { Detail = ToDetailDto(guild, memberCount, leaderName2) }
        };
    }

    [HttpPost("search_guild")]
    public async Task<ActionResult<GuildSearchGuildResponse>> SearchGuild([FromBody] GuildSearchGuildRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var v)) return Unauthorized();
        var entries = await _guild.SearchAsync(req.GuildName ?? "", req.Activity, req.JoinCondition, req.MemberConditionRange, ct);
        return new GuildSearchGuildResponse { List = entries.Select(e => ToDetailDto(e.Guild, e.MemberNum, e.LeaderName)).ToList() };
    }

    [HttpPost("emblem_list")]
    public async Task<ActionResult<GuildEmblemListResponse>> EmblemList([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var emblemIds = await _viewers.GetEmblemListAsync(viewerId, ct);
        return new GuildEmblemListResponse
        {
            EmblemList = emblemIds.Select(id => new GuildEmblemEntry { EmblemId = id }).ToList()
        };
    }

    [HttpPost("others_info")]
    public async Task<ActionResult<GuildOthersInfoResponse>> OthersInfo([FromBody] GuildOthersInfoRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();
        var guild = await _guild.GetActiveAsync(req.GuildId, ct);
        if (guild is null) return new GuildOthersInfoResponse();
        var memberCount = await _members.CountByGuildAsync(guild.GuildId, ct);
        var othersLeaderNames = guild.LeaderViewerId.HasValue
            ? await _viewers.LoadDisplayNamesAsync(new[] { guild.LeaderViewerId.Value }, ct)
            : new Dictionary<long, string>();
        var othersLeaderName = othersLeaderNames.GetValueOrDefault(guild.LeaderViewerId ?? 0L, "");
        return new GuildOthersInfoResponse
        {
            Guild = new GuildDetailSubTree
            {
                Detail = ToDetailDto(guild, memberCount, othersLeaderName)
            }
        };
    }

    [HttpPost("friend_list")]
    public async Task<ActionResult<List<GuildInviteCandidateDto>>> FriendList([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var friendInfo = await _friends.GetFriendsAsync(viewerId, ct);
        if (friendInfo.Friends.Count == 0)
            return new List<GuildInviteCandidateDto>();

        // For each friend, determine if they are already in a guild (is_join_guild).
        var friendViewerIds = friendInfo.Friends.Select(f => (long)f.ViewerId).ToList();
        var inGuild = await _members.GetViewerIdsInAGuildAsync(friendViewerIds, ct);

        var candidates = new List<GuildInviteCandidateDto>(friendInfo.Friends.Count);
        foreach (var f in friendInfo.Friends)
        {
            candidates.Add(new GuildInviteCandidateDto
            {
                ViewerId    = f.ViewerId,
                Name        = f.Name,
                EmblemId    = f.EmblemId,
                CountryCode = f.CountryCode,
                Rank        = f.Rank,
                DegreeId    = f.DegreeId,
                IsJoinGuild = inGuild.Contains(f.ViewerId),
            });
        }

        return candidates;
    }

    [HttpPost("invite_user_list")]
    public async Task<ActionResult<GuildInviteUserListResponse>> InviteUserList([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var entries = await _guild.ListOutgoingInvitesAsync(viewerId, ct);
        return new GuildInviteUserListResponse
        {
            Users = entries.Select(e => new GuildOutgoingInviteDto
            {
                ViewerId = e.InviteeViewerId,
                Name = e.Name,
                EmblemId = e.EmblemId,
                CountryCode = e.CountryCode,
                Rank = e.Rank,
                DegreeId = e.DegreeId,
                InviteId = e.InviteId,
                InviteTime = new DateTimeOffset(e.CreatedAt, TimeSpan.Zero).ToUnixTimeSeconds(),
            }).ToList(),
        };
    }

    [HttpPost("invited_guild_list")]
    public async Task<ActionResult<GuildInvitedGuildListResponse>> InvitedGuildList([FromBody] GuildInvitedGuildListRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var entries = await _guild.ListInvitedGuildsAsync(viewerId, ct);
        return new GuildInvitedGuildListResponse
        {
            List = entries.Select(e => new GuildReceivedInviteDto
            {
                GuildId = e.Guild.GuildId,
                GuildName = e.Guild.Name,
                Description = e.Guild.Description,
                GuildEmblemId = e.Guild.EmblemId,
                JoinCondition = (int)e.Guild.JoinCondition,
                Activity = (int)e.Guild.Activity,
                MemberNum = e.MemberNum,
                LeaderName = e.LeaderName,
                LeaderViewerId = e.Guild.LeaderViewerId ?? 0L,
                InviteId = e.InviteId,
            }).ToList(),
        };
    }

    [HttpPost("invite")]
    public async Task<ActionResult<EmptyResponse>> Invite([FromBody] GuildInviteRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.InviteAsync(viewerId, req.InvitedViewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("cancel_invite")]
    public async Task<ActionResult<EmptyResponse>> CancelInvite([FromBody] GuildCancelInviteRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.CancelInviteAsync(viewerId, req.InviteId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("reject_invite")]
    public async Task<ActionResult<EmptyResponse>> RejectInvite([FromBody] GuildRejectInviteRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.RejectInviteAsync(viewerId, req.InviteId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("join")]
    public async Task<ActionResult<GuildJoinResponse>> Join([FromBody] GuildJoinEndpointRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.JoinAsync(viewerId, req.GuildId, ct);
        if (!r.IsOk) return WireError();
        // guild_status is branch-specific: JOINING (2) for instant joins, APPLYING (1) for approval path.
        // GuildJoinTask.Parse() reads this directly: GuildStatus = (eGUILD_STATUS)data["guild_status"].ToInt()
        return new GuildJoinResponse { GuildStatus = r.GuildStatus ?? 2 };
    }

    [HttpPost("cancel_join_request")]
    public async Task<ActionResult<EmptyResponse>> CancelJoinRequest([FromBody] GuildCancelJoinRequestRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.CancelJoinRequestAsync(viewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("join_request_list")]
    public async Task<ActionResult<GuildJoinRequestListResponse>> JoinRequestList([FromBody] GuildJoinRequestListRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var entries = await _guild.ListPendingJoinRequestsForMyGuildAsync(viewerId, ct);
        return new GuildJoinRequestListResponse
        {
            Users = entries.Select(e => new GuildJoinRequestEntryDto
            {
                ViewerId = e.ApplicantViewerId,
                Name = e.Name,
                EmblemId = e.EmblemId,
                CountryCode = e.CountryCode,
                Rank = e.Rank,
                DegreeId = e.DegreeId,
                IsOfficialMarkDisplayed = e.IsOfficialMarkDisplayed ? 1 : 0,
                RequestTime = new DateTimeOffset(e.RequestedAt, TimeSpan.Zero).ToUnixTimeSeconds(),
            }).ToList(),
        };
    }

    [HttpPost("join_request_accept")]
    public async Task<ActionResult<EmptyResponse>> JoinRequestAccept([FromBody] GuildJoinRequestAcceptRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.AcceptJoinRequestAsync(viewerId, req.RequestViewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("reject_join_request")]
    public async Task<ActionResult<EmptyResponse>> RejectJoinRequest([FromBody] GuildRejectJoinRequestRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.RejectJoinRequestAsync(viewerId, req.RequestViewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("leave")]
    public async Task<ActionResult<EmptyResponse>> Leave([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.LeaveAsync(viewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("remove")]
    public async Task<ActionResult<EmptyResponse>> Remove([FromBody] GuildRemoveRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.RemoveAsync(viewerId, req.RemoveViewerId, ct);
        return r.IsOk ? new EmptyResponse() : MapErrorToWire(r);
    }

    [HttpPost("change_role")]
    public async Task<ActionResult<GuildChangeRoleResponse>> ChangeRole([FromBody] GuildChangeRoleRequest req, CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();
        var r = await _guild.ChangeRoleAsync(viewerId, req.TargetViewerId, req.RoleId, ct);
        if (!r.IsOk) return WireError();

        // Load caller's membership to get the guild id, then return the full updated member list.
        var membership = await _members.GetMembershipAsync(viewerId, ct);
        if (membership is null) return WireError();

        var members = await _members.ListByGuildAsync(membership.GuildId, ct);

        var memberDtos = await ToMemberDtoListAsync(members, viewerId, ct);
        return new GuildChangeRoleResponse { Members = memberDtos };
    }

    // ===== Private helpers =====

    private static GuildDetailDto ToDetailDto(SVSim.Database.Entities.Guild.Guild guild, int memberCount, string leaderName = "") => new()
    {
        GuildId = guild.GuildId,
        GuildName = guild.Name,
        Description = guild.Description,
        GuildEmblemId = guild.EmblemId,
        JoinCondition = (int)guild.JoinCondition,
        Activity = (int)guild.Activity,
        MemberNum = memberCount,
        LeaderViewerId = guild.LeaderViewerId ?? 0L,
        LeaderName = leaderName,
    };

    private async Task<List<GuildMemberInfoDto>> ToMemberDtoListAsync(
        IReadOnlyList<GuildMember> members,
        long callerViewerId,
        CancellationToken ct)
    {
        if (members.Count == 0) return new();

        var viewerIds = members.Select(m => m.ViewerId).ToList();
        var profiles = await _viewers.LoadGuildProfileBatchAsync(viewerIds, ct);
        var relations = await _friends.GetFriendRelationsAsync(callerViewerId, viewerIds, ct);

        var result = new List<GuildMemberInfoDto>(members.Count);
        foreach (var m in members)
        {
            profiles.TryGetValue(m.ViewerId, out var p);
            relations.TryGetValue(m.ViewerId, out var r);
            result.Add(ToMemberDto(m, p, r));
        }
        return result;
    }

    private static GuildMemberInfoDto ToMemberDto(GuildMember member, GuildMemberProfile? profile, FriendRelation? relation) => new()
    {
        ViewerId = member.ViewerId,
        Name = profile?.Name ?? "",
        EmblemId = profile?.EmblemId ?? 100_000_000L,
        CountryCode = profile?.CountryCode ?? "",
        Rank = profile?.Rank ?? 1,
        DegreeId = profile?.DegreeId ?? 0,
        IsFriend = relation?.IsFriend == true ? 1 : 0,
        IsFriendApply = relation?.HasOutgoingApply == true ? 1 : 0,
        IsOfficialMarkDisplayed = profile?.IsOfficialMarkDisplayed == true ? 1 : 0,
        Role = (int)member.Role,
    };

    /// <summary>
    /// Maps a failed GuildOpResult to the wire error envelope convention used throughout the
    /// codebase (see CampaignController.Fail(), AchievementController, MissionController).
    /// Returns HTTP 200 with a JSON body carrying <c>result_code: 2</c>.
    /// </summary>
    private ActionResult<EmptyResponse> MapErrorToWire(GuildOpResult res)
        => Ok(new { result_code = GuildErrorResultCode });

    /// <summary>
    /// Returns a wire-error for endpoints that return a non-EmptyResponse type.
    /// Identical result_code=2 body, but typed as ObjectResult so it fits any TResult.
    /// </summary>
    private ObjectResult WireError() => Ok(new { result_code = GuildErrorResultCode });

    private static Task<ActionResult<EmptyResponse>> Stub() =>
        Task.FromResult<ActionResult<EmptyResponse>>(new EmptyResponse());
}
