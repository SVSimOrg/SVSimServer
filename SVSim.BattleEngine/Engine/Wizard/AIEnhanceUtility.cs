using System.Collections.Generic;

namespace Wizard;

public static class AIEnhanceUtility
{
	public static bool IsEnhanced(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (card == null || card.EnhanceCostList == null)
		{
			return false;
		}
		AIVariableResultContainer valResultContainer = field.AI.ValResultContainer;
		ulong hash = AIFunctionResultHashCalculator.GetHash(card, field, playPtn, null, 0uL);
		if (valResultContainer.GetContainsResultValue(AIScriptTokenVariableType.IS_ENHANCED, hash, out var getResult))
		{
			return getResult == 1f;
		}
		bool flag = false;
		if (card.PlayedCost < 0 && card.IsInHand)
		{
			List<int> playPtn2 = (card.IsAlly ? playPtn : null);
			flag = field.AI.PlayPtnRecorder.IsCardPlayingSimulationType(card, field, playPtn2, situation, PlaySimulationType.Enhance);
		}
		else
		{
			flag = card.PlayedCost >= 0 && card.EnhanceCostList.Contains(card.PlayedCost);
		}
		valResultContainer.CheckDuplicateAndAddRecord(AIScriptTokenVariableType.IS_ENHANCED, hash, flag ? 1f : 0f, $"IsEnhanced(): Already hashed target and not equal value. CardName:[{card.CardName}] hash:[{hash}]");
		return flag;
	}

	public static int GetEnhanceCost(AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (card == null || card.EnhanceCostList == null)
		{
			return -1;
		}
		if (card.PlayedCost >= 0)
		{
			if (card.EnhanceCostList.Contains(card.PlayedCost))
			{
				return card.PlayedCost;
			}
			return -1;
		}
		List<int> playPtn2 = (card.IsAlly ? playPtn : null);
		return field.AI.PlayPtnRecorder.GetCardPlaySimulationTypeCost(card, field, playPtn2, null, PlaySimulationType.Enhance);
	}
}
