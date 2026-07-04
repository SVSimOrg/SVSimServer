using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilityUnionBurstFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilityUnionBurstFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasUnionBurst;
		if (_parameterOptionText == "=")
		{
			hasUnionBurst = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasUnionBurst = false;
		}
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.HasUnionBurst == hasUnionBurst)
			{
				yield return readOnlyBattleCardInfo;
			}
		}
	}
}
