using System.Collections.Generic;
using System.Linq;
using Wizard;

public class SkillRandomSelectUntilFilter : ISkillSelectFilter
{
	protected string _randomCountText;

	protected BattlePlayerBase _player;

	protected int _count;

	public int CalcCount(SkillOptionValue option)
	{
		return _count;
	}

	public SkillRandomSelectUntilFilter(string randomCountText, BattlePlayerBase player)
	{
		_randomCountText = randomCountText;
		_player = player;
	}

	public IEnumerable<BattleCardBase> Filtering(IEnumerable<BattleCardBase> targetCards, SkillOptionValue option, SkillConditionCheckerOption checkerOption)
	{
		_count = 1;
		if (targetCards.Count() == 0)
		{
			return Enumerable.Empty<BattleCardBase>();
		}
		BattleManagerBase ins = _player.BattleMgr;
		ApplySkillTargetFilterCollection applySkillTargetFilterCollection = new ApplySkillTargetFilterCollection();
		SkillFilterCreator.SetupTarget(applySkillTargetFilterCollection, _randomCountText, _player.Class, null);
		ApplySkillTargetFilterCollection applySkillTargetFilterCollection2 = applySkillTargetFilterCollection.ApplyAndFilter[0];
		BattlePlayerReadOnlyInfoPair battlePlayerInfoPair = ins.GetBattlePlayerInfoPair(_player.IsPlayer);
		List<BattleCardBase> list = targetCards.OrderBy((BattleCardBase x) => x.Index).ToList();
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		while (!IsHandMaxAfterDraw(list2))
		{
			int index = (ins.InstanceIsRandomDraw ? ins.StableRandom(list.Count) : 0);
			BattleCardBase battleCardBase = list[index];
			list.Remove(battleCardBase);
			list2.Add(battleCardBase);
			if (!applySkillTargetFilterCollection2.SimpleFiltering(battleCardBase, battlePlayerInfoPair, checkerOption, option) || IsHandMaxAfterDraw(list2))
			{
				break;
			}
			_count++;
			if (list.Count <= 0)
			{
				break;
			}
		}
		return list2;
	}

	private bool IsHandMaxAfterDraw(List<BattleCardBase> drawCards)
	{
		return _player.HandCardList.Count + drawCards.Count >= 9;
	}
}
