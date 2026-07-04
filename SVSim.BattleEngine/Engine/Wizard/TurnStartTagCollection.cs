using System.Collections.Generic;

namespace Wizard;

public class TurnStartTagCollection : TagCollection
{
	private static AIPlayTagType[] _managedTatType = new AIPlayTagType[5]
	{
		AIPlayTagType.TurnStartAttachTag,
		AIPlayTagType.TurnStartSubtractCountdown,
		AIPlayTagType.TurnStartDamageCut,
		AIPlayTagType.TurnStartShield,
		AIPlayTagType.TurnStartDamage
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTatType;

	public TurnStartTagCollection()
		: base(TagCollectionType.WhenTurnStart)
	{
	}

	private TurnStartTagCollection(TurnStartTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new TurnStartTagCollection(this);
	}

	public void RegisterConditionPassedTagActions(AIVirtualCard tagOwner, AISituationInfo situation, AIScriptTokenArgType turnStartSide)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return;
		}
		List<AIPlayTag> conditionPassedList = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AITurnStartTagArgument aITurnStartTagArgument && aITurnStartTagArgument.CheckTurnStartSide(turnStartSide) && aIPlayTag.CheckCondition(tagOwner, EnemyAI.EmptyPlayPtn, tagOwner.SelfField, situation))
			{
				conditionPassedList = AIParamQuery.AddElementToList(aIPlayTag, conditionPassedList, isBlockDuplicate: true);
			}
		}
		if (conditionPassedList == null || conditionPassedList.Count <= 0)
		{
			return;
		}
		situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
		{
			for (int j = 0; j < conditionPassedList.Count; j++)
			{
				if (!tagOwner.IsDead)
				{
					conditionPassedList[j].ArgumentExpressions.Execute(tagOwner, tagOwner.SelfField, EnemyAI.EmptyPlayPtn, situation);
				}
			}
		});
	}
}
