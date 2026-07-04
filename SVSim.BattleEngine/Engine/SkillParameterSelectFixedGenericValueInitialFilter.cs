public class SkillParameterSelectFixedGenericValueInitialFilter : SkillParameterSelectFixedGenericValueFilter
{
	private readonly int _initalValue;

	public SkillParameterSelectFixedGenericValueInitialFilter(string index, string option, SkillBase skill)
		: base(index, skill)
	{
		int.TryParse(option, out _initalValue);
	}

	protected override int GetGenericValue(BattleCardBase card)
	{
		int num = base.GetGenericValue(card);
		if (num == -1)
		{
			num = _initalValue;
		}
		return num;
	}
}
