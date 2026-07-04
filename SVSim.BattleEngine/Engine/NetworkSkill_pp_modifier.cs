using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_pp_modifier : Skill_pp_modifier
{
	public NetworkSkill_pp_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		if (_decreaseTurnPp != Skill_pp_modifier.PP_NONE)
		{
			base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		}
		return result;
	}

	protected override void AddPp(BattlePlayerBase player, int add)
	{
		player.CallOnAddPp(add, base.SkillPrm.ownerCard);
		base.AddPp(player, add);
	}
}
