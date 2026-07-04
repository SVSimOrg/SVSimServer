namespace SVSim.Database.Services;

/// <summary>The scalar wallet currencies the central debit primitive understands.</summary>
public enum SpendCurrency { Crystal, Rupee, RedEther, SpotPoint }

public enum SpendOutcome { Success, Insufficient }

/// <summary>
/// Result of a <see cref="ICurrencySpendService.TrySpendAsync"/> call. <see cref="PostStateTotal"/>
/// is the balance the client should show after the spend — the real post-deduction balance, or the
/// freeplay effective balance when the spend was a freeplay no-op.
/// </summary>
public sealed record SpendResult(SpendOutcome Outcome, long PostStateTotal)
{
    public bool Success => Outcome == SpendOutcome.Success;
}
