namespace SVSim.Database.Repositories.Card;

/// <summary>
/// Mutating operations on a viewer's card inventory (destruct, create, protect…).
/// Read-only catalog queries live on <see cref="ICardRepository"/>.
/// </summary>
public interface ICardInventoryRepository
{
    /// <summary>
    /// Validate-then-mutate destruct of owned cards. Atomic: all validation runs before any
    /// mutation, and the mutation phase is wrapped in an explicit DB transaction so a mid-flight
    /// EF failure rolls back currency + inventory + deck-strip together.
    /// </summary>
    /// <param name="viewerId">Authenticated viewer.</param>
    /// <param name="destructCounts">cardId → num_to_destruct. Empty dict is rejected by the caller.</param>
    /// <returns>
    ///   <see cref="DestructResult"/> with post-state totals on success, or a
    ///   <see cref="DestructError"/> when validation fails. On error nothing is written.
    /// </returns>
    Task<DestructOutcome> DestructCards(long viewerId, IReadOnlyDictionary<long, int> destructCounts);

    /// <summary>
    /// Validate-then-mutate craft of cards from RedEther. Atomic in a transaction; on validation
    /// failure nothing is written. Routes Card grants through <see cref="Services.RewardGrantService.ApplyAsync"/>
    /// so the CardCosmeticReward cascade fires for first-time owners.
    /// </summary>
    /// <param name="createCounts">cardId → num_to_create. Empty dict is rejected by the caller.</param>
    Task<CreateOutcome> CreateCards(long viewerId, IReadOnlyDictionary<long, int> createCounts);

    /// <summary>
    /// Toggle the <see cref="OwnedCardEntry.IsProtected"/> flag for a single card. Idempotent.
    /// Accepts cards with Count=0 (preserves the destruct→re-craft round-trip invariant).
    /// </summary>
    Task<ProtectOutcome> SetProtected(long viewerId, long cardId, bool isProtected);
}

/// <summary>
/// Either a success payload or an error code. Discriminated by which field is set.
/// </summary>
public sealed record DestructOutcome(DestructResult? Result, DestructError? Error)
{
    public bool IsSuccess => Result is not null;

    public static DestructOutcome Ok(DestructResult r) => new(r, null);
    public static DestructOutcome Fail(DestructError e) => new(null, e);
}

public sealed record DestructResult(
    ulong NewRedEtherTotal,
    IReadOnlyDictionary<long, int> NewOwnedCounts);   // cardId → post-destruct Count

public enum DestructError
{
    UnknownCard,
    NotDestructible,
    CardProtected,
    InsufficientCards,
}

public sealed record CreateOutcome(CreateResult? Result, CreateError? Error)
{
    public bool IsSuccess => Result is not null;

    public static CreateOutcome Ok(CreateResult r) => new(r, null);
    public static CreateOutcome Fail(CreateError e) => new(null, e);
}

/// <summary>
/// Outcome of a successful create. <see cref="Grants"/> is the flattened
/// <see cref="Services.GrantedReward"/> list returned by <see cref="Services.RewardGrantService.ApplyAsync"/>
/// — one Card entry per crafted cardId plus any cosmetic-cascade entries.
/// </summary>
public sealed record CreateResult(
    ulong NewRedEtherTotal,
    IReadOnlyList<Services.GrantedReward> Grants);

public enum CreateError
{
    UnknownCard,
    NotCraftable,
    WouldExceedMaxCopies,
    InsufficientVials,
}

public sealed record ProtectOutcome(bool IsSuccess, ProtectError? Error)
{
    public static ProtectOutcome Ok() => new(true, null);
    public static ProtectOutcome Fail(ProtectError e) => new(false, e);
}

public enum ProtectError
{
    UnknownCard,
}
