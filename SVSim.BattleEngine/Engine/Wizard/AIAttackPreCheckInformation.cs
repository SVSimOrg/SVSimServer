using System.Collections.Generic;

namespace Wizard;

public class AIAttackPreCheckInformation
{
	public float AttackBonus { get; private set; }

	public AISimulationBuffInfoCollection BuffInfo { get; private set; }

	public bool HasBuffInfo
	{
		get
		{
			if (BuffInfo != null)
			{
				return BuffInfo.Count > 0;
			}
			return false;
		}
	}

	public AIAttackPreCheckInformation(float attackBonus, AISimulationBuffInfoCollection buffInfo)
	{
		AttackBonus = attackBonus;
		BuffInfo = buffInfo;
	}

	public void ApplyBuff(ulong tagHash, AIVirtualField field, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		BuffInfo.ApplySingleBuff(tagHash, field, playPtn, situation);
	}

	public bool HasPreCheckBuffInfo(AIPlayTag tag)
	{
		if (BuffInfo == null)
		{
			return false;
		}
		return BuffInfo.HasBuffInfo(tag.Hash);
	}
}
