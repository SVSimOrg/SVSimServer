using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Guild;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;

namespace SVSim.Database.Services.Guild;

public sealed class GuildService : IGuildService
{
    private readonly IGuildRepository _guilds;
    private readonly IGuildMemberRepository _members;
    private readonly IGuildInviteRepository _invites;
    private readonly IGuildJoinRequestRepository _joinRequests;
    private readonly IGuildChatMessageRepository _chatMessages;
    private readonly IGameConfigService _config;
    private readonly IGuildIdGenerator _idGen;
    private readonly IGuildChatService _chat;
    private readonly IViewerRepository _viewers;
    private readonly SVSimDbContext _db;

    public GuildService(
        IGuildRepository guilds,
        IGuildMemberRepository members,
        IGuildInviteRepository invites,
        IGuildJoinRequestRepository joinRequests,
        IGuildChatMessageRepository chatMessages,
        IGameConfigService config,
        IGuildIdGenerator idGen,
        IGuildChatService chat,
        IViewerRepository viewers,
        SVSimDbContext db)
    {
        _guilds = guilds;
        _members = members;
        _invites = invites;
        _joinRequests = joinRequests;
        _chatMessages = chatMessages;
        _config = config;
        _idGen = idGen;
        _chat = chat;
        _viewers = viewers;
        _db = db;
    }

    public async Task<GuildFullView?> GetMyGuildAsync(long viewerId, CancellationToken ct = default)
    {
        var membership = await _members.GetMembershipAsync(viewerId, ct);
        if (membership is null) return null;

        var guild = await _guilds.GetWithMembersAsync(membership.GuildId, ct);
        if (guild is null || guild.BreakupAt is not null) return null;

        int joinReqCount = await _joinRequests.CountPendingForGuildAsync(guild.GuildId, ct);
        int inviteCount = await _invites.CountPendingForInviteeAsync(viewerId, ct);

        return new GuildFullView(guild, guild.Members, joinReqCount, inviteCount);
    }

    public Task<Entities.Guild.Guild?> GetActiveAsync(int guildId, CancellationToken ct = default)
        => _guilds.GetActiveByIdAsync(guildId, ct);

    public async Task<IReadOnlyList<GuildSearchEntry>> SearchAsync(string name, int activity, int joinCondition, int memberBucket, CancellationToken ct = default)
    {
        var cfg = _config.Get<GuildConfig>();
        var rows = await _guilds.SearchAsync(name ?? "", activity, joinCondition, memberBucket, cfg.MaxMemberNum, cfg.SearchResultCap, ct);
        var guildIds = rows.Select(r => r.GuildId).ToList();
        var memberCounts = await _members.CountBatchByGuildIdsAsync(guildIds, ct);
        var leaderIds = rows.Select(r => r.LeaderViewerId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var leaderNames = await _viewers.LoadDisplayNamesAsync(leaderIds, ct);
        return rows.Select(r => new GuildSearchEntry(r, memberCounts.GetValueOrDefault(r.GuildId, 0), leaderNames.GetValueOrDefault(r.LeaderViewerId ?? 0L, ""))).ToList();
    }

    public async Task<GuildOpResult> CreateAsync(long viewerId, CreateGuildRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 64)
            return new(GuildOpResultCode.NameInvalid);
        if (req.Activity is < 1 or > 16)
            return new(GuildOpResultCode.NameInvalid);
        if (req.JoinCondition is < 1 or > 3)
            return new(GuildOpResultCode.NameInvalid);

        if (await _members.GetMembershipAsync(viewerId, ct) is not null)
            return new(GuildOpResultCode.AlreadyInGuild);
        if (await _guilds.NameExistsAsync(req.Name, ct))
            return new(GuildOpResultCode.NameTaken);

        // Load the viewer's equipped emblem to use as the guild's initial emblem.
        // If the viewer doesn't exist (shouldn't happen post-auth), returns 100_000_000 default.
        long viewerEmblemId = await _viewers.GetEquippedEmblemIdAsync(viewerId, ct);

        var id = await _idGen.NextAsync(
            (cand, c) => _guilds.GetByIdAsync(cand, c).ContinueWith(t => t.Result is not null, c),
            ct);
        var now = DateTime.UtcNow;
        var g = new Entities.Guild.Guild
        {
            GuildId = id,
            Name = req.Name,
            Description = "",
            LeaderViewerId = viewerId,
            EmblemId = viewerEmblemId,
            Activity = (GuildActivity)req.Activity,
            JoinCondition = (GuildJoinCondition)req.JoinCondition,
            CreatedAt = now,
        };
        await _guilds.AddAsync(g, ct);
        await _members.AddAsync(
            new GuildMember { GuildId = id, ViewerId = viewerId, Role = GuildRole.Leader, JoinedAt = now },
            ct);

        // Side-effects: clear any pending invites + cancel pending join requests for this viewer.
        await _invites.ConsumePendingForViewerAsync(viewerId, now, ct);
        await _joinRequests.CancelPendingForViewerAsync(viewerId, now, ct);

        // Update Viewer.GuildId pointer for quick lookups.
        await _viewers.SetGuildIdAsync(viewerId, id, ct);

        await _chat.EmitSystemEventAsync(id, viewerId, GuildChatMessageType.CreateGuild, body: null, ct);
        return new(GuildOpResultCode.Ok, GuildId: id);
    }

    public async Task<GuildOpResult> UpdateAsync(long viewerId, UpdateGuildRequest req, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);
        if (m.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);

        if (req.Name is not null && (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length > 64))
            return new(GuildOpResultCode.NameInvalid);
        if (req.Activity.HasValue && req.Activity.Value is < 1 or > 16)
            return new(GuildOpResultCode.NameInvalid);
        if (req.JoinCondition.HasValue && req.JoinCondition.Value is < 1 or > 3)
            return new(GuildOpResultCode.NameInvalid);

        await _guilds.UpdateActivityAndJoinConditionAsync(m.GuildId, req.Activity, req.JoinCondition, req.Name, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> UpdateDescriptionAsync(long viewerId, string description, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);
        if (m.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);
        if (description.Length > 512) return new(GuildOpResultCode.NameInvalid);

        await _guilds.UpdateDescriptionAsync(m.GuildId, description, ct);
        await _chat.EmitSystemEventAsync(m.GuildId, viewerId, GuildChatMessageType.Description, body: null, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> UpdateEmblemAsync(long viewerId, long emblemId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);
        if (m.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);

        await _guilds.UpdateEmblemAsync(m.GuildId, emblemId, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> BreakupAsync(long viewerId, CancellationToken ct = default)
    {
        var membership = await _members.GetMembershipAsync(viewerId, ct);
        if (membership is null) return new(GuildOpResultCode.NotInGuild);
        if (membership.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);

        IDbContextTransaction? tx = null;
        try
        {
            try { tx = await _db.Database.BeginTransactionAsync(ct); } catch (InvalidOperationException) { /* InMemory provider */ }

            var now = DateTime.UtcNow;
            var members = await _members.ListByGuildAsync(membership.GuildId, ct);
            foreach (var m in members) await _viewers.ClearGuildIdAsync(m.ViewerId, ct);

            await _chatMessages.DeleteAllForGuildAsync(membership.GuildId, ct);
            await _invites.DeleteAllForGuildAsync(membership.GuildId, ct);
            await _joinRequests.DeleteAllForGuildAsync(membership.GuildId, ct);
            await _members.DeleteAllForGuildAsync(membership.GuildId, ct);
            await _guilds.MarkBrokenUpAsync(membership.GuildId, now, ct);

            if (tx != null) await tx.CommitAsync(ct);
            return GuildOpResult.Ok;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
        finally
        {
            tx?.Dispose();
        }
    }

    public async Task<GuildOpResult> InviteAsync(long callerViewerId, long targetViewerId, CancellationToken ct = default)
    {
        // Caller must be a guild member with Leader or SubLeader role.
        var callerMembership = await _members.GetMembershipAsync(callerViewerId, ct);
        if (callerMembership is null) return new(GuildOpResultCode.NotInGuild);
        if (callerMembership.Role is not (GuildRole.Leader or GuildRole.SubLeader))
            return new(GuildOpResultCode.PermissionDenied);

        // Target must not already be in any guild.
        var targetMembership = await _members.GetMembershipAsync(targetViewerId, ct);
        if (targetMembership is not null) return new(GuildOpResultCode.AlreadyInGuild);

        // No duplicate pending invite to the same target from the same guild.
        var existing = await _invites.GetAsync(callerMembership.GuildId, targetViewerId, ct);
        if (existing is { Status: GuildInviteStatus.Pending }) return new(GuildOpResultCode.InviteAlreadyResolved);

        var now = DateTime.UtcNow;
        await _invites.AddAsync(new GuildInvite
        {
            GuildId = callerMembership.GuildId,
            InviteeViewerId = targetViewerId,
            InviterViewerId = callerViewerId,
            Status = GuildInviteStatus.Pending,
            CreatedAt = now,
        }, ct);

        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> CancelInviteAsync(long callerViewerId, long inviteId, CancellationToken ct = default)
    {
        // Caller must be in their guild with Leader or SubLeader role.
        var callerMembership = await _members.GetMembershipAsync(callerViewerId, ct);
        if (callerMembership is null) return new(GuildOpResultCode.NotInGuild);
        if (callerMembership.Role is not (GuildRole.Leader or GuildRole.SubLeader))
            return new(GuildOpResultCode.PermissionDenied);

        var invite = await _invites.GetByIdAsync(inviteId, ct);
        if (invite is null) return new(GuildOpResultCode.InviteNotFound);
        // Caller must belong to the same guild as the invite.
        if (invite.GuildId != callerMembership.GuildId) return new(GuildOpResultCode.PermissionDenied);
        if (invite.Status != GuildInviteStatus.Pending) return new(GuildOpResultCode.InviteAlreadyResolved);

        await _invites.UpdateStatusAsync(invite.GuildId, invite.InviteeViewerId, GuildInviteStatus.Canceled, DateTime.UtcNow, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> RejectInviteAsync(long callerViewerId, long inviteId, CancellationToken ct = default)
    {
        // Invitee rejects — no guild membership check needed.
        var invite = await _invites.GetByIdAsync(inviteId, ct);
        if (invite is null) return new(GuildOpResultCode.InviteNotFound);
        // Caller must be the invitee.
        if (invite.InviteeViewerId != callerViewerId) return new(GuildOpResultCode.PermissionDenied);
        if (invite.Status != GuildInviteStatus.Pending) return new(GuildOpResultCode.InviteAlreadyResolved);

        await _invites.UpdateStatusAsync(invite.GuildId, callerViewerId, GuildInviteStatus.Rejected, DateTime.UtcNow, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> JoinAsync(long viewerId, int guildId, CancellationToken ct = default)
    {
        if (await _members.GetMembershipAsync(viewerId, ct) is not null)
            return new(GuildOpResultCode.AlreadyInGuild);

        var g = await _guilds.GetActiveByIdAsync(guildId, ct);
        if (g is null) return new(GuildOpResultCode.GuildNotFound);

        var cfg = _config.Get<GuildConfig>();
        var memberCount = await _members.CountByGuildAsync(guildId, ct);
        if (memberCount >= cfg.MaxMemberNum) return new(GuildOpResultCode.MemberCapReached);

        var now = DateTime.UtcNow;
        bool hasPendingInvite = await _invites.GetAsync(guildId, viewerId, ct)
            is { Status: GuildInviteStatus.Pending };

        if (g.JoinCondition == GuildJoinCondition.OnlyInvite && !hasPendingInvite)
            return new(GuildOpResultCode.PermissionDenied);

        if (g.JoinCondition == GuildJoinCondition.Approval && !hasPendingInvite)
        {
            var existing = await _joinRequests.GetAsync(guildId, viewerId, ct);
            if (existing is { Status: GuildJoinRequestStatus.Pending })
            {
                // Already pending — idempotent, no-op.
                return new(GuildOpResultCode.Ok, GuildStatus: 1); // APPLYING
            }

            if (existing is not null)
            {
                // Row exists but was previously Canceled/Rejected — reset to Pending in place.
                existing.Status = GuildJoinRequestStatus.Pending;
                existing.CreatedAt = now;
                existing.RespondedAt = null;
                await _db.SaveChangesAsync(ct);
            }
            else
            {
                await _joinRequests.AddAsync(new GuildJoinRequest
                {
                    GuildId = guildId,
                    ViewerId = viewerId,
                    Status = GuildJoinRequestStatus.Pending,
                    CreatedAt = now,
                }, ct);
            }
            return new(GuildOpResultCode.Ok, GuildStatus: 1); // APPLYING
        }

        await CommitJoinAsync(viewerId, guildId, now, ct);
        return new(GuildOpResultCode.Ok, GuildStatus: 2); // JOINING
    }

    private async Task CommitJoinAsync(long viewerId, int guildId, DateTime now, CancellationToken ct)
    {
        IDbContextTransaction? tx = null;
        try
        {
            try { tx = await _db.Database.BeginTransactionAsync(ct); } catch (InvalidOperationException) { /* InMemory provider */ }

            await _members.AddAsync(
                new GuildMember { GuildId = guildId, ViewerId = viewerId, Role = GuildRole.Regular, JoinedAt = now },
                ct);
            await _viewers.SetGuildIdAsync(viewerId, guildId, ct);
            await _invites.ConsumePendingForViewerAsync(viewerId, now, ct);
            await _joinRequests.CancelPendingForViewerAsync(viewerId, now, ct);

            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
        finally
        {
            tx?.Dispose();
        }

        // Emit chat event outside the transaction — chat failure doesn't roll back the join.
        await _chat.EmitSystemEventAsync(guildId, viewerId, GuildChatMessageType.Join, body: null, ct);
    }

    public async Task<GuildOpResult> CancelJoinRequestAsync(long viewerId, CancellationToken ct = default)
    {
        // Cancel all pending requests for this viewer (client sends no guildId — see decompile).
        var pending = await _joinRequests.ListPendingForViewerAsync(viewerId, ct);
        if (pending.Count == 0) return new(GuildOpResultCode.JoinRequestNotFound);

        var now = DateTime.UtcNow;
        foreach (var req in pending)
            await _joinRequests.UpdateStatusAsync(req.GuildId, viewerId, GuildJoinRequestStatus.Canceled, now, ct);

        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> AcceptJoinRequestAsync(long callerViewerId, long applicantViewerId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(callerViewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);
        if (m.Role is not (GuildRole.Leader or GuildRole.SubLeader)) return new(GuildOpResultCode.PermissionDenied);

        var req = await _joinRequests.GetAsync(m.GuildId, applicantViewerId, ct);
        if (req is null) return new(GuildOpResultCode.JoinRequestNotFound);
        if (req.Status != GuildJoinRequestStatus.Pending) return new(GuildOpResultCode.JoinRequestAlreadyResolved);

        var cfg = _config.Get<GuildConfig>();
        var memberCount = await _members.CountByGuildAsync(m.GuildId, ct);
        if (memberCount >= cfg.MaxMemberNum) return new(GuildOpResultCode.MemberCapReached);

        var now = DateTime.UtcNow;
        await _joinRequests.UpdateStatusAsync(m.GuildId, applicantViewerId, GuildJoinRequestStatus.Accepted, now, ct);
        await CommitJoinAsync(applicantViewerId, m.GuildId, now, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> RejectJoinRequestAsync(long callerViewerId, long applicantViewerId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(callerViewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);
        if (m.Role is not (GuildRole.Leader or GuildRole.SubLeader)) return new(GuildOpResultCode.PermissionDenied);

        var req = await _joinRequests.GetAsync(m.GuildId, applicantViewerId, ct);
        if (req is null) return new(GuildOpResultCode.JoinRequestNotFound);
        if (req.Status != GuildJoinRequestStatus.Pending) return new(GuildOpResultCode.JoinRequestAlreadyResolved);

        var now = DateTime.UtcNow;
        await _joinRequests.UpdateStatusAsync(m.GuildId, applicantViewerId, GuildJoinRequestStatus.Rejected, now, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> LeaveAsync(long viewerId, CancellationToken ct = default)
    {
        var m = await _members.GetMembershipAsync(viewerId, ct);
        if (m is null) return new(GuildOpResultCode.NotInGuild);

        var memberCount = await _members.CountByGuildAsync(m.GuildId, ct);

        if (m.Role == GuildRole.Leader)
        {
            // Sole-member leader: auto-breakup.
            if (memberCount == 1) return await BreakupAsync(viewerId, ct);
            // Leader with remaining members: blocked.
            return new(GuildOpResultCode.LeaderLeaveBlocked);
        }

        // Regular or SubLeader: remove self.
        await _members.RemoveAsync(m.GuildId, viewerId, ct);
        await _viewers.ClearGuildIdAsync(viewerId, ct);
        await _chat.EmitSystemEventAsync(m.GuildId, viewerId, GuildChatMessageType.Leave, body: null, ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> RemoveAsync(long callerViewerId, long targetViewerId, CancellationToken ct = default)
    {
        var caller = await _members.GetMembershipAsync(callerViewerId, ct);
        if (caller is null) return new(GuildOpResultCode.NotInGuild);
        // Only Leader may kick (GuildManager.cs has no HasAuthorityRemove — client has no SubLeader kick UI).
        if (caller.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);
        // Cannot kick yourself.
        if (callerViewerId == targetViewerId) return new(GuildOpResultCode.PermissionDenied);

        var target = await _members.GetMembershipAsync(targetViewerId, ct);
        if (target is null || target.GuildId != caller.GuildId) return new(GuildOpResultCode.TargetNotInGuild);

        await _members.RemoveAsync(caller.GuildId, targetViewerId, ct);
        await _viewers.ClearGuildIdAsync(targetViewerId, ct);
        await _chat.EmitSystemEventAsync(caller.GuildId, callerViewerId, GuildChatMessageType.Remove,
            body: targetViewerId.ToString(), ct);
        return GuildOpResult.Ok;
    }

    public async Task<GuildOpResult> ChangeRoleAsync(long callerViewerId, long targetViewerId, int newRoleId, CancellationToken ct = default)
    {
        if (newRoleId is < 0 or > 2) return new(GuildOpResultCode.InvalidRoleTransition);
        var newRole = (GuildRole)newRoleId;

        var caller = await _members.GetMembershipAsync(callerViewerId, ct);
        if (caller is null) return new(GuildOpResultCode.NotInGuild);
        if (caller.Role != GuildRole.Leader) return new(GuildOpResultCode.PermissionDenied);

        var target = await _members.GetMembershipAsync(targetViewerId, ct);
        if (target is null || target.GuildId != caller.GuildId) return new(GuildOpResultCode.TargetNotInGuild);

        // No-op: same role.
        if (target.Role == newRole) return GuildOpResult.Ok;

        // SubLeader cap check.
        if (newRole == GuildRole.SubLeader)
        {
            var subs = await _members.CountByGuildAndRoleAsync(caller.GuildId, GuildRole.SubLeader, ct);
            var cfg = _config.Get<GuildConfig>();
            if (subs >= cfg.MaxSubLeaderNum) return new(GuildOpResultCode.SubLeaderCapReached);
        }

        if (newRole == GuildRole.Leader)
        {
            // Atomic leader transfer: load both member rows and the guild row, mutate all
            // in one scope, then save once — no individual repository SaveChanges calls.
            var guildRow = await _db.Guilds.FirstOrDefaultAsync(g => g.GuildId == caller.GuildId, ct);
            var targetMember = await _db.GuildMembers
                .FirstOrDefaultAsync(m => m.GuildId == caller.GuildId && m.ViewerId == targetViewerId, ct);
            var callerMember = await _db.GuildMembers
                .FirstOrDefaultAsync(m => m.GuildId == caller.GuildId && m.ViewerId == callerViewerId, ct);

            if (guildRow is null || targetMember is null || callerMember is null)
                return new(GuildOpResultCode.TargetNotInGuild);

            targetMember.Role = GuildRole.Leader;
            callerMember.Role = GuildRole.Regular;
            guildRow.LeaderViewerId = targetViewerId;

            await _db.SaveChangesAsync(ct);
            await _chat.EmitSystemEventAsync(caller.GuildId, callerViewerId, GuildChatMessageType.ChangeLeader,
                body: targetViewerId.ToString(), ct);
        }
        else
        {
            await _members.UpdateRoleAsync(caller.GuildId, targetViewerId, newRole, ct);
            if (newRole == GuildRole.SubLeader)
                await _chat.EmitSystemEventAsync(caller.GuildId, callerViewerId, GuildChatMessageType.ChangeSubLeader,
                    body: targetViewerId.ToString(), ct);
        }

        return GuildOpResult.Ok;
    }

    public Task<IReadOnlyList<Entities.Guild.GuildInvite>> ListPendingInvitesForMeAsync(long viewerId, CancellationToken ct = default)
        => _invites.ListPendingForInviteeAsync(viewerId, ct);

    public async Task<IReadOnlyList<GuildOutgoingInviteEntry>> ListOutgoingInvitesAsync(long callerViewerId, CancellationToken ct = default)
    {
        var membership = await _members.GetMembershipAsync(callerViewerId, ct);
        if (membership is null) return Array.Empty<GuildOutgoingInviteEntry>();

        var pendingInvites = await _invites.ListPendingForGuildAsync(membership.GuildId, ct);
        if (pendingInvites.Count == 0) return Array.Empty<GuildOutgoingInviteEntry>();

        var inviteeIds = pendingInvites.Select(i => i.InviteeViewerId).Distinct().ToList();
        var profiles = await _viewers.LoadGuildProfileBatchAsync(inviteeIds, ct);

        var result = new List<GuildOutgoingInviteEntry>(pendingInvites.Count);
        foreach (var invite in pendingInvites)
        {
            profiles.TryGetValue(invite.InviteeViewerId, out var p);
            result.Add(new GuildOutgoingInviteEntry(
                InviteId: invite.Id,
                InviteeViewerId: invite.InviteeViewerId,
                Name: p?.Name ?? "",
                EmblemId: p?.EmblemId ?? 100_000_000L,
                CountryCode: p?.CountryCode ?? "",
                Rank: p?.Rank ?? 1,
                DegreeId: p?.DegreeId ?? 0,
                CreatedAt: invite.CreatedAt));
        }
        return result;
    }

    public async Task<IReadOnlyList<GuildReceivedInviteEntry>> ListInvitedGuildsAsync(long viewerId, CancellationToken ct = default)
    {
        // ListPendingForInviteeAsync already includes the Guild nav prop.
        var pendingInvites = await _invites.ListPendingForInviteeAsync(viewerId, ct);
        if (pendingInvites.Count == 0) return Array.Empty<GuildReceivedInviteEntry>();

        var guildIds = pendingInvites.Select(i => i.GuildId).Distinct().ToList();
        var memberCounts = await _members.CountBatchByGuildIdsAsync(guildIds, ct);
        var leaderIds = pendingInvites.Select(i => i.Guild.LeaderViewerId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        var leaderNames = await _viewers.LoadDisplayNamesAsync(leaderIds, ct);

        return pendingInvites.Select(invite => new GuildReceivedInviteEntry(
            InviteId: invite.Id,
            Guild: invite.Guild,
            MemberNum: memberCounts.GetValueOrDefault(invite.GuildId, 0),
            LeaderName: leaderNames.GetValueOrDefault(invite.Guild.LeaderViewerId ?? 0L, ""))).ToList();
    }

    public async Task<IReadOnlyList<GuildJoinRequestEntry>> ListPendingJoinRequestsForMyGuildAsync(long viewerId, CancellationToken ct = default)
    {
        var membership = await _members.GetMembershipAsync(viewerId, ct);
        if (membership is null) return Array.Empty<GuildJoinRequestEntry>();
        // Only leader/subleader may view the list — return empty for regular members.
        if (membership.Role is not (GuildRole.Leader or GuildRole.SubLeader))
            return Array.Empty<GuildJoinRequestEntry>();

        var requests = await _joinRequests.ListPendingForGuildAsync(membership.GuildId, ct);
        if (requests.Count == 0) return Array.Empty<GuildJoinRequestEntry>();

        var applicantIds = requests.Select(r => r.ViewerId).Distinct().ToList();
        var profiles = await _viewers.LoadGuildProfileBatchAsync(applicantIds, ct);

        return requests.Select(r =>
        {
            profiles.TryGetValue(r.ViewerId, out var p);
            return new GuildJoinRequestEntry(
                ApplicantViewerId: r.ViewerId,
                Name: p?.Name ?? "",
                EmblemId: p?.EmblemId ?? 100_000_000L,
                CountryCode: p?.CountryCode ?? "",
                Rank: p?.Rank ?? 1,
                DegreeId: p?.DegreeId ?? 0,
                IsOfficialMarkDisplayed: p?.IsOfficialMarkDisplayed ?? false,
                RequestedAt: r.CreatedAt);
        }).ToList();
    }
}
