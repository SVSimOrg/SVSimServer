using System.Collections.Generic;

namespace Wizard;

public class AIBarrierLeaveStopInformation : AILeaveStopInformation
{
	private ulong _barrierHash;

	public AIBarrierLeaveStopInformation(AIBarrierStopPreprocessOption option, AIVirtualCard provider)
		: base(option.TargetCard, provider)
	{
		_barrierHash = option.BarrierHash;
		base.Type = AITagPreprocessInfoType.BARRIER_STOP;
	}

	protected override void StopMethod(AISituationInfo situation)
	{
		base.TargetCard.BarrierInfoCollection.DepriveCertainBarrier(_barrierHash, AIBarrierStopTiming.WhenLeaveStop);
	}

	protected override void PseudoStopMethodForEvalRandomMultiDamage(List<AIBarrierPseudoSimulationInfo> barrierInfoList)
	{
		for (int i = 0; i < barrierInfoList.Count; i++)
		{
			AIBarrierPseudoSimulationInfo aIBarrierPseudoSimulationInfo = barrierInfoList[i];
			if (aIBarrierPseudoSimulationInfo.Owner.IsSameCard(base.TargetCard))
			{
				aIBarrierPseudoSimulationInfo.DepriveCertainBarrier(AIBarrierStopTiming.WhenLeaveStop, _barrierHash);
			}
		}
	}

	public override AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AIBarrierStopPreprocessOption(overridedTarget, _barrierHash);
	}
}
