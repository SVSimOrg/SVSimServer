using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessDamageGiveStop : SkillPreprocessBase
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
		Func<SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate(SkillProcessor skillProcessorOneTime)
		{
			skill.SkillPrm.ownerCard.OnGiveDamage -= callStopOneTime;
			return StopSkill(skill, skillProcessorOneTime);
		};
		skill.SkillPrm.ownerCard.OnGiveDamage += callStopOneTime;
	}
}
