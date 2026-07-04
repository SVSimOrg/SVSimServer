using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_change_super_skybound_art_count : Skill_change_super_skybound_art_count
{
	public NetworkSkill_change_super_skybound_art_count(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		List<BattleCardBase> targetCards = parameter.targetCards.Where((BattleCardBase t) => t.HasSuperSkyboundArt && (t.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)).ToList();
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnChangeUnionBurstAndSkyboundArt(targetCards);
		return result;
	}
}
