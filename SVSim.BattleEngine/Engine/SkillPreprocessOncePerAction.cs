using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessOncePerAction : SkillPreprocessBase
{
	private readonly SkillBase _skill;

	public SkillPreprocessOncePerAction(SkillBase skill)
	{
		_skill = skill;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return !option.ProcessSkillList.Any((SkillBase s) => s == _skill);
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}
}
