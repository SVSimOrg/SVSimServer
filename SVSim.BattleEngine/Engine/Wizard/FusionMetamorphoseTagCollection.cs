using System.Collections.Generic;

namespace Wizard;

public class FusionMetamorphoseTagCollection : TagCollection
{
	public FusionMetamorphoseTagCollection()
		: base(TagCollectionType.FusionMetamorphose)
	{
	}

	private FusionMetamorphoseTagCollection(FusionMetamorphoseTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new FusionMetamorphoseTagCollection(this);
	}

	public bool ExecuteMetamorphose(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				if (aIPlayTag.ArgumentExpressions is AIFusionMetamorphose aIFusionMetamorphose)
				{
					aIFusionMetamorphose.Execute(owner, field, playPtn, situation);
					return true;
				}
				AIConsoleUtility.LogError("FusionMetamorphoseTagCollection.GetMetamorphoseTargetCardID() Error!! arg is not AIFusionMetamorphose!!!!!");
			}
		}
		return false;
	}
}
