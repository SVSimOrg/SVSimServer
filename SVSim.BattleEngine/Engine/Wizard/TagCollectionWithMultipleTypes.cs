using System.Collections.Generic;

namespace Wizard;

public class TagCollectionWithMultipleTypes : TagCollectionWithTypeBase
{
	private List<AIPlayTagType> _types;

	public TagCollectionWithMultipleTypes(List<AIPlayTagType> types, TagCollection tagCollection)
	{
		_types = types;
		base.Collection = tagCollection;
	}

	public override TagCollectionWithTypeBase Clone()
	{
		TagCollection tagCollection = base.Collection.Clone();
		return new TagCollectionWithMultipleTypes(new List<AIPlayTagType>(_types), tagCollection);
	}

	public override bool IsUnderManagement(AIPlayTagType type)
	{
		return base.Collection.IsUnderManagement(type);
	}

	public override void AddTag(AIPlayTag tag)
	{
		if (!_types.Contains(tag.Type))
		{
			_types.Add(tag.Type);
		}
		base.Collection.AddTag(tag);
	}

	public override bool RemoveTagAndCheckIsNolongerHoldThisType(AIPlayTag tag)
	{
		if (base.Collection.RemoveTagAndCheckIsNoLongerHoldThisType(tag))
		{
			_types.Remove(tag.Type);
			return _types.Count <= 0;
		}
		return false;
	}

	public override void RegisterTypes(List<AIPlayTagType> targetList)
	{
		for (int i = 0; i < _types.Count; i++)
		{
			if (!targetList.Contains(_types[i]))
			{
				targetList.Add(_types[i]);
			}
		}
	}

	public override void RemoveTypes(List<AIPlayTagType> targetList)
	{
		for (int i = 0; i < _types.Count; i++)
		{
			if (targetList.Contains(_types[i]))
			{
				targetList.Remove(_types[i]);
			}
		}
	}

	public override bool HasTag(AIPlayTagType type)
	{
		if (_types.Contains(type))
		{
			return base.Collection.HasTag;
		}
		return false;
	}
}
