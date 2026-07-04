using System.Collections.Generic;

namespace Wizard;

public class AttachedSkillInfoReceiveDataCollection
{
	public List<AttachedSkillInfoReceiveData> InfoList { get; private set; }

	public AttachedSkillInfoReceiveDataCollection()
	{
		InfoList = new List<AttachedSkillInfoReceiveData>();
	}

	public void Clear()
	{
		InfoList.Clear();
	}

	public AttachedSkillInfoReceiveData GetInfoFromOwner(int baseCardId, int ownerIndex, bool ownerIsPlayer)
	{
		for (int i = 0; i < InfoList.Count; i++)
		{
			if (InfoList[i].OwnerIndex == ownerIndex && InfoList[i].OwnerIsPlayer == ownerIsPlayer)
			{
				return InfoList[i];
			}
		}
		AttachedSkillInfoReceiveData attachedSkillInfoReceiveData = new AttachedSkillInfoReceiveData(baseCardId, ownerIndex, ownerIsPlayer);
		InfoList.Add(attachedSkillInfoReceiveData);
		return attachedSkillInfoReceiveData;
	}
}
