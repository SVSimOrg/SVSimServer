using System.Collections.Generic;

namespace Wizard;

public class PreprocessTagCollection : TagCollection
{
	public static readonly AIPlayTagType[] ManagedTagTypes = new AIPlayTagType[3]
	{
		AIPlayTagType.EarthRite,
		AIPlayTagType.Necromance,
		AIPlayTagType.BurialRite
	};

	private List<AIPlayTag> _earthRiteTagList;

	private List<AIPlayTag> _necromanceTagList;

	private List<AIPlayTag> _burialRiteTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => ManagedTagTypes;

	public PreprocessTagCollection()
		: base(TagCollectionType.Preprocess)
	{
		_earthRiteTagList = null;
		_necromanceTagList = null;
		_burialRiteTagList = null;
	}

	private PreprocessTagCollection(PreprocessTagCollection tagCollection)
		: base(tagCollection)
	{
		_earthRiteTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._earthRiteTagList);
		_necromanceTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._necromanceTagList);
		_burialRiteTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._burialRiteTagList);
	}

	public override TagCollection Clone()
	{
		return new PreprocessTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.EarthRite:
			_earthRiteTagList = AIParamQuery.AddElementToList(tag, _earthRiteTagList);
			break;
		case AIPlayTagType.Necromance:
			_necromanceTagList = AIParamQuery.AddElementToList(tag, _necromanceTagList);
			break;
		case AIPlayTagType.BurialRite:
			_burialRiteTagList = AIParamQuery.AddElementToList(tag, _burialRiteTagList);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_earthRiteTagList != null)
		{
			_earthRiteTagList.Clear();
			_earthRiteTagList = null;
		}
		if (_necromanceTagList != null)
		{
			_necromanceTagList.Clear();
			_necromanceTagList = null;
		}
		if (_burialRiteTagList != null)
		{
			_burialRiteTagList.Clear();
			_burialRiteTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_earthRiteTagList, tag);
		RemoveTagFromList(_necromanceTagList, tag);
		RemoveTagFromList(_burialRiteTagList, tag);
	}

	public int GetEarthRiteCount(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType timing)
	{
		if (_earthRiteTagList == null || _earthRiteTagList.Count <= 0)
		{
			return 0;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		int num = AIStackCountUtility.GetStackCount(field, owner.IsAlly);
		int num2 = 0;
		for (int i = 0; i < _earthRiteTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _earthRiteTagList[i];
			if (!aIPlayTag.CheckCondition(owner, bestPlayPtn, field, situation))
			{
				continue;
			}
			AIEarthRite aIEarthRite = aIPlayTag.ArgumentExpressions as AIEarthRite;
			if (timing == aIEarthRite.Timing)
			{
				int earthRiteCount = aIEarthRite.GetEarthRiteCount(owner, field, bestPlayPtn, situation, num);
				num2 += earthRiteCount;
				num -= earthRiteCount;
				if (num <= 0)
				{
					break;
				}
			}
		}
		return num2;
	}

	public int GetNecromanceOrDefault(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType timing)
	{
		int result = -1;
		if (tagOwner == null || (tagOwner.IsDead && timing != AIScriptTokenArgType.WHEN_DESTROY) || (!tagOwner.IsDead && timing == AIScriptTokenArgType.WHEN_DESTROY) || _necromanceTagList == null || _necromanceTagList.Count <= 0)
		{
			return result;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		int cemeteryCount = field.VirtualCemetery.GetCemeteryCount(tagOwner.IsAlly);
		for (int i = 0; i < _necromanceTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _necromanceTagList[i];
			if (aIPlayTag.ArgumentExpressions is AINecromance aINecromance && aIPlayTag.CheckCondition(tagOwner, bestPlayPtn, field, situation) && aINecromance.CheckValidTimingType(timing) && aINecromance.Timing == timing)
			{
				result = aINecromance.GetNecromanceValue(tagOwner, cemeteryCount, field, situation, bestPlayPtn);
				break;
			}
		}
		return result;
	}

	public int GetBurialRiteCount(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, AIScriptTokenArgType timing)
	{
		if (_burialRiteTagList == null || _burialRiteTagList.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _burialRiteTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _burialRiteTagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIBurialRite aIBurialRite && aIBurialRite.Timing == timing)
			{
				num += aIBurialRite.GetBurialRiteCount(tagOwner, field, situation, playPtn);
			}
		}
		return num;
	}
}
