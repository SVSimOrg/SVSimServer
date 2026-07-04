using System;

namespace Wizard;

public class PlayTagCollection : WhenPlayTagCollection
{
	public static readonly AIPlayTagType[] ManagedTagTypes = new AIPlayTagType[45]
	{
		AIPlayTagType.PlayDamage,
		AIPlayTagType.PlayBuff,
		AIPlayTagType.PlayDestroy,
		AIPlayTagType.PlayMetamorphose,
		AIPlayTagType.PlayHandMetamorphose,
		AIPlayTagType.PlayBounce,
		AIPlayTagType.PlayBanish,
		AIPlayTagType.PlaySpellboost,
		AIPlayTagType.PlayAddCemetery,
		AIPlayTagType.PlayTokenDraw,
		AIPlayTagType.PlayHeal,
		AIPlayTagType.PlaySubtractCountdown,
		AIPlayTagType.PlayToken,
		AIPlayTagType.PlayBanAttack,
		AIPlayTagType.PlayCopyTag,
		AIPlayTagType.PlayIgnoreGuard,
		AIPlayTagType.PlaySetMaxStatus,
		AIPlayTagType.PlayChangeClass,
		AIPlayTagType.PlayChangeTribe,
		AIPlayTagType.PlaySummonHandCard,
		AIPlayTagType.PlayHandBuff,
		AIPlayTagType.PlayChangeCost,
		AIPlayTagType.PlayDiscard,
		AIPlayTagType.PlaySelect,
		AIPlayTagType.PlayHandSelect,
		AIPlayTagType.PlayNotBeAttacked,
		AIPlayTagType.PlayAttackableCount,
		AIPlayTagType.PlayRemoveSkill,
		AIPlayTagType.PlayAttachTag,
		AIPlayTagType.PlayModifyConsumeEp,
		AIPlayTagType.PlayEvo,
		AIPlayTagType.PlayShield,
		AIPlayTagType.PlayRecoverPP,
		AIPlayTagType.PlayDamageCut,
		AIPlayTagType.PlayDamageClip,
		AIPlayTagType.PlayBonusInSimulation,
		AIPlayTagType.PlayReanimate,
		AIPlayTagType.PlayAddDeck,
		AIPlayTagType.PlaySetLeaderMaxLife,
		AIPlayTagType.PlaySneak,
		AIPlayTagType.PlayQuick,
		AIPlayTagType.PlayRush,
		AIPlayTagType.PlayGuard,
		AIPlayTagType.PlayKiller,
		AIPlayTagType.PlayDrain
	};

	public static readonly AIPlayTagType[] TAG_FOR_REMOVAL_CHECK = new AIPlayTagType[6]
	{
		AIPlayTagType.PlayDamage,
		AIPlayTagType.PlayBuff,
		AIPlayTagType.PlayDestroy,
		AIPlayTagType.PlayBounce,
		AIPlayTagType.PlayBanish,
		AIPlayTagType.PlayHeal
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => ManagedTagTypes;

	public PlayTagCollection()
		: base(TagCollectionType.Play)
	{
	}

	private PlayTagCollection(PlayTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayTagCollection(this);
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
		case AIPlayTagType.PlayTokenDraw:
			_tokenDrawList = AIParamQuery.AddElementToList(tag, _tokenDrawList);
			break;
		case AIPlayTagType.PlayHeal:
			_healTags = AIParamQuery.AddElementToList(tag, _healTags);
			break;
		case AIPlayTagType.PlayToken:
		case AIPlayTagType.PlayReanimate:
			_summonTokenList = AIParamQuery.AddElementToList(tag, _summonTokenList);
			break;
		case AIPlayTagType.PlayCopyTag:
			_copyTagTags = AIParamQuery.AddElementToList(tag, _copyTagTags);
			break;
		case AIPlayTagType.PlayRecoverPP:
			_recoverPpTagList = AIParamQuery.AddElementToList(tag, _recoverPpTagList);
			break;
		}
	}
}
