using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.Card;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class AIEmoteCtrl : IAIEmoteCtrl
{

	private EnemyAI AI;

	private bool _isEmoOnDestroyPlayed;

	public AIEmoteCtrl(EnemyAI ai)
	{
		AI = ai;
	}

	public void SetUpEmoteEvent(BattlePlayerBase self, BattlePlayerBase opponent, OperateMgr operateManager)
	{
		opponent.OnTurnStartBeforeDraw += (SkillProcessor arg) => GetEmoteWithTurnStartSimulate(isAllyTurnStart: false);
		self.OnTurnStartBeforeDraw += delegate
		{
			AI.EmoteMng.ResetOpponentTurnEmoteFlag();
			return GetEmoteWithTurnStartSimulate(isAllyTurnStart: true);
		};
		Func<SkillProcessor, VfxBase> func = null;
		func = delegate
		{
			VfxBase emote = AI.GetEmote(AIEmoteCmdType.ON_OPPONENT_TURN_END);
			return NullVfx.GetInstance();
		};
		opponent.OnTurnEnd += func;
		self.OnAddCemeteryEvent += delegate(BattleCardBase card, BattlePlayerBase.CEMETERY_TYPE cemeteryType, bool isOpen, SkillBase skill)
		{
			if (cemeteryType == BattlePlayerBase.CEMETERY_TYPE.NORMAL && !(card is NullBattleCard) && !(card is IVirtualBattleCard) && card.SelfBattlePlayer.ClassAndInPlayCardList.Contains(card))
			{
				AIUnknownAction situation = new AIUnknownAction(AI.CurrentVirtualField.SearchVirtualCard(card));
				SequentialVfxPlayer sequential = SequentialVfxPlayer.Create();
				VfxBase emote = AI.GetEmote(AIEmoteCmdType.ON_CARD_DESTROY, situation);
				sequential.Register(emote);
				if (emote != NullVfx.GetInstance())
				{
					_isEmoOnDestroyPlayed = true;
					card.OnDestroy += (BattleCardBase destroyCard, SkillProcessor skillProcessor) => sequential;
				}
			}
		};
		self.Class.OnDamageAfter += (SkillProcessor skillProcessorOneTime) => (self.Class.Life <= 0) ? NullVfx.GetInstance() : AI.GetEmote(AIEmoteCmdType.ON_LEADER_DAMAGED);
		opponent.Class.OnDamageAfter += (SkillProcessor skillProcessorOneTime) => (opponent.Class.Life <= 0) ? NullVfx.GetInstance() : AI.GetEmote(AIEmoteCmdType.ON_PLAYER_LEADER_DAMAGED);
		operateManager.OnBeforeSetCard += delegate
		{
			AI.EmoteMng.EvalFieldOnBeforeSetCard();
		};
		operateManager.OnSetCardExecuted += delegate(BattleCardBase card)
		{
			AIUnknownAction situation = new AIUnknownAction(new AIVirtualCard(card, AI.CurrentVirtualField));
			return AI.GetEmote(AIEmoteCmdType.ON_CARD_PLAY_OPPONENT, situation);
		};
		operateManager.OnBeforeAttack += delegate
		{
			AI.EmoteMng.EvalFieldOnBeforeAttack();
			return NullVfx.GetInstance();
		};
		operateManager.OnAttackExecuted += (BattleCardBase attacker, BattleCardBase target, bool needAttack) => AI.GetEmote(AIEmoteCmdType.ON_OPPONENT_ATTACK);
	}

	private VfxBase GetEmoteWithTurnStartSimulate(bool isAllyTurnStart)
	{
		AIVirtualField aIVirtualField = new AIVirtualField((AI.BeforeLatestActionField != null) ? AI.BeforeLatestActionField : AI.CurrentVirtualField);
		if (aIVirtualField.AllyTurnCount > 0 || aIVirtualField.EnemyTurnCount > 0)
		{
			AIVirtualTurnEndSimulator.TurnEnd(new AIVirtualTurnEndInfo(isAllyTurnStart ? aIVirtualField.EnemyClass : aIVirtualField.AllyClass), aIVirtualField);
		}
		AIVirtualTurnStartInfo situation = new AIVirtualTurnStartInfo(isAllyTurnStart ? aIVirtualField.AllyClass : aIVirtualField.EnemyClass);
		aIVirtualField.TurnStartFieldProcess(situation);
		return AI.GetEmote(isAllyTurnStart ? AIEmoteCmdType.ON_ALLY_TURN_START : AIEmoteCmdType.ON_OPPONENT_TURN_START, situation);
	}

	public AIEmoteCmd OnOpponentEmotion(ClassCharaPrm.EmotionType emoteType)
	{
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		if (AI.IsThisTurnEmotePlayed)
		{
			return null;
		}
		if (!AI.EmoteQuery.UseInnerEmote)
		{
			return null;
		}
		ClassCharaPrm.EmotionType replyEmote = ClassCharaPrm.EmotionType.NULL;
		if (AI.EmoteMng.IsReplyAllowed(emoteType, ref replyEmote))
		{
			return new AIEmoteCmd(replyEmote);
		}
		return null;
	}

	public AIEmoteCmd OnAllyTurnStart(AISituationInfo situation)
	{
		AI.EmoteMng.SetUpOnAllyTurnStart();
		AI.EmoteMng.EvalFieldOnTurnStart();
		_isEmoOnDestroyPlayed = false;
		int emoteOnTurnStart = AI.StyleQuery.GetEmoteOnTurnStart(isAllyTurn: true, situation.Actor.SelfField);
		if (emoteOnTurnStart >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnStart);
		}
		return null;
	}

	public AIEmoteCmd OnOpponentTurnStart(AISituationInfo situation)
	{
		AI.EmoteMng.EvalFieldOnOpponentTurnStart();
		_isEmoOnDestroyPlayed = false;
		AIVirtualField selfField = situation.Actor.SelfField;
		int emoteOnTurnStart = AI.StyleQuery.GetEmoteOnTurnStart(isAllyTurn: false, selfField);
		if (emoteOnTurnStart >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnStart);
		}
		emoteOnTurnStart = AI.StyleQuery.GetPlayerEmoteOnTurnStart(selfField);
		if (emoteOnTurnStart >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnStart, isAlly: false);
		}
		return null;
	}

	public AIEmoteCmd OnAllyTurnEnd()
	{
		if (AI.IsTurnEndLethal)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(22);
		}
		for (int i = 0; i < AI.CurrentVirtualField.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			int emoteOnTurnEndCategory = AI.CurrentVirtualField.CardListSet.BothClassAndInplayCards[i].GetEmoteOnTurnEndCategory(isAllyTurnEnd: true);
			if (emoteOnTurnEndCategory >= 0)
			{
				return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEndCategory);
			}
		}
		int emoteOnTurnEnd = AI.StyleQuery.GetEmoteOnTurnEnd(isAllyTurn: true);
		if (emoteOnTurnEnd >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEnd);
		}
		emoteOnTurnEnd = AI.StyleQuery.GetPlayerEmoteOnTurnEnd(isPlayerTurn: false);
		if (emoteOnTurnEnd >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEnd, isAlly: false);
		}
		if (AI.EmoteMng.IsCheckmated())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(13);
		}
		if (AI.EmoteMng.IsHuntDown())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(14);
		}
		if (AI.IsRoyal() && AI.EmoteMng.IsEnoughUnit())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(102);
		}
		if (AI.IsNecromancer() && AI.EmoteMng.IsEnoughGrave())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(501);
		}
		if (AI.EmoteMng.IsAwaking(1))
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(401);
		}
		if (AI.EmoteMng.IsUnexpectedBattle())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(11);
		}
		if (AI.ALLY.InPlayCards.Count() < 5 && AI.EmoteMng.IsRemainTooPP())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(15);
		}
		return null;
	}

	public AIEmoteCmd OnOpponentTurnEnd()
	{
		if (AI.EmoteMng.IsOpponentTurnEmoteOccured)
		{
			return null;
		}
		for (int i = 0; i < AI.CurrentVirtualField.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			int emoteOnTurnEndCategory = AI.CurrentVirtualField.CardListSet.BothClassAndInplayCards[i].GetEmoteOnTurnEndCategory(isAllyTurnEnd: false);
			if (emoteOnTurnEndCategory >= 0)
			{
				return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEndCategory);
			}
		}
		int emoteOnTurnEnd = AI.StyleQuery.GetEmoteOnTurnEnd(isAllyTurn: false);
		if (emoteOnTurnEnd >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEnd);
		}
		emoteOnTurnEnd = AI.StyleQuery.GetPlayerEmoteOnTurnEnd(isPlayerTurn: true);
		if (emoteOnTurnEnd >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnTurnEnd, isAlly: false);
		}
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		if (AI.EmoteMng.IsOpponentEarnGreatMerit())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(33);
		}
		if (AI.OPPONENT.InPlayCards.Count() < 5 && AI.EmoteMng.IsOpponentRemainTooPP())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(31);
		}
		return null;
	}

	public AIEmoteCmd OnIterationStart()
	{
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		float num = AI.EmoteMng.EvalDisplacement_PlayedValue();
		AI.EmoteMng.ResetLastOpr();
		if (num < -4f)
		{
			return null;
		}
		if (num > 4f)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(1);
		}
		return null;
	}

	public AIEmoteCmd OnCardPlay(AISituationInfo situation)
	{
		if (situation == null || situation.Actor == null)
		{
			return null;
		}
		int num = AI.CurrentVirtualField.AllyHandCards.FindIndex((AIVirtualCard c) => c.IsSameCard(situation.Actor));
		if (num < 0)
		{
			return null;
		}
		AIVirtualField aIVirtualField = new AIVirtualField(AI.CurrentVirtualField)
		{
			BestPlayPtn = new List<int> { num }
		};
		AIVirtualCard aIVirtualCard = aIVirtualField.SearchVirtualCard(situation.Actor);
		AISelectedTargetInfoSet aISelectedTargetInfoSet = new AISelectedTargetInfoSet();
		if (situation.SelectedTargets != null)
		{
			for (int num2 = 0; num2 < AISelectedTargetInfoSet.LENGTH; num2++)
			{
				AISelectedTargetInfo aISelectedTargetInfo = situation.SelectedTargets.Get(num2);
				if (aISelectedTargetInfo == null || !aISelectedTargetInfo.HasTarget)
				{
					continue;
				}
				AISelectedTargetInfo aISelectedTargetInfo2 = new AISelectedTargetInfo(aISelectedTargetInfo.Type);
				int count = aISelectedTargetInfo.Targets.Count;
				for (int num3 = 0; num3 < count; num3++)
				{
					if (aISelectedTargetInfo.Type == TargetSelectType.Choice)
					{
						aISelectedTargetInfo2.AddTarget(aISelectedTargetInfo.Targets[num3]);
						continue;
					}
					AIVirtualCard target = aIVirtualField.SearchVirtualCard(aISelectedTargetInfo.Targets[num3]);
					aISelectedTargetInfo2.AddTarget(target);
				}
				aISelectedTargetInfoSet.Set(aISelectedTargetInfo2, num2);
			}
			if (situation.SelectedTargets.HasChoiceTarget)
			{
				aISelectedTargetInfoSet.SetChoiceTarget(situation.SelectedTargets.ChoiceTarget);
			}
		}
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, situation.ActionType, aISelectedTargetInfoSet);
		PlaySimulationInfo playSimulationInfo = AIPlayCardSimulationUtility.CreatePlaySimulationInfo(aIVirtualCard, aIVirtualTargetSelectAction, aIVirtualField);
		if (playSimulationInfo == null)
		{
			AIConsoleUtility.LogError("AIEmoteCtrl:OnCardPlay() playSimulationInfo is null. CardName:" + ((aIVirtualCard != null) ? aIVirtualCard.CardName : ""));
			return null;
		}
		if (playSimulationInfo.Type == PlaySimulationType.Accelerate || playSimulationInfo.Type == PlaySimulationType.Crystalize)
		{
			AISinglePlayptnRecord playptnRecord = AI.PlayPtnRecorder.FindMatchedPlayPtnRecord(aIVirtualField.BestPlayPtn, aIVirtualField);
			aIVirtualTargetSelectAction.SetActor(aIVirtualCard.FindRealActor(playptnRecord));
		}
		AIVirtualPlaySimulator.PlayCard(aIVirtualTargetSelectAction, aIVirtualField, playSimulationInfo);
		if (aIVirtualField.EnemyClass.Life <= 0 || aIVirtualField.EnemyClass.IsDead)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(22);
		}
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		AIRealBattleCardSearcher.SearchBattleCardFromSituation(AI.PlayerPair, situation, out var actor, out var targetList);
		if (actor == null || (situation.ActionTarget != null && (targetList == null || targetList.Count < 1 || targetList[0] == null)) || (situation.SecondActionTarget != null && (targetList == null || targetList.Count < 2 || targetList[1] == null)))
		{
			return null;
		}
		if (AI.ALLY.IsSelfTurn && AI.IsAllyCard(actor))
		{
			AIVirtualCard actor2 = situation.Actor;
			int emoteCategory = actor2.GetEmoteCategory(AIPlayTagType.EmoteOnPlay);
			if (emoteCategory >= 0)
			{
				AIEmoteCmd result = AI.EmoteQuery.SearchEmoteAtRandom(emoteCategory);
				AI.EmoteQuery.OnCardPlayEmotion();
				return result;
			}
			new BattlePlayerPair(actor.SelfBattlePlayer, actor.OpponentBattlePlayer);
			int num4 = actor2.BattleSkills.Count();
			for (int num5 = 0; num5 < num4; num5++)
			{
				SkillBase skill = actor2.BattleSkills.ElementAt(num5);
				if (AI.ParamQuery.IsBanishSkill(skill))
				{
					return OnBanishSkill(actor, aIVirtualField);
				}
			}
		}
		return null;
	}

	public AIEmoteCmd OnAllyAttack(AISituationInfo situation)
	{
		if (!(situation is AIVirtualAttackInfo { Actor: not null, AttackTarget: not null }))
		{
			AIConsoleUtility.LogError("OnAllyAttack() error!!! situation is not valid attackSituation");
			return null;
		}
		AI.EmoteMng.EvalFieldOnAttack();
		AIVirtualField aIVirtualField = new AIVirtualField(AI.CurrentVirtualField);
		AIVirtualAttackInfo aIVirtualAttackInfo2 = AISimulationUtility.CreateActionInfoOnFieldFromSituation(aIVirtualField, situation) as AIVirtualAttackInfo;
		if (!situation.IsSameSituation(aIVirtualAttackInfo2))
		{
			AIConsoleUtility.LogError("OnAllyAttack() error!!! Failed situation clone");
			return null;
		}
		AIVirtualAttackSimulator.Attack(aIVirtualAttackInfo2, aIVirtualField);
		if (aIVirtualField.EnemyClass.IsDead)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(22);
		}
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		int emoteCategory = aIVirtualAttackInfo2.Actor.GetEmoteCategory(AIPlayTagType.EmoteOnAtk);
		if (emoteCategory >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteCategory);
		}
		return null;
	}

	public AIEmoteCmd OnAllyEvolution(AISituationInfo situation)
	{
		if (situation == null || situation.Actor == null || situation.ActionType != AIOperationType.EVOLVE)
		{
			return null;
		}
		AIVirtualField aIVirtualField = new AIVirtualField(AI.CurrentVirtualField);
		AIVirtualActionInfo aIVirtualActionInfo = AISimulationUtility.CreateActionInfoOnFieldFromSituation(aIVirtualField, situation);
		if (!situation.IsSameSituation(aIVirtualActionInfo))
		{
			return null;
		}
		AIVirtualEvolutionSimulator.ManualEvolve(aIVirtualActionInfo, aIVirtualField);
		if (aIVirtualField.EnemyClass.IsDead)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(22);
		}
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		int emoteCategory = aIVirtualActionInfo.Actor.GetEmoteCategory(AIPlayTagType.EmoteOnEvo);
		if (emoteCategory >= 0)
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(emoteCategory);
		}
		return null;
	}

	public AIEmoteCmd OnBanishSkill(BattleCardBase actCard, AIVirtualField fieldAfterPlayed)
	{
		if (AI.EmoteMng.IsBanishOverExpected(actCard, fieldAfterPlayed))
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(21);
		}
		return null;
	}

	public AIEmoteCmd OnOpponentPlayCardExecuted(AISituationInfo situation)
	{
		if (AI.EmoteMng.IsCheckmated() || situation == null || situation.Actor == null)
		{
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		if (AI.EmoteMng.IsOpponentBanishSplendid(actor))
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(35);
		}
		if (AI.EmoteMng.IsOpponentBanishBad(actor))
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(36);
		}
		if (!actor.IsAlly && AI.EmoteMng.IsGiantPlayed())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(34);
		}
		if (AI.EmoteMng.IsOpponentHealClassLifeLargeOnCardPlay())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(37);
		}
		return null;
	}

	public AIEmoteCmd OnOpponentAttackExecuted()
	{
		if (AI.EmoteMng.IsCheckmated())
		{
			return null;
		}
		if (AI.EmoteMng.IsOpponentHealClassLifeLargeOnAttack())
		{
			return AI.EmoteQuery.SearchEmoteAtRandom(37);
		}
		return null;
	}

	public AIEmoteCmd OnCardDestroy(AISituationInfo situation)
	{
		if (situation == null || situation.Actor == null || situation.ActionType != AIOperationType.UNKNOWN)
		{
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		if (actor.IsAlly)
		{
			int emoteCategory = actor.GetEmoteCategory(AIPlayTagType.ForceEmoteOnDestroy);
			if (emoteCategory >= 0)
			{
				return AI.EmoteQuery.SearchEmoteAtRandom(emoteCategory);
			}
		}
		if (AI.EmoteMng.IsCheckmated() || _isEmoOnDestroyPlayed)
		{
			return null;
		}
		if (actor.IsAlly)
		{
			int emoteCategory2 = actor.GetEmoteCategory(AIPlayTagType.EmoteOnDestroy);
			if (emoteCategory2 >= 0)
			{
				return AI.EmoteQuery.SearchEmoteAtRandom(emoteCategory2);
			}
			if (AI.OPPONENT.IsSelfTurn && AI.EmoteMng.IsEliminatedAllyLegion(situation.Actor))
			{
				return AI.EmoteQuery.SearchEmoteAtRandom(101);
			}
		}
		return null;
	}

	public AIEmoteCmd OnLeaderDamaged(AIVirtualField field)
	{
		int emoteOnLeaderDamaged = AI.StyleQuery.GetEmoteOnLeaderDamaged(field);
		return AI.EmoteQuery.SearchEmoteAtRandom(emoteOnLeaderDamaged);
	}

	public AIEmoteCmd OnPlayerLeaderDamaged(AIVirtualField field)
	{
		int playerEmoteOnLeaderDamaged = AI.StyleQuery.GetPlayerEmoteOnLeaderDamaged(field);
		return AI.EmoteQuery.SearchEmoteAtRandom(playerEmoteOnLeaderDamaged, isAlly: false);
	}
}
