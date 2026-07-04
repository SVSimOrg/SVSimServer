using System.Collections.Generic;

namespace Wizard;

public class LastwordTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[18]
	{
		AIPlayTagType.LastwordBuff,
		AIPlayTagType.LastwordDamage,
		AIPlayTagType.LastwordHeal,
		AIPlayTagType.LastwordMetamorphose,
		AIPlayTagType.LastwordToken,
		AIPlayTagType.LastwordBanish,
		AIPlayTagType.LastwordReanimate,
		AIPlayTagType.LastwordDestroy,
		AIPlayTagType.LastwordAttachTag,
		AIPlayTagType.LastwordDraw,
		AIPlayTagType.LastwordAddDeck,
		AIPlayTagType.LastwordEvo,
		AIPlayTagType.LastwordRemoveSkill,
		AIPlayTagType.LastwordAddCemetery,
		AIPlayTagType.LastwordSetStatus,
		AIPlayTagType.LastwordDamageClip,
		AIPlayTagType.LastwordShield,
		AIPlayTagType.LastwordSubtractCountdown
	};

	private List<AIPlayTag> _lastwordAttachTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public List<AIPlayTag> LastwordTokenList { get; private set; }

	public List<AIPlayTag> LastwordMetamorphoseList { get; private set; }

	public List<AIPlayTag> LastwordAddDeckList { get; private set; }

	public LastwordTagCollection()
		: base(TagCollectionType.Lastword)
	{
		_lastwordAttachTagList = null;
	}

	private LastwordTagCollection(LastwordTagCollection param)
		: base(param)
	{
		_lastwordAttachTagList = param.CreateLastwordAttachTagListClone();
		LastwordTokenList = param.CreateLastwordTokenListClone();
		LastwordMetamorphoseList = param.CreateLastwordMetamorphoseListClone();
	}

	public override TagCollection Clone()
	{
		return new LastwordTagCollection(this);
	}

	public List<AIPlayTag> CreateLastwordAttachTagListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_lastwordAttachTagList);
	}

	public List<AIPlayTag> CreateLastwordTokenListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(LastwordTokenList);
	}

	public List<AIPlayTag> CreateLastwordMetamorphoseListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(LastwordMetamorphoseList);
	}

	public void RegisterExecutingTagActions(AIVirtualCard tagOwner, AISkillProcessInformation processInfo, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<int> playPtn = field.BestPlayPtn;
		List<int> conditionPassedIndexList = GetConditionPassedIndexList(tagOwner, field, playPtn, situation);
		if (conditionPassedIndexList != null && conditionPassedIndexList.Count > 0)
		{
			processInfo.AddExecutingAction(delegate
			{
				Execute(tagOwner, field, playPtn, conditionPassedIndexList, situation);
			});
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<int> tagIndexList, AISituationInfo situation)
	{
		if (!base.HasTag || tagIndexList == null || tagIndexList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < tagIndexList.Count; i++)
		{
			if (base.TagList.Count <= tagIndexList[i])
			{
				continue;
			}
			AIPlayTag tag = base.TagList[tagIndexList[i]];
			if (tag.Type == AIPlayTagType.LastwordDamage && !tagOwner.IsAlly && tagOwner.IsBreakLast(playPtn))
			{
				field.OnAfterLeaderAttackSimulation += delegate
				{
					tag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
				};
			}
			else
			{
				tag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}

	public List<AIPlayTag> GetLastwordAttachTagContents()
	{
		if (_lastwordAttachTagList == null)
		{
			return null;
		}
		List<AIPlayTag> list = new List<AIPlayTag>();
		for (int i = 0; i < _lastwordAttachTagList.Count; i++)
		{
			AILastwordAttachTag aILastwordAttachTag = _lastwordAttachTagList[i].ArgumentExpressions as AILastwordAttachTag;
			list.Add(aILastwordAttachTag.Tag);
		}
		return list;
	}

	public AITokenIdCollection GetLastwordTokenIds(AIVirtualCard owner, AIVirtualField field, List<int> playPtn)
	{
		if (LastwordTokenList == null || LastwordTokenList.Count <= 0)
		{
			return null;
		}
		AITokenIdCollection aITokenIdCollection = null;
		for (int i = 0; i < LastwordTokenList.Count; i++)
		{
			AIPlayTag aIPlayTag = LastwordTokenList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, null))
			{
				AITokenIdCollection bothSideTokenIds = (aIPlayTag.ArgumentExpressions as AILastwordToken).GetBothSideTokenIds(owner, playPtn, field);
				if (bothSideTokenIds != null && bothSideTokenIds.HasToken)
				{
					aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, bothSideTokenIds);
				}
			}
		}
		return aITokenIdCollection;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.LastwordAttachTag:
			_lastwordAttachTagList = AIParamQuery.AddElementToList(tag, _lastwordAttachTagList);
			break;
		case AIPlayTagType.LastwordToken:
			LastwordTokenList = AIParamQuery.AddElementToList(tag, LastwordTokenList);
			break;
		case AIPlayTagType.LastwordMetamorphose:
			LastwordMetamorphoseList = AIParamQuery.AddElementToList(tag, LastwordMetamorphoseList);
			break;
		case AIPlayTagType.LastwordAddDeck:
			LastwordAddDeckList = AIParamQuery.AddElementToList(tag, LastwordAddDeckList);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_lastwordAttachTagList != null)
		{
			_lastwordAttachTagList.Clear();
			_lastwordAttachTagList = null;
		}
		if (LastwordTokenList != null)
		{
			LastwordTokenList.Clear();
			LastwordTokenList = null;
		}
		if (LastwordMetamorphoseList != null)
		{
			LastwordMetamorphoseList.Clear();
			LastwordMetamorphoseList = null;
		}
		if (LastwordAddDeckList != null)
		{
			LastwordAddDeckList.Clear();
			LastwordAddDeckList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_lastwordAttachTagList, tag) && !RemoveTagFromList(LastwordTokenList, tag) && !RemoveTagFromList(LastwordMetamorphoseList, tag))
		{
			RemoveTagFromList(LastwordAddDeckList, tag);
		}
	}
}
