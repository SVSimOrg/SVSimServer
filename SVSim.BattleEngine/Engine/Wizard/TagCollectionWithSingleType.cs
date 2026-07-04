using System.Collections.Generic;

namespace Wizard;

public class TagCollectionWithSingleType : TagCollectionWithTypeBase
{
	private AIPlayTagType _type;

	public TagCollectionWithSingleType(AIPlayTagType type, TagCollection tagCollection)
	{
		_type = type;
		base.Collection = tagCollection;
	}

	public override TagCollectionWithTypeBase Clone()
	{
		TagCollection tagCollection = base.Collection.Clone();
		return new TagCollectionWithSingleType(_type, tagCollection);
	}

	public override bool IsUnderManagement(AIPlayTagType type)
	{
		return type == _type;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.Collection.AddTag(tag);
	}

	public override bool RemoveTagAndCheckIsNolongerHoldThisType(AIPlayTag tag)
	{
		return base.Collection.RemoveTagAndCheckIsNoLongerHoldThisType(tag);
	}

	public override void RegisterTypes(List<AIPlayTagType> targetList)
	{
		if (!targetList.Contains(_type))
		{
			targetList.Add(_type);
		}
	}

	public override void RemoveTypes(List<AIPlayTagType> targetList)
	{
		if (targetList.Contains(_type))
		{
			targetList.Remove(_type);
		}
	}

	public override bool HasTag(AIPlayTagType type)
	{
		if (_type == type)
		{
			return base.Collection.HasTag;
		}
		return false;
	}
}
