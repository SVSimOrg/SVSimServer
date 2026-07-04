using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessRandomCount : SkillPreprocessBase
{
	private readonly int _randomCount;

	private readonly IReadOnlyBattleCardInfo _card;

	public SkillPreprocessRandomCount(IReadOnlyBattleCardInfo card, int randomCount)
	{
		_card = card;
		_randomCount = randomCount;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _card.SkillApplyInformation.SkillRandomCount == _randomCount;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
