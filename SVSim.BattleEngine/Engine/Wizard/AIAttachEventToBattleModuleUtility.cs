using System;
using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public static class AIAttachEventToBattleModuleUtility
{
	public static void SetupEventWhenInitGame(BattlePlayerBase ally, BattlePlayerBase opponent, OperateMgr operateMgr, EnemyAI ai)
	{
		SetupOnAddCemeteryEvent(ally, opponent, ai);
		SetupOnTurnStartComplete(ally, opponent, ai);
		SetupOnTurnEndStart(ally, opponent, ai);
		SetupOperateMgrEvent(operateMgr, ai);
	}

	private static void SetupOnAddCemeteryEvent(BattlePlayerBase ally, BattlePlayerBase opponent, EnemyAI ai)
	{
		ai.PlayerBattleEvent.AllyAddCemeteryEvent = delegate(BattleCardBase card, BattlePlayerBase.CEMETERY_TYPE cemeteryType, bool isOpen, SkillBase skill)
		{
			if (cemeteryType == BattlePlayerBase.CEMETERY_TYPE.NORMAL && !(card is NullBattleCard) && card.SelfBattlePlayer.ClassAndInPlayCardList.Contains(card) && card.HasSkillWhenDestroy && ai.LatestAction != null)
			{
				ai.LatestAction.CreateRandomTargetInformation(card, (SkillBase skillBase) => skillBase.IsWhenDestroySkill && RegisterTool.IsSkillRandom(skillBase));
			}
		};
		ally.OnAddCemeteryEvent += ai.PlayerBattleEvent.AllyAddCemeteryEvent;
		ai.PlayerBattleEvent.OpponentAddCemeteryEvent = delegate(BattleCardBase card, BattlePlayerBase.CEMETERY_TYPE cemeteryType, bool isOpen, SkillBase skill)
		{
			if (cemeteryType == BattlePlayerBase.CEMETERY_TYPE.NORMAL && !(card is NullBattleCard) && card.SelfBattlePlayer.ClassAndInPlayCardList.Contains(card))
			{
				RecordOpponentBarbarossa(card, ai);
				if (card.HasSkillWhenDestroy && ai.LatestAction != null)
				{
					ai.LatestAction.CreateRandomTargetInformation(card, (SkillBase skillBase) => skillBase.IsWhenDestroySkill && RegisterTool.IsSkillRandom(skillBase));
				}
			}
		};
		opponent.OnAddCemeteryEvent += ai.PlayerBattleEvent.OpponentAddCemeteryEvent;
	}

	private static void SetupOnTurnStartComplete(BattlePlayerBase ally, BattlePlayerBase opponent, EnemyAI ai)
	{
		ai.PlayerBattleEvent.AllyTurnStartCompleteEvent = delegate
		{
			ai.UpdateAICurrentVirtualField();
			AIVirtualField currentVirtualField = ai.CurrentVirtualField;
			if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
			{
				currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
			}
			ai.LatestAction = null;
			ai.BeforeLatestActionField = null;
		};
		ally.OnTurnStartComplete = (Action)Delegate.Combine(ally.OnTurnStartComplete, ai.PlayerBattleEvent.AllyTurnStartCompleteEvent);
		ai.PlayerBattleEvent.OpponentTurnStartCompleteEvent = delegate
		{
			if (!ai.IsBattleEnd)
			{
				if (ai.IsOpponentBarbarossaDestroyed)
				{
					ai.IsOpponentBarbarossaDestroyed = false;
				}
				ai.UpdateAICurrentVirtualField();
				AIVirtualField currentVirtualField = ai.CurrentVirtualField;
				if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
				{
					currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
				}
				ai.LatestAction = null;
				ai.BeforeLatestActionField = null;
			}
		};
		opponent.OnTurnStartComplete = (Action)Delegate.Combine(opponent.OnTurnStartComplete, ai.PlayerBattleEvent.OpponentTurnStartCompleteEvent);
	}

	private static void SetupOnTurnEndStart(BattlePlayerBase ally, BattlePlayerBase opponent, EnemyAI ai)
	{
		ai.PlayerBattleEvent.AllySelfTurnEndEvent = delegate
		{
			ai.LatestAction = new AIRealActionInformation(AIOperationType.TURNEND, ally.Class, ally.Class, null);
			ai.SaveBeforeLatestActionInformation();
			for (int i = 0; i < ally.ClassAndInPlayCardList.Count; i++)
			{
				ai.LatestAction.CreateRandomTargetInformation(ally.ClassAndInPlayCardList[i], (SkillBase skill) => skill.OnSelfTurnEndStart != 0 && RegisterTool.IsSkillRandom(skill));
			}
			for (int num = 0; num < opponent.ClassAndInPlayCardList.Count; num++)
			{
				ai.LatestAction.CreateRandomTargetInformation(opponent.ClassAndInPlayCardList[num], (SkillBase skill) => skill.OnOpponentTurnEndStart != 0 && RegisterTool.IsSkillRandom(skill));
			}
		};
		ally.OnTurnEndStart += ai.PlayerBattleEvent.AllySelfTurnEndEvent;
		ai.PlayerBattleEvent.OpponentSelfTurnEndEvent = delegate
		{
			ai.LatestAction = new AIRealActionInformation(AIOperationType.TURNEND, opponent.Class, opponent.Class, null);
			ai.SaveBeforeLatestActionInformation();
			for (int i = 0; i < opponent.ClassAndInPlayCardList.Count; i++)
			{
				ai.LatestAction.CreateRandomTargetInformation(opponent.ClassAndInPlayCardList[i], (SkillBase skill) => skill.OnSelfTurnEndStart != 0 && RegisterTool.IsSkillRandom(skill));
			}
			for (int num = 0; num < ally.ClassAndInPlayCardList.Count; num++)
			{
				ai.LatestAction.CreateRandomTargetInformation(ally.ClassAndInPlayCardList[num], (SkillBase skill) => skill.OnOpponentTurnEndStart != 0 && RegisterTool.IsSkillRandom(skill));
			}
		};
		opponent.OnTurnEndStart += ai.PlayerBattleEvent.OpponentSelfTurnEndEvent;
	}

	private static void SetupVirtualPairOnAfterPickCard(BattlePlayerPair pair, AIOperationSimulatorAccessor accessor)
	{
		pair.Self.OnAfterPickCard += delegate
		{
			accessor.UpdateCurrentField(pair, EnemyAI.EmptyPlayPtn);
		};
		pair.Opponent.OnAfterPickCard += delegate
		{
			accessor.UpdateCurrentField(pair, EnemyAI.EmptyPlayPtn);
		};
	}

	private static void SetupOperateMgrEvent(OperateMgr operateMgr, EnemyAI ai)
	{
		SetupOperateMgrOnSetCardSuccess(operateMgr, ai);
		SetupOperateMgrOnSetCardExecuted(operateMgr, ai);
		SetupOperateMgrOnEvolveSuccess(operateMgr, ai);
		SetupOperateMgrOnEvolveComplete(operateMgr, ai);
		SetupOperateMgrOnBeforeAttack(operateMgr, ai);
		SetupOperateMgrOnAttackExecuted(operateMgr, ai);
		SetupOperateMgrOnBeforeFusion(operateMgr, ai);
		SetupOperateMgrOnAfterFusion(operateMgr, ai);
	}

	private static void SetupOperateMgrOnSetCardSuccess(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnSetCardSuccessEvent = delegate(BattleCardBase originalCard, BattleCardBase playCard, IEnumerable<BattleCardBase> selectedCards)
		{
			ai.LatestAction = new AIRealActionInformation(AIOperationType.PLAY, playCard, originalCard, selectedCards);
			ai.SaveBeforeLatestActionInformation();
			ai.LatestAction.CreateRandomTargetInformation(playCard, (SkillBase skill) => skill.IsWhenPlaySkill && RegisterTool.IsSkillRandom(skill));
		};
		operateMgr.OnSetCardSuccess += ai.OprMgrBattleEvent.OnSetCardSuccessEvent;
	}

	private static void SetupOperateMgrOnSetCardExecuted(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnSetCardExecutedEvent = delegate
		{
			if (ai.IsBattleEnd)
			{
				return NullVfx.GetInstance();
			}
			ai.UpdateAICurrentVirtualField();
			AIVirtualField currentVirtualField = ai.CurrentVirtualField;
			if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
			{
				currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
			}
			ai.LatestAction = null;
			ai.BeforeLatestActionField = null;
			return NullVfx.GetInstance();
		};
		operateMgr.OnSetCardExecuted += ai.OprMgrBattleEvent.OnSetCardExecutedEvent;
	}

	private static void RecordOpponentBarbarossa(BattleCardBase destroyCard, EnemyAI ai)
	{
		if (destroyCard.BaseParameter.BaseCardId == 106241020)
		{
			ai.IsOpponentBarbarossaDestroyed = true;
		}
	}

	private static void SetupOperateMgrOnEvolveSuccess(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnEvolveSuccessEvent = delegate(BattleCardBase originalEvolver, BattleCardBase realEvolver, IEnumerable<BattleCardBase> selectedTargets)
		{
			ai.SaveBeforeLatestActionInformation();
			ai.LatestAction = new AIRealActionInformation(AIOperationType.EVOLVE, realEvolver, originalEvolver, selectedTargets);
			ai.LatestAction.CreateRandomTargetInformation(realEvolver, (SkillBase skill) => skill.IsWhenEvolveSkill && RegisterTool.IsSkillRandom(skill), forceCheckEvolveSkills: true);
		};
		operateMgr.OnEvolveSuccess += ai.OprMgrBattleEvent.OnEvolveSuccessEvent;
	}

	private static void SetupOperateMgrOnEvolveComplete(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnEvolveCompleteEvent = delegate
		{
			if (!ai.IsBattleEnd)
			{
				ai.UpdateAICurrentVirtualField();
				AIVirtualField currentVirtualField = ai.CurrentVirtualField;
				if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
				{
					currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
				}
				ai.LatestAction = null;
				ai.BeforeLatestActionField = null;
			}
			return NullVfx.GetInstance();
		};
		operateMgr.OnEvoleComplete += ai.OprMgrBattleEvent.OnEvolveCompleteEvent;
	}

	private static void SetupOperateMgrOnBeforeAttack(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnBeforeAttackEvent = delegate(BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessorOneTime)
		{
			ai.LatestAction = new AIRealActionInformation(AIOperationType.ATTACK, attacker, attacker, new List<BattleCardBase> { target });
			ai.SaveBeforeLatestActionInformation();
			ai.LatestAction.CreateRandomTargetInformation(attacker, (SkillBase skill) => skill.IsBeforAttackSkill && RegisterTool.IsSkillRandom(skill));
			return NullVfx.GetInstance();
		};
		operateMgr.OnBeforeAttack += ai.OprMgrBattleEvent.OnBeforeAttackEvent;
	}

	private static void SetupOperateMgrOnAttackExecuted(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnAttackExecutedEvent = delegate
		{
			if (!ai.IsBattleEnd)
			{
				ai.UpdateAICurrentVirtualField();
				AIVirtualField currentVirtualField = ai.CurrentVirtualField;
				if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
				{
					currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
				}
				ai.LatestAction = null;
				ai.BeforeLatestActionField = null;
			}
			return NullVfx.GetInstance();
		};
		operateMgr.OnAttackExecuted += ai.OprMgrBattleEvent.OnAttackExecutedEvent;
	}

	private static void SetupOperateMgrOnBeforeFusion(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnBeforeFusionEvent = delegate(BattleCardBase fusionActor, IEnumerable<BattleCardBase> targets)
		{
			ai.LatestAction = new AIRealActionInformation(AIOperationType.FUSION, fusionActor, fusionActor, targets);
			ai.SaveBeforeLatestActionInformation();
		};
		operateMgr.OnBeforeFusion += ai.OprMgrBattleEvent.OnBeforeFusionEvent;
	}

	private static void SetupOperateMgrOnAfterFusion(OperateMgr operateMgr, EnemyAI ai)
	{
		ai.OprMgrBattleEvent.OnAfterFusionEvent = delegate
		{
			if (!ai.IsBattleEnd)
			{
				ai.UpdateAICurrentVirtualField();
				AIVirtualField currentVirtualField = ai.CurrentVirtualField;
				if (ai.LatestAction != null && ai.BeforeLatestActionField != null)
				{
					currentVirtualField.UpdateTagInformationFromLatestAction(ai.LatestAction, ai.BeforeLatestActionField);
				}
				ai.LatestAction = null;
				ai.BeforeLatestActionField = null;
			}
			return NullVfx.GetInstance();
		};
		operateMgr.OnAfterFusion += ai.OprMgrBattleEvent.OnAfterFusionEvent;
	}

	public static void SetupVirtualPairEventForOperationSimulator(BattlePlayerPair pair, AIOperationSimulatorAccessor accessor)
	{
		SetupVirtualPairOnAfterPickCard(pair, accessor);
	}
}
