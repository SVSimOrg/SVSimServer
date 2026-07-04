using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillParameterIdFilter : ISkillCardFilter
{
	protected bool _isIntersection;

	protected readonly string[] _parameterTexts;

	protected readonly Func<int, int, IReadOnlyBattleCardInfo, IEnumerable<IReadOnlyBattleCardInfo>, bool> _compareFunc;

	private string _optionText = "";

	public SkillParameterIdFilter(string parameterText, string op)
	{
		if (parameterText.Contains("|"))
		{
			_parameterTexts = parameterText.Split('|');
			_isIntersection = true;
		}
		else
		{
			_parameterTexts = parameterText.Split(':');
		}
		_compareFunc = SkillCompareFuncCreator.Create(op, SkillCompareFuncCreator.CardId);
		_optionText = op;
	}

	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (_isIntersection)
		{
			foreach (IReadOnlyBattleCardInfo card in cards)
			{
				if (!_parameterTexts.Any((string p) => !_compareFunc(card.BaseParameter.NormalCardId, option.ParseInt(p), card, cards)))
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
				list.AddRange(cards.Where((IReadOnlyBattleCardInfo c) => _compareFunc(c.BaseParameter.NormalCardId, value, c, cards)));
			}
		}
		return list;
	}

	public string[] GetFilterId()
	{
		return _parameterTexts;
	}

	public string GetOptionText()
	{
		return _optionText;
	}
}
