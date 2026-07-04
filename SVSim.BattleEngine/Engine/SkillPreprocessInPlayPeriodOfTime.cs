using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessInPlayPeriodOfTime : SkillPreprocessPeriodBase
{
	public SkillPreprocessInPlayPeriodOfTime(BattleCardBase ownerCard, string period)
		: base(ownerCard, period)
	{
		ownerCard.OnTurnStart += delegate
		{
			ReducePeriodCount();
		};
		ownerCard.OnRemoveFromInPlayAfterOneTime += delegate
		{
			ResetPeriodCount();
			return NullVfx.GetInstance();
		};
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _effectiveRange == base.PeriodCount;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
