using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessWhenEvolveEndRemove : SkillPreprocessBase
{
	public SkillPreprocessWhenEvolveEndRemove(SkillBase skill)
	{
		SetUp(skill);
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		SetUp(skill);
	}

	private void SetUp(SkillBase skill)
	{
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessorOneTime)
		{
			skill.SkillPrm.ownerCard.OnBeforeEvolve -= callStopOneTime;
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(StopSkill(skill, skillProcessorOneTime));
			skill.SkillPrm.ownerCard.Skills.Remove(skill);
			if (skill.GetAttachSkill is Skill_attach_skill skill_attach_skill && !skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
			{
				sequentialVfxPlayer.Register(skill_attach_skill.StopSpecificCard(skill.SkillPrm.ownerCard));
			}
			return sequentialVfxPlayer;
		};
		skill.SkillPrm.ownerCard.OnBeforeEvolve += callStopOneTime;
	}
}
