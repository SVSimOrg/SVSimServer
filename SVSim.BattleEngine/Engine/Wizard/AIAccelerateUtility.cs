using System.Collections.Generic;

namespace Wizard;

public static class AIAccelerateUtility
{
	public static bool IsAccelerate(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (card == null || card.AccelerateCostList == null || !card.IsInHand)
		{
			return false;
		}
		if (situation is AIVirtualActionInfo { ReservedPlayType: not PlaySimulationType.Undefined } aIVirtualActionInfo)
		{
			return aIVirtualActionInfo.ReservedPlayType == PlaySimulationType.Accelerate;
		}
		List<int> playPtn2 = (card.IsAlly ? playPtn : null);
		AIVariableResultContainer valResultContainer = field.AI.ValResultContainer;
		ulong hash = AIFunctionResultHashCalculator.GetHash(card, field, playPtn, null, 0uL);
		if (valResultContainer.GetContainsResultValue(AIScriptTokenVariableType.IS_ACCELERATE, hash, out var getResult))
		{
			return getResult == 1f;
		}
		bool flag = field.AI.PlayPtnRecorder.IsCardPlayingSimulationType(card, field, playPtn2, situation, PlaySimulationType.Accelerate);
		valResultContainer.CheckDuplicateAndAddRecord(AIScriptTokenVariableType.IS_ACCELERATE, hash, flag ? 1f : 0f, $"IsAccelerate(): Already hashed target and not equal value. CardName:[{card.CardName}] hash:[{hash}]");
		return flag;
	}

	public static int GetAccelerateCost(AIVirtualCard card, AIVirtualField field, List<int> playPtn)
	{
		if (card == null || card.AccelerateCostList == null)
		{
			return -1;
		}
		List<int> playPtn2 = (card.IsAlly ? playPtn : null);
		return field.AI.PlayPtnRecorder.GetCardPlaySimulationTypeCost(card, field, playPtn2, null, PlaySimulationType.Accelerate);
	}

	public static int GetConditionPassedAccelerateId(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation, int usedCost)
	{
		if (card.AccelerateCostList == null || card.AccelerateCostList.Count <= 0)
		{
			return -1;
		}
		for (int i = 0; i < card.AccelerateCostList.Count; i++)
		{
			AIAccelerateInformation aIAccelerateInformation = card.AccelerateCostList[i];
			if (aIAccelerateInformation.Cost == usedCost && aIAccelerateInformation.CheckCondition(card, field, playPtn, situation))
			{
				return aIAccelerateInformation.CardId;
			}
		}
		return -1;
	}
}
