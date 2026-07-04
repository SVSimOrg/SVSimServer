using Wizard.Battle.View.Vfx;

public class Skill_choice : SkillBase
{
	public override bool IsChoiceType => true;

	public Skill_choice(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
