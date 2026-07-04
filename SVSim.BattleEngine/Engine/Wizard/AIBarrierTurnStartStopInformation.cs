namespace Wizard;

public class AIBarrierTurnStartStopInformation : AITurnStartStopInformation
{
	private ulong _barrierHash;

	public AIBarrierTurnStartStopInformation(AIBarrierStopPreprocessOption option)
		: base(option.TargetCard)
	{
		base.Type = AITagPreprocessInfoType.BARRIER_STOP;
		_barrierHash = option.BarrierHash;
	}

	public override void ExecuteReservedAction(bool isAllyTurnEnd, AISituationInfo situation)
	{
		if (base.TargetCard != null)
		{
			if (isAllyTurnEnd)
			{
				base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.AllyTurnStart);
			}
			else
			{
				base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.OpponentTurnStart);
			}
		}
	}

	public override AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AIBarrierStopPreprocessOption(overridedTarget, _barrierHash);
	}
}
