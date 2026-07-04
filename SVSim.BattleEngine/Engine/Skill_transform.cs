using Wizard.Battle.View.Vfx;

public class Skill_transform : SkillBaseSummon
{

	public int TransformId { get; private set; }

	public Skill_transform(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		TransformId = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.card_id, -1);
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		return VfxWithLoadingSequential.Create();
	}
}
