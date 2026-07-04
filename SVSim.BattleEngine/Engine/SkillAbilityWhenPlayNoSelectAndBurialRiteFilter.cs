using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityWhenPlayNoSelectAndBurialRiteFilter : ISkillCardFilter
{
	private readonly bool _isEqual;

	public SkillAbilityWhenPlayNoSelectAndBurialRiteFilter(string optionText)
	{
		_isEqual = optionText == "=";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < cards.Count(); i++)
		{
			BattleCardBase battleCardBase = cards.ElementAt(i) as BattleCardBase;
			IEnumerable<SkillBase> source = battleCardBase.Skills.Where((SkillBase s) => s.IsWhenPlaySkill);
			if ((source.Any((SkillBase s) => s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite)) && !source.Any((SkillBase s) => s.IsUserSelectType)) == _isEqual)
			{
				list.Add(battleCardBase);
			}
		}
		return list;
	}
}
