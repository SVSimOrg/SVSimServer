public interface ISkillEnvironmentalFilter
{
	int Filtering(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option);

	int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option);
}
