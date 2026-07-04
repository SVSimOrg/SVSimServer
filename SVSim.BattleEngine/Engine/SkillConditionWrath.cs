using Wizard;

public class SkillConditionWrath : ISkillConditionChecker
{
	public bool IsSelfDamageCountGreaterThanSeven { get; private set; }

	public SkillConditionWrath(string flag)
	{
		IsSelfDamageCountGreaterThanSeven = flag == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int damageCount = playerInfoPair.ReadOnlySelf.SkillInfoClass.DamagedCounter.GetDamageCount(selfTurn: true);
		bool isForceWrath = playerInfoPair.ReadOnlySelf.SkillInfoClass.SkillApplyInformation.IsForceWrath;
		if (IsSelfDamageCountGreaterThanSeven)
		{
			if (!isForceWrath)
			{
				return IsWrath(damageCount);
			}
			return true;
		}
		if (!isForceWrath)
		{
			return !IsWrath(damageCount);
		}
		return false;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public static bool IsWrath(int selfDamageCount)
	{
		return selfDamageCount >= 7;
	}
}
