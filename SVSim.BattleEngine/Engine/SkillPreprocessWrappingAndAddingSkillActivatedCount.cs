using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessWrappingAndAddingSkillActivatedCount : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	public SkillPreprocessWrappingAndAddingSkillActivatedCount(BattleCardBase ownerCard, string wrapping)
	{
		_ownerCard = ownerCard;
		if (wrapping != string.Empty)
		{
			_ownerCard.SetSkillActivatedCountWrapValue(int.Parse(wrapping));
		}
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_ownerCard.IncrementSkillActivatedCount();
		return NullVfx.GetInstance();
	}
}
