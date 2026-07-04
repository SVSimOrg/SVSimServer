using Wizard;

public class SkillConditionShortageDeckWin : ISkillConditionChecker
{
	public bool IsConditionShortageDeckWin { get; private set; }

	public SkillConditionShortageDeckWin(string flg)
	{
		IsConditionShortageDeckWin = flg == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return playerInfoPair.ReadOnlySelf.SkillInfoClass.SkillApplyInformation.IsShortageDeckWin == IsConditionShortageDeckWin;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
