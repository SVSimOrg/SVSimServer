using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Operation;
using Wizard.Battle.Resource;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class Prediction
{
	protected VfxMgr _vfxMgr;

	protected IBattleResourceMgr _battleResourceManager;

	private BattlePlayerPair _pair;

	private List<BattleCardBase> _selectCards;

	public Prediction(IBattleResourceMgr battleResourceManager, BattlePlayerPair pair)
	{
		_battleResourceManager = battleResourceManager;
		_pair = pair;
		_vfxMgr = new VfxMgr();
		_selectCards = new List<BattleCardBase>();
	}

	public void Dispose()
	{
		if (_vfxMgr != null)
		{
			_vfxMgr.Dispose();
			_vfxMgr = null;
		}
		_battleResourceManager = null;
		_pair = null;
		_selectCards.Clear();
	}

	// _isFeatureEnabled was a `public static bool` default-false with no writers anywhere in the
	// codebase — dead in headless and never toggled by the client build we ported from. Collapsed
	// to a constant-false so per-battle isolation isn't polluted by a process-wide flag.
	private bool IsEnabled() => false;

	public void TurnEnd()
	{
		if (IsEnabled())
		{
			_pair = _pair.Self.BattleMgr.GetBattlePlayerPair(isPlayer: true);
			BattlePlayerPair randomAll = TurnEnd(_pair, SimulationSelection.All);
			BattlePlayerPair randomNone = TurnEnd(_pair, SimulationSelection.None);
			Display(randomAll, randomNone);
		}
	}

	public void Clear()
	{
		if (IsEnabled())
		{
			_vfxMgr.RegisterSequentialVfx(ClearView(_pair));
		}
	}

	public void Update(float deltaTime)
	{
		_vfxMgr.Update(deltaTime);
	}

	public void Play(BattleCardBase card)
	{
		if (IsEnabled() && !card.Skills.CheckWhenPlaySelectTargetSkillCondition)
		{
			BattlePlayerPair randomAll = Play(_pair, card, null, SimulationSelection.All);
			BattlePlayerPair randomNone = Play(_pair, card, null, SimulationSelection.None);
			Display(randomAll, randomNone);
		}
	}

	private bool ShouldDisplayWarningMessage(BattlePlayerPair randomAll, BattlePlayerPair randomNone)
	{
		if (FindUsedRandomSkillOriginalCard(_pair.Self, randomAll.Self) != null)
		{
			return true;
		}
		if (FindUsedRandomSkillOriginalCard(_pair.Opponent, randomAll.Opponent) != null)
		{
			return true;
		}
		if (FindUsedRandomSkillOriginalCard(_pair.Opponent, randomNone.Self) != null)
		{
			return true;
		}
		return FindUsedRandomSkillOriginalCard(_pair.Opponent, randomNone.Opponent) != null;
	}

	private void Display(BattlePlayerPair randomAll, BattlePlayerPair randomNone)
	{
		if (IsEnabled())
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			parallelVfxPlayer.Register(ClearView(_pair));
			if (ShouldDisplayWarningMessage(randomAll, randomNone))
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
			AddCardToWarning(_pair.Self, randomAll.Self);
			AddCardToWarning(_pair.Opponent, randomAll.Opponent);
			AddCardToWarning(_pair.Self, randomNone.Self);
			AddCardToWarning(_pair.Opponent, randomNone.Opponent);
			parallelVfxPlayer.Register(CreateDisplayVfx(_pair.Self, randomAll.Self, randomNone.Self));
			parallelVfxPlayer.Register(CreateDisplayVfx(_pair.Opponent, randomAll.Opponent, randomNone.Opponent));
			_vfxMgr.RegisterSequentialVfx(parallelVfxPlayer);
		}
	}

	private VfxBase CreateDisplayVfx(BattlePlayerBase original, BattlePlayerBase randomAll, BattlePlayerBase randomNone)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> classAndInPlayCardList = original.ClassAndInPlayCardList;
		for (int i = 0; i < classAndInPlayCardList.Count; i++)
		{
			BattleCardBase origin = classAndInPlayCardList[i];
			if (randomAll.PredictionWarningCards.Any((BattleCardBase x) => x.EquelsID(origin)) || randomNone.PredictionWarningCards.Any((BattleCardBase x) => x.EquelsID(origin)))
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
			}
			if (randomNone.CemeteryList.Any((BattleCardBase x) => x.EquelsID(origin)))
			{
				parallelVfxPlayer.Register(NullVfx.GetInstance());
				continue;
			}
			if (randomNone.BanishList.Any((BattleCardBase x) => x.EquelsID(origin)))
			{
				if (!randomNone.ClassAndInPlayCardList.Any((BattleCardBase x) => x.EquelsID(origin)))
				{
					parallelVfxPlayer.Register(NullVfx.GetInstance());
				}
				continue;
			}
			BattleCardBase battleCardBase = randomNone.ClassAndInPlayCardList.FindFromCardId(origin);
			if (battleCardBase != null)
			{
				int damage = origin.Life - battleCardBase.Life;
				if (battleCardBase.IsDead)
				{
					parallelVfxPlayer.Register(NullVfx.GetInstance());
				}
				else if (battleCardBase.HasMoreDamageThan(origin))
				{
					parallelVfxPlayer.Register(NullVfx.GetInstance());
				}
			}
		}
		return parallelVfxPlayer;
	}

	private static void ClearCardListView(IList<BattleCardBase> cardList)
	{
		for (int i = 0; i < cardList.Count; i++)
		{
			IBattleCardView battleCardView = cardList[i].BattleCardView;
			if (battleCardView.HasChild("forecast_banish") || battleCardView.HasReservedAttachChild("forecast_banish"))
			{
				battleCardView.DestroyChild("forecast_banish");
			}
			if (battleCardView.HasChild("forecast_damage") || battleCardView.HasReservedAttachChild("forecast_damage"))
			{
				battleCardView.DestroyChild("forecast_damage");
			}
			if (battleCardView.HasChild("forecast_death") || battleCardView.HasReservedAttachChild("forecast_death"))
			{
				battleCardView.DestroyChild("forecast_death");
			}
			if (battleCardView.HasChild("forecast_random") || battleCardView.HasReservedAttachChild("forecast_random"))
			{
				battleCardView.DestroyChild("forecast_random");
			}
		}
	}

	private static VfxBase ClearView(BattlePlayerPair pair)
	{
		ClearCardListView(pair.Self.ClassAndInPlayCardList);
		ClearCardListView(pair.Self.HandCardList);
		ClearCardListView(pair.Opponent.ClassAndInPlayCardList);
		return NullVfx.GetInstance();
	}

	private static BattleCardBase FindUsedRandomSkillOriginalCard(BattlePlayerBase originalBattlePlayer, BattlePlayerBase forecastBattlePlayer)
	{
		BattleCardBase[] forecastBattlePlayerCardArray = GetForecastBattlePlayerCardArray(forecastBattlePlayer);
		foreach (BattleCardBase card in forecastBattlePlayerCardArray)
		{
			if (card.Skills == null)
			{
				continue;
			}
			for (int j = 0; j < card.Skills.Count(); j++)
			{
				SkillBase skillBase = card.Skills.ElementAt(j);
				if (skillBase.UsedRandom && (skillBase is Skill_damage || skillBase is Skill_destroy || skillBase is Skill_banish) && (!(skillBase is Skill_banish skill_banish) || !(skill_banish.ApplyingTargetFilter is SkillTargetDeckFilter)))
				{
					return originalBattlePlayer.AllCards.Single((BattleCardBase c) => c.EquelsID(card));
				}
			}
		}
		return null;
	}

	private static void AddCardToWarning(BattlePlayerBase originalBattlePlayer, BattlePlayerBase forecastBattlePlayer)
	{
		BattleCardBase[] forecastBattlePlayerCardArray = GetForecastBattlePlayerCardArray(forecastBattlePlayer);
		List<BattleCardBase> list = new List<BattleCardBase>();
		bool flag = false;
		bool flag2 = false;
		foreach (BattleCardBase battleCardBase in forecastBattlePlayerCardArray)
		{
			if (battleCardBase.Skills == null)
			{
				continue;
			}
			if (!flag2)
			{
				List<SkillBase> repeatSkillsForPrediction = BattleUtility.GetRepeatSkillsForPrediction(originalBattlePlayer, battleCardBase);
				if (repeatSkillsForPrediction != null)
				{
					for (int j = 0; j < repeatSkillsForPrediction.Count(); j++)
					{
						if (repeatSkillsForPrediction.ElementAt(j).Used)
						{
							flag2 = true;
							break;
						}
					}
				}
			}
			for (int k = 0; k < battleCardBase.Skills.Count(); k++)
			{
				SkillBase skillBase = battleCardBase.Skills.ElementAt(k);
				if (skillBase.Used)
				{
					forecastBattlePlayer.PredictionWarningCards.Add(battleCardBase);
				}
				if (battleCardBase.IsInplay && skillBase.OnWhenSummonOtherStart != 0)
				{
					list.Add(battleCardBase);
				}
				if (skillBase.UsedRandom && skillBase is Skill_summon_card)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			forecastBattlePlayer.PredictionWarningCards.UnionWith(list);
		}
		if (flag2)
		{
			forecastBattlePlayer.PredictionWarningCards.Add(forecastBattlePlayer.Class);
		}
	}

	private static BattleCardBase[] GetForecastBattlePlayerCardArray(BattlePlayerBase forecastBattlePlayer)
	{
		BattleCardBase[] array = new BattleCardBase[forecastBattlePlayer.HandCardList.Count + forecastBattlePlayer.ClassAndInPlayCardList.Count + forecastBattlePlayer.DeckCardList.Count + forecastBattlePlayer.CemeteryList.Count + forecastBattlePlayer.BanishList.Count + forecastBattlePlayer.FusionIngredientList.Count];
		int arrayLastIndex = 0;
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.HandCardList);
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.ClassAndInPlayCardList);
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.DeckCardList);
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.CemeteryList);
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.BanishList);
		AddCardsToArray(array, ref arrayLastIndex, forecastBattlePlayer.FusionIngredientList);
		return array;
	}

	private static void AddCardsToArray(BattleCardBase[] targetArray, ref int arrayLastIndex, IList<BattleCardBase> cards)
	{
		for (int i = 0; i < cards.Count; i++)
		{
			targetArray[arrayLastIndex] = cards[i];
			arrayLastIndex++;
		}
	}

	private static BattlePlayerPair TurnEnd(BattlePlayerPair sourcePair, SimulationSelection random)
	{
		sourcePair.Self.BattleMgr.InstanceIsForecast = true;
		bool isRecovery = sourcePair.Self.BattleMgr.IsRecovery;
		sourcePair.Self.BattleMgr.IsRecovery = true;
		BattlePlayerPair battlePlayerPair = sourcePair.VirtualClone(CloneActualFlags.All);
		ChangeFilters(battlePlayerPair, random);
		CloneSkillsPreprocessAndBuffInfo(sourcePair, battlePlayerPair);
		battlePlayerPair.Self.GetTurnEndSkillProcess().Process(battlePlayerPair);
		sourcePair.Self.BattleMgr.IsRecovery = isRecovery;
		sourcePair.Self.BattleMgr.InstanceIsForecast = false;
		return battlePlayerPair;
	}

	private static BattlePlayerPair Play(BattlePlayerPair sourcePair, IBattleCardUniqueID playCardId, List<BattleCardBase> skillTargets, SimulationSelection random)
	{
		sourcePair.Self.BattleMgr.InstanceIsForecast = true;
		bool isRecovery = sourcePair.Self.BattleMgr.IsRecovery;
		sourcePair.Self.BattleMgr.IsRecovery = true;
		BattlePlayerPair battlePlayerPair = sourcePair.VirtualClone(CloneActualFlags.All);
		ChangeFilters(battlePlayerPair, random);
		CloneSkillsPreprocessAndBuffInfo(sourcePair, battlePlayerPair);
		Tuple<List<BattleCardBase>, List<int>> tuple = OperationSimulator.Play_GetTargetsAndChoice(battlePlayerPair, skillTargets);
		List<BattleCardBase> first = tuple.first;
		List<int> second = tuple.second;
		ActionProcessor actionProcessor = new ActionProcessor(battlePlayerPair);
		BattleCardBase card = battlePlayerPair.Self.HandCardList.FindFromCardId(playCardId);
		battlePlayerPair.Self.SetupActionProcessorEvent(actionProcessor);
		battlePlayerPair.Opponent.SetupActionProcessorEvent(actionProcessor);
		actionProcessor.PlayCard(card, first, second);
		sourcePair.Self.BattleMgr.IsRecovery = isRecovery;
		sourcePair.Self.BattleMgr.InstanceIsForecast = false;
		return battlePlayerPair;
	}

	public static void ChangeFilters(BattlePlayerPair virtualPair, SimulationSelection random = SimulationSelection.All)
	{
		foreach (BattleCardBase allCard in virtualPair.Self.AllCards)
		{
			foreach (SkillBase skill in allCard.Skills)
			{
				if (skill.ApplySelectFilter is SimulateRandomSelectFilter simulateRandomSelectFilter)
				{
					simulateRandomSelectFilter._selection = random;
				}
			}
		}
	}

	public static void CloneSkillsPreprocessAndBuffInfo(BattlePlayerPair sourcePair, BattlePlayerPair virtualPair)
	{
		CloneSkillsPreprocessAndBuffInfo(sourcePair.Self, virtualPair.Self, virtualPair.Opponent);
		CloneSkillsPreprocessAndBuffInfo(sourcePair.Opponent, virtualPair.Opponent, virtualPair.Self);
	}

	private static void CloneSkillsPreprocessAndBuffInfo(BattlePlayerBase sourcePlayer, BattlePlayerBase virtualPlayer, BattlePlayerBase virtualOpponentPlayer)
	{
		List<BattleCardBase> list = virtualPlayer.AllCards.ToList();
		List<BattleCardBase> allCardsWithCemeteryAndBanish = virtualPlayer.AllCardsWithCemeteryAndBanish;
		List<BattleCardBase> oppCloneCards = virtualOpponentPlayer.AllCards.ToList();
		List<BattleCardBase> list2 = sourcePlayer.AllCards.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			SkillCollectionBase skills = list[i].Skills;
			SkillCollectionBase skills2 = list2[i].Skills;
			for (int j = 0; j < skills.Count(); j++)
			{
				SkillBase skillBase = skills2.Get(j);
				SkillBase skillBase2 = skills.Get(j);
				if (skillBase == null || skillBase2 == null)
				{
					continue;
				}
				for (int k = 0; k < skillBase2.PreprocessList.Count; k++)
				{
					if (k < skillBase.PreprocessList.Count)
					{
						skillBase2.PreprocessList[k].Clone(skillBase.PreprocessList[k], skillBase2);
					}
				}
				skillBase2.CloneBuffInfoContainer(allCardsWithCemeteryAndBanish, oppCloneCards, skillBase);
			}
		}
	}
}
