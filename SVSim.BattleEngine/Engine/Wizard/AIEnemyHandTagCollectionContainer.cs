using System;
using System.Collections.Generic;

namespace Wizard;

internal class AIEnemyHandTagCollectionContainer : AITagCollectionContainer
{
	private readonly AIPlayTagType[] ENEMY_HAND_LEGAL_TAG_TYPES = new AIPlayTagType[19]
	{
		AIPlayTagType.TurnEndAttachTag,
		AIPlayTagType.Enhance,
		AIPlayTagType.Accelerate,
		AIPlayTagType.Crystalize,
		AIPlayTagType.ChoiceTransform,
		AIPlayTagType.GenerateTag,
		AIPlayTagType.PlayCopyTag,
		AIPlayTagType.FanfareCopyTag,
		AIPlayTagType.PlayAttachTag,
		AIPlayTagType.FanfareAttachTag,
		AIPlayTagType.PlaySelect,
		AIPlayTagType.FanfareSelect,
		AIPlayTagType.PlayHandSelect,
		AIPlayTagType.FanfareHandSelect,
		AIPlayTagType.FanfareAttachStyle,
		AIPlayTagType.PlayReanimate,
		AIPlayTagType.FanfareReanimate,
		AIPlayTagType.PlayRemoveSkill,
		AIPlayTagType.FanfareRemoveSkill
	};

	public AIEnemyHandTagCollectionContainer()
	{
	}

	public AIEnemyHandTagCollectionContainer(AITagCollectionContainer container, AIVirtualCard owner)
	{
		if (container.Count <= 0)
		{
			return;
		}
		_holdingTagTypes = new List<AIPlayTagType>();
		_holdingTagCollectionTypes = new List<TagCollectionType>();
		base.TagDictionary = new List<TagCollectionWithTypeBase>();
		for (int i = 0; i < container.Count; i++)
		{
			TagCollectionWithTypeBase tagCollectionWithTypeBase = container.TagDictionary[i];
			if (IsEnemyHandLegalTagCollection(tagCollectionWithTypeBase))
			{
				base.TagDictionary.Add(tagCollectionWithTypeBase.Clone());
				tagCollectionWithTypeBase.RegisterTypes(_holdingTagTypes);
				_holdingTagCollectionTypes.Add(tagCollectionWithTypeBase.Collection.Type);
			}
		}
		CreateFixedUseCostListWhenInit(owner, owner.SelfField);
	}

	public override void InitTags(AIVirtualCard owner, AIParamQuery query)
	{
		int tagCount = query.GetTagCount(owner);
		for (int i = 0; i < tagCount; i++)
		{
			AIPlayTag tag = query.GetTag(owner, i);
			if (Array.IndexOf(ENEMY_HAND_LEGAL_TAG_TYPES, tag.Type) >= 0)
			{
				AddTag(tag, owner, null);
			}
		}
		CreateFixedUseCostListWhenInit(owner, owner.SelfField);
	}

	private bool IsEnemyHandLegalTagCollection(TagCollectionWithTypeBase tagCollectionWithTypes)
	{
		for (int i = 0; i < ENEMY_HAND_LEGAL_TAG_TYPES.Length; i++)
		{
			if (tagCollectionWithTypes.IsUnderManagement(ENEMY_HAND_LEGAL_TAG_TYPES[i]))
			{
				return true;
			}
		}
		return false;
	}
}
