public class SkillParameterSelectFixedGenericValueFilter : SkillParameterSelectGenericValueFilter
{
	public SkillParameterSelectFixedGenericValueFilter(string index, SkillBase skill)
		: base(index, skill)
	{
	}

	protected override int GetGenericValue(BattleCardBase card)
	{
		if (card.SkillApplyInformation.IsContainGenericValueKey(_key))
		{
			return card.SkillApplyInformation.SkillGenericKeyAndValue[_key];
		}
		if (!int.TryParse(_key, out var result))
		{
			return -1;
		}
		if (card.SkillApplyInformation.SkillGenericValueArray != null && result >= 0 && result < card.SkillApplyInformation.SkillGenericValueArray.Length)
		{
			return card.SkillApplyInformation.SkillGenericValueArray[result];
		}
		return -1;
	}
}
