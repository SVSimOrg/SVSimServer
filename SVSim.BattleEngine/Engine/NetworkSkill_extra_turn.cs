using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_extra_turn : Skill_extra_turn
{
	public NetworkSkill_extra_turn(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		base.OnSkillStart += delegate
		{
			(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase).RegisterActionManager.Add(new RegisterExtraTurn(GetAddTurn(), base.SkillPrm.ownerCard.IsPlayer));
		};
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowSkillEffect(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}
}
