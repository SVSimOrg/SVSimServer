namespace SVSim.Database.Services.Friend;

/// <summary>
/// One friend in the requested viewer's friend list. Wire shape carries 15 fields;
/// most are cosmetic ints emitted as strings (matches prod). Numeric fields
/// (viewer_id, rank, emblem_id, degree_id) ship as native ints.
/// </summary>
public sealed record FriendEntry(
    int    ViewerId,
    string Name,
    string CountryCode,
    int    Rank,
    long   EmblemId,
    int    DegreeId,
    string LastPlayTime,        // "yyyy-MM-dd HH:mm:ss"
    string DeviceType,
    string MaxFriend,
    string IsReceivedTwoPickMission,
    string Birth,
    string MissionChangeTime,
    string MissionReceiveType,
    string IsOfficial,
    string IsOfficialMarkDisplayed);

/// <summary>
/// One friend apply (sent or received). Wire field <c>id</c> is the apply's PK.
/// </summary>
public sealed record FriendApplyEntry(
    int    Id,
    int    ViewerId,             // OTHER viewer's id
    string Name,
    string CountryCode,
    int    Rank,
    long   EmblemId,
    int    DegreeId,
    string LastPlayTime,
    string CreateTime,
    int    MissionType);          // 0 when omitted on the wire

/// <summary>
/// One recent-opponent row. <see cref="FriendStatus"/> is computed at read time:
///   0 = NO_ACTION, 1 = IS_FRIEND, 2 = IS_SEND (caller has outgoing apply),
///   3 = IS_RECEIVED (caller has incoming apply from opponent).
/// <see cref="FriendApplyId"/> is the relevant apply's PK when status is 2 or 3, else 0.
/// </summary>
public sealed record PlayedTogetherEntry(
    int    ViewerId,
    string Name,
    string CountryCode,
    int    Rank,
    long   EmblemId,
    int    DegreeId,
    string LastPlayTime,
    string PlayedTime,
    int    FriendStatus,
    int    FriendApplyId,
    int    PlayedMode,
    int    BattleType,
    int    DeckFormat,
    int    TwoPickType);

public sealed record FriendInfoResult(
    IReadOnlyList<FriendEntry> Friends,
    int Count,
    int MaxCount);

public sealed record ReceiveApplyInfoResult(
    IReadOnlyList<FriendApplyEntry> ReceiveApplies,
    int ApproveApplyCount);

public sealed record SendApplyInfoResult(
    IReadOnlyList<FriendApplyEntry> SendApplies,
    int RemainingApplyCount,
    int SendApplyMaxCount);

public sealed record PlayedTogetherResult(
    IReadOnlyList<PlayedTogetherEntry> Histories);

/// <summary>Context recorded by <see cref="IPlayedTogetherWriter.RecordAsync"/>.</summary>
public sealed record BattleParticipationContext(
    int PlayedMode,
    int BattleType,
    int DeckFormat,
    int TwoPickType);

/// <summary>
/// Caller-relative friend flags for another viewer. Used to badge rows in guild-member,
/// invite-candidate, and join-request lists on the wire (<c>is_friend</c>, <c>is_friend_apply</c>).
/// </summary>
public sealed record FriendRelation(bool IsFriend, bool HasOutgoingApply);
