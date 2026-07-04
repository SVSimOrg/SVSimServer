using System.Collections.Generic;

public struct SkillTimingInfo
{
	public bool IsWhenPlay { get; private set; }

	public bool IsWhenDestroy { get; private set; }

	public bool IsBeforeAttack { get; private set; }

	public bool IsBeforeAttackSelfAndOther { get; private set; }

	public bool IsAfterAttackSelfAndOther { get; private set; }

	public bool IsWhenFight { get; private set; }

	public SkillTimingInfo(List<SkillBase> skillList)
	{
		IsWhenPlay = false;
		IsWhenDestroy = false;
		IsBeforeAttack = false;
		IsWhenFight = false;
		IsBeforeAttackSelfAndOther = false;
		IsAfterAttackSelfAndOther = false;
		for (int i = 0; i < skillList.Count; i++)
		{
			SkillBase skillBase = skillList[i];
			if (skillBase.IsWhenPlaySkill)
			{
				IsWhenPlay = true;
			}
			if (skillBase.IsWhenDestroySkill)
			{
				IsWhenDestroy = true;
			}
			if (skillBase.IsBeforAttackSkill)
			{
				IsBeforeAttack = true;
			}
			if (skillBase.IsWhenFightSkill)
			{
				IsWhenFight = true;
			}
			if (skillBase.IsBeforeAttackSelfAndOtherSkill)
			{
				IsBeforeAttackSelfAndOther = true;
			}
			if (skillBase.IsAfterAttackSelfAndOtherSkill)
			{
				IsAfterAttackSelfAndOther = true;
			}
		}
	}
}
