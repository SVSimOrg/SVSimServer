using System.Collections.Generic;

namespace Wizard;

public class AIUseMinTagCollection : TagCollection
{
	protected List<AIPlayTag> _useMinTagList;

	protected List<AIPlayTag> _nonUseMinTagList;

	public bool HasUseMinTag
	{
		get
		{
			if (_useMinTagList != null)
			{
				return _useMinTagList.Count > 0;
			}
			return false;
		}
	}

	public bool HasNonUseMinTag
	{
		get
		{
			if (_nonUseMinTagList != null)
			{
				return _nonUseMinTagList.Count > 0;
			}
			return false;
		}
	}

	public AIUseMinTagCollection(TagCollectionType type)
		: base(type)
	{
	}

	protected AIUseMinTagCollection(AIUseMinTagCollection param)
		: base(param)
	{
		_nonUseMinTagList = AIParamQuery.AddRangeToList(param._nonUseMinTagList, _nonUseMinTagList);
		_useMinTagList = AIParamQuery.AddRangeToList(param._useMinTagList, _useMinTagList);
	}

	public override TagCollection Clone()
	{
		return new AIUseMinTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if ((tag.ArgumentExpressions as AIUseMinArgument).IsUseMin)
		{
			if (_useMinTagList == null)
			{
				_useMinTagList = new List<AIPlayTag>();
			}
			_useMinTagList.Add(tag);
		}
		else
		{
			if (_nonUseMinTagList == null)
			{
				_nonUseMinTagList = new List<AIPlayTag>();
			}
			_nonUseMinTagList.Add(tag);
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_useMinTagList, tag))
		{
			RemoveTagFromList(_nonUseMinTagList, tag);
		}
	}
}
