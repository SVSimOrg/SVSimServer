using System.Collections.Generic;

namespace Wizard;

public class AIRemovedTagCollection
{
	public List<AIRemovedTagInformation> AllList;

	public AIRemovedTagCollection()
	{
		AllList = null;
	}

	public AIRemovedTagCollection Clone()
	{
		AIRemovedTagCollection aIRemovedTagCollection = new AIRemovedTagCollection();
		if (AllList != null)
		{
			aIRemovedTagCollection.AllList = new List<AIRemovedTagInformation>(AllList);
		}
		return aIRemovedTagCollection;
	}

	public void Add(AIRemovedTagInformation info)
	{
		AllList = AIParamQuery.AddElementToList(info, AllList);
	}

	public void RemoveTagFromCard(AIVirtualCard card)
	{
		if (AllList == null || AllList.Count <= 0)
		{
			return;
		}
		AIVirtualField selfField = card.SelfField;
		for (int i = 0; i < AllList.Count; i++)
		{
			AIRemovedTagInformation aIRemovedTagInformation = AllList[i];
			if (aIRemovedTagInformation.IsAlly == card.IsAlly && aIRemovedTagInformation.CardIndex == card.CardIndex)
			{
				card.TagCollectionContainer.RemoveOneTagWithUpdatingFieldCardList(card, aIRemovedTagInformation.Tag, selfField);
				card.TagCollectionContainer.RemovedTagCollection.Add(aIRemovedTagInformation);
			}
		}
	}
}
