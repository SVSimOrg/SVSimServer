using System.Linq;

public class SkillEnvironmentalGameSuperSkyboundArtCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.SkillInfoGameSuperSkyboundArtCards.Count();
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
