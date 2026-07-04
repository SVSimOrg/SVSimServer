using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AISummonTokenUtility
{

	public static void SummonAllTokenToField(this AITokenIdCollection tokenIdCollection, AIVirtualField field, AIVirtualCard owner, AISituationInfo situation, List<AIPlayTag> condList = null)
	{
		if (tokenIdCollection == null)
		{
			AIConsoleUtility.LogError("AISummonTokenUtility.SummonAllTokenToField() error!! tokenIdCollection is null");
			return;
		}
		if (tokenIdCollection.HasAllyToken)
		{
			SummonTokenOnVirtualField(tokenIdCollection.AllyTokenIdList, owner, field, situation, isTokenAlly: true, isSkillSummon: true, condList);
		}
		if (tokenIdCollection.HasOpponentToken)
		{
			SummonTokenOnVirtualField(tokenIdCollection.OpponentTokenIdList, owner, field, situation, isTokenAlly: false, isSkillSummon: true, condList);
		}
	}

	private static void SummonTokenOnVirtualField(List<AITokenInformation> tokenIdList, AIVirtualCard tokenOwner, AIVirtualField field, AISituationInfo situation, bool isTokenAlly, bool isSkillSummon, List<AIPlayTag> condList = null)
	{
		if (situation == null)
		{
			return;
		}
		EnemyAI aI = field.AI;
		List<int> bestPlayPtn = field.BestPlayPtn;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < tokenIdList.Count; i++)
		{
			int tokenId = tokenIdList[i].TokenId;
			AITokenType tokenType = tokenIdList[i].TokenType;
			AIVirtualCard tokenCard = GetTokenCard(aI, tokenOwner, field, tokenId, isTokenAlly, condList);
			if (tokenCard == null)
			{
				continue;
			}
			bool flag = false;
			if (isTokenAlly)
			{
				if (field.AllyInplayCards.Count((AIVirtualCard card) => !card.IsDead) < 5)
				{
					tokenCard.InitAtSummonToken(tokenOwner, situation, isSkillSummon);
					if (tokenType == AITokenType.Reanimate)
					{
						tokenCard.Reanimate(situation);
					}
					field.AllyInplayCards.Add(tokenCard);
					field.CardListSet.AddAllyInplayCard(tokenCard);
					field.SummonedCardContainer.AddSummonedCard(tokenCard);
					flag = true;
				}
			}
			else if (field.EnemyInplayCards.Count((AIVirtualCard card) => !card.IsDead) < 5)
			{
				tokenCard.InitAtSummonToken(tokenOwner, situation, isSkillSummon);
				field.EnemyInplayCards.Add(tokenCard);
				field.CardListSet.AddEnemyInplayCard(tokenCard);
				field.SummonedCardContainer.AddSummonedCard(tokenCard);
				field.EnemyTokenQueue.Enqueue(new Tuple<AIVirtualCard, AIVirtualCard>(tokenOwner, tokenCard));
				flag = true;
			}
			if (flag)
			{
				list = AIParamQuery.AddElementToList(tokenCard, list);
				ExecuteSummonTags(field, tokenCard, bestPlayPtn, situation);
				AIGetOnSimulationUtility.GetOnAtField(field, tokenCard, situation);
				AIRallySimulationUtility.ExecuteAppendRallyCount(field, tokenCard);
				field.AllActivateCountHolderIncrement(situation, AIPlayTagType.SummonActivateCount, tokenCard);
			}
		}
		if (list != null && list.Count > 0)
		{
			situation.RegisterOwnSummonedCardList(list);
			field.ExecuteWhenChangeInplayTags(bestPlayPtn, situation);
		}
	}

	public static void ExecuteSummonCardAll(AIVirtualCard tagOwner, AIVirtualField field, List<AIVirtualCard> candidates, AISituationInfo situation)
	{
		EnemyAI aI = field.AI;
		List<AIVirtualCard> list = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			bool isUpdateTokenIndex = false;
			AIVirtualCard aIVirtualCard = candidates[i];
			bool isInHand = aIVirtualCard.IsInHand;
			bool isAlly = aIVirtualCard.IsAlly;
			AIVirtualCard aIVirtualCard2 = aIVirtualCard;
			if (!isAlly)
			{
				aIVirtualCard2 = new AIVirtualCard(aIVirtualCard.BaseCard, field);
			}
			if (aI.IsPlagueCityTagged && aIVirtualCard2.IsUnit)
			{
				aIVirtualCard2 = aI.tokenManager.GetZombieToken(tagOwner.IsAlly, field, needsClone: true);
				isUpdateTokenIndex = true;
			}
			aIVirtualCard2.InitAtSummonToken(tagOwner, situation, isSkillSummon: true, isUpdateTokenIndex);
			if (isAlly)
			{
				if (isInHand)
				{
					field.RemoveAllyHandCard(aIVirtualCard);
				}
				field.AllyInplayCards.Add(aIVirtualCard2);
				field.CardListSet.AddAllyInplayCard(aIVirtualCard2);
			}
			else
			{
				if (isInHand)
				{
					field.RemoveEnemyHandCard(aIVirtualCard);
				}
				field.EnemyInplayCards.Add(aIVirtualCard2);
				field.CardListSet.AddEnemyInplayCard(aIVirtualCard2);
			}
			field.SummonedCardContainer.AddSummonedCard(aIVirtualCard2);
			list = AIParamQuery.AddElementToList(aIVirtualCard2, list);
			ExecuteSummonTags(field, aIVirtualCard2, field.BestPlayPtn, situation);
			AIGetOnSimulationUtility.GetOnAtField(field, aIVirtualCard2, situation);
			AIRallySimulationUtility.ExecuteAppendRallyCount(field, aIVirtualCard2);
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.SummonActivateCount, aIVirtualCard2);
		}
		situation.RegisterOwnSummonedCardList(list);
	}

	public static void ExecuteTargetSelectSummonToken(AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType whichTarget, AISituationInfo situation)
	{
		if (field.AllyInplayCards.Count < 5)
		{
			AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
			if (situationTarget == null || !situationTarget.HasTarget)
			{
				AIConsoleUtility.LogError("ExecuteTargetSelectSummonToken() error!! targetInfo is null");
			}
			else
			{
				ExecuteSummonCardAll(tagOwner, field, situationTarget.Targets, situation);
			}
		}
	}

	public static void ExecuteSummonTags(AIVirtualField field, AIVirtualCard summonCard, List<int> playPtn, AISituationInfo situation)
	{
		if (summonCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenSummon))
		{
			summonCard.TagCollectionContainer.SummonTags.RegisterConditionPassedTag(summonCard, summonCard, playPtn, situation);
		}
		List<AIVirtualCard> otherSummonTagHolders = field.CardListSet.OtherSummonTagHolders;
		if (otherSummonTagHolders == null)
		{
			return;
		}
		for (int i = 0; i < otherSummonTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = otherSummonTagHolders[i];
			if (!aIVirtualCard.IsSameCard(summonCard))
			{
				aIVirtualCard.TagCollectionContainer.OtherSummonTags.RegisterConditionPassedTag(aIVirtualCard, summonCard, playPtn, situation);
			}
		}
	}

	public static void ExecuteSummonToken(List<AIVirtualCard> tokenIdholderCandidates, List<AIScriptTokenBase> filters, AIPolishConvertedExpression tokenCount, AIScriptTokenArgType tokenSide, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = (int)tokenCount.EvalArg(tagOwner, playPtn, field);
		if (num > 0)
		{
			AITokenIdCollection bothSideTokenIdListFromFilter = GetBothSideTokenIdListFromFilter(tagOwner, field, tokenIdholderCandidates, filters, AITokenType.Default, tokenSide, AIScriptTokenArgType.ALL_SELECT, num, playPtn, situation);
			if (bothSideTokenIdListFromFilter != null && bothSideTokenIdListFromFilter.HasToken)
			{
				bothSideTokenIdListFromFilter.SummonAllTokenToField(field, tagOwner, situation);
			}
		}
	}

	public static void ExecuteDrawToken(List<AIVirtualCard> idHolderCandidates, List<AIScriptTokenBase> filters, AIPolishConvertedExpression tokenCount, AIScriptTokenArgType tokenSide, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = (int)tokenCount.EvalArg(tagOwner, playPtn, field);
		if (num > 0)
		{
			AITokenIdCollection bothSideTokenIdListFromFilter = GetBothSideTokenIdListFromFilter(tagOwner, field, idHolderCandidates, filters, AITokenType.NONE, tokenSide, AIScriptTokenArgType.ALL_SELECT, num, playPtn, situation);
			if (bothSideTokenIdListFromFilter != null && bothSideTokenIdListFromFilter.HasToken)
			{
				bothSideTokenIdListFromFilter.DrawAllTokenToField(field, tagOwner, situation);
			}
		}
	}

	public static List<AITokenInformation> GetOwnerSideTokenIds(List<AIVirtualCard> idHolderCandidates, List<AIScriptTokenBase> filters, AIPolishConvertedExpression tokenCount, AITokenType tokenType, AIScriptTokenArgType tagSideType, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = (int)tokenCount.EvalArg(tagOwner, playPtn, field);
		if (num <= 0)
		{
			return null;
		}
		AITokenIdCollection bothSideTokenIdListFromFilter = GetBothSideTokenIdListFromFilter(tagOwner, field, idHolderCandidates, filters, tokenType, tagSideType, AIScriptTokenArgType.ALL_SELECT, num, playPtn, situation);
		if (bothSideTokenIdListFromFilter != null)
		{
			if (tagOwner.IsAlly)
			{
				return bothSideTokenIdListFromFilter.AllyTokenIdList;
			}
			return bothSideTokenIdListFromFilter.OpponentTokenIdList;
		}
		return null;
	}

	public static AITokenIdCollection CreateTokenIdCollectionForReanimate(AIVirtualCard owner, int tokenId, bool tokenIsAlly)
	{
		AITokenIdCollection aITokenIdCollection = new AITokenIdCollection();
		aITokenIdCollection.Add(tokenId, AITokenType.Reanimate, tokenIsAlly);
		return aITokenIdCollection;
	}

	public static AITokenIdCollection CreateTokenIdCollection(AIVirtualCard owner, List<int> tokenIdList, bool tokenIsAlly, AITokenType tokenType)
	{
		AITokenIdCollection aITokenIdCollection = new AITokenIdCollection();
		for (int i = 0; i < tokenIdList.Count; i++)
		{
			aITokenIdCollection.Add(tokenIdList[i], tokenType, tokenIsAlly);
		}
		return aITokenIdCollection;
	}

	public static AITokenIdCollection CreateTokenIdCollectionFromIdList(AIVirtualCard owner, AIScriptTokenArgType tokenSide, List<int> idList, AITokenType tokenType)
	{
		if (idList == null || idList.Count <= 0)
		{
			return null;
		}
		AITokenIdCollection aITokenIdCollection = new AITokenIdCollection();
		for (int i = 0; i < idList.Count; i++)
		{
			RegisterTokenIdToCollection(aITokenIdCollection, idList[i], tokenType, tokenSide, owner);
		}
		return aITokenIdCollection;
	}

	public static AITokenIdCollection GetBothSideTokenIdListFromFilter(AIVirtualCard owner, AIVirtualField field, List<AIVirtualCard> candidateRange, List<AIScriptTokenBase> filters, AITokenType tokenType, AIScriptTokenArgType tagSideType, AIScriptTokenArgType selectType, int repeatCount, List<int> playPtn, AISituationInfo situation)
	{
		if (selectType == AIScriptTokenArgType.RANDOM_SELECT || selectType == AIScriptTokenArgType.RANDOM_MULTI_SELECT)
		{
			return null;
		}
		AITokenIdCollection aITokenIdCollection = new AITokenIdCollection();
		List<AIScriptTokenBase> list = null;
		AIScriptTokenArgType aIScriptTokenArgType = AIScriptTokenArgType.NONE;
		for (int i = 0; i < filters.Count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = filters[i];
			if (aIScriptTokenBase is AIScriptIDToken aIScriptIDToken)
			{
				RegisterTokenIdToCollection(aITokenIdCollection, aIScriptIDToken.ID, tokenType, tagSideType, owner);
			}
			else if (aIScriptTokenBase is AIScriptArgumentToken aIScriptArgumentToken && IsSelectedTargetArgument(aIScriptArgumentToken.ArgumentType))
			{
				aIScriptTokenArgType = aIScriptArgumentToken.ArgumentType;
			}
			else
			{
				list = AIParamQuery.AddElementToList(aIScriptTokenBase, list);
			}
		}
		if (situation == null)
		{
			aITokenIdCollection.MultiplyByRepeatCount(repeatCount);
			return aITokenIdCollection;
		}
		if (list != null && list.Count > 0)
		{
			List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(candidateRange, list, owner, playPtn, situation, isBlockDeadCard: false);
			if (list2 != null && list2.Count > 0)
			{
				for (int j = 0; j < list2.Count; j++)
				{
					AIVirtualCard aIVirtualCard = list2[j];
					int baseId = aIVirtualCard.BaseId;
					RegisterTokenIdToCollection(aITokenIdCollection, baseId, tokenType, tagSideType, owner, aIVirtualCard, isRegisterToPool: true);
				}
			}
		}
		if (aIScriptTokenArgType != AIScriptTokenArgType.NONE)
		{
			RegisterSelectedTargetFilterTokenIdToCollection(aITokenIdCollection, aIScriptTokenArgType, situation, owner, field, playPtn, tokenType, tagSideType);
		}
		aITokenIdCollection.MultiplyByRepeatCount(repeatCount);
		return aITokenIdCollection;
	}

	private static void RegisterSelectedTargetFilterTokenIdToCollection(AITokenIdCollection collection, AIScriptTokenArgType filter, AISituationInfo situation, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AITokenType tokenType, AIScriptTokenArgType tagSideType)
	{
		List<AIVirtualCard> list = null;
		if (filter == AIScriptTokenArgType.REAL_SKILL_TARGET)
		{
			if (situation.RealTargetInformationList != null && situation.RealTargetInformationList.Count > 0)
			{
				AIVirtualCardRealTargetInformation aIVirtualCardRealTargetInformation = situation.DequeueRealTargetInfo(owner, field);
				if (aIVirtualCardRealTargetInformation != null)
				{
					list = aIVirtualCardRealTargetInformation.TargetList;
				}
			}
		}
		else
		{
			AISelectedTargetInfo situationTarget = situation.GetSituationTarget(filter);
			if (situationTarget != null && situationTarget.HasTarget)
			{
				list = situationTarget.Targets;
			}
			else if ((situation.ActionType == AIOperationType.PLAY || situation.ActionType == AIOperationType.EVOLVE) && situation is AIVirtualTargetSelectAction situation2 && owner.IsSameCard(situation.Actor))
			{
				List<AIVirtualTargetSelectInfo> list2 = owner.CreateAIVirtualSelectInfo(field, situation2);
				if (list2 != null && list2.Count > 0)
				{
					list = GetPrespectedSelectedTargetListForSummonToken(list2, owner, field, playPtn, situation);
				}
			}
		}
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				int baseId = aIVirtualCard.BaseId;
				RegisterTokenIdToCollection(collection, baseId, tokenType, tagSideType, owner, aIVirtualCard, isRegisterToPool: true);
			}
		}
	}

	private static void RegisterTokenIdToCollection(AITokenIdCollection collection, int id, AITokenType tokenType, AIScriptTokenArgType sideType, AIVirtualCard tagOwner, AIVirtualCard idHolder = null, bool isRegisterToPool = false)
	{
		bool flag = false;
		bool flag2 = false;
		switch (sideType)
		{
		case AIScriptTokenArgType.BOTH:
			flag = true;
			flag2 = true;
			break;
		case AIScriptTokenArgType.ALLY:
			flag = tagOwner.IsAlly;
			flag2 = !tagOwner.IsAlly;
			break;
		case AIScriptTokenArgType.OPPONENT:
			flag = !tagOwner.IsAlly;
			flag2 = tagOwner.IsAlly;
			break;
		case AIScriptTokenArgType.SELECTED_TARGET_SIDE:
			if (idHolder == null)
			{
				AIConsoleUtility.LogError("AISummonTokenUtility.RegisterTokenIdToCollection() error!! idHolder is null!!!!!");
				break;
			}
			flag = idHolder.IsAlly;
			flag2 = !idHolder.IsAlly;
			break;
		}
		if (flag)
		{
			collection.Add(id, tokenType, isAllyToken: true);
		}
		if (flag2)
		{
			collection.Add(id, tokenType, isAllyToken: false);
		}
	}

	private static List<AIVirtualCard> GetPrespectedSelectedTargetListForSummonToken(List<AIVirtualTargetSelectInfo> selectInfoList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < selectInfoList.Count; i++)
		{
			AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = selectInfoList[i];
			switch (aIVirtualTargetSelectInfo.Type)
			{
			case TargetSelectType.Choice:
			{
				AISelectedTargetInfo choiceTargets = aIVirtualTargetSelectInfo.GetChoiceTargets(owner, field, playPtn, situation);
				if (choiceTargets != null && choiceTargets.HasTarget)
				{
					for (int k = 0; k < choiceTargets.Targets.Count; k++)
					{
						list = AIParamQuery.AddElementToList(choiceTargets.Targets[k], list);
					}
				}
				break;
			}
			case TargetSelectType.Default:
			{
				List<AISelectedTargetInfo> allDefaultTargetPattern = aIVirtualTargetSelectInfo.GetAllDefaultTargetPattern();
				if (aIVirtualTargetSelectInfo.RemovalType != AIRemovalType.Destroy)
				{
					break;
				}
				AISelectedTargetInfo aISelectedTargetInfo = ((!situation.IsTargetExists(AIScriptTokenArgType.TARGET_SELECT)) ? GetBestTokenPatternOfDestroySelection(allDefaultTargetPattern, playPtn, situation) : situation.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT));
				if (aISelectedTargetInfo != null && aISelectedTargetInfo.HasTarget)
				{
					for (int j = 0; j < aISelectedTargetInfo.Targets.Count; j++)
					{
						list = AIParamQuery.AddElementToList(aISelectedTargetInfo.Targets[j], list);
					}
				}
				break;
			}
			}
		}
		return list;
	}

	private static AISelectedTargetInfo GetBestTokenPatternOfDestroySelection(List<AISelectedTargetInfo> allSelectPatternList, List<int> playPtn, AISituationInfo situation)
	{
		if (allSelectPatternList == null || allSelectPatternList.Count <= 0)
		{
			return null;
		}
		AISelectedTargetInfo result = null;
		float num = float.MinValue;
		for (int i = 0; i < allSelectPatternList.Count; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = allSelectPatternList[i];
			if (!aISelectedTargetInfo.HasTarget)
			{
				continue;
			}
			float num2 = 0f;
			situation.SetTarget(aISelectedTargetInfo, AIScriptTokenArgType.TARGET_SELECT);
			for (int j = 0; j < aISelectedTargetInfo.Targets.Count; j++)
			{
				AIVirtualCard aIVirtualCard = aISelectedTargetInfo.Targets[j];
				float num3 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: true);
				if (!aIVirtualCard.IsIndestructible && !aIVirtualCard.IsIndependent)
				{
					num2 -= num3;
					num2 += aIVirtualCard.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
				}
				num2 += num3;
				if (!aIVirtualCard.IsAlly)
				{
					num2 *= -1f;
				}
			}
			situation.SetTarget(null, AIScriptTokenArgType.TARGET_SELECT);
			if (num2 > num)
			{
				result = aISelectedTargetInfo;
				num = num2;
			}
		}
		return result;
	}

	public static AIVirtualCard FirstSummonedFollowerTokenCandidate(List<AITokenInformation> tokenIdList, AIVirtualCard tokenOwner, AIVirtualField field, AISituationInfo situation, bool isTokenAlly, bool isSkillSummon, List<AIPlayTag> condList = null)
	{
		if (situation == null || tokenIdList == null || tokenIdList.Count <= 0)
		{
			return null;
		}
		if ((isTokenAlly ? field.AllyInplayCards.Count((AIVirtualCard card) => !card.IsDead) : field.EnemyInplayCards.Count((AIVirtualCard card) => !card.IsDead)) >= 5)
		{
			return null;
		}
		EnemyAI aI = field.AI;
		AIVirtualCard result = null;
		for (int num = 0; num < tokenIdList.Count; num++)
		{
			int tokenId = tokenIdList[num].TokenId;
			AIVirtualCard tokenCard = GetTokenCard(aI, tokenOwner, field, tokenId, isTokenAlly, condList);
			if (tokenCard != null && tokenCard.IsUnit)
			{
				tokenCard.PseudoInitAtSummonToken(tokenOwner, situation, isSkillSummon);
				result = tokenCard;
				break;
			}
		}
		return result;
	}

	private static AIVirtualCard GetTokenCard(EnemyAI ai, AIVirtualCard parentCard, AIVirtualField field, int tokenId, bool isTokenAlly, List<AIPlayTag> condList = null)
	{
		AIVirtualCard aIVirtualCard = ai.tokenManager.GetTokenFromId(tokenId, isTokenAlly, field, needsClone: true);
		if (aIVirtualCard == null)
		{
			AIConsoleUtility.LogError($"AISummonTokenUtility.GetTokenCard(): tokenId={tokenId} is null");
			return null;
		}
		if (ai.IsPlagueCityTagged && aIVirtualCard.IsUnit)
		{
			aIVirtualCard = ai.tokenManager.GetZombieToken(parentCard.IsAlly, field, needsClone: true);
		}
		return aIVirtualCard;
	}

	private static bool IsSelectedTargetArgument(AIScriptTokenArgType argType)
	{
		if (argType != AIScriptTokenArgType.SELECTED_TARGET && argType != AIScriptTokenArgType.SECOND_SELECTED_TARGET && argType != AIScriptTokenArgType.CHOICED_TARGET)
		{
			return argType == AIScriptTokenArgType.REAL_SKILL_TARGET;
		}
		return true;
	}

	public static bool GetIsTokenAlly(AIVirtualCard tagOwner, AIScriptTokenArgType side)
	{
		if (tagOwner.IsAlly)
		{
			return side == AIScriptTokenArgType.ALLY;
		}
		return side != AIScriptTokenArgType.ALLY;
	}
}
