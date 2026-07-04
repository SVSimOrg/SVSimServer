using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillResidentPreprocessReturnHandActiveReset : SkillPreprocessBase
{
	private SkillBase _skill;

	public SkillResidentPreprocessReturnHandActiveReset(SkillBase skill)
	{
		_skill = skill;
	}

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
		Func<BattleCardBase, SkillProcessor, VfxBase> callStopOneTime = null;
		callStopOneTime = delegate
		{
			skill.SkillPrm.ownerCard.OnReturnCard -= callStopOneTime;
			skill.SetIsResidentSkillStartFlag(flg: false);
			return NullVfx.GetInstance();
		};
		skill.SkillPrm.ownerCard.OnReturnCard += callStopOneTime;
	}
}
