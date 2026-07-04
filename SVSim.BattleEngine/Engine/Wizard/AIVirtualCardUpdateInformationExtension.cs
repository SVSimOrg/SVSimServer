using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualCardUpdateInformationExtension
{
	public static void UpdateCardInformation(this AIVirtualCard card, AIVirtualCard source)
	{
		card.UpdateOtherEvolveParameter(source);
		card.UpdateAttachTagInformation(source);
		card.UpdateRemovedTagInformation(source);
		card.UpdateBarrierInfo(source);
		card.UpdateWhenChangeInplayTagInformation(source);
		card.UpdateRemovedAttachTagInformation(source);
		card.UpdateActivateCounter(source);
		card.CopyCannotAttackInfoList(source);
	}

	private static void UpdateOtherEvolveParameter(this AIVirtualCard card, AIVirtualCard source)
	{
		card.SetOtherEvolveParameterFromVirtualCard(source);
	}

	private static void UpdateWhenChangeInplayTagInformation(this AIVirtualCard owner, AIVirtualCard updateSource)
	{
		if (owner.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay) && updateSource.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay))
		{
			ChangeInplayTagCollection changeInplayTags = owner.TagCollectionContainer.ChangeInplayTags;
			ChangeInplayTagCollection changeInplayTags2 = updateSource.TagCollectionContainer.ChangeInplayTags;
			changeInplayTags.UpdateIsActivatedInformation(changeInplayTags2.TagIsActivatedList, changeInplayTags2.AddedChangeInplayActivatedInfoIncrement);
		}
	}

	private static void UpdateAttachTagInformation(this AIVirtualCard owner, AIVirtualCard updateSource)
	{
		AIAttachedTagCollection attachedTags = updateSource.TagCollectionContainer.AttachedTags;
		if (attachedTags == null || !attachedTags.HasAnyTag)
		{
			return;
		}
		List<ulong> list = null;
		AIAttachedTagCollection attachedTags2 = owner.TagCollectionContainer.AttachedTags;
		if (attachedTags2 != null && attachedTags2.HasAnyTag)
		{
			for (int i = 0; i < attachedTags2.AllList.Count; i++)
			{
				list = AIParamQuery.AddElementToList(attachedTags2.AllList[i].Hash, list);
			}
		}
		for (int j = 0; j < attachedTags.AllList.Count; j++)
		{
			AIAttachedTagInformation aIAttachedTagInformation = attachedTags.AllList[j];
			int num = -1;
			if (list != null && list.Count > 0)
			{
				num = list.IndexOf(aIAttachedTagInformation.Hash);
			}
			if (num >= 0)
			{
				list.RemoveAt(num);
			}
			else
			{
				owner.TagCollectionContainer.AttachTag(aIAttachedTagInformation, owner, null);
			}
		}
	}

	private static void UpdateRemovedTagInformation(this AIVirtualCard tagOwner, AIVirtualCard updateSource)
	{
		AIRemovedTagCollection removedTagCollection = tagOwner.TagCollectionContainer.RemovedTagCollection;
		AIRemovedTagCollection removedTagCollection2 = updateSource.TagCollectionContainer.RemovedTagCollection;
		if ((removedTagCollection.AllList == null && removedTagCollection2.AllList == null) || removedTagCollection2.AllList == null || removedTagCollection2.AllList.Count <= 0)
		{
			return;
		}
		int num = ((removedTagCollection.AllList != null) ? removedTagCollection.AllList.Count : 0);
		int num2 = ((removedTagCollection2.AllList != null) ? removedTagCollection2.AllList.Count : 0);
		if (num < num2)
		{
			for (int i = num; i < removedTagCollection2.AllList.Count; i++)
			{
				AIRemovedTagInformation aIRemovedTagInformation = removedTagCollection2.AllList[i];
				tagOwner.TagCollectionContainer.RemoveOneTagWithUpdatingFieldCardList(tagOwner, aIRemovedTagInformation.Tag, tagOwner.SelfField);
				removedTagCollection.Add(aIRemovedTagInformation);
			}
		}
	}

	private static void UpdateRemovedAttachTagInformation(this AIVirtualCard owner, AIVirtualCard source)
	{
		AIAttachedTagCollection attachedTags = owner.TagCollectionContainer.AttachedTags;
		AIAttachedTagCollection attachedTags2 = source.TagCollectionContainer.AttachedTags;
		if (attachedTags != null && attachedTags.HasAnyTag && attachedTags2 != null && attachedTags2.HasRemovedInfoCaches)
		{
			List<AIPlayTag> removedTagCacheList = attachedTags2.RemovedTagCacheList;
			for (int i = 0; i < removedTagCacheList.Count; i++)
			{
				AIPlayTag removingInfo = removedTagCacheList[i];
				AIRemoveTagUtility.RemoveTemporaryAttachedTag(owner, owner.SelfField, removingInfo, null);
			}
			removedTagCacheList.Clear();
		}
	}

	private static void UpdateBarrierInfo(this AIVirtualCard card, AIVirtualCard source)
	{
		card.BarrierInfoCollection = source.BarrierInfoCollection.Clone();
	}

	private static void UpdateActivateCounter(this AIVirtualCard card, AIVirtualCard source)
	{
		if (card.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount) && source.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
		{
			ActivateCountTagCollection activateCountTags = card.TagCollectionContainer.ActivateCountTags;
			ActivateCountTagCollection activateCountTags2 = source.TagCollectionContainer.ActivateCountTags;
			activateCountTags.UpdateCounterList(activateCountTags2.ActivateCounterList);
		}
	}
}
