using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTargetEqualOrLessCostFromLastTarget : ISkillCustomSelectFilter
{
	private string _strLastTargetIndex = "0";

	public List<IReadOnlyBattleCardInfo> KeyCards { get; private set; }

	public SkillTargetEqualOrLessCostFromLastTarget(string customValue)
	{
		_strLastTargetIndex = customValue;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		KeyCards = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		List<IReadOnlyBattleCardInfo> selectKeyCards = new SkillTargetLastTargetFilter(_strLastTargetIndex).Filtering(battlePlayerInfos, option).ToList();
		List<IReadOnlyBattleCardInfo> list2 = cards.OrderBy((IReadOnlyBattleCardInfo x) => x.Index).ToList();
		BattleManagerBase ins = list2.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr
			?? selectKeyCards.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		int i;
		for (i = 0; i < selectKeyCards.Count; i++)
		{
			IEnumerable<IReadOnlyBattleCardInfo> source = list2.Where((IReadOnlyBattleCardInfo c) => c.Cost <= selectKeyCards[i].Cost);
			if (source.Count() > 0)
			{
				KeyCards.Add(selectKeyCards[i]);
				int index = (ins.InstanceIsRandomDraw ? ins.StableRandom(source.Count()) : 0);
				IReadOnlyBattleCardInfo item = source.ElementAtOrDefault(index);
				list.Add(item);
				list2.Remove(item);
			}
		}
		return list;
	}
}
