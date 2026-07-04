using System;

public class SkillCardLimitUpperCountFilter : ISkillCardCountExtensionsFilter
{
	private int _limmitUpperCount;

	public SkillCardLimitUpperCountFilter(string limitUpperCount)
	{
		_limmitUpperCount = int.Parse(limitUpperCount);
	}

	public int Filtering(int count)
	{
		return Math.Min(count, _limmitUpperCount);
	}
}
