using System.Collections.Generic;

namespace Wizard;

public class AttachedSkillInfoReceiveData
{
	public class TargetInfo
	{
		public int TargetIndex { get; private set; }

		public bool TargetIsPlayer { get; private set; }

		public List<string> SkillHashList { get; private set; }

		public TargetInfo(BattleCardBase target)
		{
			TargetIndex = target.Index;
			TargetIsPlayer = target.IsPlayer;
			SkillHashList = new List<string>();
		}
	}

	public int OwnerBaseCardId { get; private set; }

	public int OwnerIndex { get; private set; }

	public bool OwnerIsPlayer { get; private set; }

	public List<TargetInfo> TargetInfoList { get; private set; }

	public AttachedSkillInfoReceiveData(int baseCardId, int ownerIndex, bool ownerIsPlayer)
	{
		OwnerBaseCardId = baseCardId;
		OwnerIndex = ownerIndex;
		OwnerIsPlayer = ownerIsPlayer;
	}

	public void AddTargetAndSkillHash(BattleCardBase target, string skillHash)
	{
		if (TargetInfoList == null)
		{
			TargetInfoList = new List<TargetInfo>();
		}
		TargetInfo targetInfo = null;
		for (int i = 0; i < TargetInfoList.Count; i++)
		{
			if (target.IsPlayer == TargetInfoList[i].TargetIsPlayer && target.Index == TargetInfoList[i].TargetIndex)
			{
				targetInfo = TargetInfoList[i];
				break;
			}
		}
		if (targetInfo == null)
		{
			targetInfo = new TargetInfo(target);
			TargetInfoList.Add(targetInfo);
		}
		targetInfo.SkillHashList.Add(skillHash);
	}
}
