using Wizard.Battle.View.Vfx;

public class Skill_invoke_voice : SkillBase
{
	public Skill_invoke_voice(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invoke_voice, "_OPT_NULL_");
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToLoadingVfx(NullVfx.GetInstance());
		vfxWithLoadingSequential.RegisterToMainVfx(NullVfx.GetInstance());
		return vfxWithLoadingSequential;
	}
}
