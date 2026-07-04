using Wizard;

public class SkillConditionDisplayOtherUsersMessage : ISkillConditionChecker
{
	private bool _judgeFlag;

	public SkillConditionDisplayOtherUsersMessage(string flag)
	{
		_judgeFlag = flag == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SHOW_OTHER_PLAYER_EMOTE) == _judgeFlag;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
