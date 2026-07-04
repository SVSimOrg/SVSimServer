using System.Collections.Generic;

namespace Wizard;

public class AIBuffWithTargetsInformation
{
	private ulong _tagHash;

	public List<AIVirtualCard> TargetList { get; private set; }

	public AISimulationBuffInfo BuffInfo { get; private set; }

	public AIBuffWithTargetsInformation(List<AIVirtualCard> targets, AISimulationBuffInfo buff, ulong hash)
	{
		TargetList = targets;
		BuffInfo = buff;
		_tagHash = hash;
	}

	public void PseudoApplyBuffForSimpleAttack(AIVirtualCard currentAttacker, List<AIVirtualCard> attackerList, Tuple<int, int>[] statusArray)
	{
		if (TargetList == null || TargetList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < TargetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = TargetList[i];
			if (aIVirtualCard.IsAlly == currentAttacker.IsAlly)
			{
				int num = attackerList.IndexOf(aIVirtualCard);
				statusArray[num].first += BuffInfo.TotalAttackBuff;
				statusArray[num].second += BuffInfo.TotalLifeBuff;
			}
		}
	}

	public bool VerifyArgumentAndApply(ulong hash, AIVirtualField field, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		if (hash != _tagHash)
		{
			return false;
		}
		Apply(field, playPtn, situation);
		return true;
	}

	private void Apply(AIVirtualField field, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		int totalAttackBuff = BuffInfo.TotalAttackBuff;
		int totalLifeBuff = BuffInfo.TotalLifeBuff;
		AIBuffExecutingInfo_old buffInfo = new AIBuffExecutingInfo_old
		{
			AttackValue = totalAttackBuff,
			LifeValue = totalLifeBuff,
			IsMultiplyAttack = false,
			IsMultiplyLife = false
		};
		bool isTemp = BuffInfo.TempAttackBuff > 0 || BuffInfo.TempLifeBuff > 0;
		AIBuffSimulationUtility.BuffAll_old(TargetList, field, buffInfo, isTemp, playPtn, situation);
	}

	public bool ContainsTarget(AIVirtualCard target)
	{
		if (TargetList == null || TargetList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < TargetList.Count; i++)
		{
			if (target.IsSameCard(TargetList[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHashEqual(ulong hash)
	{
		return _tagHash == hash;
	}
}
