using System.Collections.Generic;
using Wizard;
using Wizard.Battle;

public class SkillParameterIsUnlimitedFilter : ISkillCardFilter
{
	private bool _isUnlimitedOk;

	public SkillParameterIsUnlimitedFilter(bool flag)
	{
		_isUnlimitedOk = Data.CurrentFormat == Format.Unlimited == flag;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		if (_isUnlimitedOk)
		{
			return cards;
		}
		return new List<IReadOnlyBattleCardInfo>();
	}
}
