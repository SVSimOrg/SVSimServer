using Wizard.Battle.View.Vfx;

public class NetworkSkill_destroy : Skill_destroy
{
	public NetworkSkill_destroy(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnSkillDestroyOrBanish(base.SkillPrm.ownerCard);
		return base.Start(callParameter);
	}
}
