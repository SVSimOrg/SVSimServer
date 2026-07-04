using System.Collections.Generic;
using System.Linq;

public class SkillRandomEachSameBaseCardIdFilter : ISkillSelectFilter
{
	protected int _restCount;

	protected int _count;

	protected BattlePlayerBase _player;

	public int CalcCount(SkillOptionValue option)
	{
		return _count;
	}

	public SkillRandomEachSameBaseCardIdFilter(int randomCount, BattlePlayerBase player)
	{
		_restCount = randomCount;
		_player = player;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		if (cards.Count() == 0)
		{
			return Enumerable.Empty<BattleCardBase>();
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		IEnumerable<int> ids = cards.Select((BattleCardBase c) => c.BaseParameter.BaseCardId).Distinct();
		int i;
		for (i = 0; i < ids.Count(); i++)
		{
			List<BattleCardBase> list2 = cards.Where((BattleCardBase c) => c.BaseParameter.BaseCardId == ids.ElementAt(i)).ToList();
			int num = list2.Count() - _restCount;
			for (int num2 = 0; num2 < num; num2++)
			{
				BattleCardBase item = list2[_player.BattleMgr.StableRandom(list2.Count)];
				list.Add(item);
				list2.Remove(item);
				_count++;
			}
		}
		return list;
	}
}
