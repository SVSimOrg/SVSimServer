using System.Collections.Generic;

namespace Wizard;

public abstract class TagCollectionWithTypeBase
{
	public TagCollection Collection { get; protected set; }

	public abstract TagCollectionWithTypeBase Clone();

	public abstract bool IsUnderManagement(AIPlayTagType type);

	public abstract void AddTag(AIPlayTag tag);

	public abstract bool RemoveTagAndCheckIsNolongerHoldThisType(AIPlayTag tag);

	public abstract void RegisterTypes(List<AIPlayTagType> targetList);

	public abstract void RemoveTypes(List<AIPlayTagType> targetList);

	public abstract bool HasTag(AIPlayTagType type);

	public bool IsEmpty()
	{
		return !Collection.HasTag;
	}

	public bool RemoveOneTag(AIVirtualCard owner, AIVirtualField field, AIPlayTag removingTag)
	{
		if (IsEmpty())
		{
			return false;
		}
		if (RemoveTagAndCheckIsNolongerHoldThisType(removingTag))
		{
			field.CardListSet.RemoveCardFromTagHolder(owner, removingTag);
		}
		Collection.ExecuteWhenRemoveTag(owner, field, removingTag);
		return !HasTag(removingTag.Type);
	}

	public void RemoveAllTags(AIVirtualCard owner, AIVirtualField field)
	{
		if (IsEmpty())
		{
			return;
		}
		List<AIPlayTag> tagList = Collection.TagList;
		while (tagList.Count > 0)
		{
			AIPlayTag aIPlayTag = tagList[0];
			if (RemoveTagAndCheckIsNolongerHoldThisType(aIPlayTag))
			{
				field.CardListSet.RemoveCardFromTagHolder(owner, aIPlayTag);
			}
		}
		Collection.Clear();
	}
}
