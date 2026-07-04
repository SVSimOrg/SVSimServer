namespace Wizard;

public class ForceImmediateAttackTagCollection : TagCollection
{
	public ForceImmediateAttackTagCollection()
		: base(TagCollectionType.ForceImmediateAttack)
	{
	}

	private ForceImmediateAttackTagCollection(ForceImmediateAttackTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new ForceImmediateAttackTagCollection(this);
	}

	public bool IsForceImmediateAttack(AIVirtualCard tagOwner, AIVirtualField field)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, EnemyAI.EmptyPlayPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}
}
