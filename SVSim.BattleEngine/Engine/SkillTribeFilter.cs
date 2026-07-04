using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillTribeFilter : ISkillCardFilter
{
	public readonly CardBasePrm.TribeType _type;

	public readonly string OptionText;

	public readonly bool IsEqual;

	public SkillTribeFilter(CardBasePrm.TribeType tribe, string op)
	{
		_type = tribe;
		OptionText = op;
		IsEqual = op == "=";
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards.Where((IReadOnlyBattleCardInfo c) => c.IsTribe(_type) == IsEqual);
	}
}
