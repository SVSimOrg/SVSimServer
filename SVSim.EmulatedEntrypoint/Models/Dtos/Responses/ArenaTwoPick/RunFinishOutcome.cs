namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

/// <summary>
/// What <see cref="Services.IArenaTwoPickService.FinishAsync(long)"/> returns to
/// <c>ArenaTwoPickController.Finish</c>. The wire response is <see cref="Response"/>;
/// <see cref="WasFullClear"/> is a controller-side signal for the
/// <c>challenge_full_clear</c> mission emit and is never serialized.
/// </summary>
public sealed record RunFinishOutcome(FinishResponseDto Response, bool WasFullClear);
