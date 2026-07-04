using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public abstract class TagCollection
{
	public TagCollectionType Type { get; private set; }

	protected virtual AIPlayTagType[] MANAGED_TAG_TYPES => new AIPlayTagType[1] { AIPlayTagType.None };

	public List<AIPlayTag> TagList { get; protected set; }

	public bool HasTag
	{
		get
		{
			if (TagList != null)
			{
				return 0 < TagList.Count;
			}
			return false;
		}
	}

	public TagCollection(TagCollectionType type)
	{
		TagList = null;
		Type = type;
	}

	public TagCollection(TagCollection param)
	{
		Type = param.Type;
		if (param != null && param.HasTag)
		{
			TagList = new List<AIPlayTag>();
			for (int i = 0; i < param.TagList.Count; i++)
			{
				TagList.Add(param.TagList[i]);
			}
		}
	}

	public virtual void AddTag(AIPlayTag tag)
	{
		if (TagList == null)
		{
			TagList = new List<AIPlayTag>();
		}
		TagList.Add(tag);
	}

	public bool RemoveTagAndCheckIsNoLongerHoldThisType(AIPlayTag tag)
	{
		if (TagList == null)
		{
			return true;
		}
		AIPlayTag aIPlayTag = TagList.FirstOrDefault((AIPlayTag t) => t.Hash == tag.Hash);
		if (aIPlayTag != null)
		{
			TagList.Remove(aIPlayTag);
			RemoveTagFromManagedTagList(aIPlayTag);
		}
		return !TagList.Any((AIPlayTag t) => t.Type == tag.Type);
	}

	public virtual void Clear()
	{
		if (TagList != null && TagList.Count > 0)
		{
			TagList.Clear();
			TagList = null;
		}
	}

	protected virtual void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
	}

	protected bool RemoveTagFromList(List<AIPlayTag> tagList, AIPlayTag tag)
	{
		if (tagList == null || tagList.Count <= 0)
		{
			return false;
		}
		AIPlayTag aIPlayTag = tagList.FirstOrDefault((AIPlayTag t) => t.Hash == tag.Hash);
		if (aIPlayTag != null)
		{
			tagList.Remove(aIPlayTag);
			return true;
		}
		return false;
	}

	protected List<int> GetConditionPassedIndexList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<int> list = null;
		for (int i = 0; i < TagList.Count; i++)
		{
			if (TagList[i].CheckCondition(tagOwner, playPtn, field, situation))
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(i);
			}
		}
		return list;
	}

	public abstract TagCollection Clone();

	public bool IsUnderManagement(AIPlayTagType type)
	{
		AIPlayTagType[] mANAGED_TAG_TYPES = MANAGED_TAG_TYPES;
		for (int i = 0; i < mANAGED_TAG_TYPES.Length; i++)
		{
			if (mANAGED_TAG_TYPES[i] == type)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ExecuteWhenAddTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag, AISituationInfo situation)
	{
	}

	public virtual void ExecuteWhenRemoveTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag)
	{
	}
}
