using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterBaseCardIdFilter : SkillParameterIdFilter
{
	public SkillParameterBaseCardIdFilter(string parameterText, string op)
		: base(parameterText, op)
	{
	}

	public override IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (_isIntersection)
		{
			foreach (IReadOnlyBattleCardInfo card in cards)
			{
				if (!_parameterTexts.Any((string p) => !_compareFunc(card.BaseParameter.BaseCardId, option.ParseInt(p), card, cards)))
				{
					list.Add(card);
				}
			}
		}
		else
		{
			for (int num = 0; num < _parameterTexts.Length; num++)
			{
				int value = option.ParseInt(_parameterTexts[num]);
				list.AddRange(cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.BaseParameter.BaseCardId, value, c, cards)));
			}
		}
		return list;
	}
}
