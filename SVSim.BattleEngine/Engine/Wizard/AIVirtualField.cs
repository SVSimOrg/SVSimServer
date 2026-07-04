using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle;

namespace Wizard;

public class AIVirtualField
{
	public class AIVirtualFieldSearchCardOption
	{
		public bool IsSearchFromDeck;

		public bool IsOutputCannotFindError;

		public bool IsSearchFromBeforeLatestActionDeck;

		public BattleCardRealTargetInformation.TargetRange OptionalSearchRange;

		public static AIVirtualFieldSearchCardOption DefaultOption { get; } = new AIVirtualFieldSearchCardOption
		{
			IsSearchFromDeck = true,
			IsOutputCannotFindError = true,
			IsSearchFromBeforeLatestActionDeck = false,
			OptionalSearchRange = BattleCardRealTargetInformation.TargetRange.Default
		};
	}

	public List<int> BestPlayPtn = new List<int>();

	public AIParamQuery ParamQuery;

	public AIStyleQuery StyleQuery;

	public List<AIVirtualCard> AllyInplayCards;

	public List<AIVirtualCard> EnemyInplayCards;

	public List<AIVirtualCard> AllyHandCards;

	private List<AIVirtualCard> _latestActionEnemyHandCards;

	private List<AIVirtualCard> _enemyHandCards;

	public AIVirtualCard AllyClass;

	public AIVirtualCard EnemyClass;

	public int AllyTurnCount;

	public int EnemyTurnCount;

	public int AllyDeckCount;

	public int OpponentDeckCount;

	public int AllyEvolutionCount;

	public int EnemyEvolutionCount;

	public int AllyPp;

	public int AllyPpTotal;

	public int EnemyPp;

	public int EnemyPpTotal;

	public int UsedEpCount;

	public int UsedPpCount;

	public AIVirtualCard EvoUsedCard;

	public float EvoBonus;

	public int EvoHandPlus;

	public float EpValue;

	public bool IsLeftTurnEvol;

	public bool IsExceededWaitEvolveTurn;

	public float SimulationExtraBonus;

	public bool IsBreakBeforePlayKilled;

	private int NextTurnLeaderDamage;

	public int ActionLength;

	public Queue<Tuple<AIVirtualCard, AIVirtualCard>> EnemyTokenQueue = new Queue<Tuple<AIVirtualCard, AIVirtualCard>>();

	public int TokenIndex;

	public bool IsNoInstantAttack;

	public int AllyCardTotalNum;

	public int EnemyCardTotalNum;

	public AIVirtualTurnEndInfo CommonAllyTurnEndSituation;

	public AIVirtualTurnEndInfo CommonEnemyTurnEndSituation;

	private AIPlayedCardContainer _playedCardContainer;

	private bool _isPlayptnSimulationField;

	public int AllyEvolvedCountInGame;

	public int AllyEvolvedCountInPreviousTurn;

	public int EnemyEvolvedCountInGame;

	public int EnemyEvolvedCountInPreviousTurn;

	public int AllyDamageCountInGame;

	public int AllyDamageCountInTurn;

	public int EnemyDamageCountInGame;

	public int AllyNecromancedCountInGame;

	public int EnemyNecromancedCountInGame;

	private bool _isCreateTemporaryPlayPtnRecord;

	private static ulong[] PRIME_NUMBERS_FOR_BEST_PLAYPTN = new ulong[9] { 15361uL, 15373uL, 15377uL, 15383uL, 15391uL, 15401uL, 15413uL, 15427uL, 15439uL };

	public EnemyAI AI { get; set; }

	public CardListsForReference CardListSet { get; private set; }

	public BattlePlayerBase AllyBattlePlayer => AllyClass.SelfBattlePlayer;

	public BattlePlayerBase EnemyBattlePlayer => EnemyClass.SelfBattlePlayer;

	public AIDummyDeckContainer DummyDeckContainer { get; private set; }

	public AIVirtualCemetery VirtualCemetery { get; private set; }

	public int AllyRallyCount { get; private set; }

	public int EnemyRallyCount { get; private set; }

	public int JustBeforeTurnLeaderDamage { get; private set; }

	public AIHealRecorderCollection HealRecorderCollection { get; private set; }

	public int TurnDrawCount { get; private set; }

	public int GameDrawCount { get; private set; }

	public int VirtualDrawCount { get; private set; }

	public int TurnBounceCount { get; private set; }

	public int GameBounceCount { get; private set; }

	public int AllyGameResonanceStartCount { get; private set; }

	public int AllyTurnResonanceStartCount { get; private set; }

	public int EnemyGameResonanceStartCount { get; private set; }

	public int EnemyTurnResonanceStartCount { get; private set; }

	public int AllyGameUsedStackCount { get; private set; }

	public int EnemyGameUsedStackCount { get; private set; }

	public List<AIVirtualCard> AllyTurnHandAddedCards { get; private set; }

	public List<AIVirtualCard> AllyGameHandAddedCards { get; private set; }

	public List<AIVirtualCard> EnemyTurnHandAddedCards { get; private set; }

	public List<AIVirtualCard> EnemyGameHandAddedCards { get; private set; }

	public int CurrentTurnCount { get; private set; } = -1;

	public List<AIVirtualCard> AllyGameFusedCards { get; private set; }

	public List<AIVirtualCard> EnemyGameFusedCards { get; private set; }

	public List<AICannotPlayInformation> CannotPlayInformationList { get; private set; }

	public List<Tuple<AIVirtualCard, int>> DamagedCardsByLastAction { get; set; } = new List<Tuple<AIVirtualCard, int>>();

	public AITagPreprocessCollectionContainer TagPreprocessContainer { get; set; }

	public AIDamageModifierCollection DamageModifierCollection { get; set; }

	public bool IsLatestActionField { get; private set; }

	public List<AIVirtualCard> AllyGameEnhancePlayCards { get; private set; }

	public List<AIVirtualCard> EnemyGameEnhancePlayCards { get; private set; }

	public AIPlayedCardContainer PlayedCardContainer => _playedCardContainer;

	public bool IsRemovedPlayPtnCard { get; private set; }

	public AISinglePlayptnRecord BestPlayptnRecordOnSim { get; private set; }

	public List<AIVirtualCard> AllyGameAddUpdateDeckCards { get; private set; }

	public List<AIVirtualCard> EnemyGameAddUpdateDeckCards { get; private set; }

	public AISummonedCardContainer SummonedCardContainer { get; private set; }

	public event Action OnAfterLeaderAttackSimulation;

	public static AIVirtualField CreateTemporaryVirtualField(EnemyAI ai, AIParamQuery paramQuery, AIStyleQuery styleQuery, BattlePlayerPair pair, List<int> bestPlayPtn, AIVirtualFieldBuildParameterCollction buildParameters)
	{
		AIVirtualField aIVirtualField = new AIVirtualField(ai, paramQuery, styleQuery, pair, bestPlayPtn, buildParameters);
		ai.tokenManager.UpdateTokenPool(aIVirtualField);
		aIVirtualField.InitializeBothDefValue();
		return aIVirtualField;
	}

	public AIVirtualField(EnemyAI ai, AIParamQuery paramQuery, AIStyleQuery styleQuery, BattlePlayerPair pair, List<int> bestPlayPtn, AIVirtualFieldBuildParameterCollction buildParameters)
	{
		AI = ai;
		BestPlayPtn = new List<int>(bestPlayPtn);
		ParamQuery = paramQuery;
		StyleQuery = styleQuery;
		EvoUsedCard = null;
		EvoBonus = 0f;
		EvoHandPlus = 0;
		EpValue = 0f;
		SimulationExtraBonus = 0f;
		this.OnAfterLeaderAttackSimulation = null;
		_playedCardContainer = ((buildParameters.PlayedCardContainer == null) ? new AIPlayedCardContainer() : buildParameters.PlayedCardContainer.Clone());
		VirtualCemetery = new AIVirtualCemetery(pair.Self.CemeteryList.Count, pair.Opponent.CemeteryList.Count);
		AllyDeckCount = ai.ALLY.DeckCardList.Count;
		OpponentDeckCount = ai.OPPONENT.DeckCardList.Count;
		DummyDeckContainer = new AIDummyDeckContainer();
		AllyRallyCount = ai.ALLY.RallyCount;
		EnemyRallyCount = ai.OPPONENT.RallyCount;
		JustBeforeTurnLeaderDamage = pair.Self.Class.SkillApplyInformation.GetSpecificTurnDamageValue(pair.Self.Class, new TurnPlayerInfo(SkillFilterCreator.ContentKeyword.op.ToStringCustom(), 0));
		HealRecorderCollection = ((buildParameters.HealRecorderCollection == null) ? new AIHealRecorderCollection() : buildParameters.HealRecorderCollection.Clone());
		IsBreakBeforePlayKilled = false;
		AllyTurnCount = ai.ALLY.Turn;
		EnemyTurnCount = ai.OPPONENT.Turn;
		AllyEvolutionCount = ai.ALLY.CurrentEpCount;
		EnemyEvolutionCount = ai.OPPONENT.CurrentEpCount;
		AllyPp = ai.ALLY.Pp;
		AllyPpTotal = ai.ALLY.PpTotal;
		EnemyPp = ai.OPPONENT.Pp;
		EnemyPpTotal = ai.OPPONENT.PpTotal;
		UsedEpCount = pair.Self.GameUsedEpCount;
		UsedPpCount = pair.Self.GameUsedPpCount;
		IsNoInstantAttack = false;
		IsLatestActionField = false;
		if (buildParameters.CannotPlayInfoList != null)
		{
			CannotPlayInformationList = new List<AICannotPlayInformation>(buildParameters.CannotPlayInfoList);
		}
		ActionLength = 0;
		PairToVirtualCards(pair);
		InitializeCardTags(buildParameters);
		TurnDrawCount = pair.Self.TurnDrawCards.Count;
		GameDrawCount = pair.Self.GameDrawCards.Count;
		VirtualDrawCount = 0;
		List<BattlePlayerBase.TurnAndCard> gameReturnedCards = pair.Self.GameReturnedCards;
		List<BattlePlayerBase.TurnAndCard> gameReturnedCards2 = pair.Opponent.GameReturnedCards;
		GameBounceCount = gameReturnedCards.Count;
		int turn = pair.Self.Turn;
		int num = 0;
		for (int i = 0; i < GameBounceCount; i++)
		{
			if (gameReturnedCards[i].Turn == turn)
			{
				num++;
			}
		}
		for (int j = 0; j < gameReturnedCards2.Count; j++)
		{
			if (gameReturnedCards2[j].Turn == turn)
			{
				num++;
			}
		}
		TurnBounceCount = num;
		GameBounceCount += gameReturnedCards2.Count;
		AllyGameResonanceStartCount = pair.Self.GameResonanceStartCount;
		AllyTurnResonanceStartCount = pair.Self.TurnResonanceStartCount;
		EnemyGameResonanceStartCount = pair.Opponent.GameResonanceStartCount;
		EnemyTurnResonanceStartCount = pair.Opponent.TurnResonanceStartCount;
		AllyGameUsedStackCount = pair.Self.GameUsedWhiteRitualCount;
		EnemyGameUsedStackCount = pair.Opponent.GameUsedWhiteRitualCount;
		List<IReadOnlyBattleCardInfo> list = ParamQuery.GetBrokenAll(pair.Self.Class).ToList();
		int turn2 = pair.Self.BattleMgr.CurrentTurn;
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		list2.AddRange(pair.Self.SkillInfoNecromanceZoneCards);
		list2.AddRange(pair.Self.SkillInfoCemeterys);
		IEnumerable<IReadOnlyBattleCardInfo> source = list2.Where((IReadOnlyBattleCardInfo pp) => pp.IsDead && !(pp is NullBattleCard) && pp.DestroyedTurn == turn2 && pp.IsDestroySelfTurn == pair.Self.IsPlayer);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			IReadOnlyBattleCardInfo card = list[num2];
			bool isDestroyTurn = source.Any((IReadOnlyBattleCardInfo c) => card == c);
			AIVirtualCard aIVirtualCard = new AIVirtualCard((BattleCardBase)card, this);
			NonReferableVirtualCardBuildParameterCollection destroyedCardBuildParameter = buildParameters.GetDestroyedCardBuildParameter(aIVirtualCard);
			AIAttachedTagCollection attachedTagCollection = destroyedCardBuildParameter?.AttachedTags;
			AIRemovedTagCollection removedTagCollection = destroyedCardBuildParameter?.RemovedTags;
			aIVirtualCard.InitializeTags(ParamQuery, attachedTagCollection, removedTagCollection);
			CardListSet.AddAllyDestroyedCard(aIVirtualCard, isDestroyTurn);
		}
		List<IReadOnlyBattleCardInfo> list3 = ParamQuery.GetBrokenAll(pair.Opponent.Class).ToList();
		for (int num3 = 0; num3 < list3.Count; num3++)
		{
			AIVirtualCard aIVirtualCard2 = new AIVirtualCard((BattleCardBase)list3[num3], this);
			NonReferableVirtualCardBuildParameterCollection destroyedCardBuildParameter2 = buildParameters.GetDestroyedCardBuildParameter(aIVirtualCard2);
			AIAttachedTagCollection attachedTagCollection2 = destroyedCardBuildParameter2?.AttachedTags;
			AIRemovedTagCollection removedTagCollection2 = destroyedCardBuildParameter2?.RemovedTags;
			aIVirtualCard2.InitializeTags(ParamQuery, attachedTagCollection2, removedTagCollection2);
			CardListSet.AddEnemyDestroyedCard(aIVirtualCard2);
		}
		if (pair.Self.GameLeftCards != null)
		{
			List<BattleCardBase> gameLeftCards = pair.Self.GameLeftCards;
			for (int num4 = 0; num4 < gameLeftCards.Count; num4++)
			{
				AIVirtualCard card2 = new AIVirtualCard(gameLeftCards[num4], this);
				CardListSet.AddAllyLeftCard(card2);
			}
		}
		if (pair.Opponent.GameLeftCards != null)
		{
			List<BattleCardBase> gameLeftCards2 = pair.Opponent.GameLeftCards;
			for (int num5 = 0; num5 < gameLeftCards2.Count; num5++)
			{
				AIVirtualCard card3 = new AIVirtualCard(gameLeftCards2[num5], this);
				CardListSet.AddEnemyLeftCard(card3);
			}
		}
		if (pair.Self.SkillInfoGameTurnLeftCards != null)
		{
			foreach (BattlePlayerBase.TurnAndCard skillInfoGameTurnLeftCard in pair.Self.SkillInfoGameTurnLeftCards)
			{
				if (turn2 == skillInfoGameTurnLeftCard.Turn)
				{
					AIVirtualCard card4 = new AIVirtualCard((BattleCardBase)skillInfoGameTurnLeftCard.Card, this);
					CardListSet.AddAllyLeftCardThisTurn(card4);
				}
			}
		}
		if (pair.Opponent.SkillInfoGameTurnLeftCards != null)
		{
			foreach (BattlePlayerBase.TurnAndCard skillInfoGameTurnLeftCard2 in pair.Opponent.SkillInfoGameTurnLeftCards)
			{
				if (turn2 == skillInfoGameTurnLeftCard2.Turn)
				{
					AIVirtualCard card5 = new AIVirtualCard((BattleCardBase)skillInfoGameTurnLeftCard2.Card, this);
					CardListSet.AddEnemyLeftCardThisTurn(card5);
				}
			}
		}
		if (pair.Self.GameBurialRiteCards != null && pair.Self.GameBurialRiteCards.Count > 0)
		{
			List<BattleCardBase> gameBurialRiteCards = pair.Self.GameBurialRiteCards;
			for (int num6 = 0; num6 < gameBurialRiteCards.Count; num6++)
			{
				AIVirtualCard card6 = new AIVirtualCard(gameBurialRiteCards[num6], this);
				CardListSet.AddAllyBurialCard(card6);
			}
		}
		if (pair.Opponent.GameBurialRiteCards != null && pair.Opponent.GameBurialRiteCards.Count > 0)
		{
			List<BattleCardBase> gameBurialRiteCards2 = pair.Opponent.GameBurialRiteCards;
			for (int num7 = 0; num7 < gameBurialRiteCards2.Count; num7++)
			{
				AIVirtualCard card7 = new AIVirtualCard(gameBurialRiteCards2[num7], this);
				CardListSet.AddEnemyBurialCard(card7);
			}
		}
		if (pair.Self.SkillInfoFusionIngredientList != null)
		{
			foreach (BattleCardBase skillInfoFusionIngredient in pair.Self.SkillInfoFusionIngredientList)
			{
				AIVirtualCard element = new AIVirtualCard(skillInfoFusionIngredient, this);
				AllyGameFusedCards = AIParamQuery.AddElementToList(element, AllyGameFusedCards);
			}
		}
		if (pair.Opponent.SkillInfoFusionIngredientList != null)
		{
			foreach (BattleCardBase skillInfoFusionIngredient2 in pair.Opponent.SkillInfoFusionIngredientList)
			{
				AIVirtualCard element2 = new AIVirtualCard(skillInfoFusionIngredient2, this);
				EnemyGameFusedCards = AIParamQuery.AddElementToList(element2, EnemyGameFusedCards);
			}
		}
		List<BattleCardBase> banishList = pair.Self.BanishList;
		if (banishList != null && banishList.Any())
		{
			for (int num8 = 0; num8 < banishList.Count; num8++)
			{
				BattleCardBase battleCardBase = banishList[num8];
				if (!battleCardBase.IsClass)
				{
					AIVirtualCard card8 = new AIVirtualCard(battleCardBase, this);
					CardListSet.AddBanishedCard(card8);
				}
			}
		}
		List<BattleCardBase> banishList2 = pair.Opponent.BanishList;
		if (banishList2 != null && banishList2.Any())
		{
			for (int num9 = 0; num9 < banishList2.Count; num9++)
			{
				BattleCardBase battleCardBase2 = banishList2[num9];
				if (!battleCardBase2.IsClass)
				{
					AIVirtualCard card9 = new AIVirtualCard(battleCardBase2, this);
					CardListSet.AddBanishedCard(card9);
				}
			}
		}
		AllyTurnHandAddedCards = new List<AIVirtualCard>();
		AllyGameHandAddedCards = new List<AIVirtualCard>();
		EnemyTurnHandAddedCards = new List<AIVirtualCard>();
		EnemyGameHandAddedCards = new List<AIVirtualCard>();
		if (AllyClass != null)
		{
			BattlePlayerBase allyBattlePlayer = AllyBattlePlayer;
			if (allyBattlePlayer.TurnDrawCards != null)
			{
				for (int num10 = 0; num10 < allyBattlePlayer.TurnDrawCards.Count; num10++)
				{
					BattleCardBase card10 = allyBattlePlayer.TurnDrawCards[num10];
					AllyTurnHandAddedCards.Add(new AIVirtualCard(card10, this));
				}
			}
			if (allyBattlePlayer.GameDrawCards != null)
			{
				for (int num11 = 0; num11 < allyBattlePlayer.GameDrawCards.Count; num11++)
				{
					BattleCardBase card11 = allyBattlePlayer.GameDrawCards[num11];
					AllyGameHandAddedCards.Add(new AIVirtualCard(card11, this));
				}
			}
			if (allyBattlePlayer.GameDrawTokenCards != null)
			{
				for (int num12 = 0; num12 < allyBattlePlayer.GameDrawTokenCards.Count; num12++)
				{
					BattleCardBase card12 = allyBattlePlayer.GameDrawTokenCards[num12];
					AllyGameHandAddedCards.Add(new AIVirtualCard(card12, this));
				}
			}
		}
		if (EnemyClass != null)
		{
			BattlePlayerBase enemyBattlePlayer = EnemyBattlePlayer;
			if (enemyBattlePlayer.TurnDrawCards != null)
			{
				for (int num13 = 0; num13 < enemyBattlePlayer.TurnDrawCards.Count; num13++)
				{
					BattleCardBase card13 = enemyBattlePlayer.TurnDrawCards[num13];
					EnemyTurnHandAddedCards.Add(new AIVirtualCard(card13, this));
				}
			}
			if (enemyBattlePlayer.GameDrawCards != null)
			{
				for (int num14 = 0; num14 < enemyBattlePlayer.GameDrawCards.Count; num14++)
				{
					BattleCardBase card14 = enemyBattlePlayer.GameDrawCards[num14];
					EnemyGameHandAddedCards.Add(new AIVirtualCard(card14, this));
				}
			}
			if (enemyBattlePlayer.GameDrawTokenCards != null)
			{
				for (int num15 = 0; num15 < enemyBattlePlayer.GameDrawTokenCards.Count; num15++)
				{
					BattleCardBase card15 = enemyBattlePlayer.GameDrawTokenCards[num15];
					EnemyGameHandAddedCards.Add(new AIVirtualCard(card15, this));
				}
			}
		}
		AllyGameEnhancePlayCards = new List<AIVirtualCard>();
		EnemyGameEnhancePlayCards = new List<AIVirtualCard>();
		if (pair.Self.GameEnhancePlayCards != null && pair.Self.GameEnhancePlayCards.Count > 0)
		{
			List<BattlePlayerBase.TurnAndCard> gameEnhancePlayCards = pair.Self.GameEnhancePlayCards;
			for (int num16 = 0; num16 < gameEnhancePlayCards.Count; num16++)
			{
				AllyGameEnhancePlayCards.Add(new AIVirtualCard((BattleCardBase)gameEnhancePlayCards[num16].Card, this));
			}
		}
		if (pair.Opponent.GameEnhancePlayCards != null && pair.Opponent.GameEnhancePlayCards.Count > 0)
		{
			List<BattlePlayerBase.TurnAndCard> gameEnhancePlayCards2 = pair.Opponent.GameEnhancePlayCards;
			for (int num17 = 0; num17 < gameEnhancePlayCards2.Count; num17++)
			{
				EnemyGameEnhancePlayCards.Add(new AIVirtualCard((BattleCardBase)gameEnhancePlayCards2[num17].Card, this));
			}
		}
		int a = CardListSet.BothClassAndInplayCards.Min((AIVirtualCard c) => c.CardIndex);
		TokenIndex = Mathf.Max(a, 0) - 1;
		CurrentTurnCount = (pair.Self.IsSelfTurn ? AI.TurnCount : AI.OpponentTurnCount);
		AllyCardTotalNum = AllyBattlePlayer.cardTotalNum;
		EnemyCardTotalNum = EnemyBattlePlayer.cardTotalNum;
		IsLeftTurnEvol = AllyBattlePlayer.NowTurnEvol;
		IsExceededWaitEvolveTurn = AllyBattlePlayer.EvolveWaitTurnCount <= 0;
		TagPreprocessContainer = ((buildParameters.TagPreprocessContainer == null) ? new AITagPreprocessCollectionContainer() : buildParameters.TagPreprocessContainer.Clone(this));
		DamageModifierCollection = ((buildParameters.DamageModifierCollection == null) ? new AIDamageModifierCollection() : buildParameters.DamageModifierCollection.Clone(this));
		CommonAllyTurnEndSituation = new AIVirtualTurnEndInfo(AllyClass);
		CommonEnemyTurnEndSituation = new AIVirtualTurnEndInfo(EnemyClass);
		AllyEvolvedCountInGame = AllyBattlePlayer.EvolvedCards.Count;
		EnemyEvolvedCountInGame = EnemyBattlePlayer.EvolvedCards.Count;
		TurnPlayerInfo turnPlayerInfo = new TurnPlayerInfo(SkillFilterCreator.ContentKeyword.me.ToStringCustom(), 1);
		AllyEvolvedCountInPreviousTurn = AllyBattlePlayer.GetSpecificTurnEvolveCount(turnPlayerInfo);
		EnemyEvolvedCountInPreviousTurn = EnemyBattlePlayer.GetSpecificTurnEvolveCount(turnPlayerInfo);
		AllyDamageCountInGame = AllyBattlePlayer.Class.DamagedCounter.GetDamageCount(selfTurn: true);
		AllyDamageCountInTurn = 0;
		EnemyDamageCountInGame = EnemyBattlePlayer.Class.DamagedCounter.GetDamageCount(selfTurn: true);
		AllyNecromancedCountInGame = AllyBattlePlayer.GameNecromanceCount;
		EnemyNecromancedCountInGame = EnemyBattlePlayer.GameNecromanceCount;
		AllyGameAddUpdateDeckCards = new List<AIVirtualCard>();
		EnemyGameAddUpdateDeckCards = new List<AIVirtualCard>();
		List<BattleCardBase> gameAddUpdateDeckCards = pair.Self.GameAddUpdateDeckCards;
		if (gameAddUpdateDeckCards != null && gameAddUpdateDeckCards.Count > 0)
		{
			for (int num18 = 0; num18 < gameAddUpdateDeckCards.Count; num18++)
			{
				AllyGameAddUpdateDeckCards.Add(new AIVirtualCard(gameAddUpdateDeckCards[num18].Card, this));
			}
		}
		List<BattleCardBase> gameAddUpdateDeckCards2 = pair.Opponent.GameAddUpdateDeckCards;
		if (gameAddUpdateDeckCards2 != null && gameAddUpdateDeckCards2.Count > 0)
		{
			for (int num19 = 0; num19 < gameAddUpdateDeckCards2.Count; num19++)
			{
				EnemyGameAddUpdateDeckCards.Add(new AIVirtualCard(gameAddUpdateDeckCards2[num19].Card, this));
			}
		}
		SummonedCardContainer = new AISummonedCardContainer();
		SummonedCardContainer.LoadSummonedCardList(this, pair.Self, pair.Opponent);
	}

	private void PairToVirtualCards(BattlePlayerPair pair)
	{
		CardListSet = new CardListsForReference();
		AllyClass = new AIVirtualCard(pair.Self.Class, this);
		CardListSet.AddAllyClass(AllyClass);
		EnemyClass = new AIVirtualCard(pair.Opponent.Class, this);
		CardListSet.AddEnemyClass(EnemyClass);
		List<BattleCardBase> list = pair.Opponent.InPlayCards.ToList();
		int count = list.Count;
		List<BattleCardBase> list2 = pair.Self.InPlayCards.ToList();
		int count2 = list2.Count;
		AllyHandCards = new List<AIVirtualCard>();
		for (int i = 0; i < pair.Self.HandCardList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = new AIVirtualCard(pair.Self.HandCardList[i], this);
			AllyHandCards.Add(aIVirtualCard);
			CardListSet.AddHandCard(aIVirtualCard);
		}
		_enemyHandCards = new List<AIVirtualCard>();
		for (int j = 0; j < pair.Opponent.HandCardList.Count; j++)
		{
			EnemyHandVirtualCard enemyHandVirtualCard = new EnemyHandVirtualCard(pair.Opponent.HandCardList[j], this);
			_enemyHandCards.Add(enemyHandVirtualCard);
			CardListSet.AddHandCard(enemyHandVirtualCard);
		}
		AllyInplayCards = new List<AIVirtualCard>();
		for (int k = 0; k < count2; k++)
		{
			AIVirtualCard aIVirtualCard2 = new AIVirtualCard(list2[k], this);
			AllyInplayCards.Add(aIVirtualCard2);
			CardListSet.AddAllyInplayCard(aIVirtualCard2);
		}
		EnemyInplayCards = new List<AIVirtualCard>();
		for (int l = 0; l < count; l++)
		{
			AIVirtualCard aIVirtualCard3 = new AIVirtualCard(list[l], this);
			EnemyInplayCards.Add(aIVirtualCard3);
			CardListSet.AddEnemyInplayCard(aIVirtualCard3);
		}
	}

	private void InitializeCardTags(AIVirtualFieldBuildParameterCollction buildParameter)
	{
		for (int i = 0; i < CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.AllReferableCards[i];
			aIVirtualCard.InitializeTags(ParamQuery, null, null);
			CardListSet.TagClassification(aIVirtualCard);
			aIVirtualCard.FindBuildParameterAndApply(buildParameter);
		}
		if (IsLatestActionField)
		{
			for (int j = 0; j < _latestActionEnemyHandCards.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = _latestActionEnemyHandCards[j];
				NonReferableVirtualCardBuildParameterCollection enemyHandCardBuildParameter = buildParameter.GetEnemyHandCardBuildParameter(aIVirtualCard2);
				AIAttachedTagCollection attachedTagCollection = enemyHandCardBuildParameter?.AttachedTags;
				AIRemovedTagCollection removedTagCollection = enemyHandCardBuildParameter?.RemovedTags;
				aIVirtualCard2.InitializeTags(ParamQuery, attachedTagCollection, removedTagCollection);
				CardListSet.TagClassification(aIVirtualCard2);
			}
		}
		else
		{
			for (int k = 0; k < _enemyHandCards.Count; k++)
			{
				AIVirtualCard aIVirtualCard3 = _enemyHandCards[k];
				NonReferableVirtualCardBuildParameterCollection enemyHandCardBuildParameter2 = buildParameter.GetEnemyHandCardBuildParameter(aIVirtualCard3);
				AIAttachedTagCollection attachedTagCollection2 = enemyHandCardBuildParameter2?.AttachedTags;
				AIRemovedTagCollection removedTagCollection2 = enemyHandCardBuildParameter2?.RemovedTags;
				aIVirtualCard3.InitializeTags(ParamQuery, attachedTagCollection2, removedTagCollection2);
				CardListSet.TagClassification(aIVirtualCard3);
			}
		}
	}

	public void InitializeBothDefValue()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < CardListSet.BothInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.BothInplayCards[i];
			aIVirtualCard.UpdateValue(ParamQuery, StyleQuery, EnemyAI.EmptyPlayPtn, doesUseLostLife: true);
			aIVirtualCard.SetDefaultValue();
			if (!aIVirtualCard.IsAlly && !aIVirtualCard.IsDead && aIVirtualCard.IsNoInstantAttack(this, BestPlayPtn))
			{
				IsNoInstantAttack = true;
			}
			if (aIVirtualCard.TagCollectionContainer.ReferringOtherInplayIds == null)
			{
				continue;
			}
			for (int j = 0; j < aIVirtualCard.TagCollectionContainer.ReferringOtherInplayIds.Count; j++)
			{
				int key = aIVirtualCard.TagCollectionContainer.ReferringOtherInplayIds[j];
				if (dictionary.ContainsKey(key))
				{
					dictionary[key]++;
				}
				else
				{
					dictionary.Add(key, 1);
				}
			}
		}
		for (int k = 0; k < CardListSet.BothInplayCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard2 = CardListSet.BothInplayCards[k];
			if (dictionary.ContainsKey(aIVirtualCard2.BaseId))
			{
				aIVirtualCard2.ReferringSelfCount = dictionary[aIVirtualCard2.BaseId];
			}
		}
	}

	public int GetReferenceId(int originalId)
	{
		int key = originalId - originalId % 10;
		if (AI.ReferenceIdTable == null || !AI.ReferenceIdTable.ContainsKey(key))
		{
			return -1;
		}
		return AI.ReferenceIdTable[key];
	}

	public List<string> GetReferenceTribe(int baseCardId)
	{
		List<string> list = new List<string>();
		if (AI.ReferenceTribeTable == null || !AI.ReferenceTribeTable.Any())
		{
			return list;
		}
		for (int i = 0; i < AI.ReferenceTribeTable.Count; i++)
		{
			KeyValuePair<string, List<int>> keyValuePair = AI.ReferenceTribeTable.ElementAt(i);
			if (keyValuePair.Value.Contains(baseCardId))
			{
				list.Add(keyValuePair.Key);
			}
		}
		return list;
	}

	public void WhenCardLeaveFromField(AIVirtualCard leaveCard, AISituationInfo situation)
	{
		TagPreprocessContainer.SimulateWhenLeaveInfo(leaveCard, situation);
		bool flag = !leaveCard.IsAlly && IsNoInstantAttack;
		bool flag2 = leaveCard.TagCollectionContainer.ReferringOtherInplayIds != null;
		IsNoInstantAttack = false;
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.BothClassAndInplayCards[i];
			if (!IsNoInstantAttack && !aIVirtualCard.IsAlly && !aIVirtualCard.IsDead && flag && aIVirtualCard.IsNoInstantAttack(this, BestPlayPtn))
			{
				IsNoInstantAttack = true;
			}
			if (flag2 && leaveCard.TagCollectionContainer.ReferringOtherInplayIds.Contains(aIVirtualCard.BaseId) && aIVirtualCard.ReferringSelfCount > 0)
			{
				aIVirtualCard.ReferringSelfCount--;
			}
		}
	}

	public void IsNoInstantAttackRecheck()
	{
		IsNoInstantAttack = false;
		for (int i = 0; i < EnemyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = EnemyInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsNoInstantAttack(this, BestPlayPtn))
			{
				IsNoInstantAttack = true;
				break;
			}
		}
	}

	public AIVirtualField(AIVirtualField originalField, bool isLatestAction = false, bool isPlayptnSimulation = false)
	{
		IsLatestActionField = isLatestAction;
		AI = originalField.AI;
		BestPlayPtn = new List<int>(originalField.BestPlayPtn);
		ParamQuery = originalField.ParamQuery;
		StyleQuery = originalField.StyleQuery;
		_playedCardContainer = originalField.PlayedCardContainer.Clone();
		EvoBonus = originalField.EvoBonus;
		EvoHandPlus = originalField.EvoHandPlus;
		EpValue = originalField.EpValue;
		SimulationExtraBonus = originalField.SimulationExtraBonus;
		VirtualCemetery = new AIVirtualCemetery(originalField.VirtualCemetery.GetCemeteryCount(isAlly: true), originalField.VirtualCemetery.GetCemeteryCount(isAlly: false));
		AllyDeckCount = originalField.AllyDeckCount;
		OpponentDeckCount = originalField.OpponentDeckCount;
		AllyRallyCount = originalField.AllyRallyCount;
		EnemyRallyCount = originalField.EnemyRallyCount;
		JustBeforeTurnLeaderDamage = originalField.JustBeforeTurnLeaderDamage;
		HealRecorderCollection = originalField.HealRecorderCollection.Clone();
		if (originalField.CannotPlayInformationList != null)
		{
			CannotPlayInformationList = new List<AICannotPlayInformation>(originalField.CannotPlayInformationList);
		}
		AllyTurnCount = originalField.AllyTurnCount;
		EnemyTurnCount = originalField.EnemyTurnCount;
		AllyEvolutionCount = originalField.AllyEvolutionCount;
		EnemyEvolutionCount = originalField.EnemyEvolutionCount;
		AllyPp = originalField.AllyPp;
		AllyPpTotal = originalField.AllyPpTotal;
		EnemyPp = originalField.EnemyPp;
		EnemyPpTotal = originalField.EnemyPpTotal;
		UsedEpCount = originalField.UsedEpCount;
		UsedPpCount = originalField.UsedPpCount;
		NextTurnLeaderDamage = originalField.NextTurnLeaderDamage;
		IsBreakBeforePlayKilled = originalField.IsBreakBeforePlayKilled;
		IsNoInstantAttack = originalField.IsNoInstantAttack;
		TokenIndex = originalField.TokenIndex;
		this.OnAfterLeaderAttackSimulation = null;
		TurnDrawCount = originalField.TurnDrawCount;
		GameDrawCount = originalField.GameDrawCount;
		VirtualDrawCount = originalField.VirtualDrawCount;
		TurnBounceCount = originalField.TurnBounceCount;
		GameBounceCount = originalField.GameBounceCount;
		AllyGameResonanceStartCount = originalField.AllyGameResonanceStartCount;
		AllyTurnResonanceStartCount = originalField.AllyTurnResonanceStartCount;
		EnemyGameResonanceStartCount = originalField.EnemyGameResonanceStartCount;
		EnemyTurnResonanceStartCount = originalField.EnemyTurnResonanceStartCount;
		AllyGameUsedStackCount = originalField.AllyGameUsedStackCount;
		EnemyGameUsedStackCount = originalField.EnemyGameUsedStackCount;
		CardListSet = new CardListsForReference();
		AllyClass = new AIVirtualCard(originalField.AllyClass, this);
		EnemyClass = new AIVirtualCard(originalField.EnemyClass, this);
		CardListSet.AddAllyClass(AllyClass);
		CardListSet.AddEnemyClass(EnemyClass);
		AllyHandCards = new List<AIVirtualCard>();
		for (int i = 0; i < originalField.AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = new AIVirtualCard(originalField.AllyHandCards[i], this);
			AllyHandCards.Add(aIVirtualCard);
			CardListSet.AddHandCard(aIVirtualCard);
		}
		List<AIVirtualCard> enemyHandCardList = originalField.GetEnemyHandCardList();
		if (IsLatestActionField)
		{
			_latestActionEnemyHandCards = new List<AIVirtualCard>();
			if (originalField.IsLatestActionField)
			{
				for (int j = 0; j < enemyHandCardList.Count; j++)
				{
					AIVirtualCard aIVirtualCard2 = new AIVirtualCard(enemyHandCardList[j], this);
					_latestActionEnemyHandCards.Add(aIVirtualCard2);
					CardListSet.AddHandCard(aIVirtualCard2);
				}
			}
			else
			{
				for (int k = 0; k < enemyHandCardList.Count; k++)
				{
					AIVirtualCard aIVirtualCard3 = new AIVirtualCard(enemyHandCardList[k].BaseCard, this);
					aIVirtualCard3.InitializeEnemyHandParameter();
					_latestActionEnemyHandCards.Add(aIVirtualCard3);
					CardListSet.AddHandCard(aIVirtualCard3);
				}
			}
		}
		else
		{
			_enemyHandCards = new List<AIVirtualCard>();
			for (int l = 0; l < enemyHandCardList.Count; l++)
			{
				EnemyHandVirtualCard enemyHandVirtualCard = new EnemyHandVirtualCard(enemyHandCardList[l].BaseCard, this);
				_enemyHandCards.Add(enemyHandVirtualCard);
				CardListSet.AddHandCard(enemyHandVirtualCard);
			}
		}
		AllyInplayCards = new List<AIVirtualCard>();
		for (int m = 0; m < originalField.AllyInplayCards.Count; m++)
		{
			AIVirtualCard aIVirtualCard4 = new AIVirtualCard(originalField.AllyInplayCards[m], this);
			AllyInplayCards.Add(aIVirtualCard4);
			CardListSet.AddAllyInplayCard(aIVirtualCard4);
		}
		EnemyInplayCards = new List<AIVirtualCard>();
		for (int n = 0; n < originalField.EnemyInplayCards.Count; n++)
		{
			AIVirtualCard aIVirtualCard5 = new AIVirtualCard(originalField.EnemyInplayCards[n], this);
			EnemyInplayCards.Add(aIVirtualCard5);
			CardListSet.AddEnemyInplayCard(aIVirtualCard5);
		}
		if (IsLatestActionField && !originalField.IsLatestActionField)
		{
			for (int num = 0; num < _latestActionEnemyHandCards.Count; num++)
			{
				AIVirtualCard aIVirtualCard6 = enemyHandCardList[num];
				AIAttachedTagCollection attachedTags = aIVirtualCard6.TagCollectionContainer.AttachedTags;
				AIRemovedTagCollection removedTagCollection = aIVirtualCard6.TagCollectionContainer.RemovedTagCollection;
				_latestActionEnemyHandCards[num].InitializeTags(ParamQuery, attachedTags, removedTagCollection);
				AI.tokenManager.AddTokenFromCard(_latestActionEnemyHandCards[num]);
			}
		}
		for (int num2 = 0; num2 < originalField.CardListSet.AllyDestroyedCards.Count; num2++)
		{
			AIVirtualCard card = new AIVirtualCard(originalField.CardListSet.AllyDestroyedCards[num2], this);
			CardListSet.AddAllyDestroyedCard(card, originalField.CardListSet.AllyDestroyedCards[num2].DeadTurn);
		}
		for (int num3 = 0; num3 < originalField.CardListSet.EnemyDestroyedCards.Count; num3++)
		{
			AIVirtualCard card2 = new AIVirtualCard(originalField.CardListSet.EnemyDestroyedCards[num3], this);
			CardListSet.AddEnemyDestroyedCard(card2);
		}
		for (int num4 = 0; num4 < originalField.CardListSet.AllyLeftCards.Count; num4++)
		{
			AIVirtualCard card3 = new AIVirtualCard(originalField.CardListSet.AllyLeftCards[num4], this);
			CardListSet.AddAllyLeftCard(card3);
		}
		for (int num5 = 0; num5 < originalField.CardListSet.EnemyLeftCards.Count; num5++)
		{
			AIVirtualCard card4 = new AIVirtualCard(originalField.CardListSet.EnemyLeftCards[num5], this);
			CardListSet.AddEnemyLeftCard(card4);
		}
		for (int num6 = 0; num6 < originalField.CardListSet.AllyLeftCardsThisTurn.Count; num6++)
		{
			AIVirtualCard card5 = new AIVirtualCard(originalField.CardListSet.AllyLeftCardsThisTurn[num6], this);
			CardListSet.AddAllyLeftCardThisTurn(card5);
		}
		for (int num7 = 0; num7 < originalField.CardListSet.EnemyLeftCardsThisTurn.Count; num7++)
		{
			AIVirtualCard card6 = new AIVirtualCard(originalField.CardListSet.EnemyLeftCardsThisTurn[num7], this);
			CardListSet.AddEnemyLeftCardThisTurn(card6);
		}
		for (int num8 = 0; num8 < originalField.CardListSet.AllyBurialCards.Count; num8++)
		{
			AIVirtualCard card7 = new AIVirtualCard(originalField.CardListSet.AllyBurialCards[num8], this);
			CardListSet.AddAllyBurialCard(card7);
		}
		for (int num9 = 0; num9 < originalField.CardListSet.EnemyBurialCards.Count; num9++)
		{
			AIVirtualCard card8 = new AIVirtualCard(originalField.CardListSet.EnemyBurialCards[num9], this);
			CardListSet.AddEnemyBurialCard(card8);
		}
		if (originalField.AllyGameFusedCards != null)
		{
			for (int num10 = 0; num10 < originalField.AllyGameFusedCards.Count; num10++)
			{
				AIVirtualCard element = new AIVirtualCard(originalField.AllyGameFusedCards[num10], this);
				AllyGameFusedCards = AIParamQuery.AddElementToList(element, AllyGameFusedCards);
			}
		}
		if (originalField.EnemyGameFusedCards != null)
		{
			for (int num11 = 0; num11 < originalField.EnemyGameFusedCards.Count; num11++)
			{
				AIVirtualCard element2 = new AIVirtualCard(originalField.EnemyGameFusedCards[num11], this);
				EnemyGameFusedCards = AIParamQuery.AddElementToList(element2, EnemyGameFusedCards);
			}
		}
		List<AIVirtualCard> banishedCards = originalField.CardListSet.BanishedCards;
		if (banishedCards != null && banishedCards.Any())
		{
			for (int num12 = 0; num12 < banishedCards.Count; num12++)
			{
				CardListSet.AddBanishedCard(banishedCards[num12]);
			}
		}
		ActionLength = originalField.ActionLength;
		EvoUsedCard = null;
		if (originalField.EvoUsedCard != null)
		{
			for (int num13 = 0; num13 < AllyInplayCards.Count; num13++)
			{
				AIVirtualCard aIVirtualCard7 = AllyInplayCards[num13];
				if (aIVirtualCard7.CardIndex == originalField.EvoUsedCard.CardIndex)
				{
					EvoUsedCard = aIVirtualCard7;
				}
			}
		}
		DummyDeckContainer = originalField.DummyDeckContainer.Clone(this);
		AllyTurnHandAddedCards = originalField.AllyTurnHandAddedCards;
		AllyGameHandAddedCards = originalField.AllyGameHandAddedCards;
		EnemyTurnHandAddedCards = originalField.EnemyTurnHandAddedCards;
		EnemyGameHandAddedCards = originalField.EnemyGameHandAddedCards;
		CurrentTurnCount = originalField.CurrentTurnCount;
		AllyCardTotalNum = originalField.AllyCardTotalNum;
		EnemyCardTotalNum = originalField.EnemyCardTotalNum;
		IsLeftTurnEvol = originalField.IsLeftTurnEvol;
		IsExceededWaitEvolveTurn = originalField.IsExceededWaitEvolveTurn;
		for (int num14 = 0; num14 < originalField.DamagedCardsByLastAction.Count; num14++)
		{
			Tuple<AIVirtualCard, int> item = originalField.DamagedCardsByLastAction[num14];
			DamagedCardsByLastAction.Add(item);
		}
		TagPreprocessContainer = originalField.TagPreprocessContainer.Clone(this);
		DamageModifierCollection = originalField.DamageModifierCollection.Clone(this);
		CommonAllyTurnEndSituation = new AIVirtualTurnEndInfo(AllyClass);
		CommonEnemyTurnEndSituation = new AIVirtualTurnEndInfo(EnemyClass);
		AllyGameEnhancePlayCards = originalField.AllyGameEnhancePlayCards;
		EnemyGameEnhancePlayCards = originalField.EnemyGameEnhancePlayCards;
		AllyEvolvedCountInGame = originalField.AllyEvolvedCountInGame;
		EnemyEvolvedCountInGame = originalField.EnemyEvolvedCountInGame;
		AllyEvolvedCountInPreviousTurn = originalField.AllyEvolvedCountInPreviousTurn;
		EnemyEvolvedCountInPreviousTurn = originalField.EnemyEvolvedCountInPreviousTurn;
		AllyDamageCountInGame = originalField.AllyDamageCountInGame;
		AllyDamageCountInTurn = originalField.AllyDamageCountInTurn;
		EnemyDamageCountInGame = originalField.EnemyDamageCountInGame;
		AllyNecromancedCountInGame = originalField.AllyNecromancedCountInGame;
		EnemyNecromancedCountInGame = originalField.EnemyNecromancedCountInGame;
		AllyGameAddUpdateDeckCards = new List<AIVirtualCard>(originalField.AllyGameAddUpdateDeckCards);
		EnemyGameAddUpdateDeckCards = new List<AIVirtualCard>(originalField.EnemyGameAddUpdateDeckCards);
		SummonedCardContainer = originalField.SummonedCardContainer.Clone();
		_isPlayptnSimulationField = isPlayptnSimulation;
	}

	public float EvaluateField()
	{
		ulong hash = GetHash();
		if (AI.FieldHashAndValueTable.ContainsKey(hash))
		{
			return AI.FieldHashAndValueTable[hash];
		}
		if (AllyClass == null || EnemyClass == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < CardListSet.AllyClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.AllyClassAndInplayCards[i];
			num += aIVirtualCard.UpdateValue(ParamQuery, StyleQuery, BestPlayPtn, doesUseLostLife: true);
			if (aIVirtualCard.IsUnit && aIVirtualCard.IsRecoveredAttackableCount && AI.PlaySkipInfo != null)
			{
				bool isRush = aIVirtualCard.IsRush && aIVirtualCard.IsFirstTurn;
				num += AIInstantAttackUtility.EvalInstantAttack(aIVirtualCard.Attack, aIVirtualCard.Life, aIVirtualCard.AttackableCount, EnemyAI.EmptyPlayPtn, aIVirtualCard, null, isRush);
			}
		}
		for (int j = 0; j < AllyHandCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = AllyHandCards[j];
			num += aIVirtualCard2.GetHandBonus(BestPlayPtn, null, isIgnoreInFusion: false);
			if ((aIVirtualCard2.IsUnit || aIVirtualCard2.IsLeader) && !aIVirtualCard2.IsDead)
			{
				num += StyleQuery.GetBarrierBonus(aIVirtualCard2);
			}
		}
		float num2 = 0f;
		for (int k = 0; k < CardListSet.EnemyClassAndInplayCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = CardListSet.EnemyClassAndInplayCards[k];
			num2 += aIVirtualCard3.UpdateValue(ParamQuery, StyleQuery, EnemyAI.EmptyPlayPtn, doesUseLostLife: true);
			if ((!aIVirtualCard3.IsUnit && !aIVirtualCard3.IsLeader) || aIVirtualCard3.IsDead)
			{
				continue;
			}
			if (aIVirtualCard3.IsUnit)
			{
				num2 += 0.001f * (float)aIVirtualCard3.Attack;
				if (aIVirtualCard3.IsKiller)
				{
					num2 += 0.001f * (float)(aIVirtualCard3.Attack + aIVirtualCard3.Life);
				}
			}
			num2 += StyleQuery.GetBarrierBonus(aIVirtualCard3);
		}
		float num3 = num - num2 + SimulationExtraBonus;
		AIVirtualField currentVirtualField = AI.CurrentVirtualField;
		num3 -= (float)(EnemyPpTotal - currentVirtualField.EnemyPpTotal) * 4f;
		num3 += (float)(AllyPpTotal - currentVirtualField.AllyPpTotal) * 4f;
		float dEFAULT_HAND_BONUS = HandBonusTagCollection.DEFAULT_HAND_BONUS;
		num3 -= (float)(GetEnemyHandCardList().Count - currentVirtualField.GetEnemyHandCardList().Count) * dEFAULT_HAND_BONUS;
		num3 += (float)(AllyHandCards.Count - currentVirtualField.AllyHandCards.Count) * dEFAULT_HAND_BONUS;
		if (AllyClass.IsDead)
		{
			num3 -= 9999f;
		}
		else if (EnemyClass.IsDead)
		{
			float num4 = num3 + 9999f + (float)(EnemyClass.DefLife - EnemyClass.Life);
			AI.FieldHashAndValueTable.Add(hash, num4);
			return num4;
		}
		List<AIVirtualCard> selfRemainings = AllyInplayCards.FindAll((AIVirtualCard attacker) => attacker.IsUnit && !attacker.IsDead);
		List<AIVirtualCard> opponentRemainings = EnemyInplayCards.FindAll((AIVirtualCard c) => c.IsUnit && !c.IsDead);
		num3 += AIReincarnationUtility.CalcReincarnationValueAfterSimulation(this, BestPlayPtn, null, selfRemainings, opponentRemainings);
		if (EvoUsedCard != null)
		{
			num3 += EvoBonus;
			int num5 = ((BestPlayPtn.Count > 0) ? 9 : 8);
			float num6 = (float)Mathf.Max(EvoHandPlus + AI.ALLY.HandCardList.Count - num5, 0) * 2f;
			num3 -= num6;
			if (AI.CurrentBattleSimEvoCard != null && AI.CurrentBattleSimEvoCard.IsAttackable(EnemyAI.EmptyPlayPtn))
			{
				float num7 = (float)(AllyEvolutionCount - AI.CurrentBattleBeforeSimEvoEvolCount) * EpValue;
				num3 += num7;
			}
			else
			{
				float num8 = (float)(AllyEvolutionCount - AI.ALLY.CurrentEpCount) * EpValue;
				num3 += num8;
			}
		}
		float num9 = AILeaderLifeEvaluationUtility.Evaluate(EnemyClass.Life, EnemyClass.DefLife, isAllyLeader: false, isAllyOwner: true);
		num3 += num9;
		NextTurnLeaderDamage = 0;
		NextTurnLeaderDamage = AI.CalcHandNextTurnDamage(this);
		if (!EnemyClass.IsAllShield && EnemyClass.Life <= NextTurnLeaderDamage)
		{
			num3 += 500f;
		}
		if (AllyClass.Life != AllyClass.DefLife)
		{
			float num10 = AILeaderLifeEvaluationUtility.Evaluate(AllyClass.Life, AllyClass.DefLife, isAllyLeader: true, isAllyOwner: true);
			num3 += num10;
		}
		try
		{
			AI.FieldHashAndValueTable.Add(hash, num3);
			return num3;
		}
		catch (Exception)
		{
			return num3;
		}
	}

	public AIVirtualCard SearchVirtualCard(BattleCardBase baseCard, AIVirtualFieldSearchCardOption searchOption = null)
	{
		if (searchOption == null)
		{
			searchOption = AIVirtualFieldSearchCardOption.DefaultOption;
		}
		for (int i = 0; i < CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.AllReferableCards[i];
			if (baseCard.IsPlayer == aIVirtualCard.IsPlayer && baseCard.Index == aIVirtualCard.CardIndex)
			{
				return aIVirtualCard;
			}
		}
		List<AIVirtualCard> enemyHandCardList = GetEnemyHandCardList();
		for (int j = 0; j < enemyHandCardList.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = enemyHandCardList[j];
			if (baseCard.IsPlayer == aIVirtualCard2.IsPlayer && baseCard.Index == aIVirtualCard2.CardIndex)
			{
				return aIVirtualCard2;
			}
		}
		if (searchOption.IsSearchFromDeck)
		{
			List<AIVirtualCard> list = ((!searchOption.IsSearchFromBeforeLatestActionDeck) ? (AI.IsAllyCard(baseCard) ? AI.AllyDeckCards : AI.EnemyDeckCards) : (AI.IsAllyCard(baseCard) ? AI.BeforeLatestActionAllyDeckCards : AI.BeforeLatestActionEnemyDeckCards));
			for (int k = 0; k < list.Count; k++)
			{
				AIVirtualCard aIVirtualCard3 = list[k];
				if (baseCard.IsPlayer == aIVirtualCard3.IsPlayer && baseCard.Index == aIVirtualCard3.CardIndex)
				{
					return aIVirtualCard3;
				}
			}
		}
		AIVirtualCard aIVirtualCard4 = null;
		if (searchOption.OptionalSearchRange == BattleCardRealTargetInformation.TargetRange.DestroyedCardList)
		{
			aIVirtualCard4 = SearchVirtualCardFromDestroyedCardList(baseCard);
		}
		if (aIVirtualCard4 != null)
		{
			return aIVirtualCard4;
		}
		if (searchOption.IsOutputCannotFindError)
		{
			AIConsoleUtility.LogError($"SearchVirtualCard : AIVirtualCard not found. ID:{baseCard.CardId} Index:{baseCard.Index}");
		}
		return null;
	}

	private AIVirtualCard SearchVirtualCardFromDestroyedCardList(BattleCardBase baseCard)
	{
		List<AIVirtualCard> list = (AI.IsAllyCard(baseCard) ? CardListSet.AllyDestroyedCards : CardListSet.EnemyDestroyedCards);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (baseCard.IsPlayer == aIVirtualCard.IsPlayer && baseCard.Index == aIVirtualCard.CardIndex)
			{
				return aIVirtualCard;
			}
		}
		return null;
	}

	public AIVirtualCard SearchVirtualCard(AIVirtualCard baseCard, AIVirtualFieldSearchCardOption searchOption = null)
	{
		if (searchOption == null)
		{
			searchOption = AIVirtualFieldSearchCardOption.DefaultOption;
		}
		for (int i = 0; i < CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.AllReferableCards[i];
			if (baseCard.IsSameCard(aIVirtualCard))
			{
				return aIVirtualCard;
			}
		}
		List<AIVirtualCard> enemyHandCardList = GetEnemyHandCardList();
		for (int j = 0; j < enemyHandCardList.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = enemyHandCardList[j];
			if (baseCard.IsSameCard(aIVirtualCard2))
			{
				return aIVirtualCard2;
			}
		}
		if (searchOption.IsSearchFromDeck)
		{
			List<AIVirtualCard> list = (baseCard.IsAlly ? AI.AllyDeckCards : AI.EnemyDeckCards);
			for (int k = 0; k < list.Count; k++)
			{
				AIVirtualCard aIVirtualCard3 = list[k];
				if (baseCard.IsSameCard(aIVirtualCard3))
				{
					return aIVirtualCard3;
				}
			}
		}
		if (searchOption.IsOutputCannotFindError)
		{
			AIConsoleUtility.LogError("SearchVirtualCard : AIVirtualCard not found");
		}
		return null;
	}

	public List<AIVirtualCard> GetEnemyHandCardList()
	{
		if (IsLatestActionField)
		{
			return _latestActionEnemyHandCards;
		}
		return _enemyHandCards;
	}

	public List<AIVirtualCard> GetSimulationHandCards()
	{
		if (IsLatestActionField)
		{
			List<AIVirtualCard> list = new List<AIVirtualCard>(AllyHandCards);
			list.AddRange(_latestActionEnemyHandCards);
			return list;
		}
		return AllyHandCards;
	}

	public float EvaluateThreaten()
	{
		ulong hash = GetHash();
		if (AI.FieldHashAndThreatenTable.ContainsKey(hash))
		{
			return AI.FieldHashAndThreatenTable[hash];
		}
		List<AIVirtualCard> list = EnemyInplayCards.FindAll((AIVirtualCard c) => c.IsUnit && !c.IsDead);
		if (list.Count <= 0)
		{
			try
			{
				float num = EvaluateTurnEndTagThreaten(null, null, AllyClass, AllyClass.Life);
				AI.FieldHashAndThreatenTable.Add(hash, num);
				return num;
			}
			catch (Exception)
			{
				return 0f;
			}
		}
		List<int[]> list2 = AIMathematicsLibrary.EnumerateIndexListPermutations(list.Count);
		float num2 = float.MinValue;
		bool flag = true;
		for (int num3 = -1; num3 < AllyInplayCards.Count; num3++)
		{
			AIVirtualCard aIVirtualCard = ((num3 >= 0) ? AllyInplayCards[num3] : AllyClass);
			if (aIVirtualCard.IsDead || aIVirtualCard.IsIndependent || aIVirtualCard.IsAmulet || aIVirtualCard.IsSneak)
			{
				continue;
			}
			List<int[]> successPatterns = new List<int[]>();
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				int[] array = list2[num4];
				if (!IsCurrentPermListIncludedInSuccessPatterns(successPatterns, array))
				{
					Tuple<int, int>[] array2 = new Tuple<int, int>[array.Length];
					for (int num5 = 0; num5 < list.Count; num5++)
					{
						AIVirtualCard aIVirtualCard2 = list[num5];
						array2[num5] = new Tuple<int, int>(aIVirtualCard2.Attack, aIVirtualCard2.Life);
					}
					flag = false;
					float num6 = EvaluateSimpleAttack(list, array, array2, aIVirtualCard, successPatterns);
					if (num6 > num2)
					{
						num2 = num6;
					}
				}
			}
		}
		if (flag)
		{
			num2 = 0f;
		}
		try
		{
			AI.FieldHashAndThreatenTable.Add(hash, num2);
			return num2;
		}
		catch (Exception)
		{
			return num2;
		}
	}

	private Tuple<int, int>[] CalculateEvoStats(List<AIVirtualCard> attackers, Tuple<int, int>[] status)
	{
		if (attackers == null || attackers.Count <= 0)
		{
			return status;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < attackers.Count; i++)
		{
			AIVirtualCard aIVirtualCard = attackers[i];
			if (!aIVirtualCard.IsEvolution && (aIVirtualCard.IsNotConsumeEp || EnemyEvolutionCount > 0))
			{
				int evoAttackPlus = aIVirtualCard.EvoAttackPlus;
				if (evoAttackPlus > num)
				{
					num = evoAttackPlus;
					num2 = aIVirtualCard.EvoLifePlus;
					num3 = i;
				}
			}
		}
		status[num3].first += num;
		status[num3].second += num2;
		return status;
	}

	public float EvaluateSimpleAttack(List<AIVirtualCard> attackers, int[] attackSequence, Tuple<int, int>[] status, AIVirtualCard target, List<int[]> successPatterns)
	{
		int[] array = new int[attackers.Count];
		for (int i = 0; i < attackers.Count; i++)
		{
			array[i] = attackers[i].MaxAttackableCount;
		}
		if (target.IsLeader)
		{
			status = CalculateEvoStats(attackers, status);
		}
		float num = 0f;
		bool flag = false;
		int num2 = 0;
		float num3 = 0f;
		int num4 = target.Life;
		int j;
		for (j = 0; j < attackSequence.Length; j++)
		{
			int num5 = attackSequence[j];
			AIVirtualCard aIVirtualCard = attackers[num5];
			AIVirtualAttackInfo situation = new AIVirtualAttackInfo(aIVirtualCard, target);
			if (aIVirtualCard.IsCantAttackAll)
			{
				continue;
			}
			int attackDamageToCertainCard = AIAttackTagSimulator.GetAttackDamageToCertainCard(this, situation, aIVirtualCard);
			float attackBonus = aIVirtualCard.GetAttackBonus(BestPlayPtn, situation);
			AISimulationBuffInfoCollection aISimulationBuffInfoCollection = null;
			if (CardListSet.HasOtherAttackBuffHolder)
			{
				aISimulationBuffInfoCollection = AIAttackTagSimulator.GetBuffInfoListWhenCertainAttack(this, situation);
			}
			AISimulationBuffInfo buffInfo = aISimulationBuffInfoCollection?.GetBuffInfoToCertainCard(aIVirtualCard);
			for (int k = 0; k < array[num5]; k++)
			{
				if (!AIAttackSimulationUtility.IsExecuteAttackValuable(this, aIVirtualCard, status[num5].first, status[num5].second, attackDamageToCertainCard, buffInfo, attackBonus))
				{
					continue;
				}
				if (aIVirtualCard.IsDestroyWhenAttack)
				{
					if (!target.IsIndependent && !target.IsIndestructible && target.IsUnit)
					{
						flag = true;
					}
					break;
				}
				aISimulationBuffInfoCollection?.PseudoApplyBuffForSimpleAttack(aIVirtualCard, attackers, status);
				int attackerAtk = status[num5].first;
				int second = status[num5].second;
				second -= attackDamageToCertainCard;
				if (aIVirtualCard.IsAttackByLife)
				{
					attackerAtk = second;
				}
				if (target.IsUnit)
				{
					if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.ClashBonus))
					{
						num3 += aIVirtualCard.EvaluateClashBonus();
					}
					if (target.TagCollectionContainer.HasTag(AIPlayTagType.ClashBonus))
					{
						num3 -= target.EvaluateClashBonus();
					}
				}
				num3 += attackBonus;
				int targetAtk = target.Attack;
				AIAttackTagSimulator.ExecuteAttackByLife(this, aIVirtualCard, target, ref attackerAtk, ref targetAtk);
				int num6 = target.SimulateDamageAmount(AIAttackTagSimulator.GetAttackDamageToCertainCard(this, situation, target), isSkillDamage: true);
				num6 += target.SimulateDamageAmount(aIVirtualCard.SimulateAttackAmount(attackerAtk, situation));
				num2 += num6;
				int num7 = aIVirtualCard.SimulateDamageAmount(target.SimulateAttackAmount(situation));
				num4 -= num6;
				second -= num7;
				status[num5].second = second;
				if (target.IsUnit)
				{
					if (aIVirtualCard.IsKiller || num4 <= 0)
					{
						flag = true;
						num += target.Value;
					}
					if (target.IsKiller || second <= 0)
					{
						num -= aIVirtualCard.Value;
						break;
					}
					num -= (float)num7;
					if (flag)
					{
						break;
					}
				}
				else if (target.IsLeader && num4 <= 0)
				{
					num2 = target.Life;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		num += EvaluateTurnEndTagThreaten(attackers, status, target, num4);
		if (target.IsLeader)
		{
			num += AILeaderLifeEvaluationUtility.Evaluate(AllyClass.Life - num2, AllyClass.Life, isAllyLeader: true, isAllyOwner: false);
		}
		num += num3;
		if (flag)
		{
			int[] array2 = new int[j + 1];
			for (int l = 0; l <= j; l++)
			{
				array2[l] = attackSequence[l];
			}
			successPatterns.Add(array2);
		}
		return num;
	}

	private float EvaluateTurnEndTagThreaten(List<AIVirtualCard> attackers, Tuple<int, int>[] status, AIVirtualCard target, int targetLife)
	{
		float num = 0f;
		if (!CardListSet.HasEnemyTurnEndTagHolder)
		{
			return num;
		}
		Tuple<int, int>[] allInplayStatusList = new Tuple<int, int>[CardListSet.BothClassAndInplayCards.Count];
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard inplay = CardListSet.BothClassAndInplayCards[i];
			if (inplay.IsSameCard(target))
			{
				allInplayStatusList[i] = new Tuple<int, int>(inplay.Attack, targetLife);
				continue;
			}
			int num2 = -1;
			if (attackers != null && attackers.Count > 0)
			{
				num2 = attackers.FindIndex((AIVirtualCard c) => c.IsSameCard(inplay));
			}
			if (num2 >= 0)
			{
				allInplayStatusList[i] = status[num2];
			}
			else
			{
				allInplayStatusList[i] = new Tuple<int, int>(inplay.Attack, inplay.Life);
			}
		}
		for (int num3 = 0; num3 < CardListSet.EnemyTurnEndTagHolders.Count; num3++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.EnemyTurnEndTagHolders[num3];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSkillLost)
			{
				float num4 = aIVirtualCard.TagCollectionContainer.TurnEndTags.CalculateEnemyTurnEndTagThreaten(aIVirtualCard, ref allInplayStatusList);
				num += (float)((!aIVirtualCard.IsAlly) ? 1 : (-1)) * num4;
			}
		}
		return num;
	}

	public void ReverseAllCardIsSelfTurn()
	{
		for (int i = 0; i < CardListSet.AllReferableCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.AllReferableCards[i];
			aIVirtualCard.IsSelfTurn = !aIVirtualCard.IsSelfTurn;
		}
	}

	private bool IsCurrentPermListIncludedInSuccessPatterns(List<int[]> successPatterns, int[] permList)
	{
		for (int i = 0; i < successPatterns.Count; i++)
		{
			int[] array = successPatterns[i];
			_ = array.Length;
			_ = permList.Length;
			bool flag = true;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != permList[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterOtherCardAttackTags(AIVirtualAttackInfo situation)
	{
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.BothClassAndInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherAttack))
			{
				aIVirtualCard.TagCollectionContainer.OtherAttackTags.RegisterConditionPassedTagProgress(this, aIVirtualCard, situation);
			}
		}
	}

	public void ApplyOtherEvolveTags(AISituationInfo situation, AIVirtualCard evolver)
	{
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.BothClassAndInplayCards[i];
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherEvo))
			{
				aIVirtualCard.TagCollectionContainer.OtherEvoTags.RegisterPassedConditionTags(aIVirtualCard, evolver, this, BestPlayPtn, situation);
			}
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenSelfAndOtherEvo))
			{
				aIVirtualCard.TagCollectionContainer.SelfAndOtherEvoTags.RegisterPassedConditionTags(aIVirtualCard, evolver, this, BestPlayPtn, situation);
			}
		}
	}

	public void RecoverPp(int amount)
	{
		AllyPp = Mathf.Min(AllyPp + amount, AllyPpTotal);
	}

	public bool CheckDestroyByEvoTags(AISituationInfo situation, AIVirtualCard destroyTarget)
	{
		if (situation.ActionType != AIOperationType.EVOLVE)
		{
			AIConsoleUtility.LogError("AIVirtualField.CheckDestroyByEvoTags() eorror!! situation.ActionType is not EVOLVE");
			return false;
		}
		AIVirtualCard actor = situation.Actor;
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenEvo) && actor.TagCollectionContainer.EvoTags.CheckDestroyByEvolveTags(actor, situation, destroyTarget))
		{
			return true;
		}
		List<AIVirtualCard> otherEvoTagHolders = CardListSet.OtherEvoTagHolders;
		if (otherEvoTagHolders == null)
		{
			return false;
		}
		for (int i = 0; i < otherEvoTagHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = otherEvoTagHolders[i];
			if (!aIVirtualCard.IsSameCard(actor) && aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenOtherEvo) && aIVirtualCard.TagCollectionContainer.OtherEvoTags.CheckDestroyByOtherEvoTags(aIVirtualCard, destroyTarget, this, BestPlayPtn, situation))
			{
				return true;
			}
			if (aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenSelfAndOtherEvo) && aIVirtualCard.TagCollectionContainer.SelfAndOtherEvoTags.CheckDestroyBySelfAndOtherEvoTags(aIVirtualCard, destroyTarget, this, BestPlayPtn, situation))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNoSkipAttackInPlayPtn()
	{
		if (BestPlayPtn == null)
		{
			return false;
		}
		for (int i = 0; i < BestPlayPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyHandCards[BestPlayPtn[i]];
			if (aIVirtualCard.TagCollectionContainer.HasTag(AIPlayTagType.NoSkipAttack) && aIVirtualCard.TagCollectionContainer.NoSkipAttackTags.IsNoSkipAttack(aIVirtualCard, BestPlayPtn, ParamQuery))
			{
				return true;
			}
		}
		return false;
	}

	public int GetInplayAttackSumToLeader(bool isAlly)
	{
		if (isAlly)
		{
			return AllyInplayCards.Sum((AIVirtualCard card) => (card.IsUnit && AIAttackSimulationUtility.IsAttackPossible(this, card.AttackLeaderSituation)) ? card.Attack : 0);
		}
		return EnemyInplayCards.Sum((AIVirtualCard card) => (!card.IsDead) ? card.Attack : 0);
	}

	public float GetInplayTotalValue()
	{
		return GetInplayFollowerTotalValue(EnemyAI.EmptyPlayPtn, isAlly: true) - GetInplayFollowerTotalValue(EnemyAI.EmptyPlayPtn, isAlly: false);
	}

	public float GetInplayFollowerTotalValue(List<int> playPtn, bool isAlly)
	{
		float num = 0f;
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsUnit)
			{
				num += aIVirtualCard.EvaluateValueOnField(playPtn, null, useStyle: true);
			}
		}
		return num;
	}

	public int GetAllyMaxAttackableLife(bool isAlly)
	{
		int num = 0;
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsAttackable(EnemyAI.EmptyPlayPtn) && list[i].Life > num)
			{
				num = list[i].Life;
			}
		}
		return num;
	}

	public int GetMemberMaxAttack(AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null)
		{
			return 0;
		}
		int num = 0;
		if (tagOwner.IsAlly && playPtn.IsNotNullOrEmpty())
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				AIVirtualCard aIVirtualCard = AllyHandCards[playPtn[i]];
				if (aIVirtualCard.IsUnit && AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, AllyHandCards, filters, playPtn, tagOwner, situation) && aIVirtualCard.Attack > num)
				{
					num = aIVirtualCard.Attack;
				}
				if (aIVirtualCard.IsSameCard(tagOwner))
				{
					break;
				}
			}
		}
		List<AIVirtualCard> list = (tagOwner.IsAlly ? AllyInplayCards : EnemyInplayCards);
		for (int j = 0; j < list.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = list[j];
			if (!aIVirtualCard2.IsDead && aIVirtualCard2.IsUnit && AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard2, list, filters, playPtn, tagOwner, situation) && aIVirtualCard2.Attack > num)
			{
				num = aIVirtualCard2.Attack;
			}
		}
		return num;
	}

	public int GetMemberMaxLife(List<int> playPtn, bool isAlly)
	{
		int num = 0;
		if (isAlly && playPtn.IsNotNullOrEmpty())
		{
			for (int i = 0; i < playPtn.Count; i++)
			{
				AIVirtualCard aIVirtualCard = AllyHandCards[playPtn[i]];
				if (aIVirtualCard.IsUnit && aIVirtualCard.Life > num)
				{
					num = aIVirtualCard.Life;
				}
			}
		}
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		for (int j = 0; j < list.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = list[j];
			if (aIVirtualCard2.IsUnit && aIVirtualCard2.Life > num)
			{
				num = aIVirtualCard2.Life;
			}
		}
		return num;
	}

	public int GetEnemyInPlayMinAttack()
	{
		if (EnemyInplayCards.IsNotNullOrEmpty() && EnemyInplayCards.Any((AIVirtualCard c) => c.IsUnit && !c.IsDead))
		{
			int b = EnemyInplayCards.Min((AIVirtualCard card) => (!card.IsUnit || card.IsDead) ? int.MaxValue : card.Attack);
			return Mathf.Max(0, b);
		}
		return 0;
	}

	public int GetEnemyInPlayMaxAttack()
	{
		if (EnemyInplayCards.IsNotNullOrEmpty())
		{
			int b = EnemyInplayCards.Max((AIVirtualCard card) => (card.IsUnit && !card.IsDead) ? card.Attack : 0);
			return Mathf.Max(0, b);
		}
		return 0;
	}

	public int GetAllyInplayMaxAttack()
	{
		if (AllyInplayCards == null || AllyInplayCards.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyInplayCards[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead && aIVirtualCard.Attack > num)
			{
				num = aIVirtualCard.Attack;
			}
		}
		return num;
	}

	public int GetMaxAllyAttackableUnitAttack(List<int> playPtn, bool isAlly)
	{
		int num = 0;
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsUnit && list[i].IsAttackable(playPtn) && list[i].Attack > num)
			{
				num = list[i].Attack;
			}
		}
		return num;
	}

	public int GetMaxAllyNonAttackableUnitAttack(List<int> playPtn)
	{
		int num = 0;
		for (int i = 0; i < AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyInplayCards[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsAttackable(playPtn) && (aIVirtualCard.AttackableCount != 0 || aIVirtualCard.MaxAttackableCount <= 0))
			{
				int attack = aIVirtualCard.Attack;
				if (attack > num)
				{
					num = attack;
				}
			}
		}
		return num;
	}

	public int GetAttackTargetMaxAtk(AIVirtualCard tagOwner)
	{
		int num = 0;
		List<AIVirtualCard> list = (tagOwner.IsAlly ? EnemyInplayCards : AllyInplayCards);
		bool flag = list.Any((AIVirtualCard card) => card.IsUnit && !card.IsCantUnderAnyAttack() && card.IsGuard);
		bool isIgnoreGuard = tagOwner.IsIgnoreGuard;
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			AIVirtualCard aIVirtualCard = list[num2];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsCantUnderAnyAttack() && (!flag || aIVirtualCard.IsGuard || isIgnoreGuard) && aIVirtualCard.Attack > num)
			{
				num = aIVirtualCard.Attack;
			}
		}
		return num;
	}

	public int GetMemberAtkSum(List<int> playPtn, bool isAlly)
	{
		int num = 0;
		if (isAlly && playPtn.IsNotNullOrEmpty())
		{
			num += AllyHandCards.Sum((AIVirtualCard card) => (playPtn.Contains(AllyHandCards.IndexOf(card)) && card.IsUnit) ? card.Attack : 0);
		}
		List<AIVirtualCard> source = (isAlly ? AllyInplayCards : EnemyInplayCards);
		return num + source.Sum((AIVirtualCard card) => (card.IsUnit && !card.IsDead) ? card.Attack : 0);
	}

	public int GetMemberLifeSum(List<int> playPtn, bool isAlly)
	{
		int num = 0;
		if (isAlly && playPtn.IsNotNullOrEmpty())
		{
			num += AllyHandCards.Sum((AIVirtualCard card) => (playPtn.Contains(AllyHandCards.IndexOf(card)) && card.IsUnit) ? card.Life : 0);
		}
		List<AIVirtualCard> source = (isAlly ? AllyInplayCards : EnemyInplayCards);
		return num + source.Sum((AIVirtualCard card) => (card.IsUnit && !card.IsDead) ? card.Life : 0);
	}

	public int GetTotalDamage(List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(CardListSet.BothClassAndInplayCards, filters, tagOwner, BestPlayPtn, situation);
		if (list != null && list.Count > 0)
		{
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				num += list[i].MaxLife - list[i].Life;
			}
			return num;
		}
		return 0;
	}

	public int GetHandMinCost(AIVirtualCard tagOwner)
	{
		if (AllyHandCards.Count <= 0)
		{
			return 0;
		}
		int num = int.MaxValue;
		for (int i = 0; i < AllyHandCards.Count; i++)
		{
			if ((tagOwner == null || tagOwner.CardIndex != AllyHandCards[i].CardIndex || tagOwner.BaseId != AllyHandCards[i].BaseId) && AllyHandCards[i].Cost < num)
			{
				num = AllyHandCards[i].Cost;
			}
		}
		return num;
	}

	public int GetHandMaxCost(AIVirtualCard tagOwner)
	{
		if (AllyHandCards.Count <= 0 || !tagOwner.IsAlly)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < AllyHandCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyHandCards[i];
			if (!tagOwner.IsSameCard(aIVirtualCard) && aIVirtualCard.BaseCost > num)
			{
				num = aIVirtualCard.BaseCost;
			}
		}
		return num;
	}

	public int GetEvolutionCountInGame(AIVirtualCard tagOwner, AIScriptTokenArgType type)
	{
		switch (type)
		{
		case AIScriptTokenArgType.BOTH:
			return AllyEvolvedCountInGame + EnemyEvolvedCountInGame;
		case AIScriptTokenArgType.ALLY:
		case AIScriptTokenArgType.OPPONENT:
			if (type == AIScriptTokenArgType.ALLY != tagOwner.IsAlly)
			{
				return EnemyEvolvedCountInGame;
			}
			return AllyEvolvedCountInGame;
		default:
			AIConsoleUtility.LogError($"EVO_COUNT_IN_GAME Error!! {type} is invalid arg!!");
			return 0;
		}
	}

	public int GetEvolutionCountInPreviousTurn(AIVirtualCard tagOwner, AIScriptTokenArgType type)
	{
		if ((uint)(type - 84) <= 1u)
		{
			if (type == AIScriptTokenArgType.ALLY != tagOwner.IsAlly)
			{
				return EnemyEvolvedCountInPreviousTurn;
			}
			return AllyEvolvedCountInPreviousTurn;
		}
		AIConsoleUtility.LogError($"EVO_COUNT_IN_PREVIOUS_TURN Error!! {type} is invalid arg!!");
		return 0;
	}

	public void AddRallyCount(int addCount, bool isAlly)
	{
		if (isAlly)
		{
			AllyRallyCount += addCount;
		}
		else
		{
			EnemyRallyCount += addCount;
		}
	}

	public bool IsHandAllRemovalWaiting()
	{
		for (int i = 0; i < AllyHandCards.Count; i++)
		{
			if (AllyHandCards[i].HasWhenPlayRemovalTag())
			{
				return true;
			}
		}
		return false;
	}

	public float EvalRandomBounce(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, int count)
	{
		if (count == 0)
		{
			return 0f;
		}
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(CardListSet.BothInplayCards, argList, tagOwner, playPtn, null);
		if (!list.IsNotNullOrEmpty())
		{
			return 0f;
		}
		int restPp = AI.PlayPtnRecorder.GetRestPp(playPtn, field);
		if (list.Count <= count)
		{
			float num = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (!aIVirtualCard.IsIndependent && !aIVirtualCard.IsIndestructible)
				{
					float num2 = aIVirtualCard.EvaluateBounceValue(playPtn, restPp);
					num2 += aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false) * (float)(aIVirtualCard.IsAlly ? 1 : (-1));
					num += num2;
				}
			}
			return num;
		}
		float num3 = 0f;
		List<int> list2 = new List<int>();
		for (int j = 0; j < list.Count; j++)
		{
			list2.Add(j);
		}
		List<int[]> list3 = AIMathematicsLibrary.EnumerateCombinations(list2, count).ToList();
		for (int k = 0; k < list3.Count; k++)
		{
			int[] array = list3[k];
			for (int l = 0; l < array.Length; l++)
			{
				AIVirtualCard aIVirtualCard2 = list[array[l]];
				if (!aIVirtualCard2.IsIndependent)
				{
					float num4 = aIVirtualCard2.EvaluateBounceValue(playPtn, restPp);
					num4 += aIVirtualCard2.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false) * (float)(aIVirtualCard2.IsAlly ? 1 : (-1));
					num3 += num4;
				}
			}
		}
		return num3 / (float)list3.Count;
	}

	public float EvalTargetingBounce(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, List<int> playPtn)
	{
		List<AIVirtualCard> bothInplayCards = CardListSet.BothInplayCards;
		bothInplayCards = AIFilteringUtility.MultipleFiltering(bothInplayCards, argList, tagOwner, playPtn, null);
		if (!bothInplayCards.IsNotNullOrEmpty())
		{
			return 0f;
		}
		bothInplayCards = AITargetSelectFilteringUtility.SelectCandidatesWithForceTargeting(bothInplayCards, tagOwner, playPtn);
		float num = float.MinValue;
		for (int i = 0; i < bothInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = bothInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsOnField && !aIVirtualCard.IsIndependent && (aIVirtualCard.IsAlly == tagOwner.IsAlly || (!aIVirtualCard.IsUntouchable && !aIVirtualCard.IsSneak)))
			{
				int restPp = 0;
				if (aIVirtualCard.IsAlly)
				{
					restPp = AI.PlayPtnRecorder.GetRestPp(playPtn, this);
				}
				float num2 = aIVirtualCard.EvaluateBounceValue(playPtn, restPp);
				num2 += (float)(aIVirtualCard.IsAlly ? 1 : (-1)) * aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public float EvalAllBounce(List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, List<int> playPtn)
	{
		List<AIVirtualCard> bothInplayCards = CardListSet.BothInplayCards;
		bothInplayCards = AIFilteringUtility.MultipleFiltering(bothInplayCards, argList, tagOwner, playPtn, null);
		if (!bothInplayCards.IsNotNullOrEmpty())
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < bothInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = bothInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsOnField && !aIVirtualCard.IsIndependent)
			{
				int restPp = 0;
				if (aIVirtualCard.IsAlly)
				{
					restPp = AI.PlayPtnRecorder.GetRestPp(playPtn, this);
				}
				float num2 = aIVirtualCard.EvaluateBounceValue(playPtn, restPp);
				num2 += (float)(aIVirtualCard.IsAlly ? 1 : (-1)) * aIVirtualCard.GetAllLeaveBonus(playPtn, useIgnoreInBattle: false);
				num += num2;
			}
		}
		return num;
	}

	public float EvalForcedExchangeVirtual(int attackLimit, List<int> playPtn)
	{
		AIVirtualCard aIVirtualCard = null;
		float num = float.MaxValue;
		int num2 = 0;
		for (int i = 0; i < AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard2 = AllyInplayCards[i];
			if (aIVirtualCard2.IsKiller)
			{
				continue;
			}
			int attack = aIVirtualCard2.Attack;
			if (aIVirtualCard2.IsAttackable(EnemyAI.EmptyPlayPtn) && attack >= attackLimit)
			{
				float num3 = aIVirtualCard2.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: false);
				if (num3 < num)
				{
					aIVirtualCard = aIVirtualCard2;
					num = num3;
					num2 = attack;
				}
			}
		}
		if (aIVirtualCard == null)
		{
			return 0f;
		}
		AIVirtualCard aIVirtualCard3 = null;
		float num4 = float.MinValue;
		for (int j = 0; j < EnemyInplayCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard4 = EnemyInplayCards[j];
			float num5 = aIVirtualCard4.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: false);
			if (num5 > num4)
			{
				num4 = num5;
				aIVirtualCard3 = aIVirtualCard4;
			}
		}
		if (aIVirtualCard3 == null)
		{
			return 0f;
		}
		int life = aIVirtualCard3.Life;
		if (num2 >= life || attackLimit >= life)
		{
			return 0f;
		}
		return num4 - num;
	}

	public void DrawCard(bool isAlly, int drawCount, List<int> playPtn, AISituationInfo situation)
	{
		bool num = IsResonance(isAlly);
		VirtualDrawCount += drawCount;
		if (isAlly)
		{
			AllyDeckCount -= drawCount;
		}
		else
		{
			OpponentDeckCount -= drawCount;
		}
		if (num || !IsResonance(isAlly))
		{
			return;
		}
		AllyTurnResonanceStartCount++;
		AllyGameResonanceStartCount++;
		if (CardListSet.HasResonanceHolder)
		{
			List<AIVirtualCard> resonanceTagHolders = CardListSet.ResonanceTagHolders;
			for (int i = 0; i < resonanceTagHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = resonanceTagHolders[i];
				aIVirtualCard.TagCollectionContainer.ResonanceTags.RegisterPassedConditionTags(isAlly, aIVirtualCard, this, playPtn, situation);
			}
		}
	}

	public void AddDeckCard(int tokenId, int tokenCount, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isPseudo = false)
	{
		if (tokenId <= 0 || tokenCount <= 0)
		{
			return;
		}
		List<DeckVirtualCard> list = null;
		AIVirtualCard tokenFromId = AI.tokenManager.GetTokenFromId(tokenId, tagOwner.IsAlly, this);
		if (tokenFromId == null)
		{
			AIConsoleUtility.LogError("AddDeckCard: tokenCard is null");
			return;
		}
		for (int i = 0; i < tokenCount; i++)
		{
			list = AIParamQuery.AddElementToList(new DeckVirtualCard(tokenFromId.BaseCard, this), list);
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		bool isAlly = tagOwner.IsAlly;
		List<AIVirtualCard> list2 = (isAlly ? AllyGameAddUpdateDeckCards : EnemyGameAddUpdateDeckCards);
		if (isPseudo)
		{
			for (int j = 0; j < list.Count; j++)
			{
				list2.Add(list[j]);
			}
			return;
		}
		bool flag = IsResonance(isAlly);
		for (int k = 0; k < list.Count; k++)
		{
			DummyDeckContainer.AppendDummyCard(list[k], isAlly);
			list2.Add(list[k]);
		}
		if (flag || !IsResonance(isAlly))
		{
			return;
		}
		AllyTurnResonanceStartCount++;
		AllyGameResonanceStartCount++;
		if (CardListSet.HasResonanceHolder)
		{
			List<AIVirtualCard> resonanceTagHolders = CardListSet.ResonanceTagHolders;
			for (int l = 0; l < resonanceTagHolders.Count; l++)
			{
				AIVirtualCard aIVirtualCard = resonanceTagHolders[l];
				aIVirtualCard.TagCollectionContainer.ResonanceTags.RegisterPassedConditionTags(isAlly, aIVirtualCard, this, playPtn, situation);
			}
		}
	}

	public bool IsResonance(bool isAlly)
	{
		return ((isAlly ? AllyDeckCount : OpponentDeckCount) + DummyDeckContainer.GetDeck(isAlly).Count) % 2 == 0;
	}

	public bool IsPlagueCity()
	{
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			if (CardListSet.BothClassAndInplayCards[i].IsPlagueCity(this, BestPlayPtn))
			{
				return true;
			}
		}
		return false;
	}

	public void CallAfterLeaderAttackSimulation()
	{
		if (this.OnAfterLeaderAttackSimulation != null)
		{
			this.OnAfterLeaderAttackSimulation.Call();
		}
	}

	public List<AIVirtualCard> GetPlayptnCards(List<int> playPtn)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (playPtn[i] < AllyHandCards.Count)
			{
				if (list == null)
				{
					list = new List<AIVirtualCard>();
				}
				list.Add(AllyHandCards[playPtn[i]]);
			}
		}
		return list;
	}

	public List<AIVirtualCard> GetMemberCardList(List<int> playPtn)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>(CardListSet.AllyClassAndInplayCards);
		List<AIVirtualCard> playptnCards = GetPlayptnCards(playPtn);
		if (playptnCards != null && playptnCards.Count > 0)
		{
			list.AddRange(playptnCards);
		}
		return list;
	}

	public List<AIVirtualCard> GetFieldCountCardList(List<int> playPtn)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>(AllyInplayCards);
		List<AIVirtualCard> playptnCards = GetPlayptnCards(playPtn);
		if (playptnCards != null && playptnCards.Count > 0)
		{
			list.AddRange(playptnCards);
		}
		return list;
	}

	public int GetSummonDrunkenAtkMax(AIVirtualCard tagOwner)
	{
		int num = 0;
		foreach (AIVirtualCard item in AI.IsAllyCard(tagOwner) ? AllyInplayCards : EnemyInplayCards)
		{
			if (item.IsSummonDrunkenness && item.Attack > num)
			{
				num = item.Attack;
			}
		}
		return num;
	}

	public List<AIVirtualCard> CreatePlayCardList(List<int> playPtn)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < playPtn.Count; i++)
		{
			list = AIParamQuery.AddElementToList(AllyHandCards[playPtn[i]], list);
		}
		return list;
	}

	public bool IsPlayableHandList(AISinglePlayptnRecord playPtnRecord, List<AIVirtualCard> allySuicideList)
	{
		int num = 5 - AllyInplayCards.Count();
		int num2 = allySuicideList?.Count ?? 0;
		List<int> playPtn = playPtnRecord.PlayPtn;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyHandCards[playPtn[i]];
			int num3 = 0;
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard.FindRealActor(playPtnRecord), aIVirtualCard, AIOperationType.PLAY);
			int playSpaceRequired = GetPlaySpaceRequired(aIVirtualCard, playPtn, situation, needsTokenCount: false);
			List<AITokenInformation> allySideTokenIdsOfPlaySituation = AIPlayTokenSimulationUtility.GetAllySideTokenIdsOfPlaySituation(this, playPtn, situation);
			if (allySideTokenIdsOfPlaySituation != null)
			{
				num3 = allySideTokenIdsOfPlaySituation.Count;
			}
			num -= playSpaceRequired;
			if (num < 0)
			{
				num2 -= -num;
				if (num2 < 0)
				{
					return false;
				}
				num = 0;
			}
			if (StyleQuery.IsPlayBreak(aIVirtualCard, playPtn, situation))
			{
				num += playSpaceRequired;
			}
			num = Mathf.Max(num - num3, 0);
		}
		return true;
	}

	public float CalcMinEvaluateValue(List<int> playPtn, bool isAlly)
	{
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		if (list.Count <= 0)
		{
			return 0f;
		}
		float num = float.MaxValue;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			float num2 = aIVirtualCard.EvaluateValueOnField(aIVirtualCard.IsAlly ? playPtn : EnemyAI.EmptyPlayPtn, null, useStyle: true);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return num;
	}

	public float CalcMaxEvaluateValue(List<int> playPtn, bool isAlly)
	{
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		if (list.Count <= 0)
		{
			return 0f;
		}
		float num = float.MinValue;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			float num2 = aIVirtualCard.EvaluateValueOnField(aIVirtualCard.IsAlly ? playPtn : EnemyAI.EmptyPlayPtn, null, useStyle: true);
			if (num2 > num)
			{
				num = num2;
			}
		}
		return num;
	}

	public int CalculateHandPtnRequiredSpace(AISinglePlayptnRecord playptnRecord)
	{
		int num = 0;
		List<int> playPtn = playptnRecord.PlayPtn;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = AllyHandCards[playPtn[i]];
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard.FindRealActor(playptnRecord), aIVirtualCard, AIOperationType.PLAY);
			num += GetPlaySpaceRequired(aIVirtualCard, playPtn, situation);
		}
		return num;
	}

	public int GetPlaySpaceRequired(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool needsTokenCount = true)
	{
		int num = 0;
		if (tagOwner.IsUnit || tagOwner.IsAmulet)
		{
			num = ((!tagOwner.IsAccelerated(this, playPtn)) ? 1 : 0);
		}
		if (needsTokenCount)
		{
			List<AITokenInformation> allySideTokenIdsOfPlaySituation = AIPlayTokenSimulationUtility.GetAllySideTokenIdsOfPlaySituation(this, playPtn, situation);
			if (allySideTokenIdsOfPlaySituation != null)
			{
				num += allySideTokenIdsOfPlaySituation.Count;
			}
		}
		return num;
	}

	public bool IsEnableIgnoreFanfareBonus(List<int> playPtn)
	{
		for (int i = 0; i < CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = CardListSet.BothClassAndInplayCards[i];
			if (!aIVirtualCard.IsDead && aIVirtualCard.IsEnableIgnoreFanfareBonus(playPtn))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEnemyHasGuard()
	{
		for (int i = 0; i < EnemyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = EnemyInplayCards[i];
			if (aIVirtualCard.IsGuard && !aIVirtualCard.IsSneak && !aIVirtualCard.IsSkillCantUnderAnyAttack)
			{
				return true;
			}
		}
		return false;
	}

	public bool isForceImmediateAttack()
	{
		List<AIVirtualCard> forceImmediateAttackHolders = CardListSet.ForceImmediateAttackHolders;
		if (forceImmediateAttackHolders == null || forceImmediateAttackHolders.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < forceImmediateAttackHolders.Count; i++)
		{
			if (forceImmediateAttackHolders[i].IsForceImmediateAttack(this))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSuccess()
	{
		if (!AllyClass.IsDead)
		{
			return EnemyClass.IsDead;
		}
		return false;
	}

	public int GetPredictiveAllyFieldSpace()
	{
		return AI.GetAllySpaceNum() + AI.SpareSpace;
	}

	public int GetNecromanceCountInGame(AISituationInfo situation, bool isAllyCount)
	{
		int result = (isAllyCount ? AllyNecromancedCountInGame : EnemyNecromancedCountInGame);
		if (situation == null || situation.PreprocessRecorder == null || !situation.PreprocessRecorder.HasRecord)
		{
			return result;
		}
		int num = 0;
		for (int i = 0; i < situation.PreprocessRecorder.RecordList.Count; i++)
		{
			AISinglePreprocessRecord aISinglePreprocessRecord = situation.PreprocessRecorder.RecordList[i];
			if (aISinglePreprocessRecord.OriginalCard.IsAlly == isAllyCount)
			{
				num += aISinglePreprocessRecord.NecromanceCount;
			}
		}
		if (num > 0)
		{
			AIConsoleUtility.LogError("GetNecromanceCountInGame: Unexpected count detected. This func isn't implemented yet.");
		}
		return result;
	}

	public void UpdateVirtualFieldWhenEvaluation(AIVirtualCard playCard, AISituationInfo situation)
	{
		if (playCard.IsSpell || playCard.IsAccelerated(this, null, situation))
		{
			AIPlayCardSimulationUtility.UpdateFieldWhenEvaluateSpellCard(playCard, this);
		}
		playCard.ExecuteWhenPlayTagsForEvaluation(this, EnemyAI.EmptyPlayPtn, situation);
		if (situation != null && situation.PreprocessRecorder != null && situation.PreprocessRecorder.HasRecord)
		{
			for (int i = 0; i < situation.PreprocessRecorder.RecordList.Count; i++)
			{
				AISinglePreprocessRecord aISinglePreprocessRecord = situation.PreprocessRecorder.RecordList[i];
				if (aISinglePreprocessRecord.OriginalCard.IsAlly)
				{
					AllyNecromancedCountInGame += aISinglePreprocessRecord.NecromanceCount;
				}
			}
		}
		this.AllActivateCountHolderIncrement(situation, AIPlayTagType.PlayActivateCount, playCard);
		IsNoInstantAttackRecheck();
	}

	public void RegisterBestPlayPtnRecord(AISinglePlayptnRecord record)
	{
		if (_isPlayptnSimulationField)
		{
			BestPlayptnRecordOnSim = record;
		}
	}

	public void UpdateBestPlayptnRecordOnSim(List<int> addedHandIndexList)
	{
		if (_isPlayptnSimulationField)
		{
			if (addedHandIndexList != null && addedHandIndexList.Count > 0)
			{
				List<int> list = new List<int>(addedHandIndexList);
				list.AddRange(BestPlayPtn);
				BestPlayPtn = list;
			}
			if (!_isCreateTemporaryPlayPtnRecord)
			{
				BestPlayptnRecordOnSim = CreateTemporaryPlayPtnRecord(BestPlayPtn);
			}
			else
			{
				AIConsoleUtility.LogError("CreateTemporaryPlayPtnRecordで無限ループが発生しそうです！！！！！");
			}
		}
	}

	public AISinglePlayptnRecord GetPlayptnRecordOnSim(List<int> playPtn)
	{
		if (_isPlayptnSimulationField)
		{
			if (BestPlayptnRecordOnSim != null)
			{
				if (IsBestPlayPtnRecordAndPlayPtnConsistent(playPtn))
				{
					return BestPlayptnRecordOnSim;
				}
			}
			else
			{
				AIConsoleUtility.LogError("AIVirtualField.GetPlayptnRecordOnSim() error!! BestPlayptnRecordOnSim is null!!");
			}
			if (_isCreateTemporaryPlayPtnRecord)
			{
				AIConsoleUtility.LogError("CreateTemporaryPlayPtnRecordで無限ループが発生しそうです！！！！！");
				return null;
			}
			return CreateTemporaryPlayPtnRecord(playPtn);
		}
		return AI.PlayPtnRecorder.FindMatchedPlayPtnRecord(playPtn, this);
	}

	private AISinglePlayptnRecord CreateTemporaryPlayPtnRecord(List<int> playPtn)
	{
		_isCreateTemporaryPlayPtnRecord = true;
		AIVirtualFieldRollBackBasicProcessor rollBackProcessor = new AIVirtualFieldRollBackBasicProcessor(this);
		AISinglePlayptnRecord aISinglePlayptnRecord = new AISinglePlayptnRecord(playPtn, this, 0);
		AIPlayptnRecorder.BuildSinglePlayptnRecord(playPtn, aISinglePlayptnRecord, this, rollBackProcessor);
		_isCreateTemporaryPlayPtnRecord = false;
		return aISinglePlayptnRecord;
	}

	private bool IsBestPlayPtnRecordAndPlayPtnConsistent(List<int> playPtn)
	{
		if (playPtn == null || BestPlayptnRecordOnSim.PlayPtn == null)
		{
			return false;
		}
		List<int> playPtn2 = BestPlayptnRecordOnSim.PlayPtn;
		if (playPtn.Count != playPtn2.Count)
		{
			return false;
		}
		for (int i = 0; i < playPtn.Count; i++)
		{
			if (playPtn[i] != playPtn2[i])
			{
				return false;
			}
			if (playPtn[i] < 0 || AllyHandCards.Count <= playPtn[i])
			{
				return false;
			}
			AIVirtualCard aIVirtualCard = AllyHandCards[playPtn[i]];
			AIVirtualCard card = BestPlayptnRecordOnSim.PlayedCardList[i].Card;
			if (!aIVirtualCard.IsSameCard(card))
			{
				return false;
			}
		}
		return true;
	}

	public void ExecuteEarthRite(bool isAlly, int count, AISituationInfo situation, bool isPseudo, AISinglePreprocessRecord record)
	{
		bool flag = record?.EarthRiteContainer.Any() ?? false;
		List<AIVirtualCard> list = (isAlly ? AllyInplayCards : EnemyInplayCards);
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (aIVirtualCard.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL) && !aIVirtualCard.IsDead && aIVirtualCard.WhiteRitualCount > 0)
			{
				int num = aIVirtualCard.EarthRite(count, situation, isPseudo);
				count -= num;
				if (!flag)
				{
					record.AddConsumedEarthRite(aIVirtualCard, num);
				}
				if (count <= 0)
				{
					break;
				}
			}
		}
	}

	public void RemoveAllyHandCard(AIVirtualCard hand, bool isRemoveByPlay = false)
	{
		int num = AllyHandCards.FindIndex((AIVirtualCard c) => c.IsSameCard(hand));
		if (num < 0 || AllyHandCards.Count <= num)
		{
			AIConsoleUtility.LogError("ExecuteDiscard() error!! " + hand.CardName + " is not found in hand");
			return;
		}
		AllyHandCards.Remove(hand);
		CardListSet.RemoveAllyHandCard(hand);
		if (BestPlayPtn == null || BestPlayPtn.Count <= 0)
		{
			return;
		}
		if (BestPlayPtn.Contains(num))
		{
			if (!isRemoveByPlay)
			{
				IsRemovedPlayPtnCard = true;
			}
			BestPlayPtn.Remove(num);
		}
		for (int num2 = 0; num2 < BestPlayPtn.Count; num2++)
		{
			if (BestPlayPtn[num2] > num)
			{
				BestPlayPtn[num2]--;
			}
		}
	}

	public void RemoveEnemyHandCard(AIVirtualCard hand)
	{
		int num = -1;
		List<AIVirtualCard> enemyHandCardList = GetEnemyHandCardList();
		for (int i = 0; i < enemyHandCardList.Count; i++)
		{
			if (hand.IsSameCard(enemyHandCardList[i]))
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			enemyHandCardList.RemoveAt(num);
		}
	}

	public void AddCannotPlayInformation(AICannotPlayInformation info)
	{
		CannotPlayInformationList = AIParamQuery.AddElementToList(info, CannotPlayInformationList);
	}

	public void RemoveCannotPlayInformation(AIVirtualCard owner, List<AIScriptTokenBase> filters)
	{
		if (CannotPlayInformationList == null || CannotPlayInformationList.Count <= 0)
		{
			return;
		}
		AICannotPlayInformation aICannotPlayInformation = null;
		for (int i = 0; i < CannotPlayInformationList.Count; i++)
		{
			AICannotPlayInformation aICannotPlayInformation2 = CannotPlayInformationList[i];
			if (aICannotPlayInformation2.IsEqual(owner, filters))
			{
				aICannotPlayInformation = aICannotPlayInformation2;
				break;
			}
		}
		if (aICannotPlayInformation != null)
		{
			CannotPlayInformationList.Remove(aICannotPlayInformation);
		}
	}

	public void SetParametersFromOtherField(AIVirtualField otherField)
	{
		CannotPlayInformationList = otherField.CannotPlayInformationList;
		HealRecorderCollection = otherField.HealRecorderCollection;
		_playedCardContainer = otherField._playedCardContainer;
	}

	public ulong GetHash()
	{
		ulong num = 0uL;
		num += (ulong)((long)AllyPp * 97L);
		num += (ulong)((long)AllyEvolutionCount * 4441L);
		num += (ulong)((long)EnemyEvolutionCount * 1567L);
		num += (ulong)((long)VirtualCemetery.GetCemeteryCount(isAlly: true) * 11503L);
		num += (ulong)((long)VirtualCemetery.GetCemeteryCount(isAlly: false) * 1949L);
		num += (ulong)((long)AllyDeckCount * 13219L);
		num += (ulong)((long)OpponentDeckCount * 503L);
		num += (ulong)((long)NextTurnLeaderDamage * 5443L);
		num += (ulong)((long)EvoHandPlus * 6869L);
		num += (ulong)Mathf.Floor(EvoBonus * 1000f) * 253427;
		num += (ulong)Mathf.Floor(EpValue * 1000f) * 1215197;
		num += (ulong)Mathf.Floor(SimulationExtraBonus * 1000f) * 241;
		for (int i = 0; i < AllyHandCards.Count; i++)
		{
			num += AllyHandCards[i].GetHash() * 1061;
		}
		for (int j = 0; j < CardListSet.AllyClassAndInplayCards.Count; j++)
		{
			num += CardListSet.AllyClassAndInplayCards[j].GetHash() * 11299;
		}
		for (int k = 0; k < CardListSet.EnemyClassAndInplayCards.Count; k++)
		{
			num += CardListSet.EnemyClassAndInplayCards[k].GetHash() * 307;
		}
		if (BestPlayPtn != null && BestPlayPtn.Count > 0)
		{
			for (int l = 0; l < BestPlayPtn.Count; l++)
			{
				num += PRIME_NUMBERS_FOR_BEST_PLAYPTN[BestPlayPtn[l] % 9];
			}
		}
		return num;
	}

	public ulong CalculatePlayptnHash(List<int> playPtn)
	{
		ulong num = 0uL;
		for (int i = 0; i < playPtn.Count; i++)
		{
			int num2 = playPtn[i];
			if (num2 >= AllyHandCards.Count)
			{
				return 0uL;
			}
			num += AllyHandCards[num2].GetHash();
		}
		return num;
	}

	public void InitializeGameStartInnerTags()
	{
		AI.StyleQuery.ExecuteGameStartAttachTag(this);
		if (Data.CurrentFormat == Format.Avatar)
		{
			EnemyClass.TagCollectionContainer.CreateChoiceBraveTag(EnemyClass);
		}
	}
}
