using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public static class AIMetamorphoseSimulationUtility
{
	private struct SwappedMetamorphoseInfo
	{
		public AIVirtualCard TargetCard;

		public AIVirtualCard TokenCard;

		public List<AIVirtualCard> SwappedCardList;

		public int SwappedIndex;
	}

	public static AIRemovalEvaluationOption CreateMetamorphoseEvaluationOption(AIVirtualCard tagOwner, AIVirtualField field, int metamorphoseId)
	{
		return new AIRemovalEvaluationOption
		{
			TagOwner = tagOwner,
			MetamorphoseTokenId = metamorphoseId
		};
	}

	public static void MetamorphoseAll(AIVirtualField field, List<AIVirtualCard> range, int metamorphoseId, AIVirtualCard actor, AISituationInfo situation)
	{
		for (int i = 0; i < range.Count; i++)
		{
			AIVirtualCard aIVirtualCard = range[i];
			if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsDead)
			{
				ExecuteMetamorphose(aIVirtualCard, metamorphoseId, actor, field, situation);
			}
		}
	}

	public static void MetamorphoseRandom(AIVirtualField field, List<AIVirtualCard> range, int metamorphoseId, AIVirtualCard actor, List<int> playPtn, AISituationInfo situation, int selectCount = 1)
	{
		if (selectCount <= 0)
		{
			AIConsoleUtility.LogError($"AIMetamorphoseSimulationUtility.MetamorphoseRandom() error!! selectCount == {selectCount}");
			return;
		}
		AIRemovalEvaluationOption aIRemovalEvaluationOption = CreateMetamorphoseEvaluationOption(actor, field, metamorphoseId);
		if (aIRemovalEvaluationOption == null)
		{
			AIConsoleUtility.LogError("AIMetamorphoseSimulationUtility.MetamorphoseRandom() error!! failed create option");
			return;
		}
		if (selectCount == 1)
		{
			AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectRemovalTarget(range, actor, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Metamorphose, aIRemovalEvaluationOption);
			if (aIVirtualCard != null && !aIVirtualCard.IsIndependent)
			{
				ExecuteMetamorphose(aIVirtualCard, metamorphoseId, actor, field, situation);
			}
			return;
		}
		List<AIVirtualCard> list = AISimulationRemovalUtility.SelectMultipleRemovalTargets(range, actor, field, playPtn, situation, AISelectTargetPattern.Worst, AIRemovalType.Metamorphose, selectCount, aIRemovalEvaluationOption);
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ExecuteMetamorphose(list[i], metamorphoseId, actor, field, situation);
			}
		}
	}

	public static void MetamorphoseTarget(AIVirtualField field, List<AIVirtualCard> candidates, int metamorphoseId, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType whichTarget, int count)
	{
		if (situation.IsTargetExists(whichTarget))
		{
			MetamorphoseSelectedTarget(field, metamorphoseId, situation, whichTarget);
		}
		else
		{
			MetamorphoseTargetPrediction(field, candidates, metamorphoseId, playPtn, situation, whichTarget, count);
		}
	}

	private static void MetamorphoseSelectedTarget(AIVirtualField field, int metamorphoseId, AISituationInfo situation, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("MetamorphoseSelectedTarget(): error!! No target!!!!!");
			return;
		}
		List<AIVirtualCard> targets = situationTarget.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			ExecuteMetamorphose(targets[i], metamorphoseId, situation.Actor, field, situation);
		}
	}

	private static void MetamorphoseTargetPrediction(AIVirtualField field, List<AIVirtualCard> candidates, int metamorphoseId, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType whichTarget, int count)
	{
		if (candidates.Count < count)
		{
			AIConsoleUtility.LogError("MetamorphoseTargetPrediction(): Target candidates is not enough");
			return;
		}
		AIVirtualCard actor = situation.Actor;
		AIRemovalEvaluationOption aIRemovalEvaluationOption = CreateMetamorphoseEvaluationOption(actor, field, metamorphoseId);
		if (aIRemovalEvaluationOption == null)
		{
			AIConsoleUtility.LogError("MetamorphoseTargetPrediction() error!! failed create option");
			return;
		}
		if (count == 1)
		{
			AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectRemovalTarget(candidates, actor, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Metamorphose, aIRemovalEvaluationOption);
			if (aIVirtualCard != null && !aIVirtualCard.IsIndependent)
			{
				ExecuteMetamorphose(aIVirtualCard, metamorphoseId, actor, field, situation);
			}
			return;
		}
		List<AIVirtualCard> list = AISimulationRemovalUtility.SelectMultipleRemovalTargets(candidates, actor, field, playPtn, situation, AISelectTargetPattern.Best, AIRemovalType.Metamorphose, count, aIRemovalEvaluationOption);
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				ExecuteMetamorphose(list[i], metamorphoseId, actor, field, situation);
			}
		}
	}

	private static void ExecuteMetamorphose(AIVirtualCard target, int metamorphoseTokenId, AIVirtualCard actor, AIVirtualField field, AISituationInfo situation)
	{
		target.MetamorphoseLeave(situation);
		MetamorphoseTokenOnVirtualField(target, metamorphoseTokenId, actor, field);
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
	}

	public static float EvalTargetingMetamorphose(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, int tokenId, List<int> playPtn, AISituationInfo situation)
	{
		float num = float.MinValue;
		List<AIVirtualCard> bothClassAndInplayCards = tagOwner.SelfField.CardListSet.BothClassAndInplayCards;
		bothClassAndInplayCards = AIFilteringUtility.MultipleFiltering(bothClassAndInplayCards, filters, tagOwner, playPtn, situation);
		bothClassAndInplayCards.RemoveAll((AIVirtualCard c) => c.IsLeader);
		if (bothClassAndInplayCards == null || bothClassAndInplayCards.Count <= 0)
		{
			return 0f;
		}
		AIVirtualCard aIVirtualCard = null;
		if (bothClassAndInplayCards.IsNotNullOrEmpty())
		{
			bothClassAndInplayCards = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(bothClassAndInplayCards, tagOwner, playPtn);
			for (int num2 = 0; num2 < bothClassAndInplayCards.Count; num2++)
			{
				AIVirtualCard aIVirtualCard2 = bothClassAndInplayCards[num2];
				if (!aIVirtualCard2.IsIndependent && (aIVirtualCard2.IsAlly == tagOwner.IsAlly || (!aIVirtualCard2.IsUntouchable && !aIVirtualCard2.IsSneak)))
				{
					float num3 = EvaluateSingleMetamorphoseValue(aIVirtualCard2, tokenId, tagOwner, field, playPtn, situation);
					if (num < num3)
					{
						num = num3;
						aIVirtualCard = aIVirtualCard2;
					}
				}
			}
		}
		if (aIVirtualCard == null)
		{
			return 0f;
		}
		return num;
	}

	public static float EvalRandomMetamorphose(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, int tokenId, List<int> playPtn, int count, AISituationInfo situation)
	{
		List<AIVirtualCard> bothClassAndInplayCards = field.CardListSet.BothClassAndInplayCards;
		bothClassAndInplayCards = AIFilteringUtility.MultipleFiltering(bothClassAndInplayCards, filters, tagOwner, playPtn, null);
		if (bothClassAndInplayCards == null || bothClassAndInplayCards.Count <= 0)
		{
			return 0f;
		}
		bothClassAndInplayCards.RemoveAll((AIVirtualCard c) => c.IsLeader);
		if (bothClassAndInplayCards == null || bothClassAndInplayCards.Count <= 0)
		{
			return 0f;
		}
		if (bothClassAndInplayCards.Count <= count)
		{
			return EvalAllMetamorphose(bothClassAndInplayCards, tokenId, tagOwner, field, playPtn, situation);
		}
		float num = 0f;
		new List<SwappedMetamorphoseInfo>();
		List<AIVirtualCard[]> list = AIMathematicsLibrary.EnumerateCombinations(bothClassAndInplayCards, count).ToList();
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			List<AIVirtualCard> targetCards = new List<AIVirtualCard>(list[num2]);
			num += EvalAllMetamorphose(targetCards, tokenId, tagOwner, field, playPtn, situation);
		}
		return num / (float)list.Count;
	}

	public static float EvalAllMetamorphose(AIVirtualCard tagOwner, AIVirtualField field, List<AIScriptTokenBase> filters, int tokenId, List<int> playPtn, AISituationInfo situation)
	{
		return EvalAllMetamorphose(AIFilteringUtility.MultipleFiltering(tagOwner.SelfField.CardListSet.BothClassAndInplayCards, filters, tagOwner, playPtn, null), tokenId, tagOwner, field, playPtn, situation);
	}

	private static float EvalAllMetamorphose(List<AIVirtualCard> targetCards, int tokenId, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		targetCards.RemoveAll((AIVirtualCard c) => c.IsLeader);
		if (targetCards == null || targetCards.Count <= 0)
		{
			return 0f;
		}
		if (targetCards == null || targetCards.Count <= 0)
		{
			return 0f;
		}
		List<SwappedMetamorphoseInfo> swappedList = new List<SwappedMetamorphoseInfo>();
		float num = 0f;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		AIVirtualCard aIVirtualCard = null;
		AIVirtualCard aIVirtualCard2 = null;
		for (int num2 = 0; num2 < targetCards.Count; num2++)
		{
			AIVirtualCard aIVirtualCard3 = targetCards[num2];
			if (aIVirtualCard3.IsIndependent)
			{
				continue;
			}
			AIVirtualCard aIVirtualCard4 = null;
			if (aIVirtualCard3.IsAlly)
			{
				if (aIVirtualCard == null)
				{
					aIVirtualCard = field.AI.tokenManager.GetTokenFromId(tokenId, isAlly: true, field);
				}
				aIVirtualCard4 = aIVirtualCard;
			}
			else
			{
				if (aIVirtualCard2 == null)
				{
					aIVirtualCard2 = field.AI.tokenManager.GetTokenFromId(tokenId, isAlly: false, field);
				}
				aIVirtualCard4 = aIVirtualCard2;
			}
			if (aIVirtualCard4 == null)
			{
				AIConsoleUtility.LogError(string.Format("AIMetamorphosSimulationUtility.EvaluateSingleMetamorphoseValue() error!! Not found {0}: {1}", aIVirtualCard3.IsAlly ? "Ally" : "ENEMY", tokenId));
				return 0f;
			}
			num += EvaluateMetamorphoseTargetValue(aIVirtualCard3, playPtn, situation) * ((tagOwner.IsAlly != aIVirtualCard3.IsAlly) ? 1f : (-1f));
			list.Add(aIVirtualCard4);
		}
		for (int num3 = 0; num3 < targetCards.Count; num3++)
		{
			SwapInplayMetamorphoseCard(list[num3], targetCards[num3], field, ref swappedList);
		}
		float num4 = 0f;
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			AIVirtualCard aIVirtualCard5 = list[num5];
			float num6 = EvaluateMetamorphoseTokenValue(aIVirtualCard5, playPtn, situation);
			num4 += num6 * ((tagOwner.IsAlly != aIVirtualCard5.IsAlly) ? 1f : (-1f));
		}
		RestoreInplayMetamorphoseCard(swappedList);
		list.Clear();
		return (num - num4) * (tagOwner.IsAlly ? 1f : (-1f));
	}

	public static float EvaluateSingleMetamorphoseValue(AIVirtualCard targetCard, int afterMetamorphoseTokenId, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		float num = EvaluateMetamorphoseTargetValue(targetCard, playPtn, situation);
		AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(afterMetamorphoseTokenId, targetCard.IsAlly, field);
		if (tokenFromId == null)
		{
			AIConsoleUtility.LogError($"AIMetamorphosSimulationUtility.EvaluateSingleMetamorphoseValue() error!! Not found id == {afterMetamorphoseTokenId}");
			return 0f;
		}
		List<SwappedMetamorphoseInfo> swappedList = new List<SwappedMetamorphoseInfo>();
		SwapInplayMetamorphoseCard(tokenFromId, targetCard, field, ref swappedList);
		float num2 = EvaluateMetamorphoseTokenValue(tokenFromId, playPtn, situation);
		float result = (num - num2) * ((targetCard.IsAlly != tagOwner.IsAlly) ? 1f : (-1f));
		RestoreInplayMetamorphoseCard(swappedList);
		return result;
	}

	private static float EvaluateMetamorphoseTargetValue(AIVirtualCard target, List<int> playPtn, AISituationInfo situation)
	{
		return target.EvaluateValueOnField(playPtn, situation, useStyle: true) + (target.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + target.GetAllBanishBonus(playPtn, useIgnoreInBattle: false) + target.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false));
	}

	private static float EvaluateMetamorphoseTokenValue(AIVirtualCard tokenCard, List<int> playPtn, AISituationInfo situation)
	{
		return tokenCard.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) + (tokenCard.EvaluateBreakValue(playPtn, useIgnoreBreak: true) + tokenCard.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true)) * EnemyAI.BREAKBONUS_RATE_IN_HAND;
	}

	private static void SwapInplayMetamorphoseCard(AIVirtualCard tokenCard, AIVirtualCard targetCard, AIVirtualField fieldTemp, ref List<SwappedMetamorphoseInfo> swappedList)
	{
		if (targetCard.IsAlly)
		{
			Swap(fieldTemp.AllyInplayCards, swappedList);
			Swap(fieldTemp.CardListSet.AllAllyCards, swappedList);
			Swap(fieldTemp.CardListSet.AllyClassAndInplayCards, swappedList);
		}
		else
		{
			Swap(fieldTemp.EnemyInplayCards, swappedList);
			Swap(fieldTemp.CardListSet.EnemyClassAndInplayCards, swappedList);
		}
		Swap(fieldTemp.CardListSet.AllReferableCards, swappedList);
		Swap(fieldTemp.CardListSet.BothClassAndInplayCards, swappedList);
		Swap(fieldTemp.CardListSet.BothInplayCards, swappedList);
		void Swap(List<AIVirtualCard> _cardList, List<SwappedMetamorphoseInfo> _swappedList)
		{
			int num = _cardList.IndexOf(targetCard);
			if (num >= 0 && num < _cardList.Count)
			{
				_cardList.Insert(num, tokenCard);
				_cardList.Remove(targetCard);
				_swappedList = AIParamQuery.AddElementToList(new SwappedMetamorphoseInfo
				{
					TargetCard = targetCard,
					TokenCard = tokenCard,
					SwappedCardList = _cardList,
					SwappedIndex = num
				}, _swappedList);
			}
		}
	}

	private static void RestoreInplayMetamorphoseCard(List<SwappedMetamorphoseInfo> swappedList)
	{
		if (swappedList != null)
		{
			for (int i = 0; i < swappedList.Count; i++)
			{
				SwappedMetamorphoseInfo swappedMetamorphoseInfo = swappedList[i];
				List<AIVirtualCard> swappedCardList = swappedMetamorphoseInfo.SwappedCardList;
				int swappedIndex = swappedMetamorphoseInfo.SwappedIndex;
				swappedCardList.Insert(swappedIndex, swappedMetamorphoseInfo.TargetCard);
				swappedCardList.Remove(swappedMetamorphoseInfo.TokenCard);
			}
			swappedList.Clear();
		}
	}

	public static void MetamorphoseTokenOnVirtualField(AIVirtualCard targetCard, int tokenId, AIVirtualCard tagOwner, AIVirtualField field)
	{
		AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(tokenId, targetCard.IsAlly, field, needsClone: true);
		if (tokenFromId == null)
		{
			AIConsoleUtility.LogError("MetamorphoseTokenOnVirtualField: tokenCard is null");
			return;
		}
		tokenFromId.InitAtMetamorphose(targetCard, tagOwner);
		field.CardListSet.ReplaceInplayCard(targetCard.IsAlly, tokenFromId, targetCard);
		targetCard.IsMetamorphosed = true;
		if (targetCard.IsAlly)
		{
			List<AIVirtualCard> allyInplayCards = field.AllyInplayCards;
			allyInplayCards.Insert(allyInplayCards.IndexOf(targetCard), tokenFromId);
			allyInplayCards.Remove(targetCard);
		}
		else
		{
			List<AIVirtualCard> enemyInplayCards = field.EnemyInplayCards;
			enemyInplayCards.Insert(enemyInplayCards.IndexOf(targetCard), tokenFromId);
			enemyInplayCards.Remove(targetCard);
			field.EnemyTokenQueue.Enqueue(new Tuple<AIVirtualCard, AIVirtualCard>(targetCard, tokenFromId));
		}
	}

	public static void MetamorphoseHandAll(AIVirtualField field, List<AIVirtualCard> range, int metamorphoseId, AIVirtualCard actor, AISituationInfo situation)
	{
		for (int i = 0; i < range.Count; i++)
		{
			MetamorphoseHandOnVirtualField(range[i], metamorphoseId, actor, field);
		}
	}

	public static void MetamorphoseHandRandom(AIVirtualField field, List<AIVirtualCard> range, int metamorphoseId, AIVirtualCard actor, List<int> playPtn, AISituationInfo situaion)
	{
		AIVirtualCard aIVirtualCard = null;
		float num = float.MinValue;
		for (int i = 0; i < range.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = range[i];
			float num2 = aIVirtualCard2.EvaluatePlayValue(playPtn, situaion) + aIVirtualCard2.GetHandBonus(playPtn, situaion, isIgnoreInFusion: false);
			if (num < num2)
			{
				aIVirtualCard = aIVirtualCard2;
				num = num2;
			}
		}
		if (aIVirtualCard != null)
		{
			MetamorphoseHandOnVirtualField(aIVirtualCard, metamorphoseId, actor, field);
		}
	}

	public static void MetamorphoseHandTarget(AIVirtualField field, List<AIVirtualCard> candidates, int metamorphoseId, AISituationInfo situation, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget == null || !situationTarget.HasTarget)
		{
			AIConsoleUtility.LogError("MetamorphoseHandTarget error!! No target!!!!!");
			return;
		}
		for (int i = 0; i < situationTarget.Targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = situationTarget.Targets[i];
			if (candidates.Contains(aIVirtualCard))
			{
				MetamorphoseHandOnVirtualField(aIVirtualCard, metamorphoseId, situation.Actor, field);
			}
		}
	}

	public static void MetamorphoseHandOnVirtualField(AIVirtualCard targetCard, int tokenId, AIVirtualCard tagOwner, AIVirtualField field)
	{
		AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(tokenId, tagOwner.IsAlly, field, needsClone: true);
		if (tokenFromId == null)
		{
			AIConsoleUtility.LogError("MetamorphoseHandOnVirtualField: tokenCard is null");
			return;
		}
		tokenFromId.InitAtHandMetamorphose(targetCard, tagOwner);
		targetCard.IsMetamorphosed = true;
		if (targetCard.IsAlly)
		{
			field.CardListSet.ReplaceAllyHandCard(tokenFromId, targetCard);
		}
		List<AIVirtualCard> obj = (targetCard.IsAlly ? field.AllyHandCards : field.GetEnemyHandCardList());
		obj.Insert(obj.IndexOf(targetCard), tokenFromId);
		obj.Remove(targetCard);
	}
}
