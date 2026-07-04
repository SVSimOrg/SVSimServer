using System.Collections.Generic;

namespace Wizard;

public class AIAttachedTagCollection
{
	public List<AIAttachedTagInformation> AllList;

	public List<AIPlayTag> RemovedTagCacheList { get; private set; }

	public bool HasAnyTag
	{
		get
		{
			if (AllList != null)
			{
				return AllList.Count > 0;
			}
			return false;
		}
	}

	public bool HasRemovedInfoCaches
	{
		get
		{
			if (RemovedTagCacheList != null)
			{
				return RemovedTagCacheList.Count > 0;
			}
			return false;
		}
	}

	public AIAttachedTagCollection()
	{
		AllList = null;
	}

	public AIAttachedTagCollection Clone()
	{
		return new AIAttachedTagCollection
		{
			AllList = AIParamQuery.CloneList(AllList)
		};
	}

	public void AddAttachedTagInformation(AIAttachedTagInformation info)
	{
		AllList = AIParamQuery.AddElementToList(info, AllList);
	}

	public void RemoveMatchedAttachedTagInformation(bool ownerIsAlly, int ownerCardIndex, AIPlayTag removeingTag, bool needsInheritRemovedTags)
	{
		if (AllList == null || AllList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < AllList.Count; i++)
		{
			AIAttachedTagInformation aIAttachedTagInformation = AllList[i];
			if (aIAttachedTagInformation.IsReceiverAlly == ownerIsAlly && aIAttachedTagInformation.ReceiverIndex == ownerCardIndex && aIAttachedTagInformation.Tag.Hash == removeingTag.Hash)
			{
				AllList.RemoveAt(i);
				if (needsInheritRemovedTags)
				{
					RemovedTagCacheList = AIParamQuery.AddElementToList(removeingTag, RemovedTagCacheList);
				}
				break;
			}
		}
	}

	public void RemoveAllAttachedTagInformation(AISituationInfo situation)
	{
		if (AllList == null || AllList.Count <= 0)
		{
			return;
		}
		bool flag = situation?.IsLatestAction ?? false;
		while (AllList.Count > 0)
		{
			if (flag)
			{
				AIAttachedTagInformation aIAttachedTagInformation = AllList[0];
				RemovedTagCacheList = AIParamQuery.AddElementToList(aIAttachedTagInformation.Tag, RemovedTagCacheList);
			}
			AllList.RemoveAt(0);
		}
	}

	public void AttachTagToReceiver(AIVirtualCard receiver)
	{
		if (AllList == null || AllList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < AllList.Count; i++)
		{
			AIAttachedTagInformation aIAttachedTagInformation = AllList[i];
			if (aIAttachedTagInformation.IsReceiverAlly == receiver.IsAlly && aIAttachedTagInformation.ReceiverIndex == receiver.CardIndex)
			{
				receiver.TagCollectionContainer.AttachTag(aIAttachedTagInformation, receiver, null);
			}
		}
	}

	public int GetAttachedTagCountFromId(int id)
	{
		if (!HasAnyTag)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < AllList.Count; i++)
		{
			if (AllList[i].ProviderCardId == id)
			{
				num++;
			}
		}
		return num;
	}
}
