namespace Wizard;

public class AIChoiceTransform : AIScriptArgumentExpressions
{
	public AIChoiceTransform(string text)
		: base(text)
	{
	}

	public void RegisterSelfToOwner(AIVirtualCard owner, AIVirtualField field, AIConditionExpressions cond)
	{
		for (int i = 0; i < _exprList.Count; i++)
		{
			AIChoiceTransformCostInformation aIChoiceTransformCostInformation = new AIChoiceTransformCostInformation
			{
				Cost = _exprList[i]
			};
			if (!cond.IsEmpty)
			{
				aIChoiceTransformCostInformation.SetCondition(cond);
			}
			owner.ChoiceTransformCostList = AIParamQuery.AddElementToList(aIChoiceTransformCostInformation, owner.ChoiceTransformCostList);
		}
	}
}
