using Wizard;

public class SkillConditionAttachingAbility : ISkillConditionChecker
{
	private SkillFilterCreator.ContentKeyword _keyword;

	public SkillConditionAttachingAbility(SkillFilterCreator.ContentKeyword keyword)
	{
		_keyword = keyword;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (option.AttachingAbility.Skill == null)
		{
			return false;
		}
		if (option.AttachingAbility.Skill.OnWhenChangeInPlay != 0 && option.AttachingAbility.Skill.IsAttachedSkill && !option.AttachingAbility.Skill.IsAttachedInplaySkill)
		{
			return false;
		}
		return _keyword switch
		{
			SkillFilterCreator.ContentKeyword.guard => option.AttachingAbility.Skill is Skill_guard, 
			SkillFilterCreator.ContentKeyword.rush => option.AttachingAbility.Skill is Skill_rush, 
			SkillFilterCreator.ContentKeyword.quick => option.AttachingAbility.Skill is Skill_quick, 
			_ => false, 
		};
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
