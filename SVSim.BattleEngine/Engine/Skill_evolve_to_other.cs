using Wizard.Battle.View.Vfx;

public class Skill_evolve_to_other : SkillBase
{
	public Skill_evolve_to_other(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
