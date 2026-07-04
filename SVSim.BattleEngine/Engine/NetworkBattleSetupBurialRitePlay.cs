public class NetworkBattleSetupBurialRitePlay
{
	public enum CheckResult
	{
		NotBurialRite,
		NotPlay,
		Play
	}

	private NetworkBattleSetupCardEvent _networkBattleSetupCardEvent;

	public NetworkBattleSetupBurialRitePlay(NetworkBattleSetupCardEvent data)
	{
		_networkBattleSetupCardEvent = data;
	}

	public CheckResult JudgeReceiveBurialRiteSkillPlayOrNotPlay(SkillBase skill)
	{
		if (!(skill.ConditionFilterCollection.ConditionCheckerFilterList.Find((ISkillConditionChecker x) => x is SkillConditionBurialRite) is SkillConditionBurialRite skillConditionBurialRite))
		{
			return CheckResult.NotBurialRite;
		}
		if (skill.SkillPrm.ownerCard.IsHaveBurialRiteJudgeBothFlag)
		{
			bool flag = false;
			NetworkBattleData networkBattleData = _networkBattleSetupCardEvent.networkBattleData;
			if (networkBattleData.GetReceiveData() != null)
			{
				foreach (int validateSkillIndex in networkBattleData.GetReceiveData().validateSkillIndexList)
				{
					SkillBase skillBase = NetworkBattleGenericTool.SerchIndexToSkill(skill.SkillPrm.ownerCard.Skills, validateSkillIndex);
					if (skillBase != null && NetworkBattleGenericTool.IsBurialRite(skillBase))
					{
						flag = true;
						break;
					}
				}
			}
			if ((skillConditionBurialRite.judgeFlg && flag) || (!skillConditionBurialRite.judgeFlg && !flag))
			{
				return CheckResult.Play;
			}
			if ((skillConditionBurialRite.judgeFlg && !flag) || (!skillConditionBurialRite.judgeFlg && flag))
			{
				return CheckResult.NotPlay;
			}
		}
		return CheckResult.NotBurialRite;
	}
}
