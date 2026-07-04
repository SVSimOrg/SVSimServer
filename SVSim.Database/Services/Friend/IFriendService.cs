namespace SVSim.Database.Services.Friend;

public interface IFriendService
{
    Task<FriendInfoResult> GetFriendsAsync(long viewerId, CancellationToken ct);
    Task<ReceiveApplyInfoResult> GetReceiveAppliesAsync(long viewerId, CancellationToken ct);
    Task<SendApplyInfoResult> GetSendAppliesAsync(long viewerId, CancellationToken ct);
    Task<PlayedTogetherResult> GetPlayedTogetherAsync(long viewerId, CancellationToken ct);

    /// <summary>Returns null when not found, self-search, or any error.</summary>
    Task<FriendEntry?> SearchAsync(long viewerId, int targetViewerId, CancellationToken ct);

    /// <summary>No-op if target missing, self, already friends, already-pending apply, or at outgoing-apply cap.</summary>
    Task SendApplyAsync(long viewerId, int targetViewerId, CancellationToken ct);

    /// <summary>No-op if apply not addressed to caller, would push either side past friend cap. Cleans reverse-direction apply if present.</summary>
    Task ApproveApplyAsync(long viewerId, int applyId, CancellationToken ct);

    /// <summary>No-op if apply not addressed to caller.</summary>
    Task RejectApplyAsync(long viewerId, int applyId, CancellationToken ct);

    /// <summary>No-op if apply not sent by caller.</summary>
    Task CancelApplyAsync(long viewerId, int applyId, CancellationToken ct);

    Task RejectAllAppliesAsync(long viewerId, CancellationToken ct);
    Task CancelAllAppliesAsync(long viewerId, CancellationToken ct);

    /// <summary>Deletes both directions of the friendship (A→B and B→A).</summary>
    Task RejectFriendAsync(long viewerId, int targetViewerId, CancellationToken ct);

    /// <summary>
    /// Batched caller-relative friend-relation lookup. For each id in <paramref name="otherViewerIds"/>
    /// returns whether the caller is already friends with them and/or has an outgoing pending apply
    /// to them. Self and unknown ids resolve to <c>(false, false)</c>.
    /// </summary>
    Task<IReadOnlyDictionary<long, FriendRelation>> GetFriendRelationsAsync(
        long viewerId, IReadOnlyList<long> otherViewerIds, CancellationToken ct);
}
