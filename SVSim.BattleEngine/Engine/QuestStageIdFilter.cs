public class QuestStageIdFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		if (dataMgr.m_BattleType != DataMgr.BattleType.Quest)
		{
			return -1;
		}
		return dataMgr.QuestBattleData.QuestStageId;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		if (dataMgr.m_BattleType != DataMgr.BattleType.Quest)
		{
			return -1;
		}
		return dataMgr.QuestBattleData.QuestStageId;
	}
}
