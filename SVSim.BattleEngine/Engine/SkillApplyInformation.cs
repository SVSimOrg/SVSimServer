using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard.Battle;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;

public class SkillApplyInformation : ISkillApplyInformation
{
	protected BattleCardBase _card;


	protected BattlePlayerBase Player => _card.SelfBattlePlayer;

	protected BattlePlayerBase Enemy => _card.OpponentBattlePlayer;

	public List<CantPlayCardFilterInfo> CantPlayFilterList { get; protected set; }

	public int BuffCount { get; protected set; }

	public int BuffLifeCount { get; protected set; }

	public List<BuffCountInfo> TurnBuffCountList { get; protected set; }

	public bool IsBuff { get; protected set; }

	public int DebuffCount { get; protected set; }

	public bool IsDebuff { get; protected set; }

	public List<GuardInfo> GuardInfo { get; protected set; }

	public bool IsGuard { get; protected set; }

	public int DrainCount { get; protected set; }

	public bool IsDrain { get; protected set; }

	public int KillerCount { get; protected set; }

	public bool IsKiller { get; protected set; }

	public List<ShieldInfo> ShieldInfos { get; protected set; }

	public bool IsShieldAll { get; protected set; }

	public bool IsShieldSkill { get; protected set; }

	public bool IsShieldSpell { get; protected set; }

	public bool IsShieldAttack { get; protected set; }

	public int QuickCount { get; protected set; }

	public bool IsQuick { get; protected set; }

	public List<RushInfo> RushInfo { get; protected set; }

	public bool IsRush { get; protected set; }

	public int SneakCount { get; protected set; }

	public bool IsSneak => SneakCount > 0;

	public int DamageCutCount { get; protected set; }

	public bool IsDamageCut { get; protected set; }

	public int NotBeAttackedCount { get; protected set; }

	public int UntouchableCount { get; protected set; }

	public bool IsUntouchable { get; protected set; }

	public int UntouchableBySpellCount { get; protected set; }

	public bool IsUntouchableBySpell { get; protected set; }

	public int IgnoreGuardCount { get; protected set; }

	public bool IsIgnoreGuard { get; protected set; }

	public int AttackByLifeTypeBeAttackedCount { get; protected set; }

	public bool IsAttackByLifeTypeBeAttacked { get; protected set; }

	public int AttackByLifeTypeAttackCount { get; protected set; }

	public bool IsAttackByLifeTypeAttack { get; protected set; }

	public int SkillCantAtkClassCount { get; protected set; }

	public bool IsSkillCantAtkClass { get; protected set; }

	public int SkillCantAtkUnitCount { get; protected set; }

	public bool IsSkillCantAtkUnit { get; protected set; }

	public int SkillCantAtkUnitNotHasGuardCount { get; protected set; }

	public bool IsSkillCantAtkUnitNotHasGuard { get; protected set; }

	public int SkillCantAtkUnitBaseCardIdCount { get; protected set; }

	public bool IsSkillCantAtkUnitBaseCardId { get; protected set; }

	public List<int> CantAtkUnitBaseCardIdList { get; protected set; }

	public bool IsSkillCantAtkAll
	{
		get
		{
			if (IsSkillCantAtkClass)
			{
				return IsSkillCantAtkUnit;
			}
			return false;
		}
	}

	public int ReflectionClassCount { get; protected set; }

	public bool IsReflectionClass { get; protected set; }

	public int ReflectionDamageOwnerCount { get; protected set; }

	public bool IsReflectionDamageOwner { get; protected set; }

	public int InfiniteAttackCount { get; protected set; }

	public bool IsInfiniteAttack { get; protected set; }

	public int IndestructibleCount { get; protected set; }

	public bool IsIndestructible { get; protected set; }

	public int ForceBerserkCount { get; protected set; }

	public bool IsForceBerserk { get; protected set; }

	public int ForceAvariceCount { get; protected set; }

	public bool IsForceAvarice { get; protected set; }

	public int ForceWrathCount { get; protected set; }

	public bool IsForceWrath { get; protected set; }

	public int CantActivateFanfareUnitCount { get; protected set; }

	public bool IsCantActivateFanfareUnit { get; protected set; }

	public int CantActivateFanfareFieldCount { get; protected set; }

	public bool IsCantActivateFanfareField { get; protected set; }

	public int CantActivateShortageDeckWinCount { get; protected set; }

	public bool IsCantActivateShortageDeckWin { get; protected set; }

	public int ForceSkillTargetCount { get; protected set; }

	public bool IsForceSkillTarget { get; protected set; }

	public int AttractSkillTargetCount { get; protected set; }

	public bool IsAttractSkillTarget { get; protected set; }

	public int IndependentCount { get; protected set; }

	public bool IsIndependent { get; protected set; }

	public int NotBeDebuffedCount { get; protected set; }

	public bool IsNotBeDebuffed { get; protected set; }

	public int ForceAttackUnitCount { get; protected set; }

	public bool IsForceAttackUnit { get; protected set; }

	public int SkillRandomCount { get; protected set; }

	public int[] SkillRandomArray { get; protected set; }

	public List<DamageCutInfo> DamageCutList { get; protected set; }

	public List<ReflectionInfo> ReflectionInfoList { get; protected set; }

	public int TurnStartFixedPPCount { get; protected set; }

	public bool IsTurnStartFixedPP { get; protected set; }

	public int TriggerCount { get; protected set; }

	public bool IsTrigger { get; protected set; }

	public bool IsNotConsumeEp { get; protected set; }

	public int ShortageDeckWinCount { get; protected set; }

	public bool IsShortageDeckWin { get; protected set; }

	public int ReturnByBanishCount { get; protected set; }

	public bool IsReturnByBanish { get; protected set; }

	public int DestroyByBanishCount { get; protected set; }

	public bool IsDestroyByBanish { get; protected set; }

	public int BanishByDestroyCount { get; protected set; }

	public bool IsBanishByDestroy { get; protected set; }

	public List<DamageClippingInfo> DamageMaxClippingInfo { get; set; }

	public List<CardBasePrm.ClanType> ClanSkinInfo { get; set; }

	public List<CardBasePrm.TribeInfo> TribeSkinInfo { get; set; }

	public List<ICardOffenseModifier> OffenseModifierList { get; protected set; }

	public List<ICardLifeModifier> LifeModifierList { get; protected set; }

	public List<CausedDamageCardParameterModifier> CausedDamageModifierList { get; protected set; }

	public List<ICardChantCountModifier> ChantCountModifierList { get; protected set; }

	public int NotDecreasePPCounter { get; protected set; }

	public List<DamageCardParameterModifier> DamageList => LifeModifierList.Where((ICardLifeModifier l) => l is DamageCardParameterModifier).ToList().ConvertAll((ICardLifeModifier l) => (DamageCardParameterModifier)l);

	public List<CausedDamageCardParameterModifier> CausedDamageList => CausedDamageModifierList.Where((CausedDamageCardParameterModifier l) => l != null).ToList();

	public List<HealCardParameterModifier> HealList => LifeModifierList.Where((ICardLifeModifier l) => l is HealCardParameterModifier).ToList().ConvertAll((ICardLifeModifier l) => (HealCardParameterModifier)l);

	public List<ICardLifeModifier> LifeChangeList => LifeModifierList.Where((ICardLifeModifier s) => !(s is DamageCardParameterModifier) && !(s is HealCardParameterModifier)).ToList();

	public List<IPpModifier> PpModifierList { get; protected set; }

	public List<PpAddModifier> PpAddList => PpModifierList.OfType<PpAddModifier>().ToList();

	public List<ICardEpModifier> EpModifierList { get; protected set; }

	public List<NotBeAttackedInfo> NotBeAttackedInfoList { get; protected set; }

	public bool IsNotBeAttacked { get; protected set; }

	public List<NotConsumeEpModifierInfo> NotConsumeEpModifierInfoList { get; protected set; }

	public AttachedSkillInformation AttachedSkillsInfo { get; protected set; }

	public List<RepeatSkillInfo> RepeatSkillTimingList { get; protected set; }

	public List<DamageModifier> AddDamageList { get; protected set; }

	public List<HealModifier> HealModifierList { get; protected set; }

	public List<AddTargetInfo> AddTargetList { get; protected set; }

	public List<int> DecreaseTurnStartPPList { get; protected set; }

	public List<int> CantEvolutionList { get; protected set; }

	public List<Skill_cant_summon.CantSummonInfo> CantSummonList { get; protected set; }

	public bool IsDamageCutProtection { get; protected set; }

	public List<BattleCardBase> RandomSelectedCardList { get; protected set; }

	public List<BattleCardBase> SkillDrewCardList { get; protected set; }

	public List<TokenDrawModifier> TokenDrawModifiers { get; protected set; }

	public List<BattleCardBase> SavedTargetList { get; protected set; }

	public Dictionary<long, List<int>> SavedTargetCardIdDict { get; protected set; }

	public List<BattleCardBase> SavedBurialRiteTargetList { get; protected set; }

	public List<BattleCardBase> LastBurialRiteCardList { get; protected set; }

	public List<ICardUnionBurstCountModifier> UnionBurstCountModifierList { get; protected set; }

	public List<ICardSkyboundArtCountModifier> SkyboundArtCountModifierList { get; protected set; }

	public List<ICardSuperSkyboundArtCountModifier> SuperSkyboundArtCountModifierList { get; protected set; }

	public int WhiteRitualCount { get; protected set; }

	public List<int> SkillHealList { get; protected set; }

	public List<FusionIngredientInfo> FusionIngredients { get; protected set; }

	public List<BattleCardBase> GetOnCards { get; private set; }

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

	public bool CantBeFocusedSpell
	{
		get
		{
			if (!IsSneak)
			{
				return IsUntouchableBySpell;
			}
			return true;
		}
	}

	public int[] SkillGenericValueArray { get; protected set; }

	public Dictionary<string, int> SkillGenericKeyAndValue { get; protected set; }

	public int UnionBurstCount
	{
		get
		{
			int num = 10;
			for (int i = 0; i < UnionBurstCountModifierList.Count; i++)
			{
				num = UnionBurstCountModifierList[i].CalcUnionBurstCount(num);
			}
			if (num < 0)
			{
				num = 0;
			}
			return num;
		}
	}

	public int SkyboundArtCount
	{
		get
		{
			int num = 10;
			for (int i = 0; i < SkyboundArtCountModifierList.Count; i++)
			{
				num = SkyboundArtCountModifierList[i].CalcSkyboundArtCount(num);
			}
			if (num < 0)
			{
				num = 0;
			}
			return num;
		}
	}

	public int SuperSkyboundArtCount
	{
		get
		{
			int num = 15;
			for (int i = 0; i < SuperSkyboundArtCountModifierList.Count; i++)
			{
				num = SuperSkyboundArtCountModifierList[i].CalcSuperSkyboundArtCount(num);
			}
			if (num < 0)
			{
				num = 0;
			}
			return num;
		}
	}

	public int RandomAttackCount { get; protected set; }

	public bool IsLifeZeroActivateLeonSkill { get; protected set; }

	public event Func<bool, VfxBase> OnGiveCombatValueModifire;

	public TokenDrawModifier GetTokenDrawModifier(int cardId)
	{
		IEnumerable<TokenDrawModifier> enumerable = TokenDrawModifiers.Where((TokenDrawModifier modifier) => modifier.CardId == cardId);
		if (enumerable.Count() > 0)
		{
			return enumerable.FindMax((TokenDrawModifier x) => x.MultiplyCount);
		}
		return null;
	}

	private void UpdateIsDamageCutProtection()
	{
		IsDamageCutProtection = ShieldInfos.Count > 0 || IsDamageCut || ((_card is ClassBattleCardBase) ? (DamageMaxClippingInfo.Where((DamageClippingInfo i) => i.LifeLowerLimit == -1).ToList().Count > 0) : (DamageMaxClippingInfo.Count > 0));
	}

	public SkillApplyInformation(BattleCardBase card)
	{
		_card = card;
		CantPlayFilterList = new List<CantPlayCardFilterInfo>();
		DamageMaxClippingInfo = new List<DamageClippingInfo>();
		ClanSkinInfo = new List<CardBasePrm.ClanType>();
		TribeSkinInfo = new List<CardBasePrm.TribeInfo>();
		OffenseModifierList = new List<ICardOffenseModifier>();
		LifeModifierList = new List<ICardLifeModifier>();
		CausedDamageModifierList = new List<CausedDamageCardParameterModifier>();
		ChantCountModifierList = new List<ICardChantCountModifier>();
		EpModifierList = new List<ICardEpModifier>();
		NotBeAttackedInfoList = new List<NotBeAttackedInfo>();
		NotConsumeEpModifierInfoList = new List<NotConsumeEpModifierInfo>();
		AttachedSkillsInfo = new AttachedSkillInformation(_card);
		RepeatSkillTimingList = new List<RepeatSkillInfo>();
		DamageCutList = new List<DamageCutInfo>();
		ReflectionInfoList = new List<ReflectionInfo>();
		AddDamageList = new List<DamageModifier>();
		HealModifierList = new List<HealModifier>();
		AddTargetList = new List<AddTargetInfo>();
		DecreaseTurnStartPPList = new List<int>();
		CantEvolutionList = new List<int>();
		CantSummonList = new List<Skill_cant_summon.CantSummonInfo>();
		RandomSelectedCardList = new List<BattleCardBase>();
		SkillDrewCardList = new List<BattleCardBase>();
		TokenDrawModifiers = new List<TokenDrawModifier>();
		GuardInfo = new List<GuardInfo>();
		ShieldInfos = new List<ShieldInfo>();
		RushInfo = new List<RushInfo>();
		UnionBurstCountModifierList = new List<ICardUnionBurstCountModifier>();
		SkyboundArtCountModifierList = new List<ICardSkyboundArtCountModifier>();
		SuperSkyboundArtCountModifierList = new List<ICardSuperSkyboundArtCountModifier>();
		FusionIngredients = new List<FusionIngredientInfo>();
		GetOnCards = new List<BattleCardBase>();
		PpModifierList = new List<IPpModifier>();
		SavedTargetList = new List<BattleCardBase>();
		SavedTargetCardIdDict = new Dictionary<long, List<int>>();
		SavedBurialRiteTargetList = new List<BattleCardBase>();
		LastBurialRiteCardList = new List<BattleCardBase>();
		SkillGenericKeyAndValue = new Dictionary<string, int>();
		TurnBuffCountList = new List<BuffCountInfo>();
		SkillHealList = new List<int>();
		CantAtkUnitBaseCardIdList = new List<int>();
		InitializeInformation();
	}

	public void InitializeInformation(bool isReturnCard = false)
	{
		InitializeInformationWithoutLifeOffenseModifier(isReturnCard);
		ClearParameterModifier();
	}

	public void InitializeInformationWithoutLifeOffenseModifier(bool isReturnCard = false)
	{
		CantPlayFilterList.Clear();
		GuardInfo.Clear();
		IsGuard = false;
		DrainCount = 0;
		IsDrain = false;
		KillerCount = 0;
		IsKiller = false;
		ShieldInfos.Clear();
		IsShieldAll = false;
		IsShieldSkill = false;
		IsShieldSpell = false;
		IsShieldAttack = false;
		IsDamageCutProtection = false;
		QuickCount = 0;
		IsQuick = false;
		RushInfo.Clear();
		IsRush = false;
		SneakCount = 0;
		DamageCutCount = 0;
		IsDamageCut = false;
		NotBeAttackedCount = 0;
		UntouchableCount = 0;
		IsUntouchable = false;
		IsUntouchableBySpell = false;
		IgnoreGuardCount = 0;
		IsIgnoreGuard = false;
		AttackByLifeTypeBeAttackedCount = 0;
		IsAttackByLifeTypeBeAttacked = false;
		AttackByLifeTypeAttackCount = 0;
		IsAttackByLifeTypeAttack = false;
		SkillCantAtkClassCount = 0;
		IsSkillCantAtkClass = false;
		SkillCantAtkUnitCount = 0;
		IsSkillCantAtkUnit = false;
		SkillCantAtkUnitNotHasGuardCount = 0;
		IsSkillCantAtkUnitNotHasGuard = false;
		SkillCantAtkUnitBaseCardIdCount = 0;
		IsSkillCantAtkUnitBaseCardId = false;
		CantAtkUnitBaseCardIdList.Clear();
		ReflectionClassCount = 0;
		IsReflectionClass = false;
		ReflectionDamageOwnerCount = 0;
		IsReflectionDamageOwner = false;
		InfiniteAttackCount = 0;
		IsInfiniteAttack = false;
		IndestructibleCount = 0;
		IsIndestructible = false;
		ForceBerserkCount = 0;
		IsForceBerserk = false;
		ForceAvariceCount = 0;
		IsForceAvarice = false;
		ForceWrathCount = 0;
		IsForceWrath = false;
		CantActivateFanfareUnitCount = 0;
		IsCantActivateFanfareUnit = false;
		CantActivateFanfareFieldCount = 0;
		IsCantActivateFanfareField = false;
		ForceSkillTargetCount = 0;
		AttractSkillTargetCount = 0;
		IsForceSkillTarget = false;
		IsAttractSkillTarget = false;
		IndependentCount = 0;
		IsIndependent = false;
		ForceAttackUnitCount = 0;
		IsForceAttackUnit = false;
		SkillRandomCount = -1;
		SkillRandomArray = null;
		DamageCutList.Clear();
		ReflectionInfoList.Clear();
		TurnStartFixedPPCount = 0;
		IsTurnStartFixedPP = false;
		TriggerCount = 0;
		IsTrigger = false;
		IsNotConsumeEp = false;
		ShortageDeckWinCount = 0;
		IsShortageDeckWin = false;
		ReturnByBanishCount = 0;
		IsReturnByBanish = false;
		DestroyByBanishCount = 0;
		IsDestroyByBanish = false;
		BanishByDestroyCount = 0;
		IsBanishByDestroy = false;
		SkillGenericValueArray = null;
		SavedTargetList.Clear();
		SavedTargetCardIdDict.Clear();
		LastBurialRiteCardList.Clear();
		SavedBurialRiteTargetList.Clear();
		RandomAttackCount = 0;
		NotDecreasePPCounter = 0;
		IsLifeZeroActivateLeonSkill = false;
		DamageMaxClippingInfo.Clear();
		ClanSkinInfo.Clear();
		TribeSkinInfo.Clear();
		EpModifierList.Clear();
		NotBeAttackedInfoList.Clear();
		IsNotBeAttacked = false;
		NotConsumeEpModifierInfoList.Clear();
		AttachedSkillsInfo.Clear();
		RepeatSkillTimingList.Clear();
		AddDamageList.Clear();
		AddTargetList.Clear();
		DecreaseTurnStartPPList.Clear();
		CantEvolutionList.Clear();
		CantSummonList.Clear();
		RandomSelectedCardList.Clear();
		SkillDrewCardList.Clear();
		if (!isReturnCard)
		{
			ClearGetOnCards();
		}
		TokenDrawModifiers.Clear();
		UnionBurstCountModifierList.Clear();
		SkyboundArtCountModifierList.Clear();
		SuperSkyboundArtCountModifierList.Clear();
		FusionIngredients.Clear();
		SkillGenericKeyAndValue.Clear();
		CausedDamageModifierList.Clear();
		WhiteRitualCount = GetInitialWhiteRitualStack();
		TurnBuffCountList.Clear();
	}


	public SkillBase CloneAttachSkill(SkillApplyInformation cloneTarget, SkillBase skill)
	{
		int num = AttachedSkillsInfo.CreatorSkillList.IndexOf(skill);
		if (num == -1 || AttachedSkillsInfo.OwnerCardNameList.Count <= num)
		{
			return null;
		}
		string ownerName = AttachedSkillsInfo.OwnerCardNameList[num];
		int ownerId = AttachedSkillsInfo.OwnerCardIdList[num];
		long duplicateBanNum = skill.OptionValue.GetLong(SkillFilterCreator.ContentKeyword.duplicate_ban_id, 0);
		SkillCreator.SkillBuildInfo skillBuildInfo = Skill_attach_skill.CreateAttachSkillBuildInfo(skill.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.skill));
		skill.IsNotAssignPublishedActiveSkillCount = true;
		SkillBase skillBase = cloneTarget.AttachSkill(skillBuildInfo, cloneTarget._card.ResourceMgr, ownerName, ownerId, duplicateBanNum, skill);
		SkillBase skillBase2 = AttachedSkillsInfo.AttachedSkills.Get(num);
		if (skillBase2 != null)
		{
			for (int i = 0; i < skillBase.PreprocessList.Count; i++)
			{
				if (i < skillBase2.PreprocessList.Count)
				{
					skillBase.PreprocessList[i].Clone(skillBase2.PreprocessList[i], skillBase);
				}
			}
		}
		skill.IsNotAssignPublishedActiveSkillCount = false;
		return skillBase;
	}

	public SkillApplyInformation Clone(BattleCardBase card)
	{
		SkillApplyInformation skillApplyInformation = (SkillApplyInformation)MemberwiseClone();
		skillApplyInformation._card = card;
		skillApplyInformation.CantPlayFilterList = new List<CantPlayCardFilterInfo>(CantPlayFilterList);
		skillApplyInformation.DamageMaxClippingInfo = new List<DamageClippingInfo>(DamageMaxClippingInfo);
		skillApplyInformation.ClanSkinInfo = new List<CardBasePrm.ClanType>(ClanSkinInfo);
		skillApplyInformation.TribeSkinInfo = new List<CardBasePrm.TribeInfo>(TribeSkinInfo);
		skillApplyInformation.OffenseModifierList = new List<ICardOffenseModifier>(OffenseModifierList);
		skillApplyInformation.LifeModifierList = new List<ICardLifeModifier>(LifeModifierList);
		skillApplyInformation.CausedDamageModifierList = new List<CausedDamageCardParameterModifier>(CausedDamageModifierList);
		skillApplyInformation.ChantCountModifierList = new List<ICardChantCountModifier>(ChantCountModifierList);
		skillApplyInformation.EpModifierList = new List<ICardEpModifier>(EpModifierList);
		skillApplyInformation.GuardInfo = new List<GuardInfo>(GuardInfo);
		skillApplyInformation.ShieldInfos = new List<ShieldInfo>(ShieldInfos);
		skillApplyInformation.RushInfo = new List<RushInfo>(RushInfo);
		skillApplyInformation.SkillGenericKeyAndValue = new Dictionary<string, int>(SkillGenericKeyAndValue);
		skillApplyInformation.NotBeAttackedInfoList = new List<NotBeAttackedInfo>(NotBeAttackedInfoList);
		skillApplyInformation.NotConsumeEpModifierInfoList = new List<NotConsumeEpModifierInfo>(NotConsumeEpModifierInfoList);
		skillApplyInformation.AttachedSkillsInfo = new AttachedSkillInformation(skillApplyInformation._card);
		List<SkillBase> creatorSkillList = AttachedSkillsInfo.CreatorSkillList;
		for (int i = 0; i < creatorSkillList.Count; i++)
		{
			if (creatorSkillList[i] != null)
			{
				CloneAttachSkill(skillApplyInformation, creatorSkillList[i]);
			}
		}
		skillApplyInformation.RepeatSkillTimingList = new List<RepeatSkillInfo>();
		for (int j = 0; j < RepeatSkillTimingList.Count; j++)
		{
			skillApplyInformation.RepeatSkillTimingList.Add(RepeatSkillTimingList[j].CloneAndRebuildSkill(skillApplyInformation._card));
		}
		skillApplyInformation.DamageCutList = new List<DamageCutInfo>(DamageCutList);
		skillApplyInformation.ReflectionInfoList = new List<ReflectionInfo>(ReflectionInfoList);
		skillApplyInformation.AddDamageList = new List<DamageModifier>(AddDamageList);
		skillApplyInformation.HealModifierList = new List<HealModifier>(HealModifierList);
		skillApplyInformation.TokenDrawModifiers = new List<TokenDrawModifier>(TokenDrawModifiers);
		skillApplyInformation.UnionBurstCountModifierList = new List<ICardUnionBurstCountModifier>(UnionBurstCountModifierList);
		skillApplyInformation.AddTargetList = new List<AddTargetInfo>();
		for (int k = 0; k < AddTargetList.Count; k++)
		{
			skillApplyInformation.AddTargetList.Add(AddTargetList[k].Clone(skillApplyInformation._card));
		}
		skillApplyInformation.DecreaseTurnStartPPList = new List<int>(DecreaseTurnStartPPList);
		skillApplyInformation.CantEvolutionList = new List<int>(CantEvolutionList);
		skillApplyInformation.CantSummonList = new List<Skill_cant_summon.CantSummonInfo>(CantSummonList);
		skillApplyInformation.RandomSelectedCardList = new List<BattleCardBase>(RandomSelectedCardList);
		skillApplyInformation.SkillDrewCardList = new List<BattleCardBase>(SkillDrewCardList);
		UpdateIsDamageCutProtection();
		skillApplyInformation.GetOnCards = new List<BattleCardBase>(GetOnCards);
		skillApplyInformation.SavedTargetList = new List<BattleCardBase>(SavedTargetList);
		skillApplyInformation.SavedTargetCardIdDict = new Dictionary<long, List<int>>(SavedTargetCardIdDict);
		skillApplyInformation.FusionIngredients = new List<FusionIngredientInfo>();
		skillApplyInformation.SavedBurialRiteTargetList = new List<BattleCardBase>(SavedBurialRiteTargetList);
		skillApplyInformation.LastBurialRiteCardList = new List<BattleCardBase>(LastBurialRiteCardList);
		for (int l = 0; l < FusionIngredients.Count; l++)
		{
			skillApplyInformation.FusionIngredients.Add(FusionIngredients[l].Clone());
		}
		skillApplyInformation.TurnBuffCountList = new List<BuffCountInfo>(TurnBuffCountList);
		skillApplyInformation.SkillHealList = new List<int>(SkillHealList);
		return skillApplyInformation;
	}

	public void Combine(ISkillApplyInformation applyInfo)
	{
		GuardInfo.AddRange(applyInfo.GuardInfo);
		IsGuard = GuardInfo.Count > 0;
		DrainCount += applyInfo.DrainCount;
		IsDrain = DrainCount > 0;
		KillerCount += applyInfo.KillerCount;
		IsKiller = KillerCount > 0;
		ShieldInfos.AddRange(applyInfo.ShieldInfos);
		IsShieldAll = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.ALL);
		IsShieldSkill = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.SKILL);
		IsShieldSpell = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.SPELL);
		IsShieldAttack = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.ATTACK);
		UpdateIsDamageCutProtection();
		QuickCount += applyInfo.QuickCount;
		IsQuick = QuickCount > 0;
		RushInfo.AddRange(applyInfo.RushInfo);
		IsRush = RushInfo.Count > 0;
		SneakCount += applyInfo.SneakCount;
		DamageCutCount += applyInfo.DamageCutCount;
		IsDamageCut = DamageCutCount > 0;
		NotBeAttackedCount += applyInfo.NotBeAttackedCount;
		UntouchableCount += applyInfo.UntouchableCount;
		IsUntouchable = UntouchableCount > 0;
		IsUntouchableBySpell = UntouchableCount > 0;
		IgnoreGuardCount += applyInfo.IgnoreGuardCount;
		IsIgnoreGuard = IgnoreGuardCount > 0;
		AttackByLifeTypeBeAttackedCount += applyInfo.AttackByLifeTypeBeAttackedCount;
		IsAttackByLifeTypeBeAttacked = AttackByLifeTypeBeAttackedCount > 0;
		AttackByLifeTypeAttackCount += applyInfo.AttackByLifeTypeAttackCount;
		IsAttackByLifeTypeAttack = AttackByLifeTypeAttackCount > 0;
		SkillCantAtkClassCount += applyInfo.SkillCantAtkClassCount;
		IsSkillCantAtkClass = SkillCantAtkClassCount > 0;
		SkillCantAtkUnitCount += applyInfo.SkillCantAtkUnitCount;
		IsSkillCantAtkUnit = SkillCantAtkUnitCount > 0;
		SkillCantAtkUnitNotHasGuardCount += applyInfo.SkillCantAtkUnitNotHasGuardCount;
		IsSkillCantAtkUnitNotHasGuard = SkillCantAtkUnitNotHasGuardCount > 0;
		SkillCantAtkUnitBaseCardIdCount += applyInfo.SkillCantAtkUnitNotHasGuardCount;
		IsSkillCantAtkUnitBaseCardId = SkillCantAtkUnitBaseCardIdCount > 0;
		CantAtkUnitBaseCardIdList.AddRange(applyInfo.CantAtkUnitBaseCardIdList);
		ReflectionClassCount += applyInfo.ReflectionClassCount;
		IsReflectionClass = ReflectionClassCount > 0;
		ReflectionDamageOwnerCount += applyInfo.ReflectionDamageOwnerCount;
		IsReflectionDamageOwner = ReflectionDamageOwnerCount > 0;
		InfiniteAttackCount += applyInfo.InfiniteAttackCount;
		IsInfiniteAttack = InfiniteAttackCount > 0;
		IndestructibleCount += applyInfo.IndestructibleCount;
		IsIndestructible = IndestructibleCount > 0;
		ForceBerserkCount += applyInfo.ForceBerserkCount;
		IsForceBerserk = ForceBerserkCount > 0;
		CantActivateFanfareUnitCount += applyInfo.CantActivateFanfareUnitCount;
		IsCantActivateFanfareUnit = CantActivateFanfareUnitCount > 0;
		CantActivateFanfareFieldCount += applyInfo.CantActivateFanfareFieldCount;
		IsCantActivateFanfareField = CantActivateFanfareFieldCount > 0;
		ForceSkillTargetCount += applyInfo.ForceSkillTargetCount;
		IsForceSkillTarget = ForceSkillTargetCount > 0;
		AttractSkillTargetCount += applyInfo.AttractSkillTargetCount;
		IsAttractSkillTarget = AttractSkillTargetCount > 0;
		IndependentCount += applyInfo.IndependentCount;
		IsIndependent = IndependentCount > 0;
		ForceAttackUnitCount += applyInfo.ForceAttackUnitCount;
		IsForceAttackUnit = ForceAttackUnitCount > 0;
		DamageCutList.AddRange(applyInfo.DamageCutList);
		ReflectionInfoList.AddRange(applyInfo.ReflectionInfoList);
		TurnStartFixedPPCount += applyInfo.TurnStartFixedPPCount;
		IsTurnStartFixedPP = TurnStartFixedPPCount > 0;
		TriggerCount += applyInfo.TriggerCount;
		IsTrigger = TriggerCount > 0;
		NotDecreasePPCounter += applyInfo.NotDecreasePPCounter;
		NotConsumeEpModifierInfoList.AddRange(applyInfo.NotConsumeEpModifierInfoList);
		IsNotConsumeEp = NotConsumeEpModifierInfoList.Count() > 0;
		ShortageDeckWinCount += applyInfo.ShortageDeckWinCount;
		IsShortageDeckWin = ShortageDeckWinCount > 0;
		ReturnByBanishCount += applyInfo.ReturnByBanishCount;
		IsReturnByBanish = ReturnByBanishCount > 0;
		DestroyByBanishCount += applyInfo.DestroyByBanishCount;
		IsDestroyByBanish = DestroyByBanishCount > 0;
		BanishByDestroyCount += applyInfo.BanishByDestroyCount;
		IsBanishByDestroy = BanishByDestroyCount > 0;
		SkillGenericValueArray = applyInfo.SkillGenericValueArray;
		RandomAttackCount = applyInfo.RandomAttackCount;
		CantPlayFilterList.AddRange(applyInfo.CantPlayFilterList);
		DamageMaxClippingInfo.AddRange(applyInfo.DamageMaxClippingInfo);
		ClanSkinInfo.AddRange(applyInfo.ClanSkinInfo);
		TribeSkinInfo.AddRange(applyInfo.TribeSkinInfo);
		EpModifierList.AddRange(applyInfo.EpModifierList);
		NotBeAttackedInfoList.AddRange(applyInfo.NotBeAttackedInfoList);
		IsNotBeAttacked = NotBeAttackedCount > 0;
		NotConsumeEpModifierInfoList.AddRange(applyInfo.NotConsumeEpModifierInfoList);
		CantEvolutionList.AddRange(applyInfo.CantEvolutionList);
		RandomSelectedCardList.AddRange(applyInfo.RandomSelectedCardList);
		SkillDrewCardList.AddRange(applyInfo.SkillDrewCardList);
		SkillGenericKeyAndValue = new Dictionary<string, int>(applyInfo.SkillGenericKeyAndValue);
		AttachedSkillInformation attachedSkillsInfo = applyInfo.AttachedSkillsInfo;
		List<SkillBase> list = attachedSkillsInfo.AttachedSkills.ToList();
		int num = applyInfo.AttachedSkillsInfo.AttachedSkills.Count();
		for (int num2 = 0; num2 < num; num2++)
		{
			SkillBase skill = list[num2];
			if (attachedSkillsInfo.CreatorSkillList[num2].IsContainCardInBuffInfo(_card))
			{
				if (!attachedSkillsInfo.AttachedSkills.Any((SkillBase s) => s == skill))
				{
					AttachedSkillsInfo.Add(skill, attachedSkillsInfo.OwnerCardNameList[num2], attachedSkillsInfo.OwnerCardIdList[num2], attachedSkillsInfo.DuplicateBanNum[num2], attachedSkillsInfo.CreatorSkillList[num2], attachedSkillsInfo.CreatorSkillIndexList[num2]);
				}
				if (!_card.Skills.Any((SkillBase s) => s == skill))
				{
					_card.Skills.Add(skill);
				}
				skill.IsNotAssignPublishedActiveSkillCount = false;
			}
		}
		_card.Skills.Complete();
		for (int num3 = 0; num3 < applyInfo.RepeatSkillTimingList.Count; num3++)
		{
			RepeatSkillTimingList.Add(applyInfo.RepeatSkillTimingList[num3].CloneAndRebuildSkill(_card));
		}
		AddDamageList.AddRange(applyInfo.AddDamageList);
		TokenDrawModifiers.AddRange(applyInfo.TokenDrawModifiers);
		for (int num4 = 0; num4 < applyInfo.AddTargetList.Count; num4++)
		{
			AddTargetList.Add(applyInfo.AddTargetList[num4].Clone(_card));
		}
		DecreaseTurnStartPPList.AddRange(applyInfo.DecreaseTurnStartPPList);
		UpdateIsDamageCutProtection();
	}

	public bool IsCantPlay(BattleCardBase card, BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE type = BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE)
	{
		BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE cHECK_CONDITION_MUTATIONSKILL_TYPE = ((type != BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE) ? type : card.IsCheckActiveMutationSkill);
		if (cHECK_CONDITION_MUTATIONSKILL_TYPE != BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NOT_HAVE_MUTATION_SKILL && cHECK_CONDITION_MUTATIONSKILL_TYPE != BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NOT_PLAY)
		{
			if (card.HasSkillAccelerate && (card.BaseParameter.BaseCardId != 123031020 || cHECK_CONDITION_MUTATIONSKILL_TYPE != BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.CRYSTALLIZE_SKILL_ACTIVE) && (HasCantPlaySpellFilter() || CantPlayTransformId(card)))
			{
				return true;
			}
			if (card.HasSkillCrystallize && (HasCantPlayFieldFilter() || CantPlayTransformId(card)))
			{
				return true;
			}
			if (card.IsMutationMovable(type))
			{
				return false;
			}
		}
		foreach (CantPlayCardFilterInfo cantPlayFilter in CantPlayFilterList)
		{
			if (cantPlayFilter.CheckCantPlay(card))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCantPlaySpellFilter()
	{
		foreach (CantPlayCardFilterInfo cantPlayFilter in CantPlayFilterList)
		{
			if (cantPlayFilter.IsCantPlaySpell)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCantPlayFieldFilter()
	{
		foreach (CantPlayCardFilterInfo cantPlayFilter in CantPlayFilterList)
		{
			if (cantPlayFilter.IsCantPlayField)
			{
				return true;
			}
		}
		return false;
	}

	public bool CantPlayTransformId(BattleCardBase originalCard)
	{
		foreach (CantPlayCardFilterInfo cantPlayFilter in CantPlayFilterList)
		{
			if (cantPlayFilter.CheckCantPlayTransformId(originalCard))
			{
				return true;
			}
		}
		return false;
	}

	public SkillBase AttachSkill(SkillCreator.SkillBuildInfo skillBuildInfo, IBattleResourceMgr resourceMgr, string ownerName, int ownerId, long duplicateBanNum, SkillBase originSkill = null, bool isAttachEvolveSkill = false)
	{
		List<SkillPreprocessBase> lastSkillPreprocessCollection = new List<SkillPreprocessBase>();
		SkillCollectionBase skillCollectionBase = ((isAttachEvolveSkill && (!_card.IsEvolution || _card.EvolutionSkills.Count() != 0)) ? _card.EvolutionSkills : _card.Skills);
		SkillBase skillBase = skillCollectionBase.LastOrDefault();
		SkillBase skillBase2 = skillCollectionBase.LastOrDefault((SkillBase s) => !s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessReferencePrevious));
		if (skillBase2 != null)
		{
			lastSkillPreprocessCollection = skillBase2.PreprocessList;
		}
		SkillBase skillBase3 = _card.CreateSkillCreator(_card.SelfBattlePlayer, _card.OpponentBattlePlayer, resourceMgr).Create(skillBuildInfo, lastSkillPreprocessCollection, isAttachSkill: true, originSkill);
		if (skillBase3 is Skill_discard && skillBase != null && skillBase is Skill_discard && originSkill != null && skillBase.SkillPrm.ownerCard.SkillApplyInformation != null && skillBase.GetAttachSkill != null && originSkill == skillBase.GetAttachSkill && originSkill.GetAttachSkill != null && originSkill.GetAttachSkill.SkillPrm.ownerCard.BaseParameter.BaseCardId == 900344060)
		{
			return null;
		}
		if (originSkill != null)
		{
			skillBase3.IsNotAssignPublishedActiveSkillCount = originSkill.IsNotAssignPublishedActiveSkillCount;
			skillBase3.SetIndividualId(originSkill.IndividualId, isForce: true);
		}
		skillCollectionBase.Add(skillBase3);
		skillCollectionBase.Complete();
		AttachedSkillsInfo.Add(skillBase3, ownerName, ownerId, duplicateBanNum, originSkill, originSkill.SkillPrm.ownerCard.Skills.IndexOf(originSkill));
		_card.CallOnAttachSkill(_card, skillBase3);
		skillBase3.IsNotAssignPublishedActiveSkillCount = false;
		return skillBase3;
	}

	public void RemoveSkill(SkillBase skill, BattleCardBase skillOwnerCard, long duplicateBanNum, SkillBase originSkill, int creatorSkillIndex)
	{
		_card.Skills.Remove(skill);
		_card.Skills.Complete();
		AttachedSkillsInfo.Remove(skill, skillOwnerCard, duplicateBanNum, originSkill, creatorSkillIndex);
	}

	protected virtual VfxBase CombatModifierChangeCalc(bool isOldAtkBuff, bool isNowAtkBuff, bool isOldMaxLifeBuff, bool isNowMaxLifeBuff, bool isOldAtkDebuff, bool isNowAtkDebuff, bool isOldMaxLifeDebuff, bool isNowMaxLifeDebuff, bool isDebuff = false, bool isNoBuff = false, bool skipWait = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (_card.IsInplay)
		{
			if (!_card.IsDead && ((!isOldAtkBuff && isNowAtkBuff && ((!isOldMaxLifeBuff && !isNowMaxLifeBuff) || (!isOldMaxLifeBuff && isNowMaxLifeBuff))) || (!isOldMaxLifeBuff && isNowMaxLifeBuff && ((!isOldAtkBuff && !isNowAtkBuff) || (!isOldAtkBuff && isNowAtkBuff)))))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			if ((isOldAtkBuff && !isNowAtkBuff && !isNowMaxLifeBuff) || (isOldMaxLifeBuff && !isNowMaxLifeBuff && !isNowAtkBuff))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			if (!_card.IsDead && ((!isOldAtkDebuff && isNowAtkDebuff && ((!isOldMaxLifeDebuff && !isNowMaxLifeDebuff) || (!isOldMaxLifeDebuff && isNowMaxLifeDebuff))) || (!isOldMaxLifeDebuff && isNowMaxLifeDebuff && ((!isOldAtkDebuff && !isNowAtkDebuff) || (!isOldAtkDebuff && isNowAtkDebuff)))))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			if ((isOldAtkDebuff && !isNowAtkDebuff && !isNowMaxLifeDebuff) || (isOldMaxLifeDebuff && !isNowMaxLifeDebuff && !isNowAtkDebuff))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
		}
		if (_card.IsInHand && !isNoBuff)
		{
			return NullVfx.GetInstance();
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase GiveCombatValueModifier(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		int num = _card.BaseAtk;
		int num2 = _card.BaseAtk;
		int num3 = _card.BaseMaxLife;
		int num4 = _card.BaseMaxLife;
		for (int i = 0; i < OffenseModifierList.Count; i++)
		{
			num = (num2 = OffenseModifierList[i].CalcOffense(num));
		}
		if (offenseModifier != null)
		{
			if (offenseModifier.IsClearBeforeModifier)
			{
				OffenseModifierList.Clear();
				num2 = offenseModifier.CalcOffense(num2);
			}
			else
			{
				num2 = offenseModifier.CalcOffense(num);
			}
			OffenseModifierList.Add(offenseModifier);
		}
		for (int j = 0; j < LifeModifierList.Count; j++)
		{
			num3 = (num4 = LifeModifierList[j].CalcMaxLife(num3));
		}
		if (lifeModifier != null)
		{
			if (lifeModifier.IsClearBeforeModifier)
			{
				LifeModifierList.Clear();
				num4 = lifeModifier.CalcMaxLife(num4);
			}
			else
			{
				num4 = lifeModifier.CalcMaxLife(num3);
			}
			LifeModifierList.Add(lifeModifier);
		}
		bool skipWait = !_card.IsPlayer && _card.IsInHand && !_card.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch;
		bool isOldAtkBuff = num > _card.BaseAtk;
		bool isNowAtkBuff = num2 > _card.BaseAtk;
		bool isOldMaxLifeBuff = num3 > _card.BaseMaxLife;
		bool isNowMaxLifeBuff = num4 > _card.BaseMaxLife;
		bool isOldAtkDebuff = num < _card.BaseAtk;
		bool isNowAtkDebuff = num2 < _card.BaseAtk;
		bool isOldMaxLifeDebuff = num3 < _card.BaseMaxLife;
		bool isNowMaxLifeDebuff = num4 < _card.BaseMaxLife;
		int num5;
		int num6;
		if (num >= num2)
		{
			num5 = ((num3 < num4) ? 1 : 0);
			if (num5 == 0)
			{
				num6 = ((num > num2 || num3 > num4) ? 1 : 0);
				goto IL_01d4;
			}
		}
		else
		{
			num5 = 1;
		}
		num6 = 0;
		goto IL_01d4;
		IL_01d4:
		bool flag = (byte)num6 != 0;
		bool isNoBuff = num5 == 0 && !flag;
		sequentialVfxPlayer.Register(CombatModifierChangeCalc(isOldAtkBuff, isNowAtkBuff, isOldMaxLifeBuff, isNowMaxLifeBuff, isOldAtkDebuff, isNowAtkDebuff, isOldMaxLifeDebuff, isNowMaxLifeDebuff, flag, isNoBuff, skipWait));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(this.OnGiveCombatValueModifire.GetAllFuncVfxResults(arg1: false));
		return sequentialVfxPlayer;
	}

	public virtual VfxBase DepriveCombatValueModifire(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int num = _card.BaseAtk;
		int num2 = _card.BaseAtk;
		int num3 = _card.BaseMaxLife;
		int num4 = _card.BaseMaxLife;
		for (int i = 0; i < OffenseModifierList.Count; i++)
		{
			num = (num2 = OffenseModifierList[i].CalcOffense(num));
		}
		if (offenseModifier != null && !_card.IsDead)
		{
			OffenseModifierList.Remove(offenseModifier);
			num2 = _card.Atk;
		}
		for (int j = 0; j < LifeModifierList.Count; j++)
		{
			num3 = (num4 = LifeModifierList[j].CalcMaxLife(num3));
		}
		if (lifeModifier != null && !_card.IsDead)
		{
			LifeModifierList.Remove(lifeModifier);
			num4 = _card.MaxLife;
		}
		bool isOldAtkBuff = num > _card.BaseAtk;
		bool isNowAtkBuff = num2 > _card.BaseAtk;
		bool isOldMaxLifeBuff = num3 > _card.BaseMaxLife;
		bool isNowMaxLifeBuff = num4 > _card.BaseMaxLife;
		bool isOldAtkDebuff = num < _card.BaseAtk;
		bool isNowAtkDebuff = num2 < _card.BaseAtk;
		bool isOldMaxLifeDebuff = num3 < _card.BaseMaxLife;
		bool isNowMaxLifeDebuff = num4 < _card.BaseMaxLife;
		parallelVfxPlayer.Register(CombatModifierChangeCalc(isOldAtkBuff, isNowAtkBuff, isOldMaxLifeBuff, isNowMaxLifeBuff, isOldAtkDebuff, isNowAtkDebuff, isOldMaxLifeDebuff, isNowMaxLifeDebuff));
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		return parallelVfxPlayer;
	}

	public virtual VfxBase ForceDepriveCombatValueModifire()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register((_card.Atk > _card.BaseAtk || _card.MaxLife > _card.BaseMaxLife) ? NullVfx.GetInstance() : NullVfx.GetInstance());
		parallelVfxPlayer.Register((_card.Atk < _card.BaseAtk || _card.MaxLife < _card.BaseMaxLife) ? NullVfx.GetInstance() : NullVfx.GetInstance());
		if (_card.Atk > _card.BaseAtk || _card.MaxLife > _card.BaseMaxLife || _card.Atk < _card.BaseAtk || _card.MaxLife < _card.BaseMaxLife)
		{
			NullVfx.GetInstance();
		}
		return parallelVfxPlayer;
	}

	public void AddOffenseModifier(ICardOffenseModifier modifier)
	{
		if (modifier.IsClearBeforeModifier)
		{
			OffenseModifierList.Clear();
		}
		OffenseModifierList.Add(modifier);
	}

	public void AddLifeModifier(ICardLifeModifier modifier)
	{
		if (modifier.IsClearBeforeModifier)
		{
			LifeModifierList.Clear();
		}
		LifeModifierList.Add(modifier);
	}

	public void ClearParameterModifier()
	{
		OffenseModifierList.Clear();
		LifeModifierList.Clear();
	}

	public void ClearUnionBurstAndSkyboundArtModifier()
	{
		UnionBurstCountModifierList.Clear();
		SkyboundArtCountModifierList.Clear();
		SuperSkyboundArtCountModifierList.Clear();
	}

	public void AddEpModifier(ICardEpModifier modifier)
	{
		if (modifier.IsClearBeforeModifier)
		{
			EpModifierList.Clear();
		}
		EpModifierList.Add(modifier);
	}

	public void RemoveEpModifier(ICardEpModifier modifier)
	{
		EpModifierList.Remove(modifier);
	}

	public virtual int GetEp()
	{
		int num = 1;
		int count = EpModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			num = EpModifierList[i].CalcEp(num);
		}
		return Math.Max(0, num);
	}

	public virtual int GetAtk(bool ignoreLowerLimit = false)
	{
		int num = _card.BaseAtk;
		int count = OffenseModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			num = OffenseModifierList[i].CalcOffense(num);
		}
		if (ignoreLowerLimit)
		{
			return num;
		}
		return Math.Max(0, num);
	}

	public virtual int GetLife()
	{
		int num = _card.BaseMaxLife;
		int num2 = _card.BaseMaxLife;
		int count = LifeModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			ICardLifeModifier cardLifeModifier = LifeModifierList[i];
			num2 = cardLifeModifier.CalcMaxLife(num2);
			num = cardLifeModifier.CalcLife(num);
			num = Math.Min(num, num2);
		}
		return num;
	}

	public virtual bool HasMoreDamageThan(ISkillApplyInformation other)
	{
		return DamageList.Count > other.DamageList.Count;
	}

	public virtual int GetLastLife()
	{
		int result = _card.BaseMaxLife;
		int num = _card.BaseMaxLife;
		int num2 = _card.BaseMaxLife;
		int count = LifeModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			ICardLifeModifier cardLifeModifier = LifeModifierList[i];
			num = cardLifeModifier.CalcMaxLife(num);
			num2 = cardLifeModifier.CalcLife(num2);
			num2 = Math.Min(num2, num);
			if (num2 <= 0)
			{
				break;
			}
			result = num2;
		}
		return result;
	}

	public virtual int GetMaxLife()
	{
		int num = _card.BaseMaxLife;
		int count = LifeModifierList.Count;
		for (int i = 0; i < count; i++)
		{
			num = LifeModifierList[i].CalcMaxLife(num);
		}
		return num;
	}

	public virtual List<T> GetSpecificTurnPlayerValueList<T>(List<T> turnPlayerValueList, IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo) where T : TurnAndIntValue
	{
		bool isCheckSelf = cardInfo.IsPlayer == turnPlayerInfo.IsSelfPlayer;
		if (turnPlayerInfo.IsAllTurn)
		{
			if (turnPlayerInfo.IsOther)
			{
				return turnPlayerValueList;
			}
			return turnPlayerValueList.Where((T s) => s.IsSelfTurn == isCheckSelf).ToList();
		}
		int turn = GetReferenceTurn(isCheckSelf, turnPlayerInfo.TurnOffset);
		return turnPlayerValueList.Where((T d) => d.IsSpecificTurn(turn, isCheckSelf)).ToList();
	}

	public virtual int GetSpecificTurnDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		List<DamageCardParameterModifier> specificTurnDamageValueList = GetSpecificTurnDamageValueList(cardInfo, turnPlayerInfo);
		int num = 0;
		for (int i = 0; i < specificTurnDamageValueList.Count; i++)
		{
			num += specificTurnDamageValueList[i].Damage;
		}
		return num;
	}

	public virtual int GetSpecificTurnCausedDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		List<CausedDamageCardParameterModifier> specificTurnCausedDamageValueList = GetSpecificTurnCausedDamageValueList(cardInfo, turnPlayerInfo);
		int num = 0;
		for (int i = 0; i < specificTurnCausedDamageValueList.Count; i++)
		{
			num += specificTurnCausedDamageValueList[i].Damage;
		}
		return num;
	}

	public virtual int GetSpecificTurnHealValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		List<HealCardParameterModifier> specificTurnHealValueList = GetSpecificTurnHealValueList(cardInfo, turnPlayerInfo);
		int num = 0;
		for (int i = 0; i < specificTurnHealValueList.Count; i++)
		{
			num += specificTurnHealValueList[i].Heal;
		}
		return num;
	}

	public virtual int GetChangeMaxLifeCount()
	{
		int num = 0;
		for (int i = 0; i < LifeModifierList.Count; i++)
		{
			if (LifeModifierList[i].IsChangeMaxLife)
			{
				num++;
			}
		}
		return num;
	}

	public int GetInitialWhiteRitualStack()
	{
		if (_card != null && (_card.IsField || _card.IsChantField) && _card.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL))
		{
			if (_card.Skills.FirstOrDefault((SkillBase s) => s is Skill_stack_white_ritual) is Skill_stack_white_ritual skill_stack_white_ritual)
			{
				return skill_stack_white_ritual.InitialWhiteRitualStack;
			}
			return 1;
		}
		return 0;
	}

	public void SetSkillGenericArray(int[] array)
	{
		SkillGenericValueArray = array;
	}

	public void SetSkillGenericKeyAndValue(string key, int value)
	{
		if (!SkillGenericKeyAndValue.ContainsKey(key))
		{
			SkillGenericKeyAndValue.Add(key, value);
		}
		else
		{
			SkillGenericKeyAndValue[key] = value;
		}
	}

	public bool IsContainGenericValueKey(string key)
	{
		return SkillGenericKeyAndValue.ContainsKey(key);
	}

	public void AddSkillGenericValue(int value, int index)
	{
		SkillGenericValueArray[index] += value;
	}

	public void GiveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier)
	{
		UnionBurstCountModifierList.Add(unionBurstCountModifier);
	}

	public void DepriveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier)
	{
		UnionBurstCountModifierList.Remove(unionBurstCountModifier);
	}

	public void FourceDepriveUnionBurstCount()
	{
		UnionBurstCountModifierList.Clear();
	}

	public void GiveSkyboundArtCount(ICardSkyboundArtCountModifier skyboundArtCountModifier)
	{
		SkyboundArtCountModifierList.Add(skyboundArtCountModifier);
	}

	public void GiveSuperSkyboundArtCount(ICardSuperSkyboundArtCountModifier superSkyboundArtCountModifier)
	{
		SuperSkyboundArtCountModifierList.Add(superSkyboundArtCountModifier);
	}

	public void GiveWhiteRitualCount(int value)
	{
		WhiteRitualCount += value;
	}

	public void DepriveWhiteRitualCount(int value)
	{
		WhiteRitualCount -= value;
	}

	public void FourceDepriveWhiteRitualCount()
	{
		WhiteRitualCount = 0;
	}

	public virtual int GetSpecificTurnDamageCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnDamageValueList(cardInfo, turnPlayerInfo).Count;
	}

	public virtual List<DamageCardParameterModifier> GetSpecificTurnDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(DamageList, cardInfo, turnPlayerInfo);
	}

	public virtual List<CausedDamageCardParameterModifier> GetSpecificTurnCausedDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(CausedDamageList, cardInfo, turnPlayerInfo);
	}

	public virtual List<HealCardParameterModifier> GetSpecificTurnHealValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(HealList, cardInfo, turnPlayerInfo);
	}

	public virtual int GetSpecificTurnHealCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnHealValueList(cardInfo, turnPlayerInfo).Count;
	}

	public virtual int GetSpecificTurnHealCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		if (cardInfo.IsPlayer != _card.SelfBattlePlayer.BattleMgr.BattlePlayer.IsSelfTurn)
		{
			return 0;
		}
		return GetSpecificTurnHealCount(cardInfo, turnPlayerInfo);
	}

	public virtual int GetSpecificTurnBuffCount(TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(TurnBuffCountList, _card, turnPlayerInfo).Count();
	}

	public virtual int GetSpecificTurnPpAddCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPpHealList(cardInfo, turnPlayerInfo).Count;
	}

	public List<PpAddModifier> GetSpecificTurnPpHealList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(PpAddList, cardInfo, turnPlayerInfo);
	}

	private void AddPpModifier(IPpModifier ppModifier)
	{
		PpModifierList.Add(ppModifier);
	}

	public virtual int GetSpecificTurnAcceleratedCardCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnAcceleratedCardList(cardInfo, turnPlayerInfo).Count;
	}

	public virtual int GetSpecificTurnAcceleratedCardCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		if (cardInfo.IsPlayer != _card.SelfBattlePlayer.BattleMgr.BattlePlayer.IsSelfTurn)
		{
			return 0;
		}
		return GetSpecificTurnAcceleratedCardCount(cardInfo, turnPlayerInfo);
	}

	public List<IReadOnlyBattleCardInfo> GetSpecificTurnAcceleratedCardList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		bool flag = cardInfo.IsPlayer == turnPlayerInfo.IsSelfPlayer;
		int referenceTurn = GetReferenceTurn(flag, turnPlayerInfo.TurnOffset);
		BattlePlayerBase obj = (flag ? ((BattlePlayerBase)_card.SelfBattlePlayer.BattleMgr.BattlePlayer) : ((BattlePlayerBase)_card.SelfBattlePlayer.BattleMgr.BattleEnemy));
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (IReadOnlyBattleCardInfo skillInfoGamePlayCard in obj.SkillInfoGamePlayCards)
		{
			if (skillInfoGamePlayCard.TransformInfo.Type == BattleCardBase.TransformType.Accelerate && skillInfoGamePlayCard.PlayedTurn == referenceTurn)
			{
				list.Add(skillInfoGamePlayCard.TransformInfo.OriginalCard);
			}
		}
		return list;
	}

	public List<TurnAndIntValue> GetSpecificTurnStartLifeList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(Player.TurnStartLifeList, cardInfo, turnPlayerInfo);
	}

	public virtual int GetSpecificTurnFusionCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return GetSpecificTurnPlayerValueList(Player.TurnFusionCountInfo, cardInfo, turnPlayerInfo).Sum((TurnAndIntValue f) => f.Value);
	}

	public int GetReferenceTurn(bool isCheckSelf, int turnOffset)
	{
		BattleManagerBase ins = _card.SelfBattlePlayer.BattleMgr;
		return (isCheckSelf ? ins.BattlePlayer.Turn : ins.BattleEnemy.Turn) - turnOffset;
	}

	public virtual void DamageLife(int damage, int turn, bool isSelfTurn)
	{
		DamageCardParameterModifier modifier = new DamageCardParameterModifier(damage, turn, isSelfTurn);
		AddLifeModifier(modifier);
	}

	public virtual void CausedDamageLife(int damage, int turn, bool isSelfTurn)
	{
		CausedDamageCardParameterModifier item = new CausedDamageCardParameterModifier(damage, turn, isSelfTurn);
		CausedDamageModifierList.Add(item);
	}

	public virtual void HealLife(int healAmount, int turn, bool isSelfTurn)
	{
		AddLifeModifier(new HealCardParameterModifier(healAmount, turn, isSelfTurn));
	}

	public virtual void AddPp(int addPp, int currentTurn, bool isSelfTurn)
	{
		AddPpModifier(new PpAddModifier(addPp, currentTurn, isSelfTurn));
	}

	public virtual void GiveBuff(bool isReplace = false)
	{
		BuffCount++;
		IsBuff = BuffCount > 0;
		if (_card.IsInplay && !isReplace)
		{
			BattleManagerBase battleMgr = _card.SelfBattlePlayer.BattleMgr;
			GiveTurnBuffCount(battleMgr.CurrentTurn, battleMgr.BattlePlayer.IsSelfTurn);
		}
	}

	public virtual void GiveTurnBuffCount(int turn, bool isSelf)
	{
		TurnBuffCountList.Add(new BuffCountInfo(turn, isSelf));
	}

	public virtual void DepriveBuff()
	{
		BuffCount--;
		IsBuff = BuffCount > 0;
	}

	public virtual void FourceDepriveBuff()
	{
		BuffCount = 0;
		IsBuff = false;
	}

	public virtual void GiveDebuff()
	{
		DebuffCount++;
		IsDebuff = DebuffCount > 0;
	}

	public virtual void DepriveDebuff()
	{
		DebuffCount--;
		IsDebuff = DebuffCount > 0;
	}

	public virtual void FourceDepriveDebuff()
	{
		DebuffCount = 0;
		IsDebuff = false;
	}

	public virtual void GiveBuffLife()
	{
		BuffLifeCount++;
	}

	public virtual void DepriveBuffLife()
	{
		BuffLifeCount--;
	}

	public virtual void ForceDepriveBuffLife()
	{
		BuffLifeCount = 0;
	}

	public virtual VfxBase GiveGuard(GuardInfo info)
	{
		GuardInfo.Add(info);
		IsGuard = GuardInfo.Count > 0;
		if (GuardInfo.Count > 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveGuard(GuardInfo info)
	{
		GuardInfo.Remove(info);
		IsGuard = GuardInfo.Count > 0;
		if (IsGuard)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveGuard()
	{
		if (!_card.IsDead)
		{
			GuardInfo.Clear();
			IsGuard = false;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveDrain()
	{
		DrainCount++;
		IsDrain = DrainCount > 0;
		if (DrainCount > 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveDrain()
	{
		DrainCount--;
		IsDrain = DrainCount > 0;
		if (DrainCount >= 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveDrain()
	{
		if (!_card.IsDead)
		{
			DrainCount = 0;
			IsDrain = false;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveKiller()
	{
		KillerCount++;
		IsKiller = KillerCount > 0;
		if (KillerCount > 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveKiller()
	{
		KillerCount--;
		IsKiller = KillerCount > 0;
		if (KillerCount >= 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveKiller()
	{
		if (!_card.IsDead)
		{
			KillerCount = 0;
			IsKiller = false;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveShield(ShieldInfo shield)
	{
		ShieldInfos.Add(shield);
		UpdateShield(shield.Type);
		UpdateIsDamageCutProtection();
		if (ShieldInfos.Count > 1)
		{
			return NullVfx.GetInstance();
		}
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase DepriveShield(ShieldInfo shield)
	{
		ShieldInfos.Remove(shield);
		UpdateShield(shield.Type);
		UpdateIsDamageCutProtection();
		if (ShieldInfos.Count <= 0)
		{
			return CreateVfxSkillProtection();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveShield(ShieldInfo.ShieldType type)
	{
		ShieldInfos.RemoveAll((ShieldInfo i) => i.Type == type);
		UpdateShield(type);
		UpdateIsDamageCutProtection();
		return CreateVfxSkillProtection();
	}

	private void UpdateShield(ShieldInfo.ShieldType type)
	{
		switch (type)
		{
		case ShieldInfo.ShieldType.ALL:
			IsShieldAll = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.ALL);
			break;
		case ShieldInfo.ShieldType.SKILL:
			IsShieldSkill = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.SKILL);
			break;
		case ShieldInfo.ShieldType.SPELL:
			IsShieldSpell = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.SPELL);
			break;
		case ShieldInfo.ShieldType.ATTACK:
			IsShieldAttack = ShieldInfos.Any((ShieldInfo i) => i.Type == ShieldInfo.ShieldType.ATTACK);
			break;
		}
	}

	public virtual VfxBase GiveQuick()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveQuick()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveQuick()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveRush(RushInfo info)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveRush(RushInfo info)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveRush()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveSneak()
	{
		SneakCount++;
		if (SneakCount > 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveSneak()
	{
		SneakCount--;
		if (SneakCount >= 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveSneak()
	{
		if (!_card.IsDead)
		{
			SneakCount = 0;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveNotBeAttacked(NotBeAttackedInfo info)
	{
		NotBeAttackedInfoList.Add(info);
		IsNotBeAttacked = NotBeAttackedInfoList.Count > 0;
		if (NotBeAttackedInfoList.Count > 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveNotBeAttacked(NotBeAttackedInfo info)
	{
		NotBeAttackedInfoList.Remove(info);
		IsNotBeAttacked = NotBeAttackedInfoList.Count > 0;
		if (NotBeAttackedInfoList.Count >= 1)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveNotBeAttacked()
	{
		NotBeAttackedInfoList.Clear();
		IsNotBeAttacked = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveUntouchable(string cardType)
	{
		if (cardType != null && cardType == "spell")
		{
			UntouchableBySpellCount++;
			IsUntouchableBySpell = UntouchableBySpellCount > 0;
			if (UntouchableBySpellCount > 1)
			{
				return NullVfx.GetInstance();
			}
		}
		else
		{
			UntouchableCount++;
			IsUntouchable = UntouchableCount > 0;
			if (UntouchableCount > 1)
			{
				return NullVfx.GetInstance();
			}
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveUntouchable(string cardType)
	{
		if (cardType != null && cardType == "spell")
		{
			UntouchableBySpellCount--;
			IsUntouchableBySpell = UntouchableBySpellCount > 0;
			if (UntouchableBySpellCount >= 1)
			{
				return NullVfx.GetInstance();
			}
		}
		else
		{
			UntouchableCount--;
			IsUntouchable = UntouchableCount > 0;
			if (UntouchableCount >= 1)
			{
				return NullVfx.GetInstance();
			}
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveUntouchable(string cardType)
	{
		if (cardType != null && cardType == "spell")
		{
			UntouchableBySpellCount = 0;
			IsUntouchableBySpell = false;
		}
		else
		{
			UntouchableCount = 0;
			IsUntouchable = false;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveAttackByLife(string type)
	{
		switch (type)
		{
		case "attack":
			AttackByLifeTypeAttackCount++;
			IsAttackByLifeTypeAttack = AttackByLifeTypeAttackCount > 0;
			break;
		case "be_attacked":
			AttackByLifeTypeBeAttackedCount++;
			IsAttackByLifeTypeBeAttacked = AttackByLifeTypeBeAttackedCount > 0;
			break;
		default:
			AttackByLifeTypeAttackCount++;
			IsAttackByLifeTypeAttack = AttackByLifeTypeAttackCount > 0;
			AttackByLifeTypeBeAttackedCount++;
			IsAttackByLifeTypeBeAttacked = AttackByLifeTypeBeAttackedCount > 0;
			break;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveAttackByLife(string type)
	{
		switch (type)
		{
		case "attack":
			AttackByLifeTypeAttackCount--;
			IsAttackByLifeTypeAttack = AttackByLifeTypeAttackCount > 0;
			break;
		case "be_attacked":
			AttackByLifeTypeBeAttackedCount--;
			IsAttackByLifeTypeBeAttacked = AttackByLifeTypeBeAttackedCount > 0;
			break;
		default:
			AttackByLifeTypeAttackCount--;
			IsAttackByLifeTypeAttack = AttackByLifeTypeAttackCount > 0;
			AttackByLifeTypeBeAttackedCount--;
			IsAttackByLifeTypeBeAttacked = AttackByLifeTypeBeAttackedCount > 0;
			break;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveAttackByLife(string type)
	{
		switch (type)
		{
		case "attack":
			AttackByLifeTypeAttackCount = 0;
			IsAttackByLifeTypeAttack = false;
			break;
		case "be_attacked":
			AttackByLifeTypeBeAttackedCount = 0;
			IsAttackByLifeTypeBeAttacked = false;
			break;
		default:
			AttackByLifeTypeAttackCount = 0;
			IsAttackByLifeTypeAttack = false;
			AttackByLifeTypeBeAttackedCount = 0;
			IsAttackByLifeTypeBeAttacked = false;
			break;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveCantAttack(int bit_flag, int baseCardId)
	{
		bool isSkillCantAtkAll = IsSkillCantAtkAll;
		SkillCantAtkClassCount += (((bit_flag & Skill_cant_attack.BIT_FLAG_CLASS) != 0) ? 1 : 0);
		IsSkillCantAtkClass = SkillCantAtkClassCount > 0;
		SkillCantAtkUnitCount += (((bit_flag & Skill_cant_attack.BIT_FLAG_UNIT) != 0 && baseCardId == -1) ? 1 : 0);
		IsSkillCantAtkUnit = SkillCantAtkUnitCount > 0;
		SkillCantAtkUnitNotHasGuardCount += (((bit_flag & Skill_cant_attack.BIT_UNIT_NOT_HAS_GUARD) != 0) ? 1 : 0);
		IsSkillCantAtkUnitNotHasGuard = SkillCantAtkUnitNotHasGuardCount > 0;
		bool flag = (bit_flag & Skill_cant_attack.BIT_FLAG_UNIT) != 0 && baseCardId != -1;
		SkillCantAtkUnitBaseCardIdCount += (flag ? 1 : 0);
		IsSkillCantAtkUnitBaseCardId = SkillCantAtkUnitBaseCardIdCount > 0;
		if (flag)
		{
			CantAtkUnitBaseCardIdList.Add(baseCardId);
		}
		if (NeedCreateCantAttackEffect(isSkillCantAtkAll))
		{
			return ParallelVfxPlayer.Create(InstantVfx.Create(delegate
			{
				_card.BattleCardView._inPlayFrameEffect.HideFrameEffect();
			}), NullVfx.GetInstance());
		}
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
		});
	}

	private bool NeedCreateCantAttackEffect(bool isCantAttackEffectAlreadyPlaying)
	{
		if (isCantAttackEffectAlreadyPlaying)
		{
			return false;
		}
		return IsSkillCantAtkAll;
	}

	public virtual VfxBase DepriveCantAttack(int bit_flag, int baseCardId)
	{
		SkillCantAtkClassCount -= (((bit_flag & Skill_cant_attack.BIT_FLAG_CLASS) != 0) ? 1 : 0);
		IsSkillCantAtkClass = SkillCantAtkClassCount > 0;
		SkillCantAtkUnitCount -= (((bit_flag & Skill_cant_attack.BIT_FLAG_UNIT) != 0 && baseCardId == -1) ? 1 : 0);
		IsSkillCantAtkUnit = SkillCantAtkUnitCount > 0;
		SkillCantAtkUnitNotHasGuardCount -= (((bit_flag & Skill_cant_attack.BIT_UNIT_NOT_HAS_GUARD) != 0) ? 1 : 0);
		IsSkillCantAtkUnitNotHasGuard = SkillCantAtkUnitNotHasGuardCount > 0;
		bool flag = (bit_flag & Skill_cant_attack.BIT_FLAG_UNIT) != 0 && baseCardId != -1;
		SkillCantAtkUnitBaseCardIdCount -= (flag ? 1 : 0);
		IsSkillCantAtkUnitBaseCardId = SkillCantAtkUnitBaseCardIdCount > 0;
		if (flag)
		{
			CantAtkUnitBaseCardIdList.Remove(baseCardId);
		}
		if (SkillCantAtkClassCount <= 0 || SkillCantAtkUnitCount <= 0)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			if (_card.Attackable && _card.IsSelfTurn)
			{
				parallelVfxPlayer.Register(InstantVfx.Create(delegate
				{
					_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
				}));
			}
			parallelVfxPlayer.Register(NullVfx.GetInstance());
			return parallelVfxPlayer;
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantAttack()
	{
		SkillCantAtkClassCount = 0;
		IsSkillCantAtkClass = false;
		SkillCantAtkUnitCount = 0;
		IsSkillCantAtkUnit = false;
		SkillCantAtkUnitNotHasGuardCount = 0;
		IsSkillCantAtkUnitNotHasGuard = false;
		SkillCantAtkUnitBaseCardIdCount = 0;
		IsSkillCantAtkUnitBaseCardId = false;
		CantAtkUnitBaseCardIdList.Clear();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (_card.Attackable && _card.IsSelfTurn)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				if (!_card.SelfBattlePlayer.BattleView.IsNowTurnEnd)
				{
					_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
				}
			}));
		}
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		return parallelVfxPlayer;
	}

	public virtual VfxBase ForceDepriveCantAttackAll()
	{
		int num = Math.Min(SkillCantAtkClassCount, SkillCantAtkUnitCount);
		SkillCantAtkClassCount -= num;
		IsSkillCantAtkClass = SkillCantAtkClassCount > 0;
		SkillCantAtkUnitCount -= num;
		IsSkillCantAtkUnit = SkillCantAtkUnitCount > 0;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (_card.Attackable && _card.IsSelfTurn)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				if (!_card.SelfBattlePlayer.BattleView.IsNowTurnEnd)
				{
					_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
				}
			}));
		}
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		return parallelVfxPlayer;
	}

	public virtual VfxBase GiveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantPlay()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveIgnoreGuard()
	{
		IgnoreGuardCount++;
		IsIgnoreGuard = IgnoreGuardCount > 0;
		_ = IgnoreGuardCount;
		_ = 1;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveCantSummon(Skill_cant_summon.CantSummonInfo info)
	{
		CantSummonList.Add(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveCantSummon(Skill_cant_summon.CantSummonInfo info)
	{
		CantSummonList.Remove(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantSummon()
	{
		CantSummonList.Clear();
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveIgnoreGuard()
	{
		IgnoreGuardCount--;
		IsIgnoreGuard = IgnoreGuardCount > 0;
		_ = IgnoreGuardCount;
		_ = 1;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveIgnoreGuard()
	{
		IgnoreGuardCount = 0;
		IsIgnoreGuard = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveAttackCount(Skill_attack_count skill, int count)
	{
		int maxAttackableCount = _card.MaxAttackableCount;
		_card.GiveAttackCount(skill, count);
		int maxAttackableCount2 = _card.MaxAttackableCount;
		if (maxAttackableCount2 > maxAttackableCount)
		{
			_card.AttackableCount += maxAttackableCount2 - maxAttackableCount;
		}
		else if (maxAttackableCount2 < maxAttackableCount)
		{
			_card.AttackableCount = Math.Min(maxAttackableCount2, _card.AttackableCount);
		}
		if (!_card.Attackable)
		{
			return NullVfx.GetInstance();
		}
		return InstantVfx.Create(delegate
		{
			_card.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
		});
	}

	public virtual VfxBase DepriveAttackCount(Skill_attack_count skill)
	{
		int maxAttackableCount = _card.MaxAttackableCount;
		_card.DepriveAttackCount(skill);
		_card.AttackableCount = Math.Min(_card.MaxAttackableCount, _card.AttackableCount);
		if (_card.MaxAttackableCount <= 0 && maxAttackableCount > 0)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveAttackCount()
	{
		_card.ClearAttackCount();
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveInfiniteAttackCount()
	{
		InfiniteAttackCount++;
		IsInfiniteAttack = InfiniteAttackCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveInfiniteAttackCount()
	{
		InfiniteAttackCount--;
		IsInfiniteAttack = InfiniteAttackCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveInfiniteAttackCount()
	{
		InfiniteAttackCount = 0;
		IsInfiniteAttack = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveReflection(ReflectionInfo info)
	{
		ReflectionInfoList.Add(info);
		if (info.Target == ReflectionInfo.TargetType.CLASS)
		{
			ReflectionClassCount++;
			IsReflectionClass = ReflectionClassCount > 0;
			if (ReflectionClassCount <= 1)
			{
				return CreateVfxSkillProtection();
			}
		}
		else if (info.Target == ReflectionInfo.TargetType.DAMAGE_OWNER)
		{
			ReflectionDamageOwnerCount++;
			IsReflectionDamageOwner = ReflectionDamageOwnerCount > 0;
			_ = ReflectionDamageOwnerCount;
			_ = 1;
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveReflection(ReflectionInfo info)
	{
		ReflectionInfoList.Remove(info);
		if (info.Target == ReflectionInfo.TargetType.CLASS)
		{
			ReflectionClassCount--;
			IsReflectionClass = ReflectionClassCount > 0;
			if (ReflectionClassCount <= 0)
			{
				return CreateVfxSkillProtection();
			}
		}
		else if (info.Target == ReflectionInfo.TargetType.DAMAGE_OWNER)
		{
			ReflectionDamageOwnerCount--;
			IsReflectionDamageOwner = ReflectionDamageOwnerCount > 0;
			_ = ReflectionDamageOwnerCount;
			_ = 0;
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveReflection()
	{
		ReflectionClassCount = 0;
		IsReflectionClass = false;
		ReflectionDamageOwnerCount = 0;
		IsReflectionDamageOwner = false;
		ReflectionInfoList.Clear();
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase GiveIndestructible()
	{
		IndestructibleCount++;
		IsIndestructible = IndestructibleCount > 0;
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase DepriveIndestructible()
	{
		IndestructibleCount--;
		IsIndestructible = IndestructibleCount > 0;
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase ForceDepriveIndestructible()
	{
		IndestructibleCount = 0;
		IsIndestructible = false;
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase GiveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveForceAvarice(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveForceAvarice()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveForceAvarice()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveForceWrath(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveForceWrath()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveForceWrath()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase SetCantActivateFanfareCount(int count)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveForceSkillTarget()
	{
		ForceSkillTargetCount++;
		IsForceSkillTarget = ForceSkillTargetCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveForceSkillTarget()
	{
		ForceSkillTargetCount--;
		IsForceSkillTarget = ForceSkillTargetCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveForceSkillTarget()
	{
		ForceSkillTargetCount = 0;
		IsForceSkillTarget = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveAttractSkillTarget()
	{
		AttractSkillTargetCount++;
		IsAttractSkillTarget = AttractSkillTargetCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveAttractSkillTarget()
	{
		AttractSkillTargetCount--;
		IsAttractSkillTarget = AttractSkillTargetCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveAttractSkillTarget()
	{
		AttractSkillTargetCount = 0;
		IsAttractSkillTarget = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveIndependent()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		IndependentCount++;
		IsIndependent = IndependentCount > 0;
		if (IndependentCount == 1)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase DepriveIndependent()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		IndependentCount--;
		IsIndependent = IndependentCount > 0;
		if (IndependentCount == 0)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase ForceDepriveIndependent()
	{
		IndependentCount = 0;
		IsIndependent = false;
		return NullVfx.GetInstance();
	}

	public void GiveNotBeDebuffed()
	{
		NotBeDebuffedCount++;
		IsNotBeDebuffed = NotBeDebuffedCount > 0;
	}

	public void DepriveNotBeDebuffed()
	{
		NotBeDebuffedCount--;
		IsNotBeDebuffed = NotBeDebuffedCount > 0;
	}

	public void ForceDepriveNotBeDebuffed()
	{
		NotBeDebuffedCount = 0;
		IsNotBeDebuffed = false;
	}

	public virtual VfxBase GiveForceAttack(string target, string type)
	{
		ForceAttackUnitCount++;
		IsForceAttackUnit = ForceAttackUnitCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveForceAttack(string target, string type)
	{
		ForceAttackUnitCount--;
		IsForceAttackUnit = ForceAttackUnitCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveForceAttack(string target, string type)
	{
		ForceAttackUnitCount = 0;
		IsForceAttackUnit = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveExtraTurn(int addTurn)
	{
		Player.extraTurnCount += addTurn;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveSkillRandomCount(int randomCount)
	{
		SkillRandomCount = randomCount;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveSkillRandomArray(int[] array)
	{
		SkillRandomArray = array;
		return NullVfx.GetInstance();
	}

	public int GetDamageCutAmount(DamageCutInfo.DamageType type)
	{
		int num = 0;
		for (int i = 0; i < DamageCutList.Count; i++)
		{
			if (DamageCutList[i].Type == type || DamageCutList[i].Type == DamageCutInfo.DamageType.ALL)
			{
				num += DamageCutList[i].CutAmount;
			}
		}
		return num;
	}

	public virtual VfxBase GiveDamageCut(DamageCutInfo info)
	{
		DamageCutCount++;
		IsDamageCut = DamageCutCount > 0;
		UpdateIsDamageCutProtection();
		DamageCutList.Add(info);
		if (DamageCutCount > 1)
		{
			return NullVfx.GetInstance();
		}
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase DepriveDamageCut(DamageCutInfo info)
	{
		DamageCutCount--;
		IsDamageCut = DamageCutCount > 0;
		UpdateIsDamageCutProtection();
		DamageCutList.Remove(info);
		if (DamageCutCount >= 1)
		{
			return NullVfx.GetInstance();
		}
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase FourceDepriveDamageCut()
	{
		DamageCutCount = 0;
		IsDamageCut = false;
		UpdateIsDamageCutProtection();
		DamageCutList.Clear();
		return CreateVfxSkillProtection();
	}

	public int GetClippingDamage(int damage, ParallelVfxPlayer lifeLowerLimitEffectVfx)
	{
		int num = damage;
		for (int i = 0; i < DamageMaxClippingInfo.Count; i++)
		{
			if (DamageMaxClippingInfo[i].LifeLowerLimit > 0)
			{
				if (_card.Life - num >= DamageMaxClippingInfo[i].LifeLowerLimit)
				{
					continue;
				}
				num = _card.Life - DamageMaxClippingInfo[i].LifeLowerLimit;
				DataMgr.SpecialBattleSetting specialBattleSettingInfo = _card.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().SpecialBattleSettingInfo;
				if (lifeLowerLimitEffectVfx != null && !_card.SelfBattlePlayer.BattleMgr.IsRecovery && specialBattleSettingInfo != null && specialBattleSettingInfo.Id == "42")
				{
					lifeLowerLimitEffectVfx.Register(SkillBase.CreateSingleVfx(_card.ResourceMgr, () => _card.BattleCardView.GameObject.transform.position, new List<BattleCardBase> { _card }, _card.IsPlayer, _card.BattleCardView, "btl_nerva_2", EffectMgr.EngineType.SHURIKEN, "se_btl_nerva_2", EffectMgr.MoveType.DIRECT_LEADER, EffectMgr.TargetType.SINGLE, 0f));
				}
			}
			else if (DamageMaxClippingInfo[i].IsClipping(_card, damage))
			{
				num = Mathf.Min(num, DamageMaxClippingInfo[i].ClippingMax);
			}
		}
		return num;
	}

	public VfxBase GiveDamageMaxClipping(DamageClippingInfo clipping)
	{
		bool num = DamageMaxClippingInfo.Count <= 0;
		DamageMaxClippingInfo.Add(clipping);
		UpdateIsDamageCutProtection();
		if (!num)
		{
			return NullVfx.GetInstance();
		}
		return CreateVfxSkillProtection();
	}

	public VfxBase DepriveDamageMaxClipping(DamageClippingInfo clipping)
	{
		for (int i = 0; i < DamageMaxClippingInfo.Count; i++)
		{
			if (DamageMaxClippingInfo[i] == clipping)
			{
				DamageMaxClippingInfo.RemoveAt(i);
				break;
			}
		}
		UpdateIsDamageCutProtection();
		if (DamageMaxClippingInfo.Count > 0)
		{
			return NullVfx.GetInstance();
		}
		return CreateVfxSkillProtection();
	}

	public VfxBase ForceDepriveDamageMaxClipping()
	{
		DamageMaxClippingInfo.Clear();
		UpdateIsDamageCutProtection();
		return CreateVfxSkillProtection();
	}

	public virtual VfxBase GiveTurnStartFixedPP()
	{
		TurnStartFixedPPCount++;
		IsTurnStartFixedPP = TurnStartFixedPPCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveTurnStartFixedPP()
	{
		TurnStartFixedPPCount--;
		IsTurnStartFixedPP = TurnStartFixedPPCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase FourceDepriveTurnStartFixedPP()
	{
		TurnStartFixedPPCount = 0;
		IsTurnStartFixedPP = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo, bool showEffect)
	{
		if (clan != CardBasePrm.ClanType.NONE)
		{
			ClanSkinInfo.Add(clan);
		}
		if (tribeInfo != null)
		{
			TribeSkinInfo.Add(tribeInfo);
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo)
	{
		if (clan != CardBasePrm.ClanType.NONE)
		{
			ClanSkinInfo.Remove(clan);
		}
		if (tribeInfo != null)
		{
			TribeSkinInfo.Remove(tribeInfo);
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveChangeAffiliation()
	{
		if (!_card.IsDead)
		{
			ClanSkinInfo.Clear();
			TribeSkinInfo.Clear();
		}
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveNotConsumeEpModifier(NotConsumeEpModifierInfo info)
	{
		NotConsumeEpModifierInfoList.Add(info);
		IsNotConsumeEp = NotConsumeEpModifierInfoList.Count > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveNotConsumeEpModifier(NotConsumeEpModifierInfo info)
	{
		NotConsumeEpModifierInfoList.Remove(info);
		IsNotConsumeEp = NotConsumeEpModifierInfoList.Count > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveNotConsumeEpModifier()
	{
		NotConsumeEpModifierInfoList.Clear();
		IsNotConsumeEp = false;
		return NullVfx.GetInstance();
	}

	public virtual bool CheckNotConsumeEpCard(BattleCardBase card)
	{
		if (IsNotConsumeEp && NotConsumeEpModifierInfoList.Any((NotConsumeEpModifierInfo s) => s.CheckNotConsumedCard(card)))
		{
			return true;
		}
		return false;
	}

	public virtual VfxBase GiveShortageDeckWin()
	{
		ShortageDeckWinCount++;
		IsShortageDeckWin = ShortageDeckWinCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveShortageDeckWin()
	{
		ShortageDeckWinCount--;
		IsShortageDeckWin = ShortageDeckWinCount > 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveShortageDeckWin()
	{
		ShortageDeckWinCount = 0;
		IsShortageDeckWin = false;
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRemoveByBanish()
	{
		ReturnByBanishCount++;
		DestroyByBanishCount++;
		IsReturnByBanish = ReturnByBanishCount > 0;
		IsDestroyByBanish = DestroyByBanishCount > 0;
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRemoveByBanish()
	{
		ReturnByBanishCount--;
		DestroyByBanishCount--;
		IsReturnByBanish = ReturnByBanishCount > 0;
		IsDestroyByBanish = DestroyByBanishCount > 0;
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRemoveByBanish()
	{
		ReturnByBanishCount = 0;
		DestroyByBanishCount = 0;
		IsReturnByBanish = false;
		IsDestroyByBanish = false;
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRemoveByDestroy()
	{
		BanishByDestroyCount++;
		IsBanishByDestroy = BanishByDestroyCount > 0;
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRemoveByDestroy()
	{
		BanishByDestroyCount--;
		IsBanishByDestroy = BanishByDestroyCount > 0;
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRemoveByDestroy()
	{
		BanishByDestroyCount = 0;
		IsBanishByDestroy = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveRepeatSkill(string repeatTiming, string repeatTargetm, SkillBase skill)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveRepeatSkill(string repeatTiming, string repeatTarget, bool reservation, bool isProcess, SkillProcessor skillProcessor)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ReservationAllDepriveRepeatSkill()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveRepeatSkill()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveAddDamage(DamageModifier info)
	{
		AddDamageList.Add(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveAddDamage(DamageModifier info)
	{
		AddDamageList.Remove(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveAddDamage()
	{
		AddDamageList.Clear();
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveHealModifier(HealModifier info)
	{
		HealModifierList.Add(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveHealModifier(HealModifier info)
	{
		HealModifierList.Remove(info);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveHealModifier()
	{
		HealModifierList.Clear();
		return NullVfx.GetInstance();
	}

	public VfxBase GiveTriggerCount(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		TriggerCount++;
		IsTrigger = TriggerCount > 0;
		return result;
	}

	public VfxBase DepriveTriggerCount()
	{
		TriggerCount--;
		IsTrigger = TriggerCount > 0;
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveTriggerCount()
	{
		TriggerCount = 0;
		IsTrigger = false;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveAddTarget(AddTargetInfo info)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveAddTarget(AddTargetInfo info)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveAddTarget()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveDecreaseTurnStartPP(int value)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveDecreaseTurnStartPP(int value)
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveDecreaseTurnStartPP()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRandomAttack()
	{
		int randomAttackCount = RandomAttackCount + 1;
		RandomAttackCount = randomAttackCount;
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRandomAttack()
	{
		int randomAttackCount = RandomAttackCount - 1;
		RandomAttackCount = randomAttackCount;
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRandomAttack()
	{
		RandomAttackCount = 0;
		return NullVfx.GetInstance();
	}

	public virtual VfxBase GiveCantEvolution(int type)
	{
		CantEvolutionList.Add(type);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase DepriveCantEvolution(int type)
	{
		CantEvolutionList.Remove(type);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ForceDepriveCantEvolution()
	{
		CantEvolutionList.Clear();
		return NullVfx.GetInstance();
	}

	public virtual VfxBase AddRandomSelectedCard(BattleCardBase card)
	{
		RandomSelectedCardList.Add(card);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase RemoveRandomSelectedCard(BattleCardBase card)
	{
		RandomSelectedCardList.Remove(card);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ClearRandomSelectedCard()
	{
		RandomSelectedCardList.Clear();
		return NullVfx.GetInstance();
	}

	public virtual VfxBase AddSkillDrewCard(BattleCardBase card)
	{
		SkillDrewCardList.Add(card);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase RemoveSkillDrewCard(BattleCardBase card)
	{
		SkillDrewCardList.Remove(card);
		return NullVfx.GetInstance();
	}

	public virtual VfxBase ClearSkillDrewCard()
	{
		SkillDrewCardList.Clear();
		return NullVfx.GetInstance();
	}

	public VfxBase GiveChantCount(ICardChantCountModifier chantCountModifier)
	{
		if (chantCountModifier.IsClearBeforeModifier)
		{
			ChantCountModifierList.Clear();
		}
		ChantCountModifierList.Add(chantCountModifier);
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveChantCount(ICardChantCountModifier chantCountModifier)
	{
		ChantCountModifierList.Remove(chantCountModifier);
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveChantCount()
	{
		ChantCountModifierList.Clear();
		return NullVfx.GetInstance();
	}

	public int GetChantCount(int baseChantCount)
	{
		int num = baseChantCount;
		for (int i = 0; i < ChantCountModifierList.Count; i++)
		{
			num = ChantCountModifierList[i].CalcChantCount(num);
		}
		return num;
	}

	public VfxBase AllSkillEffectStop(bool isEvolve = false, bool isReturn = false, bool isBuffed = false, bool isDebuffed = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		bool flag = false;
		if ((_card.Atk > _card.BaseAtk || _card.MaxLife > _card.BaseMaxLife || BuffCount > 0 || isBuffed) && _card.IsUnit)
		{
			flag = true;
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if ((_card.Atk < _card.BaseAtk || _card.MaxLife < _card.BaseMaxLife || isDebuffed) && _card.IsUnit)
		{
			flag = true;
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (flag)
		{
			if (isReturn)
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
			else
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
		}
		if (IsGuard)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsKiller)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsUntouchable)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsUntouchableBySpell)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsNotBeAttacked)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsSneak)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsSkillCantAtkAll)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsIndependent)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsDamageCutProtection || IsIndestructible || IsReflectionClass)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		return parallelVfxPlayer;
	}

	public VfxBase AllSkillEffectRestart()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if ((_card.Atk > _card.BaseAtk || _card.MaxLife > _card.BaseMaxLife) && _card.IsUnit)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if ((_card.Atk < _card.BaseAtk || _card.MaxLife < _card.BaseMaxLife) && _card.IsUnit)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsGuard)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsKiller)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsUntouchable)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsUntouchableBySpell)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsNotBeAttacked)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsSneak)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsSkillCantAtkAll)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsIndependent)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsDamageCutProtection || IsIndestructible || IsReflectionClass)
		{
			parallelVfxPlayer.Register(CreateVfxSkillProtection());
		}
		return parallelVfxPlayer;
	}

	public VfxBase AllSkillEffectStartOnSummon()
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		bool num = _card.BaseParameter.Atk < _card.Atk;
		bool flag = _card.BaseParameter.Atk > _card.Atk;
		bool flag2 = _card.BaseMaxLife < _card.MaxLife;
		bool flag3 = _card.BaseMaxLife > _card.MaxLife;
		if (num || flag2)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (flag || flag3)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		if (IsBuff || IsDebuff || _card.Atk != _card.BaseAtk || _card.Life != _card.BaseMaxLife || _card.MaxLife != _card.BaseMaxLife)
		{
			parallelVfxPlayer.Register(NullVfx.GetInstance());
		}
		return parallelVfxPlayer;
	}

	public VfxBase CreateVfxSkillProtection(bool isForceStop = false)
	{
		if (isForceStop)
		{
			return NullVfx.GetInstance();
		}
		if (_card.IsInHand)
		{
			return NullVfx.GetInstance();
		}
		if (IsReflectionClass)
		{
			return NullVfx.GetInstance();
		}
		if (IsDamageCutProtection && IsIndestructible)
		{
			return NullVfx.GetInstance();
		}
		if (IsIndestructible)
		{
			return NullVfx.GetInstance();
		}
		if (IsDamageCutProtection)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public virtual void AddTokenDrawModifier(TokenDrawModifier modifier)
	{
	}

	public virtual void RemoveTokenDrawModifier(TokenDrawModifier modifier)
	{
	}

	public void SaveTargetList(List<BattleCardBase> targetList)
	{
		SavedTargetList = new List<BattleCardBase>(targetList);
	}

	public List<BattleCardBase> LoadTargetList()
	{
		return SavedTargetList;
	}

	public void SaveTargetCardId(long id, List<int> targetIdList)
	{
		SavedTargetCardIdDict[id] = targetIdList;
	}

	public List<int> LoadTargetCardId(long id)
	{
		if (SavedTargetCardIdDict.ContainsKey(id))
		{
			return SavedTargetCardIdDict[id];
		}
		return new List<int> { -1 };
	}

	public void SaveBurialRiteTargetList(List<BattleCardBase> targetList)
	{
		SavedBurialRiteTargetList = new List<BattleCardBase>(targetList);
	}

	public List<BattleCardBase> LoadBurialRiteTargetList()
	{
		return SavedBurialRiteTargetList;
	}

	public void AddFusionIngredientCard(BattleCardBase card)
	{
		FusionIngredients.Add(new FusionIngredientInfo(_card.SelfBattlePlayer.Turn, card));
	}

	public void AddFusionIngredients(List<FusionIngredientInfo> fusionIngredients)
	{
		FusionIngredients.AddRange(fusionIngredients);
	}

	public int GetFusionCount()
	{
		return FusionIngredients.Select((FusionIngredientInfo f) => f.FusionTurn).Distinct().Count();
	}

	public void AddGetOnCard(BattleCardBase card)
	{
		GetOnCards.Add(card);
	}

	public void ClearGetOnCards()
	{
		GetOnCards.Clear();
	}

	public void AddLastBurialRiteCardList(List<BattleCardBase> cards)
	{
		LastBurialRiteCardList.AddRange(cards);
	}

	public void ClearLastBurialRiteCardList()
	{
		LastBurialRiteCardList.Clear();
	}

	public void GiveNotDecreasePP()
	{
		NotDecreasePPCounter++;
	}

	public void DepriveNotDecreasePP()
	{
		NotDecreasePPCounter--;
	}

	public void GiveLifeZeroActivateLeonSkill()
	{
		IsLifeZeroActivateLeonSkill = true;
	}

	public void DepriveLifeZeroActivateLeonSkill()
	{
		IsLifeZeroActivateLeonSkill = false;
	}

	public void AddSkillHealValue(int healValue)
	{
		SkillHealList.Add(healValue);
		BattlePlayerBase player = Player;
		player.OnEndOneSkillProcess = (Action)Delegate.Combine(player.OnEndOneSkillProcess, (Action)delegate
		{
			SkillHealList.Clear();
		});
	}

	public VfxBase UpdateAllSkillEffectInReplay(List<NetworkBattleReceiver.InplaySkillEffect> inplaySkillEffectList, int inductionNumber, bool isInitialize, bool isOnlyCantAtk = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (IsSkillCantAtkAll != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.SkillCantAtkAll))
		{
			IsSkillCantAtkClass = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.SkillCantAtkAll);
			IsSkillCantAtkUnit = IsSkillCantAtkClass;
			parallelVfxPlayer.Register(IsSkillCantAtkAll ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (isOnlyCantAtk)
		{
			return parallelVfxPlayer;
		}
		if (IsGuard != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Guard))
		{
			IsGuard = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Guard);
			parallelVfxPlayer.Register(IsGuard ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (IsUntouchable != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Untouchable))
		{
			IsUntouchable = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Untouchable);
			parallelVfxPlayer.Register(IsUntouchable ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (IsNotBeAttacked != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.NotBeAttacked))
		{
			IsNotBeAttacked = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.NotBeAttacked);
			parallelVfxPlayer.Register(IsNotBeAttacked ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (IsSneak != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Sneak))
		{
			SneakCount = (inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Sneak) ? 1 : 0);
			parallelVfxPlayer.Register(IsSneak ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (IsIndependent != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Independent))
		{
			IsIndependent = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Independent);
			parallelVfxPlayer.Register(IsIndependent ? NullVfx.GetInstance() : NullVfx.GetInstance());
		}
		if (IsDamageCutProtection != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.DamageCutProtection) || IsIndestructible != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Indestructible) || IsReflectionClass != inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.ReflectionClass))
		{
			IsDamageCutProtection = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.DamageCutProtection);
			IsIndestructible = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.Indestructible);
			IsReflectionClass = inplaySkillEffectList.Contains(NetworkBattleReceiver.InplaySkillEffect.ReflectionClass);
			parallelVfxPlayer.Register((IsDamageCutProtection || IsIndestructible || IsReflectionClass) ? CreateVfxSkillProtection() : NullVfx.GetInstance());
		}
		if (_card.BattleCardView.BattleCardIconAnimations != null)
		{
			parallelVfxPlayer.Register(_card.BattleCardView.BattleCardIconAnimations.UpdateSkillIconInReplay(inplaySkillEffectList, inductionNumber, isInitialize));
		}
		return parallelVfxPlayer;
	}
}
