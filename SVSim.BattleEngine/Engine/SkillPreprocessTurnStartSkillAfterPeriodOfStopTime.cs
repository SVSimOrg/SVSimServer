using System;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnStartSkillAfterPeriodOfStopTime : SkillPreprocessPeriodBase
{
	private SkillBase _skill;

	private BattleCardBase _ownerCard;

	public SkillPreprocessTurnStartSkillAfterPeriodOfStopTime(SkillBase skill, BattleCardBase ownerCard, string period)
		: base(ownerCard, period)
	{
		_skill = skill;
		_ownerCard = ownerCard;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_ownerCard.SelfBattlePlayer.OnTurnEnd += delegate
		{
			ReducePeriodCount();
			return NullVfx.GetInstance();
		};
		BattlePlayerBase selfBattlePlayer = _ownerCard.SelfBattlePlayer;
		selfBattlePlayer.OnTurnStartSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Combine(selfBattlePlayer.OnTurnStartSkillAfter, (Func<SkillProcessor, VfxBase>)((SkillProcessor _) => (_effectiveRange == base.PeriodCount) ? StopSkill(_skill, _) : NullVfxWithLoading.GetInstance()));
		_ownerCard.OnRemoveFromInPlayAfterOneTime += delegate(bool flg, SkillProcessor skillProcessorOneTime)
		{
			ResetPeriodCount();
			return StopSkill(_skill, skillProcessorOneTime);
		};
		return NullVfx.GetInstance();
	}
}
