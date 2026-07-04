using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetOverCostFromLastTargetFilter : ISkillCustomSelectFilter
{
	public List<IReadOnlyBattleCardInfo> KeyDestroyedCard { get; private set; }

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		KeyDestroyedCard = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> selectKeyCards = battlePlayerInfos.SelectMany((IBattlePlayerReadOnlyInfo s) => s.SkillInfoLastTargets.First()).ToList();
		List<IReadOnlyBattleCardInfo> list2 = cards.OrderBy((IReadOnlyBattleCardInfo x) => x.Index).ToList();
		BattleManagerBase ins = list2.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr
			?? selectKeyCards.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		int i = 0;
		int count = selectKeyCards.Count;
		while (i < count)
		{
			IEnumerable<IReadOnlyBattleCardInfo> source = list2.Where((IReadOnlyBattleCardInfo c) => c.Cost > selectKeyCards[i].Cost);
			if (source.Count() > 0)
			{
				KeyDestroyedCard.Add(selectKeyCards[i]);
				int index = (ins.InstanceIsRandomDraw ? ins.StableRandom(source.Count()) : 0);
				IReadOnlyBattleCardInfo item = source.ElementAtOrDefault(index);
				list.Add(item);
				list2.Remove(item);
			}
			int num = i + 1;
			i = num;
		}
		return list;
	}
}
