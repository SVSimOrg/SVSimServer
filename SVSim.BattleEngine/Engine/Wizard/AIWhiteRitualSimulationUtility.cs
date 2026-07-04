using System.Collections.Generic;

namespace Wizard;

public static class AIWhiteRitualSimulationUtility
{
	public static void SimulateStack(AIVirtualCard stackOwner, AIVirtualField field, AISituationInfo situation, int defaultStack)
	{
		int num = defaultStack;
		List<AIVirtualCard> list = (stackOwner.IsAlly ? field.AllyInplayCards : field.EnemyInplayCards);
		if (list == null || list.Count <= 0)
		{
			stackOwner.SetWhiteRitual(num);
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsAmulet && aIVirtualCard.IsStackWhiteRitual && aIVirtualCard.WhiteRitualCount > 0 && !aIVirtualCard.IsSameCard(stackOwner))
			{
				aIVirtualCard.RemoveCard(situation, AIRemovalType.Banish, isFromSkill: true);
				if (aIVirtualCard.IsDead)
				{
					num += aIVirtualCard.WhiteRitualCount;
				}
			}
		}
		stackOwner.SetWhiteRitual(num);
	}

	public static bool AddWhiteRitualTargetCard(int count, AIVirtualCard target)
	{
		if (target == null)
		{
			AIConsoleUtility.LogError("AddWhiteRitualTargetCard : target is missing!");
			return false;
		}
		return target.AddWhiteRitual(count);
	}

	public static void AddWhiteRitualSingle(int count, List<AIVirtualCard> targets)
	{
		if (targets == null || targets.Count <= 0)
		{
			AIConsoleUtility.LogError("AddWhiteRitualSingle : target is missing!");
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (!aIVirtualCard.IsIndependent && aIVirtualCard.WhiteRitualCount > 0 && AddWhiteRitualTargetCard(count, aIVirtualCard))
			{
				break;
			}
		}
	}
}
