using Wizard.Battle.View.Vfx;

public class NetworkSkill_bp_modifier : Skill_bp_modifier
{
	public NetworkSkill_bp_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override VfxBase AddBp(BattlePlayerBase target, int value)
	{
		target.CallOnAddBp(value, base.SkillPrm.ownerCard);
		return base.AddBp(target, value);
	}
}
