namespace Wizard;

public class AIAccelerate : AIScriptArgumentExpressions
{
	public AIAccelerate(string text)
		: base(text)
	{
	}

	public void RegisterSelfToOwner(AIVirtualCard owner, AIVirtualField field, AIConditionExpressions cond)
	{
		int num = 0;
		while (num < _exprList.Count)
		{
			AIAccelerateInformation aIAccelerateInformation = new AIAccelerateInformation();
			aIAccelerateInformation.Cost = (int)EvalArg(num, owner, EnemyAI.EmptyPlayPtn, field, null);
			num++;
			if (num < _exprList.Count)
			{
				aIAccelerateInformation.CardId = EvalID(num);
				if (!cond.IsEmpty)
				{
					aIAccelerateInformation.SetCondition(cond);
				}
				owner.AccelerateCostList = AIParamQuery.AddElementToList(aIAccelerateInformation, owner.AccelerateCostList);
				num++;
				continue;
			}
			break;
		}
	}
}
