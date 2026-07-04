using System.Collections.Generic;

namespace Wizard;

public static class AISimulationRemovalUtility
{
	public static bool WillDieBySkillPrediction(AIVirtualCard card, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isHandTagValid)
	{
		LifeRecord lifeRecord = new LifeRecord
		{
			MaxLife = card.MaxLife,
			CurrentLife = card.Life
		};
		if (isHandTagValid && playPtn != null && playPtn.Count > 0)
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				GetRestLifeAfterCardPlay(field.AllyHandCards[playPtn[i]], card, field, playPtn, situation, lifeRecord);
				if (lifeRecord.CurrentLife <= 0)
				{
					break;
				}
			}
		}
		return lifeRecord.CurrentLife <= 0;
	}

	private static void GetRestLifeAfterCardPlay(AIVirtualCard playCard, AIVirtualCard target, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLife)
	{
		if (playCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			FanfareTagCollection fanfareTags = playCard.TagCollectionContainer.FanfareTags;
			if (fanfareTags.HasRemovalTags)
			{
				fanfareTags.RemovalPrediction(playCard, target, targetLife, field, playPtn, situation);
			}
		}
		if (playCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			PlayTagCollection playTags = playCard.TagCollectionContainer.PlayTags;
			if (playTags.HasRemovalTags)
			{
				playTags.RemovalPrediction(playCard, target, targetLife, field, playPtn, situation);
			}
		}
		if (!field.CardListSet.HasOtherPlayTagHolder)
		{
			return;
		}
		for (int i = 0; i < field.CardListSet.OtherPlayTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.OtherPlayTagHolders[i];
			OtherPlayTagCollection otherPlayTags = aIVirtualCard.TagCollectionContainer.OtherPlayTags;
			if (otherPlayTags.HasRemovalTags)
			{
				otherPlayTags.RemovalPrediction(aIVirtualCard, playCard, target, targetLife, field, playPtn, situation);
			}
		}
	}

	public static void PredictSurvivorsLifeAfterCardPlay(AIVirtualCard playCard, List<AIVirtualCard> targetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> targetLifeList)
	{
		if (playCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
		{
			FanfareTagCollection fanfareTags = playCard.TagCollectionContainer.FanfareTags;
			if (fanfareTags.HasRemovalTags)
			{
				fanfareTags.MultipleRemovalPrediction(playCard, targetList, field, playPtn, situation, targetLifeList);
			}
		}
		if (playCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Play))
		{
			PlayTagCollection playTags = playCard.TagCollectionContainer.PlayTags;
			if (playTags.HasRemovalTags)
			{
				playTags.MultipleRemovalPrediction(playCard, targetList, field, playPtn, situation, targetLifeList);
			}
		}
		if (!field.CardListSet.HasOtherPlayTagHolder)
		{
			return;
		}
		for (int i = 0; i < field.CardListSet.OtherPlayTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.OtherPlayTagHolders[i];
			if (!aIVirtualCard.IsSameCard(playCard))
			{
				OtherPlayTagCollection otherPlayTags = aIVirtualCard.TagCollectionContainer.OtherPlayTags;
				if (otherPlayTags.HasRemovalTags)
				{
					otherPlayTags.MultipleRemovalPrediction(aIVirtualCard, playCard, targetList, field, playPtn, situation, targetLifeList);
				}
			}
		}
	}

	public static AIVirtualCard SelectRemovalTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest, AIRemovalType removeType, AIRemovalEvaluationOption removalEvalOption = null)
	{
		if (removeType == AIRemovalType.None)
		{
			return null;
		}
		AIVirtualCard result = null;
		float num = ((worstOrBest == AISelectTargetPattern.Worst) ? float.MaxValue : float.MinValue);
		bool flag = worstOrBest == AISelectTargetPattern.Best;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			bool flag2 = tagOwner.IsAlly != aIVirtualCard.IsAlly && (aIVirtualCard.IsSneak || aIVirtualCard.IsUntouchable);
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsIndependent && !(flag && flag2))
			{
				float num2 = CalculateRemovalValue(aIVirtualCard, field, playPtn, situation, removeType, removalEvalOption);
				bool flag3 = false;
				switch (worstOrBest)
				{
				case AISelectTargetPattern.Worst:
					flag3 = num2 < num;
					break;
				case AISelectTargetPattern.Best:
					flag3 = num2 > num;
					break;
				}
				if (flag3)
				{
					num = num2;
					result = aIVirtualCard;
				}
			}
		}
		return result;
	}

	public static List<AIVirtualCard> SelectMultipleRemovalTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest, AIRemovalType removeType, int count, AIRemovalEvaluationOption removalEvalOption = null)
	{
		if (removeType == AIRemovalType.None)
		{
			return null;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		float[] array = new float[count];
		float num = ((worstOrBest == AISelectTargetPattern.Worst) ? float.MaxValue : float.MinValue);
		bool flag = worstOrBest == AISelectTargetPattern.Best;
		for (int i = 0; i < count; i++)
		{
			array[i] = num;
			list.Add(null);
		}
		for (int j = 0; j < candidates.Count; j++)
		{
			AIVirtualCard aIVirtualCard = candidates[j];
			bool flag2 = tagOwner.IsAlly != aIVirtualCard.IsAlly && (aIVirtualCard.IsSneak || aIVirtualCard.IsUntouchable);
			if (aIVirtualCard.IsDead || aIVirtualCard.IsIndependent || (flag && flag2))
			{
				continue;
			}
			float num2 = CalculateRemovalValue(aIVirtualCard, field, playPtn, situation, removeType, removalEvalOption);
			bool flag3 = false;
			int num3 = -1;
			for (int k = 0; k < list.Count; k++)
			{
				float num4 = array[k];
				switch (worstOrBest)
				{
				case AISelectTargetPattern.Worst:
					flag3 = num2 < num4;
					break;
				case AISelectTargetPattern.Best:
					flag3 = num2 > num4;
					break;
				}
				if (flag3)
				{
					num3 = k;
					break;
				}
			}
			if (flag3)
			{
				AIVirtualCard value = aIVirtualCard;
				float num5 = num2;
				for (int l = num3; l < list.Count; l++)
				{
					AIVirtualCard aIVirtualCard2 = list[l];
					float num6 = array[l];
					list[l] = value;
					array[l] = num5;
					value = aIVirtualCard2;
					num5 = num6;
				}
			}
		}
		list.RemoveAll((AIVirtualCard c) => c == null);
		return list;
	}

	public static float CalculateRemovalValue(AIVirtualCard target, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIRemovalType removeType, AIRemovalEvaluationOption removalEvalOption)
	{
		float num = 0f;
		switch (removeType)
		{
		case AIRemovalType.Destroy:
			if (!target.IsIndestructible)
			{
				num = target.EvaluateValueOnField(playPtn, situation, useStyle: true) - target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
			}
			break;
		case AIRemovalType.Banish:
			if (!target.IsUnbanishable)
			{
				num = target.EvaluateValueOnField(playPtn, situation, useStyle: true) + target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) - target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false) - target.GetAllBanishBonus(playPtn, useIgnoreInBattle: false);
			}
			break;
		case AIRemovalType.Bounce:
		{
			int restPp = (target.IsAlly ? field.AI.PlayPtnRecorder.GetRestPp(playPtn, field) : field.EnemyBattlePlayer.Pp);
			num = target.EvaluateBounceValue(playPtn, restPp);
			num -= target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false) * (float)((!target.IsAlly) ? 1 : (-1));
			break;
		}
		case AIRemovalType.Metamorphose:
			if (removalEvalOption == null)
			{
				AIConsoleUtility.LogError("SelectRemovalTarget()でMetamorphoseを指定する場合はremovalEvalOptionが必要です");
				return 0f;
			}
			num = AIMetamorphoseSimulationUtility.EvaluateSingleMetamorphoseValue(target, removalEvalOption.MetamorphoseTokenId, removalEvalOption.TagOwner, field, playPtn, situation);
			break;
		}
		if (target.IsAlly && removeType != AIRemovalType.Bounce && removeType != AIRemovalType.Metamorphose)
		{
			num *= -1f;
		}
		return num;
	}

	public static AIVirtualCard SelectWorstTargetForBuff(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo)
	{
		AIVirtualCard result = null;
		float num = float.MaxValue;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead)
			{
				int num2 = (aIVirtualCard.IsAttackable(aIVirtualCard.SelfField.BestPlayPtn) ? aIVirtualCard.AttackableCount : 0);
				float num3 = (100f - (float)aIVirtualCard.Attack) * (float)num2;
				float num4 = 100f + (float)aIVirtualCard.Attack;
				int expectedAttackBuffValue = buffInfo.GetExpectedAttackBuffValue(aIVirtualCard);
				int expectedLifeBuffValue = buffInfo.GetExpectedLifeBuffValue(aIVirtualCard);
				float num5 = (aIVirtualCard.IsAlly ? 1f : (-1f)) * ((float)expectedAttackBuffValue * num3 + (float)expectedLifeBuffValue * num4);
				if (num5 < num)
				{
					num = num5;
					result = aIVirtualCard;
				}
			}
		}
		return result;
	}

	public static AIVirtualCard SelectBestTargetForBuff(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buff)
	{
		AIVirtualCard result = null;
		float num = float.MinValue;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead)
			{
				int num2 = (aIVirtualCard.IsAttackable(aIVirtualCard.SelfField.BestPlayPtn) ? aIVirtualCard.AttackableCount : 0);
				float num3 = (100f - (float)aIVirtualCard.Attack) * (float)num2;
				float num4 = 100f + (float)aIVirtualCard.Attack;
				int expectedAttackBuffValue = buff.GetExpectedAttackBuffValue(aIVirtualCard);
				int expectedLifeBuffValue = buff.GetExpectedLifeBuffValue(aIVirtualCard);
				float num5 = (aIVirtualCard.IsAlly ? 1f : (-1f)) * ((float)expectedAttackBuffValue * num3 + (float)expectedLifeBuffValue * num4);
				if (num5 > num)
				{
					num = num5;
					result = aIVirtualCard;
				}
			}
		}
		return result;
	}
}
