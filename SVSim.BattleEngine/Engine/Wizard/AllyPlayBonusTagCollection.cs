using System.Collections.Generic;

namespace Wizard;

public class AllyPlayBonusTagCollection : AIUseMinTagCollection
{
	public AllyPlayBonusTagCollection()
		: base(TagCollectionType.AllyPlayBonus)
	{
	}

	private AllyPlayBonusTagCollection(AllyPlayBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new AllyPlayBonusTagCollection(this);
	}

	public float GetAllyPlayBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation, ref float currentUseMinValue)
	{
		if (tagOwner == null || tagOwner.IsDead || (!base.HasTag && !base.HasUseMinTag) || targetCard.IsDead)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		if (base.HasNonUseMinTag)
		{
			for (int i = 0; i < _nonUseMinTagList.Count; i++)
			{
				AIPlayTag aIPlayTag = _nonUseMinTagList[i];
				AIOtherPlayBonus aIOtherPlayBonus = aIPlayTag.ArgumentExpressions as AIOtherPlayBonus;
				if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && AIFilteringUtility.CheckMatchTargetFiltering(targetCard, selfField.AllyHandCards, aIOtherPlayBonus.Filters, playPtn, tagOwner, situation))
				{
					num += aIOtherPlayBonus.GetEvaluateValue(tagOwner, playPtn, situation);
				}
			}
		}
		if (base.HasUseMinTag)
		{
			for (int j = 0; j < _useMinTagList.Count; j++)
			{
				AIPlayTag aIPlayTag2 = _useMinTagList[j];
				AIOtherPlayBonus aIOtherPlayBonus2 = aIPlayTag2.ArgumentExpressions as AIOtherPlayBonus;
				if (aIPlayTag2.CheckCondition(tagOwner, playPtn, selfField, situation) && AIFilteringUtility.CheckMatchTargetFiltering(targetCard, selfField.AllyHandCards, aIOtherPlayBonus2.Filters, playPtn, tagOwner, situation))
				{
					float evaluateValue = aIOtherPlayBonus2.GetEvaluateValue(tagOwner, playPtn, situation);
					if (EnemyAI.IsLargerThan(currentUseMinValue, evaluateValue))
					{
						currentUseMinValue = evaluateValue;
					}
				}
			}
		}
		return num;
	}
}
