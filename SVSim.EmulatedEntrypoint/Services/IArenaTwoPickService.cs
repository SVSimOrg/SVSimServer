using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaTwoPick;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IArenaTwoPickService
{
    Task<TopResponseDto> GetTopAsync(long viewerId);
    Task<EntryResponseDto> EntryAsync(long viewerId, int consumeItemType);
    Task<ClassChooseResponseDto> ChooseClassAsync(long viewerId, int classId);
    Task<CardChooseResponseDto> ChooseCardAsync(long viewerId, long selectedId);
    Task<FinishResponseDto> RetireAsync(long viewerId);
    /// <summary>
    /// Ends a completed TK2 run (5 battles played) and grants the reward tier for
    /// <c>run.WinCount</c>. Returns both the wire-shape rewards and a controller-side
    /// signal for mission emit (<see cref="RunFinishOutcome.WasFullClear"/>).
    /// </summary>
    Task<RunFinishOutcome> FinishAsync(long viewerId);
    Task<BattleFinishResultDto> RecordBattleResultAsync(long viewerId, bool isWin);
}

public class ArenaTwoPickException : Exception
{
    public string ErrorCode { get; }
    public ArenaTwoPickException(string errorCode) : base(errorCode) { ErrorCode = errorCode; }
}
