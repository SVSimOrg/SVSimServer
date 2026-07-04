using System.Collections.Generic;

namespace Wizard;

public class AIResonanceHeal : AIFiltersAndSelectTypeArgument
{
	private readonly int HEAL_ARG_OFFSET = 1;

	private AIPolishConvertedExpression _healValueArg { get; set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIResonanceHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_healValueArg = _exprList[_exprList.Count - HEAL_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int heal = (int)_healValueArg.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targetsFromField, field, heal, playPtn, situation);
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}
}
