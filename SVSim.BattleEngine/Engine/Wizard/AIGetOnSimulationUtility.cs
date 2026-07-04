using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIGetOnSimulationUtility
{
	public static void RegisterGetOnTokenInPlayPtn(EnemyAI ai)
	{
		List<int> bestPlayPtn = ai.BestPlayPtn;
		AIVirtualField currentVirtualField = ai.CurrentVirtualField;
		if (bestPlayPtn == null || bestPlayPtn.Count <= 0 || !currentVirtualField.CardListSet.HasGetOnTagHolders)
		{
			return;
		}
		List<AIVirtualCard> getOnTagHolders = currentVirtualField.CardListSet.GetOnTagHolders;
		bool[] array = new bool[bestPlayPtn.Count];
		if (ai.EnemyAIPlay.BestPlayPtnWithToken == null)
		{
			AIConsoleUtility.LogError("AIGetOnSimulationUtility.RegisterGetOnTokenInPlayPtn() error!! BestPlayPtnWithToken is null!!");
			return;
		}
		AISinglePlayptnRecord record = ai.EnemyAIPlay.BestPlayPtnWithToken.Record;
		for (int i = 0; i < getOnTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = getOnTagHolders[i];
			if (!aIVirtualCard.IsOnField)
			{
				continue;
			}
			GetOnTagCollection getOnTags = aIVirtualCard.TagCollectionContainer.GetOnTags;
			for (int j = 0; j < bestPlayPtn.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = currentVirtualField.AllyHandCards[bestPlayPtn[j]];
				AIVirtualCard aIVirtualCard3 = record.FindRealActor(aIVirtualCard2);
				if (!array[j] && aIVirtualCard3.IsUnit)
				{
					AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard3, aIVirtualCard2, AIOperationType.PLAY);
					if (getOnTags.CanGetOn(aIVirtualCard, aIVirtualCard3, bestPlayPtn, situation))
					{
						ai.tokenManager.AddTokenFromId(aIVirtualCard3.BaseId, isAlly: true);
						array[j] = true;
						break;
					}
				}
			}
		}
	}

	public static void GetOnAtField(AIVirtualField field, AIVirtualCard summonCard, AISituationInfo situation)
	{
		if (summonCard == null || !summonCard.IsUnit || !field.CardListSet.HasGetOnTagHolders)
		{
			return;
		}
		List<AIVirtualCard> getOnTagHolders = field.CardListSet.GetOnTagHolders;
		for (int i = 0; i < getOnTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = getOnTagHolders[i];
			if (aIVirtualCard.TagCollectionContainer.GetOnTags.CanGetOn(aIVirtualCard, summonCard, field.BestPlayPtn, situation))
			{
				aIVirtualCard.GetOn(summonCard, situation);
				break;
			}
		}
	}

	public static void ExecuteGetOnTriggerTags(AIVirtualCard getOnCard, AIVirtualField field, AISituationInfo situation)
	{
		if (getOnCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenGetOn))
		{
			getOnCard.TagCollectionContainer.GetOnTriggerTags.RegisterPassedConditionTags(getOnCard, getOnCard, field.BestPlayPtn, situation);
		}
	}

	public static AIVirtualCard GetoffTokenOnVirtualField(int getOffId, AIVirtualCard tokenOwner, AIVirtualField field, AISituationInfo situation)
	{
		AIVirtualCard tokenFromId = field.AI.tokenManager.GetTokenFromId(getOffId, tokenOwner.IsAlly, field, needsClone: true);
		if (tokenFromId == null)
		{
			AIConsoleUtility.LogError("GetoffTokenOnVirtualField: baseTokenCard is null");
			return null;
		}
		bool flag = false;
		if (tokenOwner.IsAlly)
		{
			if (field.AllyInplayCards.Count((AIVirtualCard card) => !card.IsDead) < 5)
			{
				tokenFromId.InitAtSummonToken(tokenOwner, situation, isSkillSummon: false);
				field.AllyInplayCards.Add(tokenFromId);
				field.CardListSet.AddAllyInplayCard(tokenFromId);
				field.SummonedCardContainer.AddSummonedCard(tokenFromId);
				flag = true;
			}
		}
		else if (field.EnemyInplayCards.Count((AIVirtualCard card) => !card.IsDead) < 5)
		{
			tokenFromId.InitAtSummonToken(tokenOwner, situation, isSkillSummon: false);
			field.EnemyInplayCards.Add(tokenFromId);
			field.CardListSet.AddEnemyInplayCard(tokenFromId);
			field.EnemyTokenQueue.Enqueue(new Tuple<AIVirtualCard, AIVirtualCard>(tokenOwner, tokenFromId));
			field.SummonedCardContainer.AddSummonedCard(tokenFromId);
			flag = true;
		}
		if (flag)
		{
			ExecuteWhenGetOffTags(tokenOwner, field, EnemyAI.EmptyPlayPtn, situation);
			return tokenFromId;
		}
		return null;
	}

	public static void ExecuteWhenGetOffTags(AIVirtualCard tokenOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tokenOwner.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenGetOff))
		{
			tokenOwner.TagCollectionContainer.WhenGetOffTags.RegisterPaasedConditionTags(tokenOwner, field, playPtn, situation);
		}
	}
}
