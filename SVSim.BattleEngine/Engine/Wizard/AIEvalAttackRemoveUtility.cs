using System.Collections.Generic;

namespace Wizard;

public static class AIEvalAttackRemoveUtility
{

	public static float EvalAttackRemove(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<AIScriptTokenBase> argList)
	{
		List<AIVirtualCard> list = (tagOwner.IsAlly ? field.EnemyInplayCards : field.AllyInplayCards);
		if (list.Count <= 0)
		{
			return 0f;
		}
		List<AIVirtualCard> list2 = null;
		List<AIVirtualCard> list3 = null;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsCantUnderAnyAttack())
			{
				if (aIVirtualCard.IsGuard)
				{
					list2 = AIParamQuery.AddElementToList(aIVirtualCard, list2);
				}
				else
				{
					list3 = AIParamQuery.AddElementToList(aIVirtualCard, list3);
				}
			}
		}
		int num = list2?.Count ?? 0;
		int num2 = list3?.Count ?? 0;
		if (num + num2 <= 0)
		{
			return 0f;
		}
		int removeCount = (int)argList[0].Value;
		AIScriptTokenArgType removeType = GetRemoveType(argList[1]);
		List<AIVirtualCard> replacedTargetSideInplayCardList = new List<AIVirtualCard>(list);
		return CalculateAttackRemoveValue(tagOwner, field, playPtn, removeCount, removeType, list2, list3, replacedTargetSideInplayCardList);
	}

	private static float CalculateAttackRemoveValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, int removeCount, AIScriptTokenArgType removeType, List<AIVirtualCard> guardList, List<AIVirtualCard> nonGuardList, List<AIVirtualCard> replacedTargetSideInplayCardList)
	{
		AIVirtualCard sourceCard = AIInstantAttackUtility.CreateDummyAttacker(field, tagOwner, 1, 1, 1, isEvalRush: true);
		Dictionary<int, (float, bool)> dictionary = null;
		if (removeCount > 1)
		{
			dictionary = new Dictionary<int, (float, bool)>();
		}
		float num = 0f;
		for (int i = 0; i < removeCount; i++)
		{
			List<AIVirtualCard> list = ((guardList == null || guardList.Count <= 0) ? nonGuardList : guardList);
			if (list == null)
			{
				break;
			}
			float num2 = float.MinValue;
			AIVirtualCard aIVirtualCard = null;
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = list[j];
				AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(sourceCard, aIVirtualCard2);
				bool isRemoved = false;
				if (!AIAttackSimulationUtility.IsAttackPossible(field, aIVirtualAttackInfo, replacedTargetSideInplayCardList))
				{
					continue;
				}
				float num3;
				if (removeCount > 1 && dictionary.ContainsKey(aIVirtualCard2.CardIndex))
				{
					(num3, isRemoved) = dictionary[aIVirtualCard2.CardIndex];
				}
				else
				{
					num3 = CalculateSingleRemoveValue(aIVirtualAttackInfo, field, playPtn, removeType, ref isRemoved);
					if (removeCount > 1)
					{
						dictionary.Add(aIVirtualCard2.CardIndex, (num3, isRemoved));
					}
				}
				if (num3 > num2)
				{
					num2 = num3;
					aIVirtualCard = aIVirtualCard2;
					flag = isRemoved;
				}
			}
			if (aIVirtualCard == null)
			{
				break;
			}
			num += num2;
			if (flag)
			{
				list.Remove(aIVirtualCard);
				replacedTargetSideInplayCardList.Remove(aIVirtualCard);
				if (removeCount > 1)
				{
					dictionary.Remove(aIVirtualCard.CardIndex);
				}
			}
		}
		AIInstantAttackUtility.RemoveDummyCardFromField(field);
		return num;
	}

	private static float CalculateSingleRemoveValue(AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, AIScriptTokenArgType removeType, ref bool isRemoved)
	{
		AIVirtualCard attackTarget = situation.AttackTarget;
		isRemoved = true;
		AIRemovalType removeType2;
		switch (removeType)
		{
		case AIScriptTokenArgType.DESTROY:
			removeType2 = AIRemovalType.Destroy;
			isRemoved = !attackTarget.IsIndependent && !attackTarget.IsIndestructible;
			break;
		case AIScriptTokenArgType.BANISH:
			removeType2 = AIRemovalType.Banish;
			isRemoved = !attackTarget.IsIndependent && !attackTarget.IsUnbanishable;
			break;
		default:
			AIConsoleUtility.LogError("AIEvalAttackRemoveUtility.CalculateSingleRemoveValue() error!! Not implemented removeType = " + removeType);
			isRemoved = false;
			return 0f;
		}
		if (isRemoved)
		{
			return AISimulationRemovalUtility.CalculateRemovalValue(attackTarget, field, playPtn, situation, removeType2, null);
		}
		return 0f;
	}

	private static AIScriptTokenArgType GetRemoveType(AIScriptTokenBase token)
	{
		if (token is AIScriptArgumentToken aIScriptArgumentToken)
		{
			return aIScriptArgumentToken.ArgumentType;
		}
		return AIScriptTokenArgType.NONE;
	}
}
