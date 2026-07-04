using System.Collections.Generic;

namespace Wizard;

public class EnemyBattleBonusTagCollection : TagCollection
{
	public EnemyBattleBonusTagCollection()
		: base(TagCollectionType.EnemyBattleBonus)
	{
	}

	private EnemyBattleBonusTagCollection(EnemyBattleBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new EnemyBattleBonusTagCollection(this);
	}

	public float GetEnemyBattleBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn)
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
				num += aIPlayTag.EvalArg(targetCard, playPtn, selfField, null);
			}
		}
		return num;
	}
}
