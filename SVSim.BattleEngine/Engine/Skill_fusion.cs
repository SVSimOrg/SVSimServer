using Wizard.Battle.View.Vfx;

public class Skill_fusion : SkillBase
{
	public Skill_fusion(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return VfxWithLoadingSequential.Create();
	}
}
