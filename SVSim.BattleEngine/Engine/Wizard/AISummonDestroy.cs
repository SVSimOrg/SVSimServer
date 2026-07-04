using System.Collections.Generic;

namespace Wizard;

public class AISummonDestroy : AIFiltersArgument
{
	private AIPolishConvertedExpression _destroyCount;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected int SELECT_TYPE_OFFSET => 2;

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AISummonDestroy(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_destroyCount = _exprList[_exprList.Count - 1];
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int num = (int)_destroyCount.EvalArg(tagOwner, playPtn, field, situation);
		if (num > 0)
		{
			switch (SelectType)
			{
			case AIScriptTokenArgType.RANDOM_SELECT:
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation, num);
				break;
			case AIScriptTokenArgType.OLDEST_SELECT:
				AISkillSimulationUtility.DestroyOldest(targetsFromField, field, situation, num);
				break;
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT,
			AIScriptTokenArgType.OLDEST_SELECT
		};
	}
}
