using System.Collections.Generic;

namespace Wizard;

public static class AICrystalizeUtility
{
	public static bool IsCrystalize(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (card == null || card.CrystalizeCostList == null || !card.IsInHand)
		{
			return false;
		}
		AIVariableResultContainer valResultContainer = field.AI.ValResultContainer;
		ulong hash = AIFunctionResultHashCalculator.GetHash(card, field, playPtn, null, 0uL);
		if (valResultContainer.GetContainsResultValue(AIScriptTokenVariableType.IS_CRYSTALIZE, hash, out var getResult))
		{
			return getResult == 1f;
		}
		List<int> playPtn2 = (card.IsAlly ? playPtn : null);
		bool flag = field.AI.PlayPtnRecorder.IsCardPlayingSimulationType(card, field, playPtn2, situation, PlaySimulationType.Crystalize);
		valResultContainer.CheckDuplicateAndAddRecord(AIScriptTokenVariableType.IS_CRYSTALIZE, hash, flag ? 1f : 0f, $"IsCrystalize(): Already hashed target and not equal value. CardName:[{card.CardName}] hash:[{hash}]");
		return flag;
	}

	public static int GetCrystalizeId(AIVirtualCard card, int usedCost)
	{
		if (card.CrystalizeCostList == null || card.CrystalizeCostList.Count <= 0)
		{
			return -1;
		}
		for (int i = 0; i < card.CrystalizeCostList.Count; i++)
		{
			AICrystalizeInformation aICrystalizeInformation = card.CrystalizeCostList[i];
			if (aICrystalizeInformation.Cost == usedCost)
			{
				return aICrystalizeInformation.CardId;
			}
		}
		return -1;
	}
}
