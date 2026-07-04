using System;
using System.Collections.Generic;

namespace Wizard;

public class AITagCollectionContainer
{
	protected List<AIPlayTagType> _holdingTagTypes;

	protected List<TagCollectionType> _holdingTagCollectionTypes;

	private static ulong[] PRIME_NUMBERS_FOR_HASH_CALCULATION = new ulong[20]
	{
		23uL, 479uL, 811uL, 1447uL, 3541uL, 383uL, 281uL, 733uL, 71uL, 1201uL,
		2293uL, 991uL, 59uL, 1931uL, 251uL, 673uL, 1613uL, 12491uL, 5uL, 2699uL
	};

	private readonly AIPlayTagType[] DISABLED_TAG_TYPES_INPLAY = new AIPlayTagType[96]
	{
		AIPlayTagType.HandPlus,
		AIPlayTagType.CostBonus,
		AIPlayTagType.Priority,
		AIPlayTagType.PlayLimit,
		AIPlayTagType.PlayPlus,
		AIPlayTagType.PlayBonus,
		AIPlayTagType.FanfareBonus,
		AIPlayTagType.EmoteOnPlay,
		AIPlayTagType.MulliganKeep,
		AIPlayTagType.MulliganChange,
		AIPlayTagType.FanfareToken,
		AIPlayTagType.PlayToken,
		AIPlayTagType.AddCardToPlayoutPlayPtn,
		AIPlayTagType.PlayoutNextTurn,
		AIPlayTagType.PlayDraw,
		AIPlayTagType.PlayReanimate,
		AIPlayTagType.Fusion,
		AIPlayTagType.HandBonus,
		AIPlayTagType.FusionBonus,
		AIPlayTagType.FusionDraw,
		AIPlayTagType.CondChoice,
		AIPlayTagType.PlayDestroy,
		AIPlayTagType.PlayDamage,
		AIPlayTagType.PlayHeal,
		AIPlayTagType.PlayBounce,
		AIPlayTagType.PlayBanish,
		AIPlayTagType.FanfareBanish,
		AIPlayTagType.FanfareBounce,
		AIPlayTagType.FanfareDamage,
		AIPlayTagType.PlayBuff,
		AIPlayTagType.FanfareBuff,
		AIPlayTagType.PlaySetMaxStatus,
		AIPlayTagType.FanfareSetMaxStatus,
		AIPlayTagType.PlaySetLeaderMaxLife,
		AIPlayTagType.FanfareDestroy,
		AIPlayTagType.FanfareSpellboost,
		AIPlayTagType.PlaySpellboost,
		AIPlayTagType.FanfareAddCemetery,
		AIPlayTagType.PlayAddCemetery,
		AIPlayTagType.FanfareHeal,
		AIPlayTagType.PlaySubtractCountdown,
		AIPlayTagType.FanfareSubtractCountdown,
		AIPlayTagType.PlayBanAttack,
		AIPlayTagType.FanfareBanAttack,
		AIPlayTagType.PlayIgnoreGuard,
		AIPlayTagType.FanfareIgnoreGuard,
		AIPlayTagType.PlayMetamorphose,
		AIPlayTagType.FanfareMetamorphose,
		AIPlayTagType.PlayHandMetamorphose,
		AIPlayTagType.FanfareHandMetamorphose,
		AIPlayTagType.FanfareRecoverAttackableCount,
		AIPlayTagType.FanfareSneak,
		AIPlayTagType.PlaySneak,
		AIPlayTagType.FanfareQuick,
		AIPlayTagType.PlayQuick,
		AIPlayTagType.FanfareRush,
		AIPlayTagType.PlayRush,
		AIPlayTagType.FanfareGuard,
		AIPlayTagType.PlayGuard,
		AIPlayTagType.FanfareKiller,
		AIPlayTagType.PlayKiller,
		AIPlayTagType.FanfareDrain,
		AIPlayTagType.PlayDrain,
		AIPlayTagType.FanfareChangeClass,
		AIPlayTagType.PlayChangeClass,
		AIPlayTagType.FanfareChangeTribe,
		AIPlayTagType.PlayChangeTribe,
		AIPlayTagType.FanfareUntouchable,
		AIPlayTagType.PlayUntouchable,
		AIPlayTagType.FanfareSummonHandCard,
		AIPlayTagType.PlaySummonHandCard,
		AIPlayTagType.FanfareHandBuff,
		AIPlayTagType.PlayHandBuff,
		AIPlayTagType.FanfareForceTargeting,
		AIPlayTagType.PlayChangeCost,
		AIPlayTagType.FanfareChangeCost,
		AIPlayTagType.PlaySelect,
		AIPlayTagType.FanfareSelect,
		AIPlayTagType.PlayHandSelect,
		AIPlayTagType.FanfareHandSelect,
		AIPlayTagType.PlayNotBeAttacked,
		AIPlayTagType.FanfareNotBeAttacked,
		AIPlayTagType.PlayAttackableCount,
		AIPlayTagType.FanfareAttackableCount,
		AIPlayTagType.PlayRemoveSkill,
		AIPlayTagType.FanfareRemoveSkill,
		AIPlayTagType.PlayModifyConsumeEp,
		AIPlayTagType.FanfareModifyConsumeEp,
		AIPlayTagType.PlayEvo,
		AIPlayTagType.FanfareEvo,
		AIPlayTagType.PlayBonusInSimulation,
		AIPlayTagType.FanfareBonusInSimulation,
		AIPlayTagType.FanfareRemoveGuard,
		AIPlayTagType.FusionMetamorphose,
		AIPlayTagType.PlayAddDeck,
		AIPlayTagType.FanfareAddDeck
	};

	public List<TagCollectionWithTypeBase> TagDictionary { get; protected set; }

	public AIAttachedTagCollection AttachedTags { get; private set; }

	public AIRemovedTagCollection RemovedTagCollection { get; private set; }

	public List<int> ReferringOtherInplayIds { get; private set; }

	public int Count
	{
		get
		{
			if (TagDictionary != null)
			{
				return TagDictionary.Count;
			}
			return 0;
		}
	}

	public PuppetAttackTagCollection PuppetAttackTags => GetTagCollection<PuppetAttackTagCollection>(TagCollectionType.PuppetAttack);

	public DamagedTagCollection DamagedTags => GetTagCollection<DamagedTagCollection>(TagCollectionType.WhenDamaged);

	public OtherDamagedTagCollection OtherDamagedTags => GetTagCollection<OtherDamagedTagCollection>(TagCollectionType.WhenOtherDamaged);

	public AttackTagCollection AttackTags => GetTagCollection<AttackTagCollection>(TagCollectionType.WhenAttack);

	public OtherAttackTagCollection OtherAttackTags => GetTagCollection<OtherAttackTagCollection>(TagCollectionType.WhenOtherAttack);

	public BreakTagCollection BreakTags => GetTagCollection<BreakTagCollection>(TagCollectionType.WhenBreak);

	public AfterAttackTagCollection AfterAttackTags => GetTagCollection<AfterAttackTagCollection>(TagCollectionType.WhenAfterAttack);

	public SummonTagCollection SummonTags => GetTagCollection<SummonTagCollection>(TagCollectionType.WhenSummon);

	public OtherSummonTagCollection OtherSummonTags => GetTagCollection<OtherSummonTagCollection>(TagCollectionType.WhenOtherSummon);

	public FanfareTagCollection FanfareTags => GetTagCollection<FanfareTagCollection>(TagCollectionType.Fanfare);

	public PlayTagCollection PlayTags => GetTagCollection<PlayTagCollection>(TagCollectionType.Play);

	public LastwordTagCollection LastwordTags => GetTagCollection<LastwordTagCollection>(TagCollectionType.Lastword);

	public LeaveTagCollection LeaveTags => GetTagCollection<LeaveTagCollection>(TagCollectionType.WhenLeave);

	public OtherLeaveTagCollection OtherLeaveTags => GetTagCollection<OtherLeaveTagCollection>(TagCollectionType.WhenOtherLeave);

	public BounceTagCollection BounceTags => GetTagCollection<BounceTagCollection>(TagCollectionType.WhenBounce);

	public TurnStartTagCollection TurnStartTags => GetTagCollection<TurnStartTagCollection>(TagCollectionType.WhenTurnStart);

	public TurnEndTagCollection TurnEndTags => GetTagCollection<TurnEndTagCollection>(TagCollectionType.WhenTurnEnd);

	public AfterClashTagCollection AfterClashTags => GetTagCollection<AfterClashTagCollection>(TagCollectionType.WhenAfterClash);

	public EvoTagCollection EvoTags => GetTagCollection<EvoTagCollection>(TagCollectionType.WhenEvo);

	public OtherEvoTagCollection OtherEvoTags => GetTagCollection<OtherEvoTagCollection>(TagCollectionType.WhenOtherEvo);

	public SelfAndOtherEvoTagCollection SelfAndOtherEvoTags => GetTagCollection<SelfAndOtherEvoTagCollection>(TagCollectionType.WhenSelfAndOtherEvo);

	public HealTagCollection HealTags => GetTagCollection<HealTagCollection>(TagCollectionType.WhenHeal);

	public DiscardedTagCollection DiscardedTags => GetTagCollection<DiscardedTagCollection>(TagCollectionType.WhenDiscarded);

	public AfterDiscardTagCollection AfterDiscardTags => GetTagCollection<AfterDiscardTagCollection>(TagCollectionType.WhenAfterDiscard);

	public BuffTriggerTagCollection BuffTriggerTags => GetTagCollection<BuffTriggerTagCollection>(TagCollectionType.WhenBuff);

	public BanishTagCollection BanishTags => GetTagCollection<BanishTagCollection>(TagCollectionType.WhenBanish);

	public OtherBanishTagCollection OtherBanishTags => GetTagCollection<OtherBanishTagCollection>(TagCollectionType.WhenOtherBanish);

	public GetOnTriggerTagCollection GetOnTriggerTags => GetTagCollection<GetOnTriggerTagCollection>(TagCollectionType.WhenGetOn);

	public WhenGetOffTagCollection WhenGetOffTags => GetTagCollection<WhenGetOffTagCollection>(TagCollectionType.WhenGetOff);

	public ActivateCountTagCollection ActivateCountTags => GetTagCollection<ActivateCountTagCollection>(TagCollectionType.ActivateCount);

	public ClashBonusTagCollection ClashBonusTags => GetTagCollection<ClashBonusTagCollection>(TagCollectionType.ClashBonus);

	public ReincarnationSimulationTagCollection ReincarnationSimulationTags => GetTagCollection<ReincarnationSimulationTagCollection>(TagCollectionType.Reincarnation);

	public CantBeAttackedTagCollection CantBeAttackedTags => GetTagCollection<CantBeAttackedTagCollection>(TagCollectionType.CantBeAttacked);

	public ChoiceTagCollection ChoiceTags => GetTagCollection<ChoiceTagCollection>(TagCollectionType.Choice);

	public ChangeInplayTagCollection ChangeInplayTags => GetTagCollection<ChangeInplayTagCollection>(TagCollectionType.WhenChangeInplay);

	public EvolveToOtherTagCollection EvolveToOtherTags => GetTagCollection<EvolveToOtherTagCollection>(TagCollectionType.EvolveToOther);

	public OtherPlayTagCollection OtherPlayTags => GetTagCollection<OtherPlayTagCollection>(TagCollectionType.WhenOtherPlay);

	public BuffBonusTagCollection BuffBonusTags => GetTagCollection<BuffBonusTagCollection>(TagCollectionType.BuffBonus);

	public EvolvedResidentTagCollection EvolvedResidentTags => GetTagCollection<EvolvedResidentTagCollection>(TagCollectionType.EvolvedResident);

	public GenerateTagCollection GenerateTags => GetTagCollection<GenerateTagCollection>(TagCollectionType.GenerateTag);

	public BounceBonusTagCollection BounceBonusTags => GetTagCollection<BounceBonusTagCollection>(TagCollectionType.BounceBonus);

	public PriorityTagCollection PriorityTags => GetTagCollection<PriorityTagCollection>(TagCollectionType.Priority);

	public PlayBonusTagCollection PlayBonusTags => GetTagCollection<PlayBonusTagCollection>(TagCollectionType.PlayBonus);

	public PlayBonusRateTagCollection PlayBonusRateTags => GetTagCollection<PlayBonusRateTagCollection>(TagCollectionType.PlayBonusRate);

	public FanfareBonusTagCollection FanfareBonusTags => GetTagCollection<FanfareBonusTagCollection>(TagCollectionType.FanfareBonus);

	public IgnoreFanfareBonusTagCollection IgnoreFanfareBonusTags => GetTagCollection<IgnoreFanfareBonusTagCollection>(TagCollectionType.IgnoreFanfareBonus);

	public HandPlusTagCollection HandPlusTags => GetTagCollection<HandPlusTagCollection>(TagCollectionType.HandPlus);

	public AllyPlayBonusTagCollection AllyPlayBonusTags => GetTagCollection<AllyPlayBonusTagCollection>(TagCollectionType.AllyPlayBonus);

	public EnemyPlayBonusTagCollection EnemyPlayBonusTags => GetTagCollection<EnemyPlayBonusTagCollection>(TagCollectionType.EnemyPlayBonus);

	public CostBonusTagCollection CostBonusTags => GetTagCollection<CostBonusTagCollection>(TagCollectionType.CostBonus);

	public PlayDrawTagCollection PlayDrawTags => GetTagCollection<PlayDrawTagCollection>(TagCollectionType.PlayDraw);

	public PlayLimitTagCollection PlayLimitTags => GetTagCollection<PlayLimitTagCollection>(TagCollectionType.PlayLimit);

	public PlayPtnBonusTagCollection PlayptnBonusTags => GetTagCollection<PlayPtnBonusTagCollection>(TagCollectionType.PlayptnBonus);

	public AttackBonusTagCollection AttackBonusTags => GetTagCollection<AttackBonusTagCollection>(TagCollectionType.AttackBonus);

	public BattleBonusTagCollection BattleBonusTags => GetTagCollection<BattleBonusTagCollection>(TagCollectionType.BattleBonus);

	public MemberBattleBonusTagCollection MemberBattleBonusTags => GetTagCollection<MemberBattleBonusTagCollection>(TagCollectionType.MemberBattleBonus);

	public EnemyBattleBonusTagCollection EnemyBattleBonusTags => GetTagCollection<EnemyBattleBonusTagCollection>(TagCollectionType.EnemyBattleBonus);

	public BattleBonusRateTagCollection BattleBonusRateTags => GetTagCollection<BattleBonusRateTagCollection>(TagCollectionType.BattleBonusRate);

	public MemberBattleBonusRateTagCollection MemberBattleBonusRateTags => GetTagCollection<MemberBattleBonusRateTagCollection>(TagCollectionType.MemberBattleBonusRate);

	public EnemyBattleBonusRateTagCollection EnemyBattleBonusRateTags => GetTagCollection<EnemyBattleBonusRateTagCollection>(TagCollectionType.EnemyBattleBonusRate);

	public EvoBonusTagCollection EvoBonusTags => GetTagCollection<EvoBonusTagCollection>(TagCollectionType.EvoBonus);

	public MemberEvoBonusTagCollection MemberEvoBonusTags => GetTagCollection<MemberEvoBonusTagCollection>(TagCollectionType.MemberEvoBonus);

	public EnemyEvoBonusTagCollection EnemyEvoBonusTags => GetTagCollection<EnemyEvoBonusTagCollection>(TagCollectionType.EnemyEvoBonus);

	public IgnoreBreakTagCollection IgnoreBreakTags => GetTagCollection<IgnoreBreakTagCollection>(TagCollectionType.IgnoreBreak);

	public BreakBonusTagCollection BreakBonusTags => GetTagCollection<BreakBonusTagCollection>(TagCollectionType.BreakBonus);

	public OtherBreakBonusTagCollection OtherBreakBonusTags => GetTagCollection<OtherBreakBonusTagCollection>(TagCollectionType.OtherBreakBonus);

	public BanishBonusTagCollection BanishBonusTags => GetTagCollection<BanishBonusTagCollection>(TagCollectionType.BanishBonus);

	public OtherBanishBonusTagCollection OtherBanishBonusTags => GetTagCollection<OtherBanishBonusTagCollection>(TagCollectionType.OtherBanishBonus);

	public LeaveBonusTagCollection LeaveBonusTags => GetTagCollection<LeaveBonusTagCollection>(TagCollectionType.LeaveBonus);

	public OtherLeaveBonusTagCollection OtherLeaveBonusTags => GetTagCollection<OtherLeaveBonusTagCollection>(TagCollectionType.OtherLeaveBonus);

	public DiscardedBonusTagCollection DiscardedBonusTags => GetTagCollection<DiscardedBonusTagCollection>(TagCollectionType.DiscardedBonus);

	public PreprocessTagCollection PreprocessTags => GetTagCollection<PreprocessTagCollection>(TagCollectionType.Preprocess);

	public ReanimateBonusTagCollection ReanimateBonusTags => GetTagCollection<ReanimateBonusTagCollection>(TagCollectionType.ReanimateBonus);

	public ReanimateEvoTagCollection ReanimateEvoTags => GetTagCollection<ReanimateEvoTagCollection>(TagCollectionType.ReanimateEvo);

	public BreakFirstTagCollection BreakFirstTags => GetTagCollection<BreakFirstTagCollection>(TagCollectionType.BreakFirst);

	public BreakLastTagCollection BreakLastTags => GetTagCollection<BreakLastTagCollection>(TagCollectionType.BreakLast);

	public BreakBeforePlayTagCollection BreakBeforePlayTags => GetTagCollection<BreakBeforePlayTagCollection>(TagCollectionType.BreakBeforePlay);

	public FusionTagCollection FusionTags => GetTagCollection<FusionTagCollection>(TagCollectionType.Fusion);

	public FusionMetamorphoseTagCollection FusionMetamorphoseTags => GetTagCollection<FusionMetamorphoseTagCollection>(TagCollectionType.FusionMetamorphose);

	public FirstEvoTagCollection FirstEvoTags => GetTagCollection<FirstEvoTagCollection>(TagCollectionType.FirstEvo);

	public PlayPlusTagCollection PlayPlusTags => GetTagCollection<PlayPlusTagCollection>(TagCollectionType.PlayPlus);

	public PlayoutBonusTagCollection PlayoutBonusTags => GetTagCollection<PlayoutBonusTagCollection>(TagCollectionType.PlayoutBonus);

	public OtherPlayoutBonusTagCollection OtherPlayoutBonusTags => GetTagCollection<OtherPlayoutBonusTagCollection>(TagCollectionType.OtherPlayoutBonus);

	public HandBonusTagCollection HandBonusTags => GetTagCollection<HandBonusTagCollection>(TagCollectionType.HandBonus);

	public FusionBonusTagCollection FusionBonusTags => GetTagCollection<FusionBonusTagCollection>(TagCollectionType.FusionBonus);

	public FusionDrawTagCollection FusionDrawTags => GetTagCollection<FusionDrawTagCollection>(TagCollectionType.FusionDraw);

	public CondChoiceTagCollection CondChoiceTags => GetTagCollection<CondChoiceTagCollection>(TagCollectionType.CondChoice);

	public AddCardToPlayoutPlayPtnTagCollection AddCardToPlayoutPlayPtnTags => GetTagCollection<AddCardToPlayoutPlayPtnTagCollection>(TagCollectionType.AddCardToPlayoutPlayPtn);

	public NoInstantAttackTagCollection NoInstantAttackTags => GetTagCollection<NoInstantAttackTagCollection>(TagCollectionType.NoInstantAttack);

	public EmoteTagCollection EmoteTags => GetTagCollection<EmoteTagCollection>(TagCollectionType.Emote);

	public EvoHandPlusTagCollection EvoHandPlusTags => GetTagCollection<EvoHandPlusTagCollection>(TagCollectionType.EvoHandPlus);

	public NoNormalEvoTagCollection NoNormalEvoTags => GetTagCollection<NoNormalEvoTagCollection>(TagCollectionType.NoNormalEvo);

	public PlagueCityTagCollection PlagueCityTags => GetTagCollection<PlagueCityTagCollection>(TagCollectionType.PlagueCity);

	public RemoveByDestroyTagCollection RemoveByDestroyTags => GetTagCollection<RemoveByDestroyTagCollection>(TagCollectionType.RemoveByDestroy);

	public SetAITribeTagCollection SetAITribeTags => GetTagCollection<SetAITribeTagCollection>(TagCollectionType.SetAITribe);

	public PlaySkipTagCollection PlaySkipTags => GetTagCollection<PlaySkipTagCollection>(TagCollectionType.PlaySkip);

	public GiveSkillTagCollection GiveSkillTags => GetTagCollection<GiveSkillTagCollection>(TagCollectionType.GiveSkill);

	public ForceImmediateAttackTagCollection ForceImmediateAttackTags => GetTagCollection<ForceImmediateAttackTagCollection>(TagCollectionType.ForceImmediateAttack);

	public RemoveSkillTagCollection RemoveSkillTags => GetTagCollection<RemoveSkillTagCollection>(TagCollectionType.RemoveSkill);

	public OneMoreLastwordTagCollection OneMoreLastwordTags => GetTagCollection<OneMoreLastwordTagCollection>(TagCollectionType.OneMoreLastword);

	public NoSkipAttackTagCollection NoSkipAttackTags => GetTagCollection<NoSkipAttackTagCollection>(TagCollectionType.NoSkipAttack);

	public ResonanceTagCollection ResonanceTags => GetTagCollection<ResonanceTagCollection>(TagCollectionType.WhenResonance);

	public AttackByLifeTagCollection AttackByLifeTags => GetTagCollection<AttackByLifeTagCollection>(TagCollectionType.AttackByLife);

	public FixedCostTagCollection FixedCostTags => GetTagCollection<FixedCostTagCollection>(TagCollectionType.FixedCost);

	public ForceBerserkTagCollection ForceBerserkTags => GetTagCollection<ForceBerserkTagCollection>(TagCollectionType.ForceBerserk);

	public GetOnTagCollection GetOnTags => GetTagCollection<GetOnTagCollection>(TagCollectionType.GetOn);

	public ModifyHealTagCollection ModifyHealTags => GetTagCollection<ModifyHealTagCollection>(TagCollectionType.ModifyHeal);

	public PlayptnBaseStatsRateTagCollection PlayptnBaseStatsRateTags => GetTagCollection<PlayptnBaseStatsRateTagCollection>(TagCollectionType.PlayptnBaseStatsRate);

	public RallyCountPlusTagCollection RallyCountPlusTags => GetTagCollection<RallyCountPlusTagCollection>(TagCollectionType.RallyCountPlus);

	public AttackableClassTagCollection AttackableClassTags => GetTagCollection<AttackableClassTagCollection>(TagCollectionType.AttackableClass);

	public WhenNecromanceTagCollection NecromanceTags => GetTagCollection<WhenNecromanceTagCollection>(TagCollectionType.WhenNecromance);

	public TargetTagCollection TargetTags => GetTagCollection<TargetTagCollection>(TagCollectionType.Target);

	public IgnoreTargetTagCollection IgnoreTargetTags => GetTagCollection<IgnoreTargetTagCollection>(TagCollectionType.IgnoreTarget);

	public PlayoutNextTurnTagCollection PlayoutNextTurnTags => GetTagCollection<PlayoutNextTurnTagCollection>(TagCollectionType.PlayoutNextTurn);

	private T GetTagCollection<T>(TagCollectionType type) where T : TagCollection
	{
		for (int i = 0; i < Count; i++)
		{
			TagCollection collection = TagDictionary[i].Collection;
			if (collection.Type == type)
			{
				return collection as T;
			}
		}
		return null;
	}

	public AITagCollectionContainer()
	{
		_holdingTagTypes = null;
		_holdingTagCollectionTypes = null;
		TagDictionary = null;
		AttachedTags = new AIAttachedTagCollection();
		RemovedTagCollection = new AIRemovedTagCollection();
	}

	public AITagCollectionContainer(AITagCollectionContainer container, AIVirtualCard owner)
	{
		if (container.Count <= 0)
		{
			_holdingTagTypes = null;
			_holdingTagCollectionTypes = null;
			TagDictionary = null;
			AttachedTags = new AIAttachedTagCollection();
			RemovedTagCollection = new AIRemovedTagCollection();
			return;
		}
		_holdingTagTypes = new List<AIPlayTagType>();
		_holdingTagCollectionTypes = new List<TagCollectionType>();
		TagDictionary = new List<TagCollectionWithTypeBase>();
		for (int i = 0; i < container.Count; i++)
		{
			TagCollectionWithTypeBase tagCollectionWithTypeBase = container.TagDictionary[i];
			TagDictionary.Add(tagCollectionWithTypeBase.Clone());
			tagCollectionWithTypeBase.RegisterTypes(_holdingTagTypes);
			_holdingTagCollectionTypes.Add(tagCollectionWithTypeBase.Collection.Type);
		}
		AttachedTags = container.AttachedTags.Clone();
		RemovedTagCollection = container.RemovedTagCollection.Clone();
		CreateFixedUseCostListWhenInit(owner, owner.SelfField);
	}

	public virtual void InitTags(AIVirtualCard owner, AIParamQuery query)
	{
		if (owner.IsCountdownAmulet)
		{
			AIPlayTag tag = AIPlayTagInitializingUtility.CreateTurnStartSubtractCountdownTagForCountdownAmulet();
			AddTag(tag, owner, null);
		}
		int tagCount = query.GetTagCount(owner);
		for (int i = 0; i < tagCount; i++)
		{
			AIPlayTag tag2 = query.GetTag(owner, i);
			AddTag(tag2, owner, null);
		}
		CreateFixedUseCostListWhenInit(owner, owner.SelfField);
	}

	protected void CreateFixedUseCostListWhenInit(AIVirtualCard owner, AIVirtualField field)
	{
		if (HasTagCollection(TagCollectionType.FixedCost))
		{
			FixedCostTags.CreateFixedUseCostLists(owner, field);
		}
	}

	public TagCollectionWithTypeBase AddTag(AIPlayTag tag, AIVirtualCard owner, AISituationInfo situation)
	{
		if (_holdingTagTypes == null)
		{
			_holdingTagTypes = new List<AIPlayTagType>();
		}
		if (TagDictionary == null)
		{
			TagDictionary = new List<TagCollectionWithTypeBase>();
		}
		TagCollectionWithTypeBase ownerTagCollectionWithTypes = GetOwnerTagCollectionWithTypes(tag.Type);
		if (ownerTagCollectionWithTypes == null)
		{
			return null;
		}
		ownerTagCollectionWithTypes.AddTag(tag);
		if (!_holdingTagTypes.Contains(tag.Type))
		{
			_holdingTagTypes.Add(tag.Type);
		}
		UpdateReferringOtherInplayIds(tag);
		ownerTagCollectionWithTypes?.Collection.ExecuteWhenAddTag(owner, owner.SelfField, tag, situation);
		return ownerTagCollectionWithTypes;
	}

	public void RemoveOneTagWithUpdatingFieldCardList(AIVirtualCard owner, AIPlayTag removingTag, AIVirtualField field)
	{
		if (_holdingTagTypes == null || !_holdingTagTypes.Contains(removingTag.Type))
		{
			return;
		}
		TagCollectionWithTypeBase ownerTagCollectionWithTypes = GetOwnerTagCollectionWithTypes(removingTag.Type);
		if (ownerTagCollectionWithTypes.RemoveOneTag(owner, field, removingTag))
		{
			_holdingTagTypes.Remove(removingTag.Type);
			if (ownerTagCollectionWithTypes is TagCollectionWithSingleType || ownerTagCollectionWithTypes.IsEmpty())
			{
				TagDictionary.Remove(ownerTagCollectionWithTypes);
				_holdingTagCollectionTypes.Remove(ownerTagCollectionWithTypes.Collection.Type);
			}
		}
	}

	public void RemoveAllTagWithUpdatingFieldCardList(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation)
	{
		if (TagDictionary == null || Count <= 0)
		{
			if (AttachedTags != null)
			{
				AttachedTags.RemoveAllAttachedTagInformation(situation);
			}
			return;
		}
		for (int num = Count - 1; num >= 0; num--)
		{
			TagCollectionWithTypeBase tagCollectionWithTypeBase = TagDictionary[num];
			if (!tagCollectionWithTypeBase.IsUnderManagement(AIPlayTagType.PuppetAttack))
			{
				tagCollectionWithTypeBase.RemoveTypes(_holdingTagTypes);
				_holdingTagCollectionTypes.Remove(tagCollectionWithTypeBase.Collection.Type);
				tagCollectionWithTypeBase.RemoveAllTags(owner, field);
				TagDictionary.Remove(tagCollectionWithTypeBase);
			}
		}
		AttachedTags.RemoveAllAttachedTagInformation(situation);
	}

	public void AttachTag(AIAttachedTagInformation info, AIVirtualCard owner, AISituationInfo situation)
	{
		AttachedTags.AddAttachedTagInformation(info);
		AddTag(info.Tag, owner, situation);
		owner.SelfField.CardListSet.TagClassificationWhenAttachTag(owner, info.Tag);
	}

	public void CreateChoiceBraveTag(AIVirtualCard leader)
	{
		if (!leader.IsLeader || leader.IsAlly)
		{
			AIConsoleUtility.LogError("AITagCollectionContainer.CreateChoiceBraveTag() error!! Card is not leader!!!!!");
			return;
		}
		AIPlayTag aIPlayTag = new AIPlayTag();
		AIPlayTagAsset asset = new AIPlayTagAsset
		{
			Type = "choiceBrave",
			Arg = "",
			Condition = ""
		};
		aIPlayTag.InitFromTextAsset(asset);
		AIAttachedTagInformation info = new AIAttachedTagInformation(aIPlayTag, AIScriptTokenArgType.NONE, leader, leader);
		AttachTag(info, leader, null);
	}

	private TagCollectionWithTypeBase GetOwnerTagCollectionWithTypes(AIPlayTagType type)
	{
		for (int i = 0; i < Count; i++)
		{
			TagCollectionWithTypeBase tagCollectionWithTypeBase = TagDictionary[i];
			if (tagCollectionWithTypeBase.IsUnderManagement(type))
			{
				return tagCollectionWithTypeBase;
			}
		}
		TagCollectionWithTypeBase tagCollectionWithTypeBase2 = TagCollectionWithTypeCreator.Create(type);
		if (tagCollectionWithTypeBase2 != null)
		{
			TagDictionary.Add(tagCollectionWithTypeBase2);
			_holdingTagCollectionTypes = AIParamQuery.AddElementToList(tagCollectionWithTypeBase2.Collection.Type, _holdingTagCollectionTypes, isBlockDuplicate: true);
		}
		return tagCollectionWithTypeBase2;
	}

	public bool HasWhenPlayDestroyPlayPtnTags()
	{
		if (_holdingTagTypes == null || _holdingTagTypes.Count <= 0)
		{
			return false;
		}
		if (!_holdingTagTypes.Contains(AIPlayTagType.PlaySummonHandCard))
		{
			return _holdingTagTypes.Contains(AIPlayTagType.FanfareSummonHandCard);
		}
		return true;
	}

	public bool HasTag(AIPlayTagType type)
	{
		if (_holdingTagTypes != null)
		{
			return _holdingTagTypes.Contains(type);
		}
		return false;
	}

	public bool HasTagCollection(TagCollectionType type)
	{
		if (_holdingTagCollectionTypes != null)
		{
			return _holdingTagCollectionTypes.Contains(type);
		}
		return false;
	}

	public bool HasAnyTag(AIPlayTagType[] typeArray)
	{
		if (_holdingTagTypes == null || _holdingTagTypes.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < _holdingTagTypes.Count; i++)
		{
			AIPlayTagType value = _holdingTagTypes[i];
			if (Array.IndexOf(typeArray, value) >= 0)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateReferringOtherInplayIds(AIPlayTag tag)
	{
		if (Array.IndexOf(DISABLED_TAG_TYPES_INPLAY, tag.Type) >= 0)
		{
			return;
		}
		List<int> referringOtherInplayIds = tag.ArgumentExpressions.GetReferringOtherInplayIds();
		List<int> referringIds = tag.ConditionExpressions.ReferringIds;
		if (referringOtherInplayIds != null)
		{
			if (ReferringOtherInplayIds == null)
			{
				ReferringOtherInplayIds = new List<int>();
			}
			ReferringOtherInplayIds.AddRange(referringOtherInplayIds);
		}
		if (referringIds != null)
		{
			if (ReferringOtherInplayIds == null)
			{
				ReferringOtherInplayIds = new List<int>();
			}
			ReferringOtherInplayIds.AddRange(referringIds);
		}
	}

	public ulong GetHash(AIVirtualCard card)
	{
		ulong num = 0uL;
		int num2 = 0;
		for (int i = 0; i < Count; i++)
		{
			TagCollection collection = TagDictionary[i].Collection;
			for (int j = 0; j < collection.TagList.Count; j++)
			{
				AIPlayTag aIPlayTag = collection.TagList[j];
				if (!card.IsOnField || Array.IndexOf(DISABLED_TAG_TYPES_INPLAY, aIPlayTag.Type) < 0)
				{
					num += collection.TagList[j].Hash * PRIME_NUMBERS_FOR_HASH_CALCULATION[num2];
					num2 = ((num2 < PRIME_NUMBERS_FOR_HASH_CALCULATION.Length - 1) ? (num2 + 1) : 0);
				}
			}
		}
		return num;
	}

	public AITokenIdCollection GetAllRegisterTokenPoolInfo(AIVirtualCard owner)
	{
		if (TagDictionary == null || TagDictionary.Count <= 0)
		{
			return null;
		}
		AITokenIdCollection aITokenIdCollection = null;
		for (int i = 0; i < TagDictionary.Count; i++)
		{
			TagCollection collection = TagDictionary[i].Collection;
			if (collection != null && collection.HasTag)
			{
				for (int j = 0; j < collection.TagList.Count; j++)
				{
					AITokenIdCollection allRegisterTokenPoolInfo = collection.TagList[j].ArgumentExpressions.GetAllRegisterTokenPoolInfo(owner);
					aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, allRegisterTokenPoolInfo);
				}
			}
		}
		return aITokenIdCollection;
	}
}
