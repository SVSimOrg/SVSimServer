using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterSelectDamageCount : ISkillParameterSelectFilter
{
	private readonly string _option = string.Empty;

	public SkillParameterSelectDamageCount(string option)
	{
		_option = option;
	}

	public IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => c.DamagedCounter.GetDamageCount(_option == SkillFilterCreator.ContentKeyword.self.ToStringCustom()));
	}
}
