using System.Collections.Generic;

namespace Wizard;

public class AfterClashTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[2]
	{
		AIPlayTagType.AfterClashHeal,
		AIPlayTagType.AfterClashDamage
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public AfterClashTagCollection()
		: base(TagCollectionType.WhenAfterClash)
	{
	}

	private AfterClashTagCollection(AfterClashTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new AfterClashTagCollection(this);
	}

	public void RegisterConditionPassedTags(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("AfterClashTagCollection.RegisterConditionPassedTags() error!! situation is null");
		}
		else
		{
			if (!base.HasTag || situation.ActionType != AIOperationType.ATTACK || tagOwner.IsDead)
			{
				return;
			}
			List<AIPlayTag> conditionPassedTagList = null;
			List<int> playPtn = field.BestPlayPtn;
			for (int i = 0; i < base.TagList.Count; i++)
			{
				AIPlayTag aIPlayTag = base.TagList[i];
				if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
				{
					conditionPassedTagList = AIParamQuery.AddElementToList(aIPlayTag, conditionPassedTagList);
				}
			}
			if (conditionPassedTagList == null || conditionPassedTagList.Count <= 0)
			{
				return;
			}
			situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				for (int j = 0; j < conditionPassedTagList.Count; j++)
				{
					if (tagOwner.IsDead)
					{
						break;
					}
					if (field.AllyClass.Life <= 0)
					{
						break;
					}
					if (field.EnemyClass.Life <= 0)
					{
						break;
					}
					conditionPassedTagList[j].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
				}
			});
		}
	}
}
