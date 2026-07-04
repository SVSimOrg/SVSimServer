using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterLoadTargetCardId : ISkillParameterSelectFilter
{
	private long _id;

	public SkillParameterLoadTargetCardId(string option, SkillBase skill)
	{
		string[] array = option.Split(':');
		_id = long.Parse(array[0]);
		if (array.Count() > 1 && array[1] == SkillFilterCreator.ContentKeyword.is_individual.ToString())
		{
			_id += skill.IndividualId;
		}
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < cardInfos.Count(); i++)
		{
			list.AddRange(cardInfos.ElementAt(i).SkillApplyInformation.LoadTargetCardId(_id));
		}
		return list;
	}
}
