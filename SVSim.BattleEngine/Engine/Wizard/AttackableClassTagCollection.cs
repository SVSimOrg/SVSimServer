using System.Collections.Generic;

namespace Wizard;

public class AttackableClassTagCollection : TagCollection
{
	public AttackableClassTagCollection()
		: base(TagCollectionType.AttackableClass)
	{
	}

	private AttackableClassTagCollection(AttackableClassTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new AttackableClassTagCollection(this);
	}

	public void ChangeAttackableClassStatus(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasTag || tagOwner.IsDead || !tagOwner.IsUnit)
		{
			return;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, field, null))
			{
				tagOwner.IsSkillCantAttackClass = false;
			}
		}
	}
}
