using System.Collections.Generic;
using Wizard.Battle;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;

public interface ISkillApplyInformation
{
	List<CantPlayCardFilterInfo> CantPlayFilterList { get; }

	int BuffCount { get; }

	int BuffLifeCount { get; }

	List<BuffCountInfo> TurnBuffCountList { get; }

	bool IsBuff { get; }

	int DebuffCount { get; }

	bool IsDebuff { get; }

	List<GuardInfo> GuardInfo { get; }

	bool IsGuard { get; }

	int DrainCount { get; }

	bool IsDrain { get; }

	int KillerCount { get; }

	bool IsKiller { get; }

	List<ShieldInfo> ShieldInfos { get; }

	bool IsShieldAll { get; }

	bool IsShieldSkill { get; }

	bool IsShieldSpell { get; }

	bool IsShieldAttack { get; }

	int QuickCount { get; }

	bool IsQuick { get; }

	List<RushInfo> RushInfo { get; }

	bool IsRush { get; }

	int SneakCount { get; }

	bool IsSneak { get; }

	int DamageCutCount { get; }

	bool IsDamageCut { get; }

	int NotBeAttackedCount { get; }

	int UntouchableCount { get; }

	bool IsUntouchable { get; }

	bool IsUntouchableBySpell { get; }

	int IgnoreGuardCount { get; }

	bool IsIgnoreGuard { get; }

	int AttackByLifeTypeAttackCount { get; }

	bool IsAttackByLifeTypeAttack { get; }

	int AttackByLifeTypeBeAttackedCount { get; }

	bool IsAttackByLifeTypeBeAttacked { get; }

	int SkillCantAtkClassCount { get; }

	bool IsSkillCantAtkClass { get; }

	int SkillCantAtkUnitCount { get; }

	bool IsSkillCantAtkUnit { get; }

	int SkillCantAtkUnitNotHasGuardCount { get; }

	bool IsSkillCantAtkUnitNotHasGuard { get; }

	int SkillCantAtkUnitBaseCardIdCount { get; }

	bool IsSkillCantAtkUnitBaseCardId { get; }

	List<int> CantAtkUnitBaseCardIdList { get; }

	bool IsSkillCantAtkAll { get; }

	int ReflectionClassCount { get; }

	bool IsReflectionClass { get; }

	int ReflectionDamageOwnerCount { get; }

	bool IsReflectionDamageOwner { get; }

	int InfiniteAttackCount { get; }

	bool IsInfiniteAttack { get; }

	int IndestructibleCount { get; }

	bool IsIndestructible { get; }

	int ForceBerserkCount { get; }

	bool IsForceBerserk { get; }

	int ForceAvariceCount { get; }

	bool IsForceAvarice { get; }

	int ForceWrathCount { get; }

	bool IsForceWrath { get; }

	int CantActivateFanfareUnitCount { get; }

	bool IsCantActivateFanfareUnit { get; }

	int CantActivateFanfareFieldCount { get; }

	bool IsCantActivateFanfareField { get; }

	int CantActivateShortageDeckWinCount { get; }

	bool IsCantActivateShortageDeckWin { get; }

	int ForceSkillTargetCount { get; }

	bool IsForceSkillTarget { get; }

	int AttractSkillTargetCount { get; }

	bool IsAttractSkillTarget { get; }

	int IndependentCount { get; }

	bool IsIndependent { get; }

	int NotBeDebuffedCount { get; }

	bool IsNotBeDebuffed { get; }

	int ForceAttackUnitCount { get; }

	bool IsForceAttackUnit { get; }

	int SkillRandomCount { get; }

	int[] SkillRandomArray { get; }

	List<DamageCutInfo> DamageCutList { get; }

	List<ReflectionInfo> ReflectionInfoList { get; }

	int TurnStartFixedPPCount { get; }

	bool IsTurnStartFixedPP { get; }

	int TriggerCount { get; }

	bool IsTrigger { get; }

	bool IsNotConsumeEp { get; }

	int ShortageDeckWinCount { get; }

	bool IsShortageDeckWin { get; }

	int ReturnByBanishCount { get; }

	bool IsReturnByBanish { get; }

	int DestroyByBanishCount { get; }

	bool IsDestroyByBanish { get; }

	int BanishByDestroyCount { get; }

	bool IsBanishByDestroy { get; }

	bool CantBeFocusedSkill { get; }

	bool CantBeFocusedSpell { get; }

	int[] SkillGenericValueArray { get; }

	Dictionary<string, int> SkillGenericKeyAndValue { get; }

	int UnionBurstCount { get; }

	int SkyboundArtCount { get; }

	int SuperSkyboundArtCount { get; }

	int WhiteRitualCount { get; }

	int RandomAttackCount { get; }

	int NotDecreasePPCounter { get; }

	bool IsLifeZeroActivateLeonSkill { get; }

	List<DamageClippingInfo> DamageMaxClippingInfo { get; }

	List<CardBasePrm.ClanType> ClanSkinInfo { get; }

	List<CardBasePrm.TribeInfo> TribeSkinInfo { get; }

	List<ICardOffenseModifier> OffenseModifierList { get; }

	List<ICardLifeModifier> LifeModifierList { get; }

	List<ICardChantCountModifier> ChantCountModifierList { get; }

	List<DamageCardParameterModifier> DamageList { get; }

	List<HealCardParameterModifier> HealList { get; }

	List<int> SkillHealList { get; }

	List<ICardLifeModifier> LifeChangeList { get; }

	List<ICardEpModifier> EpModifierList { get; }

	List<NotBeAttackedInfo> NotBeAttackedInfoList { get; }

	bool IsNotBeAttacked { get; }

	List<NotConsumeEpModifierInfo> NotConsumeEpModifierInfoList { get; }

	AttachedSkillInformation AttachedSkillsInfo { get; }

	List<RepeatSkillInfo> RepeatSkillTimingList { get; }

	List<DamageModifier> AddDamageList { get; }

	List<HealModifier> HealModifierList { get; }

	List<AddTargetInfo> AddTargetList { get; }

	List<int> DecreaseTurnStartPPList { get; }

	List<int> CantEvolutionList { get; }

	List<Skill_cant_summon.CantSummonInfo> CantSummonList { get; }

	bool IsDamageCutProtection { get; }

	List<BattleCardBase> RandomSelectedCardList { get; }

	List<BattleCardBase> SkillDrewCardList { get; }

	List<BattleCardBase> LastBurialRiteCardList { get; }

	List<TokenDrawModifier> TokenDrawModifiers { get; }

	List<FusionIngredientInfo> FusionIngredients { get; }

	List<BattleCardBase> GetOnCards { get; }

	TokenDrawModifier GetTokenDrawModifier(int cardId);

	void InitializeInformation(bool isReturnCard = false);

	void InitializeInformationWithoutLifeOffenseModifier(bool isReturnCard = false);

	SkillBase CloneAttachSkill(SkillApplyInformation cloneTarget, SkillBase skill);

	SkillApplyInformation Clone(BattleCardBase card);

	void Combine(ISkillApplyInformation info);

	bool IsCantPlay(BattleCardBase card, BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE type = BattleCardBase.CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE);

	bool HasCantPlaySpellFilter();

	bool HasCantPlayFieldFilter();

	bool CantPlayTransformId(BattleCardBase originalCard);

	SkillBase AttachSkill(SkillCreator.SkillBuildInfo skillBuildInfo, IBattleResourceMgr resourceMgr, string ownerName, int ownerId, long duplicateBanNum, SkillBase originSkill, bool isAttachEvolveSkill = false);

	void RemoveSkill(SkillBase skill, BattleCardBase skillOwnerCard, long duplicateBanNum, SkillBase originSkill, int creatorSkillIndex);

	VfxBase GiveCombatValueModifier(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, SkillProcessor skillProcessor);

	VfxBase DepriveCombatValueModifire(ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier);

	VfxBase ForceDepriveCombatValueModifire();

	void AddOffenseModifier(ICardOffenseModifier modifier);

	void AddLifeModifier(ICardLifeModifier modifier);

	void ClearParameterModifier();

	void ClearUnionBurstAndSkyboundArtModifier();

	void AddEpModifier(ICardEpModifier modifier);

	void RemoveEpModifier(ICardEpModifier modifier);

	int GetEp();

	int GetAtk(bool ignoreLowerLimit = false);

	int GetLife();

	bool HasMoreDamageThan(ISkillApplyInformation other);

	int GetMaxLife();

	int GetLastLife();

	int GetChangeMaxLifeCount();

	int GetInitialWhiteRitualStack();

	void DamageLife(int damage, int turn, bool isSelfTurn);

	void CausedDamageLife(int damage, int turn, bool isSelfTurn);

	void HealLife(int healAmount, int turn, bool isSelfTurn);

	void AddPp(int addPp, int currentTurn, bool isSelfTurn);

	int GetSpecificTurnDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	List<DamageCardParameterModifier> GetSpecificTurnDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnCausedDamageValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	List<CausedDamageCardParameterModifier> GetSpecificTurnCausedDamageValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnDamageCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnHealValue(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	List<HealCardParameterModifier> GetSpecificTurnHealValueList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnHealCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnBuffCount(TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnHealCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnPpAddCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnAcceleratedCardCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnAcceleratedCardCountOnlySelf(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	List<TurnAndIntValue> GetSpecificTurnStartLifeList(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	int GetSpecificTurnFusionCount(IReadOnlyBattleCardInfo cardInfo, TurnPlayerInfo turnPlayerInfo);

	void SetSkillGenericArray(int[] array);

	void AddSkillGenericValue(int value, int index);

	void SetSkillGenericKeyAndValue(string key, int value);

	bool IsContainGenericValueKey(string key);

	void GiveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier);

	void DepriveUnionBurstCount(ICardUnionBurstCountModifier unionBurstCountModifier);

	void FourceDepriveUnionBurstCount();

	void GiveSkyboundArtCount(ICardSkyboundArtCountModifier skyboundArtCountModifier);

	void GiveSuperSkyboundArtCount(ICardSuperSkyboundArtCountModifier superSkyboundArtCountModifier);

	void GiveWhiteRitualCount(int value);

	void DepriveWhiteRitualCount(int value);

	void FourceDepriveWhiteRitualCount();

	void GiveBuff(bool isReplace = false);

	void DepriveBuff();

	void FourceDepriveBuff();

	void GiveDebuff();

	void DepriveDebuff();

	void FourceDepriveDebuff();

	void GiveBuffLife();

	void DepriveBuffLife();

	void ForceDepriveBuffLife();

	VfxBase GiveGuard(GuardInfo info);

	VfxBase DepriveGuard(GuardInfo info);

	VfxBase ForceDepriveGuard();

	VfxBase GiveDrain();

	VfxBase DepriveDrain();

	VfxBase FourceDepriveDrain();

	VfxBase GiveKiller();

	VfxBase DepriveKiller();

	VfxBase FourceDepriveKiller();

	VfxBase GiveShield(ShieldInfo shield);

	VfxBase DepriveShield(ShieldInfo shield);

	VfxBase FourceDepriveShield(ShieldInfo.ShieldType type);

	VfxBase GiveQuick();

	VfxBase DepriveQuick();

	VfxBase ForceDepriveQuick();

	VfxBase GiveRush(RushInfo info);

	VfxBase DepriveRush(RushInfo info);

	VfxBase ForceDepriveRush();

	VfxBase GiveSneak();

	VfxBase DepriveSneak();

	VfxBase FourceDepriveSneak();

	VfxBase GiveNotBeAttacked(NotBeAttackedInfo info);

	VfxBase DepriveNotBeAttacked(NotBeAttackedInfo info);

	VfxBase FourceDepriveNotBeAttacked();

	VfxBase GiveUntouchable(string cardType);

	VfxBase DepriveUntouchable(string cardType);

	VfxBase FourceDepriveUntouchable(string cardType);

	VfxBase GiveAttackByLife(string type);

	VfxBase DepriveAttackByLife(string type);

	VfxBase FourceDepriveAttackByLife(string type);

	VfxBase GiveCantAttack(int bit_flag, int baseCardId);

	VfxBase DepriveCantAttack(int bit_flag, int baseCardId);

	VfxBase ForceDepriveCantAttack();

	VfxBase ForceDepriveCantAttackAll();

	VfxBase GiveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter);

	VfxBase DepriveCantPlay(CantPlayCardFilterInfo cantPlayCardFilter);

	VfxBase ForceDepriveCantPlay();

	VfxBase GiveCantSummon(Skill_cant_summon.CantSummonInfo info);

	VfxBase DepriveCantSummon(Skill_cant_summon.CantSummonInfo info);

	VfxBase ForceDepriveCantSummon();

	VfxBase GiveIgnoreGuard();

	VfxBase DepriveIgnoreGuard();

	VfxBase FourceDepriveIgnoreGuard();

	VfxBase GiveAttackCount(Skill_attack_count skill, int count);

	VfxBase DepriveAttackCount(Skill_attack_count skill);

	VfxBase ForceDepriveAttackCount();

	VfxBase GiveInfiniteAttackCount();

	VfxBase DepriveInfiniteAttackCount();

	VfxBase ForceDepriveInfiniteAttackCount();

	VfxBase GiveReflection(ReflectionInfo info);

	VfxBase DepriveReflection(ReflectionInfo info);

	VfxBase ForceDepriveReflection();

	VfxBase GiveIndestructible();

	VfxBase DepriveIndestructible();

	VfxBase ForceDepriveIndestructible();

	VfxBase GiveForceBerserk(SkillProcessor skillprocessor);

	VfxBase DepriveForceBerserk(SkillProcessor skillprocessor);

	VfxBase ForceDepriveForceBerserk(SkillProcessor skillprocessor);

	VfxBase GiveForceAvarice(SkillProcessor skillprocessor);

	VfxBase DepriveForceAvarice();

	VfxBase ForceDepriveForceAvarice();

	VfxBase GiveForceWrath(SkillProcessor skillprocessor);

	VfxBase DepriveForceWrath();

	VfxBase ForceDepriveForceWrath();

	VfxBase GiveCantActivateFanfare(string type);

	VfxBase SetCantActivateFanfareCount(int count);

	VfxBase DepriveCantActivateFanfare(string type);

	VfxBase ForceDepriveCantActivateFanfare(string type);

	VfxBase GiveCantActivateShortageDeckWin();

	VfxBase DepriveCantActivateShortageDeckWin();

	VfxBase ForceDepriveCantActivateShortageDeckWin();

	VfxBase GiveForceSkillTarget();

	VfxBase DepriveForceSkillTarget();

	VfxBase ForceDepriveForceSkillTarget();

	VfxBase GiveAttractSkillTarget();

	VfxBase DepriveAttractSkillTarget();

	VfxBase ForceDepriveAttractSkillTarget();

	VfxBase GiveIndependent();

	VfxBase DepriveIndependent();

	VfxBase ForceDepriveIndependent();

	void GiveNotBeDebuffed();

	void DepriveNotBeDebuffed();

	void ForceDepriveNotBeDebuffed();

	VfxBase GiveForceAttack(string target, string type);

	VfxBase DepriveForceAttack(string target, string type);

	VfxBase ForceDepriveForceAttack(string target, string type);

	VfxBase GiveExtraTurn(int addTurn);

	VfxBase GiveSkillRandomCount(int randomCount);

	VfxBase GiveSkillRandomArray(int[] array);

	int GetDamageCutAmount(DamageCutInfo.DamageType type);

	VfxBase GiveDamageCut(DamageCutInfo info);

	VfxBase DepriveDamageCut(DamageCutInfo info);

	VfxBase FourceDepriveDamageCut();

	int GetClippingDamage(int damage, ParallelVfxPlayer lifeLowerLimitEffectVfx);

	VfxBase GiveDamageMaxClipping(DamageClippingInfo clipping);

	VfxBase DepriveDamageMaxClipping(DamageClippingInfo clipping);

	VfxBase ForceDepriveDamageMaxClipping();

	VfxBase GiveTurnStartFixedPP();

	VfxBase DepriveTurnStartFixedPP();

	VfxBase FourceDepriveTurnStartFixedPP();

	VfxBase GiveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo, bool showEffect);

	VfxBase DepriveChangeAffiliation(CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribeInfo);

	VfxBase ForceDepriveChangeAffiliation();

	VfxBase GiveNotConsumeEpModifier(NotConsumeEpModifierInfo info);

	VfxBase DepriveNotConsumeEpModifier(NotConsumeEpModifierInfo info);

	VfxBase ForceDepriveNotConsumeEpModifier();

	bool CheckNotConsumeEpCard(BattleCardBase card);

	VfxBase GiveShortageDeckWin();

	VfxBase DepriveShortageDeckWin();

	VfxBase ForceDepriveShortageDeckWin();

	VfxBase GiveRemoveByBanish();

	VfxBase DepriveRemoveByBanish();

	VfxBase ForceDepriveRemoveByBanish();

	VfxBase GiveRemoveByDestroy();

	VfxBase DepriveRemoveByDestroy();

	VfxBase ForceDepriveRemoveByDestroy();

	VfxBase GiveTriggerCount(SkillProcessor skillProcessor);

	VfxBase DepriveTriggerCount();

	VfxBase ForceDepriveTriggerCount();

	VfxBase AllSkillEffectStop(bool isEvolve = false, bool isReturn = false, bool isBuffed = false, bool isDebuffed = false);

	VfxBase GiveRepeatSkill(string repeatTiming, string repeatTarget, SkillBase skill);

	VfxBase DepriveRepeatSkill(string repeatTiming, string repeatTarget, bool reservation, bool isProcess, SkillProcessor skillProcessor);

	VfxBase ReservationAllDepriveRepeatSkill();

	VfxBase ForceDepriveRepeatSkill();

	VfxBase GiveAddDamage(DamageModifier info);

	VfxBase DepriveAddDamage(DamageModifier info);

	VfxBase ForceDepriveAddDamage();

	VfxBase GiveHealModifier(HealModifier info);

	VfxBase DepriveHealModifier(HealModifier info);

	VfxBase ForceDepriveHealModifier();

	VfxBase GiveAddTarget(AddTargetInfo info);

	VfxBase DepriveAddTarget(AddTargetInfo info);

	VfxBase ForceDepriveAddTarget();

	VfxBase GiveDecreaseTurnStartPP(int value);

	VfxBase DepriveDecreaseTurnStartPP(int value);

	VfxBase ForceDepriveDecreaseTurnStartPP();

	VfxBase GiveRandomAttack();

	VfxBase DepriveRandomAttack();

	VfxBase ForceDepriveRandomAttack();

	VfxBase GiveCantEvolution(int type);

	VfxBase DepriveCantEvolution(int type);

	VfxBase ForceDepriveCantEvolution();

	VfxBase AddRandomSelectedCard(BattleCardBase card);

	VfxBase RemoveRandomSelectedCard(BattleCardBase card);

	VfxBase ClearRandomSelectedCard();

	VfxBase AddSkillDrewCard(BattleCardBase card);

	VfxBase RemoveSkillDrewCard(BattleCardBase card);

	VfxBase ClearSkillDrewCard();

	VfxBase AllSkillEffectRestart();

	VfxBase AllSkillEffectStartOnSummon();

	VfxBase CreateVfxSkillProtection(bool isForceStop = false);

	void AddTokenDrawModifier(TokenDrawModifier modifier);

	void RemoveTokenDrawModifier(TokenDrawModifier modifier);

	void SaveTargetList(List<BattleCardBase> targetList);

	List<BattleCardBase> LoadTargetList();

	void SaveTargetCardId(long id, List<int> targetIdList);

	List<int> LoadTargetCardId(long id);

	void SaveBurialRiteTargetList(List<BattleCardBase> targetList);

	List<BattleCardBase> LoadBurialRiteTargetList();

	VfxBase GiveChantCount(ICardChantCountModifier chantCountModifier);

	VfxBase DepriveChantCount(ICardChantCountModifier chantCountModifier);

	VfxBase ForceDepriveChantCount();

	int GetChantCount(int baseChantCount);

	void AddFusionIngredientCard(BattleCardBase card);

	void AddFusionIngredients(List<FusionIngredientInfo> fusionIngredients);

	int GetFusionCount();

	void AddGetOnCard(BattleCardBase card);

	void ClearGetOnCards();

	void AddLastBurialRiteCardList(List<BattleCardBase> cards);

	void ClearLastBurialRiteCardList();

	void GiveNotDecreasePP();

	void DepriveNotDecreasePP();

	void GiveLifeZeroActivateLeonSkill();

	void DepriveLifeZeroActivateLeonSkill();

	void AddSkillHealValue(int healValue);

	VfxBase UpdateAllSkillEffectInReplay(List<NetworkBattleReceiver.InplaySkillEffect> inplaySkillEffectList, int inductionNumber, bool isInitialize, bool isOnlyCantAtk = false);
}
