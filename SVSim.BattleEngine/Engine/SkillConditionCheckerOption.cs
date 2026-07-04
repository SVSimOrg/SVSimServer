using System.Collections.Generic;
using System.Linq;
using Wizard.Battle;

public class SkillConditionCheckerOption
{
	public class SkillAndSelectTarget
	{
		public BattleCardBase SelectCard { get; private set; }

		public SkillBase SelectSkill { get; private set; }

		public SkillAndSelectTarget(BattleCardBase card, SkillBase skill)
		{
			SelectCard = card;
			SelectSkill = skill;
		}
	}

	private int _skillTargetConditionIndex;

	public BattleCardBase PlayedCard { get; set; }

	public BattleCardBase EnhanceCard { get; set; }

	public BattleCardBase AcceleratedCard { get; set; }

	public BattleCardBase CrystallizedCard { get; set; }

	public BattleCardBase SummonedCard { get; set; }

	public BattleCardBase AttackerCard { get; set; }

	public BattleCardBase AttackTargetCard { get; set; }

	public BattleCardBase DestroyedCard { get; set; }

	public BattleCardBase BanishedCard { get; set; }

	public BattleCardBase NecromanceCard { get; set; }

	public List<IReadOnlyBattleCardInfo> LeftCards { get; set; }

	public int NecromanceCount { get; set; }

	public int LastUsedWhiteRitualStackCount { get; set; }

	public List<BattleCardBase> BurialRiteCards { get; set; }

	public List<BattleCardBase> GetOffCards { get; set; }

	public List<int> ChosenCards { get; set; }

	public List<IReadOnlyBattleCardInfo> InHandCard { get; set; }

	public List<IReadOnlyBattleCardInfo> SkillDrewCards { get; set; }

	public List<IReadOnlyBattleCardInfo> SkillUpdatedDeckCards { get; set; }

	public List<BattlePlayerBase.CardAndValue> HealingCardAndValue { get; set; }

	public List<IReadOnlyBattleCardInfo> DamageCards { get; set; }

	public List<IReadOnlyBattleCardInfo> EvolutionCards { get; set; }

	public List<IReadOnlyBattleCardInfo> FusionIngredientCards { get; set; }

	public List<IReadOnlyBattleCardInfo> Discards { get; set; }

	public List<SkillAndSelectTarget> SelectedCards { get; set; }

	public List<IReadOnlyBattleCardInfo> ReturnCards { get; set; }

	public List<IReadOnlyBattleCardInfo> CantAttackAllReturnCards { get; set; }

	public List<IReadOnlyBattleCardInfo> NextTargetCards { get; set; }

	public List<IReadOnlyBattleCardInfo> ReanimatedCards { get; set; }

	public List<IReadOnlyBattleCardInfo> DeckSelfSummonedCards { get; set; }

	public List<IReadOnlyBattleCardInfo> TokenDrewCards { get; set; }

	public List<IReadOnlyBattleCardInfo> DeckDrewCards { get; set; }

	public List<IReadOnlyBattleCardInfo> DrewOverHandLimitCards { get; set; }

	public List<IReadOnlyBattleCardInfo> InplayBuffingCards { get; set; }

	public List<IReadOnlyBattleCardInfo> InplayDebuffingCards { get; set; }

	public List<IReadOnlyBattleCardInfo> ChantCountChangeCard { get; set; }

	public List<IReadOnlyBattleCardInfo> ShortageDeckWinCards { get; set; }

	public DamageInfo DefaultDamage { get; set; }

	public DamageInfo FixedDamage { get; set; }

	public List<SkillBase> ProcessSkillList { get; set; }

	public AttachingAbilityInfo AttachingAbility { get; set; }

	public int AddChargeCount { get; set; }

	public int AddOddChargeCount { get; set; }

	public int AddEvenChargeCount { get; set; }

	public bool IsRefPrev { get; set; }

	public bool IsSkipPpCheck { get; set; }

	public bool IsSkipPrivateCardCheck { get; set; }

	public List<List<string>> SkillTargetConditionList { get; set; }

	public List<string> PopSkillTargetCondition()
	{
		List<string> result = new List<string> { string.Empty };
		if (SkillTargetConditionList.Count > _skillTargetConditionIndex)
		{
			result = SkillTargetConditionList[_skillTargetConditionIndex];
			_skillTargetConditionIndex++;
		}
		return result;
	}

	public SkillConditionCheckerOption()
	{
		ReturnCards = new List<IReadOnlyBattleCardInfo>();
		CantAttackAllReturnCards = new List<IReadOnlyBattleCardInfo>();
		InHandCard = new List<IReadOnlyBattleCardInfo>();
		SkillDrewCards = new List<IReadOnlyBattleCardInfo>();
		SkillUpdatedDeckCards = new List<IReadOnlyBattleCardInfo>();
		HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>();
		DamageCards = new List<IReadOnlyBattleCardInfo>();
		EvolutionCards = new List<IReadOnlyBattleCardInfo>();
		Discards = new List<IReadOnlyBattleCardInfo>();
		NextTargetCards = new List<IReadOnlyBattleCardInfo>();
		ReanimatedCards = new List<IReadOnlyBattleCardInfo>();
		DeckSelfSummonedCards = new List<IReadOnlyBattleCardInfo>();
		TokenDrewCards = new List<IReadOnlyBattleCardInfo>();
		DeckDrewCards = new List<IReadOnlyBattleCardInfo>();
		DrewOverHandLimitCards = new List<IReadOnlyBattleCardInfo>();
		ChantCountChangeCard = new List<IReadOnlyBattleCardInfo>();
		ShortageDeckWinCards = new List<IReadOnlyBattleCardInfo>();
		SelectedCards = new List<SkillAndSelectTarget>();
		BurialRiteCards = new List<BattleCardBase>();
		GetOffCards = new List<BattleCardBase>();
		ChosenCards = new List<int>();
		DefaultDamage = new DamageInfo(null, -1);
		FixedDamage = new DamageInfo(null, -1);
		ProcessSkillList = new List<SkillBase>();
		AttachingAbility = new AttachingAbilityInfo(null, new List<IReadOnlyBattleCardInfo>());
		SkillTargetConditionList = new List<List<string>>();
		_skillTargetConditionIndex = 0;
	}

	public SkillConditionCheckerOption ShallowCopy()
	{
		return new SkillConditionCheckerOption
		{
			PlayedCard = PlayedCard,
			EnhanceCard = EnhanceCard,
			AcceleratedCard = AcceleratedCard,
			CrystallizedCard = CrystallizedCard,
			SummonedCard = SummonedCard,
			AttackerCard = AttackerCard,
			AttackTargetCard = AttackTargetCard,
			DestroyedCard = DestroyedCard,
			NecromanceCard = NecromanceCard,
			BurialRiteCards = new List<BattleCardBase>(BurialRiteCards),
			GetOffCards = new List<BattleCardBase>(GetOffCards),
			ChosenCards = new List<int>(ChosenCards),
			InHandCard = new List<IReadOnlyBattleCardInfo>(InHandCard),
			SkillDrewCards = new List<IReadOnlyBattleCardInfo>(SkillDrewCards),
			SkillUpdatedDeckCards = new List<IReadOnlyBattleCardInfo>(SkillUpdatedDeckCards),
			HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>(HealingCardAndValue),
			DamageCards = new List<IReadOnlyBattleCardInfo>(DamageCards),
			EvolutionCards = new List<IReadOnlyBattleCardInfo>(EvolutionCards),
			Discards = new List<IReadOnlyBattleCardInfo>(Discards),
			SelectedCards = new List<SkillAndSelectTarget>(SelectedCards),
			ReturnCards = new List<IReadOnlyBattleCardInfo>(ReturnCards),
			CantAttackAllReturnCards = new List<IReadOnlyBattleCardInfo>(CantAttackAllReturnCards),
			NextTargetCards = new List<IReadOnlyBattleCardInfo>(NextTargetCards),
			ReanimatedCards = new List<IReadOnlyBattleCardInfo>(ReanimatedCards),
			DeckSelfSummonedCards = new List<IReadOnlyBattleCardInfo>(DeckSelfSummonedCards),
			TokenDrewCards = new List<IReadOnlyBattleCardInfo>(TokenDrewCards),
			DeckDrewCards = new List<IReadOnlyBattleCardInfo>(DeckDrewCards),
			DrewOverHandLimitCards = new List<IReadOnlyBattleCardInfo>(DrewOverHandLimitCards),
			ChantCountChangeCard = new List<IReadOnlyBattleCardInfo>(ChantCountChangeCard),
			ShortageDeckWinCards = new List<IReadOnlyBattleCardInfo>(ShortageDeckWinCards),
			DefaultDamage = DefaultDamage,
			FixedDamage = FixedDamage,
			ProcessSkillList = new List<SkillBase>(ProcessSkillList),
			AttachingAbility = new AttachingAbilityInfo(AttachingAbility.Skill, new List<IReadOnlyBattleCardInfo>(AttachingAbility.TargetCards)),
			SkillTargetConditionList = new List<List<string>>(SkillTargetConditionList),
			_skillTargetConditionIndex = _skillTargetConditionIndex,
			AddChargeCount = AddChargeCount,
			AddEvenChargeCount = AddEvenChargeCount,
			AddOddChargeCount = AddOddChargeCount,
			IsRefPrev = IsRefPrev
		};
	}

	public void SetCheckerOptionTransientValue(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		if (selfBattlePlayer.ReturnList.Count > 0)
		{
			ReturnCards.AddRange(BattlePlayerBase.ConvertToSkillInfoCollection(selfBattlePlayer.ReturnList));
			ReturnCards = ReturnCards.Distinct().ToList();
		}
		if (opponentBattlePlayer.ReturnList.Count > 0)
		{
			ReturnCards.AddRange(BattlePlayerBase.ConvertToSkillInfoCollection(opponentBattlePlayer.ReturnList));
			ReturnCards = ReturnCards.Distinct().ToList();
		}
	}
}
