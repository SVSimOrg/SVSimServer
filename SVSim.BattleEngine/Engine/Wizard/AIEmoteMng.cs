using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Wizard;

public class AIEmoteMng
{
	private EnemyAI AI;

	private float _allyAdvOnTurnStart;

	private float _opponentAdvOnTurnStart;

	private int _allyLegionNumOnTurnStart;

	private float _allyAdvOnFirstAttack;

	private float _advOnBeforeSetCard;

	private int _opponentLifeOnBeforeSetCard;

	private int _opponentLifeOnBeforeAttack;

	private bool _isLastOpr_CardPlay;

	private float _playedCardValue;

	private float _allyAdvOnCardPlay;

	private bool _isFirstAttacked;

	private bool _isOpponentTurnEmoteOccured;

	private bool _isAIGreeted;

	private bool _isAIThanked;

	private readonly Stopwatch _stopwatch;

	private bool _isThinkingEmoteOccured;

	public Dictionary<int, int> EmoteCategoryPlayedCounter { get; private set; }

	public bool IsOpponentTurnEmoteOccured => _isOpponentTurnEmoteOccured;

	public int EmoteRandomSeed { get; private set; }

	public AIEmoteMng(EnemyAI ai)
	{
		AI = ai;
		EmoteRandomSeed = new Random().Next();
		_stopwatch = new Stopwatch();
		_isAIGreeted = false;
		_isAIThanked = false;
		EmoteCategoryPlayedCounter = new Dictionary<int, int>();
	}

	public float EvalDisplacement_PlayedValue()
	{
		if (!_isLastOpr_CardPlay)
		{
			return 0f;
		}
		return AI.CalcFieldAdvantage() - (_allyAdvOnCardPlay + _playedCardValue);
	}

	public void ResetLastOpr()
	{
		_isLastOpr_CardPlay = false;
	}

	public void SetUpOnAllyTurnStart()
	{
		_isFirstAttacked = false;
		ResetLastOpr();
		AI.EmoteQuery.UpdateCategoryInterval();
		_stopwatch.Reset();
		_stopwatch.Start();
		_isThinkingEmoteOccured = false;
	}

	public void EvalFieldOnTurnStart()
	{
		_allyAdvOnTurnStart = AI.CalcFieldAdvantage();
		_allyLegionNumOnTurnStart = AI.ALLY.InPlayCards.Count((BattleCardBase c) => c.IsTribe(CardBasePrm.TribeType.LEGION));
	}

	public void EvalFieldOnOpponentTurnStart()
	{
		_opponentAdvOnTurnStart = AI.CalcFieldAdvantage();
	}

	public void EvalFieldOnAttack()
	{
		if (!_isFirstAttacked)
		{
			_allyAdvOnFirstAttack = AI.CalcFieldAdvantage();
			_isFirstAttacked = true;
		}
	}

	public void EvalFieldOnBeforeSetCard()
	{
		_advOnBeforeSetCard = AI.CalcFieldAdvantage();
		_opponentLifeOnBeforeSetCard = AI.OPPONENT.Class.Life;
	}

	public void EvalFieldOnBeforeAttack()
	{
		_opponentLifeOnBeforeAttack = AI.OPPONENT.Class.Life;
	}

	public void EvalAllyOnCardPlay(AIVirtualCard playCard, AISinglePlayptnRecord playptnRecord)
	{
		if (playptnRecord != null && playptnRecord.PlayPtn != null && playptnRecord.PlayPtn.Count > 0)
		{
			_isLastOpr_CardPlay = true;
			_allyAdvOnCardPlay = AI.CalcFieldAdvantage();
			AIVirtualTargetSelectAction play = new AIVirtualTargetSelectAction(playCard, playCard, AIOperationType.PLAY);
			_playedCardValue = AI.EnemyAIPlay.EvaluateHandValue(playCard, playptnRecord.PlayPtn, play, isOnBattleSimulate: false, playptnRecord);
		}
	}

	public void OnOpponentTurnEmote()
	{
		_isOpponentTurnEmoteOccured = true;
	}

	public void ResetOpponentTurnEmoteFlag()
	{
		_isOpponentTurnEmoteOccured = false;
	}

	public bool IsOpponentEarnGreatMerit()
	{
		float num = AI.CalcFieldAdvantage();
		int ppTotal = AI.OPPONENT.PpTotal;
		return _opponentAdvOnTurnStart - num > (float)ppTotal * 2f;
	}

	public bool IsRemainTooPP()
	{
		return AI.ALLY.Pp >= 2;
	}

	public bool IsOpponentRemainTooPP()
	{
		return AI.OPPONENT.Pp > AI.OPPONENT.PpTotal / 2;
	}

	public bool IsGiantPlayed()
	{
		float num = 10f;
		return AI.FieldInplayValueDifference >= num;
	}

	public bool IsEnoughUnit()
	{
		return AI.ALLY.InPlayCards.Count() >= 3;
	}

	public bool IsEnoughGrave()
	{
		return AI.ALLY.CemeteryList.Count >= 6;
	}

	public bool IsCheckmated()
	{
		int num = 0;
		foreach (BattleCardBase inPlayCard in AI.OPPONENT.InPlayCards)
		{
			if (inPlayCard.IsUnit)
			{
				num += inPlayCard.Atk;
			}
		}
		if (AI.OPPONENT.IsEvolve && AI.OPPONENT.InPlayCards.Any((BattleCardBase card) => card.IsUnit && !card.IsEvolution))
		{
			num += AI.OPPONENT.InPlayCards.Max((BattleCardBase card) => (!card.IsEvolution) ? (card.BaseParameter.EvoAtk - card.BaseParameter.Atk) : 0);
		}
		int num2 = 0;
		List<AIVirtualCard> bothClassAndInplayCards = AI.CurrentVirtualField.CardListSet.BothClassAndInplayCards;
		for (int num3 = 0; num3 < bothClassAndInplayCards.Count; num3++)
		{
			AIVirtualCard aIVirtualCard = bothClassAndInplayCards[num3];
			if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.TurnEndDamage))
			{
				num2 += aIVirtualCard.TagCollectionContainer.TurnEndTags.GetTurnEndDamageToAllyLeader(aIVirtualCard);
			}
		}
		return num + num2 >= AI.ALLY.Class.Life;
	}

	public bool IsHuntDown()
	{
		if (AI.OPPONENT.InPlayCards.Any((BattleCardBase c) => c.SkillApplyInformation.IsGuard))
		{
			return false;
		}
		int num = 0;
		foreach (BattleCardBase inPlayCard in AI.ALLY.InPlayCards)
		{
			if (inPlayCard.IsUnit)
			{
				num += inPlayCard.Atk;
			}
		}
		if (AI.ALLY.CurrentEpCount > 0)
		{
			num += 2;
		}
		return num >= AI.OPPONENT.Class.Life;
	}

	public bool IsEliminatedAllyLegion(AIVirtualCard destroyingCard)
	{
		if (_allyLegionNumOnTurnStart < 2)
		{
			return false;
		}
		if (AI.CurrentVirtualField.AllyInplayCards.Count((AIVirtualCard c) => c.IsTribe(CardBasePrm.TribeType.LEGION) && c.IsSameCard(destroyingCard)) > 0)
		{
			return false;
		}
		return true;
	}

	public bool IsAwaking(int turnOffset)
	{
		if (AI.ALLY.Pp + turnOffset == 7)
		{
			return true;
		}
		return false;
	}

	public bool IsUnexpectedBattle()
	{
		if (!_isFirstAttacked)
		{
			return false;
		}
		return AI.CalcFieldAdvantage() < _allyAdvOnFirstAttack;
	}

	public bool IsBanishOverExpected(BattleCardBase actCard, AIVirtualField fieldAfterPlayed)
	{
		if (!actCard.IsSpell)
		{
			return false;
		}
		float num = (float)actCard.Cost * 2f + 4f;
		float num2 = AI.ParamQuery.EvaluateBattlePlayerPair(AI.CurrentVirtualField);
		if (AI.ParamQuery.EvaluateBattlePlayerPair(fieldAfterPlayed) - num2 >= num)
		{
			return true;
		}
		return false;
	}

	public bool IsOpponentBanishSplendid(AIVirtualCard actCard)
	{
		if (actCard.IsAlly)
		{
			return false;
		}
		float num = (float)actCard.Cost * 2f + 4f;
		float num2 = AI.CalcFieldAdvantage();
		if (_advOnBeforeSetCard - num2 >= num)
		{
			return true;
		}
		return false;
	}

	public bool IsOpponentBanishBad(AIVirtualCard actCard)
	{
		if (actCard.IsAlly)
		{
			return false;
		}
		float num = actCard.Cost;
		float num2 = AI.CalcFieldAdvantage();
		if (_advOnBeforeSetCard - num2 < num)
		{
			return true;
		}
		return false;
	}

	public bool IsOpponentHealClassLifeLargeOnCardPlay()
	{
		int num = 4;
		if (AI.OPPONENT.Class.Life - _opponentLifeOnBeforeSetCard >= num)
		{
			return true;
		}
		return false;
	}

	public bool IsOpponentHealClassLifeLargeOnAttack()
	{
		int num = 4;
		if (AI.OPPONENT.Class.Life - _opponentLifeOnBeforeAttack >= num)
		{
			return true;
		}
		return false;
	}

	public void OnOperationRequest()
	{
		if (!_isThinkingEmoteOccured)
		{
			_stopwatch.Reset();
			_stopwatch.Start();
		}
	}

	public bool IsReplyAllowed(ClassCharaPrm.EmotionType emoteType, ref ClassCharaPrm.EmotionType replyEmote)
	{
		switch (emoteType)
		{
		case ClassCharaPrm.EmotionType.GREET:
			if (_isAIGreeted)
			{
				return false;
			}
			replyEmote = ClassCharaPrm.EmotionType.GREET;
			_isAIGreeted = true;
			return true;
		case ClassCharaPrm.EmotionType.THANK:
			if (_isAIThanked)
			{
				return false;
			}
			replyEmote = ClassCharaPrm.EmotionType.THANK;
			_isAIThanked = true;
			return true;
		default:
			return false;
		}
	}

	public void AddPlayedCountOnEmotePlaying(int category)
	{
		if (EmoteCategoryPlayedCounter.ContainsKey(category))
		{
			EmoteCategoryPlayedCounter[category]++;
		}
		else
		{
			EmoteCategoryPlayedCounter.Add(category, 1);
		}
	}

	public int GetEmotePlayCount(int category)
	{
		if (EmoteCategoryPlayedCounter.ContainsKey(category))
		{
			return EmoteCategoryPlayedCounter[category];
		}
		return 0;
	}
}
