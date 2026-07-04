using Wizard.Battle.View.Vfx;

public class Skill_select : SkillBase
{
	public Skill_select(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
