namespace Wizard;

public class AICrystalize : AIScriptArgumentExpressions
{
	public AICrystalize(string text)
		: base(text)
	{
	}

	public void RegisterSelfToOwner(AIVirtualCard owner, AIVirtualField field)
	{
		int num = 0;
		while (num < _exprList.Count)
		{
			AICrystalizeInformation aICrystalizeInformation = new AICrystalizeInformation();
			aICrystalizeInformation.Cost = (int)EvalArg(num, owner, EnemyAI.EmptyPlayPtn, field, null);
			num++;
			if (num < _exprList.Count)
			{
				aICrystalizeInformation.CardId = EvalID(num);
				owner.CrystalizeCostList = AIParamQuery.AddElementToList(aICrystalizeInformation, owner.CrystalizeCostList);
				num++;
				continue;
			}
			break;
		}
	}
}
