public class RandomValueFilter : ISkillEnvironmentalFilter
{
	private int _range;

	public RandomValueFilter(string rangeString)
	{
		_range = int.Parse(rangeString);
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return 0; // Pre-Phase-5b: RandomValueFilter is skill-condition-checker only headless
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
