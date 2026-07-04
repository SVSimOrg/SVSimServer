using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIReincarnationUtility
{
	public static float CalcReincarnationValueAfterSimulation(AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIVirtualCard> selfRemainings, List<AIVirtualCard> opponentRemainings)
	{
		float result = 0f;
		int count = playPtn.Count;
		for (int i = 0; i < count; i++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[i]];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Reincarnation))
			{
				result = aIVirtualCard.TagCollectionContainer.ReincarnationSimulationTags.CalcReincarnationValueAfterSimulation(field.AI, playPtn, situation, selfRemainings, opponentRemainings);
			}
		}
		return result;
	}

	public static float EvalMaxReincarnationBonus(AIVirtualCard actCard, List<int> playPtn)
	{
		float num = float.MinValue;
		AIVirtualField selfField = actCard.SelfField;
		if (!selfField.AllyInplayCards.Any((AIVirtualCard card) => card.IsUnit))
		{
			num = 0f;
		}
		int count = selfField.CardListSet.AllyClassAndInplayCards.Count;
		for (int num2 = 1; num2 < count; num2++)
		{
			AIVirtualCard aIVirtualCard = selfField.CardListSet.AllyClassAndInplayCards[num2];
			if (aIVirtualCard.IsUnit)
			{
				float num3 = CalcReincarnationValueToVirtualCard(aIVirtualCard, playPtn);
				if (num3 > num)
				{
					num = num3;
				}
			}
		}
		int count2 = playPtn.Count;
		for (int num4 = 0; num4 < count2; num4++)
		{
			AIVirtualCard aIVirtualCard2 = selfField.AllyHandCards[playPtn[num4]];
			if (aIVirtualCard2.IsUnit && aIVirtualCard2 != actCard)
			{
				float num5 = CalcReincarnationValueToVirtualCard(aIVirtualCard2, playPtn);
				if (num5 > num)
				{
					num = num5;
				}
			}
		}
		int count3 = selfField.CardListSet.EnemyClassAndInplayCards.Count;
		for (int num6 = 1; num6 < count3; num6++)
		{
			AIVirtualCard aIVirtualCard3 = selfField.CardListSet.EnemyClassAndInplayCards[num6];
			if (aIVirtualCard3.IsUnit)
			{
				float num7 = 0f - CalcReincarnationValueToVirtualCard(aIVirtualCard3, playPtn);
				if (num7 > num)
				{
					num = num7;
				}
			}
		}
		return num;
	}

	public static float CalcReincarnationValueToVirtualCard(AIVirtualCard target, List<int> playPtn, AISituationInfo situation = null)
	{
		float num = 0.75f;
		float num2 = target.EvaluateBreakValue(playPtn, useIgnoreBreak: false) + target.EvaluateLeaveValue(playPtn, useIgnoreInBattle: false);
		float num3 = num2 - target.EvaluateValueOnField(playPtn, situation, useStyle: true);
		float num4 = EvaluateFollowerPrimaryValue(target, playPtn, useStyle: true) * target.EvaluateAllBattleBonusRate(playPtn, useOthersTag: true, useIgnoreInBattle: false, situation) + target.GetFieldBonus(playPtn) + num2 * num;
		return num3 + num4;
	}

	public static float EvaluateFollowerPrimaryValue(AIVirtualCard targetCard, List<int> playPtn, bool useStyle)
	{
		int num = 0;
		int num2 = 0;
		float num3 = targetCard.DefaultAttack + num + targetCard.DefaultLife + num2;
		if (useStyle)
		{
			num3 += targetCard.SelfField.StyleQuery.GetUnitBonus(targetCard.SelfField, targetCard, playPtn);
			num3 *= targetCard.SelfField.StyleQuery.GetUnitRate(targetCard.SelfField, targetCard, playPtn);
		}
		return num3;
	}
}
