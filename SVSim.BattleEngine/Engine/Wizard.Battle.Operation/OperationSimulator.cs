using System;
using System.Collections.Generic;
using Cute;

namespace Wizard.Battle.Operation;

public static class OperationSimulator
{

	public static BattlePlayerPair Play(BattlePlayerPair sourcePair, IBattleCardUniqueID playCardId, List<BattleCardBase> skillTargets, Action<BattlePlayerPair> OnVirtualPairCloned = null, Action<BattleCardBase> playCardSkillEvent = null)
	{
		sourcePair.Self.BattleMgr.InstanceIsForecast = true;
		bool isRecovery = sourcePair.Self.BattleMgr.IsRecovery;
		sourcePair.Self.BattleMgr.IsRecovery = true;
		BattlePlayerPair battlePlayerPair = sourcePair.VirtualClone(CloneActualFlags.All);
		OnVirtualPairCloned?.Call(battlePlayerPair);
		Prediction.CloneSkillsPreprocessAndBuffInfo(sourcePair, battlePlayerPair);
		List<BattleCardBase> first = Play_GetTargetsAndChoice(battlePlayerPair, skillTargets).first;
		ActionProcessor actionProcessor = new ActionProcessor(battlePlayerPair);
		BattleCardBase battleCardBase = battlePlayerPair.Self.HandCardList.FindFromCardId(playCardId);
		playCardSkillEvent?.Call(battleCardBase);
		battlePlayerPair.Self.SetupActionProcessorEvent(actionProcessor);
		battlePlayerPair.Opponent.SetupActionProcessorEvent(actionProcessor);
		actionProcessor.PlayCard(battleCardBase, first);
		sourcePair.Self.BattleMgr.IsRecovery = isRecovery;
		sourcePair.Self.BattleMgr.InstanceIsForecast = false;
		return battlePlayerPair;
	}

	public static Tuple<List<BattleCardBase>, List<int>> Play_GetTargetsAndChoice(BattlePlayerPair virtualPair, List<BattleCardBase> skillTargets)
	{
		List<BattleCardBase> list = null;
		List<int> list2 = null;
		if (skillTargets.IsNotNullOrEmpty())
		{
			list = new List<BattleCardBase>();
			foreach (BattleCardBase skillTarget in skillTargets)
			{
				BattleCardBase battleCardBase = FindSkillTargetCard(virtualPair, skillTarget);
				if (battleCardBase == null)
				{
					list.Add(skillTarget);
					if (list2 == null)
					{
						list2 = new List<int>();
					}
					list2.Add(skillTarget.Index);
				}
				else
				{
					list.Add(battleCardBase);
				}
			}
		}
		return new Tuple<List<BattleCardBase>, List<int>>(list, list2);
	}

	public static BattlePlayerPair Evolve(BattlePlayerPair sourcePair, IBattleCardUniqueID evolutionCardId, List<BattleCardBase> skillTargets, Action<BattlePlayerPair> OnVirtualPairCloned = null)
	{
		sourcePair.Self.BattleMgr.InstanceIsForecast = true;
		bool isRecovery = sourcePair.Self.BattleMgr.IsRecovery;
		sourcePair.Self.BattleMgr.IsRecovery = true;
		BattlePlayerPair battlePlayerPair = sourcePair.VirtualClone(CloneActualFlags.All);
		OnVirtualPairCloned?.Call(battlePlayerPair);
		Prediction.CloneSkillsPreprocessAndBuffInfo(sourcePair, battlePlayerPair);
		BattleCardBase[] first = Evolve_GetTargetsAndChoice(battlePlayerPair, skillTargets).first;
		BattleCardBase card = battlePlayerPair.Self.ClassAndInPlayCardList.FindFromCardId(evolutionCardId);
		new ActionProcessor(battlePlayerPair).Evolution(card, first);
		sourcePair.Self.BattleMgr.IsRecovery = isRecovery;
		sourcePair.Self.BattleMgr.InstanceIsForecast = false;
		return battlePlayerPair;
	}

	public static Tuple<BattleCardBase[], List<int>> Evolve_GetTargetsAndChoice(BattlePlayerPair virtualPair, List<BattleCardBase> skillTargets)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<int> list2 = null;
		if (skillTargets != null)
		{
			for (int i = 0; i < skillTargets.Count; i++)
			{
				BattleCardBase battleCardBase = FindSkillTargetCard(virtualPair, skillTargets[i]);
				if (battleCardBase == null)
				{
					list.Add(skillTargets[i]);
					if (list2 == null)
					{
						list2 = new List<int>();
					}
					list2.Add(skillTargets[i].Index);
				}
				else
				{
					list.Add(battleCardBase);
				}
			}
		}
		return new Tuple<BattleCardBase[], List<int>>(list.ToArray(), list2);
	}

	private static BattleCardBase FindSkillTargetCard(BattlePlayerPair pair, IBattleCardUniqueID skillTargetCardId)
	{
		BattleCardBase battleCardBase = pair.Self.ClassAndInPlayCardList.FindFromCardId(skillTargetCardId);
		if (battleCardBase != null)
		{
			return battleCardBase;
		}
		BattleCardBase battleCardBase2 = pair.Self.HandCardList.FindFromCardId(skillTargetCardId);
		if (battleCardBase2 != null)
		{
			return battleCardBase2;
		}
		BattleCardBase battleCardBase3 = pair.Opponent.ClassAndInPlayCardList.FindFromCardId(skillTargetCardId);
		if (battleCardBase3 != null)
		{
			return battleCardBase3;
		}
		BattleCardBase battleCardBase4 = pair.Opponent.HandCardList.FindFromCardId(skillTargetCardId);
		if (battleCardBase4 != null)
		{
			return battleCardBase4;
		}
		return null;
	}

	public static BattlePlayerPair TurnStart(BattlePlayerPair sourcePair)
	{
		sourcePair.Self.BattleMgr.InstanceIsForecast = true;
		bool isRecovery = sourcePair.Self.BattleMgr.IsRecovery;
		sourcePair.Self.BattleMgr.IsRecovery = true;
		BattlePlayerPair battlePlayerPair = sourcePair.VirtualClone(CloneActualFlags.All);
		Prediction.CloneSkillsPreprocessAndBuffInfo(sourcePair, battlePlayerPair);
		battlePlayerPair.Self.IsSelfTurn = true;
		battlePlayerPair.Opponent.IsSelfTurn = false;
		SkillProcessor skillProcessor = new SkillProcessor();
		foreach (BattleCardBase inPlayCard in battlePlayerPair.Self.InPlayCards)
		{
			inPlayCard.TurnStart(skillProcessor);
		}
		foreach (BattleCardBase inPlayCard2 in battlePlayerPair.Opponent.InPlayCards)
		{
			inPlayCard2.OpponentTurnStart(skillProcessor);
		}
		skillProcessor.Process(battlePlayerPair);
		sourcePair.Self.BattleMgr.IsRecovery = isRecovery;
		sourcePair.Self.BattleMgr.InstanceIsForecast = false;
		return battlePlayerPair;
	}
}
