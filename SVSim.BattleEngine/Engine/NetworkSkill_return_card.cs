using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_return_card : Skill_return_card
{
	public NetworkSkill_return_card(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter callParameter)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnSkillReturn();
		return base.Start(callParameter);
	}

	protected override void RegisterReturnOtherTriggerSkill(SkillProcessor skillProcessor, List<BattleCardBase> targets)
	{
		base.RegisterReturnOtherTriggerSkill(skillProcessor, targets);
		if (base.SkillPrm.selfBattlePlayer.BattleMgr is NetworkStandardBattleMgr)
		{
			(base.SkillPrm.selfBattlePlayer.BattleMgr as NetworkStandardBattleMgr).RegisterReturnCardTrigger(base.SkillPrm.selfBattlePlayer, targets.Count);
		}
	}
}
