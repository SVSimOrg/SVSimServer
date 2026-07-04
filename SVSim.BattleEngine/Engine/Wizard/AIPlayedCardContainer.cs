using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIPlayedCardContainer
{
	private List<AIVirtualCard> _allyTurnPlayedCardList;

	private List<AIVirtualCard> _allyInGamePlayedCardList;

	private List<AIVirtualCard> _allyPreviousTurnPlayedCardList;

	private List<AIVirtualCard> _enemyTurnPlayedCardList;

	private List<AIVirtualCard> _enemyInGamePlayedCardList;

	private List<AIVirtualCard> _enemyPreviousTurnPlayedCardList;

	public int AllyTurnPlayedCardCount => _allyTurnPlayedCardList.Count;

	public int AllyInGamePlayedCardCount => _allyInGamePlayedCardList.Count;

	public AIPlayedCardContainer()
	{
		_allyTurnPlayedCardList = new List<AIVirtualCard>();
		_allyInGamePlayedCardList = new List<AIVirtualCard>();
		_allyPreviousTurnPlayedCardList = new List<AIVirtualCard>();
		_enemyTurnPlayedCardList = new List<AIVirtualCard>();
		_enemyInGamePlayedCardList = new List<AIVirtualCard>();
		_enemyPreviousTurnPlayedCardList = new List<AIVirtualCard>();
	}

	public AIPlayedCardContainer(AIPlayedCardContainer source)
	{
		_allyTurnPlayedCardList = new List<AIVirtualCard>(source._allyTurnPlayedCardList);
		_allyInGamePlayedCardList = new List<AIVirtualCard>(source._allyInGamePlayedCardList);
		_allyPreviousTurnPlayedCardList = new List<AIVirtualCard>(source._allyPreviousTurnPlayedCardList);
		_enemyTurnPlayedCardList = new List<AIVirtualCard>(source._enemyTurnPlayedCardList);
		_enemyInGamePlayedCardList = new List<AIVirtualCard>(source._enemyInGamePlayedCardList);
		_enemyPreviousTurnPlayedCardList = new List<AIVirtualCard>(source._enemyPreviousTurnPlayedCardList);
	}

	public AIPlayedCardContainer Clone()
	{
		return new AIPlayedCardContainer(this);
	}

	private void CopyPreviousPlayCardOnTurnShift(AIVirtualTurnStartInfo turnStartSituation)
	{
		if (turnStartSituation != null)
		{
			if (turnStartSituation.Actor.IsAlly)
			{
				_enemyPreviousTurnPlayedCardList = new List<AIVirtualCard>(_enemyTurnPlayedCardList);
			}
			else
			{
				_allyPreviousTurnPlayedCardList = new List<AIVirtualCard>(_allyTurnPlayedCardList);
			}
		}
	}

	public void ClearInTurn(AIVirtualTurnStartInfo turnStartSituation = null)
	{
		CopyPreviousPlayCardOnTurnShift(turnStartSituation);
		_allyTurnPlayedCardList.Clear();
		_enemyTurnPlayedCardList.Clear();
	}

	public int GetPlayedCountInTurn(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.MultipleFiltering(tagOwner.IsAlly ? _allyTurnPlayedCardList : _enemyTurnPlayedCardList, filters, tagOwner, playPtn, situation, isBlockDeadCard: false)?.Count ?? 0;
	}

	public int GetPlayedCountInPreviousTurn(bool isAllyList, List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.MultipleFiltering(isAllyList ? _allyPreviousTurnPlayedCardList : _enemyPreviousTurnPlayedCardList, filters, tagOwner, playPtn, situation, isBlockDeadCard: false)?.Count ?? 0;
	}

	public int GetAllPlayedCountInTurn(bool isAITurn)
	{
		return (isAITurn ? _allyTurnPlayedCardList : _enemyTurnPlayedCardList)?.Count ?? 0;
	}

	public int GetPlayedCountInGame(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		num = ((!AIPlayPtnUtility.IsInPlayPtn(tagOwner, playPtn)) ? (num + AIPlayPtnUtility.GetPlayPtnCardCount(filters, tagOwner, playPtn, situation)) : (num + AIPlayPtnUtility.GetBeforePlayPtnCount(filters, tagOwner, playPtn, situation)));
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(tagOwner.IsAlly ? _allyInGamePlayedCardList : _enemyInGamePlayedCardList, filters, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list != null)
		{
			num += list.Count;
		}
		return num;
	}

	public int GetPlayedNameCount(List<AIScriptTokenBase> filters, AIScriptTokenArgType turnOrGame, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		switch (turnOrGame)
		{
		case AIScriptTokenArgType.TURN:
			list = (tagOwner.IsAlly ? _allyTurnPlayedCardList : _enemyTurnPlayedCardList);
			break;
		case AIScriptTokenArgType.GAME:
			list = (tagOwner.IsAlly ? _allyInGamePlayedCardList : _enemyInGamePlayedCardList);
			break;
		default:
			AIConsoleUtility.LogError($"GetPlayedNameCount() error!! turnOrGame == {turnOrGame}");
			return 0;
		}
		if (list == null || list.Count <= 0)
		{
			return 0;
		}
		return AIFilteringUtility.GetCardNameCountFromList(list, filters, tagOwner, playPtn, situation);
	}

	public bool IsPlayedAsAccelerate(AIVirtualCard card)
	{
		CardParameter baseParameter = card.BaseCard.BaseParameter;
		if (!card.IsSpell || baseParameter.BaseCardId == baseParameter.ResourceCardId || !IsPlayedCard(card))
		{
			return false;
		}
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(baseParameter.ResourceCardId);
		if (cardParameterFromId == null)
		{
			return false;
		}
		if (cardParameterFromId.Cost > baseParameter.Cost && cardParameterFromId.CharType == CardBasePrm.CharaType.NORMAL)
		{
			return true;
		}
		return false;
	}

	public bool IsPlayedAsCrystalize(AIVirtualCard card)
	{
		if (!IsPlayedCard(card) || !card.IsAmulet)
		{
			return false;
		}
		if (card.BaseCard.TransformInfo.Type != BattleCardBase.TransformType.Crystallize)
		{
			return false;
		}
		return true;
	}

	public bool IsPlayedCard(AIVirtualCard card)
	{
		return (card.IsAlly ? _allyInGamePlayedCardList : _enemyInGamePlayedCardList).Any((AIVirtualCard c) => c.IsSameCard(card));
	}

	public void AddPlayedCard(AIVirtualCard card)
	{
		if (!IsPlayedCard(card))
		{
			List<AIVirtualCard> container = (card.IsAlly ? _allyTurnPlayedCardList : _enemyTurnPlayedCardList);
			AIParamQuery.AddElementToList(card, container);
			List<AIVirtualCard> container2 = (card.IsAlly ? _allyInGamePlayedCardList : _enemyInGamePlayedCardList);
			AIParamQuery.AddElementToList(card, container2);
		}
	}

	public void RollBackFromOneRecord(AIVirtualFieldRollBackRecord.PlayedCardContainerRecord rollBackRecord)
	{
		for (int num = _allyTurnPlayedCardList.Count - 1; num >= rollBackRecord.AllyTurnPlayedCardCount; num--)
		{
			_allyTurnPlayedCardList.RemoveAt(num);
		}
		for (int num2 = _allyInGamePlayedCardList.Count - 1; num2 >= rollBackRecord.AllyInGamePlayedCardCount; num2--)
		{
			_allyInGamePlayedCardList.RemoveAt(num2);
		}
	}
}
