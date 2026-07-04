using SVSim.Database.Enums;

namespace SVSim.Database.Repositories.Viewer;

/// <summary>
/// Lightweight profile shape used by chat surfaces (guild_chat, gathering_chat).
/// Fields match the ChatUserDto wire shape.
/// </summary>
public record ChatUserProfile(
    string Name,
    long   EmblemId,
    string CountryCode,
    int    Rank,
    int    DegreeId);

/// <summary>
/// Richer profile shape used by guild management surfaces (invite list, join-request list).
/// Extends <see cref="ChatUserProfile"/> with <c>IsOfficialMarkDisplayed</c>.
/// </summary>
public record GuildMemberProfile(
    string Name,
    long   EmblemId,
    string CountryCode,
    int    Rank,
    int    DegreeId,
    bool   IsOfficialMarkDisplayed);

public interface IViewerRepository
{
    Task<Models.Viewer?> GetViewerBySocialConnection(SocialAccountType accountType, ulong socialId);
    Task<Models.Viewer?> GetViewerWithSocials(long id);
    Task<Models.Viewer?> GetViewerByShortUdid(long shortUdid);
    Task<Models.Viewer?> GetViewerByUdid(Guid udid);

    Task<Models.Viewer> RegisterViewer(string displayName, SocialAccountType socialType,
        ulong socialAccountIdentifier, ulong? shortUdid = null);
    Task<Models.Viewer> RegisterAnonymousViewer(Guid udid);
    Task LinkSteamToViewer(long viewerId, ulong steamId);

    /// <summary>
    /// Merges an anonymous viewer (just created by <c>/tool/signup</c> on a fresh UDID)
    /// into a target viewer that the Steam ticket resolved to. Transfers the anonymous
    /// viewer's UDID to the target, then deletes the anonymous viewer.
    /// </summary>
    Task MergeAnonymousViewerInto(long anonymousViewerId, long targetViewerId);

    /// <summary>
    /// Focused load for building a battle-node <c>MatchContext</c>: viewer + Info + Info's
    /// equipped Emblem/Degree nav refs. Read-only (AsNoTracking). Returns null if the viewer
    /// doesn't exist.
    /// </summary>
    Task<Models.Viewer?> LoadForMatchContextAsync(long viewerId);

    /// <summary>
    /// Focused load for class-XP grants: viewer with owned <c>Classes</c> collection and
    /// each <c>ViewerClassData.Class</c> nav ref included. Tracked (not AsNoTracking) so
    /// the caller can mutate <c>Exp</c>/<c>Level</c> and <c>SaveChangesAsync</c>. Returns
    /// null if the viewer does not exist.
    /// </summary>
    Task<Models.Viewer?> LoadForBattleXpGrantAsync(long viewerId, CancellationToken ct = default);

    /// <summary>
    /// Load a viewer with the joins <see cref="Controllers.RankBattleController"/>'s <c>Finish</c>
    /// needs: Classes (for the class-XP grant) + RankProgress (for the rank grant). Split-query
    /// per <c>project_ef_split_query</c> to avoid the cartesian explosion when both nav
    /// collections are populated on the same viewer.
    /// </summary>
    Task<Models.Viewer?> LoadForRankProgressAsync(long viewerId, CancellationToken ct = default);

    /// <summary>Sets Viewer.GuildId to <paramref name="guildId"/>. No-op if the viewer does not exist.</summary>
    Task SetGuildIdAsync(long viewerId, int guildId, CancellationToken ct = default);

    /// <summary>Clears Viewer.GuildId to null. No-op if the viewer does not exist.</summary>
    Task ClearGuildIdAsync(long viewerId, CancellationToken ct = default);

    /// <summary>
    /// Batch-loads <c>DisplayName</c> for a set of viewer ids. Returns a dictionary keyed by viewer id;
    /// ids with no matching row are absent from the result. Read-only (AsNoTracking).
    /// </summary>
    Task<Dictionary<long, string>> LoadDisplayNamesAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default);

    /// <summary>
    /// Batch-loads the <see cref="ChatUserProfile"/> fields for a set of viewer ids. Used by chat
    /// surfaces (guild_chat, gathering_chat) to populate <c>users[]</c> without a direct DB context
    /// in the controller. Ids with no matching row are absent from the result. Read-only (AsNoTracking).
    /// </summary>
    Task<IReadOnlyDictionary<long, ChatUserProfile>> LoadChatProfilesAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default);

    /// <summary>
    /// Batch-loads the <see cref="GuildMemberProfile"/> fields (emblem, degree,
    /// <c>IsOfficialMarkDisplayed</c>) for a set of viewer ids. Used by guild management surfaces
    /// (invite_user_list, join_request_list). Ids with no matching row are absent. Read-only.
    /// </summary>
    Task<IReadOnlyDictionary<long, GuildMemberProfile>> LoadGuildProfileBatchAsync(IReadOnlyCollection<long> viewerIds, CancellationToken ct = default);

    /// <summary>
    /// Loads a viewer's currently-equipped emblem id, or <c>100_000_000</c> (default) if none.
    /// Used by <see cref="SVSim.Database.Services.Guild.GuildService"/> to seed the guild's initial emblem.
    /// Returns <c>100_000_000</c> if the viewer doesn't exist.
    /// </summary>
    Task<long> GetEquippedEmblemIdAsync(long viewerId, CancellationToken ct = default);

    /// <summary>
    /// Batch-loads owned guild-emblem ids for a viewer. Used by <c>/guild/emblem_list</c>.
    /// </summary>
    Task<List<long>> GetEmblemListAsync(long viewerId, CancellationToken ct = default);

    /// <summary>
    /// Counts the viewer's unclaimed <see cref="Models.ViewerPresent"/> rows. Drives
    /// <c>/mypage/index.unread_present_count</c>, which the client casts to
    /// <c>Data.MyPage.data.unread_mail_count</c> to render the home-screen crate badge
    /// (MyPageItemHome.cs:148 → SetUnreadGiftCount).
    /// </summary>
    Task<int> CountUnclaimedPresentsAsync(long viewerId, CancellationToken ct = default);
}
