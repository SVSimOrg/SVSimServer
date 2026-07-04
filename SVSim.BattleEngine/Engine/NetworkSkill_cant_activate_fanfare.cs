using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_cant_activate_fanfare : Skill_cant_activate_fanfare
{
	public NetworkSkill_cant_activate_fanfare(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(parameter.targetCards.ToList());
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnGiveCantActivateFanfare(base.SkillPrm.ownerCard, parameter.targetCards.ToList());
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < buffInfoContainer.Count; i++)
		{
			list.Add(buffInfoContainer[i]._targetCard);
		}
		VfxWithLoading result = base.Stop(skillProcessor);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnDepriveCantActivateFanfare(base.SkillPrm.ownerCard, list);
		return result;
	}
}
