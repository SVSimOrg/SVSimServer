using System.Collections.Generic;

namespace Wizard;

public class EnemyBattleBonusRateTagCollection : TagCollection
{
	public EnemyBattleBonusRateTagCollection()
		: base(TagCollectionType.EnemyBattleBonusRate)
	{
	}

	private EnemyBattleBonusRateTagCollection(EnemyBattleBonusRateTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new EnemyBattleBonusRateTagCollection(this);
	}

	public float GetEnemyBattleBonusRate(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation)
	{
		float num = 1f;
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag || targetCard.IsDead)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> allCandidates = (tagOwner.IsAlly ? selfField.CardListSet.EnemyClassAndInplayCards : selfField.GetMemberCardList(playPtn));
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherBattleBonusRate && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				AIOtherBattleBonusRate aIOtherBattleBonusRate = aIPlayTag.ArgumentExpressions as AIOtherBattleBonusRate;
				num *= aIOtherBattleBonusRate.GetBonusValue(tagOwner, targetCard, allCandidates, selfField, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}
}
