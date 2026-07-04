using System;
using System.Collections.Generic;

namespace Wizard;

public class FanfareTagCollection : WhenPlayTagCollection
{
	public static readonly AIPlayTagType[] ManagedTagTypes = new AIPlayTagType[48]
	{
		AIPlayTagType.FanfareDamage,
		AIPlayTagType.FanfareBuff,
		AIPlayTagType.FanfareDestroy,
		AIPlayTagType.FanfareMetamorphose,
		AIPlayTagType.FanfareHandMetamorphose,
		AIPlayTagType.FanfareBounce,
		AIPlayTagType.FanfareBanish,
		AIPlayTagType.FanfareSpellboost,
		AIPlayTagType.FanfareAddCemetery,
		AIPlayTagType.FanfareTokenDraw,
		AIPlayTagType.FanfareHeal,
		AIPlayTagType.FanfareSubtractCountdown,
		AIPlayTagType.FanfareToken,
		AIPlayTagType.FanfareBanAttack,
		AIPlayTagType.FanfareCopyTag,
		AIPlayTagType.FanfareIgnoreGuard,
		AIPlayTagType.FanfareSetMaxStatus,
		AIPlayTagType.FanfareRecoverAttackableCount,
		AIPlayTagType.FanfareChangeClass,
		AIPlayTagType.FanfareChangeTribe,
		AIPlayTagType.FanfareSummonHandCard,
		AIPlayTagType.FanfareHandBuff,
		AIPlayTagType.FanfareDiscard,
		AIPlayTagType.FanfareSelect,
		AIPlayTagType.FanfareHandSelect,
		AIPlayTagType.FanfareAttackableCount,
		AIPlayTagType.FanfareRemoveSkill,
		AIPlayTagType.FanfareAttachTag,
		AIPlayTagType.FanfareModifyConsumeEp,
		AIPlayTagType.FanfareEvo,
		AIPlayTagType.FanfareShield,
		AIPlayTagType.FanfareRecoverPp,
		AIPlayTagType.FanfareDamageCut,
		AIPlayTagType.FanfareDamageClip,
		AIPlayTagType.FanfareAttachStyle,
		AIPlayTagType.FanfareBonusInSimulation,
		AIPlayTagType.FanfareReanimate,
		AIPlayTagType.FanfareRemoveGuard,
		AIPlayTagType.FanfareAddDeck,
		AIPlayTagType.FanfareSneak,
		AIPlayTagType.FanfareQuick,
		AIPlayTagType.FanfareRush,
		AIPlayTagType.FanfareGuard,
		AIPlayTagType.FanfareKiller,
		AIPlayTagType.FanfareDrain,
		AIPlayTagType.FanfareUntouchable,
		AIPlayTagType.FanfareForceTargeting,
		AIPlayTagType.FanfareNotBeAttacked
	};

	public static readonly AIPlayTagType[] TAG_FOR_REMOVAL_CHECK = new AIPlayTagType[6]
	{
		AIPlayTagType.FanfareDamage,
		AIPlayTagType.FanfareBuff,
		AIPlayTagType.FanfareDestroy,
		AIPlayTagType.FanfareBanish,
		AIPlayTagType.FanfareBounce,
		AIPlayTagType.FanfareHeal
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => ManagedTagTypes;

	public FanfareTagCollection()
		: base(TagCollectionType.Fanfare)
	{
	}

	private FanfareTagCollection(FanfareTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new FanfareTagCollection(this);
	}

	protected override bool WhenPlayCommonCheckCondition(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!owner.IsSpell && !owner.IsAccelerated(field, playPtn, situation))
		{
			return !field.IsEnableIgnoreFanfareBonus(playPtn);
		}
		return false;
	}

	protected override bool IsRemovalCheckTagType(AIPlayTagType type)
	{
		return Array.IndexOf(TAG_FOR_REMOVAL_CHECK, type) >= 0;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.FanfareTokenDraw:
			_tokenDrawList = AIParamQuery.AddElementToList(tag, _tokenDrawList);
			break;
		case AIPlayTagType.FanfareHeal:
			_healTags = AIParamQuery.AddElementToList(tag, _healTags);
			break;
		case AIPlayTagType.FanfareToken:
		case AIPlayTagType.FanfareReanimate:
			_summonTokenList = AIParamQuery.AddElementToList(tag, _summonTokenList);
			break;
		case AIPlayTagType.FanfareCopyTag:
			_copyTagTags = AIParamQuery.AddElementToList(tag, _copyTagTags);
			break;
		case AIPlayTagType.FanfareRecoverPp:
			_recoverPpTagList = AIParamQuery.AddElementToList(tag, _recoverPpTagList);
			break;
		case AIPlayTagType.FanfareRemoveGuard:
			_removeKeywordSkillTagList = AIParamQuery.AddElementToList(tag, _removeKeywordSkillTagList);
			break;
		}
	}
}
