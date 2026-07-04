using System.Linq;

public class SkillEnvironmentalGamePlayCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.TurnPlayCardCountInfo.Where((TurnAndIntValue c) => c.IsSelfTurn == playerInfo.IsPlayer).Sum((TurnAndIntValue c) => c.Value);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
