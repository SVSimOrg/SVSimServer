using Wizard.Battle.View.Vfx;

public class NetworkSkill_evolve : Skill_evolve
{
	public NetworkSkill_evolve(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		VfxWithLoading vfxWithLoading = base.Start(callParameter);
		if (!(vfxWithLoading is NullVfxWithLoading))
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnAfterSkillEvolve(_evolvedBySkill);
		}
		return vfxWithLoading;
	}
}
