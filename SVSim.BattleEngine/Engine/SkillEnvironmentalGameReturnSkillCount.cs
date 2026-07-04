public class SkillEnvironmentalGameReturnSkillCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = 0;
		for (int i = 0; i < playerInfo.GameSkillReturnCardCountList.Count; i++)
		{
			num += playerInfo.GameSkillReturnCardCountList[i].Value;
		}
		return num;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
