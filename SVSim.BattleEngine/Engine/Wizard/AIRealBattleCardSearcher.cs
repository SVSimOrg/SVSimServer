using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIRealBattleCardSearcher
{
	public static BattleCardBase SearchBattleCardFromVirtualCard(BattlePlayerPair playerPair, AIVirtualCard virtualCard)
	{
		BattlePlayerBase battlePlayerBase = (virtualCard.IsAlly ? playerPair.Self : playerPair.Opponent);
		if (virtualCard is ChoiceVirtualCard choiceVirtualCard)
		{
			return choiceVirtualCard.VirtualBattleCard;
		}
		BattleCardBase battleCardBase = null;
		battleCardBase = ((!virtualCard.IsInHand) ? FindBattleCardFromList(virtualCard, battlePlayerBase.ClassAndInPlayCardList) : FindBattleCardFromList(virtualCard, battlePlayerBase.HandCardList));
		if (battleCardBase == null)
		{
			return null;
		}
		return battleCardBase;
	}

	public static void SearchBattleCardFromSituation(BattlePlayerPair playerPair, AISituationInfo situation, out BattleCardBase actor, out List<BattleCardBase> targetList)
	{
		actor = null;
		targetList = null;
		if (situation == null || situation.Actor == null)
		{
			AIConsoleUtility.LogError("SearchBattleCardFromSituation() error!!! situation is null");
			return;
		}
		AIVirtualCard actor2 = situation.Actor;
		actor2.ResetPosition(situation.ActionType);
		actor = SearchBattleCardFromVirtualCard(playerPair, actor2);
		if (actor == null)
		{
			AIConsoleUtility.LogError("SearchBattleCardFromSituation() error!!! Not found actor");
		}
		if (situation.SelectedTargets == null)
		{
			return;
		}
		AISelectedTargetInfoSet selectedTargets = situation.SelectedTargets;
		AISelectedTargetInfo preprocessTarget = selectedTargets.PreprocessTarget;
		if (preprocessTarget != null && preprocessTarget.HasTarget)
		{
			for (int i = 0; i < preprocessTarget.Targets.Count; i++)
			{
				BattleCardBase battleCardBase = SearchBattleCardFromVirtualCard(playerPair, preprocessTarget.Targets[i]);
				if (battleCardBase != null)
				{
					targetList = AIParamQuery.AddElementToList(battleCardBase, targetList);
				}
			}
		}
		if (selectedTargets.HasChoiceTarget)
		{
			AISelectedTargetInfo choiceTarget = selectedTargets.ChoiceTarget;
			for (int j = 0; j < choiceTarget.Targets.Count; j++)
			{
				BattleCardBase battleCardBase2 = SearchBattleCardFromVirtualCard(playerPair, choiceTarget.Targets[j]);
				if (battleCardBase2 != null)
				{
					targetList = AIParamQuery.AddElementToList(battleCardBase2, targetList);
				}
			}
		}
		for (int k = 0; k < AISelectedTargetInfoSet.LENGTH; k++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = selectedTargets.Get(k);
			if (aISelectedTargetInfo == null || !aISelectedTargetInfo.HasTarget)
			{
				continue;
			}
			for (int l = 0; l < aISelectedTargetInfo.Targets.Count; l++)
			{
				BattleCardBase battleCardBase3 = SearchBattleCardFromVirtualCard(playerPair, aISelectedTargetInfo.Targets[l]);
				if (battleCardBase3 != null)
				{
					targetList = AIParamQuery.AddElementToList(battleCardBase3, targetList);
				}
			}
		}
	}

	public static void SearchAttackPairFromSituation(BattlePlayerPair playerPair, AIVirtualAttackInfo attackSituation, out BattleCardBase attacker, out BattleCardBase attackTarget)
	{
		attacker = null;
		attackTarget = null;
		if (attackSituation == null || attackSituation.Actor == null || attackSituation.AttackTarget == null)
		{
			AIConsoleUtility.LogError("SearchAttackPairFromSituation() error!!! situation is invalid");
			return;
		}
		attacker = SearchBattleCardFromVirtualCard(playerPair, attackSituation.Actor);
		if (attacker == null)
		{
			AIConsoleUtility.LogError("SearchAttackPairFromSituation() error!!! Not found attacker");
			return;
		}
		attackTarget = SearchBattleCardFromVirtualCard(playerPair, attackSituation.AttackTarget);
		if (attackTarget == null)
		{
			AIConsoleUtility.LogError("SearchAttackPairFromSituation() error!!! Not found attackTarget");
		}
	}

	private static BattleCardBase FindBattleCardFromList(AIVirtualCard virtualCard, List<BattleCardBase> battleCardList)
	{
		return battleCardList.FirstOrDefault((BattleCardBase c) => c.Index == virtualCard.CardIndex);
	}
}
