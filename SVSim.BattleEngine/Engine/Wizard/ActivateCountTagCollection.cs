using System.Collections.Generic;

namespace Wizard;

public class ActivateCountTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[11]
	{
		AIPlayTagType.PlayActivateCount,
		AIPlayTagType.AttackActivateCount,
		AIPlayTagType.BreakActivateCount,
		AIPlayTagType.BanishActivateCount,
		AIPlayTagType.DamagedActivateCount,
		AIPlayTagType.BuffActivateCounnt,
		AIPlayTagType.TurnEndActivateCount,
		AIPlayTagType.HealActivateCount,
		AIPlayTagType.NecromanceActivateCount,
		AIPlayTagType.EvoActivateCount,
		AIPlayTagType.SummonActivateCount
	};

	private List<ulong> _duplicateTagHashList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public List<AIActivateCounter> ActivateCounterList { get; private set; }

	private bool HasCounter
	{
		get
		{
			if (ActivateCounterList != null)
			{
				return ActivateCounterList.Count > 0;
			}
			return false;
		}
	}

	public ActivateCountTagCollection()
		: base(TagCollectionType.ActivateCount)
	{
		ActivateCounterList = null;
	}

	private ActivateCountTagCollection(ActivateCountTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection._duplicateTagHashList != null)
		{
			_duplicateTagHashList = new List<ulong>(tagCollection._duplicateTagHashList);
		}
		ActivateCounterList = tagCollection.CreateCloneCounterList();
	}

	public override TagCollection Clone()
	{
		return new ActivateCountTagCollection(this);
	}

	public List<AIActivateCounter> CreateCloneCounterList()
	{
		if (!HasCounter)
		{
			return null;
		}
		List<AIActivateCounter> list = new List<AIActivateCounter>();
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			list.Add(ActivateCounterList[i].Clone());
		}
		return list;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (_duplicateTagHashList == null)
		{
			_duplicateTagHashList = new List<ulong>();
		}
		_duplicateTagHashList.Add(CalcDuplicateTagHash(tag, _duplicateTagHashList.Count));
	}

	public override void Clear()
	{
		base.Clear();
		if (_duplicateTagHashList != null)
		{
			_duplicateTagHashList.Clear();
		}
	}

	private ulong CalcDuplicateTagHash(AIPlayTag ownerTag, int duplicateIndex)
	{
		return ownerTag.Hash + (ulong)((long)duplicateIndex * 3457L);
	}

	public void UpdateCounterList(List<AIActivateCounter> inheritanceCounterList)
	{
		if (inheritanceCounterList == null || inheritanceCounterList.Count <= 0 || !HasCounter)
		{
			return;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			AIActivateCounter aIActivateCounter = ActivateCounterList[i];
			for (int j = 0; j < inheritanceCounterList.Count; j++)
			{
				AIActivateCounter counter = inheritanceCounterList[j];
				if (aIActivateCounter.InheritFromOtherCounter(counter))
				{
					break;
				}
			}
		}
	}

	public void ResetAllCounter(bool isOwnerTurn)
	{
		if (HasCounter)
		{
			for (int i = 0; i < ActivateCounterList.Count; i++)
			{
				ActivateCounterList[i].Reset(isOwnerTurn);
			}
		}
	}

	public override void ExecuteWhenAddTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag, AISituationInfo situation)
	{
		if (!tag.CheckCondition(card, field.BestPlayPtn, field, situation))
		{
			return;
		}
		if (!HasCounter)
		{
			ActivateCounterList = new List<AIActivateCounter>();
			ActivateCounterList.Add(new AIActivateCounter(tag));
			return;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			if (ActivateCounterList[i].IsDuplicate(tag))
			{
				return;
			}
		}
		ActivateCounterList.Add(new AIActivateCounter(tag));
	}

	public override void ExecuteWhenRemoveTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag)
	{
		if (!HasCounter)
		{
			return;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			if (ActivateCounterList[i].IsDuplicate(tag))
			{
				ActivateCounterList.RemoveAt(i);
				break;
			}
		}
	}

	public int GetActivateCount(int sourceCardId, int index)
	{
		if (!HasCounter)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			int countIfMatched = ActivateCounterList[i].GetCountIfMatched(sourceCardId, index);
			num += countIfMatched;
		}
		return num;
	}

	public bool IsSkillOccurred(int sourceCardId, int index)
	{
		if (!HasCounter)
		{
			return false;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			AIActivateCounter aIActivateCounter = ActivateCounterList[i];
			if (sourceCardId == aIActivateCounter.SourceCardId && index == aIActivateCounter.CounterIndex && !aIActivateCounter.IsSkillOccurred())
			{
				return false;
			}
		}
		return true;
	}

	public void Increment(AIVirtualCard owner, AISituationInfo situation, AIPlayTagType counterType, AIVirtualCard triggerCard)
	{
		if (!HasCounter)
		{
			return;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			AIActivateCounter aIActivateCounter = ActivateCounterList[i];
			if (aIActivateCounter.CounterType == counterType)
			{
				aIActivateCounter.CheckConditionAndIncrement(owner, situation, triggerCard);
			}
		}
	}

	public void Increment(AIVirtualCard owner, AISituationInfo situation, AIPlayTagType counterType, List<AIVirtualCard> triggerCardList)
	{
		if (!HasCounter)
		{
			return;
		}
		for (int i = 0; i < ActivateCounterList.Count; i++)
		{
			AIActivateCounter aIActivateCounter = ActivateCounterList[i];
			if (aIActivateCounter.CounterType == counterType)
			{
				aIActivateCounter.CheckConditionAndIncrement(owner, situation, triggerCardList);
			}
		}
	}
}
