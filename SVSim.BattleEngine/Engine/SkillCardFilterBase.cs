using System.Collections.Generic;
using Wizard.Battle;

public class SkillCardFilterBase : ISkillCardFilter
{
	protected readonly string _parameterText;

	protected readonly string _parameterOptionText;

	public SkillCardFilterBase(string parameterText, string op)
	{
		_parameterOptionText = op;
		_parameterText = parameterText;
	}

	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IReadOnlyBattleCardInfo> cards, SkillOptionValue option)
	{
		return cards;
	}

	public virtual string GetParameterText()
	{
		return _parameterText;
	}

	public virtual string GetParameterOptionText()
	{
		return _parameterOptionText;
	}
}
