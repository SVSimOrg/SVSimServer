using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessRemoveFromInPlayStop : SkillPreprocessBase
{
	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		SetUp(skill);
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		SetUp(skill);
	}

	private void SetUp(SkillBase skill)
	{
		Func<bool, SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(bool flag, SkillProcessor skillProcessorOneTime)
		{
			skill.SkillPrm.ownerCard.OnRemoveFromInPlayAfterOneTime -= callStopOneTime;
			return StopSkill(skill, skillProcessorOneTime);
		};
		skill.SkillPrm.ownerCard.OnRemoveFromInPlayAfterOneTime += callStopOneTime;
		if (skill.GetAttachSkill != null)
		{
			Func<bool, SkillProcessor, VfxBase> callStopOneTimeAttach = null;
			BattleCardBase attachOwner = skill.GetAttachSkill.SkillPrm.ownerCard;
			callStopOneTimeAttach = delegate(bool flag, SkillProcessor skillProcessorOneTime)
			{
				attachOwner.OnRemoveFromInPlayAfterOneTime -= callStopOneTimeAttach;
				return StopSkill(skill, skillProcessorOneTime);
			};
			attachOwner.OnRemoveFromInPlayAfterOneTime += callStopOneTimeAttach;
		}
	}
}
