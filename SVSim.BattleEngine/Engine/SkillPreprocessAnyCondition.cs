using System.Collections.Generic;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessAnyCondition : SkillPreprocessBase
{
	private List<SkillPreprocessBase> _preprocessList = new List<SkillPreprocessBase>();

	public SkillPreprocessAnyCondition(List<SkillPreprocessBase> list)
	{
		_preprocessList = list;
	}

	public SkillPreprocessBase GetRightPreprocess(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		foreach (SkillPreprocessBase preprocess in _preprocessList)
		{
			if (preprocess.IsRight(playerInfoPair, option))
			{
				return preprocess;
			}
		}
		return null;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return GetRightPreprocess(playerInfoPair, option) != null;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		SkillPreprocessBase rightPreprocess = GetRightPreprocess(playerPair, checkerOption);
		if (rightPreprocess != null)
		{
			return rightPreprocess.Start(playerPair, skill, skillProcessor, optionValue, checkerOption);
		}
		return NullVfx.GetInstance();
	}
}
