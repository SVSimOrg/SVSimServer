using Wizard.Battle.View.Vfx;

public class Skill_life_zero_activate_leon_skill : SkillBase
{
	public Skill_life_zero_activate_leon_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			targetCard.SkillApplyInformation.GiveLifeZeroActivateLeonSkill();
		}
		return NullVfxWithLoading.GetInstance();
	}
}
