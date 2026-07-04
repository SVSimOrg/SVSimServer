using Wizard.Battle.View.Vfx;

public class NetworkSkill_banish : Skill_banish
{
	public NetworkSkill_banish(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		bool isOpen = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.is_open, SkillFilterCreator.ContentKeyword._false.ToStringCustom()) == SkillFilterCreator.ContentKeyword._true.ToStringCustom();
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnSkillDestroyOrBanish(base.SkillPrm.ownerCard, isBurialRite: false, isOpen);
		return base.Start(callParameter);
	}
}
