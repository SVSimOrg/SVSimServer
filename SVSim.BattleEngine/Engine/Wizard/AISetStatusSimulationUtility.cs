using System.Collections.Generic;

namespace Wizard;

public static class AISetStatusSimulationUtility
{
	public static bool IsNoneSetValue(AIPolishConvertedExpression expression)
	{
		if (expression.TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken)
		{
			return aIScriptArgumentToken.ArgumentType == AIScriptTokenArgType.NONE;
		}
		return false;
	}

	public static void SetMaxStatusToAll(List<AIVirtualCard> targets, int attack, int life, AISituationInfo situation)
	{
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (attack >= 0)
			{
				aIVirtualCard.SetAttack(attack);
			}
			if (life >= 0)
			{
				aIVirtualCard.SetMaxLife(situation, life);
			}
		}
	}

	public static void SetMaxStatusToTarget(AISituationInfo situation, int attack, int life, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(whichTarget);
		if (situationTarget != null && situationTarget.HasTarget)
		{
			SetMaxStatusToAll(situationTarget.Targets, attack, life, situation);
		}
	}

	public static (AIVirtualCard allyLeader, AIVirtualCard enemyLeader) GetTargetSideLeaders(AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (tagOwner == null)
		{
			return (allyLeader: null, enemyLeader: null);
		}
		if (!tagOwner.IsAlly)
		{
			return (allyLeader: field.EnemyClass, enemyLeader: field.AllyClass);
		}
		return (allyLeader: field.AllyClass, enemyLeader: field.EnemyClass);
	}

	public static void SetLeaderMaxLife(AIVirtualCard tagOwner, int maxLife, AIScriptTokenArgType sideType, AIVirtualField field, AISituationInfo situation = null)
	{
		(AIVirtualCard, AIVirtualCard) targetSideLeaders = GetTargetSideLeaders(tagOwner, field);
		if (targetSideLeaders.Item1 == null || targetSideLeaders.Item2 == null)
		{
			AIConsoleUtility.LogError("AISetStatusSimulationUtility.SetLeaderMaxLife(): Failed to get the leader.");
			return;
		}
		switch (sideType)
		{
		case AIScriptTokenArgType.ALLY:
			targetSideLeaders.Item1.SetMaxLife(situation, maxLife);
			break;
		case AIScriptTokenArgType.OPPONENT:
			targetSideLeaders.Item2.SetMaxLife(situation, maxLife);
			break;
		case AIScriptTokenArgType.BOTH:
			targetSideLeaders.Item1.SetMaxLife(situation, maxLife);
			targetSideLeaders.Item2.SetMaxLife(situation, maxLife);
			break;
		default:
			AIConsoleUtility.LogError($"AISetStatusSimulationUtility.SetLeaderMaxLife(): Unexpected side type. type:{sideType}");
			break;
		}
	}
}
