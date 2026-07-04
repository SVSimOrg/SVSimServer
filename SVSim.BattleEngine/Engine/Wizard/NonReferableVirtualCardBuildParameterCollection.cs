namespace Wizard;

public class NonReferableVirtualCardBuildParameterCollection : AIVirtualCardBuildParameterCollectionBase
{
	public AIAttachedTagCollection AttachedTags;

	public AIRemovedTagCollection RemovedTags;

	public NonReferableVirtualCardBuildParameterCollection(AIVirtualCard card)
		: base(card)
	{
		AttachedTags = card.TagCollectionContainer.AttachedTags;
		RemovedTags = card.TagCollectionContainer.RemovedTagCollection;
	}
}
