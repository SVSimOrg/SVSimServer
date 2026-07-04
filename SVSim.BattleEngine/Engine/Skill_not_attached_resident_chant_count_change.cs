using Wizard.Battle.View.Vfx;

public class Skill_not_attached_resident_chant_count_change : SkillBase
{
	public Skill_not_attached_resident_chant_count_change(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
