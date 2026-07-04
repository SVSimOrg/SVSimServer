using System.Collections.Generic;

namespace Wizard;

public class AITurnEndStopCollection : AITagPreprocessCollectionBase
{
	public AITurnEndStopCollection Clone(List<AIVirtualCard> overrideCardList)
	{
		AITurnEndStopCollection aITurnEndStopCollection = new AITurnEndStopCollection();
		aITurnEndStopCollection.CopyInfoListWithReplaceCardReference(overrideCardList, base.InfoList);
		return aITurnEndStopCollection;
	}

	public void SimulateActionAll(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
		List<AITagPreprocessInformationBase> list = null;
		for (int i = 0; i < base.InfoList.Count; i++)
		{
			if (base.InfoList[i] is AITurnEndStopInformation aITurnEndStopInformation && aITurnEndStopInformation.ExecuteReservedAction(isAllyTurnEnd, situation))
			{
				list = AIParamQuery.AddElementToList(aITurnEndStopInformation, list);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				base.InfoList.Remove(list[j]);
			}
		}
	}

	public void AppendInfo(AITagPreprocessCreationOptionBase option, int defaultDecrementCounter)
	{
		AITurnEndStopInformation aITurnEndStopInformation = CreateInfo(option, defaultDecrementCounter);
		if (aITurnEndStopInformation != null)
		{
			base.InfoList.Add(aITurnEndStopInformation);
		}
	}

	private AITurnEndStopInformation CreateInfo(AITagPreprocessCreationOptionBase option, int defaultDecrementCounter)
	{
		AIVirtualCard targetCard = option.TargetCard;
		if (targetCard == null)
		{
			AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Target card is null");
			return null;
		}
		switch (option.PreprocessInfoType)
		{
		case AITagPreprocessInfoType.STATUS_CHANGE_STOP:
			return new AIBuffTurnEndStopInformation(targetCard);
		case AITagPreprocessInfoType.BARRIER_STOP:
			return new AIBarrierTurnEndStopInformation(option as AIBarrierStopPreprocessOption);
		case AITagPreprocessInfoType.REMOVE_ATTACHED_TAG:
			return new AIAttachedTagTurnEndStopInformation(option as AIAttachedTagStopPreprocessOption, defaultDecrementCounter);
		default:
			AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Missing TurnEndInfo type");
			return null;
		}
	}

	protected override void GetOverrideCardAndAppendCopyInfo(List<AIVirtualCard> overrideCardList, AITagPreprocessInformationBase originalInfo)
	{
		if (overrideCardList == null || overrideCardList.Count <= 0)
		{
			return;
		}
		AIVirtualCard overrideTargetCard = originalInfo.GetOverrideTargetCard(overrideCardList);
		if (overrideTargetCard != null)
		{
			AITagPreprocessCreationOptionBase option = originalInfo.CreateOptionInfoForOverride(overrideTargetCard);
			if (originalInfo is AITurnEndStopInformation { TurnEndDecrement: var turnEndDecrement })
			{
				AppendInfo(option, turnEndDecrement);
			}
		}
	}
}
