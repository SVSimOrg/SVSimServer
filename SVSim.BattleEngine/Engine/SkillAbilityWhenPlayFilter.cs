using System.Collections.Generic;
using Wizard.Battle;

public class SkillAbilityWhenPlayFilter : ISkillCardFilter
{
	private readonly string _parameterOptionText;

	private readonly bool _isOnlyNoSelect;

	public SkillAbilityWhenPlayFilter(string op, bool isOnlyNoSelect)
	{
		_parameterOptionText = op;
		_isOnlyNoSelect = isOnlyNoSelect;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		bool hasWhenPlay = (IsOperaterEqual() ? true : false);
		foreach (IReadOnlyBattleCardInfo card in cards)
		{
			if (card.HasSkillWhenPlay(_isOnlyNoSelect) == hasWhenPlay)
			{
				yield return card;
			}
		}
	}

	public bool IsOperaterEqual()
	{
		return _parameterOptionText switch
		{
			"=" => true, 
			"!=" => false, 
			_ => true, 
		};
	}
}
