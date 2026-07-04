using Wizard;
using Wizard.Battle;

public class SkillConditionHalfLife : ISkillConditionChecker
{
	public bool IsConditionLesserHalfLife { get; private set; }

	public SkillConditionHalfLife(string flg)
	{
		IsConditionLesserHalfLife = flg == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		IReadOnlyBattleCardInfo skillInfoClass = playerInfoPair.ReadOnlySelf.SkillInfoClass;
		if (IsConditionLesserHalfLife)
		{
			if (!skillInfoClass.SkillApplyInformation.IsForceBerserk)
			{
				return IsHalfLife(skillInfoClass.Life);
			}
			return true;
		}
		if (!skillInfoClass.SkillApplyInformation.IsForceBerserk)
		{
			return !IsHalfLife(skillInfoClass.Life);
		}
		return false;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public static bool IsHalfLife(int life)
	{
		return life <= 10;
	}
}
