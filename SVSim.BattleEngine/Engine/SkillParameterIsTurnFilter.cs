using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterIsTurnFilter : ISkillParameterSelectFilter
{
	private readonly bool _isSelf;

	public SkillParameterIsTurnFilter(string target)
	{
		_isSelf = target == SkillFilterCreator.ContentKeyword.self.ToString();
	}

	public virtual IEnumerable<int> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		return cardInfos.Select((IReadOnlyBattleCardInfo c) => (c.IsSelfTurn == _isSelf) ? 1 : 0);
	}
}
