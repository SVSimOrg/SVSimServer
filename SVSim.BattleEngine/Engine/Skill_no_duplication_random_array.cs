using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_no_duplication_random_array : SkillBase
{
	protected Dictionary<BattleCardBase, List<int>> _selectedIndex = new Dictionary<BattleCardBase, List<int>>();

	public override bool IsAllowDestroyTarget => true;

	public Skill_no_duplication_random_array(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.random_range, -1);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.sum, -1);
		bool flag = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.show_battle_log) == "true";
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (!_selectedIndex.ContainsKey(targetCard))
			{
				_selectedIndex[targetCard] = new List<int>();
			}
			int[] array = ((targetCard.SkillApplyInformation.SkillRandomArray == null) ? new int[num] : targetCard.SkillApplyInformation.SkillRandomArray);
			for (int i = 0; i < num2; i++)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < num; j++)
				{
					if (array[j] == 0 && !_selectedIndex[targetCard].Contains(j))
					{
						list.Add(j);
					}
				}
				if (list.Count() == 0)
				{
					break;
				}
				int randomCount = GetRandomCount(list.Count);
				_selectedIndex[targetCard].Add(list[randomCount]);
				array[list[randomCount]]++;
			}
			sequentialVfxPlayer.Register(targetCard.SkillApplyInformation.GiveSkillRandomArray(array));
			SetOnLoseEvent(targetCard, null, null);
			if (IsBattleLog && flag)
			{
				BattleLogManager.GetInstance().AddLogSkillRandomArray(parameter.targetCards.ToList(), array, this);
			}
		}
		return VfxWithLoading.Create(sequentialVfxPlayer);
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		_selectedIndex.Clear();
		return base.Stop(skillProcessor);
	}

	private int GetRandomCount(int range)
	{
		return base.SkillPrm.selfBattlePlayer.BattleMgr.StableRandom(range);
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			_selectedIndex.Remove(card);
			return NullVfx.GetInstance();
		};
	}
}
