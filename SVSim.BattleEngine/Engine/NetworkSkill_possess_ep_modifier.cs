using Wizard.Battle.View.Vfx;

public class NetworkSkill_possess_ep_modifier : Skill_possess_ep_modifier
{
	public NetworkSkill_possess_ep_modifier(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected override VfxBase AddEp(SkillProcessor skillProcessor, int add, BattlePlayerBase battlePlayer)
	{
		battlePlayer.CallOnEpModifier(base.SkillPrm.ownerCard, add, isAdd: true);
		if (base.SkillPrm.selfBattlePlayer.BattleMgr is NetworkStandardBattleMgr networkStandardBattleMgr && add < 0 && battlePlayer.IsSelfTurn)
		{
			networkStandardBattleMgr.RegisterUseEpTrigger(base.SkillPrm.selfBattlePlayer);
		}
		return base.AddEp(skillProcessor, add, battlePlayer);
	}

	protected override VfxBase SetEp(int set, BattlePlayerBase battlePlayer)
	{
		if (set >= 0)
		{
			battlePlayer.CallOnEpModifier(base.SkillPrm.ownerCard, set, isAdd: false);
		}
		return base.SetEp(set, battlePlayer);
	}
}
