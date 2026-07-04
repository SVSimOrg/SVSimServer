namespace Wizard;

public class BounceBonusTagCollection : TagCollection
{
	public BounceBonusTagCollection()
		: base(TagCollectionType.BounceBonus)
	{
	}

	private BounceBonusTagCollection(BounceBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BounceBonusTagCollection(this);
	}

	public float GetBounceBonus(AIVirtualField field, AIVirtualCard tagOwner)
	{
		if (base.HasTag)
		{
			float num = 0f;
			for (int i = 0; i < base.TagList.Count; i++)
			{
				if (base.TagList[i].CheckCondition(tagOwner, field.BestPlayPtn, field, null))
				{
					num += base.TagList[i].EvalArg(tagOwner, field.BestPlayPtn, field, null);
				}
			}
			return num;
		}
		return 0f;
	}
}
