public class SkillEnvironmentalGameDiscardSkillCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = 0;
		for (int i = 0; i < playerInfo.GameSkillDiscardCountList.Count; i++)
		{
			num += playerInfo.GameSkillDiscardCountList[i].Value;
		}
		return num;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
