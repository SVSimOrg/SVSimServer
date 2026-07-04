using System.Collections.Generic;

namespace Wizard;

public class AILeaveStopCollection : AITagPreprocessCollectionBase
{
	public AILeaveStopCollection Clone(List<AIVirtualCard> overrideCardList)
	{
		AILeaveStopCollection aILeaveStopCollection = new AILeaveStopCollection();
		aILeaveStopCollection.CopyInfoListWithReplaceCardReference(overrideCardList, base.InfoList);
		return aILeaveStopCollection;
	}

	public void SimulateActionAll(AIVirtualCard leaveCard, AISituationInfo situation)
	{
		List<AITagPreprocessInformationBase> list = null;
		for (int i = 0; i < base.InfoList.Count; i++)
		{
			if (base.InfoList[i] is AILeaveStopInformation aILeaveStopInformation && aILeaveStopInformation.ExecuteReservedAction(leaveCard, situation))
			{
				list = AIParamQuery.AddElementToList(aILeaveStopInformation, list);
			}
		}
		if (list != null && list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				base.InfoList.Remove(list[j]);
			}
		}
	}

	public void PseudoSimulateForEvalRandomMultiDamage(List<AIBarrierPseudoSimulationInfo> barrierInfoList, AIVirtualCard leaveCard)
	{
		for (int i = 0; i < base.InfoList.Count; i++)
		{
			if (base.InfoList[i] is AILeaveStopInformation aILeaveStopInformation)
			{
				aILeaveStopInformation.PseudoExecuteForEvalRandomMultiDamage(leaveCard, barrierInfoList);
			}
		}
	}

	public void AppendInfo(AITagPreprocessCreationOptionBase option, AIVirtualCard provider)
	{
		AILeaveStopInformation aILeaveStopInformation = CreateInfo(option, provider);
		if (aILeaveStopInformation != null)
		{
			base.InfoList.Add(aILeaveStopInformation);
		}
	}

	private AILeaveStopInformation CreateInfo(AITagPreprocessCreationOptionBase option, AIVirtualCard provider)
	{
		switch (option.PreprocessInfoType)
		{
		case AITagPreprocessInfoType.BARRIER_STOP:
			return new AIBarrierLeaveStopInformation(option as AIBarrierStopPreprocessOption, provider);
		case AITagPreprocessInfoType.REMOVE_ATTACHED_TAG:
			return new AIAttachedTagLeaveStopInformation(option as AIAttachedTagStopPreprocessOption, provider);
		case AITagPreprocessInfoType.UNTOUCHABLE_STOP:
			return new AIUntouchableLeaveStopInformation(option, provider);
		default:
			AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Missing TurnEndInfo type");
			return null;
		}
	}

	protected override void GetOverrideCardAndAppendCopyInfo(List<AIVirtualCard> overrideCardList, AITagPreprocessInformationBase originalInfo)
	{
		if (overrideCardList != null && overrideCardList.Count > 0 && originalInfo is AILeaveStopInformation aILeaveStopInformation)
		{
			AIVirtualCard overrideTargetCard = aILeaveStopInformation.GetOverrideTargetCard(overrideCardList);
			AIVirtualCard overrideProviderCard = aILeaveStopInformation.GetOverrideProviderCard(overrideCardList);
			if (overrideTargetCard != null && overrideProviderCard != null)
			{
				AITagPreprocessCreationOptionBase option = originalInfo.CreateOptionInfoForOverride(overrideTargetCard);
				AppendInfo(option, overrideProviderCard);
			}
		}
	}
}
