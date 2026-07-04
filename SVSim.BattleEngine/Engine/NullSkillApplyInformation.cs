using System.Collections.Generic;
using Wizard.Battle;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;

public class NullSkillApplyInformation : ISkillApplyInformation
{
	public List<CantPlayCardFilterInfo> CantPlayFilterList { get; protected set; }

	public int BuffCount => 0;

	public int BuffLifeCount => 0;

	public List<BuffCountInfo> TurnBuffCountList => null;

	public bool IsBuff => false;

	public int DebuffCount => 0;

	public bool IsDebuff => false;

	public List<GuardInfo> GuardInfo => null;

	public bool IsGuard => false;

	public int DrainCount => 0;

	public bool IsDrain => false;

	public int KillerCount => 0;

	public bool IsKiller => false;

	public List<ShieldInfo> ShieldInfos => null;

	public bool IsShieldAll => false;

	public bool IsShieldSkill => false;

	public bool IsShieldSpell => false;

	public bool IsShieldAttack => false;

	public int QuickCount => 0;

	public bool IsQuick => false;

	public List<RushInfo> RushInfo => null;

	public bool IsRush => false;

	public int SneakCount => 0;

	public bool IsSneak => false;

	public int DamageCutCount => 0;

	public bool IsDamageCut => false;

	public int NotBeAttackedCount => 0;

	public int UntouchableCount => 0;

	public bool IsUntouchable => false;

	public bool IsUntouchableBySpell => false;

	public int IgnoreGuardCount => 0;

	public bool IsIgnoreGuard => false;

	public int AttackByLifeTypeBeAttackedCount => 0;

	public bool IsAttackByLifeTypeBeAttacked => false;

	public int AttackByLifeTypeAttackCount => 0;

	public bool IsAttackByLifeTypeAttack => false;

	public int SkillCantAtkClassCount => 0;

	public bool IsSkillCantAtkClass => false;

	public int SkillCantAtkUnitCount => 0;

	public bool IsSkillCantAtkUnit => false;

	public int SkillCantAtkUnitNotHasGuardCount => 0;

	public bool IsSkillCantAtkUnitNotHasGuard => false;

	public int SkillCantAtkUnitBaseCardIdCount => 0;

	public bool IsSkillCantAtkUnitBaseCardId => false;

	public List<int> CantAtkUnitBaseCardIdList => null;

	public bool IsSkillCantAtkAll => false;

	public int ReflectionClassCount => 0;

	public bool IsReflectionClass => false;

	public int ReflectionDamageOwnerCount => 0;

	public bool IsReflectionDamageOwner => false;

	public int InfiniteAttackCount => 0;

	public bool IsInfiniteAttack => false;

	public int IndestructibleCount => 0;

	public bool IsIndestructible => false;

	public int ForceBerserkCount => 0;

	public bool IsForceBerserk => false;

	public int ForceAvariceCount => 0;

	public bool IsForceAvarice => false;

	public int ForceWrathCount => 0;

	public bool IsForceWrath => false;

	public int CantActivateFanfareUnitCount => 0;

	public bool IsCantActivateFanfareUnit => false;

	public int CantActivateFanfareFieldCount => 0;

	public bool IsCantActivateFanfareField => false;

	public int CantActivateShortageDeckWinCount => 0;

	public bool IsCantActivateShortageDeckWin => false;

	public int ForceSkillTargetCount => 0;

	public bool IsForceSkillTarget => false;

	public int AttractSkillTargetCount => 0;

	public bool IsAttractSkillTarget => false;

	public int IndependentCount => 0;

	public bool IsIndependent => false;

	public int NotBeDebuffedCount => 0;

	public bool IsNotBeDebuffed => false;

	public int ForceAttackUnitCount => 0;

	public bool IsForceAttackUnit => false;

	public int SkillRandomCount => 0;

	public int[] SkillRandomArray => null;

	public List<DamageCutInfo> DamageCutList => null;

	public List<ReflectionInfo> ReflectionInfoList => null;

	public int TurnStartFixedPPCount => 0;

	public bool IsTurnStartFixedPP => false;

	public int TriggerCount => 0;

	public bool IsTrigger => false;

	public bool IsNotConsumeEp => false;

	public int ShortageDeckWinCount => 0;

	public bool IsShortageDeckWin => false;

	public int ReturnByBanishCount => 0;

	public bool IsReturnByBanish => false;

	public int DestroyByBanishCount => 0;

	public bool IsDestroyByBanish => false;

	public int BanishByDestroyCount => 0;

	public bool IsBanishByDestroy => false;

	public bool CantBeFocusedSkill => false;

	public bool CantBeFocusedSpell => false;

	public int[] SkillGenericValueArray => null;

	public int NotDecreasePPCounter
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public Dictionary<string, int> SkillGenericKeyAndValue => null;

	public int UnionBurstCount => 0;

	public int SkyboundArtCount => 0;

	public int SuperSkyboundArtCount => 0;

	public int WhiteRitualCount => 0;

	public int RandomAttackCount => 0;

	public bool IsLifeZeroActivateLeonSkill => false;

	public List<DamageClippingInfo> DamageMaxClippingInfo => null;

	public List<CardBasePrm.ClanType> ClanSkinInfo => null;

	public List<CardBasePrm.TribeInfo> TribeSkinInfo => null;

	public List<ICardOffenseModifier> OffenseModifierList => null;

	public List<ICardLifeModifier> LifeModifierList => null;

	public List<ICardChantCountModifier> ChantCountModifierList => null;

	public List<DamageCardParameterModifier> DamageList => null;

	public List<HealCardParameterModifier> HealList => null;

	public List<int> SkillHealList => null;

	public List<ICardLifeModifier> LifeChangeList => null;

	public List<ICardEpModifier> EpModifierList => null;

	public List<NotBeAttackedInfo> NotBeAttackedInfoList => null;

	public bool IsNotBeAttacked => false;

	public List<NotConsumeEpModifierInfo> NotConsumeEpModifierInfoList => null;

	public AttachedSkillInformation AttachedSkillsInfo => null;

	public List<RepeatSkillInfo> RepeatSkillTimingList => null;

	public List<DamageModifier> AddDamageList => null;

	public List<HealModifier> HealModifierList => null;

	public List<AddTargetInfo> AddTargetList => null;

	public List<int> DecreaseTurnStartPPList => null;

	public List<int> CantEvolutionList => null;

	public List<Skill_cant_summon.CantSummonInfo> CantSummonList => null;

	public bool IsDamageCutProtection => false;

	public List<BattleCardBase> RandomSelectedCardList => null;

	public List<BattleCardBase> SkillDrewCardList => null;

	public List<BattleCardBase> LastBurialRiteCardList => null;

	public List<TokenDrawModifier> TokenDrawModifiers => null;

	public List<FusionIngredientInfo> FusionIngredients => null;

	public List<BattleCardBase> GetOnCards { get; }

	public TokenDrawModifier GetTokenDrawModifier(int cardId)
	{
		return null;
	}

	public void InitializeInformation(bool isReturnCard = false)
	{
	}

	public void InitializeInformationWithoutLifeOffenseModifier(bool isReturnCard = false)
	{
	}

	public SkillBase CloneAttachSkill(SkillApplyInformation cloneTarget, SkillBase skill)
	{
		return null;
	}

	public SkillApplyInformation Clone(BattleCardBase card)
	{
		return null;
	}

	public bool IsCantPlay(BattleCardBase card, BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE type = BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE)
	{
		return false;
	}

	public bool HasCantPlaySpellFilter()
	{
		return false;
	}

	public bool HasCantPlayFieldFilter()
	{
		return false;
	}

	public bool CantPlayTransformId(BattleCardBase originalCard)
	{
		return false;
	}

	public SkillBase AttachSkill(SkillCreator.SkillBuildInfo skillBuildInfo, IBattleResourceMgr resourceMgr, string ownerName, int ownerId, long duplicateBanNum, SkillBase originSkill, bool isAttachEvolveSkill)
	{
		return null;
	}

	public void RemoveSkill(SkillBase skill, BattleCardBase skillOwnerCard, long duplicateBanNum, SkillBase originSkill, int createSkillIndex)
	{
	}

	public VfxBase GiveCombatValueModifier(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, SkillProcessor skillProcessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCombatValueModifire(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCombatValueModifire()
	{
		return NullVfx.GetInstance();
	}

	public void AddOffenseModifier(ICardOffenseModifier modifier)
	{
	}

	public void AddLifeModifier(ICardLifeModifier modifier)
	{
	}

	public void ClearParameterModifier()
	{
	}

	public void ClearUnionBurstAndSkyboundArtModifier()
	{
	}

	public void AddEpModifier(ICardEpModifier modifier)
	{
	}

	public void RemoveEpModifier(ICardEpModifier modifier)
	{
	}

	public int GetEp()
	{
		return 0;
	}

	public int GetAtk(bool ignoreLowerLimit = false)
	{
		return 0;
	}

	public int GetLife()
	{
		return 0;
	}

	public bool HasMoreDamageThan(ISkillApplyInformation other)
	{
		return false;
	}

	public int GetMaxLife()
	{
		return 0;
	}

	public int GetLastLife()
	{
		return 0;
	}

	public int GetChangeMaxLifeCount()
	{
		return 0;
	}

	public int GetInitialWhiteRitualStack()
	{
		return 0;
	}

	public void DamageLife(int damage, int turn, bool isSelfTurn)
	{
	}

	public void CausedDamageLife(int damage, int turn, bool isSelfTurn)
	{
	}

	public int GetSpecificTurnDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnCausedDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public List<DamageCardParameterModifier> GetSpecificTurnDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return new List<DamageCardParameterModifier>();
	}

	public List<CausedDamageCardParameterModifier> GetSpecificTurnCausedDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return new List<CausedDamageCardParameterModifier>();
	}

	public int GetSpecificTurnDamageCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnHealValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public List<HealCardParameterModifier> GetSpecificTurnHealValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return new List<HealCardParameterModifier>();
	}

	public int GetSpecificTurnHealCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnHealCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnBuffCount(TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnPpAddCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnAcceleratedCardCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public int GetSpecificTurnAcceleratedCardCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public List<TurnAndIntValue> GetSpecificTurnStartLifeList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return new List<TurnAndIntValue>();
	}

	public int GetSpecificTurnFusionCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo)
	{
		return 0;
	}

	public void SetSkillGenericArray(int[] array)
	{
	}

	public void SetSkillGenericKeyAndValue(string key, int value)
	{
	}

	public bool IsContainGenericValueKey(string key)
	{
		return false;
	}

	public void AddSkillGenericValue(int value, int index)
	{
	}

	public void GiveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier)
	{
	}

	public void DepriveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier)
	{
	}

	public void FourceDepriveUnionBurstCount()
	{
	}

	public void GiveSkyboundArtCount(ICardSkyboundArtCountModifier skyboundArtCountModifier)
	{
	}

	public void GiveSuperSkyboundArtCount(ICardSuperSkyboundArtCountModifier superSkyboundArtCountModifier)
	{
	}

	public void GiveWhiteRitualCount(int value)
	{
	}

	public void DepriveWhiteRitualCount(int value)
	{
	}

	public void FourceDepriveWhiteRitualCount()
	{
	}

	public void HealLife(int healAmount, int turn, bool isSelfTurn)
	{
	}

	public void AddPp(int addPp, int currentTurn, bool isSelfTurn)
	{
	}

	public void GiveBuff(bool isReplace = false)
	{
	}

	public void DepriveBuff()
	{
	}

	public void FourceDepriveBuff()
	{
	}

	public void GiveDebuff()
	{
	}

	public void DepriveDebuff()
	{
	}

	public void FourceDepriveDebuff()
	{
	}

	public void GiveBuffLife()
	{
	}

	public void DepriveBuffLife()
	{
	}

	public void ForceDepriveBuffLife()
	{
	}

	public void Combine(ISkillApplyInformation info)
	{
	}

	public VfxBase GiveGuard(GuardInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveGuard(GuardInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveGuard()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveDrain()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveDrain()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveDrain()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveKiller()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveKiller()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveKiller()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveShield(ShieldInfo shield)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveShield(ShieldInfo shield)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveShield(ShieldInfo.ShieldType type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveQuick()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveQuick()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveQuick()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRush(RushInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRush(RushInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRush()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveSneak()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveSneak()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveSneak()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveNotBeAttacked(NotBeAttackedInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveNotBeAttacked(NotBeAttackedInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveNotBeAttacked()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveUntouchable(string cardType)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveUntouchable(string cardType)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveUntouchable(string cardType)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveAttackByLife(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveAttackByLife(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveAttackByLife(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantAttack(int bit_flag, int baseCardId)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantAttack(int bit_flag, int baseCardId)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantAttack()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantAttackAll()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantPlay()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantSummon(Skill_cant_summon.CantSummonInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantSummon(Skill_cant_summon.CantSummonInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantSummon()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveIgnoreGuard()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveIgnoreGuard()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveIgnoreGuard()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveAttackCount(Skill_attack_count skill, int count)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveAttackCount(Skill_attack_count skill)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveAttackCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveInfiniteAttackCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveInfiniteAttackCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveInfiniteAttackCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveReflection(ReflectionInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveReflection(ReflectionInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveReflection()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveIndestructible()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveIndestructible()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveIndestructible()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveForceBerserk(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveForceAvarice(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveForceAvarice()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveForceAvarice()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveForceWrath(SkillProcessor skillprocessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveForceWrath()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveForceWrath()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase SetCantActivateFanfareCount(int count)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantActivateFanfare(string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantActivateShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveForceSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveForceSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveForceSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveAttractSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveAttractSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveAttractSkillTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveIndependent()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveIndependent()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveIndependent()
	{
		return NullVfx.GetInstance();
	}

	public void GiveNotBeDebuffed()
	{
	}

	public void DepriveNotBeDebuffed()
	{
	}

	public void ForceDepriveNotBeDebuffed()
	{
	}

	public VfxBase GiveForceAttack(string target, string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveForceAttack(string target, string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveForceAttack(string target, string type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveExtraTurn(int addTurn)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveSkillRandomCount(int randomCount)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveSkillRandomArray(int[] array)
	{
		return NullVfx.GetInstance();
	}

	public int GetDamageCutAmount(DamageCutInfo.DamageType type)
	{
		return 0;
	}

	public VfxBase GiveDamageCut(DamageCutInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveDamageCut(DamageCutInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveDamageCut()
	{
		return NullVfx.GetInstance();
	}

	public int GetClippingDamage(int damage, ParallelVfxPlayer lifeLowerLimitEffectVfx)
	{
		return 0;
	}

	public VfxBase GiveDamageMaxClipping(DamageClippingInfo clipping)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveDamageMaxClipping(DamageClippingInfo clipping)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveDamageMaxClipping()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveTurnStartFixedPP()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveTurnStartFixedPP()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase FourceDepriveTurnStartFixedPP()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo, bool showEffect)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveChangeAffiliation()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveNotConsumeEpModifier(NotConsumeEpModifierInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveNotConsumeEpModifier(NotConsumeEpModifierInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveNotConsumeEpModifier()
	{
		return NullVfx.GetInstance();
	}

	public bool CheckNotConsumeEpCard(BattleCardBase card)
	{
		return false;
	}

	public VfxBase GiveShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveShortageDeckWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRemoveByBanish()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRemoveByBanish()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRemoveByBanish()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRemoveByDestroy()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRemoveByDestroy()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRemoveByDestroy()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRepeatSkill(string repeatTiming, string repeatTarget, SkillBase skill)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRepeatSkill(string repeatTiming, string repeatTarget, bool reservation, bool isProcess, SkillProcessor skillProcessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ReservationAllDepriveRepeatSkill()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRepeatSkill()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveAddDamage(DamageModifier info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveAddDamage(DamageModifier info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveAddDamage()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveHealModifier(HealModifier info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveHealModifier(HealModifier info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveHealModifier()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveTriggerCount(SkillProcessor skillProcessor)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveTriggerCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveTriggerCount()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveAddTarget(AddTargetInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveAddTarget(AddTargetInfo info)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveAddTarget()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveDecreaseTurnStartPP(int value)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveDecreaseTurnStartPP(int value)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveDecreaseTurnStartPP()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveRandomAttack()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveRandomAttack()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveRandomAttack()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase GiveCantEvolution(int type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveCantEvolution(int type)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveCantEvolution()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase AddRandomSelectedCard(BattleCardBase card)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase RemoveRandomSelectedCard(BattleCardBase card)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ClearRandomSelectedCard()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase AddSkillDrewCard(BattleCardBase card)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase RemoveSkillDrewCard(BattleCardBase card)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ClearSkillDrewCard()
	{
		return NullVfx.GetInstance();
	}

	public void AddTokenDrawModifier(TokenDrawModifier modifier)
	{
	}

	public void RemoveTokenDrawModifier(TokenDrawModifier modifier)
	{
	}

	public void SaveTargetList(List<BattleCardBase> targetList)
	{
	}

	public List<BattleCardBase> LoadTargetList()
	{
		return null;
	}

	public void SaveTargetCardId(long id, List<int> targetIdList)
	{
	}

	public List<int> LoadTargetCardId(long id)
	{
		return null;
	}

	public void SaveBurialRiteTargetList(List<BattleCardBase> targetList)
	{
	}

	public List<BattleCardBase> LoadBurialRiteTargetList()
	{
		return null;
	}

	public VfxBase GiveChantCount(ICardChantCountModifier chantCountModifier)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase DepriveChantCount(ICardChantCountModifier chantCountModifier)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase ForceDepriveChantCount()
	{
		return NullVfx.GetInstance();
	}

	public int GetChantCount(int baseChantCount)
	{
		return 0;
	}

	public VfxBase AllSkillEffectStop(bool isEvolve = false, bool isReturn = false, bool isBuffed = false, bool isDebuffed = false)
	{
		return NullVfx.GetInstance();
	}

	public VfxBase AllSkillEffectRestart()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase AllSkillEffectStartOnSummon()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase CreateVfxSkillProtection(bool isForceStop = false)
	{
		return NullVfx.GetInstance();
	}

	public void AddFusionIngredientCard(BattleCardBase card)
	{
	}

	public void AddFusionIngredients(List<FusionIngredientInfo> fusionIngredients)
	{
	}

	public int GetFusionCount()
	{
		return 0;
	}

	public void AddGetOnCard(BattleCardBase card)
	{
	}

	public void ClearGetOnCards()
	{
	}

	public void AddLastBurialRiteCardList(List<BattleCardBase> cards)
	{
	}

	public void ClearLastBurialRiteCardList()
	{
	}

	public void GiveNotDecreasePP()
	{
	}

	public void DepriveNotDecreasePP()
	{
	}

	public void GiveLifeZeroActivateLeonSkill()
	{
	}

	public void DepriveLifeZeroActivateLeonSkill()
	{
	}

	public void AddSkillHealValue(int healValue)
	{
	}

	public VfxBase UpdateAllSkillEffectInReplay(List<NetworkBattleReceiver.InplaySkillEffect> inplaySkillEffectList, int inductionNumber, bool isInitialize, bool isOnlyCantAtk = false)
	{
		return NullVfx.GetInstance();
	}
}
