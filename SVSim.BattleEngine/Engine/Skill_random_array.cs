using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_random_array : SkillBase
{
	public override bool IsAllowDestroyTarget => true;

	public override bool IsShowSideLogSkillType => false;

	public Dictionary<BattleCardBase, List<int>> SelectedIndex { get; protected set; } = new Dictionary<BattleCardBase, List<int>>();

	public string RandomDistinctSkill { get; protected set; }

	public Skill_random_array(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		string[] array = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_random_distinct, "NONE").Split(':');
		RandomDistinctSkill = ((array.Length > 1) ? array[1] : "NONE");
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.random_range, -1);
		int num2 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.max, -1);
		int num3 = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.sum, -1);
		bool flag = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.show_battle_log) == "true";
		bool flag2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_random_distinct, "NONE").Split(':')[0] == "true";
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (!SelectedIndex.ContainsKey(targetCard))
			{
				SelectedIndex[targetCard] = new List<int>();
			}
			int[] array = new int[num];
			for (int i = 0; i < num3; i++)
			{
				if (num2 != -1)
				{
					List<int> list = new List<int>();
					for (int j = 0; j < num; j++)
					{
						List<int> allSkillSelectedIndex = GetAllSkillSelectedIndex(base.SkillPrm.ownerCard, targetCard);
						if (array[j] < num2 && (!flag2 || !allSkillSelectedIndex.Contains(j)))
						{
							list.Add(j);
						}
					}
					if (list.Count() == 0)
					{
						break;
					}
					int randomCount = GetRandomCount(list.Count);
					SelectedIndex[targetCard].Add(list[randomCount]);
					array[list[randomCount]]++;
				}
				else
				{
					int randomCount2 = GetRandomCount(num);
					array[randomCount2]++;
				}
			}
			sequentialVfxPlayer.Register(targetCard.SkillApplyInformation.GiveSkillRandomArray(array));
			BattleCardBase target = targetCard;
			SetOnLoseEvent(target, null, null);
			if (IsBattleLog && flag)
			{
				BattleLogManager.GetInstance().AddLogSkillRandomArray(parameter.targetCards.ToList(), array, this);
			}
		}
		return VfxWithLoading.Create(sequentialVfxPlayer);
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		SelectedIndex.Clear();
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
			SelectedIndex.Remove(card);
			return NullVfx.GetInstance();
		};
	}

	public List<int> GetAllSkillSelectedIndex(BattleCardBase card, BattleCardBase targetCard)
	{
		List<int> list = new List<int>();
		if (RandomDistinctSkill == "NONE")
		{
			if (SelectedIndex.ContainsKey(targetCard))
			{
				list.AddRange(SelectedIndex[targetCard]);
			}
		}
		else
		{
			list.AddRange(GetSkillSelectedIndex(card.NormalSkills, targetCard));
			list.AddRange(GetSkillSelectedIndex(card.EvolutionSkills, targetCard));
		}
		return list;
	}

	private List<int> GetSkillSelectedIndex(SkillCollectionBase skills, BattleCardBase targetCard)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < skills.Count(); i++)
		{
			if (skills.ElementAt(i) is Skill_random_array skill_random_array && skill_random_array.RandomDistinctSkill == RandomDistinctSkill && skill_random_array.SelectedIndex.ContainsKey(targetCard))
			{
				list.AddRange(skill_random_array.SelectedIndex[targetCard]);
			}
		}
		return list;
	}
}
