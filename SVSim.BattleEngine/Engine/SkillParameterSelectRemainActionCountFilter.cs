using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectRemainActionCountFilter : ISkillParameterSelectFilter
{
	private string _skillId;

	public SkillParameterSelectRemainActionCountFilter(string option, SkillBase skill)
	{
		string[] array = option.Split(':');
		long num = long.Parse(array[0]);
		if (array.Count() > 1 && array[1] == SkillFilterCreator.ContentKeyword.is_individual.ToString() && skill != null)
		{
			num += skill.IndividualId;
		}
		_skillId = num.ToString();
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < cardInfos.Count(); i++)
		{
			IEnumerable<SkillPreprocessBase> source = (cardInfos.ElementAt(i) as BattleCardBase).Skills.SelectMany((SkillBase s) => s.PreprocessList.Where((SkillPreprocessBase p) => p is SkillPreprocessRemoveAfterAction));
			for (int num = 0; num < source.Count(); num++)
			{
				SkillPreprocessRemoveAfterAction skillPreprocessRemoveAfterAction = source.ElementAt(num) as SkillPreprocessRemoveAfterAction;
				if (skillPreprocessRemoveAfterAction.BanId == _skillId)
				{
					list.Add(skillPreprocessRemoveAfterAction.Count);
					break;
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(0);
		}
		return list;
	}
}
