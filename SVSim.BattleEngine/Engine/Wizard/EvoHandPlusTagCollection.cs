using System.Collections.Generic;

namespace Wizard;

public class EvoHandPlusTagCollection : TagCollection
{
	public EvoHandPlusTagCollection()
		: base(TagCollectionType.EvoHandPlus)
	{
	}

	private EvoHandPlusTagCollection(EvoHandPlusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new EvoHandPlusTagCollection(this);
	}

	public int GetEvoHandPlusCount(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				num += (int)aIPlayTag.EvalArg(owner, playPtn, field, situation);
			}
		}
		return num;
	}
}
