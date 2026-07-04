using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillInOrderFromOldestFilter : ISkillCustomSelectFilter
{

	public List<IReadOnlyBattleCardInfo> OldTargets { get; private set; }

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		OldTargets = cards.ToList();
		if (option.NextTargetCards != null && option.NextTargetCards.Count > 0)
		{
			list.AddRange(option.NextTargetCards);
		}
		else
		{
			List<IReadOnlyBattleCardInfo> firstInOrderFromOldest = GetFirstInOrderFromOldest(OldTargets, battlePlayerInfos.Single((IBattlePlayerReadOnlyInfo p) => p.IsPlayer == OldTargets[0].IsPlayer));
			if (firstInOrderFromOldest != null)
			{
				list.AddRange(firstInOrderFromOldest);
			}
		}
		return list;
	}

	private List<IReadOnlyBattleCardInfo> GetFirstInOrderFromOldest(IEnumerable<IReadOnlyBattleCardInfo> list, IBattlePlayerReadOnlyInfo player)
	{
		List<IReadOnlyBattleCardInfo> list2 = null;
		List<IReadOnlyBattleCardInfo> list3 = list.Where((IReadOnlyBattleCardInfo s) => player.SkillInfoClassAndInPlayCards.Any((IReadOnlyBattleCardInfo p) => p == s)).ToList();
		if (list3.Count > 0)
		{
			list2 = new List<IReadOnlyBattleCardInfo>();
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = list3.SingleOrDefault((IReadOnlyBattleCardInfo s) => player.SkillInfoClass == s);
			list3.Remove(readOnlyBattleCardInfo);
			if (list3.Count > 0)
			{
				list2.Add(list3.First());
			}
			else if (readOnlyBattleCardInfo != null)
			{
				list2.Add(readOnlyBattleCardInfo);
			}
		}
		return list2;
	}
}
