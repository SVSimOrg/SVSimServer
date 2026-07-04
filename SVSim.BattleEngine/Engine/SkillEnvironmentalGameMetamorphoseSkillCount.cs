public class SkillEnvironmentalGameMetamorphoseSkillCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = 0;
		for (int i = 0; i < playerInfo.GameSkillMetamorphoseCountList.Count; i++)
		{
			num += playerInfo.GameSkillMetamorphoseCountList[i].Value;
		}
		return num;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
