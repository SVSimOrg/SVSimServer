using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

internal class NetworkSkill_invoke_skill : Skill_invoke_skill
{
	public NetworkSkill_invoke_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsUseUnapprovedList(base.SkillPrm.ownerCard.IsPlayer))
		{
			string invokeType = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.invoke_type);
			NetworkBattleManagerBase obj = base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
			List<CardDataModel> skillConditionCheckList = obj.networkBattleData.GetReceiveData().SkillConditionCheckList;
			List<CardDataModel> unapprovedList = obj.networkBattleData.GetReceiveData().unapprovedList;
			foreach (BattleCardBase targetCard in parameter.targetCards)
			{
				List<SkillBase> list = ((!(invokeType == SkillTiming.when_play.ToString())) ? targetCard.Skills.Where((SkillBase s) => IsInvokableSkill(s, invokeType)).ToList() : targetCard.NormalSkills.Where((SkillBase s) => IsInvokableSkill(s, invokeType)).ToList());
				foreach (SkillBase skill in list)
				{
					if (skillConditionCheckList.Any((CardDataModel c) => skill.PublishedActiveSkillCount == c.publishedActiveSkillCount && c.IsInvoked))
					{
						skill.SetInvoked(flag: true);
					}
					else if (unapprovedList.Any((CardDataModel c) => skill.PublishedActiveSkillCount == c.publishedActiveSkillCount && c.IsInvoked))
					{
						skill.SetInvoked(flag: true);
					}
				}
			}
		}
		return base.Start(parameter);
	}
}
