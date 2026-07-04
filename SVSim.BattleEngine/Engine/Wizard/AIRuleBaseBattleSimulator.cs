using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIRuleBaseBattleSimulator
{
	private EnemyAI _ai;

	private PlayPtnWithToken[] _bestPlayPtnWithToken;

	private bool _canChangeToTimeOverLogic;

	public AIRuleBaseBattleSimulator(EnemyAI ai, PlayPtnWithToken[] bestPlayPtnWithToken, bool canChangeToTimeOverLogic)
	{
		_ai = ai;
		_bestPlayPtnWithToken = bestPlayPtnWithToken;
		_canChangeToTimeOverLogic = canChangeToTimeOverLogic;
	}

	public void RunSequence(AIVirtualField originalField, int[] actionPermList, int evoIndex, List<int> enemyTargets, SimulationResult bestRecord, SimulationAdditionalActionInfoSet additionalOptions, InplayMovePatternFilter patternFilter, bool checkActFailure = true)
	{
		AIVirtualField aIVirtualField = new AIVirtualField(originalField);
		bool[] array = new bool[actionPermList.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = true;
		}
		AIVirtualActionInfo aIVirtualActionInfo = AISimulationUtility.CreateActionInfoOnFieldFromSituation(aIVirtualField, additionalOptions?.ForceEvoAction);
		List<AIVirtualActionInfo> actionInfoSequence = AISimulationUtility.GetActionInfoSequence(aIVirtualField, actionPermList, array, (aIVirtualActionInfo != null) ? (-1) : evoIndex);
		if (aIVirtualActionInfo != null && !actionInfoSequence.Any((AIVirtualActionInfo info) => info.ActionType == AIOperationType.EVOLVE))
		{
			bool flag = false;
			for (int num = 0; num < actionInfoSequence.Count; num++)
			{
				if (actionInfoSequence[num].Actor.IsSameCard(aIVirtualActionInfo.Actor))
				{
					actionInfoSequence.Insert(num, aIVirtualActionInfo);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				actionInfoSequence.Add(aIVirtualActionInfo);
			}
		}
		List<AIVirtualCard> enemyTargetSequence = AISimulationUtility.GetEnemyTargetSequence(aIVirtualField, enemyTargets);
		SimulationSetting simSetting = new SimulationSetting(isHandRemovalValid: true, useLeaderAttackPreCheck: true, aIVirtualField.IsNoSkipAttackInPlayPtn(), checkActFailure);
		ExecuteSimulationWithActionSequence(aIVirtualField, actionInfoSequence, enemyTargetSequence, simSetting, bestRecord, patternFilter);
	}

	private void ExecuteSimulationWithActionSequence(AIVirtualField fieldTemp, List<AIVirtualActionInfo> actionInfoSequence, List<AIVirtualCard> enemyTargetSequence, SimulationSetting simSetting, SimulationResult bestRecord, InplayMovePatternFilter patternFilter)
	{
		BattleSequencer.ExecSimulation(fieldTemp, actionInfoSequence, enemyTargetSequence, simSetting);
		if (NeedsChangeToTimeOverLogic())
		{
			return;
		}
		AIVirtualTurnEndInfo aIVirtualTurnEndInfo = new AIVirtualTurnEndInfo(fieldTemp.AllyClass);
		actionInfoSequence.Add(aIVirtualTurnEndInfo);
		BattleSequencer.OnAfterExecuteAttackCommon(fieldTemp, aIVirtualTurnEndInfo, ref actionInfoSequence, isAttackToLeaderAgain: true);
		if (NeedsChangeToTimeOverLogic())
		{
			return;
		}
		float num = AISimulationUtility.EvaluateField(fieldTemp, EvaluationType.SELF_TURN);
		if (NeedsChangeToTimeOverLogic())
		{
			return;
		}
		float num2 = num;
		if (_bestPlayPtnWithToken == null)
		{
			num2 = num - (fieldTemp.EnemyClass.IsDead ? 0f : fieldTemp.EvaluateThreaten());
		}
		if (AISimulationUtility.IsBestRecord(fieldTemp, num2, num, bestRecord))
		{
			bestRecord.BestResultField = fieldTemp;
			bestRecord.MaxFieldValue = num2;
			bestRecord.ActionSequence = actionInfoSequence;
			bestRecord.MaxFieldValueAtSelfTurnEnd = num;
			if (fieldTemp.EnemyClass.IsDead)
			{
				bestRecord.MinActionLengthOfLethal = fieldTemp.ActionLength;
			}
			patternFilter?.ConfirmBestPattern(enemyTargetSequence);
		}
	}

	private bool NeedsChangeToTimeOverLogic()
	{
		if (_ai.IsRunWeakLogic)
		{
			return !_canChangeToTimeOverLogic;
		}
		return false;
	}
}
