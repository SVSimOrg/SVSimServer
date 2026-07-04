using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_heal : Skill_heal
{
	public NetworkSkill_heal(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		VfxWithLoading vfxWithLoading = base.Start(callParameter);
		if (vfxWithLoading is NullVfxWithLoading)
		{
			return vfxWithLoading;
		}
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnHeal(base.SkillPrm.ownerCard, callParameter.targetCards.ToList(), HealResultList.Select((BattleCardBase.HealResult r) => r.HealAmount).ToList());
		return vfxWithLoading;
	}
}
