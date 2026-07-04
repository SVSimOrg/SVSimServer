public class SkillTextVariableComareFilter : SkillVariableComareFilter
{
	public int LhsFilteringResult { get; set; }

	public int RhsFilteringResult { get; set; }

	public SkillTextVariableComareFilter(string text)
		: base(text)
	{
	}

	public int FilteringLhs(SkillOptionValue optionValue)
	{
		return LhsFilteringResult = optionValue.ParseInt(base.Lhs);
	}

	public int FilteringRhs(SkillOptionValue optionValue)
	{
		return RhsFilteringResult = optionValue.ParseInt(base.Rhs);
	}

	public override bool Filtering(SkillOptionValue optionValue)
	{
		return _compareFunc(LhsFilteringResult, RhsFilteringResult);
	}
}
