using System.Collections.Generic;

namespace Wizard;

public class AIVirtualFieldBuildParameterCollction
{
	public AIHealRecorderCollection HealRecorderCollection;

	public List<AICannotPlayInformation> CannotPlayInfoList;

	public AITagPreprocessCollectionContainer TagPreprocessContainer;

	public AIDamageModifierCollection DamageModifierCollection;

	public AIPlayedCardContainer PlayedCardContainer;

	public List<ReferableVirtualCardBuildParameterCollection> AllyInplayCardBuildParameterList;

	public List<ReferableVirtualCardBuildParameterCollection> OpponentInplayCardBuildParameterList;

	public List<ReferableVirtualCardBuildParameterCollection> AllyHandCardBuildParameterList;

	public List<NonReferableVirtualCardBuildParameterCollection> OpponentHandCardBuildParameterList;

	public ReferableVirtualCardBuildParameterCollection AllyClassBuildParameter;

	public ReferableVirtualCardBuildParameterCollection EnemyClassBuildParameter;

	public List<NonReferableVirtualCardBuildParameterCollection> AllyDestroyedCardBuildParameterList;

	public List<NonReferableVirtualCardBuildParameterCollection> OpponentDestroyedCardBuildParameterList;

	public AIVirtualFieldBuildParameterCollction(AIVirtualField field)
	{
		if (field == null)
		{
			HealRecorderCollection = null;
			CannotPlayInfoList = null;
			TagPreprocessContainer = null;
			DamageModifierCollection = null;
			PlayedCardContainer = null;
			AllyInplayCardBuildParameterList = null;
			OpponentInplayCardBuildParameterList = null;
			AllyHandCardBuildParameterList = null;
			OpponentHandCardBuildParameterList = null;
			AllyClassBuildParameter = null;
			EnemyClassBuildParameter = null;
			AllyDestroyedCardBuildParameterList = null;
			OpponentDestroyedCardBuildParameterList = null;
		}
		else
		{
			HealRecorderCollection = field.HealRecorderCollection;
			CannotPlayInfoList = field.CannotPlayInformationList;
			TagPreprocessContainer = field.TagPreprocessContainer;
			DamageModifierCollection = field.DamageModifierCollection;
			PlayedCardContainer = field.PlayedCardContainer;
			CreateInplayCardBuildParameterList(field);
		}
	}

	private void CreateInplayCardBuildParameterList(AIVirtualField field)
	{
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			ReferableVirtualCardBuildParameterCollection element = new ReferableVirtualCardBuildParameterCollection(field.AllyInplayCards[i]);
			AllyInplayCardBuildParameterList = AIParamQuery.AddElementToList(element, AllyInplayCardBuildParameterList);
		}
		for (int j = 0; j < field.EnemyInplayCards.Count; j++)
		{
			ReferableVirtualCardBuildParameterCollection element2 = new ReferableVirtualCardBuildParameterCollection(field.EnemyInplayCards[j]);
			OpponentInplayCardBuildParameterList = AIParamQuery.AddElementToList(element2, OpponentInplayCardBuildParameterList);
		}
		for (int k = 0; k < field.AllyHandCards.Count; k++)
		{
			ReferableVirtualCardBuildParameterCollection element3 = new ReferableVirtualCardBuildParameterCollection(field.AllyHandCards[k]);
			AllyHandCardBuildParameterList = AIParamQuery.AddElementToList(element3, AllyHandCardBuildParameterList);
		}
		List<AIVirtualCard> enemyHandCardList = field.GetEnemyHandCardList();
		for (int l = 0; l < enemyHandCardList.Count; l++)
		{
			NonReferableVirtualCardBuildParameterCollection element4 = new NonReferableVirtualCardBuildParameterCollection(enemyHandCardList[l]);
			OpponentHandCardBuildParameterList = AIParamQuery.AddElementToList(element4, OpponentHandCardBuildParameterList);
		}
		AllyClassBuildParameter = new ReferableVirtualCardBuildParameterCollection(field.AllyClass);
		EnemyClassBuildParameter = new ReferableVirtualCardBuildParameterCollection(field.EnemyClass);
		for (int m = 0; m < field.CardListSet.AllyDestroyedCards.Count; m++)
		{
			NonReferableVirtualCardBuildParameterCollection element5 = new NonReferableVirtualCardBuildParameterCollection(field.CardListSet.AllyDestroyedCards[m]);
			AllyDestroyedCardBuildParameterList = AIParamQuery.AddElementToList(element5, AllyDestroyedCardBuildParameterList);
		}
		for (int n = 0; n < field.CardListSet.EnemyDestroyedCards.Count; n++)
		{
			NonReferableVirtualCardBuildParameterCollection element6 = new NonReferableVirtualCardBuildParameterCollection(field.CardListSet.EnemyDestroyedCards[n]);
			OpponentDestroyedCardBuildParameterList = AIParamQuery.AddElementToList(element6, OpponentDestroyedCardBuildParameterList);
		}
	}

	public ReferableVirtualCardBuildParameterCollection GetReferableCardBuildParameter(AIVirtualCard card)
	{
		if (card.IsLeader)
		{
			if (!card.IsAlly)
			{
				return EnemyClassBuildParameter;
			}
			return AllyClassBuildParameter;
		}
		List<ReferableVirtualCardBuildParameterCollection> list = null;
		if (card.IsOnField)
		{
			list = (card.IsAlly ? AllyInplayCardBuildParameterList : OpponentInplayCardBuildParameterList);
		}
		else if (card.IsInHand)
		{
			list = (card.IsAlly ? AllyHandCardBuildParameterList : null);
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			ReferableVirtualCardBuildParameterCollection referableVirtualCardBuildParameterCollection = list[i];
			if (referableVirtualCardBuildParameterCollection.IsMatch(card))
			{
				return referableVirtualCardBuildParameterCollection;
			}
		}
		return null;
	}

	public NonReferableVirtualCardBuildParameterCollection GetEnemyHandCardBuildParameter(AIVirtualCard card)
	{
		if (card.IsAlly || !card.IsInHand)
		{
			return null;
		}
		if (OpponentHandCardBuildParameterList == null || OpponentHandCardBuildParameterList.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < OpponentHandCardBuildParameterList.Count; i++)
		{
			NonReferableVirtualCardBuildParameterCollection nonReferableVirtualCardBuildParameterCollection = OpponentHandCardBuildParameterList[i];
			if (nonReferableVirtualCardBuildParameterCollection.IsMatch(card))
			{
				return nonReferableVirtualCardBuildParameterCollection;
			}
		}
		return null;
	}

	public NonReferableVirtualCardBuildParameterCollection GetDestroyedCardBuildParameter(AIVirtualCard card)
	{
		List<NonReferableVirtualCardBuildParameterCollection> list = (card.IsAlly ? AllyDestroyedCardBuildParameterList : OpponentDestroyedCardBuildParameterList);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			NonReferableVirtualCardBuildParameterCollection nonReferableVirtualCardBuildParameterCollection = list[i];
			if (nonReferableVirtualCardBuildParameterCollection.IsMatch(card))
			{
				return nonReferableVirtualCardBuildParameterCollection;
			}
		}
		return null;
	}
}
