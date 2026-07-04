using System.Collections.Generic;

namespace Wizard;

public class AILeaveStopInformation : AITagPreprocessInformationBase
{
	private AIVirtualCard _provider;

	public AILeaveStopInformation(AIVirtualCard target, AIVirtualCard provider)
		: base(target)
	{
		_provider = provider;
	}

	public bool ExecuteReservedAction(AIVirtualCard leaveCard, AISituationInfo situation)
	{
		if (leaveCard.IsSameCard(_provider))
		{
			StopMethod(situation);
			return true;
		}
		return false;
	}

	protected virtual void StopMethod(AISituationInfo situation)
	{
	}

	public void PseudoExecuteForEvalRandomMultiDamage(AIVirtualCard leaveCard, List<AIBarrierPseudoSimulationInfo> barrierInfoList)
	{
		if (leaveCard.IsSameCard(_provider))
		{
			PseudoStopMethodForEvalRandomMultiDamage(barrierInfoList);
		}
	}

	protected virtual void PseudoStopMethodForEvalRandomMultiDamage(List<AIBarrierPseudoSimulationInfo> barrierInfoList)
	{
	}

	public AIVirtualCard GetOverrideProviderCard(List<AIVirtualCard> overrideCardList)
	{
		for (int i = 0; i < overrideCardList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = overrideCardList[i];
			if (aIVirtualCard.IsSameCard(_provider))
			{
				return aIVirtualCard;
			}
		}
		return null;
	}
}
