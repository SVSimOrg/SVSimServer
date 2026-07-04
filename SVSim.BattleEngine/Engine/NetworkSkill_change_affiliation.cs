using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_change_affiliation : Skill_change_affiliation
{
	public NetworkSkill_change_affiliation(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnChangeAffiliation(base.SkillPrm.ownerCard, parameter.targetCards.ToList(), _clan, _tribeInfo);
		return result;
	}

	public override void RegisterStop(BattleCardBase card)
	{
		RegisterAttach register = new RegisterAttach(new List<BattleCardBase> { card }, this, isDelete: true);
		(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase)._networkBattleSetupCardEventBase.AddRegisterActionManager(register);
	}

	public override void SkillCreateEnd()
	{
		base.SkillCreateEnd();
		if (NetworkBattleGenericTool.IsUnapprovedTarget(this) && base.SkillPrm.ownerCard.IsPlayer)
		{
			base.OnSkillEnd += NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent;
		}
	}
}
