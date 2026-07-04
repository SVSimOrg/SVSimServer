using System.Collections.Generic;

namespace Wizard;

public class ChangeInplayTagCollection : TagCollection
{
	public class ChangeInplayArgumentIsActivatedInfo
	{
		public AIPlayTag OwnerTag;

		public int DuplicateTagIndex;

		public bool IsActivated;
	}

	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[13]
	{
		AIPlayTagType.ChangeInplayImmediateRemoveByBanish,
		AIPlayTagType.ChangeInplayImmediateRemoveByDestroy,
		AIPlayTagType.ChangeInplayCannotPlay,
		AIPlayTagType.ChangeInplayCannotAttack,
		AIPlayTagType.ChangeInplayAttachTag,
		AIPlayTagType.ChangeInplayImmediateShield,
		AIPlayTagType.ChangeInplayImmediateDamageCut,
		AIPlayTagType.ChangeInplayImmediateDamageClip,
		AIPlayTagType.ChangeInplayImmediateLifeLowerLimit,
		AIPlayTagType.ChangeInplayImmediateDamageModifier,
		AIPlayTagType.ChangeInplayImmediateUntouchable,
		AIPlayTagType.ChangeInplayImmediateIndestructible,
		AIPlayTagType.ChangePpTotalBuff
	};

	private List<ChangeInplayArgumentIsActivatedInfo> _turnStartTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public int AddedChangeInplayActivatedInfoIncrement { get; private set; }

	public List<ChangeInplayArgumentIsActivatedInfo> TagIsActivatedList { get; private set; }

	public bool HasTurnStartTags
	{
		get
		{
			if (_turnStartTagList != null)
			{
				return _turnStartTagList.Count > 0;
			}
			return false;
		}
	}

	public ChangeInplayTagCollection()
		: base(TagCollectionType.WhenChangeInplay)
	{
		TagIsActivatedList = null;
		_turnStartTagList = null;
	}

	public ChangeInplayTagCollection(ChangeInplayTagCollection tagCollection)
		: base(TagCollectionType.WhenChangeInplay)
	{
		AddedChangeInplayActivatedInfoIncrement = tagCollection.AddedChangeInplayActivatedInfoIncrement;
		if (tagCollection != null && tagCollection.HasTag)
		{
			base.TagList = new List<AIPlayTag>();
			TagIsActivatedList = new List<ChangeInplayArgumentIsActivatedInfo>();
			List<ChangeInplayArgumentIsActivatedInfo> tagIsActivatedList = tagCollection.TagIsActivatedList;
			for (int i = 0; i < tagIsActivatedList.Count; i++)
			{
				ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = tagIsActivatedList[i];
				AIPlayTag ownerTag = changeInplayArgumentIsActivatedInfo.OwnerTag;
				_ = ownerTag.ArgumentExpressions;
				base.TagList.Add(ownerTag);
				ChangeInplayArgumentIsActivatedInfo activatedInfo = new ChangeInplayArgumentIsActivatedInfo
				{
					OwnerTag = ownerTag,
					DuplicateTagIndex = changeInplayArgumentIsActivatedInfo.DuplicateTagIndex,
					IsActivated = changeInplayArgumentIsActivatedInfo.IsActivated
				};
				AddChangeInplayActivatedInfo(activatedInfo);
			}
		}
	}

	public override TagCollection Clone()
	{
		return new ChangeInplayTagCollection(this);
	}

	public void Execute(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = null;
		for (int i = 0; i < TagIsActivatedList.Count; i++)
		{
			ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = TagIsActivatedList[i];
			AIPlayTag ownerTag = changeInplayArgumentIsActivatedInfo.OwnerTag;
			bool flag = false;
			if (ownerTag.ArgumentExpressions is AIWhenChangeInplayTagArgument aIWhenChangeInplayTagArgument)
			{
				flag = aIWhenChangeInplayTagArgument.IsImmediate;
			}
			if (!flag && aISkillProcessInformation == null)
			{
				aISkillProcessInformation = situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined);
			}
			if (changeInplayArgumentIsActivatedInfo == null)
			{
				AIConsoleUtility.LogError("ChangeInplayTagCollection.Execute() error!! Cannot find isActivatedInfo!!!!!");
				break;
			}
			OneTagProcess(ownerTag, owner, field, playPtn, situation, aISkillProcessInformation, changeInplayArgumentIsActivatedInfo);
		}
	}

	public override void ExecuteWhenAddTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag, AISituationInfo situation)
	{
		bool flag = false;
		if (tag.ArgumentExpressions is AIWhenChangeInplayTagArgument aIWhenChangeInplayTagArgument)
		{
			flag = aIWhenChangeInplayTagArgument.IsImmediate;
		}
		ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = CreateNewChangeInplayActivatedInfoFromTag(tag);
		AddChangeInplayActivatedInfo(changeInplayArgumentIsActivatedInfo);
		if (situation != null && changeInplayArgumentIsActivatedInfo != null)
		{
			AISkillProcessInformation processInfo = (flag ? null : situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined));
			OneTagProcess(tag, card, field, field.BestPlayPtn, situation, processInfo, changeInplayArgumentIsActivatedInfo);
		}
	}

	public void ExecuteWhenTurnStart(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation)
	{
		if (situation == null || !HasTurnStartTags)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = null;
		for (int i = 0; i < _turnStartTagList.Count; i++)
		{
			ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = _turnStartTagList[i];
			AIPlayTag ownerTag = changeInplayArgumentIsActivatedInfo.OwnerTag;
			if (aISkillProcessInformation == null)
			{
				aISkillProcessInformation = situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined);
			}
			OneTagProcess(ownerTag, tagOwner, field, field.BestPlayPtn, situation, aISkillProcessInformation, changeInplayArgumentIsActivatedInfo);
		}
	}

	public override void ExecuteWhenRemoveTag(AIVirtualCard card, AIVirtualField field, AIPlayTag tag)
	{
		tag.ArgumentExpressions.ExecuteWhenRemove(card, field, tag);
	}

	public override void Clear()
	{
		base.Clear();
		if (HasTurnStartTags)
		{
			_turnStartTagList.Clear();
			_turnStartTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (TagIsActivatedList == null || TagIsActivatedList.Count <= 0)
		{
			return;
		}
		ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = null;
		for (int i = 0; i < TagIsActivatedList.Count; i++)
		{
			ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo2 = TagIsActivatedList[i];
			if (changeInplayArgumentIsActivatedInfo2.OwnerTag.Hash == tag.Hash)
			{
				changeInplayArgumentIsActivatedInfo = changeInplayArgumentIsActivatedInfo2;
				break;
			}
		}
		if (changeInplayArgumentIsActivatedInfo == null)
		{
			AIConsoleUtility.LogError($"ChangeInplayTagCollection.RemoveTagFromManagedTagList() error!! {tag.Type} is not found in TagIsActivatedList!");
			return;
		}
		TagIsActivatedList.Remove(changeInplayArgumentIsActivatedInfo);
		if (_turnStartTagList != null)
		{
			_turnStartTagList.Remove(changeInplayArgumentIsActivatedInfo);
		}
	}

	public void UpdateIsActivatedInformation(List<ChangeInplayArgumentIsActivatedInfo> sourceList, int addedChangeInplayActivatedIncrement)
	{
		AddedChangeInplayActivatedInfoIncrement = addedChangeInplayActivatedIncrement;
		if (!base.HasTag || sourceList == null || sourceList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < TagIsActivatedList.Count; i++)
		{
			ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo = TagIsActivatedList[i];
			ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo2 = null;
			for (int j = 0; j < sourceList.Count; j++)
			{
				ChangeInplayArgumentIsActivatedInfo changeInplayArgumentIsActivatedInfo3 = sourceList[j];
				if (changeInplayArgumentIsActivatedInfo.DuplicateTagIndex == changeInplayArgumentIsActivatedInfo3.DuplicateTagIndex)
				{
					changeInplayArgumentIsActivatedInfo2 = changeInplayArgumentIsActivatedInfo3;
					break;
				}
			}
			if (changeInplayArgumentIsActivatedInfo2 != null)
			{
				changeInplayArgumentIsActivatedInfo.IsActivated = changeInplayArgumentIsActivatedInfo2.IsActivated;
			}
		}
	}

	private void OneTagProcess(AIPlayTag tag, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISkillProcessInformation processInfo, ChangeInplayArgumentIsActivatedInfo isActivatedInfo)
	{
		AIWhenChangeInplayTagArgument aIWhenChangeInplayTagArgument = tag.ArgumentExpressions as AIWhenChangeInplayTagArgument;
		bool flag = tag.CheckCondition(owner, playPtn, field, situation);
		if (isActivatedInfo.IsActivated && !flag)
		{
			aIWhenChangeInplayTagArgument.Stop(owner, field, playPtn, processInfo, situation);
			isActivatedInfo.IsActivated = false;
		}
		else if (!isActivatedInfo.IsActivated && flag)
		{
			aIWhenChangeInplayTagArgument.Execute(owner, field, playPtn, processInfo, situation);
			isActivatedInfo.IsActivated = true;
		}
	}

	private ChangeInplayArgumentIsActivatedInfo CreateNewChangeInplayActivatedInfoFromTag(AIPlayTag tag)
	{
		_ = tag.ArgumentExpressions;
		return new ChangeInplayArgumentIsActivatedInfo
		{
			OwnerTag = tag,
			IsActivated = false,
			DuplicateTagIndex = AddedChangeInplayActivatedInfoIncrement++
		};
	}

	private void AddChangeInplayActivatedInfo(ChangeInplayArgumentIsActivatedInfo activatedInfo)
	{
		TagIsActivatedList = AIParamQuery.AddElementToList(activatedInfo, TagIsActivatedList);
		if (activatedInfo.OwnerTag.Type == AIPlayTagType.ChangePpTotalBuff)
		{
			_turnStartTagList = AIParamQuery.AddElementToList(activatedInfo, _turnStartTagList);
		}
	}
}
