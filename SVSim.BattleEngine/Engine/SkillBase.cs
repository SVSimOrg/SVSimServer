using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cute;
using LitJson;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.Touch;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class SkillBase
{
	public class BuffInfoContainer
	{
		public BattleCardBase _targetCard;

		public BuffInfo _buffInfo;

		public SkillBase _attachSkill;

		public long _duplicateBanNum;

		public int _intValue;

		public string _stringValue;

		public NotConsumeEpModifierInfo _notConsumeEpModifierInfo;

		public ICardCostModifier CostModifier;

		public ICardOffenseModifier OffenseModifier;

		public ICardLifeModifier LifeModifier;

		public BuffInfoContainer(BattleCardBase targetCard, BuffInfo buffInfo, int intValue = -1, string stringValue = "", SkillBase attachSkill = null, long duplicateBanNum = 0L, NotConsumeEpModifierInfo notConsumeEpModifierInfo = null, ICardCostModifier costModifire = null, ICardOffenseModifier offenseModifire = null, ICardLifeModifier lifeModifire = null)
		{
			_targetCard = targetCard;
			_buffInfo = buffInfo;
			_attachSkill = attachSkill;
			_duplicateBanNum = duplicateBanNum;
			_intValue = intValue;
			_stringValue = stringValue;
			_notConsumeEpModifierInfo = notConsumeEpModifierInfo;
			CostModifier = costModifire;
			OffenseModifier = offenseModifire;
			LifeModifier = lifeModifire;
		}

		public BuffInfoContainer Clone()
		{
			return (BuffInfoContainer)MemberwiseClone();
		}
	}

	public class ExecutionBelongInfo
	{
		public bool IsDeck;

		public bool IsHand;

		public bool IsField;

		public bool IsDestroy;

		public bool IsFusionIngredient;

		public bool IsSpell;

		public ExecutionBelongInfo()
		{
			IsDeck = false;
			IsHand = false;
			IsField = false;
			IsDestroy = false;
			IsSpell = false;
		}

		public void SetBelong(BattlePlayerBase player, BattleCardBase card)
		{
			if (player.DeckCardList.Any((BattleCardBase c) => c == card))
			{
				IsDeck = true;
			}
			else if (player.HandCardList.Any((BattleCardBase c) => c == card))
			{
				IsHand = true;
			}
			else if (player.ClassAndInPlayCardList.Any((BattleCardBase c) => c == card))
			{
				IsField = true;
			}
			else if (player.CemeteryList.Any((BattleCardBase c) => c == card) || player.BanishList.Any((BattleCardBase c) => c == card) || player.NecromanceZoneList.Any((BattleCardBase c) => c == card))
			{
				IsDestroy = true;
			}
			else if (player.FusionIngredientList.Any((BattleCardBase c) => c == card))
			{
				IsFusionIngredient = true;
			}
			if (card.IsSpell)
			{
				IsSpell = true;
			}
		}

		public bool CheckBelong(BattlePlayerBase player, BattleCardBase card, SkillConditionCheckerOption checkerOption, bool isOnWhenReturn = false)
		{
			if (IsSpell && card.IsSpell && card == checkerOption.PlayedCard)
			{
				return true;
			}
			if (IsDeck && player.DeckCardList.Any((BattleCardBase c) => c == card))
			{
				return true;
			}
			if (IsHand && player.HandCardList.Any((BattleCardBase c) => c == card))
			{
				return true;
			}
			if (IsField && player.ClassAndInPlayCardList.Any((BattleCardBase c) => c == card))
			{
				return true;
			}
			if (IsDestroy && (player.CemeteryList.Any((BattleCardBase c) => c == card) || player.BanishList.Any((BattleCardBase c) => c == card) || player.NecromanceZoneList.Any((BattleCardBase c) => c == card)))
			{
				return true;
			}
			if (IsFusionIngredient && player.FusionIngredientList.Any((BattleCardBase c) => c == card))
			{
				return true;
			}
			if (isOnWhenReturn && IsField && (player.CemeteryList.Any((BattleCardBase c) => c == card) || player.HandCardList.Any((BattleCardBase c) => c == card)))
			{
				return true;
			}
			return false;
		}
	}

	public class SkillResultInfo
	{
		public List<IReadOnlyBattleCardInfo> drawCards;

		public List<IReadOnlyBattleCardInfo> drewOverHandLimitCards;

		public List<BattleCardBase> AddLastTargetCards;

		public List<List<BattleCardBase>> SelfLastTargetCards;

		public List<List<BattleCardBase>> OpponentLastTargetCards;

		public List<IReadOnlyBattleCardInfo> UpdatedDeckCards;

		public SkillResultInfo()
		{
			drawCards = new List<IReadOnlyBattleCardInfo>();
			drewOverHandLimitCards = new List<IReadOnlyBattleCardInfo>();
			AddLastTargetCards = new List<BattleCardBase>();
			SelfLastTargetCards = new List<List<BattleCardBase>>();
			OpponentLastTargetCards = new List<List<BattleCardBase>>();
			UpdatedDeckCards = new List<IReadOnlyBattleCardInfo>();
		}
	}

	public class CallParameter
	{
		public IEnumerable<BattleCardBase> targetCards;

		public SkillProcessor skillProcessor;

		public SkillResultInfo calledSkillResultInfo;
	}

	public class WaitEffectLoadVfx : SequentialVfxPlayer
	{
		public VfxBase LoadVfx { get; private set; }

		public string EffectPath { get; private set; }

		public WaitEffectLoadVfx(string effectPath, EffectMgr.EngineType engineType, string sePath, IBattleResourceMgr resourceMgr, Action<EffectBattle> loadEndCallback)
		{
			// Pre-Phase-5b: guarded on the mgr's IsRecovery. Headless has no visible EffectMgr;
			// preserve the pre-cull default (register the LoadVfx) — resourceMgr is a null-shim
			// in headless, so the register call is a no-op.
			EffectPath = effectPath;
			LoadVfx = resourceMgr.LoadAndCreateEffectBattleInstance(effectPath, engineType, sePath, delegate(EffectBattle eb)
			{
				loadEndCallback.Call(eb);
			});
			Register(LoadVfx);
		}
	}

	protected List<BuffInfoContainer> buffInfoContainer;

	public bool IsNotAssignPublishedActiveSkillCount;

	private string _inductionSkillVoice;

	private string _inductionEvolutionSkillVoice;

	public string SkillTimingText;

	public uint OnWhenPlayStart;

	public uint OnWhenHandToNotPlayStart;

	public uint OnWhenDestroyStart;

	public uint OnWhenDestroyOtherStart;

	public uint OnWhenReturnStart;

	public uint OnWhenReturnOtherStart;

	public uint OnWhenReturnSkillActivateStart;

	public uint OnWhenNecromance;

	public uint OnWhenUseWhiteRitualStack;

	public uint OnBeforeAttackStart;

	public uint OnBeforeAttackSelfAndOtherStart;

	public uint OnAfterAttackStart;

	public uint OnAfterAttackSelfAndOtherStart;

	public uint OnSelfTurnStartStart;

	public uint OnWhenTurnStartStartImmediate;

	public uint OnSelfTurnEndStart;

	public uint OnOpponentTurnStartStart;

	public uint OnOpponentTurnEndStart;

	public uint OnDisCardStart;

	public uint OnDisCardOtherStart;

	public uint OnWhenUseEpSelfAndOtherStart;

	public uint OnWhenEvolveStart;

	public uint OnWhenEvolveOtherStart;

	public uint OnWhenEvolveSelfAndOtherStart;

	public uint OnWhenEvolveBeforeStart;

	public uint OnWhenPlayOtherStart;

	public uint OnWhenSummonStart;

	public uint OnWhenSummonOtherStart;

	public uint OnWhenSummonSelfAndOtherStart;

	public uint OnWhenSpellChargeStart;

	public uint OnInHandStart;

	public uint OnInHandStopStart;

	public uint OnWhenHealingSelfAndOtherStart;

	public uint OnWhenHealOtherStart;

	public uint OnWhenDamageStart;

	public uint OnWhenDamageSelfAndOtherStart;

	public uint OnWhenFightStart;

	public uint OnWhenBuffStart;

	public uint OnWhenAddToDeckStart;

	public uint OnWhenEnhanceStart;

	public uint OnWhenAccelerateStart;

	public uint OnWhenCrystallizeStart;

	public uint OnWhenBurialRiteOther;

	public uint OnWhenChoicePlayStart;

	public uint OnWhenChoiceEvolveStart;

	public uint OnWhenChoiceBrave;

	public uint OnWhenBanish;

	public uint OnWhenBanishOther;

	public uint OnWhenResonanceStart;

	public uint OnWhenAccelerate;

	public uint OnWhenCrystallize;

	public uint OnWhenLeave;

	public uint OnWhenDraw;

	public uint OnWhenDrawOtherStart;

	public uint OnWhenPpHealStart;

	public uint OnWhenFusion;

	public uint OnWhenFusioned;

	public uint OnWhenFusionMetamorphose;

	public uint OnAfterFightStart;

	public uint OnWhenChantCountChangeStart;

	public uint OnWhenChantCountGain;

	public uint OnWhenChantCountGainSelfAndOther;

	public uint OnWhenFusionOtherStart;

	public uint OnWhenGetOnStart;

	public uint OnWhenGetOff;

	public uint OnWhenLeaveOther;

	public uint OnWhenAttachAbility;

	public uint OnWhenBuffSelfAndOther;

	public uint OnWhenDebuffSelfAndOther;

	public uint OnWhenDebuffIncludeSetMaxLife;

	public uint OnWhenHealing;

	public uint OnWhenShortageDeck;

	public uint OnWhenShortageDeckWinSkillActivate;

	public uint OnWhenSpecialLose;

	public uint OnWhenChangeInPlayImmediate;

	public uint OnWhenChangeInPlay;

	public uint OnWhenChangeInplaySelfhand;

	public uint OnWhenChangeClassLifeInplay;

	public uint OnWhenChangeClassLifeSelfhand;

	public uint OnWhenChangePPTotal;

	public uint OnWhenDrawPriority;

	public uint OnWhenAddToHand;

	public uint OnSelfTurnStartPriority;

	public uint OnWhenBattleStart;

	protected bool _isResidentSkillStartFlag;

	private List<BattleCardBase> _targetCards;

	public Func<Func<VfxBase>, VfxBase> CreateInductionSkillActivationVfxFunc;

	protected bool? _isMakeFoil = false;

	private readonly string[] _previousGetSideLogKeyword = new string[4]
	{
		SkillFilterCreator.ContentKeyword.fixed_generic_value.ToString(),
		SkillFilterCreator.ContentKeyword.activated_random_array.ToString(),
		SkillFilterCreator.ContentKeyword.skill_summoned_card_list.ToString(),
		SkillFilterCreator.ContentKeyword.distinct_base_card_and_random_index.ToString()
	};

	protected bool IsContainSelfFilter
	{
		get
		{
			if (ApplyAndFilter.Count > 0)
			{
				return ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetSelfFilter || f.TargetFilter is SkillTargetHandSelfFilter);
			}
			if (!(ApplyingTargetFilter is SkillTargetSelfFilter))
			{
				return ApplyingTargetFilter is SkillTargetHandSelfFilter;
			}
			return true;
		}
	}

	protected virtual bool IsBattleLog => !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle;

	public bool IsHaveLastTarget
	{
		get
		{
			if (ApplyingTargetFilter is NetworkSkillTargetLastTargetFilter)
			{
				return true;
			}
			if (ApplyAndFilter.Count > 0)
			{
				for (int i = 0; i < ApplyAndFilter.Count; i++)
				{
					if (ApplyAndFilter[i].TargetFilter is NetworkSkillTargetLastTargetFilter)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public virtual bool IsAllowDestroyTarget => false;

	public bool IsInvoked { get; protected set; }

	public ExecutionInfoCreatorBase _executionInfoCreator { get; protected set; }

	public ExecutionBelongInfo _executionBelongInfo { get; protected set; }

	public int PublishedActiveSkillCount { get; protected set; }

	public string Option { get; private set; }

	public int CallCount => OptionValue.ParseInt(CallCountTextValue);

	public string CallCountTextValue { get; set; }

	public string CallCountText { get; protected set; }

	public SkillParameter SkillPrm { get; private set; }

	public SkillOptionValue OptionValue { get; set; }

	public bool IsUserSelectType
	{
		get
		{
			if (!(ApplySelectFilter is SkillUserSelectFilter))
			{
				return this is Skill_select;
			}
			return true;
		}
	}

	public bool IsEmptyHandedUserSelectType
	{
		get
		{
			if (ApplySelectFilter is SkillUserSelectFilter)
			{
				return ((SkillUserSelectFilter)ApplySelectFilter).IsEmptyHanded;
			}
			return false;
		}
	}

	public bool IsBurialRite => PreprocessList.Any((SkillPreprocessBase s) => s is SkillPreprocessBurialRite);

	public bool IsRandomUntilDrawSkill
	{
		get
		{
			if (this is Skill_draw)
			{
				return ApplySelectFilter is SkillRandomSelectUntilFilter;
			}
			return false;
		}
	}

	public bool IsInductionIcon { get; set; }

	public ConditionSkillFilterCollection ConditionFilterCollection { get; private set; }

	public List<ISkillConditionChecker> ConditionCheckerList
	{
		get
		{
			return ConditionFilterCollection.ConditionCheckerFilterList;
		}
		set
		{
			ConditionFilterCollection.ConditionCheckerFilterList = value;
		}
	}

	public ISkillTargetFilter ConditionTargetFilter
	{
		get
		{
			return ConditionFilterCollection.TargetFilter;
		}
		set
		{
			ConditionFilterCollection.TargetFilter = value;
		}
	}

	public ApplySkillTargetFilterCollection ApplyFilterCollection { get; private set; }

	public ISkillBattlePlayerFilter ApplyBattlePlayerFilter
	{
		get
		{
			return ApplyFilterCollection.BattlePlayerFilter;
		}
		set
		{
			ApplyFilterCollection.BattlePlayerFilter = value;
		}
	}

	public ISkillTargetFilter ApplyingTargetFilter
	{
		get
		{
			return ApplyFilterCollection.TargetFilter;
		}
		set
		{
			ApplyFilterCollection.TargetFilter = value;
		}
	}

	public List<ISkillCardFilter> ApplyCardFilterList => ApplyFilterCollection.CardFilterList;

	public List<ISkillCustomSelectFilter> ApplyCustomSelectFilterList => ApplyFilterCollection.ApplyCustomSelectFilterList;

	public List<ISkillExclutionFilter> ApplyExclutionFilterList => ApplyFilterCollection.ApplyExclutionFilterList;

	public ISkillSelectFilter ApplySelectFilter
	{
		get
		{
			return ApplyFilterCollection.ApplySelectFilter;
		}
		set
		{
			ApplyFilterCollection.ApplySelectFilter = value;
		}
	}

	public List<ApplySkillTargetFilterCollection> ApplyAndFilter => ApplyFilterCollection.ApplyAndFilter;

	public Dictionary<BattleCardBase, List<int>> ApplyAndFilterIndexes { get; private set; } = new Dictionary<BattleCardBase, List<int>>();

	public virtual bool IsChoiceType => false;

	public bool IsAttachedSkill { get; private set; }

	public bool IsAttachedInplaySkill { get; private set; }

	public int IndividualId { get; private set; } = -1;

	public bool HasIndividualId => IndividualId != -1;

	public bool IsDuplicateBanSelfSkill { get; private set; }

	public string DuplicateBanSkillNum { get; private set; } = string.Empty;

	public List<SkillPreprocessBase> PreprocessList { get; set; }

	public IEnumerable<IReadOnlyBattleCardInfo> SkillDrewCards { get; private set; }

	public bool IsAllResidentTiming
	{
		get
		{
			if (!IsResidentTiming)
			{
				return IsHandResident;
			}
			return true;
		}
	}

	public bool IsResidentTiming
	{
		get
		{
			if (OnWhenChangeInPlayImmediate == 0 && OnWhenChangeInPlay == 0 && OnWhenChangeClassLifeInplay == 0 && OnWhenChangePPTotal == 0 && OnWhenDrawPriority == 0 && OnWhenAddToHand == 0 && OnSelfTurnStartPriority == 0)
			{
				return OnWhenTurnStartStartImmediate != 0;
			}
			return true;
		}
	}

	public bool IsHandResident
	{
		get
		{
			if (OnWhenChangeInplaySelfhand == 0)
			{
				return OnWhenChangeClassLifeSelfhand != 0;
			}
			return true;
		}
	}

	public bool IsResidentSkillStartFlag => _isResidentSkillStartFlag;

	public virtual bool ShowSideLog
	{
		get
		{
			if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdmin && !SkillPrm.ownerCard.IsPlayer && SkillPrm.ownerCard.IsInHand && !PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard) && OnWhenReturnStart == 0 && OnWhenLeave == 0)
			{
				return OnWhenGetOff != 0;
			}
			return true;
		}
	}

	public virtual bool IsShowSideLogSkillType => true;

	public virtual bool IsTargetIndicate => true;

	public virtual bool IsVisibleTarget { get; protected set; }

	public bool IsWhenPlaySkill => OnWhenPlayStart != 0;

	public bool IsWhenDestroySkill => OnWhenDestroyStart != 0;

	public bool IsWhenEvolveSkill => OnWhenEvolveStart != 0;

	public bool IsBeforAttackSkill => OnBeforeAttackStart != 0;

	public bool IsBeforeAttackSelfAndOtherSkill => OnBeforeAttackSelfAndOtherStart != 0;

	public bool IsAfterAttackSelfAndOtherSkill => OnAfterAttackSelfAndOtherStart != 0;

	public bool IsWhenFightSkill => OnWhenFightStart != 0;

	public virtual bool IsInductionSkill => IsInductionIcon;

	public SkillBase GetAttachSkill
	{
		get
		{
			ISkillApplyInformation skillApplyInformation = SkillPrm.ownerCard.SkillApplyInformation;
			if (skillApplyInformation != null && skillApplyInformation.AttachedSkillsInfo != null)
			{
				for (int i = 0; i < skillApplyInformation.AttachedSkillsInfo.AttachedSkills.Count(); i++)
				{
					if (skillApplyInformation.AttachedSkillsInfo.AttachedSkills.ToList()[i] == this)
					{
						return skillApplyInformation.AttachedSkillsInfo.CreatorSkillList.ToList()[i];
					}
				}
			}
			return null;
		}
	}

	public bool IsActivity { get; protected set; }

	public bool IsReferencePreviousSkill { get; protected set; }

	public bool Used { get; protected set; }

	public bool UsedRandom { get; protected set; }

	public bool IsScanConditionOk { get; protected set; }

	public bool IsOnceCallTiming { get; protected set; }

	protected virtual bool IsMakeFoil
	{
		get
		{
			if (GetAttachSkill != null)
			{
				return GetAttachSkill.SkillPrm.ownerCard.BaseParameter.IsFoil;
			}
			if (SkillPrm.ownerCard is ClassBattleCardBase)
			{
				if (_isMakeFoil.HasValue)
				{
					return _isMakeFoil.Value;
				}
				return false;
			}
			return SkillPrm.ownerCard.BaseParameter.IsFoil;
		}
	}

	public bool IsDeckSelfSkill
	{
		get
		{
			if (!(ConditionTargetFilter is SkillTargetDeckSelfFilter))
			{
				return ApplyingTargetFilter is SkillTargetDeckSelfFilter;
			}
			return true;
		}
	}

	public bool IsTargetChoiceSelectSkill => ApplyingTargetFilter is SkillTargetChosenCardsFilter;

	public event Action<SkillBase> OnBeforeProcess;

	public event Action<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption> OnSkillStart;

	public event Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> OnSkillEnd;

	public event Action<SkillBase, IEnumerable<BattleCardBase>> OnCalcApplyTargets;

	public event Action<SkillBase, List<BattleCardBase>, SkillProcessor> OnSkillStopStart;

	public event Func<SkillBase, List<BattleCardBase>, SkillConditionCheckerOption, SkillProcessor, VfxBase> OnSkillStopEnd;

	public event Action<SkillBase> OnInactiveSkill;

	public void CloneBuffInfoContainer(List<BattleCardBase> selfCloneCards, List<BattleCardBase> oppCloneCards, SkillBase sourceSkill)
	{
		if (sourceSkill == null || sourceSkill.buffInfoContainer == null)
		{
			return;
		}
		if (this.buffInfoContainer == null)
		{
			this.buffInfoContainer = new List<BuffInfoContainer>();
		}
		for (int i = 0; i < sourceSkill.buffInfoContainer.Count; i++)
		{
			BuffInfoContainer buffInfoContainer = sourceSkill.buffInfoContainer[i].Clone();
			BattleCardBase battleCardBase = selfCloneCards.FindFromCardId(buffInfoContainer._targetCard);
			if (battleCardBase == null)
			{
				battleCardBase = oppCloneCards.FindFromCardId(buffInfoContainer._targetCard);
			}
			buffInfoContainer._targetCard = battleCardBase;
			this.buffInfoContainer.Add(buffInfoContainer);
		}
	}

	public bool IsAddLogIfResident(BattleCardBase target, bool isBuffInfo = false)
	{
		if (isBuffInfo && target.IsClass && target == SkillPrm.ownerCard)
		{
			if (!(this is Skill_shield))
			{
				return this is Skill_damage_cut;
			}
			return true;
		}
		if (!IsAllResidentTiming)
		{
			return true;
		}
		if (target != SkillPrm.ownerCard)
		{
			return true;
		}
		return false;
	}

	public BuffInfo AddBuffInfoIfNeeded(BattleCardBase target, BattleCardBase previousOwner = null)
	{
		if (!IsAddLogIfResident(target, isBuffInfo: true))
		{
			return null;
		}
		CardParameter cardParameter = ((previousOwner != null) ? previousOwner.BaseParameter : SkillPrm.ownerCard.BaseParameter);
		BuffInfo buffInfo = new BuffInfo(cardParameter.BaseCardId, cardParameter.NormalCardId, this);
		target.AddBuffInfo(buffInfo);
		return buffInfo;
	}

	public BuffInfo InsertBuffInfoIfNeeded(BattleCardBase target, int index, bool isBaseCardId = false)
	{
		if (!IsAddLogIfResident(target, isBuffInfo: true))
		{
			return null;
		}
		CardParameter baseParameter = SkillPrm.ownerCard.BaseParameter;
		BuffInfo buffInfo = new BuffInfo(isBaseCardId ? baseParameter.BaseCardId : baseParameter.CardId, baseParameter.NormalCardId, this);
		target.InsertBuffInfo(buffInfo, index);
		return buffInfo;
	}

	public virtual void AddIndividualIdSkillBuffLog(Skill_attach_skill attachSkill, BattleCardBase target)
	{
	}

	public void UpdateClassBuffIfActive(BattleCardBase target)
	{
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			return;
		}
		IDetailPanelControl detailPanelControl = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.DetailMgr.DetailPanelControl;
		if (detailPanelControl.IsShow)
		{
			SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr();
			List<BattlePlayerBase.MyRotationBonusCondition> list = new List<BattlePlayerBase.MyRotationBonusCondition>();
			list.AddRange(target.SelfBattlePlayer.BonusConditionList);
			if (detailPanelControl._card != null && detailPanelControl._card.IsClass && detailPanelControl._card.IsPlayer == target.IsPlayer)
			{
				detailPanelControl.UpdateBuffInfo(target, list);
			}
		}
	}

	public void AddBuffInfo(BuffInfoContainer buff)
	{
		buffInfoContainer.Add(buff);
	}

	public bool IsContainCardInBuffInfo(BattleCardBase card)
	{
		return buffInfoContainer.Any((BuffInfoContainer b) => b._targetCard == card);
	}

	public void ReplaceBuffInfoTargetCard(BattleCardBase oldCard, BattleCardBase newCard)
	{
		IEnumerable<BuffInfoContainer> enumerable = buffInfoContainer.Where((BuffInfoContainer b) => b._targetCard == oldCard);
		if (!enumerable.Any())
		{
			return;
		}
		foreach (BuffInfoContainer item in enumerable)
		{
			item._targetCard = newCard;
		}
	}

	public void ReplaceBuffInfoSkill(BattleCardBase targetCard, SkillBase skill)
	{
		List<BuffInfoContainer> list = buffInfoContainer.Where((BuffInfoContainer b) => b._targetCard == targetCard).ToList();
		for (int num = 0; num < list.Count; num++)
		{
			list[num]._attachSkill = skill;
		}
	}

	public BuffInfoContainer PopBuffInfo(BattleCardBase card)
	{
		BuffInfoContainer buffInfoContainer = this.buffInfoContainer.FirstOrDefault((BuffInfoContainer b) => b._targetCard == card);
		if (buffInfoContainer != null)
		{
			this.buffInfoContainer.Remove(buffInfoContainer);
		}
		return buffInfoContainer;
	}

	public BuffInfoContainer GetBuffInfo(BattleCardBase card)
	{
		return buffInfoContainer.FirstOrDefault((BuffInfoContainer b) => b._targetCard == card);
	}

	public List<BuffInfoContainer> GetBuffInfoContainer()
	{
		return buffInfoContainer;
	}

	public void SetInvoked(bool flag)
	{
		IsInvoked = flag;
	}

	public bool IsHaveApplicableTargetFilter<T>()
	{
		if (!(ApplyingTargetFilter is T))
		{
			return ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is T);
		}
		return true;
	}

	public void InitSetIndividualId()
	{
		if (Option.Contains(SkillFilterCreator.ContentKeyword.is_individual.ToString()) || SkillPrm.buildInfo._preprocess.Contains(SkillFilterCreator.ContentKeyword.is_individual.ToString()))
		{
			IndividualId = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.NextIndividualId;
		}
	}

	public void SetIndividualId(int id, bool isForce = false)
	{
		if (isForce || Option.Contains(SkillFilterCreator.ContentKeyword.is_individual.ToString()) || SkillPrm.buildInfo._preprocess.Contains(SkillFilterCreator.ContentKeyword.is_individual.ToString()))
		{
			IndividualId = id;
		}
	}

	public void SetDuplicateBanSkillNum()
	{
		string text = OptionValue.GetString(SkillFilterCreator.ContentKeyword.duplicate_ban, string.Empty);
		if (text != string.Empty)
		{
			IsDuplicateBanSelfSkill = text.Split(':')[0] == "self_skill";
			DuplicateBanSkillNum = text.Split(':')[1];
		}
	}

	public void SetIsResidentSkillStartFlag(bool flg)
	{
		_isResidentSkillStartFlag = flg;
	}

	public void CallOnCalcApplyTargets(SkillBase skill, IEnumerable<BattleCardBase> unapprovedCards)
	{
		this.OnCalcApplyTargets.Call(skill, unapprovedCards);
	}

	public SkillBase(SkillParameter skillPrm, string option)
	{
		PublishedActiveSkillCount = -1;
		IsActivity = false;
		SkillPrm = skillPrm;
		OnSkillEnd += delegate(SkillBase skill, List<BattleCardBase> targets, SkillConditionCheckerOption checkeroption, SkillProcessor skillProcessor)
		{
			if (skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter x) => x.Text.Contains("{self.super_skybound_art_count}<={me.inplay.class.turn}")) && skill.SkillPrm.ownerCard.NormalSkills.Where((SkillBase x) => x.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter y) => y.Text.Contains("{self.super_skybound_art_count}<={me.inplay.class.turn}"))).First() == skill)
			{
				skill.SkillPrm.selfBattlePlayer.GameSuperSkyboundArtCards.Add(skill.SkillPrm.ownerCard);
			}
			return NullVfx.GetInstance();
		};
		OptionValue = new SkillOptionValue(skillPrm.buildInfo._parsedOption);
		Option = option;
		IsInductionIcon = skillPrm.buildInfo._icon.Contains("induction");
		buffInfoContainer = new List<BuffInfoContainer>();
		ConditionFilterCollection = new ConditionSkillFilterCollection();
		ConditionCheckerList = new List<ISkillConditionChecker>();
		ApplyFilterCollection = new ApplySkillTargetFilterCollection();
		PreprocessList = new List<SkillPreprocessBase>();
		if (skillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork)
		{
			if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle)
			{
				_executionInfoCreator = new ReplayExecutionInfoCreator(this);
			}
			else
			{
				_executionInfoCreator = new NetworkExecutionInfoCreator(this);
			}
		}
		else
		{
			_executionInfoCreator = new SingleExecutionInfoCreator(this);
		}
		_executionBelongInfo = new ExecutionBelongInfo();
		IsReferencePreviousSkill = GetIsReferencePreviousSkill();
	}

	public SkillProcessor.ProcessInfo CreateStopProcessInfoResidentSkill(bool isPlayer, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return new SkillProcessor.StopProcessInfoResidentSkill(SkillPrm.ownerCard, this, isPlayer, skillProcessor, playerInfoPair, checkerOption);
	}

	private bool GetIsReferencePreviousSkill()
	{
		return Data.Master.WhenPlayEffectKeywordMaster.Any((string key) => SkillPrm.buildInfo._checkTargetKeywords.Contains(key));
	}

	public void SetAndAddPublishedActiveSkillCount()
	{
		if (!SkillPrm.selfBattlePlayer.BattleMgr.IsVirtualBattle && !(this is Skill_none) && !IsNotAssignPublishedActiveSkillCount)
		{
			PublishedActiveSkillCount = SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedActiveSkillCount;
			SkillPrm.selfBattlePlayer.BattleMgr.AllPublishedActiveSkillCount++;
			SkillPrm.selfBattlePlayer.BattleMgr.AddPublishedSkillList(this);
		}
	}

	public void SetPublishedActiveSkillCount(int count)
	{
		if (!SkillPrm.selfBattlePlayer.BattleMgr.IsVirtualBattle && !(this is Skill_none) && PublishedActiveSkillCount == -1)
		{
			PublishedActiveSkillCount = count;
		}
	}

	public void SetCallCountText(string text)
	{
		CallCountText = text;
	}

	public bool CheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		bool result = false;
		if (SkillPrm.ownerCard.IsCantActivateFanfare && IsWhenPlaySkill)
		{
			return result;
		}
		result = _executionInfoCreator.CheckCondition(playerInfoPair, option, isPrePlay);
		if (result && !isPrePlay)
		{
			_executionBelongInfo.SetBelong(SkillPrm.ownerCard.SelfBattlePlayer, SkillPrm.ownerCard);
		}
		return result;
	}

	public bool CheckConditionAI(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		bool result = false;
		if (SkillPrm.ownerCard.IsCantActivateFanfare && IsWhenPlaySkill)
		{
			return result;
		}
		result = _executionInfoCreator.CheckCondition(playerInfoPair, option, isPrePlay);
		if (result && !isPrePlay)
		{
			_executionBelongInfo.SetBelong(SkillPrm.ownerCard.SelfBattlePlayer, SkillPrm.ownerCard);
		}
		return result;
	}

	public void SetScanCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		IsScanConditionOk = _executionInfoCreator.CheckScanCondition(playerInfoPair, option, isPrePlay);
	}

	public bool VisualCheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		bool result = false;
		if (SkillPrm.ownerCard.IsCantActivateFanfare && IsWhenPlaySkill)
		{
			return result;
		}
		if ((ApplyingTargetFilter is SkillTargetHandFilter || ApplyingTargetFilter is SkillTargetHandOtherSelfFilter) && IsUserSelectType && GetSelectableCards(playerInfoPair, option).Count() == 0)
		{
			return false;
		}
		return _executionInfoCreator.VisualCheckCondition(playerInfoPair, option, isPrePlay);
	}

	public VfxBase Preprocess(SkillProcessor skillProcessor, SkillConditionCheckerOption checkerOptionr)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (SkillPreprocessBase preprocess in PreprocessList)
		{
			BattlePlayerPair playerPair = new BattlePlayerPair(SkillPrm.selfBattlePlayer, SkillPrm.opponentBattlePlayer);
			VfxBase vfx = preprocess.Start(playerPair, this, skillProcessor, OptionValue, checkerOptionr);
			parallelVfxPlayer.Register(vfx);
		}
		return parallelVfxPlayer;
	}

	public IEnumerable<BattleCardBase> CalcApplyTargets(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, ref int targetCount, bool isCheckInHand = false)
	{
		return _executionInfoCreator.CalcApplyTargets(playerInfoPair, option, ref targetCount, isCheckInHand);
	}

	public IEnumerable<BattleCardBase> GetSelectableCards(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isSkipForceSelect = false, List<BattleCardBase> selectedCards = null)
	{
		return _executionInfoCreator.GetSelectableCards(playerInfoPair, option, isSkipForceSelect, selectedCards);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> FilteringSneakTarget(IEnumerable<IReadOnlyBattleCardInfo> targets)
	{
		if (IsUserSelectType)
		{
			return targets.Where((IReadOnlyBattleCardInfo c) => (!c.SkillApplyInformation.CantBeFocusedSkill && (!(SkillPrm.ownerCard is SpellBattleCard) || !c.SkillApplyInformation.CantBeFocusedSpell)) || SkillPrm.ownerCard.IsPlayer == c.IsPlayer);
		}
		return targets;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> FilteringByTargetFilter(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		if (ApplyAndFilter.Count > 0)
		{
			List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
			for (int i = 0; i < ApplyAndFilter.Count; i++)
			{
				IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = ApplyAndFilter[i].BattlePlayerFilter.Filtering(playerInfoPair);
				list.AddRange(ApplyAndFilter[i].TargetFilter.Filtering(battlePlayerInfos, option));
			}
			return list.Distinct();
		}
		IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos2 = ApplyBattlePlayerFilter.Filtering(playerInfoPair);
		return ApplyingTargetFilter.Filtering(battlePlayerInfos2, option);
	}

	public IEnumerable<IReadOnlyBattleCardInfo> FilteringForceSelectTargets(IEnumerable<IReadOnlyBattleCardInfo> targets)
	{
		if (IsUserSelectType && targets.Any((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.IsForceSkillTarget && c.IsPlayer != SkillPrm.ownerCard.IsPlayer))
		{
			return targets.Where((IReadOnlyBattleCardInfo c) => c.SkillApplyInformation.IsForceSkillTarget && SkillPrm.ownerCard.IsPlayer != c.IsPlayer);
		}
		return targets;
	}

	protected VfxWithLoading CreateSkillEffect(IBattleResourceMgr resourceMgr, IEnumerable<BattleCardBase> targetCards, bool isFollowInHand = false, bool addToLastOperation = false, bool skipCallOnEffect = false)
	{
		if (!SkillPrm.ownerCard.IsPlayer && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch && ApplyingTargetFilter is SkillTargetHandSelfFilter)
		{
			return NullVfxWithLoading.GetInstance();
		}
		if (!skipCallOnEffect)
		{
			SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnEffect(SkillPrm.buildInfo, isFollowInHand, isTargetPosition: false, addToLastOperation, OnWhenFusioned != 0);
		}
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			return NullVfxWithLoading.GetInstance();
		}
		Func<Vector3> func = SkillPrm.afterFallPos;
		if (SkillPrm.ownerCard.IsSpell || OnWhenFusioned != 0)
		{
			func = () => SkillPrm.selfBattlePlayer.Class.BattleCardView.GameObject.transform.position;
		}
		if (string.IsNullOrEmpty(SkillPrm.buildInfo._effectPath))
		{
			return NullVfxWithLoading.GetInstance();
		}
		switch (SkillPrm.buildInfo._effectTargetType)
		{
		case EffectMgr.TargetType.NONE:
			return NullVfxWithLoading.GetInstance();
		case EffectMgr.TargetType.NONE_WAIT:
			return VfxWithLoading.Create(WaitVfx.Create(SkillPrm.buildInfo._effectTime));
		case EffectMgr.TargetType.SINGLE:
			if (isFollowInHand)
			{
				return CreateSingleFollowInHandVfx(targetCards, SkillPrm.buildInfo._effectPath, SkillPrm.buildInfo._sePath);
			}
			return CreateSingleVfx(resourceMgr, func, targetCards, addToLastOperation);
		case EffectMgr.TargetType.AREA_SELF:
			return CreateAreaVfx(resourceMgr, func, SkillPrm.selfBattlePlayer.GetFieldCenterPosition());
		case EffectMgr.TargetType.AREA_OPPONENT:
			return CreateAreaVfx(resourceMgr, func, SkillPrm.opponentBattlePlayer.GetFieldCenterPosition());
		case EffectMgr.TargetType.AREA_ALL:
			return CreateAreaVfx(resourceMgr, func, Vector3.zero);
		case EffectMgr.TargetType.SINGLE_ONLY_OPPONENT:
		{
			List<BattleCardBase> targetCards2 = targetCards.Where((BattleCardBase s) => s.IsPlayer != SkillPrm.ownerCard.IsPlayer).ToList();
			return CreateSingleVfx(resourceMgr, func, targetCards2, addToLastOperation);
		}
		default:
			return NullVfxWithLoading.GetInstance();
		}
	}

	public VfxWithLoading CreateSkillEffectFromPath(string effectPath, string sePath, IBattleResourceMgr resourceMgr, EffectMgr.EngineType engineType, EffectMgr.MoveType moveType, Func<Vector3> getEffectStartPoint, Func<Vector3> getEffectEndPoint, float animationTime, Color color)
	{
		if (string.IsNullOrEmpty(effectPath))
		{
			return NullVfxWithLoading.GetInstance();
		}
		EffectBattle effectBattle = null;
		WaitEffectLoadVfx loadingVfx = new WaitEffectLoadVfx(effectPath, engineType, sePath, resourceMgr, delegate(EffectBattle eb)
		{
			effectBattle = eb;
		});
		VfxBase mainVfx = NullVfx.GetInstance();
		return VfxWithLoading.Create(loadingVfx, mainVfx);
	}

	public static VfxWithLoading CreateSingleFollowInHandVfx(IEnumerable<BattleCardBase> targetCards, string effectPath, string sePath)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in targetCards)
		{
			GameObject effectGameObject = null;
			VfxBase vfx = NullVfx.GetInstance();
			parallelVfxPlayer.Register(vfx);
			VfxBase vfx2 = NullVfx.GetInstance();
			parallelVfxPlayer2.Register(vfx2);
		}
		return VfxWithLoading.Create(parallelVfxPlayer, parallelVfxPlayer2);
	}

	protected VfxWithLoading CreateSingleVfx(IBattleResourceMgr resourceMgr, Func<Vector3> getEffectStartPoint, IEnumerable<BattleCardBase> targetCards, bool addToLastOperation = false)
	{
		return CreateSingleVfx(resourceMgr, getEffectStartPoint, targetCards, SkillPrm.ownerCard.IsPlayer, SkillPrm.ownerCard.BattleCardView, SkillPrm.buildInfo._effectPath, SkillPrm.buildInfo._engineType, SkillPrm.buildInfo._sePath, SkillPrm.buildInfo._effectMoveType, SkillPrm.buildInfo._effectTargetType, SkillPrm.buildInfo._effectTime);
	}

	public static VfxWithLoading CreateSingleVfx(IBattleResourceMgr resourceMgr, Func<Vector3> getEffectStartPoint, IEnumerable<BattleCardBase> targetCards, bool isPlayer, IBattleCardView battleCardView, string effectPath, EffectMgr.EngineType engineType, string sePath, EffectMgr.MoveType effectMoveType, EffectMgr.TargetType effectTargetType, float effectTime)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		for (int i = 0; i < targetCards.Count(); i++)
		{
			BattleCardBase targetCard = targetCards.ElementAt(i);
			EffectBattle effectBattle = null;
			parallelVfxPlayer.Register(new WaitEffectLoadVfx(effectPath, engineType, sePath, resourceMgr, delegate(EffectBattle eb)
			{
				effectBattle = eb;
			}));
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			parallelVfxPlayer2.Register(sequentialVfxPlayer);
		}
		return VfxWithLoading.Create(parallelVfxPlayer, parallelVfxPlayer2);
	}

	private VfxWithLoading CreateAreaVfx(IBattleResourceMgr resourceMgr, Func<Vector3> effectStartPoint, Vector3 effectEndPoint)
	{
		return CreateAreaVfx(resourceMgr, effectStartPoint, effectEndPoint, SkillPrm.ownerCard.IsPlayer, SkillPrm.ownerCard.BattleCardView, SkillPrm.buildInfo._effectPath, SkillPrm.buildInfo._engineType, SkillPrm.buildInfo._sePath, SkillPrm.buildInfo._effectMoveType, SkillPrm.buildInfo._effectTargetType, SkillPrm.buildInfo._effectTime);
	}

	public static VfxWithLoading CreateAreaVfx(IBattleResourceMgr resourceMgr, Func<Vector3> effectStartPoint, Vector3 effectEndPoint, bool isPlayer, IBattleCardView battleCardView, string effectPath, EffectMgr.EngineType engineType, string sePath, EffectMgr.MoveType effectMoveType, EffectMgr.TargetType effectTargetType, float effectTime)
	{
		EffectBattle effectBattle = null;
		WaitEffectLoadVfx loadingVfx = new WaitEffectLoadVfx(effectPath, engineType, sePath, resourceMgr, delegate(EffectBattle eb)
		{
			effectBattle = eb;
		});
		VfxBase mainVfx = NullVfx.GetInstance();
		return VfxWithLoading.Create(loadingVfx, mainVfx);
	}

	public abstract VfxWithLoading Start(CallParameter parameter);

	public virtual VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		this.OnSkillStopStart.Call(this, _targetCards, skillProcessor);
		return NullVfxWithLoading.GetInstance();
	}

	protected void StopEnd(SkillProcessor skillProcessor)
	{
		this.OnSkillStopEnd.Call(this, _targetCards, new SkillConditionCheckerOption(), skillProcessor);
	}

	public virtual VfxWithLoading RemoveAfter()
	{
		return NullVfxWithLoading.GetInstance();
	}

	public virtual bool IsRefVariable(string variableName)
	{
		return Regex.IsMatch(Option, variableName);
	}

	public virtual void SkillCreateEnd()
	{
	}

	public int GetSkillSelectCount()
	{
		if (IsChoiceType)
		{
			return ChoiceUtility.GetNumberOfCardsToSelect(this);
		}
		if (IsBurialRite && !IsUserSelectType)
		{
			return 1;
		}
		if (ApplySelectFilter is SkillUserSelectFilter skillUserSelectFilter)
		{
			return skillUserSelectFilter.CalcCount(OptionValue);
		}
		if (this is Skill_select)
		{
			return 1;
		}
		return 0;
	}

	public void SetIsAttachSkill(bool flag, bool isInplay)
	{
		IsAttachedSkill = flag;
		IsAttachedInplaySkill = flag && isInplay;
	}

	public bool IsStopTiming()
	{
		if (IsAllResidentTiming)
		{
			return IsResidentSkillStartFlag;
		}
		return false;
	}

	public SkillProcessor.ProcessInfo CreateProcessInfo(bool isPlayer, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, bool isInductionSkill)
	{
		return new SkillProcessor.ProcessInfoSkill(SkillPrm.ownerCard, this, isPlayer, skillProcessor, playerInfoPair, checkerOption);
	}

	public VfxBase CallStart(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, CallParameter parameter, SkillProcessor.ProcessCallType callType = SkillProcessor.ProcessCallType.Start, bool isLoop = false)
	{
		this.OnBeforeProcess.Call(this);
		bool flag = false;
		SkillBase skill = this;
		checkerOption.SkillDrewCards = parameter.calledSkillResultInfo.drawCards.ToList();
		SkillDrewCards = checkerOption.SkillDrewCards;
		checkerOption.SkillUpdatedDeckCards = parameter.calledSkillResultInfo.UpdatedDeckCards.ToList();
		checkerOption.DrewOverHandLimitCards = parameter.calledSkillResultInfo.drewOverHandLimitCards.ToList();
		checkerOption.ProcessSkillList = skillProcessor.GetProcessSkillList();
		parameter.calledSkillResultInfo.SelfLastTargetCards = skill.SkillPrm.ownerCard.SelfBattlePlayer.LastTargetCardsList;
		parameter.calledSkillResultInfo.OpponentLastTargetCards = skill.SkillPrm.ownerCard.OpponentBattlePlayer.LastTargetCardsList;
		SkillCollectionBase.SetupOptionValue(skill.OptionValue, playerInfoPair, SkillPrm.ownerCard, skill, checkerOption);
		int num = skill.CallCount;
		if (skill is Skill_summon_token)
		{
			int max = 6 - skill.SkillPrm.ownerCard.SelfBattlePlayer.InPlayCards.Count();
			num = Mathf.Clamp(num, 1, max);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSkillVfxStart();
		for (int i = 0; i < num; i++)
		{
			if (SkillPrm.selfBattlePlayer.Class.IsDead || SkillPrm.opponentBattlePlayer.Class.IsDead)
			{
				continue;
			}
			if (!skill.PreprocessList.All((SkillPreprocessBase p) => p.IsRight(playerInfoPair, checkerOption, PreexecutionCheck: true, callType == SkillProcessor.ProcessCallType.ResidentStop)) || !LightCheckAvailable(skill, checkerOption))
			{
				checkerOption.IsRefPrev = false;
				break;
			}
			string skillDescription = string.Empty;
			JsonData sideLogCardData = null;
			bool isDeckSelf = (skill is Skill_summon_card || skill is Skill_draw || skill is Skill_token_draw || skill is Skill_summon_token) && IsDeckSelfSkill;
			bool isInHand = skill.SkillPrm.ownerCard.IsInHand;
			if (skill.SkillPrm.ownerCard.BaseParameter.SkillDescription.Contains("skill_activated_count") && skill.SkillPrm.ownerCard.HasSkillActivatedCountWrapValue)
			{
				skillDescription = skill.SkillPrm.ownerCard.GetCardSkillDescription(new BattlePlayerBase.SideLogInfo(skill), SkillPrm.ownerCard.IsEvolution);
				sideLogCardData = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnCreateSideLogCardData(skill.SkillPrm.ownerCard, skill, isDeckSelf, isInHand);
			}
			VfxBase vfxToRegister = skill.Preprocess(skillProcessor, checkerOption);
			checkerOption.IsRefPrev = true;
			vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
			int targetCount = 0;
			VfxWith<List<BattleCardBase>, Dictionary<int, BattleCardBase>> vfxWith = _executionInfoCreator.FixedSkillApplyTarget(skill.SkillPrm.CreateInfoPair(), checkerOption, ref targetCount);
			skill.Used = true;
			if (ApplySelectFilter is SimulateRandomSelectFilter)
			{
				skill.UsedRandom = true;
			}
			List<IReadOnlyBattleCardInfo> nextTargetCards = new List<IReadOnlyBattleCardInfo>();
			if (vfxWith.Value_1.Count() > 0)
			{
				nextTargetCards = GetNextBattleCard(vfxWith.Value_1, vfxWith.Value_1.ToList()[0].SelfBattlePlayer);
			}
			this.OnSkillStart.Call(this, vfxWith.Value_1, checkerOption);
			bool flag2 = CheckAvailableTargets(skill, vfxWith.Value_1, checkerOption);
			VfxBase vfxBase = null;
			bool flag3 = IsSkipInduction(callType);
			if (!flag && !flag3)
			{
				vfxBase = CreateSkillInductionEffect();
				vfxWithLoadingSequential.RegisterToMainVfx(vfxBase);
				flag = true;
			}
			bool isEvolve = SkillPrm.ownerCard.IsEvolution || (SkillPrm.ownerCard.IsEvolvedOnWhenLeave && (skill.OnWhenLeave != 0 || skill.OnWhenReturnStart != 0));
			if (IsPreviousGetSideLogText(skill.SkillPrm.ownerCard.BaseParameter.SkillDescription))
			{
				skillDescription = skill.SkillPrm.ownerCard.GetCardSkillDescription(new BattlePlayerBase.SideLogInfo(skill), isEvolve);
				sideLogCardData = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnCreateSideLogCardData(skill.SkillPrm.ownerCard, skill, isDeckSelf, isInHand);
			}
			bool flag4 = true;
			if (callType == SkillProcessor.ProcessCallType.ResidentStop && ConditionCheckerList.Where((ISkillConditionChecker c) => c is SkillConditionTurn).Any((ISkillConditionChecker t) => !t.IsRight(playerInfoPair, checkerOption)))
			{
				flag4 = false;
			}
			bool flag5 = !(ApplyingTargetFilter is SkillTargetChosenCardsFilter) || (ApplyingTargetFilter is SkillTargetChosenCardsFilter && skill.IsWhenEvolveSkill);
			if (!flag3 && skill.ShowSideLog && CheckShowSideLogCondition(vfxWith.Value_1, flag2, callType) && skill.OnWhenSpellChargeStart == 0 && !skill.IsChoiceType && flag5 && i == 0 && !isLoop && flag4 && IsShowSideLogSkillType)
			{
				SideLogControl sideLogControl = skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleView.GetSideLogControl(isSkillTargetSelect: false);
				bool isOnSummonOrReturnTiming = sideLogControl != null && sideLogControl.IsOnSummonOrReturnTimingSkill(skill);
				skillProcessor.OnSkillStart += delegate
				{
					SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnCreateSideLog(skill.SkillPrm.ownerCard, skill, isEvolve, isOnSummonOrReturnTiming, isDeckSelf, isInHand, sideLogCardData);
					return skill.SkillPrm.ownerCard.CreateShowLogVfx(3f, skill, isEvolve, skillDescription);
				};
			}
			if (flag2)
			{
				parameter.targetCards = vfxWith.Value_1;
				VfxWithLoading vfxWithLoading;
				if (callType == SkillProcessor.ProcessCallType.ResidentStop)
				{
					vfxWithLoading = skill.Stop(parameter.skillProcessor);
				}
				else
				{
					if (skill.IsBattleLog)
					{
						BattleLogManager.GetInstance().InsertExclusionTargetListLog(skill);
					}
					vfxWithLoading = skill.Start(parameter);
					skillProcessor.AddProcessSkilList(this);
				}
				if (vfxBase != null && !(vfxBase is NullVfx) && !(vfxWithLoading.MainVfx is NullVfx))
				{
					vfxWithLoadingSequential.RegisterToMainVfx(vfxBase);
				}
				vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoading);
			}
			vfxWithLoadingSequential.RegisterToMainVfx(vfxWith.Vfx);
			if (!(this is Skill_select) && !(this is Skill_copy_skill))
			{
				List<BattleCardBase> list = vfxWith.Value_2.Values.ToList();
				if (list.Count > 0)
				{
					SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnShowIndependentEffect(list);
				}
			}
			vfxWithLoadingSequential.RegisterToMainVfx(this.OnSkillEnd.GetAllFuncVfxResults(this, vfxWith.Value_1, checkerOption, parameter.skillProcessor));
			List<BattleCardBase> value_ = vfxWith.Value_1;
			foreach (KeyValuePair<int, BattleCardBase> item3 in vfxWith.Value_2)
			{
				value_.Insert(item3.Key, item3.Value);
			}
			if (OnWhenChangeInPlayImmediate == 0)
			{
				List<BattleCardBase> list2 = value_.Where((BattleCardBase c) => skill.SkillPrm.ownerCard.IsPlayer == c.IsPlayer).ToList();
				List<BattleCardBase> list3 = value_.Where((BattleCardBase c) => skill.SkillPrm.ownerCard.IsPlayer != c.IsPlayer).ToList();
				list2.AddRange(parameter.calledSkillResultInfo.AddLastTargetCards.Where((BattleCardBase c) => skill.SkillPrm.ownerCard.IsPlayer == c.IsPlayer).ToList());
				list3.AddRange(parameter.calledSkillResultInfo.AddLastTargetCards.Where((BattleCardBase c) => skill.SkillPrm.ownerCard.IsPlayer != c.IsPlayer).ToList());
				skill.SkillPrm.ownerCard.SelfBattlePlayer.LastTargetCardsList.Insert(0, new List<BattleCardBase>());
				skill.SkillPrm.ownerCard.OpponentBattlePlayer.LastTargetCardsList.Insert(0, new List<BattleCardBase>());
				parameter.calledSkillResultInfo.AddLastTargetCards.Clear();
				if (!IsHandResident)
				{
					for (int num2 = 0; num2 < list2.Count; num2++)
					{
						BattleCardBase item = list2[num2];
						skill.SkillPrm.ownerCard.SelfBattlePlayer.LastTargetCardsList.First().Add(item);
					}
					for (int num3 = 0; num3 < list3.Count; num3++)
					{
						BattleCardBase item2 = list3[num3];
						skill.SkillPrm.ownerCard.OpponentBattlePlayer.LastTargetCardsList.First().Add(item2);
					}
				}
			}
			_targetCards = vfxWith.Value_1.ToList();
			checkerOption.NextTargetCards = nextTargetCards;
			if (OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.save_target))
			{
				skill.SkillPrm.ownerCard.SkillApplyInformation.SaveTargetList(_targetCards);
			}
			if (OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.save_target_card_id))
			{
				skill.SkillPrm.ownerCard.SkillApplyInformation.SaveTargetList(_targetCards);
				string[] array = OptionValue.GetString(SkillFilterCreator.ContentKeyword.save_target_card_id).Split(':');
				long num4 = long.Parse(array[0]);
				if (array.Count() > 1 && array[1] == SkillFilterCreator.ContentKeyword.is_individual.ToString())
				{
					num4 += IndividualId;
				}
				skill.SkillPrm.selfBattlePlayer.Class.SkillApplyInformation.SaveTargetCardId(num4, _targetCards.Select((BattleCardBase c) => c.BaseParameter.NormalCardId).ToList());
			}
			checkerOption.SetCheckerOptionTransientValue(SkillPrm.ownerCard.SelfBattlePlayer, SkillPrm.ownerCard.OpponentBattlePlayer);
		}
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr is NetworkBattleManagerBase && (SkillPrm.selfBattlePlayer.Class.IsDead || SkillPrm.opponentBattlePlayer.Class.IsDead))
		{
			SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.LethalPublishedActiveSkillCount = NetworkBattleGenericTool.GetPublishSkillCount(this);
			SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.LethalMovementCount = NetworkBattleGenericTool.GetSkillMovementNum(this);
		}
		SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSkillVfxEnd();
		return vfxWithLoadingSequential;
	}

	protected virtual bool CheckShowSideLogCondition(IEnumerable<BattleCardBase> targets, bool isTargetsAvailable, SkillProcessor.ProcessCallType type)
	{
		return true;
	}

	protected void AddLastTarget(CallParameter parameter, List<BattleCardBase> cards)
	{
		foreach (BattleCardBase card in cards)
		{
			parameter.calledSkillResultInfo.AddLastTargetCards.Add(card);
		}
	}

	private List<IReadOnlyBattleCardInfo> GetNextBattleCard(IEnumerable<BattleCardBase> nowTarget, BattlePlayerBase player)
	{
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		if (ApplyCustomSelectFilterList.Any((ISkillCustomSelectFilter s) => s is SkillInOrderFromOldestFilter))
		{
			SkillInOrderFromOldestFilter obj = (SkillInOrderFromOldestFilter)ApplyCustomSelectFilterList.SingleOrDefault((ISkillCustomSelectFilter s) => s is SkillInOrderFromOldestFilter);
			List<IReadOnlyBattleCardInfo> list2 = obj.OldTargets.ToList();
			IReadOnlyBattleCardInfo readOnlyBattleCardInfo = obj.OldTargets.SingleOrDefault((IReadOnlyBattleCardInfo s) => s.IsClass);
			list2.Remove(readOnlyBattleCardInfo);
			if (list2.Count > 0 && nowTarget.Count() > 0)
			{
				int num = list2.IndexOf(nowTarget.Last());
				if (nowTarget.Last().IsClass && list2.Count <= 0)
				{
					list.Add(readOnlyBattleCardInfo);
				}
				else if (nowTarget.Last().IsClass && list2.Count > 0)
				{
					list.Add(list2[0]);
				}
				else if (num != -1 && list2.Count > num + 1)
				{
					list.Add(list2[num + 1]);
				}
				else if (readOnlyBattleCardInfo != null)
				{
					list.Add(readOnlyBattleCardInfo);
				}
			}
			else if (readOnlyBattleCardInfo != null)
			{
				list.Add(readOnlyBattleCardInfo);
			}
		}
		return list;
	}

	public void SetSkillVoiceIndex(int voiceIndex)
	{
		if (voiceIndex == -1)
		{
			return;
		}
		int num;
		IReadOnlyVoiceInfo readOnlyVoiceInfo;
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			num = ((GetAttachSkill.SkillPrm.ownerCard.BattleCardView is NullBattleCardView) ? 1 : 0);
			if (num != 0)
			{
				readOnlyVoiceInfo = CardVoiceInfoCache.GetCardVoiceInfoForBattle(GetAttachSkill.SkillPrm.ownerCard.CardId);
				goto IL_006c;
			}
		}
		else
		{
			num = 0;
		}
		readOnlyVoiceInfo = GetAttachSkill.SkillPrm.ownerCard.BattleCardView.VoiceInfo;
		goto IL_006c;
		IL_006c:
		IReadOnlyVoiceInfo readOnlyVoiceInfo2 = readOnlyVoiceInfo;
		if (num != 0 && GetAttachSkill is Skill_attach_skill skill_attach_skill)
		{
			readOnlyVoiceInfo2.AddAttachSkillVoice(skill_attach_skill.BuildInfo._voice);
		}
		GetAttachSkill.SkillPrm.ownerCard.BattleCardView.VoiceInfo.AddAttachSkillVoice((GetAttachSkill as Skill_attach_skill).BuildInfo._voice);
		_inductionSkillVoice = readOnlyVoiceInfo2.GetAttachSkillVoice(voiceIndex);
		_inductionEvolutionSkillVoice = readOnlyVoiceInfo2.GetAttachSkillVoice(voiceIndex);
	}

	public void SetInductionVoiceIndex(bool isSpecialWin = false, bool isNewReplayRecordInRecovery = false)
	{
		if (!IsInductionSkill && !isSpecialWin)
		{
			return;
		}
		BattleCardBase ownerCard = SkillPrm.ownerCard;
		IBattleCardView battleCardView = SkillPrm.ownerCard.BattleCardView;
		if ((battleCardView == null || battleCardView is NullBattleCardView || ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle || IsAttachedSkill) && !isNewReplayRecordInRecovery)
		{
			return;
		}
		IReadOnlyVoiceInfo readOnlyVoiceInfo = (isNewReplayRecordInRecovery ? CardVoiceInfoCache.GetCardVoiceInfoForBattle(ownerCard.CardId) : battleCardView.VoiceInfo);
		if (readOnlyVoiceInfo == null)
		{
			return;
		}
		int num = ownerCard.NormalSkills.IndexOf(this);
		if (num != -1 && ownerCard.IsSpell && ownerCard.NormalSkills.FirstOrDefault() is Skill_spell_charge)
		{
			num--;
		}
		if (num != -1)
		{
			_inductionSkillVoice = readOnlyVoiceInfo.GetSkillVoice(isEvolution: false, num).Voice;
			string[] array = _inductionSkillVoice.Split('|');
			if (array.Length >= 2)
			{
				_inductionSkillVoice = array[0];
				_inductionEvolutionSkillVoice = array[1];
			}
		}
		if (_inductionEvolutionSkillVoice == null)
		{
			int num2 = ownerCard.EvolutionSkills.IndexOf(this);
			if (num2 != -1)
			{
				_inductionEvolutionSkillVoice = readOnlyVoiceInfo.GetSkillVoice(isEvolution: true, num2).Voice;
			}
			else if (num != -1)
			{
				_inductionEvolutionSkillVoice = _inductionSkillVoice;
			}
		}
	}

	protected VfxBase CreateSkillActivationVoiceVfx()
	{
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
		{
			return NullVfx.GetInstance();
		}
		string text = (SkillPrm.ownerCard.IsEvolution ? _inductionEvolutionSkillVoice : _inductionSkillVoice);
		if (ReadOnlyVoiceInfo.IsInvalidFileName(text))
		{
			return NullVfx.GetInstance();
		}
		IBattleCardView battleCardView = SkillPrm.ownerCard.BattleCardView;
		string text2 = battleCardView.VoiceInfo.VoiceId;
		if (string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text) && text != "NONE".ToLower())
		{
			text2 = text.Split(new string[1] { "_" }, StringSplitOptions.None)[0];
		}
		return VfxWithLoading.Create(NullVfx.GetInstance(), NullVfx.GetInstance());
	}

	private bool LightCheckAvailable(SkillBase skill, SkillConditionCheckerOption checkerOption)
	{
		if (skill.IsWhenDestroySkill || skill.OnDisCardStart != 0 || skill.OnWhenChoicePlayStart != 0 || skill.OnWhenChoiceEvolveStart != 0 || skill.OnWhenBanish != 0 || skill.OnWhenLeave != 0 || skill.OnWhenGetOff != 0)
		{
			return true;
		}
		if (skill.SkillPrm.ownerCard.IsDead)
		{
			return false;
		}
		if (skill.SkillPrm.ownerCard.IsInHand && !skill.SkillPrm.ownerCard.IsSpell && skill.OnWhenPlayStart != 0)
		{
			return false;
		}
		if (!skill._executionBelongInfo.CheckBelong(skill.SkillPrm.ownerCard.SelfBattlePlayer, skill.SkillPrm.ownerCard, checkerOption, skill.OnWhenReturnStart != 0))
		{
			return false;
		}
		return true;
	}

	private bool CheckAvailableTargets(SkillBase skill, IEnumerable<BattleCardBase> targets, SkillConditionCheckerOption checkerOption)
	{
		if (skill.IsWhenDestroySkill || skill.OnDisCardStart != 0 || skill.OnWhenBanish != 0 || skill.OnWhenLeave != 0 || skill.OnWhenGetOff != 0)
		{
			return true;
		}
		if (skill.SkillPrm.ownerCard.IsDead)
		{
			return false;
		}
		if (skill.OnWhenChoicePlayStart != 0 || skill.OnWhenChoiceEvolveStart != 0)
		{
			return true;
		}
		if (!skill._executionBelongInfo.CheckBelong(skill.SkillPrm.ownerCard.SelfBattlePlayer, skill.SkillPrm.ownerCard, checkerOption, skill.OnWhenReturnStart != 0))
		{
			return false;
		}
		if (targets.Count() == 0 && !skill.IsTargetIndicate)
		{
			return true;
		}
		if (skill.ApplyingTargetFilter is SkillTargetDestroyedCardListFilter || skill.ApplyingTargetFilter is SkillTargetDestroyedCardFilter)
		{
			return true;
		}
		if (targets.Any((BattleCardBase c) => c.IsDead) && skill is SkillBaseSummon)
		{
			return true;
		}
		if (skill is Skill_stack_white_ritual || skill is Skill_change_white_ritual_stack)
		{
			return true;
		}
		if (!targets.Any((BattleCardBase t) => !t.IsDead))
		{
			return skill.IsAllowDestroyTarget;
		}
		return true;
	}

	public bool DoesSkillFulfillActivationConditions(BattlePlayerReadOnlyInfoPair playerInfoPair, bool ignoreHandSkill, bool isPrePlay)
	{
		if (ignoreHandSkill && IsHandResident)
		{
			return false;
		}
		return CheckCondition(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay);
	}

	private VfxBase CreateSkillInductionEffect()
	{
		if (IsInductionSkill && !SkillPrm.ownerCard.IsDead && !SkillPrm.ownerCard.IsInHand)
		{
			VfxBase inductionVoiceVfx = CreateSkillActivationVoiceVfx();
			return ParallelVfxPlayer.Create(CreateInductionSkillActivationVfxFunc.GetAllFuncVfxResults(() => inductionVoiceVfx), NullVfx.GetInstance());
		}
		return NullVfx.GetInstance();
	}

	public bool IsTargetInOpponentHand()
	{
		if ((SkillPrm.ownerCard.IsPlayer && ApplyBattlePlayerFilter is OpponentBattlePlayerFilter) || (!SkillPrm.ownerCard.IsPlayer && ApplyBattlePlayerFilter is SelfBattlePlayerFilter))
		{
			return IsTargetInHand();
		}
		return false;
	}

	public bool IsTargetInHand()
	{
		if (ApplyAndFilter.Count > 0)
		{
			if (ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetHandFilter || f.TargetFilter is SkillTargetHandOtherSelfFilter || f.TargetFilter is SkillTargetSkillDrewCardFilter))
			{
				return true;
			}
		}
		else if (ApplyingTargetFilter is SkillTargetHandFilter || ApplyingTargetFilter is SkillTargetHandOtherSelfFilter || ApplyingTargetFilter is SkillTargetSkillDrewCardFilter)
		{
			return true;
		}
		List<SkillTargetLastTargetFilter> list = new List<SkillTargetLastTargetFilter>();
		if (ApplyAndFilter.Count > 0)
		{
			list = (from f in ApplyAndFilter
				select f.TargetFilter as SkillTargetLastTargetFilter into f
				where f != null
				select f).ToList();
		}
		else if (ApplyingTargetFilter is SkillTargetLastTargetFilter)
		{
			list.Add(ApplyingTargetFilter as SkillTargetLastTargetFilter);
		}
		for (int num = 0; num < list.Count; num++)
		{
			if (list[num] == null)
			{
				continue;
			}
			int lastTargetIndex = list[num].LastTargetIndex;
			bool num2 = ApplyBattlePlayerFilter is SelfBattlePlayerFilter || ApplyingTargetFilter is BothBattlePlayerFilter;
			bool flag = ApplyBattlePlayerFilter is OpponentBattlePlayerFilter || ApplyingTargetFilter is BothBattlePlayerFilter;
			if (num2)
			{
				List<BattleCardBase> lastTargetCardsList = SkillPrm.selfBattlePlayer.GetLastTargetCardsList(lastTargetIndex);
				if (lastTargetCardsList != null && lastTargetCardsList.Any((BattleCardBase c) => SkillPrm.selfBattlePlayer.HandCardList.Any((BattleCardBase cc) => cc.EquelsID(c))))
				{
					return true;
				}
			}
			if (flag)
			{
				List<BattleCardBase> lastTargetCardsList2 = SkillPrm.opponentBattlePlayer.GetLastTargetCardsList(lastTargetIndex);
				if (lastTargetCardsList2 != null && lastTargetCardsList2.Any((BattleCardBase c) => SkillPrm.opponentBattlePlayer.HandCardList.Any((BattleCardBase cc) => cc.EquelsID(c))))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCheckLastTarget()
	{
		if (!(ConditionTargetFilter is SkillTargetLastTargetFilter))
		{
			return ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Lhs.Contains(SkillFilterCreator.ContentKeyword.last_target.ToString()));
		}
		return true;
	}

	public bool IsNeedCheckConditionOnScan()
	{
		if (ConditionFilterCollection.ConditionCheckerFilterList.Count < 1)
		{
			return ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Lhs.Contains(SkillFilterCreator.ContentKeyword.charge_count.ToString()));
		}
		return true;
	}

	public bool IsEnhance()
	{
		if (!IsWhenPlaySkill)
		{
			return false;
		}
		if (ConditionCheckerList.FirstOrDefault((ISkillConditionChecker f) => f is SkillConditionPP && ((f as SkillConditionPP).Operator == ">=" || (f as SkillConditionPP).Operator == ">")) is SkillConditionPP skillConditionPP)
		{
			int pp = skillConditionPP.PpBorder;
			return SkillPrm.ownerCard.NormalSkills.Any((SkillBase s) => s.ConditionCheckerList.Any((ISkillConditionChecker f) => f is SkillConditionPP && (f as SkillConditionPP).PpBorder == pp));
		}
		return false;
	}

	protected virtual bool IsSkipInduction(SkillProcessor.ProcessCallType callType)
	{
		if (callType == SkillProcessor.ProcessCallType.ResidentStop)
		{
			return ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Lhs.Contains("me.hand.count") && f.Rhs == "9");
		}
		return false;
	}

	public void ChacheIsMakeFoil()
	{
		_isMakeFoil = IsMakeFoil;
	}

	public bool IsLastTargetDiscardOrBanishSkill(SkillConditionCheckerOption option, bool isOnlyCheckBanish = false)
	{
		if (option == null)
		{
			return false;
		}
		if (!ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Lhs.Contains(SkillFilterCreator.ContentKeyword.last_target.ToString())))
		{
			return false;
		}
		List<SkillBase> list = new List<SkillBase>(option.ProcessSkillList);
		SkillBase skillBase = list.LastOrDefault();
		if (skillBase == null)
		{
			return false;
		}
		while (skillBase.ApplyingTargetFilter is SkillTargetLastTargetFilter || !skillBase.IsUserSelectType || (!(skillBase.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) && !(skillBase.ApplyBattlePlayerFilter is BothBattlePlayerFilter)))
		{
			list.Remove(skillBase);
			skillBase = list.LastOrDefault();
			if (skillBase == null)
			{
				return false;
			}
		}
		if ((!isOnlyCheckBanish && skillBase is Skill_discard) || skillBase is Skill_banish)
		{
			return true;
		}
		return false;
	}

	public bool IsSameSkill(SkillBase skill)
	{
		bool num = skill.SkillPrm.ownerCard == SkillPrm.ownerCard;
		bool flag = skill.SkillPrm.buildInfo.IsSameSkill(SkillPrm.buildInfo);
		bool flag2 = skill.IsAttachedSkill == IsAttachedSkill;
		bool flag3 = true;
		bool flag4 = skill.IndividualId == IndividualId;
		if (skill.IsAttachedSkill && IsAttachedSkill && skill.GetAttachSkill != null && GetAttachSkill != null)
		{
			flag3 = skill.GetAttachSkill.SkillPrm.ownerCard == GetAttachSkill.SkillPrm.ownerCard;
		}
		return num && flag && flag2 && flag3 && flag4;
	}

	public bool CheckConditionWithoutBurialRite(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay)
	{
		bool result = false;
		List<ISkillConditionChecker> conditionCheckerFilterList = ConditionFilterCollection.ConditionCheckerFilterList.ToList();
		ConditionFilterCollection.ConditionCheckerFilterList = ConditionFilterCollection.ConditionCheckerFilterList.Where((ISkillConditionChecker p) => !(p is SkillPreprocessBurialRite)).ToList();
		bool notCheckBuriaRiteCondition = false;
		if (_executionInfoCreator is NetworkExecutionInfoCreator networkExecutionInfoCreator)
		{
			notCheckBuriaRiteCondition = networkExecutionInfoCreator.IsNotCheckBuriaRiteCondition;
			networkExecutionInfoCreator.SetNotCheckBuriaRiteCondition(value: false);
		}
		if (PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite) && _executionInfoCreator.CheckCondition(playerInfoPair, option, isPrePlay))
		{
			result = true;
		}
		ConditionFilterCollection.ConditionCheckerFilterList = conditionCheckerFilterList;
		if (_executionInfoCreator is NetworkExecutionInfoCreator networkExecutionInfoCreator2)
		{
			networkExecutionInfoCreator2.SetNotCheckBuriaRiteCondition(notCheckBuriaRiteCondition);
		}
		return result;
	}

	public virtual void SetEventAfterReplace(BattleCardBase card, BuffInfo buff)
	{
	}

	private bool IsPreviousGetSideLogText(string text)
	{
		return _previousGetSideLogKeyword.Any(text.Contains);
	}

	public void SetIsOnceCallTiming()
	{
		IsOnceCallTiming = OnWhenPlayStart + OnWhenHandToNotPlayStart + OnWhenDestroyStart + OnWhenReturnStart + OnBeforeAttackStart + OnAfterAttackStart + OnSelfTurnStartStart + OnSelfTurnEndStart + OnOpponentTurnStartStart + OnOpponentTurnEndStart + OnDisCardStart + OnWhenEvolveStart + OnWhenEvolveBeforeStart + OnWhenPlayOtherStart + OnWhenSummonStart + OnInHandStart + OnInHandStopStart + OnWhenFightStart + OnWhenChoicePlayStart + OnWhenChoiceEvolveStart + OnWhenChoiceBrave + OnWhenBanish + OnWhenAccelerate + OnWhenCrystallize + OnWhenLeave + OnWhenDraw + OnWhenFusion + OnAfterFightStart + OnWhenGetOnStart + OnWhenGetOff + OnWhenShortageDeck + OnWhenBattleStart != 0;
	}

	public virtual void SetOnLoseEvent(BattleCardBase target, BuffInfo buff, BuffInfoContainer container)
	{
	}

	protected void CallOnUpdateSkillEffect(List<BattleCardBase> targetCards, bool updateAttackEffect = false)
	{
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsNetworkBattle)
		{
			SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnUpdateSkillEffect(targetCards, updateAttackEffect);
		}
	}

	public void CallOnInactiveSkill()
	{
		this.OnInactiveSkill.Call(this);
	}
}
