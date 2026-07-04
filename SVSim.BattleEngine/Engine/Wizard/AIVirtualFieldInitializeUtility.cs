namespace Wizard;

public static class AIVirtualFieldInitializeUtility
{
	public static void FindBuildParameterAndApply(this AIVirtualCard self, AIVirtualFieldBuildParameterCollction fieldBuildParameter)
	{
		ReferableVirtualCardBuildParameterCollection referableCardBuildParameter = fieldBuildParameter.GetReferableCardBuildParameter(self);
		if (referableCardBuildParameter != null)
		{
			self.GetInformationFromCardBuildParameter(referableCardBuildParameter);
		}
	}

	private static void GetInformationFromCardBuildParameter(this AIVirtualCard self, ReferableVirtualCardBuildParameterCollection cardBuildParameter)
	{
		self.BarrierInfoCollection.GetBarrierInfoFromPreviousField(cardBuildParameter.BarrierInfoCollection);
		cardBuildParameter.AttachedTags.AttachTagToReceiver(self);
		cardBuildParameter.RemovedTags.RemoveTagFromCard(self);
		if (self.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenChangeInplay))
		{
			self.TagCollectionContainer.ChangeInplayTags.UpdateIsActivatedInformation(cardBuildParameter.ChangeInplayTagIsActivatedInfoList, cardBuildParameter.AddedChangeInplayTagActivatedInfoIncrement);
		}
		if (self.TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
		{
			self.TagCollectionContainer.ActivateCountTags.UpdateCounterList(cardBuildParameter.ActivateCounterList);
		}
		self.SetOtherEvolveParameterFromBuildParameter(cardBuildParameter);
	}
}
