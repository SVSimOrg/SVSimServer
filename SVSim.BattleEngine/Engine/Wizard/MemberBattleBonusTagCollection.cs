using System.Collections.Generic;

namespace Wizard;

public class MemberBattleBonusTagCollection : TagCollection
{
	public MemberBattleBonusTagCollection()
		: base(TagCollectionType.MemberBattleBonus)
	{
	}

	private MemberBattleBonusTagCollection(MemberBattleBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new MemberBattleBonusTagCollection(this);
	}

	public float GetMemberBattleBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn)
	{
		float num = 0f;
		if (tagOwner == null || tagOwner.IsDead || targetCard == null || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, null);
			}
		}
		return num;
	}
}
