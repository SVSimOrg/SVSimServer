using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterActivatedRandomArrayFilter : ISkillParameterSelectFilter
{
	private int _index;

	public SkillParameterActivatedRandomArrayFilter(int index)
	{
		_index = index;
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select(delegate(IReadOnlyBattleCardInfo c)
		{
			BattleCardBase battleCardBase = c as BattleCardBase;
			return (battleCardBase.Skills.FirstOrDefault((SkillBase s) => s is Skill_random_array) is Skill_random_array skill_random_array) ? (skill_random_array.GetAllSkillSelectedIndex(skill_random_array.SkillPrm.ownerCard, battleCardBase).Contains(_index) ? 1 : 0) : 0;
		});
	}
}
