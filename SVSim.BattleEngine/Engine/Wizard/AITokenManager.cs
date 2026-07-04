using System.Collections.Generic;

namespace Wizard;

public class AITokenManager
{

	private EnemyAI _ai;

	private AITokenPool _allyTokenPool;

	private AITokenPool _enemyTokenPool;

	private AITokenPool _allyChoiceTokenPool;

	private AITokenPool _enemyChoiceTokenPool;

	public void RegistPermanentlyToken()
	{
		AddTokenFromId(900511030, isAlly: true);
		AddTokenFromId(900511030, isAlly: false);
		AddTokenFromId(900811050, isAlly: true);
		AddTokenFromId(900811050, isAlly: false);
	}

	public void AddTokenFromId(int tokenId, bool isAlly, bool isChoice = false)
	{
		AITokenPool aITokenPool = ((!isChoice) ? (isAlly ? _allyTokenPool : _enemyTokenPool) : (isAlly ? _allyChoiceTokenPool : _enemyChoiceTokenPool));
		aITokenPool.AddTokenToPool(_ai, tokenId, isChoice);
	}

	public AIVirtualCard GetTokenFromId(int tokenId, bool isAlly, AIVirtualField field, bool needsClone = false)
	{
		AIVirtualCard tokenFromPool = (isAlly ? _allyTokenPool : _enemyTokenPool).GetTokenFromPool(tokenId);
		if (tokenFromPool == null)
		{
			return null;
		}
		if (needsClone)
		{
			return new AIVirtualCard(tokenFromPool, field);
		}
		tokenFromPool.ReplaceSelfField(field);
		return tokenFromPool;
	}

	public AIVirtualCard GetChoiceTokenFromId(int tokenId, bool isAlly)
	{
		return (isAlly ? _allyChoiceTokenPool : _enemyChoiceTokenPool).GetTokenFromPool(tokenId);
	}

	public AIVirtualCard GetZombieToken(bool isAlly, AIVirtualField field, bool needsClone = false)
	{
		return GetTokenFromId(900511030, isAlly, field, needsClone);
	}

	public AIVirtualCard GetPuppetToken(bool isAlly, AIVirtualField field, bool needsClone = false)
	{
		return GetTokenFromId(900811050, isAlly, field, needsClone);
	}

	public bool IsRegisteredToken(int tokenId, bool isAlly)
	{
		return (isAlly ? _allyTokenPool : _enemyTokenPool).IsRegisteredToken(tokenId);
	}

	public AITokenManager(BattlePlayerBase ally, BattlePlayerBase opponent, EnemyAI ai)
	{
		_allyTokenPool = new AITokenPool(ally);
		_enemyTokenPool = new AITokenPool(opponent);
		_allyChoiceTokenPool = new AITokenPool(ally);
		_enemyChoiceTokenPool = new AITokenPool(opponent);
		_ai = ai;
	}

	public void UpdateTokenPool(AIVirtualField field)
	{
		if (field == null)
		{
			AIConsoleUtility.LogError("UpdateTokenPool Error!!! AIVirtualField is NULL!!!");
			return;
		}
		for (int i = 0; i < field.CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.AllReferableCards[i];
			if (!aIVirtualCard.IsLeader)
			{
				AddTokenFromId(aIVirtualCard.BaseId, aIVirtualCard.IsAlly);
			}
			AddTokenFromCard(aIVirtualCard);
			if (aIVirtualCard.IsGetOn)
			{
				int getOnCardId = aIVirtualCard.GetOnCardId;
				if (!IsRegisteredToken(getOnCardId, aIVirtualCard.IsAlly))
				{
					AddTokenFromId(getOnCardId, aIVirtualCard.IsAlly);
					AIVirtualCard tokenFromId = GetTokenFromId(getOnCardId, aIVirtualCard.IsAlly, null);
					AddTokenFromCard(tokenFromId);
				}
			}
		}
		for (int j = 0; j < field.CardListSet.AllyDestroyedCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = field.CardListSet.AllyDestroyedCards[j];
			AddTokenFromId(aIVirtualCard2.BaseId, aIVirtualCard2.IsAlly);
		}
		for (int k = 0; k < field.CardListSet.EnemyDestroyedCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = field.CardListSet.EnemyDestroyedCards[k];
			AddTokenFromId(aIVirtualCard3.BaseId, aIVirtualCard3.IsAlly);
		}
	}

	public void AddTokenFromCard(AIVirtualCard card, List<int> preRegisteredTokenIdList = null)
	{
		AITokenIdCollection allRegisterTokenPoolInfo = card.TagCollectionContainer.GetAllRegisterTokenPoolInfo(card);
		if (allRegisterTokenPoolInfo != null && allRegisterTokenPoolInfo.HasToken)
		{
			List<int> list = ((preRegisteredTokenIdList != null) ? new List<int>(preRegisteredTokenIdList) : new List<int>());
			list.Add(card.BaseId);
			AddTokenFromRegisterInfo(allRegisterTokenPoolInfo, card.SelfField, list);
		}
	}

	private void AddTokenFromRegisterInfo(AITokenIdCollection idCollection, AIVirtualField field, List<int> registeredTokenIdList)
	{
		if (idCollection.HasAllyToken)
		{
			for (int i = 0; i < idCollection.AllyTokenIdList.Count; i++)
			{
				AITokenInformation aITokenInformation = idCollection.AllyTokenIdList[i];
				AddTokenFromId(aITokenInformation.TokenId, isAlly: true, aITokenInformation.IsChoice);
				if (registeredTokenIdList != null && !registeredTokenIdList.Contains(aITokenInformation.TokenId))
				{
					AIVirtualCard card = (aITokenInformation.IsChoice ? GetChoiceTokenFromId(aITokenInformation.TokenId, isAlly: true) : GetTokenFromId(aITokenInformation.TokenId, isAlly: true, field));
					AddTokenFromCard(card, registeredTokenIdList);
				}
			}
		}
		if (!idCollection.HasOpponentToken)
		{
			return;
		}
		for (int j = 0; j < idCollection.OpponentTokenIdList.Count; j++)
		{
			AITokenInformation aITokenInformation2 = idCollection.OpponentTokenIdList[j];
			AddTokenFromId(aITokenInformation2.TokenId, isAlly: false, aITokenInformation2.IsChoice);
			if (registeredTokenIdList != null && !registeredTokenIdList.Contains(aITokenInformation2.TokenId))
			{
				AIVirtualCard card2 = (aITokenInformation2.IsChoice ? GetChoiceTokenFromId(aITokenInformation2.TokenId, isAlly: false) : GetTokenFromId(aITokenInformation2.TokenId, isAlly: false, field));
				AddTokenFromCard(card2, registeredTokenIdList);
			}
		}
	}

	public static AIVirtualCard ProcessToken(BattleCardBase card, AIVirtualField field)
	{
		AIVirtualCard aIVirtualCard = new AIVirtualCard(card, field);
		aIVirtualCard.InitializeTags(field.ParamQuery, null, null);
		return aIVirtualCard;
	}
}
