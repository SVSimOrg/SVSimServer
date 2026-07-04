using Cute;
using Wizard;

public class SkillConditionMeLanguage : ISkillConditionChecker
{
	private string _language;

	public SkillConditionMeLanguage(string language)
	{
		_language = language;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _language == CustomPreference.GetTextLanguage();
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
