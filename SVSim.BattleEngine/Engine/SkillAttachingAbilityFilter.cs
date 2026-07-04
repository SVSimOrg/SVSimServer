using System.Collections.Generic;
using Wizard.Battle;

public class SkillAttachingAbilityFilter : ISkillTargetFilter
{
	private SkillFilterCreator.ContentKeyword _keyword;

	public SkillAttachingAbilityFilter(SkillFilterCreator.ContentKeyword keyword)
	{
		_keyword = keyword;
	}

	public virtual IEnumerable<IReadOnlyBattleCardInfo> Filtering(IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos, SkillConditionCheckerOption option)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		switch (_keyword)
		{
		case SkillFilterCreator.ContentKeyword.guard:
			list = ((option.AttachingAbility.Skill is Skill_guard) ? option.AttachingAbility.TargetCards : list);
			break;
		case SkillFilterCreator.ContentKeyword.rush:
			list = ((option.AttachingAbility.Skill is Skill_rush) ? option.AttachingAbility.TargetCards : list);
			break;
		}
		return list;
	}
}
