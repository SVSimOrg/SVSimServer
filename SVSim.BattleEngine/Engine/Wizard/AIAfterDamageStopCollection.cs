using System.Collections.Generic;

namespace Wizard;

public class AIAfterDamageStopCollection : AITagPreprocessCollectionBase
{
	public AIAfterDamageStopCollection Clone(List<AIVirtualCard> overrideCardList)
	{
		AIAfterDamageStopCollection aIAfterDamageStopCollection = new AIAfterDamageStopCollection();
		aIAfterDamageStopCollection.CopyInfoListWithReplaceCardReference(overrideCardList, base.InfoList);
		return aIAfterDamageStopCollection;
	}

	public void SimulateActionAll(AIVirtualCard damagedCard)
	{
		List<AITagPreprocessInformationBase> list = null;
		for (int i = 0; i < base.InfoList.Count; i++)
		{
			if (base.InfoList[i] is AIAfterDamageStopInformation aIAfterDamageStopInformation && aIAfterDamageStopInformation.ExecuteReservedAction(damagedCard))
			{
				list = AIParamQuery.AddElementToList(aIAfterDamageStopInformation, list);
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

	public void AppendInfo(AITagPreprocessCreationOptionBase option)
	{
		AIAfterDamageStopInformation aIAfterDamageStopInformation = CreateInfo(option);
		if (aIAfterDamageStopInformation != null)
		{
			base.InfoList.Add(aIAfterDamageStopInformation);
		}
	}

	private AIAfterDamageStopInformation CreateInfo(AITagPreprocessCreationOptionBase option)
	{
		if (option.PreprocessInfoType == AITagPreprocessInfoType.BARRIER_STOP)
		{
			return new AIBarrierAfterDamageStopInformation(option as AIBarrierStopPreprocessOption);
		}
		AIConsoleUtility.LogError("AITurnEndStopInfoContainer-CreateInfo(): Missing TurnEndInfo type");
		return null;
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
