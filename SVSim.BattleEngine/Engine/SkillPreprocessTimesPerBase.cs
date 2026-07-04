using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessTimesPerBase : SkillPreprocessBase
{
	protected int _invokeCount;

	protected int _limitCount;

	public SkillPreprocessTimesPerBase(int limit)
	{
		_limitCount = limit;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _invokeCount < _limitCount;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		_invokeCount++;
		return NullVfx.GetInstance();
	}

	public override void Clone(SkillPreprocessBase source, SkillBase skill)
	{
		if (source is SkillPreprocessTimesPerBase skillPreprocessTimesPerBase)
		{
			_limitCount = skillPreprocessTimesPerBase._limitCount;
			_invokeCount = skillPreprocessTimesPerBase._invokeCount;
		}
	}
}
