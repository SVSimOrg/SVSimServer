using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wizard;

public class AIVirtualCard
{
	public class AIVirtualBanishInfo
	{
		public int Turn { get; private set; }

		public bool IsCardOwnerTurn { get; private set; }

		public BattleCardBase.BanishInfo.BanishPlace Place { get; private set; }

		public AIVirtualBanishInfo(int turn, bool isCardOwnerTurn, BattleCardBase.BanishInfo.BanishPlace place)
		{
			Turn = turn;
			IsCardOwnerTurn = isCardOwnerTurn;
			Place = place;
		}
	}

	protected AIVirtualField _field;

	public bool IsFirstTurn;

	public int BaseCost;

	public int Cost;

	public int RealCost;

	public int PlayedCost = -1;

	public List<int> EnhanceCostList;

	public List<AIAccelerateInformation> AccelerateCostList;

	public List<AICrystalizeInformation> CrystalizeCostList;

	public List<AIChoiceTransformCostInformation> ChoiceTransformCostList;

	public int LastHealAmount;

	public int AttackableCount;

	public bool IsSummonDrunkenness;

	private bool IsDebuffCantEvolve;

	public bool IsCantAttackAll;

	public bool IsSkillCantAttackClass;

	public bool IsSkillCantAttackUnit;

	public bool IsSkillCantAtkUnitNotHasGuard;

	private List<AICannotAttackInformation> _cannotAttackInfoList;

	private bool _isCantUnderAttack;

	public bool IsSkillCantUnderAnyAttack;

	public bool IsUseEvo;

	public bool IsGuard;

	public bool IsIgnoreGuard;

	public bool IsSkillLost;

	public bool IsSelfTurn;

	public bool IsNotAttackYet;

	public bool IsMinimumBreakBonus;

	public bool IsRobbedLastword;

	public bool IsNotConsumeEp;

	public bool IsSkillSummoned;

	public bool HasAnySkill;

	public bool HasSkillNecromance;

	public bool IsPlayer;

	public bool IsForceTargeting;

	public bool IsUnbanishable;

	public bool IsMetamorphosed;

	private int UntouchableCount;

	public AIBarrierInfoCollection BarrierInfoCollection;

	public bool IsIndestructible;

	public bool IsIndependent;

	private int _destroyByBanishCount;

	private int _bounceByBanishCount;

	private int _banishByDestroyCount;

	public int AttackByLifeCount;

	public bool IsSneak;

	private List<AIPlayTag> _giveSneakTagList;

	public bool IsKiller;

	public bool IsDrain;

	public bool IsRush;

	public bool IsQuick;

	public bool IsDestroyWhenAttack;

	public bool HasSpellboost;

	public int SpellboostCount;

	public int RealSpellboostCount;

	private int _originalWhiteRitualCount;

	public bool IsStackWhiteRitual;

	public int TempAtkBuff;

	public int TempLifeBuff;

	public bool HasAfterAttackHeal;

	public bool IsRecoveredAttackableCount;

	public bool IsAmulet;

	public bool IsCountdownAmulet;

	public int ChantCount;

	public bool IsSpell;

	public bool IsUnit;

	public bool IsLeader;

	protected bool _isCompletedRemove;

	public bool IsDead;

	private AIHandPlayEstimator _handPlayEstimator;

	protected List<CardBasePrm.TribeType> _tribeList;

	public readonly AIVirtualAttackInfo AttackLeaderSituation;

	public int GetOnCardId;

	public AIVirtualCard GetOffCard;

	public AIVirtualCard BeforeTransformedCardForSimulation;

	public List<string> PermanentAITribeList;

	public int ReferringSelfCount;

	protected readonly ulong[] PRIME_NUMBERS_FOR_CLAN = new ulong[12]
	{
		282143uL, 282157uL, 282167uL, 282221uL, 282229uL, 282239uL, 282241uL, 282253uL, 282281uL, 282287uL,
		282299uL, 282307uL
	};

	protected readonly ulong[] PRIME_NUMBERS_FOR_TRIBE = new ulong[13]
	{
		47521uL, 47527uL, 47533uL, 47543uL, 47563uL, 47569uL, 47581uL, 47591uL, 47599uL, 47609uL,
		47623uL, 47629uL, 47639uL
	};

	public AIVirtualField SelfField => _field;

	public AICardData AIData { get; private set; }

	public BattlePlayerBase SelfBattlePlayer { get; protected set; }

	public int BaseId
	{
		get
		{
			return CardParameter.BaseId;
		}
		protected set
		{
			CardParameter.BaseId = value;
		}
	}

	public int CardIndex { get; protected set; }

	public string CardName => CardParameter.CardName;

	public int EvolutionAttack => CardParameter.Attack + CardParameter.EvoAttackPlus;

	public int EvolutionLife => CardParameter.Life + CardParameter.EvoLifePlus;

	public CardBasePrm.ClanType Clan { get; protected set; }

	public bool IsEvolution { get; private set; }

	public bool IsPreviousTurnAttacked { get; private set; }

	public bool IsOnField { get; protected set; }

	public bool IsInHand { get; protected set; }

	public BattleCardBase BaseCard { get; protected set; }

	public SkillCollectionBase BattleSkills { get; protected set; }

	public List<string> BattleSkillHashList { get; protected set; }

	public AIVirtualCardParameter BaseParameter { get; private set; }

	public AIVirtualCardParameter OtherEvolveParameter { get; private set; }

	public AIVirtualCardParameter CardParameter
	{
		get
		{
			if (OtherEvolveParameter != null)
			{
				return OtherEvolveParameter;
			}
			return BaseParameter;
		}
	}

	public int Life
	{
		get
		{
			return CardParameter.Life;
		}
		set
		{
			CardParameter.Life = value;
		}
	}

	public int MaxLife
	{
		get
		{
			return CardParameter.MaxLife;
		}
		set
		{
			CardParameter.MaxLife = value;
		}
	}

	public int DefLife
	{
		get
		{
			return CardParameter.DefLife;
		}
		set
		{
			CardParameter.DefLife = value;
		}
	}

	public int LastLife
	{
		get
		{
			return CardParameter.LastLife;
		}
		set
		{
			CardParameter.LastLife = value;
		}
	}

	public int Attack
	{
		get
		{
			return CardParameter.Attack;
		}
		set
		{
			CardParameter.Attack = value;
		}
	}

	public int MaxAttackableCount
	{
		get
		{
			return CardParameter.MaxAttackableCount;
		}
		set
		{
			CardParameter.MaxAttackableCount = value;
		}
	}

	public int DefaultLife
	{
		get
		{
			return CardParameter.DefaultLife;
		}
		set
		{
			CardParameter.DefaultLife = value;
		}
	}

	public int DefaultAttack
	{
		get
		{
			return CardParameter.DefaultAttack;
		}
		set
		{
			CardParameter.DefaultAttack = value;
		}
	}

	public int DefaultMaxAttackableCount
	{
		get
		{
			return CardParameter.DefaultMaxAttackableCount;
		}
		set
		{
			CardParameter.DefaultMaxAttackableCount = value;
		}
	}

	public bool IsAttacked => AttackableCount < MaxAttackableCount;

	public bool IsUntouchable => UntouchableCount > 0;

	public int DestroyedTurn { get; protected set; }

	public int EvoAttackPlus
	{
		get
		{
			return CardParameter.EvoAttackPlus;
		}
		private set
		{
			CardParameter.EvoAttackPlus = value;
		}
	}

	public int EvoLifePlus
	{
		get
		{
			return CardParameter.EvoLifePlus;
		}
		private set
		{
			CardParameter.EvoLifePlus = value;
		}
	}

	public int BuffCount { get; private set; }

	public AIBuffRecorderCollection BuffRecorderCollection { get; private set; }

	public int SelfTurnDamagedCount { get; private set; }

	public int OpponentTurnDamagedCount { get; private set; }

	public int HealCount { get; private set; }

	public bool IsAllShield
	{
		get
		{
			if (BarrierInfoCollection == null)
			{
				return false;
			}
			return BarrierInfoCollection.HasShieldAll;
		}
	}

	public bool IsDestroyByBanish => _destroyByBanishCount > 0;

	public bool IsBounceByBanish => _bounceByBanishCount > 0;

	public bool IsBanishByDestroy => _banishByDestroyCount > 0;

	public bool IsAttackByLife => AttackByLifeCount > 0;

	public bool IsLastword { get; private set; }

	public float Value { get; private set; }

	public float DefaultValue { get; private set; }

	public int WhiteRitualCount { get; protected set; }

	public bool IsAlly { get; protected set; }

	public bool DeadTurn { get; set; }

	public AITagCollectionContainer TagCollectionContainer { get; protected set; }

	public List<int> ChildrenIndexList { get; private set; }

	public bool HasUnionBurst { get; private set; }

	public int UnionBurstCount { get; private set; }

	public bool HasSkyboundArt { get; private set; }

	public int SkyboundArtCount { get; private set; }

	public bool HasSuperSkyboundArt { get; private set; }

	public int SuperSkyboundArtCount { get; private set; }

	public bool IsFusionable { get; private set; }

	public AIVirtualFusionIngredientsInfo FusionIngredients { get; protected set; }

	public BeforeTransformVirtualCard BeforeTransformCard { get; private set; }

	public bool IsGetOn => GetOnCardId > 0;

	public bool CantBeFocusedSkill
	{
		get
		{
			if (!IsSneak)
			{
				return IsUntouchable;
			}
			return true;
		}
	}

	public AIVirtualBanishInfo BanishedInfo { get; private set; }

	public bool IsTribe(CardBasePrm.TribeType tribe)
	{
		if (_tribeList == null)
		{
			return false;
		}
		for (int i = 0; i < _tribeList.Count; i++)
		{
			if (_tribeList[i] == tribe)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyTribe()
	{
		if (_tribeList == null)
		{
			return false;
		}
		for (int i = 0; i < _tribeList.Count; i++)
		{
			if (_tribeList[i] != CardBasePrm.TribeType.ALL)
			{
				return true;
			}
		}
		return false;
	}

	public AIVirtualCard()
	{
	}

	public AIVirtualCard(BattleCardBase card, AIVirtualField field)
	{
		_field = field;
		IsAlly = _field.AI.IsAllyCard(card);
		TagCollectionContainer = new AITagCollectionContainer();
		BarrierInfoCollection = new AIBarrierInfoCollection();
		InitializeFromBattleCardBase(card);
		InitializeAIParameter();
		if (IsAlly && !IsLeader)
		{
			AttackLeaderSituation = new AIVirtualAttackInfo(this, _field.EnemyClass);
		}
		else
		{
			AttackLeaderSituation = null;
		}
	}

	public AIVirtualCard(AIVirtualCard virtualCard, AIVirtualField field)
	{
		_field = field;
		SelfBattlePlayer = virtualCard.SelfBattlePlayer;
		AIData = virtualCard.AIData;
		IsAlly = virtualCard.IsAlly;
		BaseCost = virtualCard.BaseCost;
		Cost = virtualCard.Cost;
		RealCost = virtualCard.RealCost;
		PlayedCost = virtualCard.PlayedCost;
		IsDead = virtualCard.IsDead;
		_isCompletedRemove = virtualCard._isCompletedRemove;
		ChildrenIndexList = virtualCard.ChildrenIndexList;
		IsPlayer = virtualCard.IsPlayer;
		BaseCard = virtualCard.BaseCard;
		CardIndex = virtualCard.CardIndex;
		BattleSkills = virtualCard.BattleSkills;
		BattleSkillHashList = virtualCard.BattleSkillHashList;
		BaseParameter = virtualCard.BaseParameter.Clone();
		OtherEvolveParameter = ((virtualCard.OtherEvolveParameter != null) ? virtualCard.OtherEvolveParameter.Clone() : null);
		IsSpell = virtualCard.IsSpell;
		IsUnit = virtualCard.IsUnit;
		IsLeader = virtualCard.IsLeader;
		Clan = virtualCard.Clan;
		AttackableCount = virtualCard.AttackableCount;
		IsCantAttackAll = virtualCard.IsCantAttackAll;
		IsFirstTurn = virtualCard.IsFirstTurn;
		IsSummonDrunkenness = virtualCard.IsSummonDrunkenness;
		IsSkillCantAttackClass = virtualCard.IsSkillCantAttackClass;
		IsSkillCantAttackUnit = virtualCard.IsSkillCantAttackUnit;
		_cannotAttackInfoList = AIParamQuery.CloneList(virtualCard._cannotAttackInfoList);
		_isCantUnderAttack = virtualCard._isCantUnderAttack;
		IsSkillCantAtkUnitNotHasGuard = virtualCard.IsSkillCantAtkUnitNotHasGuard;
		IsGuard = virtualCard.IsGuard;
		IsSkillCantUnderAnyAttack = virtualCard.IsSkillCantUnderAnyAttack;
		IsIgnoreGuard = virtualCard.IsIgnoreGuard;
		IsSkillLost = virtualCard.IsSkillLost;
		IsSelfTurn = virtualCard.IsSelfTurn;
		IsNotAttackYet = virtualCard.IsNotAttackYet;
		IsMinimumBreakBonus = virtualCard.IsMinimumBreakBonus;
		IsRobbedLastword = virtualCard.IsRobbedLastword;
		IsNotConsumeEp = virtualCard.IsNotConsumeEp;
		IsSkillSummoned = virtualCard.IsSkillSummoned;
		HasAnySkill = virtualCard.HasAnySkill;
		IsOnField = virtualCard.IsOnField;
		IsInHand = virtualCard.IsInHand;
		IsUseEvo = virtualCard.IsUseEvo;
		IsEvolution = virtualCard.IsEvolution;
		IsPreviousTurnAttacked = virtualCard.IsPreviousTurnAttacked;
		IsIndestructible = virtualCard.IsIndestructible;
		IsIndependent = virtualCard.IsIndependent;
		IsMetamorphosed = virtualCard.IsMetamorphosed;
		IsLastword = virtualCard.IsLastword;
		IsSneak = virtualCard.IsSneak;
		_giveSneakTagList = AIParamQuery.CloneList(virtualCard._giveSneakTagList);
		IsKiller = virtualCard.IsKiller;
		IsDrain = virtualCard.IsDrain;
		IsRush = virtualCard.IsRush;
		IsQuick = virtualCard.IsQuick;
		IsDestroyWhenAttack = virtualCard.IsDestroyWhenAttack;
		IsRecoveredAttackableCount = virtualCard.IsRecoveredAttackableCount;
		IsForceTargeting = virtualCard.IsForceTargeting;
		IsUnbanishable = virtualCard.IsUnbanishable;
		UntouchableCount = virtualCard.UntouchableCount;
		IsDebuffCantEvolve = virtualCard.IsDebuffCantEvolve;
		_destroyByBanishCount = virtualCard._destroyByBanishCount;
		_bounceByBanishCount = virtualCard._bounceByBanishCount;
		_banishByDestroyCount = virtualCard._banishByDestroyCount;
		HasSpellboost = virtualCard.HasSpellboost;
		SpellboostCount = virtualCard.SpellboostCount;
		RealSpellboostCount = virtualCard.RealSpellboostCount;
		WhiteRitualCount = virtualCard.WhiteRitualCount;
		_originalWhiteRitualCount = virtualCard._originalWhiteRitualCount;
		IsStackWhiteRitual = virtualCard.IsStackWhiteRitual;
		AttackByLifeCount = virtualCard.AttackByLifeCount;
		TempAtkBuff = virtualCard.TempAtkBuff;
		TempLifeBuff = virtualCard.TempLifeBuff;
		BuffCount = virtualCard.BuffCount;
		if (virtualCard.BuffRecorderCollection != null)
		{
			BuffRecorderCollection = virtualCard.BuffRecorderCollection.Clone();
		}
		SelfTurnDamagedCount = virtualCard.SelfTurnDamagedCount;
		OpponentTurnDamagedCount = virtualCard.OpponentTurnDamagedCount;
		HealCount = virtualCard.HealCount;
		HasAfterAttackHeal = virtualCard.HasAfterAttackHeal;
		DestroyedTurn = virtualCard.DestroyedTurn;
		ReferringSelfCount = virtualCard.ReferringSelfCount;
		TagCollectionContainer = new AITagCollectionContainer(virtualCard.TagCollectionContainer, this);
		BarrierInfoCollection = virtualCard.BarrierInfoCollection.Clone();
		IsAmulet = virtualCard.IsAmulet;
		IsCountdownAmulet = virtualCard.IsCountdownAmulet;
		ChantCount = virtualCard.ChantCount;
		Value = virtualCard.Value;
		DefaultValue = virtualCard.DefaultValue;
		if (virtualCard._tribeList != null)
		{
			_tribeList = new List<CardBasePrm.TribeType>(virtualCard._tribeList);
		}
		IsFusionable = virtualCard.IsFusionable;
		if (virtualCard.FusionIngredients != null)
		{
			FusionIngredients = new AIVirtualFusionIngredientsInfo(virtualCard.FusionIngredients);
		}
		HasUnionBurst = virtualCard.HasUnionBurst;
		UnionBurstCount = virtualCard.UnionBurstCount;
		HasSkyboundArt = virtualCard.HasSkyboundArt;
		SkyboundArtCount = virtualCard.SkyboundArtCount;
		HasSuperSkyboundArt = virtualCard.HasSuperSkyboundArt;
		SuperSkyboundArtCount = virtualCard.SuperSkyboundArtCount;
		GetOnCardId = virtualCard.GetOnCardId;
		GetOffCard = virtualCard.GetOffCard;
		BeforeTransformCard = null;
		if (virtualCard.BeforeTransformCard != null)
		{
			BeforeTransformCard = new BeforeTransformVirtualCard(virtualCard.BeforeTransformCard, _field);
		}
		if (IsAlly && !IsLeader)
		{
			AttackLeaderSituation = new AIVirtualAttackInfo(this, _field.EnemyClass);
		}
		else
		{
			AttackLeaderSituation = null;
		}
		BanishedInfo = virtualCard.BanishedInfo;
		BeforeTransformedCardForSimulation = virtualCard.BeforeTransformedCardForSimulation;
		PermanentAITribeList = virtualCard.PermanentAITribeList;
	}

	public AIVirtualCard(AIVirtualField field, AIVirtualCardParameter baseParameter, AIVirtualCard evalInstantAttackOwner, bool isEvalRush)
	{
		_field = field;
		BaseParameter = baseParameter;
		OtherEvolveParameter = null;
		IsAlly = evalInstantAttackOwner.IsAlly;
		IsPlayer = evalInstantAttackOwner.IsPlayer;
		SelfBattlePlayer = evalInstantAttackOwner.SelfBattlePlayer;
		AIData = evalInstantAttackOwner.AIData;
		BaseCost = 0;
		Cost = 0;
		RealCost = 0;
		PlayedCost = 0;
		ChildrenIndexList = null;
		BaseCard = evalInstantAttackOwner.BaseCard;
		CardIndex = evalInstantAttackOwner.CardIndex;
		BattleSkills = evalInstantAttackOwner.BattleSkills;
		BattleSkillHashList = evalInstantAttackOwner.BattleSkillHashList;
		IsUnit = true;
		Clan = evalInstantAttackOwner.Clan;
		AttackableCount = BaseParameter.MaxAttackableCount;
		IsFirstTurn = true;
		IsSummonDrunkenness = false;
		IsSelfTurn = evalInstantAttackOwner.IsSelfTurn;
		IsNotAttackYet = true;
		IsMinimumBreakBonus = evalInstantAttackOwner.IsMinimumBreakBonus;
		HasAnySkill = evalInstantAttackOwner.HasAnySkill;
		IsOnField = true;
		IsRush = isEvalRush;
		IsQuick = !isEvalRush;
		_destroyByBanishCount = 0;
		_bounceByBanishCount = 0;
		_banishByDestroyCount = 0;
		SpellboostCount = 0;
		RealSpellboostCount = 0;
		WhiteRitualCount = 0;
		_originalWhiteRitualCount = 0;
		AttackByLifeCount = 0;
		TempAtkBuff = 0;
		TempLifeBuff = 0;
		BuffCount = 0;
		SelfTurnDamagedCount = 0;
		OpponentTurnDamagedCount = 0;
		HealCount = 0;
		DestroyedTurn = evalInstantAttackOwner.DestroyedTurn;
		ReferringSelfCount = 0;
		ChantCount = 0;
		UnionBurstCount = 0;
		SkyboundArtCount = 0;
		SuperSkyboundArtCount = 0;
		TagCollectionContainer = new AITagCollectionContainer(evalInstantAttackOwner.TagCollectionContainer, this);
		BarrierInfoCollection = new AIBarrierInfoCollection();
		BuffRecorderCollection = null;
		if (evalInstantAttackOwner._tribeList != null)
		{
			_tribeList = new List<CardBasePrm.TribeType>(evalInstantAttackOwner._tribeList);
		}
		if (IsAlly && !IsLeader)
		{
			AttackLeaderSituation = new AIVirtualAttackInfo(this, _field.EnemyClass);
		}
		else
		{
			AttackLeaderSituation = null;
		}
	}

	protected virtual void InitializeFromBattleCardBase(BattleCardBase origin)
	{
		InitializeFromBattleCardBaseBasic(origin);
		SelfBattlePlayer = origin.SelfBattlePlayer;
		Value = 0f;
		DefaultValue = 0f;
		CardIndex = origin.Index;
		IsDead = origin.IsDead;
		IsPlayer = origin.IsPlayer;
		BattleSkills = origin.Skills;
		BattleSkillHashList = CardSkillHashUtility.GetSingleSkillHashStringList(BattleSkills);
		IsSkillSummoned = origin.SelfBattlePlayer.SkillSummonedCards != null && origin.SelfBattlePlayer.SkillSummonedCards.Any((BattleCardBase c) => c.Index == origin.Index);
		ISkillApplyInformation skillApplyInformation = origin.SkillApplyInformation;
		Cost = origin.Cost;
		RealCost = origin.Cost;
		PlayedCost = origin.PlayedCost;
		if (origin.Tribe != null && origin.Tribe.Count > 0)
		{
			for (int num = 0; num < origin.Tribe.Count; num++)
			{
				AppendTribe(origin.Tribe[num]);
			}
		}
		IsOnField = origin.IsInplay;
		IsInHand = origin.IsInHand;
		IsSelfTurn = origin.IsSelfTurn;
		IsFirstTurn = origin.IsFirstTurn;
		IsSummonDrunkenness = origin.IsSummonDrunkenness;
		IsCantAttackAll = origin.IsCantAttack;
		IsSkillCantAttackClass = skillApplyInformation.IsSkillCantAtkClass;
		IsSkillCantAttackUnit = skillApplyInformation.IsSkillCantAtkUnit;
		IsSkillCantAtkUnitNotHasGuard = skillApplyInformation.IsSkillCantAtkUnitNotHasGuard;
		IsSkillCantUnderAnyAttack = skillApplyInformation.NotBeAttackedInfoList.Count > 0;
		_isCantUnderAttack = skillApplyInformation.IsSneak || IsSkillCantUnderAnyAttack;
		IsIgnoreGuard = skillApplyInformation.IsIgnoreGuard;
		IsNotConsumeEp = origin.SelfBattlePlayer.CheckNotConsumeEpCard(origin);
		IsEvolution = IsUnit && origin.IsEvolution;
		IsPreviousTurnAttacked = IsUnit && origin.IsPreviousTurnAttacked;
		HasAnySkill = origin.HasAnySkill;
		HasSkillNecromance = origin.HasSkillNecromance;
		IsSkillLost = origin.IsSkillLost;
		IsLastword = origin.Skills._skillTimingInfo.IsWhenDestroy;
		IsSneak = IsUnit && skillApplyInformation.IsSneak;
		IsKiller = IsUnit && skillApplyInformation.IsKiller;
		IsDrain = IsUnit && skillApplyInformation.IsDrain;
		IsRush = IsUnit && skillApplyInformation.IsRush;
		IsQuick = skillApplyInformation.IsQuick;
		IsGuard = IsUnit && skillApplyInformation.IsGuard;
		IsIndestructible = skillApplyInformation.IndestructibleCount > 0;
		IsIndependent = skillApplyInformation.IndependentCount > 0;
		DestroyedTurn = origin.DestroyedTurn;
		UntouchableCount = skillApplyInformation.UntouchableCount;
		IsForceTargeting = skillApplyInformation.IsForceSkillTarget;
		IsUnbanishable = false;
		HasSpellboost = origin.HasSpellCharge;
		SpellboostCount = origin.SpellChargeCount;
		RealSpellboostCount = origin.SpellChargeCount;
		WhiteRitualCount = (IsAmulet ? skillApplyInformation.WhiteRitualCount : 0);
		_originalWhiteRitualCount = WhiteRitualCount;
		IsStackWhiteRitual = origin.HasSkillStackWhiteRitual;
		IsDebuffCantEvolve = skillApplyInformation.CantEvolutionList != null && skillApplyInformation.CantEvolutionList.Any((int f) => (f & Skill_cant_evolution.BIT_FLAG_EPUSE) != 0);
		_banishByDestroyCount = skillApplyInformation.BanishByDestroyCount;
		_destroyByBanishCount = skillApplyInformation.DestroyByBanishCount;
		_bounceByBanishCount = skillApplyInformation.ReturnByBanishCount;
		if (IsCountdownAmulet)
		{
			ChantCount = ((ChantFieldBattleCard)origin).ChantCount;
		}
		AttackableCount = (IsUnit ? origin.AttackableCount : 0);
		BuffCount = (IsUnit ? skillApplyInformation.BuffCount : 0);
		BuffRecorderCollection = (IsUnit ? new AIBuffRecorderCollection(skillApplyInformation.TurnBuffCountList) : null);
		SelfTurnDamagedCount = ((!IsAmulet) ? origin.DamagedCounter.SelfTurnDamage : 0);
		OpponentTurnDamagedCount = ((!IsAmulet) ? origin.DamagedCounter.OpponentTurnDamage : 0);
		HealCount = ((!IsAmulet && skillApplyInformation.HealList != null) ? skillApplyInformation.HealList.Count : 0);
		HasUnionBurst = origin.HasUnionBurst;
		if (HasUnionBurst)
		{
			UnionBurstCount = skillApplyInformation.UnionBurstCount;
		}
		HasSkyboundArt = origin.HasSkyboundArt;
		if (HasSkyboundArt)
		{
			SkyboundArtCount = skillApplyInformation.SkyboundArtCount;
		}
		HasSuperSkyboundArt = origin.HasSuperSkyboundArt;
		if (HasSuperSkyboundArt)
		{
			SuperSkyboundArtCount = skillApplyInformation.SuperSkyboundArtCount;
		}
		BeforeTransformCard = null;
		BattleCardBase.TransformInformation transformInfo = origin.TransformInfo;
		if (transformInfo.Type != BattleCardBase.TransformType.Metamorphose && transformInfo.OriginalCard != null)
		{
			BeforeTransformCard = new BeforeTransformVirtualCard(transformInfo.OriginalCard, _field);
		}
		if (skillApplyInformation.GetOnCards != null && skillApplyInformation.GetOnCards.Count > 0)
		{
			GetOnCardId = skillApplyInformation.GetOnCards[0].BaseParameter.BaseCardId;
		}
		if (origin.GetOffCards != null && origin.GetOffCards.Count > 0)
		{
			GetOffCard = new AIVirtualCard(origin.GetOffCards[0], _field);
		}
		IsFusionable = origin.IsFusionable;
		FusionIngredients = new AIVirtualFusionIngredientsInfo();
		if (skillApplyInformation.FusionIngredients != null && skillApplyInformation.FusionIngredients.Count > 0)
		{
			FusionIngredients.CopyFusionIngredientsFromBattleCard(skillApplyInformation.FusionIngredients, _field);
		}
		ConvertBaseBanishInfo(origin.BanishedInfo);
		BeforeTransformedCardForSimulation = null;
	}

	protected void InitializeFromBattleCardBaseBasic(BattleCardBase origin)
	{
		BaseCard = origin;
		BaseCost = origin.BaseCost;
		Clan = origin.Clan;
		IsUnit = origin.IsUnit;
		IsSpell = origin.IsSpell;
		IsAmulet = origin.IsField;
		IsCountdownAmulet = origin.IsChantField;
		IsLeader = origin.IsClass;
		IsRecoveredAttackableCount = false;
		BaseParameter = new AIVirtualCardParameter(origin);
		OtherEvolveParameter = null;
		PermanentAITribeList = _field.GetReferenceTribe(BaseId);
		int referenceId = _field.GetReferenceId(origin.CardId);
		if (referenceId > 0)
		{
			BaseId = referenceId;
		}
	}

	protected virtual void InitializeAIParameter()
	{
		IsUseEvo = false;
		IsNotAttackYet = !IsAmulet;
		IsMinimumBreakBonus = false;
		IsRobbedLastword = false;
		IsDestroyWhenAttack = false;
		HasAfterAttackHeal = false;
		IsMetamorphosed = false;
		Value = 0f;
		DefaultValue = 0f;
		AttackByLifeCount = 0;
		TempAtkBuff = 0;
		TempLifeBuff = 0;
		ChildrenIndexList = null;
	}

	public void InitializeEnemyHandParameter()
	{
		IsAlly = false;
		IsInHand = true;
		IsOnField = false;
	}

	private void InitializeFollowerTags(AIParamQuery query)
	{
		BattleCardBase baseCard = BaseCard;
		if (IsSkillLost)
		{
			IsRobbedLastword = false;
			IsDestroyWhenAttack = false;
			HasAfterAttackHeal = false;
		}
		else
		{
			IsRobbedLastword = query.IsRobbedLastword(baseCard);
			IsDestroyWhenAttack = query.IsDestroyBeforeAttack(baseCard);
			TagCollectionContainer.InitTags(this, query);
			HasAfterAttackHeal = TagCollectionContainer.HasTag(AIPlayTagType.AfterAttackHeal);
		}
	}

	private void InitializeAmuletTags(AIParamQuery query)
	{
		if (!IsSkillLost)
		{
			TagCollectionContainer.InitTags(this, query);
		}
	}

	public virtual void InitializeTags(AIParamQuery query, AIAttachedTagCollection attachedTagCollection, AIRemovedTagCollection removedTagCollection)
	{
		AIData = query.SearchAICardData(this);
		if (IsAmulet)
		{
			InitializeAmuletTags(query);
		}
		else
		{
			InitializeFollowerTags(query);
		}
		attachedTagCollection?.AttachTagToReceiver(this);
		removedTagCollection?.RemoveTagFromCard(this);
	}

	public void InitAtSummonToken(AIVirtualCard parent, AISituationInfo situation, bool isSkillSummon, bool isUpdateTokenIndex = true)
	{
		AIPreprocessSimulationUtility.SimulatePreprocess(this, situation, _field, AIScriptTokenArgType.WHEN_SUMMON, isPseudo: false);
		IsOnField = true;
		if (isUpdateTokenIndex)
		{
			if (_field.IsLatestActionField)
			{
				if (IsAlly)
				{
					CardIndex = _field.AllyCardTotalNum;
					_field.AllyCardTotalNum++;
				}
				else
				{
					CardIndex = _field.EnemyCardTotalNum;
					_field.EnemyCardTotalNum++;
				}
			}
			else
			{
				CardIndex = _field.TokenIndex;
				_field.TokenIndex--;
			}
		}
		parent.AddChildren(CardIndex);
		AttackableCount = MaxAttackableCount;
		IsFirstTurn = true;
		IsSkillSummoned = isSkillSummon;
		if (IsUseEvo)
		{
			AttackableCount = 1;
			AIAutoEvolutionSimulationUtility.AutoEvolveSingle(this, _field, situation);
		}
	}

	public void PseudoInitAtSummonToken(AIVirtualCard parent, AISituationInfo situation, bool isSkillSummon)
	{
		IsOnField = true;
		AttackableCount = MaxAttackableCount;
		IsFirstTurn = true;
		IsSkillSummoned = isSkillSummon;
		if (IsUseEvo)
		{
			AttackableCount = ((AttackableCount <= 1) ? 1 : AttackableCount);
			AIAutoEvolutionSimulationUtility.AutoEvolveSingle(this, _field, situation);
		}
	}

	public void InitAtDrawToken(AIVirtualCard parent, AISituationInfo situation)
	{
		AIPreprocessSimulationUtility.SimulatePreprocess(this, situation, _field, AIScriptTokenArgType.TOKEN_DRAW, isPseudo: false);
		IsOnField = false;
		IsInHand = true;
		if (_field.IsLatestActionField)
		{
			if (IsAlly)
			{
				CardIndex = _field.AllyCardTotalNum;
				_field.AllyCardTotalNum++;
			}
			else
			{
				CardIndex = _field.EnemyCardTotalNum;
				_field.EnemyCardTotalNum++;
			}
		}
		else
		{
			CardIndex = _field.TokenIndex;
			_field.TokenIndex--;
		}
		parent.AddChildren(CardIndex);
		AddToHand();
	}

	public void InitAtMetamorphose(AIVirtualCard originCard, AIVirtualCard parent)
	{
		IsOnField = true;
		CardIndex = originCard.CardIndex;
		parent.AddChildren(CardIndex);
		AttackableCount = MaxAttackableCount;
		IsFirstTurn = true;
		IsSkillSummoned = false;
	}

	public void InitAtHandMetamorphose(AIVirtualCard originCard, AIVirtualCard parent)
	{
		IsInHand = true;
		CardIndex = originCard.CardIndex;
		parent.AddChildren(CardIndex);
	}

	public void ReplaceSelfField(AIVirtualField field)
	{
		_field = field;
	}

	public void AddChildren(int index)
	{
		if (ChildrenIndexList == null)
		{
			ChildrenIndexList = new List<int>();
		}
		ChildrenIndexList.Add(index);
	}

	public virtual bool IsFollower(List<int> playPtn)
	{
		if (IsUnit)
		{
			if (!this.IsAccelerated(_field, playPtn))
			{
				return !this.IsCrystalize(_field, playPtn);
			}
			return false;
		}
		return false;
	}

	public virtual bool BasePlayable()
	{
		if (!IsAlly || !IsInHand)
		{
			return false;
		}
		return true;
	}

	public bool IsHoldKeywordSkill(AIScriptTokenArgType skill)
	{
		return skill switch
		{
			AIScriptTokenArgType.SNEAK => IsSneak, 
			AIScriptTokenArgType.KILLER => IsKiller, 
			AIScriptTokenArgType.GUARD => IsGuard, 
			AIScriptTokenArgType.RUSH => IsRush, 
			AIScriptTokenArgType.QUICK => IsQuick, 
			AIScriptTokenArgType.DRAIN => IsDrain, 
			AIScriptTokenArgType.UNTOUCHABLE => IsUntouchable, 
			AIScriptTokenArgType.FORCE_TARGETING => IsForceTargeting, 
			AIScriptTokenArgType.UNBANISHABLE => IsUnbanishable, 
			AIScriptTokenArgType.IGNORE_GUARD => IsIgnoreGuard, 
			_ => false, 
		};
	}

	public bool IsAttackable(List<int> playPtn)
	{
		bool flag = AttackableCount > 0 && !IsCantAttackAll && (!IsSummonDrunkenness || IsEvolution);
		if (IsAlly && !flag && IsInHand && playPtn != null && playPtn.Count > 0)
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				if (_field.AllyHandCards[playPtn[i]].IsSameCard(this))
				{
					flag = AISkillSimulationUtility.HasSkill(this, AIScriptTokenArgType.QUICK, _field.AI, playPtn) || AISkillSimulationUtility.HasSkill(this, AIScriptTokenArgType.RUSH, _field.AI, playPtn);
					break;
				}
			}
		}
		return flag;
	}

	public bool IsEvolDrunkenness()
	{
		if (IsFirstTurn && IsSummonDrunkenness)
		{
			return !IsQuick;
		}
		return false;
	}

	public bool IsCantAttackClass()
	{
		if (!IsSkillCantAttackClass && !IsEvolDrunkenness())
		{
			if (IsFirstTurn && IsRush)
			{
				return !IsQuick;
			}
			return false;
		}
		return true;
	}

	public void RecoverAttackableCount()
	{
		AttackableCount = MaxAttackableCount;
		IsRecoveredAttackableCount = true;
	}

	public void ChangeClass(CardBasePrm.ClanType classType)
	{
		if (classType >= CardBasePrm.ClanType.ALL && classType < CardBasePrm.ClanType.MAX)
		{
			Clan = classType;
		}
	}

	public bool IsTribe(AIScriptTokenArgType tribeType)
	{
		CardBasePrm.TribeType tribeType2 = AITribeSimulationUtility.ConvertTokenArgTypeToTribeType(tribeType);
		if (tribeType2 >= CardBasePrm.TribeType.MAX)
		{
			return false;
		}
		return IsTribe(tribeType2);
	}

	public void AppendTribe(CardBasePrm.TribeType tribe)
	{
		if (_tribeList == null)
		{
			_tribeList = new List<CardBasePrm.TribeType>();
		}
		if (!_tribeList.Contains(tribe))
		{
			_tribeList.Add(tribe);
		}
	}

	public void ChangeTribe(CardBasePrm.TribeType tribe)
	{
		if (_tribeList == null)
		{
			_tribeList = new List<CardBasePrm.TribeType>();
		}
		else
		{
			_tribeList.Clear();
		}
		_tribeList.Add(tribe);
	}

	public float UpdateValue(AIParamQuery paramQuery, AIStyleQuery styleQuery, List<int> playPtn, bool doesUseLostLife)
	{
		if (IsDead)
		{
			Value = 0f;
			return Value;
		}
		if (IsUnit)
		{
			if (Life <= 0)
			{
				Value = 0f;
				return Value;
			}
			Value = this.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife, useOthersTag: true, useIgnoreInBattle: true);
			Value += (this.GetAllBreakBonus(playPtn, useIgnoreInBattle: false) + this.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false)) * EnemyAI.BREAKBONUS_RATE_ON_FIELD;
		}
		else if (IsLeader)
		{
			Value = this.GetBattleBonus(playPtn);
		}
		else
		{
			Value = this.GetFieldBonus(playPtn);
		}
		return Value;
	}

	public void SetDefaultValue()
	{
		DefaultValue = Value;
	}

	public int SimulateAttackAmount(AISituationInfo situation)
	{
		return SimulateAttackAmount(Attack, situation);
	}

	public int SimulateAttackAmount(int attack, AISituationInfo situation)
	{
		return _field.DamageModifierCollection.CalcModifiedDamage(_field, EnemyAI.EmptyPlayPtn, situation, this, attack);
	}

	public int SimulateDamageAmount(int damageAmount, bool isSkillDamage = false, bool isSpellDamage = false)
	{
		damageAmount = BarrierInfoCollection.CalcDamageAmount(this, damageAmount, isSkillDamage, isSpellDamage);
		return Math.Max(damageAmount, 0);
	}

	public int SimulateDamageShield(int damageAmount, bool isSkillDamage = false, bool isSpellDamage = false)
	{
		bool flag = !isSkillDamage && !isSpellDamage;
		if ((isSkillDamage && BarrierInfoCollection.HasShieldSkill) || (isSpellDamage && BarrierInfoCollection.HasShieldSpell) || (flag && BarrierInfoCollection.HasShieldAttack))
		{
			return 0;
		}
		return damageAmount;
	}

	public int AddDamage(AISituationInfo situation, int baseDamage, bool isSkillDamage)
	{
		if (IsDead)
		{
			return 0;
		}
		bool isSpellDamage = situation.ActionType == AIOperationType.PLAY && situation.Actor.IsSpell;
		int num = SimulateDamageAmount(baseDamage, isSkillDamage, isSpellDamage);
		Life -= num;
		if (Life > 0)
		{
			LastLife = Life;
			SelfField.DamagedCardsByLastAction.Add(new Tuple<AIVirtualCard, int>(this, num));
		}
		else
		{
			IsDead = true;
		}
		_field.TagPreprocessContainer.SimulateAfterDamageInfo(this);
		_field.ExecuteWhenDamageTags(this, baseDamage, SelfField.BestPlayPtn, situation);
		_field.AllActivateCountHolderIncrement(situation, AIPlayTagType.DamagedActivateCount, this);
		return num;
	}

	public void Heal(int healLife)
	{
		if (!IsDead)
		{
			int life = Life;
			Life = Math.Min(Life + healLife, MaxLife);
			LastHealAmount = Life - life;
			LastLife = Life;
			int turn = (IsAlly ? _field.AllyTurnCount : _field.EnemyTurnCount);
			_field.HealRecorderCollection.AppendHealCount(turn, this, IsAlly);
		}
	}

	private void ModifyAttack(int buff, bool isMultiply, bool isTemp)
	{
		int attack = Attack;
		if (isMultiply)
		{
			Attack *= buff;
		}
		else
		{
			Attack += buff;
		}
		if (isTemp)
		{
			TempAtkBuff += Attack - attack;
		}
	}

	private void ModifyLife(AISituationInfo situation, int buff, bool isMultiply, bool isTemp)
	{
		int life = Life;
		if (isMultiply)
		{
			Life *= buff;
		}
		else
		{
			Life += buff;
		}
		if (Life <= 0)
		{
			RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
		int newLife = (isMultiply ? (MaxLife * buff) : (MaxLife + buff));
		SetMaxLife(situation, newLife);
		if (isTemp && !IsDead)
		{
			TempLifeBuff += Life - life;
		}
		LastLife = Life;
	}

	public void GiveBuff(AISituationInfo situation, AIBuffExecutingInfo_old buff, bool isTemp)
	{
		int attack = Attack;
		int life = Life;
		ModifyAttack(buff.AttackValue, buff.IsMultiplyAttack, isTemp);
		ModifyLife(situation, buff.LifeValue, buff.IsMultiplyLife, isTemp);
		int turn = (IsAlly ? _field.AllyTurnCount : _field.EnemyTurnCount);
		if (BuffRecorderCollection == null)
		{
			BuffRecorderCollection = new AIBuffRecorderCollection();
		}
		int atkBuff = Attack - attack;
		int lifeBuff = Life - life;
		BuffRecorderCollection.AddBuffRecord(turn, IsSelfTurn, atkBuff, lifeBuff);
		if (isTemp)
		{
			AIBuffStopPreprocessOption option = new AIBuffStopPreprocessOption(this);
			_field.TagPreprocessContainer.AppendAllyTurnEndStopInfo(option);
			_field.TagPreprocessContainer.AppendOpponentTurnEndStopInfo(option);
		}
	}

	public void MultiplyAttack(int rate)
	{
		Attack *= rate;
	}

	public void MultiplyLife(int rate)
	{
		Life *= rate;
	}

	public void SetAttack(int newAttack)
	{
		Attack = newAttack;
	}

	public void SetLife(int newLife, AISituationInfo situation)
	{
		Life = newLife;
		MaxLife = newLife;
		TempLifeBuff = 0;
		if (Life <= 0)
		{
			RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
	}

	public void SetMaxLife(AISituationInfo situation, int newLife)
	{
		MaxLife = newLife;
		if (Life > MaxLife)
		{
			Life = MaxLife;
		}
		if (Life <= 0)
		{
			RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
	}

	public void SetNewStatus(AISituationInfo situation, int newAttack, int newLife)
	{
		Attack = newAttack;
		Life = newLife;
		SetMaxLife(situation, newLife);
		LastLife = Life;
		TempAtkBuff = 0;
		TempLifeBuff = 0;
	}

	public void ChantCountDown(AISituationInfo situation, int value)
	{
		ChantCount -= value;
		if (ChantCount <= 0)
		{
			RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
	}

	public int EarthRite(int count, AISituationInfo situation, bool isPseudo)
	{
		int num = Math.Min(WhiteRitualCount, count);
		WhiteRitualCount -= num;
		if (!isPseudo)
		{
			_originalWhiteRitualCount -= num;
		}
		if (WhiteRitualCount <= 0)
		{
			if (isPseudo)
			{
				IsDead = true;
			}
			else
			{
				Destroy(situation, isFromSkill: true);
			}
		}
		return num;
	}

	public void SetWhiteRitual(int count)
	{
		if (count <= 0)
		{
			AIConsoleUtility.LogError($"SetWhiteRitual : Ilegal add count Error [count={count}]");
		}
		else if (!IsDead && IsStackWhiteRitual && WhiteRitualCount > 0)
		{
			WhiteRitualCount = count;
		}
	}

	public bool AddWhiteRitual(int count)
	{
		if (count <= 0)
		{
			AIConsoleUtility.LogError($"AddWhiteRitual : Ilegal add count Error [count={count}]");
			return false;
		}
		if (IsDead || !IsStackWhiteRitual || WhiteRitualCount <= 0)
		{
			return false;
		}
		WhiteRitualCount += count;
		return true;
	}

	public void ResetEarthRite(int resetCount)
	{
		if (WhiteRitualCount <= 0 && IsDead)
		{
			IsDead = false;
		}
		WhiteRitualCount += Mathf.Max(0, resetCount);
	}

	public void ChangeIsCantUnderAttack(bool isCantUnderAttack)
	{
		_isCantUnderAttack = isCantUnderAttack;
	}

	public void RegisterGiveSneakTag(AIPlayTag tag)
	{
		_giveSneakTagList = AIParamQuery.AddElementToList(tag, _giveSneakTagList);
	}

	public void DepriveSneakWithGiveSneakTag()
	{
		IsSneak = false;
		if (_giveSneakTagList != null && _giveSneakTagList.Count > 0)
		{
			for (int i = 0; i < _giveSneakTagList.Count; i++)
			{
				AIPlayTag removingTag = _giveSneakTagList[i];
				TagCollectionContainer.RemoveOneTagWithUpdatingFieldCardList(this, removingTag, SelfField);
			}
			_giveSneakTagList.Clear();
		}
	}

	public bool IsCantUnderAttack(AIParamQuery query, AIVirtualCard attacker, List<int> playPtn, AIVirtualField field)
	{
		if (TagCollectionContainer.HasTag(AIPlayTagType.CantBeAttacked) && TagCollectionContainer.CantBeAttackedTags.CantBeAttacked(this, attacker, playPtn, field))
		{
			return false;
		}
		return _isCantUnderAttack;
	}

	public bool IsCantUnderAnyAttack()
	{
		return _isCantUnderAttack;
	}

	public void AfterClash(AIVirtualCard clashTarget, int finalDamage, bool isAttacker, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		if (!clashTarget.IsIndependent && IsKiller)
		{
			clashTarget.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: true);
		}
		if (isAttacker)
		{
			if (IsSneak)
			{
				DepriveSneakWithGiveSneakTag();
			}
			DealDamageDrain(finalDamage, playPtn, situation);
		}
		if (Life <= 0 && !IsDead)
		{
			RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
	}

	public void DealDamageDrain(int damage, List<int> playPtn, AISituationInfo situation)
	{
		if (IsDrain && damage >= 0)
		{
			AISkillSimulationUtility.HealSingle(IsAlly ? _field.AllyClass : _field.EnemyClass, SelfField, damage, playPtn, situation);
		}
	}

	public void RemoveTempBuff()
	{
		if (TempAtkBuff != 0)
		{
			Attack -= TempAtkBuff;
			TempAtkBuff = 0;
		}
		if (TempLifeBuff != 0)
		{
			Life -= TempLifeBuff;
			LastLife = Life;
			TempLifeBuff = 0;
		}
	}

	public bool IsEqual(BattleCardBase card)
	{
		if (card.Index == CardIndex && _field.AI.IsAllyCard(card) == IsAlly)
		{
			return card.BaseParameter.BaseCardId == BaseId;
		}
		return false;
	}

	public bool IsSameCard(AIVirtualCard card)
	{
		if (card.CardIndex == CardIndex && card.IsAlly == IsAlly)
		{
			return card.BaseParameter.BaseId == BaseParameter.BaseId;
		}
		return false;
	}

	public bool IsSameCardType(AIVirtualCard card)
	{
		if (card.IsUnit == IsUnit && card.IsSpell == IsSpell)
		{
			return (card.IsAmulet || card.IsCountdownAmulet) == (IsAmulet || IsCountdownAmulet);
		}
		return false;
	}

	public bool IsSameCardIncluded(List<AIVirtualCard> list)
	{
		if (list == null || list.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard card = list[i];
			if (IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public void NormalPlay()
	{
		IsOnField = true;
		IsInHand = false;
		IsFirstTurn = true;
		AttackableCount = MaxAttackableCount;
		IsSummonDrunkenness = !IsQuick && !IsRush;
	}

	public void ResetPosition(AIOperationType actionType)
	{
		if (actionType == AIOperationType.PLAY)
		{
			IsInHand = true;
			IsOnField = false;
		}
	}

	public void EvolveStatusUp()
	{
		Life += EvoLifePlus;
		LastLife = Life;
		MaxLife += EvoLifePlus;
		Attack += EvoAttackPlus;
		IsEvolution = true;
	}

	public void AddToHand()
	{
		IsInHand = true;
		IsOnField = false;
		if (IsAlly)
		{
			_field.AllyHandCards.Add(this);
		}
		else
		{
			_field.GetEnemyHandCardList().Add(this);
		}
		_field.CardListSet.AddHandCard(this);
	}

	private void LeaveFromField(AISituationInfo situation)
	{
		IsDead = true;
		IsOnField = false;
		_isCompletedRemove = true;
		if (IsAlly)
		{
			_field.CardListSet.AddAllyLeftCard(this);
			_field.CardListSet.AddAllyLeftCardThisTurn(this);
		}
		else
		{
			_field.CardListSet.AddEnemyLeftCard(this);
			_field.CardListSet.AddEnemyLeftCardThisTurn(this);
		}
		_field.WhenCardLeaveFromField(this, situation);
		GetOff(situation);
	}

	private void ExecuteLeaveSkills(AISituationInfo situation)
	{
		_field.SimulationExtraBonus += (float)(IsAlly ? 1 : (-1)) * this.GetAllLeaveBonus(_field.BestPlayPtn, useIgnoreInBattle: true);
		if (TagCollectionContainer.HasTagCollection(TagCollectionType.WhenLeave))
		{
			TagCollectionContainer.LeaveTags.Execute(this, _field.BestPlayPtn, situation);
		}
		if (!_field.CardListSet.HasLeaveOtherTagHolders)
		{
			return;
		}
		for (int i = 0; i < _field.CardListSet.WhenLeaveOtherTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = _field.CardListSet.WhenLeaveOtherTagHolders[i];
			if (!aIVirtualCard.IsSameCard(this))
			{
				aIVirtualCard.TagCollectionContainer.OtherLeaveTags.RegisterPassedConditionTags(aIVirtualCard, SelfField, SelfField.BestPlayPtn, situation, this);
			}
		}
	}

	private void Destroy(AISituationInfo situation, bool isFromSkill)
	{
		if (_field == null || _isCompletedRemove || (isFromSkill && IsIndestructible))
		{
			return;
		}
		AISkillProcessInformation processInfo = situation.RegisterNewProcessInfo(this, AISituationTriggerInformation.TriggerType.Undefined);
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		situation.RegisterOwnDestroyedCard(this);
		LeaveFromField(situation);
		_field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
		ExecuteLeaveSkills(situation);
		if (IsLeader)
		{
			return;
		}
		_field.VirtualCemetery.AddCemetery(1, IsAlly);
		if (TagCollectionContainer.HasTag(AIPlayTagType.RemoveSkill))
		{
			TagCollectionContainer.RemoveSkillTags.Execute(this, AIScriptTokenArgType.WHEN_DESTROY, null);
		}
		if (this.IsBreakBeforePlay(_field.BestPlayPtn))
		{
			_field.IsBreakBeforePlayKilled = true;
		}
		if (!IsSkillLost && (this.HasBreakBonus(_field) || TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword)))
		{
			if (IsAmulet)
			{
				_field.SimulationExtraBonus += (float)(IsAlly ? 1 : (-1)) * this.GetAllBreakBonus(_field.BestPlayPtn, useIgnoreInBattle: true);
				ExecuteLastwordSkills(processInfo, situation);
			}
			else
			{
				AIVirtualCard aIVirtualCard = (IsAlly ? _field.AllyClass : _field.EnemyClass);
				int num = 1;
				if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.OneMoreLastword) && aIVirtualCard.TagCollectionContainer.OneMoreLastwordTags.CheckConditionAndRemovePassedTags(aIVirtualCard, _field, situation))
				{
					num++;
				}
				_field.SimulationExtraBonus += (float)(IsAlly ? 1 : (-1)) * this.GetBreakBonus(_field.BestPlayPtn, useIgnoreBreak: true) * (float)num;
				_field.SimulationExtraBonus += (float)(IsAlly ? 1 : (-1)) * AIEvaluateBonusFromOhterUtility.GetOtherBreakBonus(this, _field, _field.BestPlayPtn, useIgnoreInBattle: true);
				for (int i = 0; i < num; i++)
				{
					ExecuteLastwordSkills(processInfo, situation);
				}
			}
		}
		if (IsAlly)
		{
			_field.CardListSet.AddAllyDestroyedCard(this, isDestroyTurn: true);
		}
		else
		{
			_field.CardListSet.AddEnemyDestroyedCard(this);
		}
		if (SelfField.CardListSet.HasBreakTagHolder)
		{
			ApplyWhenDestroyTags(situation);
		}
		_field.AllActivateCountHolderIncrement(situation, AIPlayTagType.BreakActivateCount, this);
	}

	private void ExecuteLastwordSkills(AISkillProcessInformation processInfo, AISituationInfo situation)
	{
		AIPreprocessSimulationUtility.SimulatePreprocess(this, situation, _field, AIScriptTokenArgType.WHEN_DESTROY, isPseudo: false);
		if (TagCollectionContainer.HasTagCollection(TagCollectionType.Lastword))
		{
			TagCollectionContainer.LastwordTags.RegisterExecutingTagActions(this, processInfo, situation);
		}
		situation.ProcessCollection.CombinePreprocessToProcessQueue();
	}

	private void Banish(AISituationInfo situation)
	{
		if (!_isCompletedRemove)
		{
			if (situation == null)
			{
				AIConsoleUtility.LogError("AIVirtualCard.Banish() error!! situation == null!!!!!");
			}
			situation.RegisterNewProcessInfo(this, AISituationTriggerInformation.TriggerType.Banish);
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			SetBanishedCardInfo();
			situation.RegisterOwnBanishedCard(this);
			LeaveFromField(situation);
			_field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
			ExecuteLeaveSkills(situation);
			ApplyWhenBanishTags(situation);
			_field.AllActivateCountHolderIncrement(situation, AIPlayTagType.BanishActivateCount, this);
			_field.CardListSet.AddBanishedCard(this);
		}
	}

	private void SetBanishedCardInfo()
	{
		if (_field == null)
		{
			AIConsoleUtility.LogError("SetBanishedCardInfo() Error : Field is Null");
			return;
		}
		int currentTurnCount = _field.CurrentTurnCount;
		BattleCardBase.BanishInfo.BanishPlace place = (IsOnField ? BattleCardBase.BanishInfo.BanishPlace.Field : (IsInHand ? BattleCardBase.BanishInfo.BanishPlace.Hand : BattleCardBase.BanishInfo.BanishPlace.Deck));
		BanishedInfo = new AIVirtualBanishInfo(currentTurnCount, IsSelfTurn, place);
	}

	public bool IsBanishedTargetTurn(int turn)
	{
		if (BanishedInfo == null)
		{
			return false;
		}
		if (BanishedInfo.Place == BattleCardBase.BanishInfo.BanishPlace.None || BanishedInfo.Place == BattleCardBase.BanishInfo.BanishPlace.Deck)
		{
			return false;
		}
		if (BanishedInfo.Turn != turn || BanishedInfo.IsCardOwnerTurn != IsSelfTurn)
		{
			return false;
		}
		return true;
	}

	private void ConvertBaseBanishInfo(BattleCardBase.BanishInfo baseInfo)
	{
		if (baseInfo == null)
		{
			AIConsoleUtility.LogError("ConvertBaseBanishInfo() : convert target is Null");
			return;
		}
		bool isSelfTurn = baseInfo.IsSelfTurn;
		bool isCardOwnerTurn = (IsPlayer ? isSelfTurn : (!isSelfTurn));
		BanishedInfo = new AIVirtualBanishInfo(baseInfo.Turn, isCardOwnerTurn, baseInfo.Place);
	}

	public void MetamorphoseLeave(AISituationInfo situation)
	{
		if (!_isCompletedRemove)
		{
			IsDead = true;
			IsOnField = false;
			LeaveFromField(situation);
		}
	}

	private void Bounce(AISituationInfo situation)
	{
		if (!_isCompletedRemove)
		{
			_field.SimulationExtraBonus += (float)(IsAlly ? 1 : (-1)) * this.GetBounceBonus();
			LeaveFromField(situation);
			ExecuteLeaveSkills(situation);
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			situation.BounceCardList = AIParamQuery.AddElementToList(this, situation.BounceCardList);
			this.ExecuteBounceSkills(this, _field, emptyPlayPtn, situation);
			List<AIVirtualCard> list;
			if (IsAlly)
			{
				list = _field.AllyHandCards;
				_field.AllyInplayCards.Remove(this);
				_field.CardListSet.RemoveAllyInplayCard(this);
			}
			else
			{
				list = _field.GetEnemyHandCardList();
				_field.EnemyInplayCards.Remove(this);
				_field.CardListSet.RemoveEnemyInplayCard(this);
			}
			if (list.Count >= 9)
			{
				_field.VirtualCemetery.AddCemetery(1, IsAlly);
			}
			else
			{
				AIVirtualCard aIVirtualCard = new AIVirtualCard(BaseCard, _field)
				{
					IsInHand = true
				};
				aIVirtualCard.InitializeTags(_field.ParamQuery, null, null);
				list.Add(aIVirtualCard);
				_field.CardListSet.AddHandCard(aIVirtualCard);
			}
			_field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
		}
	}

	public void Reanimate(AISituationInfo situation)
	{
		if (this.IsReanimateEvo(_field.BestPlayPtn))
		{
			AttackableCount = 1;
			AIAutoEvolutionSimulationUtility.AutoEvolveSingle(this, _field, situation);
		}
	}

	public void GetOn(AIVirtualCard rideCard, AISituationInfo situation)
	{
		if (TagCollectionContainer.HasTag(AIPlayTagType.GetOn) && rideCard != null && rideCard.IsAlly == IsAlly && rideCard.IsUnit && !IsGetOn)
		{
			GetOnCardId = rideCard.BaseId;
			rideCard.ToRide();
			IsCantAttackAll = false;
			IsSkillCantAttackUnit = false;
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			_field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
			AIGetOnSimulationUtility.ExecuteGetOnTriggerTags(this, _field, situation);
		}
	}

	public void GetOff(AISituationInfo situation)
	{
		if (IsGetOn)
		{
			GetOffCard = AIGetOnSimulationUtility.GetoffTokenOnVirtualField(GetOnCardId, this, _field, situation);
			List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
			_field.ExecuteWhenChangeInplayTags(emptyPlayPtn, situation);
		}
	}

	public void ToRide()
	{
		IsDead = true;
	}

	public void Spellboost(int count)
	{
		if (IsInHand && HasSpellboost)
		{
			SpellboostCount += count;
		}
	}

	public void SetSpellboostCount(int count)
	{
		SpellboostCount = count;
	}

	public float EvaluateClashBonus()
	{
		if (!TagCollectionContainer.HasTagCollection(TagCollectionType.ClashBonus))
		{
			return 0f;
		}
		return TagCollectionContainer.ClashBonusTags.GetClashBonus(this, SelfField, SelfField.BestPlayPtn);
	}

	private void ApplyWhenDestroyTags(AISituationInfo situation)
	{
		for (int i = 0; i < SelfField.CardListSet.BreakTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = SelfField.CardListSet.BreakTagHolders[i];
			if (!aIVirtualCard.IsDead && (aIVirtualCard.CardIndex != CardIndex || aIVirtualCard.BaseId != BaseId) && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenBreak))
			{
				aIVirtualCard.TagCollectionContainer.BreakTags.RegisterExecutingTagActions(SelfField, aIVirtualCard, this, SelfField.BestPlayPtn, situation);
			}
		}
	}

	private void ApplyWhenBanishTags(AISituationInfo situation)
	{
		if (TagCollectionContainer.HasTagCollection(TagCollectionType.WhenBanish))
		{
			TagCollectionContainer.BanishTags.RegisterExecutingTagActions(SelfField, this, SelfField.BestPlayPtn, situation);
		}
		if (!SelfField.CardListSet.HasBanishTagHolder)
		{
			return;
		}
		for (int i = 0; i < SelfField.CardListSet.BanishTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = SelfField.CardListSet.BanishTagHolders[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSameCard(this) && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherBanish))
			{
				aIVirtualCard.TagCollectionContainer.OtherBanishTags.RegisterExecutingTagActions(SelfField, aIVirtualCard, this, SelfField.BestPlayPtn, situation);
			}
		}
	}

	public void GiveAttackable()
	{
		IsCantAttackAll = false;
		IsSkillCantAttackUnit = false;
		IsSkillCantAttackClass = false;
	}

	public void GiveAttackableCount(int count)
	{
		int maxAttackableCount = MaxAttackableCount;
		if (count > maxAttackableCount)
		{
			AttackableCount = Math.Min(AttackableCount + (count - maxAttackableCount), count);
		}
		else if (count < maxAttackableCount)
		{
			AttackableCount = Math.Min(count, AttackableCount);
		}
		MaxAttackableCount = count;
	}

	public void RemoveAllSkills(AISituationInfo situation)
	{
		DepriveSneakWithGiveSneakTag();
		IsKiller = false;
		IsDrain = false;
		IsRush = false;
		IsQuick = false;
		IsDestroyWhenAttack = false;
		HasSpellboost = false;
		IsForceTargeting = false;
		IsUnbanishable = false;
		UntouchableCount = 0;
		_bounceByBanishCount = 0;
		_banishByDestroyCount = 0;
		_destroyByBanishCount = 0;
		TagCollectionContainer.RemoveAllTagWithUpdatingFieldCardList(this, SelfField, situation);
		IsSkillLost = true;
	}

	public void CallOnAfterBattleSimulation(AIVirtualTurnEndInfo turnEndSituation)
	{
		turnEndSituation.ProcessCollection.ClearTempPreprocessList();
		AIPreprocessSimulationUtility.SimulatePreprocess(this, turnEndSituation, _field, AIScriptTokenArgType.WHEN_TURNEND, isPseudo: false);
		AISkillProcessInformation aISkillProcessInformation = turnEndSituation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined);
		if (TagCollectionContainer.HasTagCollection(TagCollectionType.WhenTurnEnd))
		{
			TurnEndTagCollection turnEndTags = TagCollectionContainer.TurnEndTags;
			if (turnEndTags.HasTagExecuteWhenAllyTurnEnd(turnEndSituation.Actor.IsAlly == IsAlly))
			{
				turnEndTags.RegisterConditionPassedTagActions(this, SelfField.BestPlayPtn, turnEndSituation, aISkillProcessInformation);
			}
		}
		turnEndSituation.ProcessCollection.CombinePreprocessToProcessQueue();
		aISkillProcessInformation.AddExecutingAction(delegate
		{
			if (!turnEndSituation.Actor.IsLeader)
			{
				AIConsoleUtility.LogError("turnEndSituation.Actor is not Leader!!");
			}
			else
			{
				this.Increment(turnEndSituation, AIPlayTagType.TurnEndActivateCount);
			}
		});
	}

	public bool IsAbleEvolution()
	{
		if (!IsUnit)
		{
			return false;
		}
		if (IsEvolution)
		{
			return false;
		}
		if (!_field.IsExceededWaitEvolveTurn)
		{
			return false;
		}
		if (!_field.IsLeftTurnEvol)
		{
			return false;
		}
		if (_field.AllyEvolutionCount <= 0 && !IsNotConsumeEp)
		{
			return false;
		}
		if (IsDebuffCantEvolve)
		{
			return false;
		}
		return true;
	}

	public bool IsAbleToPlay(List<int> playPtn, AIVirtualTargetSelectAction playSituation)
	{
		if (playSituation == null)
		{
			AIConsoleUtility.LogError("IsAbleToPlay() : situation is null");
			return false;
		}
		return AIPlayCardSimulationUtility.IsAbleToPlayCard(playSituation, _field, playPtn);
	}

	public int GetUseCost(int restPp, List<int> playPtn, AISituationInfo situation, out PlaySimulationType playType)
	{
		int result = GetPlayEnhancedCost(restPp, out playType);
		if (playType == PlaySimulationType.Undefined)
		{
			result = GetPlayChoiceTransformedCost(restPp, playPtn, situation, out playType);
		}
		if (playType == PlaySimulationType.Undefined)
		{
			result = GetPlayNormalCost(restPp, playPtn, situation, out playType);
		}
		if (playType == PlaySimulationType.Undefined)
		{
			result = GetPlayAcceleratedCost(restPp, playPtn, situation, out playType);
		}
		if (playType == PlaySimulationType.Undefined)
		{
			result = GetPlayCrystalizedCost(restPp, out playType);
		}
		if (situation is AIVirtualActionInfo aIVirtualActionInfo)
		{
			aIVirtualActionInfo.ReservedPlayType = playType;
		}
		return result;
	}

	public bool IsPlayingSimulationType(int restPp, List<int> playPtn, AISituationInfo situation, PlaySimulationType checkPlayType)
	{
		PlaySimulationType playType = PlaySimulationType.Undefined;
		switch (checkPlayType)
		{
		case PlaySimulationType.Accelerate:
			GetPlayNormalCost(restPp, playPtn, situation, out playType);
			if (playType != PlaySimulationType.Normal)
			{
				GetPlayAcceleratedCost(restPp, playPtn, situation, out playType);
			}
			break;
		case PlaySimulationType.ChoiceTransform:
			GetPlayChoiceTransformedCost(restPp, playPtn, null, out playType);
			break;
		case PlaySimulationType.Crystalize:
			GetPlayNormalCost(restPp, playPtn, situation, out playType);
			if (playType != PlaySimulationType.Normal)
			{
				GetPlayCrystalizedCost(restPp, out playType);
			}
			break;
		case PlaySimulationType.Enhance:
			GetPlayEnhancedCost(restPp, out playType);
			break;
		case PlaySimulationType.Normal:
			GetPlayNormalCost(restPp, playPtn, situation, out playType);
			break;
		default:
			AIConsoleUtility.LogError("IsPlayingSimulationType() : Check Target is Undefined Play Type!");
			break;
		}
		return playType == checkPlayType;
	}

	public int GetPlaySimulationTypeCost(int restPp, List<int> playPtn, AISituationInfo situation, PlaySimulationType checkPlayType)
	{
		PlaySimulationType playType = PlaySimulationType.Undefined;
		int result = -1;
		switch (checkPlayType)
		{
		case PlaySimulationType.Accelerate:
			result = GetPlayNormalCost(restPp, playPtn, situation, out playType);
			if (playType != PlaySimulationType.Normal)
			{
				result = GetPlayAcceleratedCost(restPp, playPtn, situation, out playType);
			}
			break;
		case PlaySimulationType.ChoiceTransform:
			result = GetPlayChoiceTransformedCost(restPp, playPtn, null, out playType);
			break;
		case PlaySimulationType.Crystalize:
			result = GetPlayNormalCost(restPp, playPtn, situation, out playType);
			if (playType != PlaySimulationType.Normal)
			{
				result = GetPlayCrystalizedCost(restPp, out playType);
			}
			break;
		case PlaySimulationType.Enhance:
			result = GetPlayEnhancedCost(restPp, out playType);
			break;
		case PlaySimulationType.Normal:
			result = GetPlayNormalCost(restPp, playPtn, situation, out playType);
			break;
		default:
			AIConsoleUtility.LogError("GetPlaySimulationTypeCost() : Check Target is Undefined Play Type!");
			break;
		}
		if (playType != checkPlayType)
		{
			return -1;
		}
		return result;
	}

	public int GetPlayNormalCost(int restPp, List<int> playPtn, AISituationInfo situation, out PlaySimulationType playType)
	{
		playType = PlaySimulationType.Undefined;
		int num = Mathf.Max(0, Cost - this.GetCostBonus(playPtn, situation));
		if (restPp >= num)
		{
			playType = PlaySimulationType.Normal;
		}
		return num;
	}

	public int GetPlayEnhancedCost(int restPp, out PlaySimulationType playType)
	{
		playType = PlaySimulationType.Undefined;
		int num = -1;
		if (EnhanceCostList != null && EnhanceCostList.Count > 0)
		{
			for (int i = 0; i < EnhanceCostList.Count; i++)
			{
				int num2 = EnhanceCostList[i];
				if (restPp >= num2 && num2 > num)
				{
					num = num2;
				}
			}
			if (num >= 0)
			{
				playType = PlaySimulationType.Enhance;
			}
		}
		return num;
	}

	public int GetPlayChoiceTransformedCost(int restPp, List<int> playPtn, AISituationInfo situation, out PlaySimulationType playType)
	{
		playType = PlaySimulationType.Undefined;
		int num = -1;
		if (ChoiceTransformCostList != null && ChoiceTransformCostList.Count > 0)
		{
			for (int i = 0; i < ChoiceTransformCostList.Count; i++)
			{
				AIChoiceTransformCostInformation aIChoiceTransformCostInformation = ChoiceTransformCostList[i];
				if (aIChoiceTransformCostInformation.CheckCondition(this, _field, playPtn, situation))
				{
					int num2 = (int)aIChoiceTransformCostInformation.Cost.EvalArg(this, playPtn, _field, situation);
					if (restPp >= num2 && num2 > num)
					{
						num = num2;
					}
				}
			}
			if (num >= 0)
			{
				playType = PlaySimulationType.ChoiceTransform;
			}
		}
		return num;
	}

	public int GetPlayAcceleratedCost(int restPp, List<int> playPtn, AISituationInfo situation, out PlaySimulationType playType)
	{
		playType = PlaySimulationType.Undefined;
		int num = -1;
		if (AccelerateCostList != null && AccelerateCostList.Count > 0)
		{
			for (int i = 0; i < AccelerateCostList.Count; i++)
			{
				AIAccelerateInformation aIAccelerateInformation = AccelerateCostList[i];
				if (aIAccelerateInformation.CheckCondition(this, _field, playPtn, situation))
				{
					int cost = aIAccelerateInformation.Cost;
					if (restPp >= cost && cost > num)
					{
						num = cost;
					}
				}
			}
			if (num >= 0)
			{
				playType = PlaySimulationType.Accelerate;
			}
		}
		return num;
	}

	public int GetPlayCrystalizedCost(int restPp, out PlaySimulationType playType)
	{
		playType = PlaySimulationType.Undefined;
		int num = -1;
		if (CrystalizeCostList != null && CrystalizeCostList.Count > 0)
		{
			for (int i = 0; i < CrystalizeCostList.Count; i++)
			{
				AICrystalizeInformation aICrystalizeInformation = CrystalizeCostList[i];
				if (restPp >= aICrystalizeInformation.Cost && aICrystalizeInformation.Cost > num)
				{
					num = aICrystalizeInformation.Cost;
				}
			}
			if (num >= 0)
			{
				playType = PlaySimulationType.Crystalize;
			}
		}
		return num;
	}

	public void SetCurrentCost(int target_cost)
	{
		if (target_cost < 0)
		{
			AIConsoleUtility.LogError($"SetCurrentCost: Set ilegal values [{target_cost}]");
		}
		Cost = target_cost;
	}

	public void AddCurrentCost(int add_cost)
	{
		Cost = Mathf.Max(0, Cost + add_cost);
	}

	public void CreateHandPlayEstimator(AIParamQuery paramQuery, int handIndex, BattlePlayerPair sourcePair, EnemyAI ai)
	{
		_handPlayEstimator = new AIHandPlayEstimator(paramQuery, handIndex, this, sourcePair, ai);
	}

	public AIHandPlayEstimator GetHandPlayEstimator()
	{
		return _handPlayEstimator;
	}

	public int GetBaseSkillCount()
	{
		return BattleSkills.Count();
	}

	public bool IsHoldingBattleSkill(string hash)
	{
		if (BattleSkillHashList == null || BattleSkillHashList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < BattleSkillHashList.Count; i++)
		{
			if (hash == BattleSkillHashList[i].ToString())
			{
				return true;
			}
		}
		return false;
	}

	public void RollBackFromOneRecord(AIVirtualFieldRollBackRecord.CardParamRecord record)
	{
		Cost = record.Cost;
		SpellboostCount = record.SpellBoost;
		IsDead = record.IsDead;
		WhiteRitualCount = record.WhiteRitualCount;
		if (TagCollectionContainer.HasTagCollection(TagCollectionType.ActivateCount))
		{
			TagCollectionContainer.ActivateCountTags.UpdateCounterList(record.ActivateCounterList);
		}
	}

	public void NotBeAttacked()
	{
		IsSkillCantUnderAnyAttack = true;
	}

	public float EvalKillerAttackValue(List<int> playPtn, AISituationInfo situation)
	{
		float num = (IsUnit ? (0f - this.EvaluateValueOnField(playPtn, situation, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) + this.EvaluateBreakValue(playPtn, useIgnoreBreak: true) + this.EvaluateLeaveValue(playPtn, useIgnoreInBattle: true)) : 0f);
		float num2 = float.MinValue;
		AIVirtualCard aIVirtualCard = null;
		bool flag = _field.EnemyInplayCards.Any((AIVirtualCard card) => card.IsGuard);
		for (int num3 = 0; num3 < _field.EnemyInplayCards.Count; num3++)
		{
			float num4 = 0f;
			AIVirtualCard aIVirtualCard2 = _field.EnemyInplayCards[num3];
			if (!aIVirtualCard2.IsUnit || aIVirtualCard2.IsCantUnderAnyAttack() || (flag && !aIVirtualCard2.IsGuard && !IsIgnoreGuard) || new AIVirtualAttackInfo(this, aIVirtualCard2).WillTargetDestroyByAttackTags(_field, playPtn, this))
			{
				continue;
			}
			bool flag2 = true;
			if (aIVirtualCard2.IsIndependent || aIVirtualCard2.IsIndestructible)
			{
				int damage = BaseCard.DamageCalculationAtkTypeAttack.Damage;
				if (aIVirtualCard2.BaseCard.CalculateFinalDamageAmount(damage) < aIVirtualCard2.Life)
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				float num5 = aIVirtualCard2.EvaluateValueOnField(EnemyAI.EmptyPlayPtn, null, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) - aIVirtualCard2.EvaluateBreakValue(EnemyAI.EmptyPlayPtn, useIgnoreBreak: true) - aIVirtualCard2.EvaluateLeaveValue(EnemyAI.EmptyPlayPtn, useIgnoreInBattle: true);
				num4 += num5;
			}
			int damage2 = aIVirtualCard2.BaseCard.DamageCalculationAtkTypeBeAttacked.Damage;
			if (BaseCard.CalculateFinalDamageAmount(damage2) >= Life)
			{
				num4 += num;
			}
			if (num4 > num2)
			{
				num2 = num4;
				aIVirtualCard = aIVirtualCard2;
			}
		}
		if (aIVirtualCard != null)
		{
			return num2;
		}
		return 0f;
	}

	public void GiveRemoveByBanish()
	{
		_destroyByBanishCount++;
		_bounceByBanishCount++;
	}

	public void DepriveRemoveByBanish()
	{
		if (_destroyByBanishCount > 0)
		{
			_destroyByBanishCount--;
		}
		if (_bounceByBanishCount > 0)
		{
			_bounceByBanishCount--;
		}
	}

	public void GiveRemoveByDestroy()
	{
		_banishByDestroyCount++;
	}

	public void DepriveRemoveByDestroy()
	{
		if (_banishByDestroyCount > 0)
		{
			_banishByDestroyCount--;
		}
	}

	public void AddUntouchableCount()
	{
		UntouchableCount++;
	}

	public void SubUntouchableCount()
	{
		UntouchableCount--;
		if (UntouchableCount < 0)
		{
			UntouchableCount = 0;
		}
	}

	public void RemoveCard(AISituationInfo situation, AIRemovalType type, bool isFromSkill)
	{
		switch (type)
		{
		case AIRemovalType.Destroy:
			TryDestroyCard(situation, isFromSkill);
			break;
		case AIRemovalType.Banish:
			TryBanishCard(situation);
			break;
		case AIRemovalType.Bounce:
			TryBounceCard(situation);
			break;
		}
		RemoveTempBuff();
	}

	private void TryDestroyCard(AISituationInfo situation, bool isFromSkill)
	{
		if (IsDestroyByBanish)
		{
			if (IsBanishByDestroy || this.IsRemoveByDestroy(EnemyAI.EmptyPlayPtn, situation))
			{
				Destroy(situation, isFromSkill);
			}
			else if (!isFromSkill || !IsIndestructible)
			{
				Banish(situation);
			}
		}
		else
		{
			Destroy(situation, isFromSkill);
		}
	}

	private void TryBanishCard(AISituationInfo situation)
	{
		if (IsBanishByDestroy || this.IsRemoveByDestroy(EnemyAI.EmptyPlayPtn, situation))
		{
			Destroy(situation, isFromSkill: true);
		}
		else
		{
			Banish(situation);
		}
	}

	private void TryBounceCard(AISituationInfo situation)
	{
		if (IsBounceByBanish)
		{
			if (IsBanishByDestroy || this.IsRemoveByDestroy(EnemyAI.EmptyPlayPtn, situation))
			{
				if (IsIndestructible)
				{
					Bounce(situation);
				}
				else
				{
					Destroy(situation, isFromSkill: true);
				}
			}
			else
			{
				Banish(situation);
			}
		}
		else
		{
			Bounce(situation);
		}
	}

	public bool CreateOtherEvolveParameterFromBattleCardBase(EnemyAI ai, BattleCardBase origin, BattleCardBase baseParamCard, AISituationInfo situation)
	{
		if (origin == null)
		{
			AIConsoleUtility.LogError("CreateOtherEvolvedCardParam() error!! param origin card is null");
			return false;
		}
		if (baseParamCard == null)
		{
			AIConsoleUtility.LogError("CreateOtherEvolvedCardParam() error!! new param card is null!");
			return false;
		}
		if (OtherEvolveParameter != null)
		{
			AIConsoleUtility.LogError("SetOtherEvolvedCardParam() error!! exist param data already!");
			return false;
		}
		OtherEvolveParameter = new AIVirtualCardParameter(origin, baseParamCard, BuffRecorderCollection);
		AIAttachedTagCollection attachedTags = TagCollectionContainer.AttachedTags;
		AIRemovedTagCollection removedTagCollection = TagCollectionContainer.RemovedTagCollection;
		TagCollectionContainer.RemoveAllTagWithUpdatingFieldCardList(this, SelfField, situation);
		InitializeTags(ai.ParamQuery, attachedTags, removedTagCollection);
		return true;
	}

	public void SetOtherEvolveParameterFromVirtualCard(AIVirtualCard source)
	{
		if (source.OtherEvolveParameter != null)
		{
			OtherEvolveParameter = source.OtherEvolveParameter;
			BaseParameter = source.BaseParameter;
		}
	}

	public void SetOtherEvolveParameterFromBuildParameter(ReferableVirtualCardBuildParameterCollection buildParameter)
	{
		if (buildParameter.OtherEvolveCardParameter != null)
		{
			OtherEvolveParameter = buildParameter.OtherEvolveCardParameter;
			BaseParameter = buildParameter.BaseParameter;
		}
	}

	public void AddCannotAttackInformation(AICannotAttackInformation info)
	{
		_cannotAttackInfoList = AIParamQuery.AddElementToList(info, _cannotAttackInfoList);
	}

	public void CopyCannotAttackInfoList(AIVirtualCard sourceCard)
	{
		_cannotAttackInfoList = AIParamQuery.CloneList(sourceCard._cannotAttackInfoList);
	}

	public void RemoveCannotAttackInformation(AICannotAttackInformation info)
	{
		if (_cannotAttackInfoList == null || _cannotAttackInfoList.Count <= 0)
		{
			return;
		}
		for (int num = _cannotAttackInfoList.Count - 1; num >= 0; num--)
		{
			if (_cannotAttackInfoList[num].IsEqual(info))
			{
				_cannotAttackInfoList.RemoveAt(num);
				break;
			}
		}
	}

	public bool IsCannotAttackByTag(AIVirtualAttackInfo situation)
	{
		if (_cannotAttackInfoList == null || _cannotAttackInfoList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < _cannotAttackInfoList.Count; i++)
		{
			if (_cannotAttackInfoList[i].IsCannotAttack(_field, situation))
			{
				return true;
			}
		}
		return false;
	}

	public virtual ulong GetHash()
	{
		if (IsDead && !IsLeader)
		{
			return 0uL;
		}
		ulong num = 0uL;
		num += (ulong)((long)Attack * 6337L);
		num += (ulong)((long)Life * 11383L);
		num += (ulong)((long)DefLife * 173L);
		num += (ulong)((long)EvolutionAttack * 1488017L);
		num += (ulong)((long)EvolutionLife * 937477L);
		num += (ulong)((long)Cost * 14401L);
		num += PRIME_NUMBERS_FOR_CLAN[(int)Clan % PRIME_NUMBERS_FOR_CLAN.Length];
		num += (ulong)((long)SpellboostCount * 8389L);
		num += (ulong)((long)WhiteRitualCount * 3571L);
		if (_tribeList != null)
		{
			for (int i = 0; i < _tribeList.Count; i++)
			{
				num += PRIME_NUMBERS_FOR_TRIBE[(int)_tribeList[i] % PRIME_NUMBERS_FOR_TRIBE.Length];
			}
		}
		num += TagCollectionContainer.GetHash(this) * 5237;
		bool flag = false;
		if (!IsLeader && AIData != null)
		{
			flag = true;
		}
		if (FusionIngredients != null)
		{
			num += FusionIngredients.GetHash();
		}
		if (IsOnField)
		{
			num += (ulong)((long)AttackableCount * 557L);
			num += (ulong)((long)ChantCount * 5683L);
			num += (ulong)((long)ReferringSelfCount * 558869L);
			num += BarrierInfoCollection.GetHash();
			if (flag)
			{
				num += (ulong)((long)AIData.BattleBonusExpr.Hash * 761L);
			}
			if (IsAlly)
			{
				num += (ulong)(IsFirstTurn ? 191 : 19);
				if (IsAttackable(EnemyAI.EmptyPlayPtn))
				{
					num = ((!AIAttackSimulationUtility.IsAttackPossible(_field, AttackLeaderSituation)) ? (num * 37) : (num * 151));
				}
				num *= 11;
			}
			num *= (ulong)(IsUnit ? 103 : 1);
			num *= (ulong)(IsEvolution ? 59 : 1);
			num *= (ulong)(IsGetOn ? 577 : 1);
			num *= (ulong)(IsPreviousTurnAttacked ? 9749 : 1);
			num *= (ulong)(IsCantAttackAll ? 8089 : 1);
			num *= (ulong)(IsNotConsumeEp ? 3527 : 1);
			num *= (ulong)(IsLastword ? 809 : 1);
			num *= (ulong)(IsSneak ? 811 : 1);
			num *= (ulong)(IsKiller ? 821 : 1);
			num *= (ulong)(IsDrain ? 823 : 1);
			num *= (ulong)(IsRush ? 827 : 1);
			num *= (ulong)(IsQuick ? 829 : 1);
			num *= (ulong)(IsDestroyWhenAttack ? 839 : 1);
			num *= (ulong)(IsGuard ? 853 : 1);
			num *= (ulong)(IsIgnoreGuard ? 857 : 1);
			num *= (ulong)(IsUntouchable ? 859 : 1);
			num *= (ulong)(IsForceTargeting ? 863 : 1);
			num *= (ulong)(IsUnbanishable ? 877 : 1);
		}
		if (IsInHand && flag)
		{
			num += (ulong)((long)AIData.PlayBonusExpr.Hash * 12007L);
			num += (ulong)((long)AIData.BattleBonusExpr.Hash * 761L);
			num += (ulong)((long)AIData.PriorityExpr.Hash * 101L);
		}
		return num;
	}
}
