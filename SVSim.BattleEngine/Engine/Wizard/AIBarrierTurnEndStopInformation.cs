namespace Wizard;

public class AIBarrierTurnEndStopInformation : AITurnEndStopInformation
{
	private ulong _barrierHash;

	public AIBarrierTurnEndStopInformation(AIBarrierStopPreprocessOption option)
		: base(option.TargetCard)
	{
		_barrierHash = option.BarrierHash;
		base.Type = AITagPreprocessInfoType.BARRIER_STOP;
	}

	protected override void RunMethod(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
		if (base.TargetCard != null)
		{
			if (isAllyTurnEnd)
			{
				base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.AllyTurnEnd);
			}
			else
			{
				base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.OpponentTurnEnd);
			}
		}
	}

	public override AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AIBarrierStopPreprocessOption(overridedTarget, _barrierHash);
	}
}
