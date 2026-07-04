using System.Collections.Generic;

namespace Wizard;

public class RemoveByDestroyTagCollection : TagCollection
{
	public RemoveByDestroyTagCollection()
		: base(TagCollectionType.RemoveByDestroy)
	{
	}

	private RemoveByDestroyTagCollection(RemoveByDestroyTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new RemoveByDestroyTagCollection(this);
	}

	public bool IsRemoveByDestroy(AIVirtualCard targetCard, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || targetCard == null || !base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIRemoveByDestroy aIRemoveByDestroy && aIPlayTag.CheckCondition(tagOwner, playPtn, tagOwner.SelfField, situation) && aIRemoveByDestroy.IsRemoveByDestroy(targetCard, tagOwner, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}
}
