using Wizard.Battle.View.Vfx;

public class Skill_none : SkillBase
{
	public override bool IsShowSideLogSkillType => false;

	public Skill_none(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return NullVfxWithLoading.GetInstance();
	}
}
