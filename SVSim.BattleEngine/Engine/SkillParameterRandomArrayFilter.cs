using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterRandomArrayFilter : ISkillParameterSelectFilter
{
	protected readonly int index;

	public SkillParameterRandomArrayFilter(string option)
	{
		int.TryParse(option, out index);
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => (c.SkillApplyInformation.SkillRandomArray == null || c.SkillApplyInformation.SkillRandomArray.Length <= index) ? (-1) : c.SkillApplyInformation.SkillRandomArray[index]);
	}
}
