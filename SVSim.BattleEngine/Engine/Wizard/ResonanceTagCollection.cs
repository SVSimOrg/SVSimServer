using System.Collections.Generic;

namespace Wizard;

public class ResonanceTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[3]
	{
		AIPlayTagType.ResonanceDamage,
		AIPlayTagType.ResonanceHeal,
		AIPlayTagType.ResonanceKiller
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public ResonanceTagCollection()
		: base(TagCollectionType.WhenResonance)
	{
	}

	private ResonanceTagCollection(ResonanceTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new ResonanceTagCollection(this);
	}

	public void RegisterPassedConditionTags(bool leaderIsAlly, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags, isBlockDuplicate: true);
			}
		}
		if (passedConditionTags != null)
		{
			AIVirtualCard triggerCard = (leaderIsAlly ? field.AllyClass : field.EnemyClass);
			situation.RegisterNewProcessInfo(triggerCard, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, owner, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.IsDead)
		{
			for (int i = 0; i < tags.Count; i++)
			{
				tags[i].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}
}
