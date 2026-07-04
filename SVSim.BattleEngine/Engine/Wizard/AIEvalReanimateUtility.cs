using System.Collections.Generic;

namespace Wizard;

public static class AIEvalReanimateUtility
{

	public static float EvalReanimate(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		if (argList.Count < 2)
		{
			AIConsoleUtility.LogError("EVAL_REANIMATE error!!! argList count = " + argList.Count);
			return 0f;
		}
		argList.Reverse();
		if (!TryGetIsSummonToAllySide(argList[0], out var isSummonToAllySide))
		{
			return 0f;
		}
		int reanimateCost = (int)argList[1].Value;
		argList.RemoveRange(0, 2);
		return EvalReanimate(tagOwner, isSummonToAllySide, reanimateCost, argList, playPtn, situation);
	}

	private static float EvalReanimate(AIVirtualCard tagOwner, bool isSummonAllyField, int reanimateCost, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> list = (((tagOwner.IsAlly && isSummonAllyField) || (!tagOwner.IsAlly && !isSummonAllyField)) ? selfField.CardListSet.AllyDestroyedCards : selfField.CardListSet.EnemyDestroyedCards);
		if (list == null || list.Count <= 0)
		{
			return 0f;
		}
		list = AIReanimateSimulationUtility.FilteringReanimateTargets(list, reanimateCost, tagOwner, filters, playPtn, situation);
		if (list != null && list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard target = list[i];
				num += EvalReanimateTargetValue(target, selfField, playPtn, situation);
			}
			num /= (float)list.Count;
		}
		return num * (isSummonAllyField ? 1f : (-1f));
	}

	public static float EvalReanimateTargetValue(AIVirtualCard target, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return ((float)(target.DefaultAttack * target.DefaultMaxAttackableCount) + (float)target.DefaultLife + field.StyleQuery.GetUnitBonus(field, target, playPtn)) * field.StyleQuery.GetUnitRate(field, target, playPtn) * target.EvaluateAllBattleBonusRate(playPtn, useOthersTag: true, useIgnoreInBattle: false, situation) + target.GetFieldBonus(playPtn) + target.GetReanimateBonus(playPtn);
	}

	private static bool TryGetIsSummonToAllySide(AIScriptTokenBase sideToken, out bool isSummonToAllySide)
	{
		if (sideToken is AIScriptArgumentToken { ArgumentType: var argumentType })
		{
			switch (argumentType)
			{
			case AIScriptTokenArgType.ALLY:
				isSummonToAllySide = true;
				return true;
			case AIScriptTokenArgType.OPPONENT:
				isSummonToAllySide = false;
				return true;
			}
		}
		AIConsoleUtility.LogError("EVAL_REANIMATE error!!! Argument[0] is not ALLY or ENEMY");
		isSummonToAllySide = true;
		return false;
	}
}
