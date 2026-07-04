using System.Collections.Generic;
using Wizard.Battle;

public class AttachingAbilityInfo
{
	public SkillBase Skill { get; private set; }

	public List<IReadOnlyBattleCardInfo> TargetCards { get; private set; }

	public AttachingAbilityInfo(SkillBase skill, List<IReadOnlyBattleCardInfo> targetCards)
	{
		Skill = skill;
		TargetCards = targetCards;
	}
}
