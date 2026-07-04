using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessDontActivate : SkillPreprocessBase
{
	private BattleCardBase _ownerCard;

	private string _timingText;

	public SkillPreprocessDontActivate(BattleCardBase ownerCard, string timingString)
	{
		_ownerCard = ownerCard;
		_timingText = timingString;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		string timingText = _timingText;
		if (timingText != null && timingText == "getoff")
		{
			return !option.GetOffCards.Contains(_ownerCard);
		}
		return true;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
