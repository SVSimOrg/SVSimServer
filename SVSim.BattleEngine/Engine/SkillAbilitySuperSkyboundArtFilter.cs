using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillAbilitySuperSkyboundArtFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	public SkillAbilitySuperSkyboundArtFilter(string op)
	{
		_parameterOptionText = op;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasSuperSkyboundArt;
		if (_parameterOptionText == "=")
		{
			hasSuperSkyboundArt = true;
		}
		else
		{
			if (!(_parameterOptionText == "!="))
			{
				yield break;
			}
			hasSuperSkyboundArt = false;
		}
		for (int i = 0; i < cards.Count(); i++)
		{
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = cards.ElementAt(i);
			if (readOnlyBattleCardInfo.HasSuperSkyboundArt == hasSuperSkyboundArt)
			{
				yield return readOnlyBattleCardInfo;
			}
		}
	}
}
