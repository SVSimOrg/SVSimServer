using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_attach_skill : Skill_attach_skill
{
	public NetworkSkill_attach_skill(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		NetworkSkill_attach_skill networkSkill_attach_skill = this;
		base.OnSkillStart += delegate(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption)
		{
			if (networkSkill_attach_skill.IsBattleLog && networkSkill_attach_skill.IsTargetInOpponentHand() && networkSkill_attach_skill.IsTargetSelectedCard)
			{
				SkillCreator.SkillBuildInfo buildInfo = Skill_attach_skill.CreateAttachSkillBuildInfo(option);
				networkSkill_attach_skill.LoggingOpponentPrivateCard(targetCards, buildInfo);
			}
		};
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		VfxWithLoading result = base.Start(parameter);
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(_effectTargets);
		IEnumerable<NetworkSkill_token_draw> source = from s in _attachSkills
			where s is NetworkSkill_token_draw && s.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.token_draw, "_OPT_NULL_").Contains(SkillFilterCreator.ContentKeyword.load_target_card_id.ToString())
			select s as NetworkSkill_token_draw;
		if (source.Count() == 0)
		{
			return result;
		}
		for (int num = 0; num < source.Count(); num++)
		{
			source.ElementAt(num).CreateTokenCardAttachTiming(base.SkillPrm.ownerCard.SkillApplyInformation.LoadTargetList());
		}
		return result;
	}

	public override void CallOnAttachSkill(List<BattleCardBase> targetCards)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnAttachSkill(base.BuildInfo, targetCards, base.SkillPrm.ownerCard);
	}
}
