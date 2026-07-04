using System.Collections.Generic;

namespace Wizard;

public class AIChoiceTransformCostInformation
{
	public AIPolishConvertedExpression Cost;

	public AIConditionExpressions Condition { get; private set; }

	public void SetCondition(AIConditionExpressions cond)
	{
		Condition = cond;
	}

	public bool CheckCondition(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (Condition == null || Condition.IsEmpty)
		{
			return true;
		}
		return Condition.CheckCondition(owner, playPtn, field, situation);
	}
}
