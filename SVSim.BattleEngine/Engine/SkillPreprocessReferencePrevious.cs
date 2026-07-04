using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessReferencePrevious : SkillPreprocessBase
{
	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return option.IsRefPrev;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}

	public static SkillBase GetPreviousSkill(SkillCollectionBase skills, SkillBase skill)
	{
		int num = skills.IndexOf(skill) - 1;
		while (num >= 0 && num < skills.Count())
		{
			SkillBase skillBase = skills.Get(num);
			if (!skillBase.PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessReferencePrevious))
			{
				return skillBase;
			}
			num--;
		}
		return null;
	}
}
