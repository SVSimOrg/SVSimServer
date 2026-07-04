using System.Collections.Generic;

namespace Wizard;

public class AIBarrierInfoCollection
{
	private int _shieldAllCount;

	private int _shieldAttackCount;

	private int _shieldSpellCount;

	private int _shieldSkillCount;

	public List<AIBarrierInfoBase> BarrierList { get; private set; }

	public bool HasBarrier
	{
		get
		{
			if (BarrierList != null)
			{
				return BarrierList.Count > 0;
			}
			return false;
		}
	}

	public int BarrierCount
	{
		get
		{
			if (BarrierList == null)
			{
				return 0;
			}
			return BarrierList.Count;
		}
	}

	public bool HasShieldAll => _shieldAllCount > 0;

	public bool HasShieldAttack => _shieldAttackCount > 0;

	public bool HasShieldSpell
	{
		get
		{
			if (_shieldSpellCount <= 0)
			{
				return HasShieldSkill;
			}
			return true;
		}
	}

	public bool HasShieldSkill => _shieldSkillCount > 0;

	public AIBarrierInfoCollection Clone()
	{
		AIBarrierInfoCollection aIBarrierInfoCollection = new AIBarrierInfoCollection();
		if (HasBarrier)
		{
			for (int i = 0; i < BarrierList.Count; i++)
			{
				aIBarrierInfoCollection.AddBarrierInfo(BarrierList[i].Clone());
			}
		}
		return aIBarrierInfoCollection;
	}

	public AIBarrierInfoCollection GetInheritanceForNewVirtualField()
	{
		AIBarrierInfoCollection aIBarrierInfoCollection = new AIBarrierInfoCollection();
		if (HasBarrier)
		{
			for (int i = 0; i < BarrierList.Count; i++)
			{
				AIBarrierInfoBase barrierInfo = BarrierList[i];
				aIBarrierInfoCollection.AddBarrierInfo(barrierInfo);
			}
		}
		return aIBarrierInfoCollection;
	}

	public void GetBarrierInfoFromPreviousField(AIBarrierInfoCollection previousBarrierCollection)
	{
		if (HasBarrier)
		{
			for (int num = BarrierList.Count - 1; num >= 0; num--)
			{
				_ = BarrierList[num];
				BarrierList.RemoveAt(num);
			}
		}
		if (previousBarrierCollection.HasBarrier)
		{
			for (int i = 0; i < previousBarrierCollection.BarrierList.Count; i++)
			{
				AddBarrierInfo(previousBarrierCollection.BarrierList[i].Clone());
			}
		}
	}

	public ulong GetHash()
	{
		ulong num = 0uL;
		if (!HasBarrier)
		{
			return num;
		}
		for (int i = 0; i < BarrierList.Count; i++)
		{
			num += BarrierList[i].Hash;
		}
		return num;
	}

	public void AddBarrierInfo(AIBarrierInfoBase barrierInfo)
	{
		BarrierList = AIParamQuery.AddElementToList(barrierInfo, BarrierList);
		IncrementHasShieldCount(barrierInfo);
	}

	public void DepriveAllBarrierOfOneTiming(AIBarrierStopTiming timing)
	{
		if (!HasBarrier)
		{
			return;
		}
		for (int num = BarrierList.Count - 1; num >= 0; num--)
		{
			AIBarrierInfoBase aIBarrierInfoBase = BarrierList[num];
			if (aIBarrierInfoBase.IsMatchingStopTiming(timing))
			{
				BarrierList.RemoveAt(num);
				DecrementHasShieldCount(aIBarrierInfoBase);
			}
		}
	}

	public void DepriveCertainBarrier(ulong depriveShieldHash, AIBarrierStopTiming timing)
	{
		if (!HasBarrier)
		{
			return;
		}
		for (int num = BarrierList.Count - 1; num >= 0; num--)
		{
			AIBarrierInfoBase aIBarrierInfoBase = BarrierList[num];
			if (aIBarrierInfoBase.Hash == depriveShieldHash && aIBarrierInfoBase.IsMatchingStopTiming(timing))
			{
				BarrierList.RemoveAt(num);
				DecrementHasShieldCount(aIBarrierInfoBase);
				break;
			}
		}
	}

	public int CalcDamageAmount(AIVirtualCard owner, int damage, bool isSkillDamage, bool isSpellDamage)
	{
		if (BarrierList == null || BarrierList.Count <= 0)
		{
			return damage;
		}
		int num = damage;
		for (int i = 0; i < BarrierList.Count; i++)
		{
			num = BarrierList[i].GetDamageAmount(owner, num, isSkillDamage, isSpellDamage);
		}
		return num;
	}

	private void IncrementHasShieldCount(AIBarrierInfoBase appendInfo)
	{
		if (appendInfo != null && appendInfo.IsShield())
		{
			AppendShieldCount(appendInfo.DamageType, 1);
		}
	}

	private void DecrementHasShieldCount(AIBarrierInfoBase removedInfo)
	{
		if (removedInfo != null && removedInfo.IsShield())
		{
			AppendShieldCount(removedInfo.DamageType, -1);
		}
	}

	private void AppendShieldCount(AIDamageType type, int appendValue)
	{
		switch (type)
		{
		case AIDamageType.All:
			_shieldAllCount += appendValue;
			break;
		case AIDamageType.Attack:
			_shieldAttackCount += appendValue;
			break;
		case AIDamageType.Spell:
			_shieldSpellCount += appendValue;
			break;
		case AIDamageType.Skill:
			_shieldSkillCount += appendValue;
			break;
		}
	}
}
