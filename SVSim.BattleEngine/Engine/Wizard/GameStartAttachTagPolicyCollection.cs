using System.Collections.Generic;

namespace Wizard;

public class GameStartAttachTagPolicyCollection : AIPolicyCollection
{
	public void ExecuteAttachTag(AIVirtualField field)
	{
		if (!base.HasPolicy)
		{
			return;
		}
		AIVirtualCard allyClass = field.AllyClass;
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		AIUnknownAction situation = new AIUnknownAction(allyClass);
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(allyClass, emptyPlayPtn, field, situation) && aIPolicyData.Argument is AIGameStartAttachTag aIGameStartAttachTag)
			{
				aIGameStartAttachTag.Execute(allyClass, field, emptyPlayPtn, situation);
			}
		}
	}
}
