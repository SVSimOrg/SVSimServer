using System.Collections.Generic;

namespace Wizard.Battle;

public interface IReadOnlyBattleCardInfo : IBattleCardUniqueID
{
	CardParameter BaseParameter { get; }

	List<CardBasePrm.TribeType> Tribe { get; }

	CardBasePrm.ClanType Clan { get; }

	int CardId { get; }

	int Cost { get; }

	int BaseCost { get; }

	int PlayedCost { get; }

	int LastCost { get; }

	int Atk { get; }

	int Life { get; }

	int MaxLife { get; }

	int ChantCount { get; }

	int SpellChargeCount { get; }

	int SkillActivatedCount { get; }

	int ThisTurnSkillActivatedCount { get; }

	List<BattleCardBase.SkillActivationInfo> SkillActivationList { get; }

	ISkillApplyInformation SkillApplyInformation { get; }

	bool IsEvolution { get; }

	bool IsDead { get; }

	bool IsLifeZeroDead { get; }

	bool IsInplay { get; }

	bool IsInHand { get; }

	bool IsInDeck { get; }

	bool IsReanimate { get; }

	int DestroyedTurn { get; }

	bool IsDestroySelfTurn { get; }

	List<BattleCardBase.DestroyedBySkillInfo> DestroyedBySkillList { get; }

	BattleCardBase.DeathTypeInformation DeathTypeInfo { get; }

	BattleCardBase.BanishInfo BanishedInfo { get; }

	SkillBase DiscardedSkill { get; }

	SkillBase ReturnedSkill { get; }

	bool IsSelfTurn { get; }

	bool IsClass { get; }

	bool IsUnit { get; }

	bool IsSpell { get; }

	bool IsField { get; }

	bool IsChantField { get; }

	bool HasSpellCharge { get; }

	bool HasAnySkill { get; }

	int FixedUseCost { get; }

	List<int> UseCostList { get; }

	bool HasSkillFixedUseCost { get; }

	bool HasSkillAccelerate { get; }

	bool HasSkillEnhance { get; }

	bool HasSkillCrystallize { get; }

	bool HasSkillDestroyWhiteRitual { get; }

	bool HasSkillStackWhiteRitual { get; }

	int AttackableCount { get; }

	int MaxAttackableCount { get; }

	int ExecutedFixedUseCostIndex { get; }

	bool IsExecutedEarthRite { get; }

	bool IsSkillLost { get; }

	bool HasSkillWhenDestroy { get; }

	bool HasWhenAttack { get; }

	bool HasWhenFight { get; }

	bool HasUnionBurst { get; }

	bool HasSkyboundArt { get; }

	bool HasSuperSkyboundArt { get; }

	bool HasSkillBurialRite { get; }

	bool HasSkillReanimate { get; }

	bool HasSkillFusion { get; }

	bool HasSkillWhenEvolve { get; }

	bool IsCantActivateFanfare { get; }

	BattleCardBase.ItWasDamagedCounter DamagedCounter { get; }

	BattleCardBase.TransformInformation TransformInfo { get; }

	int PlayedTurn { get; }

	int DrawTurn { get; }

	List<BattleCardBase> FusionIngredients { get; }

	int FusionedTurn { get; }

	List<BattleCardBase> GetOnCards { get; }

	List<BattleCardBase> GetOffCards { get; }

	BattleCardBase MetamorphoseCard { get; }

	BattleCardBase FinalMetamorphoseCard { get; }

	BattlePlayerBase SelfBattlePlayer { get; }

	BattlePlayerBase OpponentBattlePlayer { get; }

	bool IsTribe(CardBasePrm.TribeType tribe);

	bool HasSkillWhenPlay(bool isOnlyNoSelect);
}
