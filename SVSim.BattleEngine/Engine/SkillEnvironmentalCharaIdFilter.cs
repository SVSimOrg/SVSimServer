public class SkillEnvironmentalCharaIdFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		if (!playerInfo.IsPlayer)
		{
			return dataMgr.GetEnemyCharaId();
		}
		return dataMgr.GetPlayerCharaId();
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
