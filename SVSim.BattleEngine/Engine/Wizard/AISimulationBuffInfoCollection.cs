using System.Collections.Generic;

namespace Wizard;

public class AISimulationBuffInfoCollection
{
	public List<AIBuffWithTargetsInformation> BuffInfoList { get; private set; }

	public int Count
	{
		get
		{
			if (BuffInfoList != null)
			{
				return BuffInfoList.Count;
			}
			return 0;
		}
	}

	public AISimulationBuffInfoCollection()
	{
		BuffInfoList = null;
	}

	public void Add(ulong tagHash, List<AIVirtualCard> target, AISimulationBuffInfo buffInfo)
	{
		AIBuffWithTargetsInformation element = new AIBuffWithTargetsInformation(target, buffInfo, tagHash);
		BuffInfoList = AIParamQuery.AddElementToList(element, BuffInfoList);
	}

	public void PseudoApplyBuffForSimpleAttack(AIVirtualCard currentAttacker, List<AIVirtualCard> attackerList, Tuple<int, int>[] statusArray)
	{
		if (BuffInfoList != null && BuffInfoList.Count > 0)
		{
			for (int i = 0; i < BuffInfoList.Count; i++)
			{
				BuffInfoList[i].PseudoApplyBuffForSimpleAttack(currentAttacker, attackerList, statusArray);
			}
		}
	}

	public void ApplySingleBuff(ulong tagHash, AIVirtualField field, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		if (BuffInfoList != null && BuffInfoList.Count > 0)
		{
			for (int i = 0; i < BuffInfoList.Count && !BuffInfoList[i].VerifyArgumentAndApply(tagHash, field, playPtn, situation); i++)
			{
			}
		}
	}

	public AISimulationBuffInfo GetBuffInfoToCertainCard(AIVirtualCard card)
	{
		AISimulationBuffInfo result = null;
		if (BuffInfoList != null && BuffInfoList.Count > 0)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < BuffInfoList.Count; i++)
			{
				AIBuffWithTargetsInformation aIBuffWithTargetsInformation = BuffInfoList[i];
				if (aIBuffWithTargetsInformation.ContainsTarget(card))
				{
					num += aIBuffWithTargetsInformation.BuffInfo.TempAttackBuff;
					num2 += aIBuffWithTargetsInformation.BuffInfo.TempLifeBuff;
					num3 += aIBuffWithTargetsInformation.BuffInfo.TotalAttackBuff;
					num4 += aIBuffWithTargetsInformation.BuffInfo.TotalLifeBuff;
				}
			}
			if (num != 0 || num2 != 0 || num3 != 0 || num4 != 0)
			{
				result = new AISimulationBuffInfo(num, num2, num3, num4);
			}
		}
		return result;
	}

	public bool HasBuffInfo(ulong hash)
	{
		if (BuffInfoList == null || BuffInfoList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < BuffInfoList.Count; i++)
		{
			if (BuffInfoList[i].IsHashEqual(hash))
			{
				return true;
			}
		}
		return false;
	}
}
