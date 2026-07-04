using System.Collections.Generic;

namespace Wizard;

public class MoveFirstBonusPolicyCollection : AIPolicyCollection
{
	private struct MoveFirstBonusInfo
	{
		public bool IsLeastValue;

		public bool IsAllyAttackFollower;
	}

	public float GetMoveFirstBonus(AIVirtualCard card, AIVirtualField field, List<AIVirtualActionInfo> moves)
	{
		if (!base.HasPolicy)
		{
			return 0f;
		}
		AIVirtualActionInfo situation = moves[0];
		float num = 0f;
		MoveFirstBonusInfo moveFirstBonusInfo = GetMoveFirstBonusInfo(moves);
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (!(aIPolicyData.Argument is AIFirstMoveBonus { ActionArgType: var actionArgType } aIFirstMoveBonus))
			{
				continue;
			}
			switch (actionArgType)
			{
			case AIScriptTokenArgType.LEAST_VALUE:
				if (!moveFirstBonusInfo.IsLeastValue)
				{
					continue;
				}
				break;
			case AIScriptTokenArgType.ALLY_ATTACK_FOLLOWER:
				if (!moveFirstBonusInfo.IsAllyAttackFollower)
				{
					continue;
				}
				break;
			default:
				AIConsoleUtility.LogError($"MoveFirstBonusPolicyCollection.GetMoveFirstBonus(): Arg type is unsupported. type:[{aIFirstMoveBonus.ActionArgType}]");
				continue;
			case AIScriptTokenArgType.ALL:
				break;
			}
			if (aIPolicyData.CheckCondition(card, EnemyAI.EmptyPlayPtn, field, situation))
			{
				num += aIFirstMoveBonus.GetEvaluateValue(card, situation);
			}
		}
		return num;
	}

	private MoveFirstBonusInfo GetMoveFirstBonusInfo(List<AIVirtualActionInfo> moves)
	{
		AIVirtualCard actor = moves[0].Actor;
		bool flag = true;
		bool flag2 = false;
		if (moves.Count > 1)
		{
			float defaultValue = actor.DefaultValue;
			for (int i = 1; i < moves.Count; i++)
			{
				AIVirtualActionInfo aIVirtualActionInfo = moves[i];
				if (aIVirtualActionInfo.ActionType == AIOperationType.TURNEND)
				{
					break;
				}
				if (flag && aIVirtualActionInfo.Actor != null && aIVirtualActionInfo.Actor.DefaultValue < defaultValue)
				{
					flag = false;
				}
				if (!flag2 && aIVirtualActionInfo is AIVirtualAttackInfo aIVirtualAttackInfo && !aIVirtualAttackInfo.AttackTarget.IsLeader)
				{
					flag2 = true;
				}
				if (!flag && flag2)
				{
					break;
				}
			}
		}
		return new MoveFirstBonusInfo
		{
			IsLeastValue = flag,
			IsAllyAttackFollower = flag2
		};
	}
}
