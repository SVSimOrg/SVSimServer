using System.Linq;
using Wizard;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessConditionCheck : SkillPreprocessBase
{
	protected readonly ConditionSkillFilterCollection _filter;

	protected readonly SkillBase _skill;

	public string ConditionText { get; private set; }

	public SkillPreprocessConditionCheck(SkillBase skill, string conditionString)
	{
		_skill = skill;
		_filter = new ConditionSkillFilterCollection();
		ConditionText = conditionString;
		if (conditionString.First() != '(' || conditionString.Last() != ')')
		{
			return;
		}
		conditionString = conditionString.Substring(1, conditionString.Length - 2);
		string[] array = conditionString.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].First() == '{')
			{
				_filter.VariableCompareFilter.Add(new SkillVariableComareFilter(array[i]));
			}
		}
	}

	public bool Contains(string text)
	{
		return _filter.VariableCompareFilter.Any((SkillVariableComareFilter x) => x.Text.Contains(text));
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (PreexecutionCheck)
		{
			SkillCollectionBase.SetupOptionValue(_skill.OptionValue, playerInfoPair, _skill.SkillPrm.ownerCard, _skill, option);
			return _filter.VariableCompareFilter.All((SkillVariableComareFilter s) => s.Filtering(_skill.OptionValue));
		}
		return true;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false, bool isRegidentStop = false)
	{
		if (!isRegidentStop)
		{
			return IsRight(playerInfoPair, option, PreexecutionCheck);
		}
		return !IsRight(playerInfoPair, option, PreexecutionCheck);
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		return NullVfx.GetInstance();
	}

	public ConditionSkillFilterCollection GetFilter()
	{
		return _filter;
	}
}
