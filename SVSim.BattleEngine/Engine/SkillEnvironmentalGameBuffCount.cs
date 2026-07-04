public class SkillEnvironmentalGameBuffCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		int num = 0;
		for (int i = 0; i < playerInfo.GameSkillBuffCountList.Count; i++)
		{
			num += playerInfo.GameSkillBuffCountList[i].Value;
		}
		return num;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
