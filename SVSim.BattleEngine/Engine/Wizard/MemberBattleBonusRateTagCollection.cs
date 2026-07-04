using System.Collections.Generic;

namespace Wizard;

public class MemberBattleBonusRateTagCollection : TagCollection
{
	public MemberBattleBonusRateTagCollection()
		: base(TagCollectionType.MemberBattleBonusRate)
	{
	}

	private MemberBattleBonusRateTagCollection(MemberBattleBonusRateTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new MemberBattleBonusRateTagCollection(this);
	}

	public float GetMemberBattleBonusRate(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation)
	{
		float num = 1f;
		if (tagOwner == null || targetCard == null || tagOwner.IsDead || targetCard.IsDead || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		List<AIVirtualCard> allCandidates = (tagOwner.IsAlly ? selfField.CardListSet.EnemyClassAndInplayCards : selfField.GetMemberCardList(playPtn));
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && aIPlayTag.ArgumentExpressions is AIOtherBattleBonusRate)
			{
				AIOtherBattleBonusRate aIOtherBattleBonusRate = aIPlayTag.ArgumentExpressions as AIOtherBattleBonusRate;
				num *= aIOtherBattleBonusRate.GetBonusValue(tagOwner, targetCard, allCandidates, selfField, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}
}
