namespace Wizard;

public class AIEnhance : AIScriptArgumentExpressions
{
	public AIEnhance(string text)
		: base(text)
	{
	}

	public void RegisterSelfToOwner(AIVirtualCard owner, AIVirtualField field)
	{
		for (int i = 0; i < _exprList.Count; i++)
		{
			int element = (int)EvalArg(i, owner, EnemyAI.EmptyPlayPtn, field, null);
			owner.EnhanceCostList = AIParamQuery.AddElementToList(element, owner.EnhanceCostList);
		}
	}
}
