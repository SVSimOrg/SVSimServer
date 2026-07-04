using System.Collections.Generic;
using Wizard;
using Wizard.Battle.Card;

public class SpecialSkillBattleCard : BattleCardBase
{
	public BossRushSpecialSkill Skill;

	public override bool IsSpecialSkill => true;

	public SpecialSkillBattleCard(BossRushSpecialSkill skill, BuildInfo buildInfo)
		: base(buildInfo)
	{
		Skill = skill;
	}

	public override string SkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo = null, bool isSkipOption = false, BuffInfo buff = null, string divergenceId = "", List<int> skillDescriptionValueList = null, List<int> sideLogDescriptionValueList = null)
	{
		return ConvertSkillDescription(Skill.SkillDescText, sideLogInfo, isSkipOption, buff, divergenceId, skillDescriptionValueList, (base.IsBuffDetail && base.ReplayBuffDetailSkillDescriptionValueList.Count > 0) ? base.ReplayBuffDetailSkillDescriptionValueList : base.ReplaySkillDescriptionValueList);
	}

	public override string EvoSkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo = null, bool isSkipOption = false, BuffInfo buff = null, string divergenceId = "", List<int> skillDescriptionValueList = null, List<int> sideLogDescriptionValueList = null)
	{
		return string.Empty;
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		VirtualSpecialSkillBattleCard virtualSpecialSkillBattleCard = new VirtualSpecialSkillBattleCard(Skill, _buildInfo.VirtualClone(selfBattlePlayer, opponentBattlePlayer));
		CopyToVirtualCardBase(virtualSpecialSkillBattleCard);
		return virtualSpecialSkillBattleCard;
	}
}
