using System.Collections.Generic;

namespace Wizard;

public class ReferableVirtualCardBuildParameterCollection : AIVirtualCardBuildParameterCollectionBase
{
	public AIBarrierInfoCollection BarrierInfoCollection;

	public int AddedChangeInplayTagActivatedInfoIncrement;

	public List<ChangeInplayTagCollection.ChangeInplayArgumentIsActivatedInfo> ChangeInplayTagIsActivatedInfoList;

	public AIVirtualCardParameter BaseParameter;

	public AIVirtualCardParameter OtherEvolveCardParameter;

	public AIAttachedTagCollection AttachedTags;

	public AIRemovedTagCollection RemovedTags;

	public List<AIActivateCounter> ActivateCounterList;

	public ReferableVirtualCardBuildParameterCollection(AIVirtualCard card)
		: base(card)
	{
		BarrierInfoCollection = card.BarrierInfoCollection.GetInheritanceForNewVirtualField();
		if (card.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay))
		{
			AddedChangeInplayTagActivatedInfoIncrement = card.TagCollectionContainer.ChangeInplayTags.AddedChangeInplayActivatedInfoIncrement;
			ChangeInplayTagIsActivatedInfoList = card.TagCollectionContainer.ChangeInplayTags.TagIsActivatedList;
		}
		BaseParameter = card.BaseParameter;
		OtherEvolveCardParameter = card.OtherEvolveParameter;
		AttachedTags = card.TagCollectionContainer.AttachedTags;
		RemovedTags = card.TagCollectionContainer.RemovedTagCollection;
		if (card.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
		{
			ActivateCounterList = card.TagCollectionContainer.ActivateCountTags.ActivateCounterList;
		}
	}
}
