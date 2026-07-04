public abstract class SkillPreprocessPeriodBase : SkillPreprocessBase
{
	protected int _effectiveRange;

	protected int _defPeriodCount;

	public int PeriodCount { get; protected set; }

	protected SkillPreprocessPeriodBase(BattleCardBase ownerCard, string period)
	{
		string[] array = period.Split(':');
		_defPeriodCount = int.Parse(array[0]);
		_effectiveRange = int.Parse(array[1]);
		PeriodCount = _defPeriodCount;
	}

	public void ReducePeriodCount()
	{
		if (PeriodCount >= 0)
		{
			PeriodCount--;
		}
	}

	public void ResetPeriodCount()
	{
		PeriodCount = _defPeriodCount;
	}
}
