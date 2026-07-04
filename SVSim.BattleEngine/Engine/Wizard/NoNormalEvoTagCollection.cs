namespace Wizard;

public class NoNormalEvoTagCollection : TagCollection
{
	public NoNormalEvoTagCollection()
		: base(TagCollectionType.NoNormalEvo)
	{
	}

	private NoNormalEvoTagCollection(NoNormalEvoTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new NoNormalEvoTagCollection(this);
	}

	public bool IsNoNormalEvo(AIVirtualCard owner, AIVirtualField field)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(owner, EnemyAI.EmptyPlayPtn, field, null))
			{
				return true;
			}
		}
		return false;
	}
}
