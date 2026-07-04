namespace SVSim.Database.Services.Friend;

/// <summary>
/// Records a recent-opponent entry on the owner viewer. Upserts the (owner, opponent)
/// row to PlayedAt = now, enforces a 50-row per-viewer retention cap by deleting the
/// owner's oldest row when at cap. No-op if owner equals opponent.
/// </summary>
public interface IPlayedTogetherWriter
{
    Task RecordAsync(long ownerViewerId, long opponentViewerId, BattleParticipationContext ctx, CancellationToken ct);
}
