using System.Collections.Generic;

namespace Wizard;

public class OtherSummonTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[18]
	{
		AIPlayTagType.OtherSummonDamage,
		AIPlayTagType.OtherSummonRush,
		AIPlayTagType.OtherSummonGuard,
		AIPlayTagType.OtherSummonQuick,
		AIPlayTagType.OtherSummonKiller,
		AIPlayTagType.OtherSummonDrain,
		AIPlayTagType.OtherSummonBanish,
		AIPlayTagType.OtherSummonHeal,
		AIPlayTagType.OtherSummonAttachTag,
		AIPlayTagType.OtherSummonBuff,
		AIPlayTagType.OtherSummonEvo,
		AIPlayTagType.OtherSummonDamageCut,
		AIPlayTagType.OtherSummonDamageClip,
		AIPlayTagType.OtherSummonSubtractCountdown,
		AIPlayTagType.OtherSummonAddCemetery,
		AIPlayTagType.OtherSummonUntouchable,
		AIPlayTagType.OtherSummonDestory,
		AIPlayTagType.OtherSummonDraw
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public List<AIPlayTag> SummonAttachTagList { get; private set; }

	public bool HasSummonAttachTag
	{
		get
		{
			if (SummonAttachTagList != null)
			{
				return SummonAttachTagList.Count > 0;
			}
			return false;
		}
	}

	public OtherSummonTagCollection()
		: base(TagCollectionType.WhenOtherSummon)
	{
	}

	private OtherSummonTagCollection(OtherSummonTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasSummonAttachTag)
		{
			SummonAttachTagList = new List<AIPlayTag>(tagCollection.SummonAttachTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new OtherSummonTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.OtherSummonAttachTag)
		{
			SummonAttachTagList = AIParamQuery.AddElementToList(tag, SummonAttachTagList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (SummonAttachTagList != null)
		{
			SummonAttachTagList.Clear();
			SummonAttachTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(SummonAttachTagList, tag);
	}

	public void RegisterConditionPassedTag(AIVirtualCard tagOwner, AIVirtualCard summonCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag || tagOwner.IsDead || summonCard == null)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<int> list = new List<int>();
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, field, null))
			{
				list.Add(i);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(summonCard, AISituationTriggerInformation.TriggerType.Summon);
		for (int j = 0; j < list.Count; j++)
		{
			AIPlayTag tag = base.TagList[list[j]];
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				if (!tagOwner.IsDead)
				{
					if (tag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase)
					{
						(tag.ArgumentExpressions as AITriggerAndTargetFiltersTagBase).Execute(tagOwner, field, playPtn, summonCard, situation);
					}
					else
					{
						tag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
					}
				}
			});
		}
	}

	public float GetOtherSummonBonus(AIVirtualCard playCard, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		float result = 0f;
		if (!base.HasTag)
		{
			return result;
		}
		if (situation == null)
		{
			AIConsoleUtility.LogError("OtherSummonTagCollection.GetOtherSummonBonus() : Unexpected error !! situation is null!!");
			return result;
		}
		AISkillProcessInformation currentSkillProcessInfo = situation.CurrentSkillProcessInfo;
		AISkillProcessInformation executingSkillProcess = new AISkillProcessInformation(new AISituationTriggerInformation(playCard, AISituationTriggerInformation.TriggerType.WhenPlay));
		situation.SetExecutingSkillProcess(executingSkillProcess);
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherSummonKeywordSkill aIOtherSummonKeywordSkill && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				float quickBonus = aIOtherSummonKeywordSkill.GetQuickBonus(playCard, field, playPtn, tagOwner, situation);
				if (quickBonus > 0f)
				{
					result = quickBonus;
					break;
				}
			}
		}
		situation.SetExecutingSkillProcess(currentSkillProcessInfo);
		return result;
	}
}
