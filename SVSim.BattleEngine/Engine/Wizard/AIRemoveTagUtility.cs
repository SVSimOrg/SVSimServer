namespace Wizard;

public static class AIRemoveTagUtility
{
	public static void RemoveTemporaryAttachedTag(AIVirtualCard owner, AIVirtualField field, AIPlayTag removingInfo, AISituationInfo situation)
	{
		AIAttachedTagCollection attachedTags = owner.TagCollectionContainer.AttachedTags;
		if (attachedTags != null && attachedTags.HasAnyTag)
		{
			owner.TagCollectionContainer.RemoveOneTagWithUpdatingFieldCardList(owner, removingInfo, field);
			bool needsInheritRemovedTags = situation?.IsLatestAction ?? false;
			attachedTags.RemoveMatchedAttachedTagInformation(owner.IsAlly, owner.CardIndex, removingInfo, needsInheritRemovedTags);
		}
	}

	public static void RemoveOneTag(AIVirtualCard owner, AIVirtualField field, AIPlayTag removingTag, AISituationInfo situation)
	{
		owner.TagCollectionContainer.RemoveOneTagWithUpdatingFieldCardList(owner, removingTag, field);
		if (situation != null && situation.IsLatestAction)
		{
			AIRemovedTagInformation info = new AIRemovedTagInformation(owner, removingTag);
			owner.TagCollectionContainer.RemovedTagCollection.Add(info);
		}
	}
}
