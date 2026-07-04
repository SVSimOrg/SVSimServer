using System;
using System.Collections.Generic;
using System.Linq;
using Wizard;

public class SkillRandomSelectFilter : ISkillSelectFilter
{
	private readonly string _context;

	private int _count;

	public SkillRandomSelectFilter(string randomCountText)
	{
		_context = randomCountText;
	}

	public int CalcCount(SkillOptionValue option)
	{
		return option.ParseInt(_context);
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> cards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		_count = CalcCount(option);
		cards = cards.OrderBy((BattleCardBase x) => x.Index);
		List<BattleCardBase> attractSkillCardList = cards.Where((BattleCardBase c) => c.SkillApplyInformation.IsAttractSkillTarget).ToList();
		List<BattleCardBase> nonAttractSkillCardList = cards.Where((BattleCardBase c) => !c.SkillApplyInformation.IsAttractSkillTarget).ToList();
		// Route through the first card's mgr — all cards in a filtering pass share the same mgr,
		// and if cards is empty the loop below runs zero times so the null branch is unreachable.
		BattleManagerBase battleMgr = attractSkillCardList.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr
			?? nonAttractSkillCardList.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		_count = Math.Min(_count, attractSkillCardList.Count + nonAttractSkillCardList.Count);
		for (int i = 0; i < _count; i++)
		{
			List<BattleCardBase> list = ((attractSkillCardList.Count > 0) ? attractSkillCardList : nonAttractSkillCardList);
			int index = (battleMgr.InstanceIsRandomDraw ? battleMgr.StableRandom(list.Count) : 0);
			BattleCardBase battleCardBase = list[index];
			list.Remove(battleCardBase);
			yield return battleCardBase;
		}
	}

	public static IEnumerable<BattleCardBase> Filtering(int selectCount, IEnumerable<BattleCardBase> cards, BattleManagerBase battleMgr)
	{
		cards = cards.OrderBy((BattleCardBase x) => x.Index);
		List<BattleCardBase> list = cards.ToList();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		int num = Math.Min(selectCount, list.Count);
		LocalLog.AccumulateLastTraceLog("drawDeck " + list.Count);
		for (int num2 = 0; num2 < num; num2++)
		{
			int index = battleMgr.StableRandom(list.Count);
			BattleCardBase item = list[index];
			list.Remove(item);
			list2.Add(item);
		}
		return list2;
	}

	public static IEnumerable<T> Filtering<T>(int selectCount, IEnumerable<T> targets, BattleManagerBase battleMgr, bool isDistinct = false)
	{
		List<T> list = targets.ToList();
		List<T> list2 = new List<T>();
		int num = selectCount;
		if (isDistinct)
		{
			num = Math.Min(selectCount, list.Count);
		}
		for (int i = 0; i < num; i++)
		{
			if (list.Count > 0)
			{
				int index = battleMgr.StableRandom(list.Count);
				T item = list[index];
				list2.Add(item);
				if (isDistinct)
				{
					list.Remove(item);
				}
			}
		}
		return list2;
	}
}
