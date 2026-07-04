using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTurnEndPeriodOfStopTime : SkillPreprocessPeriodBase
{
	private SkillBase _skill;

	private BattleCardBase _ownerCard;

	public SkillPreprocessTurnEndPeriodOfStopTime(SkillBase skill, BattleCardBase ownerCard, string period)
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
		_ownerCard.SelfBattlePlayer.OnTurnStartBeforeDraw -= TurnStartFunction;
		_ownerCard.SelfBattlePlayer.OnTurnStartBeforeDraw += TurnStartFunction;
		_ownerCard.SelfBattlePlayer.OnTurnEnd -= TurnEndFunction;
		_ownerCard.SelfBattlePlayer.OnTurnEnd += TurnEndFunction;
		return NullVfx.GetInstance();
	}

	private VfxBase TurnStartFunction(SkillProcessor skillProcessor)
	{
		ReducePeriodCount();
		return NullVfx.GetInstance();
	}

	private VfxBase TurnEndFunction(SkillProcessor skillProcessor)
	{
		if (_effectiveRange == base.PeriodCount)
		{
			return StopSkill(_skill, skillProcessor);
		}
		return NullVfxWithLoading.GetInstance();
	}
}
