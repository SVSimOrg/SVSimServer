using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterDistinctRandomSelectedFilter : ISkillParameterSelectFilter
{
	private int _id;

	private int _index;

	public SkillParameterDistinctRandomSelectedFilter(string str)
	{
		string[] array = str.Split(':');
		_id = int.Parse(array[0]);
		_index = int.Parse(array[1]);
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select(delegate(IReadOnlyBattleCardInfo c)
		{
			BattleCardBase battleCardBase = c as BattleCardBase;
			for (int i = 0; i < battleCardBase.SkillApplyInformation.AttachedSkillsInfo.AttachedSkills.Count(); i++)
			{
				if (battleCardBase.SkillApplyInformation.AttachedSkillsInfo.AttachedSkills.ElementAt(i) is Skill_random_array { GetAttachSkill: not null } skill_random_array && skill_random_array.GetAttachSkill.SkillPrm.ownerCard.BaseParameter.BaseCardId == _id && skill_random_array.SelectedIndex.ContainsKey(battleCardBase))
				{
					if (!skill_random_array.SelectedIndex[battleCardBase].Contains(_index))
					{
						return 0;
					}
					return 1;
				}
			}
			return 0;
		});
	}
}
