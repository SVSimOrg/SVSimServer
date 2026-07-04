using System.Collections.Generic;

namespace Wizard;

public class AITurnStartStopCollection : AITagPreprocessCollectionBase
{
	public AITurnStartStopCollection Clone(List<AIVirtualCard> overrideCardList)
	{
		AITurnStartStopCollection aITurnStartStopCollection = new AITurnStartStopCollection();
		aITurnStartStopCollection.CopyInfoListWithReplaceCardReference(overrideCardList, base.InfoList);
		return aITurnStartStopCollection;
	}

	public void AppendInfo(AITagPreprocessCreationOptionBase option)
	{
		AITurnStartStopInformation aITurnStartStopInformation = CreateInfo(option);
		if (aITurnStartStopInformation != null)
		{
			base.InfoList.Add(aITurnStartStopInformation);
		}
	}

	private AITurnStartStopInformation CreateInfo(AITagPreprocessCreationOptionBase option)
	{
		if (option.TargetCard == null)
		{
			AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Target card is null");
			return null;
		}
		switch (option.PreprocessInfoType)
		{
		case AITagPreprocessInfoType.BARRIER_STOP:
			return new AIBarrierTurnStartStopInformation(option as AIBarrierStopPreprocessOption);
		case AITagPreprocessInfoType.REMOVE_ATTACHED_TAG:
			return new AIAttachedTagTurnStartStopInformation(option as AIAttachedTagStopPreprocessOption);
		default:
			AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Missing TurnEndInfo type");
			return null;
		}
	}

	public void SimulateActionAll(bool isAllyTurnEnd, AISituationInfo situation)
	{
		for (int i = 0; i < base.InfoList.Count; i++)
		{
			if (base.InfoList[i] is AITurnStartStopInformation aITurnStartStopInformation)
			{
				aITurnStartStopInformation.ExecuteReservedAction(isAllyTurnEnd, situation);
			}
		}
	}

	protected override void GetOverrideCardAndAppendCopyInfo(List<AIVirtualCard> overrideCardList, AITagPreprocessInformationBase originalInfo)
	{
		if (overrideCardList != null && overrideCardList.Count > 0)
		{
			AIVirtualCard overrideTargetCard = originalInfo.GetOverrideTargetCard(overrideCardList);
			if (overrideTargetCard != null)
			{
				AITagPreprocessCreationOptionBase option = originalInfo.CreateOptionInfoForOverride(overrideTargetCard);
				AppendInfo(option);
			}
		}
	}
}
