using Wizard.Battle.View.Vfx;

public class Skill_can_play_self : SkillBase
{
	public Skill_can_play_self(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return VfxWithLoadingSequential.Create();
	}
}
