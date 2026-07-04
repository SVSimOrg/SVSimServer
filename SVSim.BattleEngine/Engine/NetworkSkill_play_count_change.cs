using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_play_count_change : Skill_play_count_change
{
	private NetworkBattleManagerBase _networkBattleMgr;

	public NetworkSkill_play_count_change(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		_networkBattleMgr = battleManager;
		base.OnSkillEnd += RegisterPlayCountChange;
	}

	private VfxBase RegisterPlayCountChange(SkillBase skill, List<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		_networkBattleMgr.RegisterActionManager.Add(new RegisterPlayCountChange(base.SkillPrm.ownerCard.IsPlayer, GetAddCount()));
		return NullVfx.GetInstance();
	}
}
