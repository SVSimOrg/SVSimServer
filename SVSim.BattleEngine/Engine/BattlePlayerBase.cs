using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class BattlePlayerBase : IBattlePlayerReadOnlyInfo
{
	public class TurnAndCard
	{
		public int Turn { get; private set; }

		public bool IsSelfTurn { get; private set; }

		public IReadOnlyBattleCardInfo Card { get; private set; }

		public bool IsTurnEnd { get; private set; }

		public TurnAndCard(int turn, bool isSelfTurn, IReadOnlyBattleCardInfo card, bool isTurnEnd)
		{
			Turn = turn;
			Card = card;
			IsSelfTurn = isSelfTurn;
			IsTurnEnd = isTurnEnd;
		}
	}

	public class CardAndId
	{
		public IReadOnlyBattleCardInfo Card { get; private set; }

		public int Id { get; private set; }

		public CardAndId(IReadOnlyBattleCardInfo card, int id)
		{
			Card = card;
			Id = id;
		}
	}

	public class CardAndTribe
	{
		public IReadOnlyBattleCardInfo Card { get; private set; }

		public List<CardBasePrm.TribeType> Tribes { get; private set; }

		public CardAndTribe(IReadOnlyBattleCardInfo card, List<CardBasePrm.TribeType> tribes)
		{
			Card = card;
			Tribes = tribes;
		}
	}

	public class CardAndValue
	{
		public IReadOnlyBattleCardInfo Card { get; private set; }

		public int Value { get; private set; }

		public CardAndValue(IReadOnlyBattleCardInfo card, int value)
		{
			Card = card;
			Value = value;
		}
	}

	public enum CARD_MANAGEMENT
	{
		NONE,
		DESTROY,
		BANISH,
		RETURN,
		FUSION_MATERIAL,
		GETON,
		GETOFF,
		SUMMON
	}

	public class SideLogInfo
	{
		public SkillBase Skill;

		public SideLogInfo(SkillBase skill)
		{
			Skill = skill;
		}
	}

	public class MyRotationBonusCondition
	{
		public MyRotationInfo.MyRotationBonus MyRotationBonus { get; }

		public int RemainingIncreaseAddPptotalTurn { get; private set; }

		public bool IsRemainIncreaseAddPptotalTurn { get; private set; }

		public int RemainingSkillCount { get; private set; }

		public bool IsRemainSkill { get; private set; }

		public MyRotationBonusCondition(MyRotationInfo.MyRotationBonus myRotationBonus)
		{
			MyRotationBonus = myRotationBonus;
			RemainingIncreaseAddPptotalTurn = myRotationBonus.IncreaseAddPptotalTurn;
			IsRemainIncreaseAddPptotalTurn = RemainingIncreaseAddPptotalTurn > 0;
			RemainingSkillCount = myRotationBonus.AttachAbilities.Length;
			IsRemainSkill = RemainingSkillCount > 0;
		}

		public bool GetAndReduceAddPpTurn()
		{
			bool num = RemainingIncreaseAddPptotalTurn > 0;
			if (num)
			{
				RemainingIncreaseAddPptotalTurn--;
			}
			return num;
		}

		public void ReduceSkillCount()
		{
			RemainingSkillCount--;
		}

		public void UseUpAddPpTotalBonus()
		{
			IsRemainIncreaseAddPptotalTurn = RemainingIncreaseAddPptotalTurn > 0;
		}

		public void UseUpSkill()
		{
			IsRemainSkill = RemainingSkillCount > 0;
		}
	}

	public class AvatarBattleDescInfo
	{
		public string DescText;

		public string Cost;

		public List<int> ReplaySkillDescriptionValueList;

		public AvatarBattleDescInfo(string descText, string cost)
		{
			DescText = descText;
			Cost = cost;
			ReplaySkillDescriptionValueList = new List<int>();
		}
	}

	public enum CEMETERY_TYPE
	{
		NORMAL,
		FIELD_RETURN_HAND_OVER,
		DECK_DRAW_HAND_OVER
	}

	public class SummonInfo
	{
		public bool IsPlayer { get; private set; }

		public SkillBaseSummon.SummonedCardsList SummonedCardsList { get; private set; }

		public SkillBaseSummon.SUMMON_TYPE SummonType { get; private set; }

		public bool IsReanimate { get; private set; }

		public bool IsDeckSelfSummon { get; private set; }

		public SummonInfo(bool isPlayer, SkillBaseSummon.SummonedCardsList summonedCardsList, SkillBaseSummon.SUMMON_TYPE summonType, bool isReanimate = false, bool isDeckSelfSummon = false)
		{
			IsPlayer = isPlayer;
			SummonedCardsList = summonedCardsList;
			SummonType = summonType;
			IsReanimate = isReanimate;
			IsDeckSelfSummon = isDeckSelfSummon;
		}
	}

	public List<BattleCardBase> SelfDiscardList = new List<BattleCardBase>();

	protected BattlePlayerBase _opponentBattlePlayer;

	private List<IBattlePlayerSkill> _skillList = new List<IBattlePlayerSkill>();

	protected IBattlePlayerVfxCreator m_vfxCreator;

	protected readonly IInnerOptionsBuilder _innerOptionsBuilder;

	private int _ppTotal;

	protected int m_EpTotal;

	public bool CantPlayChoiceBrave;

	public HashSet<BattleCardBase> PredictionWarningCards = new HashSet<BattleCardBase>();

	public Func<SkillProcessor, VfxBase> OnTurnStartSkillAfter;

	public Func<SkillProcessor, VfxBase> OnTurnEndSkillAfter;

	public Action OnTurnStartComplete;

	public Action OnPreTurnEndComplete;

	public Action OnPostTurnEndComplete;

	public Action OnEndOneSkillProcess;

	public List<MyRotationBonusCondition> BonusConditionList;

	public List<BossRushSpecialSkill> BossRushSpecialSkillList;

	public AvatarBattleDescInfo AvatarBattlePassiveSkillDescInfo;

	public List<AvatarBattleDescInfo> ChoiceBraveSkillDescInfoList;

	protected int _gameUsedEpCount;

	protected int _turnUsedEpCount;

	public BattleManagerBase BattleMgr { get; protected set; }

	public virtual bool IsGameFirst => false;

	public BattleCamera BattleCamera { get; private set; }

	public BackGroundBase BackGround { get; private set; }

	protected DataMgr _dataMgr { get; set; }

	public virtual bool IsPlayer => true;

	public virtual int Turn
	{
		get
		{
			if (!BattleMgr.IsFirst)
			{
				return BattleMgr.SecondTurn;
			}
			return BattleMgr.FirstTurn;
		}
		set
		{
			if (BattleMgr.IsFirst)
			{
				BattleMgr.FirstTurn = value;
			}
			else
			{
				BattleMgr.SecondTurn = value;
			}
		}
	}

	public int Pp { get; set; }

	public int PpTotal
	{
		get
		{
			return _ppTotal;
		}
		set
		{
			_ppTotal = value;
		}
	}

	public int EpTotal
	{
		get
		{
			return m_EpTotal;
		}
		set
		{
			m_EpTotal = value;
		}
	}

	public int CurrentEpCount { get; private set; }

	public int EvolveWaitTurnCount { get; set; }

	public bool NowTurnEvol { get; set; }

	public bool IsEpEvolveThisTurn { get; set; }

	public bool IsEvolve
	{
		get
		{
			if (NowTurnEvol && CurrentEpCount > 0)
			{
				return EvolveWaitTurnCount <= 0;
			}
			return false;
		}
	}

	public bool IsExceptionEvolve
	{
		get
		{
			if (NowTurnEvol && EvolveWaitTurnCount <= 0)
			{
				return InPlayCards.Any((BattleCardBase c) => !c.IsEvolution && CheckNotConsumeEpCard(c));
			}
			return false;
		}
	}

	public int GameUsedEpCount => _gameUsedEpCount;

	public int TurnUsedEpCount => _turnUsedEpCount;

	public int Bp { get; private set; }

	public bool IsAlreadyChoiceBraveInThisTurn { get; set; } = true;

	public bool IsChoiceBraveEffectTiming { get; set; }

	public List<BattleCardBase> ChoiceBraveCards
	{
		get
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			SkillBase skillBase = Class.Skills.FirstOrDefault((SkillBase s) => s.OnWhenChoiceBrave != 0);
			if (skillBase == null)
			{
				return list;
			}
			List<int> list2 = SkillOptionValue.ParseOptionTokenID(skillBase.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.card_id, "_OPT_NULL_")).ToList();
			for (int num = 0; num < list2.Count(); num++)
			{
				BattleCardBase item = BattleMgr.CreateTransformCardRegisterVfx(Class, list2[num], IsPlayer, null, isRecoveryFinish: false, isChoice: true);
				list.Add(item);
			}
			return list;
		}
	}

	public bool CanPlayAnyChoiceBraveCard
	{
		get
		{
			if (IsAlreadyChoiceBraveInThisTurn)
			{
				return false;
			}
			List<BattleCardBase> choiceBraveCards = ChoiceBraveCards;
			for (int i = 0; i < choiceBraveCards.Count(); i++)
			{
				if (choiceBraveCards[i].CanPlayAsChoiceBraveCard)
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual bool CanChoiceBraveThisTurn
	{
		get
		{
			if (!IsAlreadyChoiceBraveInThisTurn)
			{
				return IsChoiceBraveEffectTiming;
			}
			return false;
		}
	}

	public virtual bool CanChoiceBrave
	{
		get
		{
			if (CanChoiceBraveThisTurn && CanPlayAnyChoiceBraveCard && BattleView.IsTouchable())
			{
				return !BattleView.IsSelecting;
			}
			return false;
		}
	}

	public bool IsShortageDeckLose { get; protected set; }

	public bool IsShortageDeckWin => Class.SkillApplyInformation.IsShortageDeckWin;

	public bool IsChangeShortageDeck
	{
		get
		{
			if (!Class.SkillApplyInformation.IsShortageDeckWin)
			{
				return Class.Skills.Any((SkillBase s) => s.OnWhenShortageDeck != 0);
			}
			return false;
		}
	}

	public List<BattleCardBase> HandCardList { get; private set; }

	public List<BattleCardBase> DeckCardList { get; private set; }

	public List<BattleCardBase> BattleStartDeckCardList { get; set; }

	public List<BattleCardBase> DeckSkillCardList { get; private set; }

	public List<BattleCardBase> ClassAndInPlayCardList { get; private set; }

	public List<BattleCardBase> CemeteryList { get; private set; }

	public List<BattleCardBase> BanishList { get; set; }

	public List<BattleCardBase> FusionIngredientList { get; set; }

	public List<BattleCardBase> TurnFusionCards { get; set; }

	public List<BattleCardBase> NecromanceZoneList { get; set; }

	public List<BattleCardBase> DiscardedCardList { get; set; }

	public List<BattleCardBase> FusionIngredientAndDiscardedCardList { get; set; }

	public List<BattleCardBase> ReservedCardList { get; set; }

	public List<BattleCardBase> UniteList { get; set; }

	public List<BattleCardBase> GetOnList { get; set; }

	public List<BattleCardBase> BlackHole { get; set; }

	public List<BattleCardBase> ChoiceBraveCardList { get; set; }

	public List<BattleCardBase> PredictionCemeteryRandomCards { get; private set; }

	public List<BattleCardBase> PredictionDamageRandomCards { get; private set; }

	public List<BattleCardBase> PredictionBanishRandomCards { get; private set; }

	public virtual IStatusPanelControl StatusPanelControl
	{
		get
		{
			if ((bool)BattleView.StatusParentPanel)
			{
				return BattleView.StatusParentPanel.GetComponent<IStatusPanelControl>();
			}
			return new NullStatusPanelControl();
		}
	}

	public ClassInformationUIController ClassInformationUIController { get; protected set; }

	public bool IsBuffDetail
	{
		get
		{
			if (IsShowBuffDetail || IsRecordingBuffDetail)
			{
				return !IsRecordingExceptBuffDetail;
			}
			return false;
		}
	}

	public bool IsShowBuffDetail { get; set; }

	public bool IsRecordingBuffDetail { get; set; }

	public bool IsRecordingExceptBuffDetail { get; set; }

	public SideLogInfo SideLogSkill { get; set; }

	protected BattleCardBase _class { get; set; }

	public HandControl HandControl => BattleView.HandControl;

	public abstract IBattlePlayerView BattleView { get; }

	public abstract IEmotion Emotion { get; }

	public bool IsSelfTurn { get; set; }

	public List<BattleCardBase> ReturnList { get; set; }

	public List<List<BattleCardBase>> LastTargetCardsList { get; set; }

	public List<BattleCardBase> InHandCards { get; set; }

	public List<BattleCardBase> SkillDiscards { get; set; }

	public List<BattleCardBase> SkillBanishCards { get; set; }

	public List<BattleCardBase> HealingCards { get; set; }

	public List<BattleCardBase> SkillSummonedCards { get; set; }

	public List<BattleCardBase> SummonedCards { get; set; }

	public BattleCardBase DrewSkillCard { get; set; }

	public List<BattleCardBase> EvolvedCards { get; set; }

	public List<BattleCardBase> DestroyedWhenDestroyCards { get; set; }

	public List<TurnAndIntValue> TurnPlayCardCountInfo { get; set; }

	public List<TurnAndIntValue> TurnFusionCountInfo { get; set; }

	public List<TurnAndIntValue> TurnEvolveCardCountInfo { get; set; }

	public int TurnNecromanceCount { get; set; }

	public int GameNecromanceCount { get; set; }

	public int GameUsedPpCount { get; set; }

	public BattleCardBase CardOnPlay { get; set; }

	public List<BattleCardBase> TurnPlayCards { get; set; }

	public List<BattleCardBase> TurnDrawCards { get; set; }

	public List<CardAndId> TurnDrawTokenCardsWithId { get; set; }

	public List<BattleCardBase> GameDrawCards { get; set; }

	public List<BattleCardBase> GameDrawTokenCards { get; set; }

	public List<BattleCardBase> GameAddUpdateDeckCards { get; set; }

	public List<TurnAndCard> GameSummonCards { get; set; }

	public List<CardAndTribe> GameSummonMomentTribe { get; set; }

	public List<CardAndTribe> GamePlayMomentTribe { get; set; }

	public List<BattleCardBase> GamePlayMomentSpellChargeCards { get; set; }

	public List<CardAndTribe> GameUpdateDeckMomentTribe { get; set; }

	public List<BattleCardBase> GamePlayCards { get; set; }

	public List<TurnAndCard> GameTurnPlayCards { get; set; }

	public List<TurnAndCard> GameEnhancePlayCards { get; set; }

	public List<BattleCardBase> GameCrystallizedPlayCards { get; set; }

	public List<BattleCardBase> GameLeftCards { get; set; }

	public List<TurnAndCard> GameTurnLeftCards { get; set; }

	public List<TurnAndCard> GameReturnedCards { get; set; }

	public List<BattleCardBase> GameSuperSkyboundArtCards { get; set; }

	public List<BattleCardBase> GameInplayMetamorphoseCards { get; set; }

	public List<SkillBase> OkSkillInProcess { get; set; }

	public List<TurnAndCard> TurnDestroyCards { get; set; }

	public List<TurnAndIntValue> TurnWhenHealingCount { get; set; }

	public List<BattleCardBase> GameBurialRiteCards { get; set; }

	public List<BattleCardBase> TurnBurialRiteCards { get; set; }

	public List<int> BurialRiteOrDiscardCardHandIndexList { get; set; }

	public List<TurnAndCard> GameReanimatedCards { get; set; }

	protected List<BattleCardBase> AddToDeckCardList { get; set; }

	public List<TurnAndIntValue> TurnStartLifeList { get; protected set; }

	public int RallyCount { get; protected set; }

	public int DeckBanishCount { get; protected set; }

	public int GameResonanceStartCount { get; set; }

	public int TurnResonanceStartCount { get; set; }

	public int GameUsedWhiteRitualCount { get; set; }

	public int LastInplayWhiteRitualStack { get; set; }

	public List<TurnAndIntValue> GameSkillReturnCardCountList { get; set; }

	public List<TurnAndIntValue> GameSkillDiscardCountList { get; set; }

	public List<TurnAndIntValue> GameSkillBuffCountList { get; set; }

	public List<TurnAndIntValue> GameSkillMetamorphoseCountList { get; set; }

	public int GameSkillDiscardCount { get; set; }

	public List<BattleCardBase> GameQuickAttackCards { get; set; }

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDeckCards => ConvertToSkillInfoCollection(DeckCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoBattleStartDeckCards => ConvertToSkillInfoCollection(BattleStartDeckCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoHandCards => ConvertToSkillInfoCollection(HandCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoClassAndInPlayCards => ConvertToSkillInfoCollection(ClassAndInPlayCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoCemeterys => ConvertToSkillInfoCollection(CemeteryList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoBanishCards => ConvertToSkillInfoCollection(BanishList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoFusionIngredientList => ConvertToSkillInfoCollection(FusionIngredientList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnFusionCards => ConvertToSkillInfoCollection(TurnFusionCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoNecromanceZoneCards => ConvertToSkillInfoCollection(NecromanceZoneList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoInPlayCards => ConvertToSkillInfoCollection(InPlayCards);

	public IEnumerable<IEnumerable<IReadOnlyBattleCardInfo>> SkillInfoLastTargets => ConvertToSkillInfoCollectionList(LastTargetCardsList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDiscards => ConvertToSkillInfoCollection(SkillDiscards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDiscardedCards => ConvertToSkillInfoCollection(DiscardedCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoFusionIngredientAndDiscardedCards => ConvertToSkillInfoCollection(FusionIngredientAndDiscardedCardList);

	public IEnumerable<TurnAndCard> SkillInfoReturnedCards => GameReturnedCards;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoHealingCards => ConvertToSkillInfoCollection(HealingCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoSkillSummonedCards => ConvertToSkillInfoCollection(SkillSummonedCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoEvolvedCards => ConvertToSkillInfoCollection(EvolvedCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoDestroyedWhenDestroyCards => ConvertToSkillInfoCollection(DestroyedWhenDestroyCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnPlayCards => ConvertToSkillInfoCollection(TurnPlayCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnDrawCards => ConvertToSkillInfoCollection(TurnDrawCards);

	public IEnumerable<CardAndId> SkillInfoTurnDrawTokenCardsWithId => TurnDrawTokenCardsWithId;

	public IEnumerable<TurnAndCard> SkillInfoGameSummonCards => GameSummonCards;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGamePlayCards => ConvertToSkillInfoCollection(GamePlayCards);

	public IEnumerable<TurnAndCard> SkillInfoGameTurnPlayCards => GameTurnPlayCards;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameCrystallizedPlayCards => ConvertToSkillInfoCollection(GameCrystallizedPlayCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameSkillActivated => ConvertToSkillInfoCollection(ChoiceBraveCardList);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoInplayMetamorphosedCards => ConvertToSkillInfoCollection(GameInplayMetamorphoseCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameBurialRiteCards => ConvertToSkillInfoCollection(GameBurialRiteCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoTurnBurialRiteCards => ConvertToSkillInfoCollection(TurnBurialRiteCards);

	public IEnumerable<TurnAndCard> SkillInfoGameReanimatedCards => GameReanimatedCards;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameDrawCards => ConvertToSkillInfoCollection(GameDrawCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameDrawTokenCards => ConvertToSkillInfoCollection(GameDrawTokenCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameAddUpdateDeckCards => ConvertToSkillInfoCollection(GameAddUpdateDeckCards);

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameLeftCards => ConvertToSkillInfoCollection(GameLeftCards);

	public IEnumerable<TurnAndCard> SkillInfoGameTurnLeftCards => GameTurnLeftCards;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameSuperSkyboundArtCards => ConvertToSkillInfoCollection(GameSuperSkyboundArtCards);

	public IReadOnlyBattleCardInfo SkillInfoClass => Class;

	public IEnumerable<IReadOnlyBattleCardInfo> SkillInfoGameQuickAttackCards => ConvertToSkillInfoCollection(GameQuickAttackCards);

	public AvatarBattleInfo AvatarBattleInfo { get; set; }

	public int extraTurnCount { get; set; }

	public bool IsExtraTurn => extraTurnCount > 0;

	public int cardTotalNum { get; set; }

	public bool IsShortageDeck { get; private set; }

	public int _cumulativeEvolutionCount { get; protected set; }

	public IEnumerable<BattleCardBase> AllCards
	{
		get
		{
			for (int i = 0; i < HandCardList.Count(); i++)
			{
				yield return HandCardList[i];
			}
			for (int i = 0; i < ClassAndInPlayCardList.Count; i++)
			{
				yield return ClassAndInPlayCardList[i];
			}
			for (int i = 0; i < DeckCardList.Count(); i++)
			{
				yield return DeckCardList[i];
			}
		}
	}

	public List<BattleCardBase> AllCardsWithCemeteryAndBanish
	{
		get
		{
			List<BattleCardBase> list = AllCards.ToList();
			list.AddRange(CemeteryList);
			list.AddRange(NecromanceZoneList);
			list.AddRange(BanishList);
			return list;
		}
	}

	public IEnumerable<BattleCardBase> InPlayCards
	{
		get
		{
			for (int i = 0; i < ClassAndInPlayCardList.Count; i++)
			{
				if (!(ClassAndInPlayCardList[i] is ClassBattleCardBase))
				{
					yield return ClassAndInPlayCardList[i];
				}
			}
		}
	}

	public BattleCardBase Class => _class;

	public event Action OnTurnStartStart;

	public event Func<VfxBase> OnTurnStartFinish;

	public event Action OnTurnEndStart;

	public event Func<SkillProcessor, VfxBase> OnTurnEnd;

	public event Func<VfxBase> OnTurnEndFinish;

	public event Func<SkillProcessor, VfxBase> OnTurnStartBeforeDraw;

	public event Func<VfxBase> OnTurnStartAfterDraw;

	public event Action<BattleCardBase, SkillProcessor, int, bool> OnNecromance;

	public event Action<BattleCardBase> OnPickCard;

	public event Action OnAfterPickCard;

	public event Action OnSetupClassEvent;

	public event Action<BattleCardBase> OnSetupCardEvent;

	public event Func<VfxBase> OnShortageDeck;

	public event Action<BattlePlayerBase, List<BattleCardBase>> OnMulliganStart;

	public event Action<IEnumerable<BattleCardBase>, IEnumerable<int>> OnMulliganEnd;

	public event Action OnClearDeck;

	public event Action<int> OnChangePP;

	public event Action<int, int, bool, BattleCardBase, bool> OnAddPpTotal;

	public event Action<int, bool, BattleCardBase> OnAddPp;

	public event Action<int, bool, BattleCardBase> OnAddBp;

	public event Action<BattleCardBase, int, bool, bool> OnEpModifier;

	public event Action<BattleCardBase, NetworkBattleDefine.NetworkCardPlaceState, bool, SkillBase> OnAddHandCardEvent;

	public event Action<BattleCardBase, CEMETERY_TYPE, bool, SkillBase> OnAddCemeteryEvent;

	public event Action<BattleCardBase, bool, SkillBase> OnAddPlayCardEvent;

	public event Action<BattleCardBase, SkillBase, bool> OnAddBanishEvent;

	public event Action<BattleCardBase, SkillBase> OnAddDeckEvent;

	public event Action<BattleCardBase, SkillBase> OnAddUniteEvent;

	public event Action<BattleCardBase> OnSpellPlayEvent;

	public event Action<BattleCardBase> OnFusion;

	public event Action<BattleCardBase, SkillBase> OnGeton;

	public event Action<List<BattleCardBase>, List<BattleCardBase>, SkillBase> OnGetoff;

	public event Action<List<BattleCardBase>, SkillBase> OnAddBlackHole;

	public event Action<BattleCardBase> OnAfterReturnCardEvent;

	public event Func<SkillProcessor, List<BattleCardBase>, VfxBase> OnAfterSummonCardEvent;

	public event Action OnAddHandCardAfterEvent;

	public event Func<VfxBase> OnAddCemeteryAfterEvent;

	public event Action OnAddPlayCardAfterEvent;

	public event Action<BattleCardBase> OnAddBanishAfterEvent;

	public event Action<BattleCardBase> OnAddUniteAfterEvent;

	public event Action<BattleCardBase> OnLeaveAfterEvent;

	public event Action<BattleCardBase> OnSummonAfterEvent;

	public event Action<BattleCardBase, BattleCardBase> OnMetamorphoseAfterEvent;

	public event Action<int, SkillProcessor, List<BattleCardBase>> OnChangeDeckAfterEvent;

	public event Action<int, int, List<BattleCardBase>, BattlePlayerBase, bool> OnDrawCards;

	public event Action<BattleCardBase, List<BattleCardBase>, List<BattleCardBase>, bool, bool, bool> OnTokenDrawCards;

	public event Action<BattleCardBase, List<BattleCardBase>, bool> OnCreateReservedCards;

	public event Action<BattleCardBase, List<BattleCardBase>, bool, bool, bool> OnUpdateDeck;

	public event Action<int, int, bool> OnIndexChange;

	public event Action<BattleCardBase, List<BattleCardBase>, List<BattleCardBase>, bool, bool, bool, bool, bool> OnSummonTokenCards;

	public event Action<BattleCardBase, List<BattleCardBase>, bool, bool, bool, bool> OnSummonCards;

	public event Action<BattleCardBase, List<BattleCardBase>, List<int>, List<int>, List<bool>, bool, bool, bool> OnCostChange;

	public event Action<List<SkillBase.BuffInfoContainer>, bool, bool> OnRemoveCostChange;

	public event Action<BattleCardBase, List<BattleCardBase>, int, int, int, int, int> OnPowerUp;

	public event Action OnPowerDownStart;

	public event Action<BattleCardBase, List<BattleCardBase>, int, int, int, bool> OnPowerDown;

	public event Action<List<Skill_powerup.PowerUpModifierContainer>> OnDeprivePowerUp;

	public event Action<List<SkillBase.BuffInfoContainer>> OnDeprivePowerDown;

	public event Action<BattleCardBase, List<BattleCardBase>, List<int>> OnSpellCharge;

	public event Action<int> OnDrain;

	public event Action<BattleCardBase> OnSkillDamageStart;

	public event Action<List<BattleCardBase>, List<BattleCardBase>, List<BattleCardBase.DamageResult>> OnDamage;

	public event Action<BattleCardBase, List<BattleCardBase>, List<int>> OnHeal;

	public event Action<List<BattleCardBase>> OnDiscard;

	public event Action OnStartLeaveCard;

	public event Action<BattleCardBase> OnDestroy;

	public event Action<BattleCardBase, bool, bool> OnSkillDestroyOrBanish;

	public event Action<BattleCardBase> OnBanish;

	public event Action<BattleCardBase> OnPlayVoiceOnDeath;

	public event Action<BattleCardBase> OnReturn;

	public event Action OnSkillReturn;

	public event Action<BattleCardBase, List<BattleCardBase>> OnBeforeSkillEvolve;

	public event Action<BattleCardBase> OnEvolveMeWhenAttack;

	public event Action<List<BattleCardBase>> OnAfterSkillEvolve;

	public event Action<BattleCardBase, BattleCardBase, bool> OnPlayCard;

	public event Action<SkillCollectionBase.WhenPlayEffectType, BattleCardBase, bool> OnWhenPlayEffect;

	public event Action<BattleCardBase, List<BattleCardBase>, int> OnChantCountChange;

	public event Action<BattleCardBase, int, bool> OnChangeWhiteRitualStack;

	public event Action<BattleCardBase, List<BattleCardBase>, int> OnChangeMaxAttackableCount;

	public event Action<BattleCardBase, List<BattleCardBase>, int> OnMetamorphose;

	public event Action<int> OnFusionMetamorphose;

	public event Action<BattleCardBase> OnOpenCard;

	public event Action<BattleCardBase, List<BattleCardBase>, BattleCardBase> OnUnite;

	public event Action<NetworkBattleReceiver.ReplayOperationType> OnRemoveLatestOperationJsonData;

	public event Action OnPlayComplete;

	public event Action<bool> OnClearDestroyedCardList;

	public void SetCurrentEpCount(int setCount)
	{
		CurrentEpCount = setCount;
	}

	public void AddCurrentEpCount(int addCount = 1)
	{
		CurrentEpCount += addCount;
	}

	public void GainCurrentEpCount(int gainCount = 1)
	{
		CurrentEpCount -= gainCount;
	}

	public VfxBase SetBp(int value)
	{
		Bp = value;
		Bp = Math.Max(0, Bp);
		Bp = Math.Min(Bp, 99);
		return BattleView.SetBp(value);
	}

	public VfxBase AddBp(int value)
	{
		Bp += value;
		Bp = Math.Max(0, Bp);
		Bp = Math.Min(Bp, 99);
		return BattleView.SetBp(Bp);
	}

	public void SetIsShortageDeckLose(bool flag)
	{
		IsShortageDeckLose = flag;
	}

	public void ResetIsShortageDeck()
	{
		IsShortageDeck = false;
	}

	public int AddDamageByClassUseCard(string damageType)
	{
		if (Class != null)
		{
			int num = 0;
			for (int i = 0; i < Class.SkillApplyInformation.AddDamageList.Count; i++)
			{
				if (Class.SkillApplyInformation.AddDamageList[i] is AddDamageInfo addDamageInfo && addDamageInfo.IsEffective(damageType, Class.Clan, isUseClass: true))
				{
					num += addDamageInfo.AddDamage;
				}
			}
			return num;
		}
		return 0;
	}

	protected BattlePlayerBase(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround, IInnerOptionsBuilder innerOptionsBuilder)
	{
		BattleMgr = battleMgr;
		_dataMgr = BattleMgr.GameMgr.GetDataMgr();
		BattleCamera = battleCamera;
		BackGround = backGround;
		_innerOptionsBuilder = innerOptionsBuilder;
		Initialize();
		HandCardList = new List<BattleCardBase>();
		DeckCardList = new List<BattleCardBase>();
		BattleStartDeckCardList = new List<BattleCardBase>();
		DeckSkillCardList = new List<BattleCardBase>();
		ClassAndInPlayCardList = new List<BattleCardBase>();
		CemeteryList = new List<BattleCardBase>();
		PredictionCemeteryRandomCards = new List<BattleCardBase>();
		PredictionDamageRandomCards = new List<BattleCardBase>();
		PredictionBanishRandomCards = new List<BattleCardBase>();
		BanishList = new List<BattleCardBase>();
		FusionIngredientList = new List<BattleCardBase>();
		TurnFusionCards = new List<BattleCardBase>();
		NecromanceZoneList = new List<BattleCardBase>();
		DiscardedCardList = new List<BattleCardBase>();
		FusionIngredientAndDiscardedCardList = new List<BattleCardBase>();
		ReservedCardList = new List<BattleCardBase>();
		UniteList = new List<BattleCardBase>();
		GetOnList = new List<BattleCardBase>();
		BlackHole = new List<BattleCardBase>();
		ChoiceBraveCardList = new List<BattleCardBase>();
		ReturnList = new List<BattleCardBase>();
		HealingCards = new List<BattleCardBase>();
		LastTargetCardsList = new List<List<BattleCardBase>>();
		SkillSummonedCards = new List<BattleCardBase>();
		SummonedCards = new List<BattleCardBase>();
		EvolvedCards = new List<BattleCardBase>();
		DestroyedWhenDestroyCards = new List<BattleCardBase>();
		InHandCards = new List<BattleCardBase>();
		SkillDiscards = new List<BattleCardBase>();
		SkillBanishCards = new List<BattleCardBase>();
		TurnPlayCards = new List<BattleCardBase>();
		TurnDrawCards = new List<BattleCardBase>();
		TurnDrawTokenCardsWithId = new List<CardAndId>();
		GameSummonCards = new List<TurnAndCard>();
		GameSummonMomentTribe = new List<CardAndTribe>();
		GamePlayMomentTribe = new List<CardAndTribe>();
		GamePlayMomentSpellChargeCards = new List<BattleCardBase>();
		GameUpdateDeckMomentTribe = new List<CardAndTribe>();
		GamePlayCards = new List<BattleCardBase>();
		GameTurnPlayCards = new List<TurnAndCard>();
		GameEnhancePlayCards = new List<TurnAndCard>();
		GameCrystallizedPlayCards = new List<BattleCardBase>();
		GameInplayMetamorphoseCards = new List<BattleCardBase>();
		GameBurialRiteCards = new List<BattleCardBase>();
		GameQuickAttackCards = new List<BattleCardBase>();
		TurnBurialRiteCards = new List<BattleCardBase>();
		BurialRiteOrDiscardCardHandIndexList = new List<int>();
		GameReanimatedCards = new List<TurnAndCard>();
		OkSkillInProcess = new List<SkillBase>();
		TurnDestroyCards = new List<TurnAndCard>();
		AddToDeckCardList = new List<BattleCardBase>();
		GameDrawCards = new List<BattleCardBase>();
		GameDrawTokenCards = new List<BattleCardBase>();
		GameAddUpdateDeckCards = new List<BattleCardBase>();
		TurnStartLifeList = new List<TurnAndIntValue>();
		TurnWhenHealingCount = new List<TurnAndIntValue>();
		TurnPlayCardCountInfo = new List<TurnAndIntValue>();
		TurnFusionCountInfo = new List<TurnAndIntValue>();
		TurnEvolveCardCountInfo = new List<TurnAndIntValue>();
		GameLeftCards = new List<BattleCardBase>();
		GameTurnLeftCards = new List<TurnAndCard>();
		GameReturnedCards = new List<TurnAndCard>();
		GameSuperSkyboundArtCards = new List<BattleCardBase>();
		GameSkillReturnCardCountList = new List<TurnAndIntValue>();
		GameSkillDiscardCountList = new List<TurnAndIntValue>();
		GameSkillBuffCountList = new List<TurnAndIntValue>();
		GameSkillMetamorphoseCountList = new List<TurnAndIntValue>();
		BonusConditionList = new List<MyRotationBonusCondition>();
		BossRushSpecialSkillList = new List<BossRushSpecialSkill>();
		ChoiceBraveSkillDescInfoList = new List<AvatarBattleDescInfo>();
		NowTurnEvol = true;
		m_vfxCreator = CreateVfxCreator();
		CreateSelfBattleCard();
		_class = ClassAndInPlayCardList[0];
	}

	protected abstract void Initialize();

	protected abstract void CreateSelfBattleCard();

	protected virtual IBattlePlayerVfxCreator CreateVfxCreator()
	{
		return new BattlePlayerVfxCreatorBase(BattleView);
	}

	public virtual void Setup(BattlePlayerBase opponentBattlePlayer)
	{
		IsShortageDeckLose = false;
		extraTurnCount = 0;
		_cumulativeEvolutionCount = 0;
		_opponentBattlePlayer = opponentBattlePlayer;
		_opponentBattlePlayer.Class.ChangeClassClanParameter();
		GameMgr ins = BattleMgr.GameMgr;
		List<IClassInfomationUI> list = new List<IClassInfomationUI>();
		BattleManagerBase ins2 = BattleMgr;
		if (IsPlayer)
		{
			int key = (IsPlayer ? ins.GetNetworkUserInfoData().GetSelfChaosId() : ins.GetNetworkUserInfoData().GetOpponentChaosId());
			if (Data.Master.ClassInfomationOrder != null && Data.Master.ClassInfomationOrder.ContainsKey(key))
			{
				List<int> value = new List<int>();
				int num = 1;
				Data.Master.ClassInfomationOrder.TryGetValue(key, out value);
				for (int i = 0; i < value.Count; i++)
				{
					list.Add(CreateClassInfomationUI(num, value.Count, value[i]));
					num++;
				}
			}
			else if (ins.GetDataMgr().GetPlayerSubClassId() != 10)
			{
				int num2 = 1;
				List<int> crossOverClassInfoListOrNull = Data.Master.GetCrossOverClassInfoListOrNull(ins.GetDataMgr().GetPlayerClassId(), ins.GetDataMgr().GetPlayerSubClassId());
				if (crossOverClassInfoListOrNull == null || crossOverClassInfoListOrNull.Count <= 0)
				{
					list.Add(CreateClassInfomationUI());
				}
				else
				{
					for (int j = 0; j < crossOverClassInfoListOrNull.Count; j++)
					{
						list.Add(CreateClassInfomationUI(num2, crossOverClassInfoListOrNull.Count, crossOverClassInfoListOrNull[j]));
						num2++;
					}
				}
			}
			else if (Data.CurrentFormat == Format.Avatar)
			{
				int key2 = ((ins2 is NetworkBattleManagerBase) ? int.Parse(ins.GetNetworkUserInfoData().GetSelfAvatarBattleId()) : ins.GetDataMgr().GetPlayerCharaId());
				List<int> value2 = new List<int>();
				Data.Master.AvatarClassInformationOrder.TryGetValue(key2, out value2);
				for (int k = 0; k < value2.Count; k++)
				{
					list.Add(CreateClassInfomationUI(k + 1, value2.Count, value2[k]));
				}
			}
			else if (ins2.IsPuzzleMgr)
			{
				int playerClass = (ins2 as PuzzleBattleManager).PuzzleQuestData.BattleData.PlayerClass;
				list.Add(CreateClassInfomationUI(1, 1, playerClass));
			}
			else
			{
				list.Add(CreateClassInfomationUI());
			}
		}
		else if (ins.GetDataMgr().GetEnemySubClassId() != 10)
		{
			int num3 = 1;
			List<int> crossOverClassInfoListOrNull2 = Data.Master.GetCrossOverClassInfoListOrNull(ins.GetDataMgr().GetEnemyClassId(), ins.GetDataMgr().GetEnemySubClassId());
			if (crossOverClassInfoListOrNull2 == null || crossOverClassInfoListOrNull2.Count <= 0)
			{
				list.Add(CreateClassInfomationUI());
			}
			else
			{
				for (int l = 0; l < crossOverClassInfoListOrNull2.Count; l++)
				{
					list.Add(CreateClassInfomationUI(num3, crossOverClassInfoListOrNull2.Count, crossOverClassInfoListOrNull2[l]));
					num3++;
				}
			}
		}
		else if (Data.CurrentFormat == Format.Avatar && ins2 is NetworkBattleManagerBase)
		{
			int key3 = int.Parse(ins.GetNetworkUserInfoData().GetOpponentAvatarBattleId());
			List<int> value3 = new List<int>();
			Data.Master.AvatarClassInformationOrder.TryGetValue(key3, out value3);
			for (int m = 0; m < value3.Count; m++)
			{
				list.Add(CreateClassInfomationUI(m + 1, value3.Count, value3[m]));
			}
		}
		else
		{
			list.Add(CreateClassInfomationUI());
		}
		ClassInformationUIController = new ClassInformationUIController(list);
		ClassInformationUIController.SetUpEvent(this);
		SetUpClassEvent();
		foreach (BattleCardBase deckCard in DeckCardList)
		{
			SetupCardEvent(deckCard);
		}
		foreach (BattleCardBase handCard in HandCardList)
		{
			SetupCardEvent(handCard);
		}
		foreach (BattleCardBase classAndInPlayCard in ClassAndInPlayCardList)
		{
			SetupCardEvent(classAndInPlayCard);
		}
		OnNecromance += delegate(BattleCardBase necromanceCard, SkillProcessor skillProcessor, int necromanceCount, bool isFusion)
		{
			List<BattleCardBase> list2 = ClassAndInPlayCardList.ToList();
			list2.AddRange(DeckSkillCardList);
			foreach (BattleCardBase item in list2)
			{
				item.Necromance(necromanceCard, skillProcessor, necromanceCount);
			}
			if (BattleMgr is NetworkBattleManagerBase networkBattleManagerBase)
			{
				networkBattleManagerBase.RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.cemetery, -1 * necromanceCount, necromanceCard.IsPlayer));
			}
		};
		Emotion.OnPlay += (ClassCharaPrm.EmotionType emoteType) => opponentBattlePlayer.Emotion.ReceiveOpponentEmotion(emoteType);
		OnNecromance += delegate
		{
			BattleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				StatusPanelControl.SetGrave(CemeteryList.Count((BattleCardBase c) => !c.IsClass));
				UpdateHandCardsPlayability();
			}));
		};
		OnAddPlayCardAfterEvent += delegate
		{
			UpdateStatusPanelHandCount();
		};
		OnAddHandCardAfterEvent += delegate
		{
			UpdateStatusPanelHandCount();
		};
		OnAddCemeteryAfterEvent += delegate
		{
			UpdateStatusPanelHandCount();
			return NullVfx.GetInstance();
		};
		OnAddCemeteryAfterEvent += delegate
		{
			BattleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				if (StatusPanelControl != null)
				{
					StatusPanelControl.SetGrave(CemeteryList.Count((BattleCardBase c) => !c.IsClass));
				}
			}));
			return InstantVfx.Create(delegate
			{
				UpdateHandCardsPlayability();
			});
		};
		OnAddBanishAfterEvent += delegate
		{
			UpdateHandCardsPlayability();
			UpdateStatusPanelHandCount();
		};
		OnAddUniteAfterEvent += delegate
		{
			UpdateHandCardsPlayability();
			UpdateStatusPanelHandCount();
		};
		OnChangeDeckAfterEvent += delegate(int previousCount, SkillProcessor skillProcessor, List<BattleCardBase> summonCards)
		{
			if (previousCount % 2 == 1 && DeckCardList.Count % 2 == 0)
			{
				int gameResonanceStartCount = GameResonanceStartCount;
				GameResonanceStartCount = gameResonanceStartCount + 1;
				gameResonanceStartCount = TurnResonanceStartCount;
				TurnResonanceStartCount = gameResonanceStartCount + 1;
				StartSkillWhenResonanceStart(skillProcessor, summonCards);
			}
		};
		OnChangeDeckAfterEvent += delegate
		{
			DeckSkillCardList.RemoveAll((BattleCardBase c) => !c.IsInDeck);
		};
		OnMulliganEnd += delegate
		{
			DeckSkillCardList.RemoveAll((BattleCardBase c) => !c.IsInDeck);
		};
		OnTurnStartComplete = (Action)Delegate.Combine(OnTurnStartComplete, new Action(AddToDeckCardIndexChange));
		OnTurnStartComplete = (Action)Delegate.Combine(OnTurnStartComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent));
		OnPostTurnEndComplete = (Action)Delegate.Combine(OnPostTurnEndComplete, new Action(AddToDeckCardIndexChange));
		OnPreTurnEndComplete = (Action)Delegate.Combine(OnPreTurnEndComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent));
	}

	private void UpdateStatusPanelHandCount()
	{
		if (StatusPanelControl != null && HandCardList != null)
		{
			StatusPanelControl.SetHandCount(HandCardList.Count);
		}
	}

	public virtual IClassInfomationUI CreateClassInfomationUI(int orderCount = 1, int totalInfoNum = 1, int clanId = -1)
	{
		return null;
	}

	public abstract void SetupClone(BattlePlayerBase sourceBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer, CloneActualFlags cloneFlags);

	public void SetupActionProcessorEvent(ActionProcessor processor)
	{
		processor.OnBeforePlayCard += delegate(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCards)
		{
			if (!card.IsChoiceBraveSkillCard)
			{
				AddCurrentTrunPlayCount(1);
				CardOnPlay = card;
				TurnPlayCards.Add(card);
				GamePlayCards.Add(card);
				GameTurnPlayCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, card, BattleMgr.IsTurnEnd));
				GamePlayMomentTribe.Add(new CardAndTribe(card, card.Tribe));
				if (card.HasSpellCharge)
				{
					GamePlayMomentSpellChargeCards.Add(card);
				}
				if (card.CheckConditionFixedUseCost(isPrePlay: true))
				{
					GameEnhancePlayCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, card, BattleMgr.IsTurnEnd));
				}
				if (card.TransformInfo.Type == BattleCardBase.TransformType.Crystallize)
				{
					GameCrystallizedPlayCards.Add(card);
				}
				if (!BattleMgr.IsVirtualBattle)
				{
					BattleLogManager.GetInstance().AddLogDestFollower(BattleLogWindow.BattleLogType.PlayCardLog, card);
				}
				BattleView.HandView.RemoveCardFromView(card.BattleCardView, 0.3f);
			}
		};
		processor.OnAfterFusion += delegate
		{
			AddToDeckCardIndexChange();
			return NullVfx.GetInstance();
		};
		processor.OnPlayComplete = (Action)Delegate.Combine(processor.OnPlayComplete, new Action(AddToDeckCardIndexChange));
		processor.OnPlayComplete = (Action)Delegate.Combine(processor.OnPlayComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent));
		processor.OnPlayComplete = (Action)Delegate.Combine(processor.OnPlayComplete, this.OnPlayComplete);
		processor.OnPlayComplete = (Action)Delegate.Combine(processor.OnPlayComplete, (Action)delegate
		{
			CardOnPlay = null;
		});
		processor.OnEvolutionComplete = (Action)Delegate.Combine(processor.OnEvolutionComplete, new Action(AddToDeckCardIndexChange));
		processor.OnEvolutionComplete = (Action)Delegate.Combine(processor.OnEvolutionComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvolutionEvent));
		processor.OnAttackComplete = (Action)Delegate.Combine(processor.OnAttackComplete, new Action(AddToDeckCardIndexChange));
		processor.OnAttackComplete = (Action)Delegate.Combine(processor.OnAttackComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent));
		if (BattleMgr.GameMgr.IsWatchBattle && !BattleMgr.GameMgr.IsReplayBattle)
		{
			processor.OnFusionComplete = (Action)Delegate.Combine(processor.OnFusionComplete, new Action(BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent));
		}
	}

	private void SetUpClassEvent()
	{
		this.OnSetupClassEvent.Call();
	}

	public virtual void SetupCardEvent(BattleCardBase card)
	{
		if (card.IsUnit)
		{
			card.OnEvolveEvent += delegate(bool isSkill)
			{
				EvolveProcess(card, isSkill);
			};
		}
		this.OnSetupCardEvent.Call(card);
		if (card.IsSpell)
		{
			card.OnPlay += () => RemoveSpellCardFromHand(card);
			card.OnFinishWhenPlaySkill += () => AddSpellCardToCemetery(card);
		}
		card.OnRemoveFromInPlayAfterOneTime += (bool flg, SkillProcessor skillProcessorOneTime) => flg ? NullVfx.GetInstance() : card.SkillApplyInformation.AllSkillEffectStop();
	}

	public virtual BattleCardBase CreateCard(int cardId, int cardIndex, bool isChoiceBrave = false)
	{
		BattleCardBase battleCardBase = CardCreatorBase.CreateCard(cardId, IsPlayer, cardIndex, BattleMgr.SBattleLoad, BattleMgr, BattleMgr.BattleResourceMgr, _innerOptionsBuilder, isChoiceBrave);
		SetupCardEvent(battleCardBase);
		return battleCardBase;
	}

	public BattleCardBase CreateVirtualCard(int cardId, int cardIndex)
	{
		BattleCardBase battleCardBase = CardCreatorBase.CreateVirtualCard(cardId, cardIndex, IsPlayer, BattleMgr, this, _opponentBattlePlayer, _innerOptionsBuilder);
		SetupCardEvent(battleCardBase);
		return battleCardBase;
	}

	public BattleCardBase CreateNextIndexCard(int cardId, bool isChoiceBrave = false)
	{
		BattleCardBase result = CreateCard(cardId, cardTotalNum, isChoiceBrave);
		if (!isChoiceBrave)
		{
			cardTotalNum++;
		}
		return result;
	}

	public VfxBase CardManagement(BattleCardBase card, SkillProcessor skillProcessor, CARD_MANAGEMENT management, bool isRandom, List<BattleCardBase> fusionCards = null, BattleCardBase vehicleCard = null, SkillBase skill = null, SummonInfo summonInfo = null, bool isOpen = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		switch (management)
		{
		case CARD_MANAGEMENT.DESTROY:
			if (card.SkillApplyInformation.IsBanishByDestroy && card.SkillApplyInformation.IsDestroyByBanish)
			{
				sequentialVfxPlayer.Register(DestroyManagement(card, skillProcessor, isRandom, skill));
			}
			else if (!card.SkillApplyInformation.IsBanishByDestroy && card.SkillApplyInformation.IsDestroyByBanish)
			{
				if (card.SkillApplyInformation.IsIndestructible && card.IsDestroyedBySkill)
				{
					card.ResetFlagCardAsDestroyed();
				}
				else
				{
					sequentialVfxPlayer.Register(BanishManagement(card, skillProcessor, skill, isReturn: false, isRandom, isOpen));
				}
			}
			else
			{
				sequentialVfxPlayer.Register(DestroyManagement(card, skillProcessor, isRandom, skill));
			}
			break;
		case CARD_MANAGEMENT.BANISH:
			if (card.SkillApplyInformation.IsBanishByDestroy && card.SkillApplyInformation.IsDestroyByBanish)
			{
				sequentialVfxPlayer.Register(DestroyManagement(card, skillProcessor, isRandom, skill));
			}
			else if (card.SkillApplyInformation.IsBanishByDestroy && !card.SkillApplyInformation.IsDestroyByBanish)
			{
				sequentialVfxPlayer.Register(DestroyManagement(card, skillProcessor, isRandom, skill));
			}
			else
			{
				sequentialVfxPlayer.Register(BanishManagement(card, skillProcessor, skill, isReturn: false, isRandom, isOpen));
			}
			break;
		case CARD_MANAGEMENT.RETURN:
			GameReturnedCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, card, BattleMgr.IsTurnEnd));
			card.SetReturnedSkill(skill);
			if (card.SkillApplyInformation.IsReturnByBanish && card.SkillApplyInformation.IsBanishByDestroy)
			{
				if (card.SkillApplyInformation.IsIndestructible)
				{
					sequentialVfxPlayer.Register(ReturnCardManagement(card, skillProcessor, skill));
					break;
				}
				card.FlagCardAsDestroyedBySkill();
				sequentialVfxPlayer.Register(DestroyManagement(card, skillProcessor, isRandom, skill));
			}
			else if (card.SkillApplyInformation.IsReturnByBanish)
			{
				sequentialVfxPlayer.Register(BanishManagement(card, skillProcessor, skill, isReturn: true, isRandom, isOpen));
			}
			else
			{
				sequentialVfxPlayer.Register(ReturnCardManagement(card, skillProcessor, skill));
			}
			break;
		case CARD_MANAGEMENT.FUSION_MATERIAL:
			sequentialVfxPlayer.Register(Fusion(card, fusionCards, skillProcessor));
			break;
		case CARD_MANAGEMENT.GETON:
			sequentialVfxPlayer.Register(GetOnCardManagement(card, skillProcessor, vehicleCard, skill));
			break;
		case CARD_MANAGEMENT.GETOFF:
			return GetOffCardManagement(summonInfo, skillProcessor, vehicleCard, skill);
		case CARD_MANAGEMENT.SUMMON:
			return SummonCardManagement(summonInfo, skillProcessor, skill);
		}
		return sequentialVfxPlayer;
	}

	public void CallOnSummonTokenCards(BattleCardBase card, List<BattleCardBase> summonCards, List<BattleCardBase> overflowCards, bool isSelf, bool isOwnerEffect, bool isIgnoreVoice, bool isRandomVoice, bool isEvoVoice)
	{
		this.OnSummonTokenCards.Call(card, summonCards, overflowCards, isSelf, isOwnerEffect, isIgnoreVoice, isRandomVoice, isEvoVoice);
	}

	public void CallOnSummonCards(BattleCardBase card, List<BattleCardBase> cards, bool isSelf, bool isDeckSelf, bool isIgnoreVoice, bool isBurialRite = false)
	{
		this.OnSummonCards.Call(card, cards, isSelf, isDeckSelf, isIgnoreVoice, isBurialRite);
	}

	protected VfxBase Fusion(BattleCardBase fusionCard, List<BattleCardBase> ingredientCards, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (!fusionCard.IsFusionable)
		{
			return sequentialVfxPlayer;
		}
		if (!BattleMgr.GameMgr.IsAdminWatch && !BattleMgr.GameMgr.IsReplayBattle)
		{
			BattleLogManager.GetInstance().AddFusionIngredients(fusionCard, isCreateClone: false);
		}
		if (!BattleMgr.IsVirtualBattle && !BattleMgr.IsRecovery)
		{
			sequentialVfxPlayer.Register(BattleView.CreateBeforeFusionVfx(fusionCard, ingredientCards));
		}
		new SkillConditionCheckerOption();
		SkillProcessor.ProcessInfo processInfo = fusionCard.Skills.CreateWhenFusionMetamorphoseInfo(ingredientCards, skillProcessor, new BattlePlayerPair(fusionCard.SelfBattlePlayer, fusionCard.OpponentBattlePlayer));
		skillProcessor.Register(processInfo);
		bool flag = processInfo != null;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase ingredientCard in ingredientCards)
		{
			parallelVfxPlayer.Register(UseFusionIngredientManagement(ingredientCard, fusionCard, skillProcessor, isRandom: false, flag));
		}
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		TurnFusionCards.Add(fusionCard);
		AddCurrentTurnFusionCount(1);
		if (BattleMgr.GameMgr.IsAdminWatch || BattleMgr.GameMgr.IsReplayBattle)
		{
			BattleLogManager.GetInstance().AddFusionIngredients(fusionCard, isCreateClone: true);
		}
		BattleCardBase originalCard = fusionCard;
		fusionCard.CallOnFusionEvent(ingredientCards);
		sequentialVfxPlayer.Register(skillProcessor.Process(new BattlePlayerPair(this, _opponentBattlePlayer)));
		if (flag)
		{
			fusionCard = fusionCard.MetamorphoseCard;
		}
		sequentialVfxPlayer.Register(BattleView.ReturnActCardAfterFusion(fusionCard.BattleCardView, flag));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
		}));
		VfxBase vfx = fusionCard.Fusion(skillProcessor, ingredientCards, flag);
		sequentialVfxPlayer.Register(vfx);
		VfxBase vfx2 = skillProcessor.Process(new BattlePlayerPair(this, _opponentBattlePlayer));
		sequentialVfxPlayer.Register(vfx2);
		return sequentialVfxPlayer;
	}

	public VfxBase CardManagement(List<BattleCardBase> cards, SkillProcessor skillProcessor, CARD_MANAGEMENT management, bool isRandom, SkillBase skill = null)
	{
		if (!BattleMgr.IsRecovery)
		{
			for (int i = 0; i < cards.Count; i++)
			{
				BurialRiteOrDiscardCardHandIndexList.Add(HandCardList.IndexOf(cards[i]));
			}
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (management == CARD_MANAGEMENT.DESTROY)
		{
			sequentialVfxPlayer.Register(DestroyManagement(cards, skillProcessor, isRandom, skill));
		}
		return sequentialVfxPlayer;
	}

	protected VfxBase DestroyManagement(BattleCardBase card, SkillProcessor skillProcessor, bool isRandom, SkillBase destroyedSkill = null)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool num = ClassAndInPlayCardList.Contains(card);
		bool flag = HandCardList.Contains(card);
		if (num)
		{
			this.OnStartLeaveCard.Call();
			sequentialVfxPlayer.Register(DestroyCard(card, skillProcessor, isRandom, destroyedSkill));
			sequentialVfxPlayer.Register(card.DestroyInPlay(skillProcessor, useDestroy: true, destroyedSkill));
			this.OnDestroy.Call(card);
		}
		else if (flag)
		{
			sequentialVfxPlayer.Register(DestroyCard(card, skillProcessor, isRandom, destroyedSkill));
			sequentialVfxPlayer.Register(DisCard(card, new List<BattleCardBase> { card }, skillProcessor, destroyedSkill));
		}
		return sequentialVfxPlayer;
	}

	protected VfxBase DestroyManagement(List<BattleCardBase> cards, SkillProcessor skillProcessor, bool isRandom, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (HandCardList.Contains(cards.First()))
		{
			for (int i = 0; i < cards.Count; i++)
			{
				sequentialVfxPlayer.Register(DestroyCard(cards[i], skillProcessor, isRandom, skill));
			}
			sequentialVfxPlayer.Register(DisCards(cards, skillProcessor, skill));
		}
		return sequentialVfxPlayer;
	}

	protected VfxBase BanishManagement(BattleCardBase card, SkillProcessor skillProcessor, SkillBase skill, bool isReturn = false, bool isRandom = false, bool isOpen = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool num = InPlayCards.Contains(card);
		bool flag = HandCardList.Contains(card);
		bool flag2 = DeckCardList.Contains(card);
		this.OnStartLeaveCard.Call();
		if (num)
		{
			card.SetBanishedInfo(BattleCardBase.BanishInfo.BanishPlace.Field);
			sequentialVfxPlayer.Register(card.SkillApplyInformation.AllSkillEffectStop());
			sequentialVfxPlayer.Register(BanishCard(card, skillProcessor, isRandom, skill, isOpen));
			sequentialVfxPlayer.Register(card.Banish(skillProcessor, isReturn));
		}
		else if (flag)
		{
			card.SetBanishedInfo(BattleCardBase.BanishInfo.BanishPlace.Hand);
			sequentialVfxPlayer.Register(BanishCard(card, skillProcessor, isRandom, skill, isOpen));
			VfxWithLoading vfxWithLoading = card.BanishInHand(skillProcessor);
			sequentialVfxPlayer.Register(vfxWithLoading.LoadingVfx);
			sequentialVfxPlayer.Register(vfxWithLoading.MainVfx);
		}
		else if (flag2)
		{
			card.SetBanishedInfo(BattleCardBase.BanishInfo.BanishPlace.Deck);
			sequentialVfxPlayer.Register(BanishCard(card, skillProcessor, isRandom, skill, isOpen));
			sequentialVfxPlayer.Register(card.BanishInDeck(skillProcessor));
		}
		this.OnBanish.Call(card);
		return sequentialVfxPlayer;
	}

	public VfxBase UseFusionIngredientManagement(BattleCardBase ingredientCard, BattleCardBase fusionCard, SkillProcessor skillProcessor, bool isRandom = false, bool isFusionMetamorphose = false)
	{
		if (FusionIngredientList.Any((BattleCardBase c) => c == fusionCard))
		{
			return NullVfx.GetInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxWithLoading vfxWithLoading = ingredientCard.FusionMaterialized(skillProcessor, fusionCard, isFusionMetamorphose);
		this.OnFusion.Call(ingredientCard);
		HandCardList.Remove(ingredientCard);
		CallSkill((IBattlePlayerSkill s) => s.StopBattleHandCard, ingredientCard);
		FusionIngredientList.Add(ingredientCard);
		FusionIngredientAndDiscardedCardList.Add(ingredientCard);
		if (isRandom)
		{
			PredictionBanishRandomCards.Add(ingredientCard);
		}
		this.OnAddBanishAfterEvent.Call(fusionCard);
		sequentialVfxPlayer.Register(ingredientCard.UnloadResource());
		sequentialVfxPlayer.Register(vfxWithLoading.LoadingVfx);
		sequentialVfxPlayer.Register(vfxWithLoading.MainVfx);
		return sequentialVfxPlayer;
	}

	protected VfxBase ReturnCardManagement(BattleCardBase card, SkillProcessor skillProcessor, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		this.OnStartLeaveCard.Call();
		sequentialVfxPlayer.Register(ReturnCard(card, skillProcessor, skill));
		this.OnReturn.Call(card);
		return sequentialVfxPlayer;
	}

	protected VfxBase GetOnCardManagement(BattleCardBase getOnCard, SkillProcessor skillProcessor, BattleCardBase vehicleCard, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		vehicleCard.SkillApplyInformation.AddGetOnCard(getOnCard);
		getOnCard.DeathTypeInfo.LeaveByGetOn = true;
		sequentialVfxPlayer.Register(getOnCard.RemoveFromInPlay());
		sequentialVfxPlayer.Register(CardToVehicleZone(getOnCard, skill));
		SkillProcessor.ProcessInfo info = vehicleCard.Skills.CreateWhenGetOnInfo(skillProcessor, playerInfoPair);
		sequentialVfxPlayer.Register(getOnCard.RemoveFromInPlayAfter(skillProcessor));
		sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(null, null, skillProcessor));
		skillProcessor.Register(info);
		sequentialVfxPlayer.Register(getOnCard.GetOn(vehicleCard.BattleCardView.Transform, vehicleCard.BattleCardView, skillProcessor));
		return sequentialVfxPlayer;
	}

	protected VfxBase GetOffCardManagement(SummonInfo summonInfo, SkillProcessor skillProcessor, BattleCardBase vehicleCard, SkillBase skill)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		foreach (BattleCardBase summonedCard in summonInfo.SummonedCardsList.summonedCards)
		{
			vfxWithLoadingSequential.RegisterVfxWithLoading(summonedCard.SkillPlayCard(IsPlayer, SkillBaseSummon.SUMMON_TYPE.TOKEN, skillProcessor, skill, isGetoff: true));
		}
		vfxWithLoadingSequential.RegisterToLoadingVfx(BattleMgr.LoadCardResources(summonInfo.SummonedCardsList.summonedCards.ToList()));
		vfxWithLoadingSequential.RegisterToLoadingVfx(BattleMgr.LoadCardResources(summonInfo.SummonedCardsList.overflowCards.ToList()));
		this.OnGetoff.Call(summonInfo.SummonedCardsList.summonedCards.ToList(), summonInfo.SummonedCardsList.overflowCards.ToList(), skill);
		vfxWithLoadingSequential.RegisterToMainVfx(StartSkillWhenChangeInplay(null, summonInfo.SummonedCardsList.summonedCards.ToList(), skillProcessor, isSummonCheck: false));
		vehicleCard.GetOffCards.AddRange(summonInfo.SummonedCardsList.summonedCards);
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		skillProcessor.Register(vehicleCard.Skills.CreateWhenGetOffInfo(skillProcessor, playerInfoPair), ignoreOwnerDeadCheck: true);
		UpdateHandCardsPlayability();
		return vfxWithLoadingSequential;
	}

	protected VfxBase SummonCardManagement(SummonInfo summonInfo, SkillProcessor skillProcessor, SkillBase skill)
	{
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		if (summonInfo.IsReanimate)
		{
			skillConditionCheckerOption.ReanimatedCards = ConvertToSkillInfoCollection(summonInfo.SummonedCardsList);
		}
		if (summonInfo.IsDeckSelfSummon)
		{
			skillConditionCheckerOption.DeckSelfSummonedCards = ConvertToSkillInfoCollection(summonInfo.SummonedCardsList);
		}
		foreach (BattleCardBase summonedCard in summonInfo.SummonedCardsList.summonedCards)
		{
			vfxWithLoadingSequential.RegisterVfxWithLoading(summonedCard.SkillPlayCard(summonInfo.IsPlayer, summonInfo.SummonType, skillProcessor, skill, isGetoff: false, summonInfo.IsReanimate));
		}
		vfxWithLoadingSequential.RegisterToLoadingVfx(BattleMgr.LoadCardResources(summonInfo.SummonedCardsList.summonedCards.ToList()));
		SkillSummonedCards = new List<BattleCardBase>(summonInfo.SummonedCardsList.summonedCards);
		SummonedCards = new List<BattleCardBase>(summonInfo.SummonedCardsList.ToList());
		vfxWithLoadingSequential.RegisterToLoadingVfx(BattleMgr.LoadCardResources(summonInfo.SummonedCardsList.overflowCards.ToList()));
		vfxWithLoadingSequential.RegisterToMainVfx(StartSkillWhenChangeInplay(null, summonInfo.SummonedCardsList.summonedCards.ToList(), skillProcessor, isSummonCheck: true, null, skillConditionCheckerOption));
		for (int i = 0; i < SkillSummonedCards.Count; i++)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			for (int j = i + 1; j < SkillSummonedCards.Count; j++)
			{
				list.Add(SkillSummonedCards[j]);
			}
			StartSkillWhenSummonOther(SkillSummonedCards[i], skillProcessor, summonInfo.IsReanimate, list);
		}
		BattleCardBase[] array = summonInfo.SummonedCardsList.Where((BattleCardBase c) => c.IsInplay && c.IsUnit).ToArray();
		for (int num = 0; num < array.Length; num++)
		{
			array[num].SelfBattlePlayer.AddRallyCount(1);
		}
		UpdateHandCardsPlayability();
		return vfxWithLoadingSequential;
	}

	protected VfxBase DestroyCard(BattleCardBase destroyCard, SkillProcessor skillProcessor, bool isRandom, SkillBase skill)
	{
		bool flag = destroyCard.IsChantField && destroyCard.ChantCount <= 0;
		if (destroyCard.SkillApplyInformation.IsIndestructible && destroyCard.IsDestroyedBySkill && !flag && !destroyCard.DeathTypeInfo.MysteriesDestroy && (!destroyCard.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL) || destroyCard.SkillApplyInformation.WhiteRitualCount > 0 || (!(destroyCard is FieldBattleCard) && !(destroyCard is ChantFieldBattleCard))))
		{
			destroyCard.ResetFlagCardAsDestroyed();
			if (!destroyCard.IsDead)
			{
				return NullVfx.GetInstance();
			}
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (skillProcessor != null)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
			if (ClassAndInPlayCardList.Any((BattleCardBase c) => c == destroyCard))
			{
				new SkillConditionCheckerOption().DestroyedCard = destroyCard;
				TurnDestroyCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, destroyCard, BattleMgr.IsTurnEnd));
				GameLeftCards.Add(destroyCard);
				GameTurnLeftCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, destroyCard, BattleMgr.IsTurnEnd));
				sequentialVfxPlayer.Register(destroyCard.RemoveFromInPlay());
				sequentialVfxPlayer.Register(CardToCemetery(destroyCard, skill, CEMETERY_TYPE.NORMAL, isRandom));
				SkillProcessor.ProcessInfo info = destroyCard.Skills.CreateWhenLeaveInfo(skillProcessor, playerInfoPair);
				VfxWith<SkillProcessor.ProcessInfo> info2 = destroyCard.Skills.CreateWhenDestroyInfo(destroyCard, skillProcessor, playerInfoPair);
				if (destroyCard.HasSkillWhenDestroy && !DestroyedWhenDestroyCards.Any((BattleCardBase x) => x.Index == destroyCard.Index))
				{
					DestroyedWhenDestroyCards.Add(destroyCard);
				}
				sequentialVfxPlayer.Register(destroyCard.RemoveFromInPlayAfter(skillProcessor));
				sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(null, null, skillProcessor));
				skillProcessor.Register(info, ignoreOwnerDeadCheck: true);
				skillProcessor.Register(info2.Value, ignoreOwnerDeadCheck: true);
				destroyCard.OnDestroy += (BattleCardBase _card, SkillProcessor _skillProcessor) => info2.Vfx;
				destroyCard.SetDestroyedBySkillList(skill);
				StartSkillWhenDestroyOther(destroyCard, skillProcessor);
			}
			else if (HandCardList.Any((BattleCardBase c) => c == destroyCard))
			{
				skillProcessor.Register(destroyCard.Skills.CreateDisCardInfo(skillProcessor, playerInfoPair));
				sequentialVfxPlayer.Register(CardToCemetery(destroyCard, skill));
			}
			return sequentialVfxPlayer;
		}
		return CardToCemetery(destroyCard, skill);
	}

	protected VfxBase BanishCard(BattleCardBase banishedCard, SkillProcessor skillProcessor, bool isRandom, SkillBase skill, bool isOpen)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		new SkillConditionCheckerOption();
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		if (InPlayCards.Any((BattleCardBase c) => c == banishedCard))
		{
			SkillProcessor.ProcessInfo info = banishedCard.Skills.CreateWhenLeaveInfo(skillProcessor, playerInfoPair);
			SkillProcessor.ProcessInfo info2 = banishedCard.Skills.CreateWhenBanishInfo(banishedCard, skillProcessor, playerInfoPair);
			skillProcessor.Register(info, ignoreOwnerDeadCheck: true);
			skillProcessor.Register(info2, ignoreOwnerDeadCheck: true);
			sequentialVfxPlayer.Register(banishedCard.RemoveFromInPlay());
			sequentialVfxPlayer.Register(CardToBanishZone(banishedCard, skill, registerEvent: true, isRandom, isOpen));
			sequentialVfxPlayer.Register(banishedCard.RemoveFromInPlayAfter(skillProcessor));
			StartSkillWhenBanishOther(banishedCard, skillProcessor, isInplay: true);
		}
		else if (HandCardList.Any((BattleCardBase c) => c == banishedCard))
		{
			SkillProcessor.ProcessInfo info3 = banishedCard.Skills.CreateWhenBanishInfo(banishedCard, skillProcessor, playerInfoPair);
			skillProcessor.Register(info3, ignoreOwnerDeadCheck: true);
			sequentialVfxPlayer.Register(CardToBanishZone(banishedCard, skill, registerEvent: true, isRandom, isOpen));
			StartSkillWhenBanishOther(banishedCard, skillProcessor, isInplay: false);
		}
		else if (DeckCardList.Any((BattleCardBase c) => c == banishedCard))
		{
			SkillProcessor.ProcessInfo info4 = banishedCard.Skills.CreateWhenBanishInfo(banishedCard, skillProcessor, playerInfoPair);
			skillProcessor.Register(info4, ignoreOwnerDeadCheck: true);
			sequentialVfxPlayer.Register(CardToBanishZone(banishedCard, skill, registerEvent: true, isRandom, isOpen));
			if (!this.BattleMgr.InstanceIsForecast)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
		}
		sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(null, null, skillProcessor));
		return sequentialVfxPlayer;
	}

	protected VfxBase ReturnCard(BattleCardBase targetCard, SkillProcessor skillProcessor, SkillBase skill)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		List<BattleCardBase> list = new List<BattleCardBase>();
		bool flag = HandCardList.Count < 9;
		if (flag)
		{
			list.Add(targetCard);
			targetCard.DrawTurn = ((targetCard.SelfBattlePlayer.IsSelfTurn && !BattleMgr.IsTurnEnd) ? targetCard.SelfBattlePlayer.Turn : (targetCard.SelfBattlePlayer.Turn + 1));
		}
		BattleCardBase battleCardBase = targetCard.VirtualClone(this, _opponentBattlePlayer);
		GameLeftCards.Add(battleCardBase);
		GameTurnLeftCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, battleCardBase, BattleMgr.IsTurnEnd));
		skillConditionCheckerOption.InHandCard = ConvertToSkillInfoCollection(list);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		List<IReadOnlyBattleCardInfo> list2 = new List<IReadOnlyBattleCardInfo>();
		if (targetCard.SkillApplyInformation.IsSkillCantAtkAll)
		{
			list2.Add(targetCard);
		}
		SkillProcessor.ProcessInfo info = targetCard.Skills.CreateWhenLeaveInfo(skillProcessor, playerInfoPair);
		skillProcessor.Register(info, ignoreOwnerDeadCheck: true);
		SkillProcessor.ProcessInfo info2 = targetCard.Skills.CreateWhenReturnInfo(skillProcessor, playerInfoPair);
		sequentialVfxPlayer2.Register(targetCard.ReturnCard(skillProcessor));
		parallelVfxPlayer.Register(ReturnToHand(targetCard, skill));
		sequentialVfxPlayer2.Register(targetCard.RemoveFromInPlayAfter(skillProcessor, isReturn: true));
		parallelVfxPlayer.Register(sequentialVfxPlayer2);
		this.OnAfterReturnCardEvent.Call(targetCard);
		this.OnLeaveAfterEvent.Call(targetCard);
		skillProcessor.Register(info2, ignoreOwnerDeadCheck: true);
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(new List<BattleCardBase> { targetCard }, null, skillProcessor, isSummonCheck: true, flag ? ((Func<SkillBase, uint>)((SkillBase s) => s.OnWhenAddToHand)) : null, skillConditionCheckerOption));
		StartSkillWhenReturnOther(targetCard, skillProcessor, list2);
		return sequentialVfxPlayer;
	}

	public VfxBase UniteCard(BattleCardBase destroyCard, SkillProcessor skillProcessor, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		new SkillConditionCheckerOption();
		destroyCard.FlagCardAsDestroyedBySkill();
		sequentialVfxPlayer.Register(CardToUniteZone(destroyCard, skill));
		sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(null, null, skillProcessor));
		return sequentialVfxPlayer;
	}

	protected virtual VfxBase DisCard(BattleCardBase ownerCard, List<BattleCardBase> targetCards, SkillProcessor skillProcessor, SkillBase discardedSkill)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		for (int i = 0; i < targetCards.Count; i++)
		{
			parallelVfxPlayer.Register(targetCards[i].DestroyInHand(skillProcessor));
			targetCards[i].SetDiscardedSkill(discardedSkill);
		}
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true, containsDeck: false, (BattleCardBase card) => card != ownerCard && card.Skills.Any((SkillBase s) => s.OnDisCardOtherStart != 0));
		for (int num = 0; num < cardsOrderBySkillActivation.Count; num++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateDisCardOtherInfo(targetCards, cardsOrderBySkillActivation[num].IsPlayer, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2));
		}
		return SequentialVfxPlayer.Create(parallelVfxPlayer);
	}

	protected virtual VfxBase DisCards(List<BattleCardBase> targetCards, SkillProcessor skillProcessor, SkillBase discardedSkill)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
			BurialRiteOrDiscardCardHandIndexList.Clear();
		}));
		for (int num = 0; num < targetCards.Count; num++)
		{
			parallelVfxPlayer.Register(targetCards[num].DestroyInHand(skillProcessor));
			targetCards[num].SetDiscardedSkill(discardedSkill);
		}
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true, containsDeck: false, (BattleCardBase card) => !targetCards.Contains(card) && card.Skills.Any((SkillBase s) => s.OnDisCardOtherStart != 0));
		for (int num2 = 0; num2 < cardsOrderBySkillActivation.Count; num2++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[num2].Skills.CreateDisCardOtherInfo(targetCards, cardsOrderBySkillActivation[num2].IsPlayer, skillProcessor, (cardsOrderBySkillActivation[num2].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2));
		}
		return SequentialVfxPlayer.Create(parallelVfxPlayer);
	}

	protected virtual VfxBase ReturnToHand(BattleCardBase returnCard, SkillBase skill)
	{
		return FieldCardToHandCard(returnCard, skill);
	}

	public VfxBase StartBattleMainView(bool playEffect = true)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(CreateUpdateClassInfoVfx(playEffect));
		for (int i = 0; i < InHandCards.Count; i++)
		{
			parallelVfxPlayer.Register(InHandCards.ToList()[i].BattleCardView.ShowHandCardInfo());
		}
		return parallelVfxPlayer;
	}

	public virtual VfxBase TurnStart()
	{
		foreach (BattleCardBase handCard in HandCardList)
		{
			handCard.SetOnDraw(draw: true);
		}
		IsSelfTurn = true;
		_opponentBattlePlayer.IsSelfTurn = false;
		this.OnTurnStartStart.Call();
		NowTurnEvol = true;
		IsEpEvolveThisTurn = false;
		TurnNecromanceCount = 0;
		TurnPlayCards.Clear();
		TurnDrawCards.Clear();
		TurnDrawTokenCardsWithId.Clear();
		TurnBurialRiteCards.Clear();
		TurnFusionCards.Clear();
		if (!IsGameFirst || Turn != 1)
		{
			IsAlreadyChoiceBraveInThisTurn = false;
		}
		_turnUsedEpCount = 0;
		TurnStartLifeList.Add(new TurnAndIntValue(Class.Life, BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn));
		_opponentBattlePlayer.TurnStartLifeList.Add(new TurnAndIntValue(_opponentBattlePlayer.Class.Life, BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn));
		SkillProcessor skillProcessor = new SkillProcessor();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		_ = PpTotal;
		BattleMgr.GameMgr.GetDataMgr();
		int num = 0;
		IDetailPanelControl detailPanelControl = BattleMgr.DetailMgr.DetailPanelControl;
		if (EvolveWaitTurnCount <= 0)
		{
			for (int i = 0; i < BonusConditionList.Count; i++)
			{
				if (BonusConditionList[i].GetAndReduceAddPpTurn())
				{
					BonusConditionList[i].UseUpAddPpTotalBonus();
					if (detailPanelControl._card != null && detailPanelControl._card.IsClass && detailPanelControl._card.IsPlayer == IsPlayer)
					{
						detailPanelControl.UpdateBuffInfo(_class, BonusConditionList);
					}
					num += BonusConditionList[i].MyRotationBonus.IncreaseAddPptotalAmount;
				}
			}
		}
		int count = ((!Class.SkillApplyInformation.IsTurnStartFixedPP) ? (1 + num) : 0);
		VfxBase vfx = AddPpTotal(count, isUpdatePp: true, skillProcessor);
		sequentialVfxPlayer.Register(vfx);
		BattleLogManager.GetInstance().AddLogTurn(IsPlayer);
		BattleLogManager.GetInstance().BeginLogBlockTurnChangeReactive();
		VfxBase[] allFuncCallResults = this.OnTurnStartBeforeDraw.GetAllFuncCallResults(skillProcessor);
		foreach (VfxBase vfx2 in allFuncCallResults)
		{
			sequentialVfxPlayer.Register(vfx2);
		}
		BattleCardBase[] array = HandCardList.ToArray();
		BattleCardBase[] array2 = _opponentBattlePlayer.HandCardList.ToArray();
		BattleCardBase[] array3 = ClassAndInPlayCardList.ToArray();
		BattleCardBase[] array4 = _opponentBattlePlayer.ClassAndInPlayCardList.ToArray();
		BattleCardBase[] array5 = DeckSkillCardList.ToArray();
		BattleCardBase[] array6 = _opponentBattlePlayer.DeckSkillCardList.ToArray();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattleCardBase[] array7 = array;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx3 = array7[j].TurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx3);
		}
		array7 = array2;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx4 = array7[j].OpponentTurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx4);
		}
		array7 = array3;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx5 = array7[j].TurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx5);
		}
		array7 = array4;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx6 = array7[j].OpponentTurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx6);
		}
		array7 = array5;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx7 = array7[j].TurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx7);
		}
		array7 = array6;
		for (int j = 0; j < array7.Length; j++)
		{
			VfxBase vfx8 = array7[j].OpponentTurnStart(skillProcessor);
			parallelVfxPlayer.Register(vfx8);
		}
		BattleMgr.OperateMgr.CallOnUpdateAttackableEffect(array3.Where((BattleCardBase c) => c.Attackable).ToList(), array4.Where((BattleCardBase c) => c.Attackable).ToList());
		sequentialVfxPlayer.Register(parallelVfxPlayer);
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		VfxBase vfx9 = skillProcessor.Process(battlePlayerPair);
		sequentialVfxPlayer.Register(vfx9);
		allFuncCallResults = OnTurnStartSkillAfter.GetAllFuncCallResults(skillProcessor);
		foreach (VfxBase vfx10 in allFuncCallResults)
		{
			sequentialVfxPlayer.Register(vfx10);
		}
		if (IsGameFirst && Turn == 1)
		{
			for (int num2 = 0; num2 < HandCardList.Count(); num2++)
			{
				HandCardList[num2].Skills.CreateAndRegisterWhenChangeInplaySelfhandInfo(HandCardList, skillProcessor, battlePlayerPair);
			}
			BattlePlayerPair playerInfoPair = new BattlePlayerPair(_opponentBattlePlayer, this);
			for (int num3 = 0; num3 < _opponentBattlePlayer.HandCardList.Count(); num3++)
			{
				_opponentBattlePlayer.HandCardList[num3].Skills.CreateAndRegisterWhenChangeInplaySelfhandInfo(_opponentBattlePlayer.HandCardList, skillProcessor, playerInfoPair);
			}
		}
		sequentialVfxPlayer.Register(TurnStartDraw(skillProcessor));
		sequentialVfxPlayer.Register(skillProcessor.Process(battlePlayerPair));
		BattleUIContainer battleUIContainer = BattleMgr.BattleUIContainer;
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			battleUIContainer.EnableMenu();
		}));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			BattleMgr.BattlePlayer.TurnStartEffectEnd();
		}));
		BattleLogManager.GetInstance().EndLogBlockTurnChangeReactive();
		if (!BattleMgr.IsBattleEnd)
		{
			if (BattleMgr.GameMgr.IsNetworkBattle && IsPlayer)
			{
				NetworkBattleManagerBase networkBattleManagerBase = BattleMgr as NetworkBattleManagerBase;
				if (networkBattleManagerBase.turnEndTimeController != null)
				{
					networkBattleManagerBase.turnEndTimeController.AddTurnEndTimerLog("Player SetActiveVFX");
				}
			}
			ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
			parallelVfxPlayer2.Register(InstantVfx.Create(SetActive));
			sequentialVfxPlayer.Register(parallelVfxPlayer2);
		}
		VfxBase allFuncVfxResults = this.OnTurnStartAfterDraw.GetAllFuncVfxResults();
		sequentialVfxPlayer.Register(allFuncVfxResults);
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			foreach (BattleCardBase handCard2 in HandCardList)
			{
				handCard2.SetOnDraw(draw: false);
			}
			UpdateHandCardsPlayability();
		}));
		sequentialVfxPlayer.Register(UpdateHandCardsCost());
		sequentialVfxPlayer.Register(UpdateInPlayBattleCardIconLabel());
		sequentialVfxPlayer.Register(this.OnTurnStartFinish.GetAllFuncVfxResults());
		OnTurnStartComplete.Call();
		return sequentialVfxPlayer;
	}

	public abstract VfxBase StartTurnControl(string log = "");

	public SequentialVfxPlayer TurnEvolveControl(GameObject eqIcon)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool firstEvolve = false;
		if (EvolveWaitTurnCount > 0)
		{
			if (EvolveWaitTurnCount == 1)
			{
				firstEvolve = true;
			}
			EvolveWaitTurnCount--;
		}
		if (BattleMgr.IsRecovery || BattleMgr.IsPuzzleMgr)
		{
			sequentialVfxPlayer.Register(m_vfxCreator.CreateUpdateEp(CurrentEpCount, EvolveWaitTurnCount));
			if (NowTurnEvol && CurrentEpCount > 0 && EvolveWaitTurnCount <= 0)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
		}
		else
		{
			sequentialVfxPlayer.Register(BattleMgr.LoadTurnPanelResource());
			sequentialVfxPlayer.Register(m_vfxCreator.CreateUpdateEp(CurrentEpCount, EvolveWaitTurnCount));
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{

				BattleMgr.TurnPanelControl.StartUI(Turn, EvolveWaitTurnCount, IsPlayer);
				if (BattleMgr.GameMgr.IsWatchBattle || BattleMgr.GameMgr.IsReplayBattle)
				{
					BattleView.TurnEndButtonUI.GameObject.SetActive(value: true);
					BattleView.TurnEndButtonUI.ChangeButtonView(IsPlayer);
				}
			}));
			if (NowTurnEvol && CurrentEpCount > 0 && EvolveWaitTurnCount <= 0)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			sequentialVfxPlayer.Register(WaitVfx.Create(1.6f));
		}
		return sequentialVfxPlayer;
	}

	public virtual VfxBase TurnStartDraw(SkillProcessor skillProcessor)
	{
		DeckCardList.Sort((BattleCardBase a, BattleCardBase b) => a.Index - b.Index);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (!Class.IsDead && !_opponentBattlePlayer.Class.IsDead && !BattleMgr.IsPuzzleMgr)
		{
			sequentialVfxPlayer.Register(TurnStartDrawCard(skillProcessor));
		}
		return sequentialVfxPlayer;
	}

	protected abstract VfxBase TurnStartDrawCard(SkillProcessor skillProcessor);

	protected abstract void SetActive();

	public abstract BattlePlayerBase CreateVirtualPlayer();

	public VfxBase AddPpTotal(int count, bool isUpdatePp, SkillProcessor skillProcessor, BattleCardBase ownerCard = null, bool bySkill = false)
	{
		string text = "";
		bool flag = false;
		if (Turn <= 1 && !IsPlayer)
		{
			flag = true;
		}
		if (flag)
		{
			text = text + "AddPpTotal " + count + "isUpdatePp " + isUpdatePp + "NotPPcounter " + Class.SkillApplyInformation.NotDecreasePPCounter + "IsTurnStartFixedPP " + Class.SkillApplyInformation.IsTurnStartFixedPP + ":" + StackTraceUtility.ExtractStackTrace();
		}
		if (count < 0 && Class.SkillApplyInformation.NotDecreasePPCounter > 0)
		{
			if (flag)
			{
				LocalLog.AccumulateLastTraceLog(text);
			}
			return NullVfx.GetInstance();
		}
		new SkillConditionCheckerOption();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int ppTotal = PpTotal;
		PpTotal += count;
		if (PpTotal > 10)
		{
			PpTotal = 10;
		}
		else if (PpTotal < 0)
		{
			PpTotal = 0;
		}
		int num = PpTotal;
		if (flag)
		{
			text = text + "DecreaseList " + Class.SkillApplyInformation.DecreaseTurnStartPPList.Count;
		}
		for (int i = 0; i < Class.SkillApplyInformation.DecreaseTurnStartPPList.Count; i++)
		{
			if (flag)
			{
				text = text + "Decrease:" + Class.SkillApplyInformation.DecreaseTurnStartPPList[i];
			}
			num -= Class.SkillApplyInformation.DecreaseTurnStartPPList[i];
			if (num < 0)
			{
				num = 0;
			}
		}
		Pp = (isUpdatePp ? num : Math.Min(Pp, PpTotal));
		if (flag)
		{
			text = text + "nowPP " + Pp;
			LocalLog.AccumulateLastTraceLog(text);
		}
		parallelVfxPlayer.Register(NullVfx.GetInstance());
		this.OnChangePP.Call(PpTotal - ppTotal);
		this.OnAddPpTotal.Call(PpTotal - ppTotal, Pp, IsPlayer, ownerCard, bySkill);
		if (skillProcessor != null)
		{
			StartSkillWhenChangePPTotal(skillProcessor);
		}
		return parallelVfxPlayer;
	}

	public VfxBase SetPpTotal(int pp, bool isUpdatePp, SkillProcessor skillProcessor)
	{
		return AddPpTotal(pp - PpTotal, isUpdatePp, skillProcessor);
	}

	public SkillProcessor GetTurnEndSkillProcess()
	{
		SkillProcessor skillProcessor = new SkillProcessor();
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(HandCardList);
		list.AddRange(_opponentBattlePlayer.HandCardList);
		list.AddRange(ClassAndInPlayCardList);
		list.AddRange(_opponentBattlePlayer.ClassAndInPlayCardList);
		list.AddRange(DeckSkillCardList);
		list.AddRange(_opponentBattlePlayer.DeckSkillCardList);
		int num = list.Count();
		for (int i = 0; i < num; i++)
		{
			list[i].TurnEndSkillProcess(skillProcessor);
		}
		return skillProcessor;
	}

	public virtual VfxBase TurnEnd()
	{
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		SequentialVfxPlayer turnEndVfx = SequentialVfxPlayer.Create();
		if (this.OnTurnEndStart != null)
		{
			this.OnTurnEndStart.Call();
		}
		BattleLogManager.GetInstance().BeginLogBlockTurnChangeReactive();
		turnEndVfx.Register(BattleView.SetIsNowTurnEnd(flg: true));
		SkillProcessor turnEndSkillProcess = GetTurnEndSkillProcess();
		VfxBase vfx = turnEndSkillProcess.Process(battlePlayerPair);
		turnEndVfx.Register(vfx);
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(ClassAndInPlayCardList);
		list.AddRange(_opponentBattlePlayer.ClassAndInPlayCardList);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].CheckPreviousTurnAttacked();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase inPlayCard in InPlayCards)
		{
			parallelVfxPlayer.Register(inPlayCard.TurnEndPostProcess());
		}
		turnEndVfx.Register(parallelVfxPlayer);
		VfxBase[] allFuncCallResults = this.OnTurnEnd.GetAllFuncCallResults(turnEndSkillProcess);
		foreach (VfxBase vfx2 in allFuncCallResults)
		{
			turnEndVfx.Register(vfx2);
		}
		allFuncCallResults = OnTurnEndSkillAfter.GetAllFuncCallResults(turnEndSkillProcess);
		foreach (VfxBase vfx3 in allFuncCallResults)
		{
			turnEndVfx.Register(vfx3);
		}
		turnEndVfx.Register(turnEndSkillProcess.Process(battlePlayerPair));
		turnEndVfx.Register(UpdateInPlayBattleCardIconLabel());
		turnEndVfx.Register(InstantVfx.Create(delegate
		{
			if (HandCardList.Count <= 0)
			{
				turnEndVfx.Register(BattleView.HandUnfocus());
			}
		}));
		TurnPlayCards.Clear();
		TurnDrawCards.Clear();
		TurnDrawTokenCardsWithId.Clear();
		TurnBurialRiteCards.Clear();
		TurnFusionCards.Clear();
		TurnNecromanceCount = 0;
		_turnUsedEpCount = 0;
		TurnResonanceStartCount = 0;
		_opponentBattlePlayer.TurnResonanceStartCount = 0;
		BattleLogManager.GetInstance().EndLogBlockTurnChangeReactive();
		turnEndVfx.Register(BattleMgr.JudgeBattleResult());
		if (!BattleMgr.IsRecovery)
		{
			turnEndVfx.Register(InstantVfx.Create(delegate
			{
				int count = ClassAndInPlayCardList.Count;
				for (int k = 0; k < count; k++)
				{
					BattleCardBase card = ClassAndInPlayCardList[k];
					if (!card.BattleCardView.GameObject.activeSelf)
					{
						turnEndVfx.Register(InstantVfx.Create(delegate
						{
							card.BattleCardView.GameObject.SetActive(value: true);
							if (card.IsInplay)
							{
								card.SetOnDraw(draw: false);
							}
						}));
					}
				}
			}));
		}
		OnPreTurnEndComplete.Call();
		if (!Class.IsDead && !_opponentBattlePlayer.Class.IsDead)
		{
			IsSelfTurn = false;
			VfxBase allFuncVfxResults = this.OnTurnEndFinish.GetAllFuncVfxResults();
			turnEndVfx.Register(allFuncVfxResults);
		}
		OnPostTurnEndComplete.Call();
		turnEndVfx.Register(BattleView.SetIsNowTurnEnd(flg: false));
		LocalLog.RecordTurnEndIfLoadErrorOccured();
		return turnEndVfx;
	}

	public void DecreasesExtraTurnCount()
	{
		extraTurnCount = Math.Max(0, extraTurnCount - 1);
	}

	public void Clear()
	{
		ClearSpineObject();
		ClearClassAndMainPlace();
		ClearBattleCount();
	}

	protected void ClearSpineObject()
	{
		if (Class is ClassBattleCardBase classBattleCardBase)
		{
			classBattleCardBase.ClearSpineObject();
		}
	}

	public void ClearClassAndMainPlace()
	{
		HandCardList.Clear();
		DeckCardList.Clear();
		BattleStartDeckCardList.Clear();
		DeckSkillCardList.Clear();
		ClassAndInPlayCardList.Clear();
	}

	public void ClearBattleCount()
	{
		CemeteryList.Clear();
		PredictionCemeteryRandomCards.Clear();
		PredictionDamageRandomCards.Clear();
		PredictionBanishRandomCards.Clear();
		BanishList.Clear();
		NecromanceZoneList.Clear();
		UniteList.Clear();
		GetOnList.Clear();
		BlackHole.Clear();
		ChoiceBraveCardList.Clear();
		ReturnList.Clear();
		HealingCards.Clear();
		LastTargetCardsList.Clear();
		SkillSummonedCards.Clear();
		SummonedCards.Clear();
		EvolvedCards.Clear();
		DestroyedWhenDestroyCards.Clear();
		InHandCards.Clear();
		SkillDiscards.Clear();
		SkillBanishCards.Clear();
		DrewSkillCard = null;
		TurnPlayCards.Clear();
		TurnDrawCards.Clear();
		TurnDrawTokenCardsWithId.Clear();
		GamePlayCards.Clear();
		GameTurnPlayCards.Clear();
		GameEnhancePlayCards.Clear();
		GameCrystallizedPlayCards.Clear();
		OkSkillInProcess.Clear();
		TurnDestroyCards.Clear();
		AddToDeckCardList.Clear();
		GameSummonCards.Clear();
		GameSummonMomentTribe.Clear();
		GamePlayMomentTribe.Clear();
		GamePlayMomentSpellChargeCards.Clear();
		GameUpdateDeckMomentTribe.Clear();
		GameDrawCards.Clear();
		GameDrawTokenCards.Clear();
		GameAddUpdateDeckCards.Clear();
		GameQuickAttackCards.Clear();
		TurnBurialRiteCards.Clear();
		GameReanimatedCards.Clear();
		TurnWhenHealingCount.Clear();
		GameLeftCards.Clear();
		GameTurnLeftCards.Clear();
		GameReturnedCards.Clear();
		GameSuperSkyboundArtCards.Clear();
		TurnPlayCardCountInfo.Clear();
		TurnFusionCountInfo.Clear();
		TurnNecromanceCount = 0;
		GameNecromanceCount = 0;
		GameUsedPpCount = 0;
		RallyCount = 0;
		DeckBanishCount = 0;
		GameResonanceStartCount = 0;
		TurnResonanceStartCount = 0;
		GameUsedWhiteRitualCount = 0;
		LastInplayWhiteRitualStack = 0;
		_turnUsedEpCount = 0;
		GameSkillReturnCardCountList.Clear();
		GameSkillDiscardCountList.Clear();
		GameSkillBuffCountList.Clear();
		GameSkillMetamorphoseCountList.Clear();
		GameSkillDiscardCount = 0;
		DiscardedCardList.Clear();
		FusionIngredientList.Clear();
		FusionIngredientAndDiscardedCardList.Clear();
		ReservedCardList.Clear();
		TurnEvolveCardCountInfo.Clear();
		GameInplayMetamorphoseCards.Clear();
		GameBurialRiteCards.Clear();
		TurnStartLifeList.Clear();
		TurnFusionCards.Clear();
	}

	public BattleCardBase FindCardFromGameObject(GameObject cardObject)
	{
		return AllCards.FirstOrDefault((BattleCardBase s) => s.BattleCardView.GameObject == cardObject);
	}

	protected void AddInplayCard(BattleCardBase card, bool isGetoff = false, bool isReanimate = false)
	{
		ClassAndInPlayCardList.Add(card);
		if (!card.IsClass && !isGetoff)
		{
			GameSummonCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, card, BattleMgr.IsTurnEnd));
			GameSummonMomentTribe.Add(new CardAndTribe(card, card.Tribe));
			if (isReanimate)
			{
				GameReanimatedCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, card, BattleMgr.IsTurnEnd));
			}
		}
	}

	public void AddRallyCount(int addCount)
	{
		RallyCount += addCount;
	}

	public VfxBase PickCard(BattleCardBase unit, SkillBase skill, SkillBaseSummon.SUMMON_TYPE summonType = SkillBaseSummon.SUMMON_TYPE.HAND, bool isGetoff = false, bool isReanimate = false)
	{
		this.OnPickCard.Call(unit);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		switch (summonType)
		{
		case SkillBaseSummon.SUMMON_TYPE.HAND:
			HandCardToField(unit, skill);
			break;
		case SkillBaseSummon.SUMMON_TYPE.DECK:
			DeckCardToField(unit, skill);
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			break;
		case SkillBaseSummon.SUMMON_TYPE.TOKEN:
			TokenToField(unit, skill, isGetoff, isReanimate);
			break;
		case SkillBaseSummon.SUMMON_TYPE.DESTROYED:
			DestroyedToField(unit, skill);
			break;
		}
		this.OnAfterPickCard.Call();
		return sequentialVfxPlayer;
	}

	public virtual void HandCardToField(BattleCardBase targetCard, SkillBase skill = null)
	{
		BattleCardBase battleCardBase = HandCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		if (battleCardBase == null)
		{
			throw new Exception("Target card was not found in hand cards.");
		}
		this.OnAddPlayCardEvent.Call(battleCardBase, arg2: false, skill);
		AddInplayCard(battleCardBase);
		HandCardList.Remove(battleCardBase);
		CallSkill((IBattlePlayerSkill s) => s.StopBattleHandCard, battleCardBase);
		this.OnAddPlayCardAfterEvent.Call();
		this.OnSummonAfterEvent.Call(battleCardBase);
	}

	private bool TokenToField(BattleCardBase targetCard, SkillBase skill, bool isGetoff = false, bool isReanimate = false)
	{
		if (InPlayCards.Count() < 6)
		{
			this.OnAddPlayCardEvent.Call(targetCard, isGetoff, skill);
			AddInplayCard(targetCard, isGetoff, isReanimate);
			this.OnSummonAfterEvent.Call(targetCard);
			return true;
		}
		return false;
	}

	public bool CemeteryConsumption(int num, BattleCardBase necromanceCard, SkillProcessor skillprocessor, bool isFusion)
	{
		if (CemeteryList.Count < num)
		{
			return false;
		}
		if (num == -1)
		{
			num = 0;
		}
		for (int i = 0; i < num; i++)
		{
			BattleCardBase item = CemeteryList.First();
			NecromanceZoneList.Add(item);
			CemeteryList.Remove(item);
		}
		SuccessNecromance(necromanceCard, skillprocessor, num, isFusion);
		return true;
	}

	public void SuccessNecromance(BattleCardBase necromanceCard, SkillProcessor skillprocessor, int necromanceCount, bool isFusion)
	{
		int turnNecromanceCount = TurnNecromanceCount + 1;
		TurnNecromanceCount = turnNecromanceCount;
		GameNecromanceCount += necromanceCount;
		this.OnNecromance.Call(necromanceCard, skillprocessor, necromanceCount, isFusion);
	}

	public VfxBase GainCemetery(int gainCount)
	{
		if (gainCount > CemeteryList.Count())
		{
			gainCount = CemeteryList.Count();
		}
		for (int i = 0; i < gainCount; i++)
		{
			BattleCardBase item = CemeteryList.First();
			NecromanceZoneList.Add(item);
			CemeteryList.Remove(item);
		}
		return this.OnAddCemeteryAfterEvent.GetAllFuncVfxResults();
	}

	protected VfxBase FieldCardToHandCard(BattleCardBase targetCard, SkillBase skill)
	{
		BattleCardBase battleCardBase = ClassAndInPlayCardList.Single((BattleCardBase c) => c == targetCard);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(battleCardBase.RemoveFromInPlay());
		if (HandCardList.Count >= 9)
		{
			VfxBase vfx = CardToCemetery(battleCardBase, skill, CEMETERY_TYPE.FIELD_RETURN_HAND_OVER);
			sequentialVfxPlayer.Register(vfx);
			return sequentialVfxPlayer;
		}
		ClassAndInPlayCardList.Remove(battleCardBase);
		HandCardAddList(battleCardBase, NetworkBattleDefine.NetworkCardPlaceState.Field, skill);
		return sequentialVfxPlayer;
	}

	protected void HandCardAddList(BattleCardBase targetCard, NetworkBattleDefine.NetworkCardPlaceState fromState, SkillBase skill, bool isOpen = false)
	{
		InHandCards.Add(targetCard);
		this.OnAddHandCardEvent.Call(targetCard, fromState, isOpen, skill);
		HandCardList.Add(targetCard);
		CallSkill((IBattlePlayerSkill s) => s.StartBattleHandCard, targetCard);
		this.OnAddHandCardAfterEvent.Call();
	}

	public VfxBase CardToCemetery(BattleCardBase targetCard, SkillBase skill, CEMETERY_TYPE cemeteryType = CEMETERY_TYPE.NORMAL, bool wasRandom = false)
	{
		if (CemeteryList.Any((BattleCardBase c) => c == targetCard))
		{
			return NullVfx.GetInstance();
		}
		BattleCardBase battleCardBase = ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		BattleCardBase battleCardBase2 = HandCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		BattleCardBase battleCardBase3 = DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		if (battleCardBase == null && battleCardBase2 == null && battleCardBase3 == null)
		{
			return NullVfx.GetInstance();
		}
		this.OnAddCemeteryEvent.Call(targetCard, cemeteryType, arg3: false, skill);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattleCardBase battleCardBase4 = null;
		if (battleCardBase != null)
		{
			ClassAndInPlayCardList.Remove(battleCardBase);
			battleCardBase4 = battleCardBase;
			if (battleCardBase.IsUnit && cemeteryType != CEMETERY_TYPE.FIELD_RETURN_HAND_OVER && !BattleMgr.IsVirtualBattle)
			{
				BattleLogManager.GetInstance().AddLogDestFollower(BattleLogWindow.BattleLogType.Destruction, battleCardBase);
			}
		}
		else
		{
			battleCardBase4 = ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		}
		if (battleCardBase4 == null && battleCardBase2 != null)
		{
			HandCardList.Remove(battleCardBase2);
			CallSkill((IBattlePlayerSkill s) => s.StopBattleHandCard, battleCardBase2);
			battleCardBase4 = battleCardBase2;
		}
		if (battleCardBase4 == null && battleCardBase3 != null)
		{
			DeckCardList.Remove(battleCardBase3);
			sequentialVfxPlayer.Register(CreateUpdateDeckCountLabelVfx());
			battleCardBase4 = battleCardBase3;
		}
		VfxBase vfxBase = NullVfx.GetInstance();
		if (battleCardBase4 != null)
		{
			CemeteryList.Add(battleCardBase4);
			if (!battleCardBase4.IsClass)
			{
				vfxBase = battleCardBase4.UnloadResource();
				sequentialVfxPlayer.Register(this.OnAddCemeteryAfterEvent.GetAllFuncVfxResults());
				this.OnLeaveAfterEvent.Call(battleCardBase4);
			}
			if (wasRandom)
			{
				PredictionCemeteryRandomCards.Add(battleCardBase4);
			}
		}
		return SequentialVfxPlayer.Create(vfxBase, sequentialVfxPlayer);
	}

	public VfxBase CardToVehicleZone(BattleCardBase targetCard, SkillBase skill)
	{
		if (CemeteryList.Any((BattleCardBase c) => c == targetCard))
		{
			return NullVfx.GetInstance();
		}
		if (!ClassAndInPlayCardList.Any((BattleCardBase c) => c == targetCard))
		{
			return NullVfx.GetInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		this.OnGeton.Call(targetCard, skill);
		ClassAndInPlayCardList.Remove(targetCard);
		GetOnList.Add(targetCard);
		sequentialVfxPlayer.Register(targetCard.UnloadResource());
		return sequentialVfxPlayer;
	}

	public VfxBase DummyCardToCemetery(BattleCardBase targetCard, SkillBase skill = null)
	{
		this.OnAddCemeteryEvent.Call(targetCard, CEMETERY_TYPE.NORMAL, arg3: false, skill);
		CemeteryList.Add(targetCard);
		return this.OnAddCemeteryAfterEvent.GetAllFuncVfxResults();
	}

	public VfxBase ClearDestroyedAndDiscardedCardList(SkillBase skill)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int count = CemeteryList.Count;
		this.OnAddBlackHole.Call(CemeteryList, skill);
		this.OnAddBlackHole.Call(NecromanceZoneList, skill);
		BlackHole.AddRange(CemeteryList);
		BlackHole.AddRange(NecromanceZoneList);
		CemeteryList.Clear();
		NecromanceZoneList.Clear();
		DestroyedWhenDestroyCards.Clear();
		for (int i = 0; i < DiscardedCardList.Count; i++)
		{
			FusionIngredientAndDiscardedCardList.Remove(DiscardedCardList[i]);
		}
		DiscardedCardList.Clear();
		for (int j = 0; j < count; j++)
		{
			parallelVfxPlayer.Register(DummyCardToCemetery(CardCreatorBase.GetDummyInstance(), skill));
		}
		return parallelVfxPlayer;
	}

	public VfxBase RemoveSpellCardFromHand(BattleCardBase targetSpellCard)
	{
		if (targetSpellCard.IsChoiceBraveSkillCard)
		{
			return NullVfx.GetInstance();
		}
		BattleCardBase battleCardBase = HandCardList.SingleOrDefault((BattleCardBase c) => c == targetSpellCard);
		if (battleCardBase == null)
		{
			throw new Exception("Target card was not found in hand cards.");
		}
		this.OnSpellPlayEvent.Call(battleCardBase);
		HandCardList.Remove(battleCardBase);
		return NullVfx.GetInstance();
	}

	public VfxBase AddSpellCardToCemetery(BattleCardBase targetSpellCard)
	{
		if (CemeteryList.Any((BattleCardBase c) => c == targetSpellCard))
		{
			return NullVfx.GetInstance();
		}
		if (!targetSpellCard.IsChoiceBraveSkillCard)
		{
			CemeteryList.Add(targetSpellCard);
		}
		else
		{
			ChoiceBraveCardList.Add(targetSpellCard);
		}
		BattleMgr.VfxMgr.RegisterImmediateVfx(this.OnAddCemeteryAfterEvent.GetAllFuncVfxResults());
		return targetSpellCard.UnloadResource();
	}

	public VfxBase CardToBanishZone(BattleCardBase targetCard, SkillBase skill, bool registerEvent = true, bool wasRandom = false, bool isOpen = false)
	{
		targetCard.DeathTypeInfo.BanishDestroy = true;
		if (BanishList.Any((BattleCardBase c) => c == targetCard))
		{
			return NullVfx.GetInstance();
		}
		if (registerEvent)
		{
			bool arg = isOpen || (skill is Skill_banish && skill.OnSelfTurnEndStart != 0 && skill.ConditionFilterCollection.VariableCompareFilter.Any((SkillVariableComareFilter f) => f.Text.Contains("hand_self")));
			this.OnAddBanishEvent.Call(targetCard, skill, arg);
		}
		BattleCardBase battleCardBase = null;
		if (ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			battleCardBase = ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
			ClassAndInPlayCardList.Remove(ClassAndInPlayCardList.SingleOrDefault((BattleCardBase c) => c == targetCard));
			BattleCardBase battleCardBase2 = battleCardBase.VirtualClone(this, _opponentBattlePlayer);
			GameLeftCards.Add(battleCardBase2);
			GameTurnLeftCards.Add(new TurnAndCard(IsSelfTurn ? Turn : BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn, battleCardBase2, BattleMgr.IsTurnEnd));
		}
		else if (HandCardList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			battleCardBase = HandCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
			HandCardList.Remove(battleCardBase);
			CallSkill((IBattlePlayerSkill s) => s.StopBattleHandCard, battleCardBase);
		}
		else if (DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			battleCardBase = DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
			if (battleCardBase == null)
			{
				throw new Exception("Target card was not found in either field, hand or deck.");
			}
			DeckCardList.Remove(battleCardBase);
			battleCardBase.SelfBattlePlayer.DeckBanishCount++;
		}
		if (battleCardBase == null)
		{
			return NullVfx.GetInstance();
		}
		BanishList.Add(battleCardBase);
		SkillBanishCards.Add(battleCardBase);
		if (wasRandom)
		{
			PredictionBanishRandomCards.Add(battleCardBase);
		}
		this.OnAddBanishAfterEvent.Call(targetCard);
		this.OnLeaveAfterEvent.Call(targetCard);
		return battleCardBase.UnloadResource();
	}

	public VfxBase CardToUniteZone(BattleCardBase targetCard, SkillBase skill)
	{
		targetCard.DeathTypeInfo.BanishDestroy = true;
		if (UniteList.Any((BattleCardBase c) => c == targetCard))
		{
			return NullVfx.GetInstance();
		}
		this.OnAddUniteEvent.Call(targetCard, skill);
		ClassAndInPlayCardList.Remove(targetCard);
		UniteList.Add(targetCard);
		this.OnAddUniteAfterEvent.Call(targetCard);
		this.OnLeaveAfterEvent.Call(targetCard);
		return targetCard.UnloadResource();
	}

	private void DeckCardToField(BattleCardBase targetCard, SkillBase skill)
	{
		BattleCardBase battleCardBase = DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		if (battleCardBase == null)
		{
			throw new Exception("Target card is not found from deck cards.");
		}
		this.OnAddPlayCardEvent.Call(battleCardBase, arg2: false, skill);
		AddInplayCard(battleCardBase);
		DeckCardList.Remove(battleCardBase);
		this.OnSummonAfterEvent.Call(battleCardBase);
	}

	private void DestroyedToField(BattleCardBase targetCard, SkillBase skill)
	{
		BattleCardBase battleCardBase = null;
		battleCardBase = CemeteryList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		if (battleCardBase == null)
		{
			battleCardBase = NecromanceZoneList.SingleOrDefault((BattleCardBase c) => c == targetCard);
		}
		if (battleCardBase == null)
		{
			throw new Exception("Target card is not found from destroyed cards.");
		}
		this.OnAddPlayCardEvent.Call(battleCardBase, arg2: false, skill);
		AddInplayCard(battleCardBase);
		CemeteryList.Remove(battleCardBase);
		NecromanceZoneList.Remove(battleCardBase);
		this.OnSummonAfterEvent.Call(battleCardBase);
	}

	public virtual VfxBase CardDrawVfx(IEnumerable<BattleCardBase> DrawList, bool skipShuffle = false, bool isOpenDrawSkill = false)
	{
		return NullVfx.GetInstance();
	}

	private VfxBase DrawCard(BattleCardBase targetCard, SkillBase skill, bool isOpen = false, bool isToken = false, bool isReservation = false)
	{
		bool flag = false;
		if (!isToken && DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			DeckCardList.Remove(targetCard);
			if (!BattleMgr.IsVirtualBattle)
			{
				flag = true;
				StatusPanelControl.SetDeck(DeckCardList.Count);
			}
		}
		if (HandCardList.Count >= 9)
		{
			return NullVfx.GetInstance();
		}
		if (isToken)
		{
			HandCardAddList(targetCard, isReservation ? NetworkBattleDefine.NetworkCardPlaceState.Reservation : NetworkBattleDefine.NetworkCardPlaceState.None, skill, isOpen);
			return CreateUpdateDeckCountLabelVfx();
		}
		targetCard.ResetCardParameterInHand();
		HandCardAddList(targetCard, NetworkBattleDefine.NetworkCardPlaceState.Deck, skill, isOpen);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (DeckCardList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			DeckCardList.Remove(targetCard);
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				StatusPanelControl.SetDeck(DeckCardList.Count);
			}));
		}
		else if (NecromanceZoneList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			NecromanceZoneList.Remove(targetCard);
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				targetCard.BattleCardView.ResetCardView(targetCard.BaseParameter);
			}));
		}
		else if (CemeteryList.SingleOrDefault((BattleCardBase c) => c == targetCard) != null)
		{
			CemeteryList.Remove(targetCard);
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				targetCard.BattleCardView.ResetCardView(targetCard.BaseParameter);
			}));
		}
		if (flag)
		{
			return parallelVfxPlayer;
		}
		return SequentialVfxPlayer.Create(parallelVfxPlayer, CreateUpdateDeckCountLabelVfx());
	}

	public virtual VfxBase CreateUpdateDeckCountLabelVfx()
	{
		return InstantVfx.Create(delegate
		{
			StatusPanelControl.SetDeck(DeckCardList.Count);
		});
	}

	protected virtual VfxBase CreateUpdateClassInfoVfx(bool playEffect)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
			StatusPanelControl.SetHandCount(HandCardList.Count);
		}));
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
			ClassInformationUIController.ShowInfomation(playEffect);
		}));
		return parallelVfxPlayer;
	}

	private void EvolveProcess(BattleCardBase card, bool isSkill)
	{
		if (!CheckNotConsumeEpCard(card))
		{
			GainCurrentEpCount(card.SkillApplyInformation.GetEp());
			_gameUsedEpCount += card.SkillApplyInformation.GetEp();
			_turnUsedEpCount += card.SkillApplyInformation.GetEp();
		}
		if (!isSkill)
		{
			_cumulativeEvolutionCount++;
		}
	}

	public bool UseEpCount(int count)
	{
		if (CurrentEpCount >= count)
		{
			GainCurrentEpCount(count);
			_gameUsedEpCount += count;
			_turnUsedEpCount += count;
			return true;
		}
		return false;
	}

	public VfxBase AddDeckTokenCards(List<BattleCardBase> cards, SkillProcessor skillProcessor, string updateType, SkillBase skill, bool isOpen)
	{
		if (updateType == "change")
		{
			DeckClear(skill);
		}
		for (int i = 0; i < cards.Count; i++)
		{
			AddToDeck(cards[i], callEvent: true, skill);
		}
		this.OnUpdateDeck.Call(skill.SkillPrm.ownerCard, cards, IsPlayer, updateType == "change", isOpen);
		return NullVfx.GetInstance();
	}

	public void CallOnChangeDeckAfterEvent(int previousCount, SkillProcessor skillProcessor, List<BattleCardBase> summonCards)
	{
		this.OnChangeDeckAfterEvent.Call(previousCount, skillProcessor, summonCards);
	}

	public void DeckClear(SkillBase skill)
	{
		this.OnClearDeck.Call();
		this.OnAddBlackHole.Call(DeckCardList, skill);
		BlackHole.AddRange(DeckCardList);
		DeckCardList.Clear();
	}

	public void AddToDeck(BattleCardBase card, bool callEvent = false, SkillBase skill = null)
	{
		if (callEvent)
		{
			this.OnAddDeckEvent.Call(card, skill);
		}
		DeckCardList.Add(card);
		if (card.HasDeckSelfSkill)
		{
			AddDeckSkillCard(card);
		}
		if (BattleMgr.XorShiftRandom(card.IsPlayer) != null && BattleMgr.XorShiftRandom(card.IsPlayer).IsActive && BattleMgr.IsMulliganEnd)
		{
			AddToDeckCardList.Add(card);
		}
	}

	public void AddDeckSkillCard(BattleCardBase card)
	{
		DeckSkillCardList.Add(card);
		DeckSkillCardList = DeckSkillCardList.OrderBy((BattleCardBase c) => c.Index).ToList();
	}

	public void RemoveOriginalAndAddDeckSkillCard(BattleCardBase card)
	{
		DeckSkillCardList.RemoveAll((BattleCardBase c) => c.Index == card.Index);
		AddDeckSkillCard(card);
	}

	private void AddToDeckCardIndexChange()
	{
		if (AddToDeckCardList.Count == 0 && _opponentBattlePlayer.AddToDeckCardList.Count == 0)
		{
			return;
		}
		if (AddToDeckCardList.Count > 0 && BattleMgr.XorShiftRandom(AddToDeckCardList.First().IsPlayer) != null && BattleMgr.XorShiftRandom(AddToDeckCardList.First().IsPlayer).IsActive && BattleMgr.IsMulliganEnd)
		{
			for (int i = 0; i < AddToDeckCardList.Count; i++)
			{
				if (AddToDeckCardList[i].IsInDeck)
				{
					int changeInt = BattleMgr.XorShiftRandom(AddToDeckCardList[i].IsPlayer).GetChangeInt(DeckCardList.Count());
					BattleCardBase battleCardBase = DeckCardList[changeInt];
					int index = AddToDeckCardList[i].Index;
					AddToDeckCardList[i].SetIndex(battleCardBase.Index);
					this.OnIndexChange.Call(index, battleCardBase.Index, IsPlayer);
					battleCardBase.SetIndex(index);
					DeckCardList.Sort((BattleCardBase a, BattleCardBase b) => a.Index - b.Index);
				}
			}
		}
		AddToDeckCardList.Clear();
		if (_opponentBattlePlayer.AddToDeckCardList.Count > 0)
		{
			_opponentBattlePlayer.AddToDeckCardIndexChange();
		}
	}

	public VfxBase ReplaceInPlay(BattleCardBase originalCard, BattleCardBase newCard, SkillProcessor skillProcessor, bool isMetamorphose = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		ClassAndInPlayCardList.Insert(ClassAndInPlayCardList.IndexOf(originalCard), newCard);
		ClassAndInPlayCardList.Remove(originalCard);
		sequentialVfxPlayer.Register(originalCard.RemoveFromInPlay());
		sequentialVfxPlayer.Register(originalCard.RemoveFromInPlayAfter(skillProcessor));
		sequentialVfxPlayer.Register(StartSkillWhenChangeInplay(null, new List<BattleCardBase> { newCard }, skillProcessor, !isMetamorphose, null, null, isReplace: true));
		if (isMetamorphose)
		{
			this.OnMetamorphoseAfterEvent.Call(originalCard, newCard);
		}
		return sequentialVfxPlayer;
	}

	public VfxBase ReplaceInHand(BattleCardBase originalCard, BattleCardBase newCard, SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		HandCardList.Insert(HandCardList.IndexOf(originalCard), newCard);
		HandCardList.Remove(originalCard);
		StartSkillWhenChangeInplaySelfHand(new List<BattleCardBase> { newCard }, skillProcessor);
		return result;
	}

	private void CallSkill(Func<IBattlePlayerSkill, Func<BattleCardBase, VfxBase>> getFunc, BattleCardBase targetCard)
	{
		foreach (IBattlePlayerSkill skill in _skillList)
		{
			getFunc(skill)(targetCard);
		}
	}

	public virtual VfxBase UsePp(int pp, bool isNewReplayMoveTurn = false)
	{
		Pp -= pp;
		AddGameUsedPp(pp);
		return NullVfx.GetInstance();
	}

	public void AddGameUsedPp(int pp)
	{
		GameUsedPpCount += pp;
	}

	public VfxBase UseBp(int bp, bool isVariableCost, bool isSelf)
	{
		Bp -= bp;
		int bp2 = Bp;
		return m_vfxCreator.CreateUseBp(bp2, bp, () => BattleView.GetBPLabelPosition(), isVariableCost, isSelf);
	}

	public virtual VfxBase MoveToHand(List<BattleCardBase> cardsToMoveToHand)
	{
		return NullVfx.GetInstance();
	}

	protected virtual VfxWith<IEnumerable<BattleCardBase>> LotteryRandomDrawCard(int drawCount, SkillProcessor skillProcessor)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		if (CheckShortageDeck(drawCount, skillProcessor, out var _))
		{
			return new VfxWith<IEnumerable<BattleCardBase>>(SendShortageDeck(), list);
		}
		if (this.BattleMgr.InstanceIsRandomDraw)
		{
			list = SkillRandomSelectFilter.Filtering(drawCount, DeckCardList, BattleMgr).ToList();
		}
		else
		{
			int num = Math.Min(drawCount, DeckCardList.Count);
			for (int i = 0; i < num; i++)
			{
				list.Add(DeckCardList[i]);
			}
		}
		return new VfxWith<IEnumerable<BattleCardBase>>(NullVfx.GetInstance(), list);
	}

	public VfxWith<IEnumerable<BattleCardBase>> RandomCardDraw(int drawCount, SkillProcessor skillProcessor)
	{
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = LotteryRandomDrawCard(drawCount, skillProcessor);
		VfxWith<IEnumerable<BattleCardBase>> vfxWith2 = DrawCards(vfxWith.Value, skillProcessor);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(vfxWith2.Vfx);
		sequentialVfxPlayer.Register(vfxWith.Vfx);
		return new VfxWith<IEnumerable<BattleCardBase>>(sequentialVfxPlayer, vfxWith2.Value);
	}

	public VfxWith<IEnumerable<BattleCardBase>> DrawManagement(List<BattleCardBase> drawCards, SkillProcessor skillProcessor, bool isVisible, bool shortageDeck, SkillBase.SkillResultInfo resultInfo, SkillBase skill)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = DrawCards(drawCards, skillProcessor, isVisible, isMulligan: false, isToken: false, skill != null, skill, isReservation: false, resultInfo);
		if (vfxWith.Value.Count() <= 0 && !shortageDeck)
		{
			resultInfo.drawCards = new List<IReadOnlyBattleCardInfo>();
			return new VfxWith<IEnumerable<BattleCardBase>>(NullVfxWithLoading.GetInstance(), new List<BattleCardBase>());
		}
		if (resultInfo != null)
		{
			resultInfo.drawCards = ConvertToSkillInfoCollection(vfxWith.Value);
		}
		if (IsPlayer || BattleMgr.GameMgr.IsAdminWatch || isVisible || BattleMgr is SingleBattleMgr)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			foreach (BattleCardBase card in drawCards)
			{
				int cost = card.Cost;
				List<int> costList = card.BattleCardView.GetUseCostList(card.Cost);
				bool isInHand = card.IsInHand;
				parallelVfxPlayer.Register(InstantVfx.Create(delegate
				{
					if (card.BaseCost != cost)
					{
						card.BattleCardView.UpdateCost(costList, isGenerateInHand: true, playEffect: true, isInHand);
					}
				}));
			}
			sequentialVfxPlayer.Register(parallelVfxPlayer);
		}
		if (IsPlayer)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		else
		{
			if (!(BattleMgr.GameMgr.IsAdminWatch && isVisible))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToMainVfx(InstantVfx.Create(delegate
		{
			foreach (BattleCardBase drawCard in drawCards)
			{
				drawCard.BattleCardView.HideCanPlayEffect();
			}
		}));
		vfxWithLoadingSequential.RegisterToMainVfx(vfxWith.Vfx);
		vfxWithLoadingSequential.RegisterToMainVfx(sequentialVfxPlayer);
		if (shortageDeck)
		{
			vfxWithLoadingSequential.RegisterToMainVfx(SendShortageDeck());
		}
		vfxWithLoadingSequential.RegisterToMainVfx(InstantVfx.Create(delegate
		{
			UpdateHandCardsPlayability();
		}));
		return new VfxWith<IEnumerable<BattleCardBase>>(vfxWithLoadingSequential, vfxWith.Value);
	}

	public VfxWith<IEnumerable<BattleCardBase>> DrawCards(IEnumerable<BattleCardBase> drawList, SkillProcessor skillProcessor, bool isOpen = false, bool isMulligan = false, bool isToken = false, bool isSkillDraw = false, SkillBase skill = null, bool isReservation = false, SkillBase.SkillResultInfo skillResultInfo = null, int tokenDrawSkillId = -1)
	{
		if (skillResultInfo != null)
		{
			skillResultInfo.drewOverHandLimitCards = new List<IReadOnlyBattleCardInfo>();
		}
		if (!drawList.Any())
		{
			return new VfxWith<IEnumerable<BattleCardBase>>(NullVfx.GetInstance(), drawList);
		}
		int count = DeckCardList.Count;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase card in drawList)
		{
			SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.Add(card);
			bool num = HandCardList.Count >= 9;
			if (num)
			{
				list.RemoveAt(0);
				this.OnAddCemeteryEvent.Call(card, CEMETERY_TYPE.DECK_DRAW_HAND_OVER, isOpen, skill);
				CemeteryList.Add(card);
				skillConditionCheckerOption.DrewOverHandLimitCards.Add(card);
				skillResultInfo?.drewOverHandLimitCards.Add(card);
				BattleMgr.VfxMgr.RegisterImmediateVfx(this.OnAddCemeteryAfterEvent.GetAllFuncVfxResults());
				int cost = card.Cost;
				List<int> costList = card.BattleCardView.GetUseCostList(card.Cost);
				parallelVfxPlayer.Register(InstantVfx.Create(delegate
				{
					if (card.BaseCost != cost)
					{
						card.BattleCardView.UpdateCost(costList, isGenerateInHand: true, playEffect: true, isForceUpdate: true);
					}
				}));
			}
			skillConditionCheckerOption.InHandCard = ConvertToSkillInfoCollection(list);
			parallelVfxPlayer.Register(DrawCard(card, skill, isOpen, isToken, isReservation));
			parallelVfxPlayer2.Register(card.StartHandEffect());
			if (!isMulligan)
			{
				StartSkillWhenChangeInplaySelfHand(new List<BattleCardBase> { card }, skillProcessor);
			}
			if (!num)
			{
				StartSkillWhenAddToHand(skillProcessor, skillConditionCheckerOption);
			}
		}
		if (!isToken)
		{
			CallOnChangeDeckAfterEvent(count, skillProcessor, new List<BattleCardBase>());
		}
		sequentialVfxPlayer.Register(SequentialVfxPlayer.Create(parallelVfxPlayer, parallelVfxPlayer2));
		if (!isMulligan)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
			SkillConditionCheckerOption skillConditionCheckerOption2 = new SkillConditionCheckerOption();
			if (skill != null)
			{
				DrewSkillCard = skill.SkillPrm.ownerCard;
			}
			List<BattleCardBase> list2 = new List<BattleCardBase>();
			List<BattleCardBase> list3 = new List<BattleCardBase>();
			if (isToken)
			{
				list2.AddRange(drawList);
				skillConditionCheckerOption2.TokenDrewCards.AddRange(list2);
			}
			else
			{
				list3.AddRange(drawList);
				skillConditionCheckerOption2.DeckDrewCards.AddRange(list3);
			}
			foreach (BattleCardBase item in drawList.Where((BattleCardBase c) => c.IsInHand))
			{
				skillProcessor.Register(item.Skills.CreateWhenDraw(skillProcessor, playerInfoPair, skillConditionCheckerOption2));
				item.DrawTurn = ((item.SelfBattlePlayer.IsSelfTurn && !BattleMgr.IsTurnEnd) ? item.SelfBattlePlayer.Turn : (item.SelfBattlePlayer.Turn + 1));
			}
			if (!IsPlayer)
			{
				IEnumerable<BattleCardBase> enumerable = drawList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenDraw != 0 && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard)));
				foreach (BattleCardBase item2 in enumerable)
				{
					item2.LastDrawOpenCard = enumerable.LastOrDefault();
				}
			}
			List<BattleCardBase> list4 = new List<BattleCardBase>(HandCardList);
			list4.AddRange(ClassAndInPlayCardList);
			foreach (BattleCardBase item3 in list4.Where((BattleCardBase c) => !drawList.Contains(c)))
			{
				SkillProcessor.ProcessInfo info = item3.Skills.CreateWhenDrawOther(list3, list2, skillProcessor, new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer), isSkillDraw);
				skillProcessor.Register(info);
			}
			if (!isToken)
			{
				int count2 = TurnDrawCards.Count;
				TurnDrawCards.AddRange(drawList);
				GameDrawCards.AddRange(drawList);
				this.OnDrawCards.Call(count2, TurnDrawCards.Count, drawList.ToList(), this, isOpen);
			}
			else
			{
				GameDrawTokenCards.AddRange(drawList);
				if (tokenDrawSkillId != -1)
				{
					for (int num2 = 0; num2 < drawList.Count(); num2++)
					{
						TurnDrawTokenCardsWithId.Add(new CardAndId(drawList.ElementAt(num2), tokenDrawSkillId));
					}
				}
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(VfxWithLoading.Create(BattleMgr.LoadCardResources(drawList.ToList())));
		vfxWithLoadingSequential.RegisterToMainVfx(sequentialVfxPlayer);
		return new VfxWith<IEnumerable<BattleCardBase>>(vfxWithLoadingSequential, drawList);
	}

	public bool CheckShortageDeck(int drawNum, SkillProcessor skillProcessor, out bool isActiveChangeShortageDeck)
	{
		bool flag = drawNum > DeckCardList.Count;
		isActiveChangeShortageDeck = false;
		if (!IsChangeShortageDeck)
		{
			IsShortageDeck = flag;
			return IsShortageDeck;
		}
		if (IsChangeShortageDeck && flag)
		{
			StartSkillWhenShortageDeck(skillProcessor);
			isActiveChangeShortageDeck = true;
		}
		return false;
	}

	public void CallRecordingMulliganStart(List<BattleCardBase> cards)
	{
		this.OnMulliganStart.Call(this, cards);
	}

	public virtual VfxBase CallRecordingMulligan(IList<BattleCardBase> abandonCards, IList<int> completeCards)
	{
		this.OnMulliganEnd.Call(abandonCards, completeCards);
		return NullVfx.GetInstance();
	}

	protected virtual void PlayerActive()
	{
	}

	public virtual void UpdateHandCardsPlayability(bool areArrowsForcedOff = false)
	{
	}

	public VfxBase UpdateHandCardsCost(bool playEffect = true, bool isOnlyFixedUseCost = false)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		int i = 0;
		for (int count = HandCardList.Count; i < count; i++)
		{
			parallelVfxPlayer.Register(HandCardList[i].CalcHandCost(playEffect, isOnlyFixedUseCost));
		}
		return parallelVfxPlayer;
	}

	public VfxWithLoadingSequential AddSpellChargeCountVfx(List<BattleCardBase> targetCardList, List<int> addCountList)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<int> list2 = new List<int>();
		List<float> list3 = new List<float>();
		for (int i = 0; i < targetCardList.Count; i++)
		{
			BattleCardBase battleCardBase = targetCardList[i];
			int num = addCountList[i];
			battleCardBase.AddSpellChargeCount(num);
			if ((!battleCardBase.IsPlayer && !BattleMgr.GameMgr.IsAdminWatch) || battleCardBase.IsInDeck)
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				list.Add(battleCardBase);
				if (num >= Skill_spell_charge.SPELL_CHARGE_SUMMARY_COUNT)
				{
					list2.Add(num);
					list3.Add(Skill_spell_charge.SPELL_CHARGE_SUMMARY_INTERVAL);
					break;
				}
				list2.Add(1);
				list3.Add(Skill_spell_charge.SPELL_CHARGE_INTERVAL);
			}
		}
		return VfxWithLoadingSequential.Create();
	}

	public abstract EffectBattle GetSkillEffect(string skillEffectPath);

	public abstract Vector3 GetFieldCenterPosition();

	public void StartSkillWhenChangeInplaySelfHand(List<BattleCardBase> inHandCards, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			cardsOrderBySkillActivation[i].Skills.CreateAndRegisterWhenChangeInplaySelfhandInfo(inHandCards, skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2);
		}
	}

	public VfxBase StartSkillWhenChangeInplay(List<BattleCardBase> inHandCards, List<BattleCardBase> inPlayCards, SkillProcessor skillProcessor, bool isSummonCheck = true, Func<SkillBase, uint> inplayCheckFunc = null, SkillConditionCheckerOption option = null, bool isReplace = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		BattlePlayerPair battlePlayerPair2 = new BattlePlayerPair(_opponentBattlePlayer, this);
		if (inPlayCards != null && !isReplace && isSummonCheck)
		{
			sequentialVfxPlayer.Register(this.OnAfterSummonCardEvent.GetAllFuncVfxResults(skillProcessor, inPlayCards));
		}
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true);
		List<BattleCardBase> cardsOrderBySkillActivation2 = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: true, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation2.Count; i++)
		{
			sequentialVfxPlayer.Register(cardsOrderBySkillActivation2[i].Skills.RegisterAndProcessWhenChangeInplayImmediateInfo((cardsOrderBySkillActivation2[i].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2));
		}
		for (int j = 0; j < cardsOrderBySkillActivation.Count; j++)
		{
			cardsOrderBySkillActivation[j].Skills.CreateAndRegisterWhenChangeInplaySelfhandInfo(inHandCards, skillProcessor, (cardsOrderBySkillActivation[j].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2);
		}
		for (int k = 0; k < cardsOrderBySkillActivation2.Count; k++)
		{
			cardsOrderBySkillActivation2[k].Skills.CreateAndRegisterWhenChangeInplayInfo(inPlayCards, skillProcessor, (cardsOrderBySkillActivation2[k].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2, isSummonCheck, inplayCheckFunc, option);
		}
		return sequentialVfxPlayer;
	}

	public VfxBase StartSkillWhenChangeClassLife(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		BattlePlayerPair battlePlayerPair2 = new BattlePlayerPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true);
		List<BattleCardBase> cardsOrderBySkillActivation2 = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: false, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			cardsOrderBySkillActivation[i].Skills.CreateAndRegisterWhenChangeClassLifeSelfHandInfo(skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2);
		}
		for (int j = 0; j < cardsOrderBySkillActivation2.Count; j++)
		{
			cardsOrderBySkillActivation2[j].Skills.CreateAndRegisterWhenChangeClassLifeInplayInfo(skillProcessor, (cardsOrderBySkillActivation2[j].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2);
		}
		return result;
	}

	public VfxBase StartSkillWhenChangePPTotal(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer result = SequentialVfxPlayer.Create();
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		BattlePlayerPair battlePlayerPair2 = new BattlePlayerPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: false, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			cardsOrderBySkillActivation[i].Skills.CreateAndRegisterWhenChangePPTotalInfo(skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2);
		}
		return result;
	}

	public void StartSkillWhenAddToHand(SkillProcessor skillProcessor, SkillConditionCheckerOption option)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, BattleMgr is SingleBattleMgr, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			cardsOrderBySkillActivation[i].Skills.CreateAndRegisterWhenAddToHandInfo(skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, option);
		}
	}

	public void StartSkillWhenDestroyOther(BattleCardBase destroyCard, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: true, containsHand: false, containsClass: false, containsInplay: false, containsDeck: false, (BattleCardBase card) => card != destroyCard);
		for (int num = 0; num < cardsOrderBySkillActivation.Count; num++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateWhenDestroyOtherInfo(destroyCard, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2));
		}
	}

	public void StartSkillWhenReturnOther(BattleCardBase returnedCard, SkillProcessor skillProcessor, List<IReadOnlyBattleCardInfo> cantAttackAllReturnCards)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true, containsDeck: false, (BattleCardBase card) => card != returnedCard);
		for (int num = 0; num < cardsOrderBySkillActivation.Count; num++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateWhenReturnOtherInfo(returnedCard, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, cantAttackAllReturnCards));
		}
	}

	public void StartSkillWhenReturnSkillActivate(List<BattleCardBase> returnedCards, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenReturnSkillActivateInfo(returnedCards, skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2));
		}
	}

	public VfxBase StartSkillWhenPlayOtherEnhanceAndAccelerateAndCrystallize(BattleCardBase playedCard, bool isEnhance, SkillProcessor skillProcessor)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (playedCard.IsChoiceBraveSkillCard)
		{
			return parallelVfxPlayer;
		}
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		foreach (BattleCardBase item in SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: true, containsHand: false, containsClass: false, containsInplay: false, containsDeck: false, (BattleCardBase card) => card != playedCard))
		{
			foreach (SkillBase skill in item.Skills)
			{
				VfxWith<SkillProcessor.ProcessInfo> vfxWith = item.Skills.CreateWhenPlayOtherEnhanceAndAccelerateAndCrystallizeInfo(skill, playedCard, isEnhance ? playedCard : null, skillProcessor, (item.SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2);
				skillProcessor.Register(vfxWith.Value);
				parallelVfxPlayer.Register(vfxWith.Vfx);
			}
		}
		return parallelVfxPlayer;
	}

	public VfxBase StartSkillWhenSummonOther(BattleCardBase summonedCard, SkillProcessor skillProcessor, bool isReanimate = false, List<BattleCardBase> ignoreCheckCard = null)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true, containsDeck: false, (BattleCardBase card) => ignoreCheckCard == null || ignoreCheckCard.Count == 0 || !ignoreCheckCard.Contains(card));
		for (int num = 0; num < cardsOrderBySkillActivation.Count; num++)
		{
			if (cardsOrderBySkillActivation[num] != summonedCard)
			{
				skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateWhenSummonOtherInfo(summonedCard, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, isReanimate));
			}
			skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateWhenSummonSelfAndOtherInfo(summonedCard, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, isReanimate));
		}
		return NullVfx.GetInstance();
	}

	public void StartSkillWhenFusionOther(List<BattleCardBase> fusionIngredientCards, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(ClassAndInPlayCardList);
		foreach (BattleCardBase item in list)
		{
			skillProcessor.Register(item.Skills.CreateWhenFusionOtherInfo(skillProcessor, playerInfoPair, fusionIngredientCards));
		}
	}

	public void StartSkillWhenUseEpSelfAndOther(SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenUseEpSelfAndOtherInfo(skillProcessor, playerInfoPair));
		}
	}

	public VfxBase StartSkillWhenHealingSelfAndOther(List<BattleCardBase> healingCards, SkillProcessor skillProcessor, List<int> healAmountList)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true);
		if (cardsOrderBySkillActivation.Any())
		{
			TurnAndIntValue turnAndIntValue = TurnWhenHealingCount.FirstOrDefault((TurnAndIntValue t) => t.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && t.Turn == Turn);
			if (turnAndIntValue != null)
			{
				turnAndIntValue.Increment();
			}
			else
			{
				TurnWhenHealingCount.Add(new TurnAndIntValue(1, Turn, BattleMgr.BattlePlayer.IsSelfTurn));
			}
		}
		for (int num = 0; num < cardsOrderBySkillActivation.Count; num++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[num].Skills.CreateWhenHealingSelfAndOtherInfo(healingCards, skillProcessor, (cardsOrderBySkillActivation[num].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, healAmountList));
		}
		return NullVfx.GetInstance();
	}

	public VfxBase StartSkillWhenDamageSelfAndOther(SkillBase skill, List<BattleCardBase> damageCards, SkillProcessor skillProcessor, int defDamage, int fixedDamage)
	{
		if (damageCards != null)
		{
			BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
			BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
			List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true, containsDeck: true);
			for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
			{
				skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenDamageSelfAndOtherInfo(skill, damageCards, skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, defDamage, fixedDamage));
			}
		}
		return NullVfx.GetInstance();
	}

	public void StartSkillWhenBurialRiteOther(BattleCardBase burialRiteCard, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: true, containsInplay: true, containsDeck: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenBurialRiteOther(burialRiteCard, skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2));
		}
	}

	public void StartSkillWhenBanishOther(BattleCardBase banishedCard, SkillProcessor skillProcessor, bool isInplay)
	{
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		BattlePlayerReadOnlyInfoPair battlePlayerReadOnlyInfoPair2 = new BattlePlayerReadOnlyInfoPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: true, containsClass: true, containsInplay: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenBanishOther(banishedCard, skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerReadOnlyInfoPair : battlePlayerReadOnlyInfoPair2, isInplay));
		}
	}

	public void StartSkillWhenUseWhiteRitualStack(SkillProcessor skillProcessor, SkillConditionCheckerOption checkerOption)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		List<BattleCardBase> list = HandCardList.ToList();
		list.AddRange(ClassAndInPlayCardList);
		list.AddRange(DeckSkillCardList);
		for (int i = 0; i < list.Count; i++)
		{
			skillProcessor.Register(list[i].Skills.CreateWhenUseWhiteRitualStackInfo(skillProcessor, playerInfoPair, checkerOption));
		}
	}

	public void StartSkillWhenResonanceStart(SkillProcessor skillProcessor, List<BattleCardBase> SummonCardList)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		List<BattleCardBase> list = HandCardList.ToList();
		list.AddRange(ClassAndInPlayCardList);
		list.AddRange(DeckSkillCardList);
		for (int i = 0; i < list.Count; i++)
		{
			BattleCardBase card = list[i];
			if (!SummonCardList.Any((BattleCardBase s) => s == card))
			{
				skillProcessor.Register(card.Skills.CreateWhenResonanceStart(skillProcessor, playerInfoPair));
			}
		}
	}

	public VfxBase StartSkillWhenPpHealing(SkillProcessor skillProcessor)
	{
		List<BattleCardBase> list = ClassAndInPlayCardList.ToList();
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		for (int i = 0; i < list.Count; i++)
		{
			skillProcessor.Register(list[i].Skills.CreateWhenPpHealingInfo(skillProcessor, playerInfoPair));
		}
		return NullVfx.GetInstance();
	}

	public void StartSkillWhenBuffDebuffSelfAndOther(IEnumerable<BattleCardBase> targetCards, IEnumerable<BattleCardBase> inplayTargetCards, SkillProcessor skillProcessor)
	{
		if (targetCards.Count() != 0)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
			List<BattleCardBase> list = ClassAndInPlayCardList.ToList();
			SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
			List<IReadOnlyBattleCardInfo> inplayDebuffingCards = (skillConditionCheckerOption.InplayBuffingCards = ConvertToSkillInfoCollection(inplayTargetCards));
			skillConditionCheckerOption.InplayDebuffingCards = inplayDebuffingCards;
			for (int i = 0; i < list.Count; i++)
			{
				skillProcessor.Register(list[i].Skills.CreateWhenBuffDebuffSelfAndOtherInfo(skillProcessor, playerInfoPair, skillConditionCheckerOption));
			}
		}
	}

	public void StartSkillWhenBuffSelfAndOther(IEnumerable<BattleCardBase> buffingCards, IEnumerable<BattleCardBase> inplayBuffingCards, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		List<BattleCardBase> list = ClassAndInPlayCardList.ToList();
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.InplayBuffingCards = ConvertToSkillInfoCollection(inplayBuffingCards);
		if (buffingCards.Count() > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				skillProcessor.Register(list[i].Skills.CreateWhenBuffSelfAndOtherInfo(skillProcessor, playerInfoPair, skillConditionCheckerOption));
			}
		}
	}

	public void StartSkillWhenDebuffSelfAndOther(IEnumerable<BattleCardBase> debuffingCards, IEnumerable<BattleCardBase> inplayDebuffingCards, SkillProcessor skillProcessor)
	{
		if (debuffingCards.Count() != 0)
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
			List<BattleCardBase> list = ClassAndInPlayCardList.ToList();
			SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
			skillConditionCheckerOption.InplayDebuffingCards = ConvertToSkillInfoCollection(inplayDebuffingCards);
			for (int i = 0; i < list.Count; i++)
			{
				skillProcessor.Register(list[i].Skills.CreateWhenDebuffSelfAndOtherInfo(skillProcessor, playerInfoPair, skillConditionCheckerOption));
			}
		}
	}

	public void StartSkillWhenDebuffIncludeSetMaxLife(BattleCardBase debuffingCard, IEnumerable<BattleCardBase> inplayDebuffingCards, SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.InplayDebuffingCards = ConvertToSkillInfoCollection(inplayDebuffingCards);
		skillProcessor.Register(debuffingCard.Skills.CreateWhenDebuffIncludeSetMaxLifeInfo(skillProcessor, playerInfoPair, skillConditionCheckerOption));
	}

	public void StartSkillWhenShortageDeck(SkillProcessor skillProcessor)
	{
		BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(this, _opponentBattlePlayer);
		skillProcessor.Register(Class.Skills.CreateWhenShortageDeck(skillProcessor, playerInfoPair));
	}

	public void StartSkillWhenShortageDeckWinSkillActivate(List<BattleCardBase> shortageDeckWinCards, SkillProcessor skillProcessor)
	{
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		BattlePlayerPair battlePlayerPair2 = new BattlePlayerPair(_opponentBattlePlayer, this);
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.ShortageDeckWinCards = ConvertToSkillInfoCollection(shortageDeckWinCards);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenShortageDeckWinSkillActivate(skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2, skillConditionCheckerOption));
		}
	}

	public VfxBase StartSkillWhenBattleStart(SkillProcessor skillProcessor)
	{
		BattlePlayerPair battlePlayerPair = new BattlePlayerPair(this, _opponentBattlePlayer);
		BattlePlayerPair battlePlayerPair2 = new BattlePlayerPair(_opponentBattlePlayer, this);
		List<BattleCardBase> cardsOrderBySkillActivation = SkillCollectionBase.GetCardsOrderBySkillActivation(this, _opponentBattlePlayer, isAll: false, containsHand: false, containsClass: true);
		for (int i = 0; i < cardsOrderBySkillActivation.Count; i++)
		{
			skillProcessor.Register(cardsOrderBySkillActivation[i].Skills.CreateWhenBattleStartInfo(skillProcessor, (cardsOrderBySkillActivation[i].SelfBattlePlayer == this) ? battlePlayerPair : battlePlayerPair2));
		}
		return skillProcessor.Process(battlePlayerPair);
	}

	public static List<IReadOnlyBattleCardInfo> ConvertToSkillInfoCollection(IEnumerable<BattleCardBase> cards)
	{
		if (cards == null)
		{
			return new List<IReadOnlyBattleCardInfo>();
		}
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		foreach (BattleCardBase card in cards)
		{
			list.Add(card);
		}
		return list;
	}

	public static List<IEnumerable<IReadOnlyBattleCardInfo>> ConvertToSkillInfoCollectionList(List<List<BattleCardBase>> cardsList)
	{
		if (cardsList == null)
		{
			return new List<IEnumerable<IReadOnlyBattleCardInfo>>();
		}
		List<IEnumerable<IReadOnlyBattleCardInfo>> list = new List<IEnumerable<IReadOnlyBattleCardInfo>>();
		foreach (List<BattleCardBase> cards in cardsList)
		{
			List<IReadOnlyBattleCardInfo> item = cards.ToList().ConvertAll<IReadOnlyBattleCardInfo>(ConvertIReadOnlyBattleCardInfo);
			list.Add(item);
		}
		return list;
	}

	public static IReadOnlyBattleCardInfo ConvertIReadOnlyBattleCardInfo(BattleCardBase card)
	{
		return card;
	}

	public void AddLastTargetCardsList(BattleCardBase addCard)
	{
		if (LastTargetCardsList.Count > 0)
		{
			LastTargetCardsList.First().Add(addCard);
			return;
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<BattleCardBase> item = new List<BattleCardBase>();
		list.Add(addCard);
		LastTargetCardsList.Add(list);
		_opponentBattlePlayer.LastTargetCardsList.Add(item);
	}

	public List<BattleCardBase> GetLastTargetCardsList(int index)
	{
		if (0 <= index && index < LastTargetCardsList.Count)
		{
			return LastTargetCardsList[index];
		}
		return new List<BattleCardBase>();
	}

	public void SkillsEndProcess()
	{
		ReturnList.Clear();
		LastTargetCardsList.Clear();
		InHandCards.Clear();
		HealingCards.Clear();
		SkillSummonedCards.Clear();
		SummonedCards.Clear();
		SkillDiscards.Clear();
		SkillBanishCards.Clear();
		DrewSkillCard = null;
		OkSkillInProcess.Clear();
		LastInplayWhiteRitualStack = 0;
		Class.SkillApplyInformation.ReservationAllDepriveRepeatSkill();
	}

	public void OnCallOneSkillProcess()
	{
		if (OnEndOneSkillProcess != null)
		{
			OnEndOneSkillProcess();
			OnEndOneSkillProcess = null;
		}
	}

	public VfxBase SendShortageDeck()
	{
		return this.OnShortageDeck.GetAllFuncVfxResults();
	}

	public void CopyToVirtualBase(BattlePlayerBase target, BattlePlayerBase virtualOpponentBattlePlayer, CloneActualFlags cloneFlags)
	{
		target._opponentBattlePlayer = virtualOpponentBattlePlayer;
		target._skillList = _skillList.ToList();
		target.Pp = Pp;
		target._ppTotal = _ppTotal;
		target.SetCurrentEpCount(CurrentEpCount);
		target.EvolveWaitTurnCount = EvolveWaitTurnCount;
		target.NowTurnEvol = NowTurnEvol;
		target._gameUsedEpCount = _gameUsedEpCount;
		target._turnUsedEpCount = _turnUsedEpCount;
		target.HandCardList = CloneCardList(HandCardList, target, virtualOpponentBattlePlayer, cloneFlags.Hand);
		target.DeckCardList = CloneCardList(DeckCardList, target, virtualOpponentBattlePlayer, cloneFlags.Deck);
		target.BattleStartDeckCardList = CloneCardList(BattleStartDeckCardList, target, virtualOpponentBattlePlayer, cloneFlags.Deck);
		target.ClassAndInPlayCardList = CloneCardList(ClassAndInPlayCardList, target, virtualOpponentBattlePlayer, cloneFlags.InPlay);
		target.CemeteryList = CloneCardList(CemeteryList, target, virtualOpponentBattlePlayer, cloneFlags.Cemetery);
		if (target.ClassAndInPlayCardList.Count > 0 && target.ClassAndInPlayCardList[0] is ClassBattleCardBase)
		{
			target._class = target.ClassAndInPlayCardList[0];
		}
		else
		{
			BattleCardBase battleCardBase = target.CemeteryList.FirstOrDefault((BattleCardBase c) => c is ClassBattleCardBase);
			if (battleCardBase != null)
			{
				target._class = battleCardBase;
			}
			else
			{
				target._class = _class.VirtualClone(target, virtualOpponentBattlePlayer);
			}
		}
		target.BanishList = CloneCardList(BanishList, target, virtualOpponentBattlePlayer, cloneFlags.Banish);
		target.FusionIngredientList = CloneCardList(FusionIngredientList, target, virtualOpponentBattlePlayer, cloneFlags.FusionMaterial);
		target.NecromanceZoneList = CloneCardList(NecromanceZoneList, target, virtualOpponentBattlePlayer, cloneFlags.NecromanceZone);
		target.UniteList = CloneCardList(UniteList, target, virtualOpponentBattlePlayer, cloneFlags.Unite);
		target.GetOnList = CloneCardList(GetOnList, target, virtualOpponentBattlePlayer, cloneFlags.GetOn);
		target.SummonedCards = CloneCardList(SummonedCards, target, virtualOpponentBattlePlayer, isActualClone: true);
		target.IsSelfTurn = IsSelfTurn;
		target.DrewSkillCard = DrewSkillCard;
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(target.HandCardList);
		list.AddRange(target.ClassAndInPlayCardList);
		list.AddRange(target.DeckCardList);
		list.AddRange(target.CemeteryList);
		list.AddRange(target.BanishList);
		list.AddRange(target.NecromanceZoneList);
		list.AddRange(target.FusionIngredientList);
		list.AddRange(target.UniteList);
		list.AddRange(target.GetOnList);
		target.ReturnList = FindClonedIdCards(list, ReturnList);
		List<List<BattleCardBase>> list2 = new List<List<BattleCardBase>>();
		foreach (List<BattleCardBase> lastTargetCards in LastTargetCardsList)
		{
			list2.Add(FindClonedIdCards(list, lastTargetCards));
		}
		target.LastTargetCardsList = list2;
		target.InHandCards = FindClonedIdCards(list, InHandCards);
		target.SkillDiscards = FindClonedIdCards(list, SkillDiscards);
		target.SkillBanishCards = FindClonedIdCards(list, SkillBanishCards);
		target.TurnPlayCards = FindClonedIdCards(list, TurnPlayCards);
		target.TurnDrawCards = FindClonedIdCards(list, TurnDrawCards);
		target.TurnDrawTokenCardsWithId = new List<CardAndId>();
		int i;
		for (i = 0; i < TurnDrawTokenCardsWithId.Count; i++)
		{
			target.TurnDrawTokenCardsWithId.Add(new CardAndId(list.FirstOrDefault((BattleCardBase c) => c.EquelsID(TurnDrawTokenCardsWithId[i].Card)), TurnDrawTokenCardsWithId[i].Id));
		}
		target.GamePlayCards = FindClonedIdCards(list, GamePlayCards);
		target.GameCrystallizedPlayCards = FindClonedIdCards(list, GameCrystallizedPlayCards);
		target.OkSkillInProcess = OkSkillInProcess.ToList();
		target.GameInplayMetamorphoseCards = FindClonedIdCards(list, GameInplayMetamorphoseCards);
		target.GameBurialRiteCards = FindClonedIdCards(list, GameBurialRiteCards);
		target.TurnBurialRiteCards = FindClonedIdCards(list, TurnBurialRiteCards);
		target.EvolvedCards = FindClonedIdCards(AllCards, EvolvedCards);
		target.GameDrawCards = FindClonedIdCards(list, GameDrawCards);
		target.GameDrawTokenCards = FindClonedIdCards(list, GameDrawTokenCards);
		target.GameAddUpdateDeckCards = FindClonedIdCards(list, GameAddUpdateDeckCards);
		target.GameLeftCards = FindClonedIdCards(list, GameLeftCards);
		target.GameSuperSkyboundArtCards = FindClonedIdCards(list, GameSuperSkyboundArtCards);
		List<BattleCardBase> list3 = FindClonedIdCards(list, GameTurnLeftCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num = 0; num < list3.Count; num++)
		{
			target.GameTurnLeftCards.Add(new TurnAndCard(GameTurnLeftCards[num].Turn, GameTurnLeftCards[num].IsSelfTurn, list3[num], GameTurnLeftCards[num].IsTurnEnd));
		}
		List<BattleCardBase> list4 = FindClonedIdCards(list, GameSummonCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num2 = 0; num2 < list4.Count; num2++)
		{
			target.GameSummonCards.Add(new TurnAndCard(GameSummonCards[num2].Turn, GameSummonCards[num2].IsSelfTurn, list4[num2], GameSummonCards[num2].IsTurnEnd));
		}
		for (int num3 = 0; num3 < GameSummonMomentTribe.Count; num3++)
		{
			target.GameSummonMomentTribe.Add(new CardAndTribe(GameSummonMomentTribe[num3].Card, GameSummonMomentTribe[num3].Tribes));
		}
		for (int num4 = 0; num4 < GamePlayMomentTribe.Count; num4++)
		{
			target.GamePlayMomentTribe.Add(new CardAndTribe(GamePlayMomentTribe[num4].Card, GamePlayMomentTribe[num4].Tribes));
		}
		for (int num5 = 0; num5 < GamePlayMomentSpellChargeCards.Count; num5++)
		{
			target.GamePlayMomentSpellChargeCards.Add(GamePlayMomentSpellChargeCards[num5].Card);
		}
		for (int num6 = 0; num6 < GameUpdateDeckMomentTribe.Count; num6++)
		{
			target.GameUpdateDeckMomentTribe.Add(new CardAndTribe(GameUpdateDeckMomentTribe[num6].Card, GameUpdateDeckMomentTribe[num6].Tribes));
		}
		List<BattleCardBase> list5 = FindClonedIdCards(list, GameReanimatedCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num7 = 0; num7 < list5.Count; num7++)
		{
			target.GameReanimatedCards.Add(new TurnAndCard(GameReanimatedCards[num7].Turn, GameReanimatedCards[num7].IsSelfTurn, list5[num7], GameReanimatedCards[num7].IsTurnEnd));
		}
		List<BattleCardBase> list6 = FindClonedIdCards(list, GameReturnedCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num8 = 0; num8 < list6.Count; num8++)
		{
			target.GameReturnedCards.Add(new TurnAndCard(GameReturnedCards[num8].Turn, GameReturnedCards[num8].IsSelfTurn, list6[num8], GameReturnedCards[num8].IsTurnEnd));
		}
		List<BattleCardBase> list7 = FindClonedIdCards(list, GameTurnPlayCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num9 = 0; num9 < list7.Count; num9++)
		{
			target.GameTurnPlayCards.Add(new TurnAndCard(GameTurnPlayCards[num9].Turn, GameTurnPlayCards[num9].IsSelfTurn, list7[num9], GameTurnPlayCards[num9].IsTurnEnd));
		}
		List<BattleCardBase> list8 = FindClonedIdCards(list, GameEnhancePlayCards.Select((TurnAndCard c) => c.Card as BattleCardBase));
		for (int num10 = 0; num10 < list8.Count; num10++)
		{
			target.GameEnhancePlayCards.Add(new TurnAndCard(GameEnhancePlayCards[num10].Turn, GameEnhancePlayCards[num10].IsSelfTurn, list8[num10], GameEnhancePlayCards[num10].IsTurnEnd));
		}
		for (int num11 = 0; num11 < TurnPlayCardCountInfo.Count; num11++)
		{
			TurnAndIntValue turnAndIntValue = TurnPlayCardCountInfo[num11];
			target.TurnPlayCardCountInfo.Add(new TurnAndIntValue(turnAndIntValue.Value, turnAndIntValue.Turn, turnAndIntValue.IsSelfTurn));
		}
		for (int num12 = 0; num12 < TurnFusionCountInfo.Count; num12++)
		{
			TurnAndIntValue turnAndIntValue2 = TurnFusionCountInfo[num12];
			target.TurnFusionCountInfo.Add(new TurnAndIntValue(turnAndIntValue2.Value, turnAndIntValue2.Turn, turnAndIntValue2.IsSelfTurn));
		}
		target.extraTurnCount = extraTurnCount;
		target.cardTotalNum = cardTotalNum;
		for (int num13 = 0; num13 < TurnEvolveCardCountInfo.Count; num13++)
		{
			TurnAndIntValue turnAndIntValue3 = TurnEvolveCardCountInfo[num13];
			target.TurnEvolveCardCountInfo.Add(new TurnAndIntValue(turnAndIntValue3.Value, turnAndIntValue3.Turn, turnAndIntValue3.IsSelfTurn));
		}
		target.IsShortageDeck = IsShortageDeck;
		target.RallyCount = RallyCount;
		target.DeckBanishCount = DeckBanishCount;
		target.GameResonanceStartCount = GameResonanceStartCount;
		target.TurnResonanceStartCount = TurnResonanceStartCount;
		target.GameNecromanceCount = GameNecromanceCount;
		target.GameUsedPpCount = GameUsedPpCount;
		target.GameUsedWhiteRitualCount = GameUsedWhiteRitualCount;
		target.LastInplayWhiteRitualStack = LastInplayWhiteRitualStack;
		for (int num14 = 0; num14 < GameSkillReturnCardCountList.Count; num14++)
		{
			TurnAndIntValue turnAndIntValue4 = GameSkillReturnCardCountList[num14];
			target.GameSkillReturnCardCountList.Add(new TurnAndIntValue(turnAndIntValue4.Value, turnAndIntValue4.Turn, turnAndIntValue4.IsSelfTurn));
		}
		for (int num15 = 0; num15 < GameSkillDiscardCountList.Count; num15++)
		{
			TurnAndIntValue turnAndIntValue5 = GameSkillDiscardCountList[num15];
			target.GameSkillDiscardCountList.Add(new TurnAndIntValue(turnAndIntValue5.Value, turnAndIntValue5.Turn, turnAndIntValue5.IsSelfTurn));
		}
		for (int num16 = 0; num16 < GameSkillBuffCountList.Count; num16++)
		{
			TurnAndIntValue turnAndIntValue6 = GameSkillBuffCountList[num16];
			target.GameSkillBuffCountList.Add(new TurnAndIntValue(turnAndIntValue6.Value, turnAndIntValue6.Turn, turnAndIntValue6.IsSelfTurn));
		}
		for (int num17 = 0; num17 < GameSkillMetamorphoseCountList.Count; num17++)
		{
			TurnAndIntValue turnAndIntValue7 = GameSkillMetamorphoseCountList[num17];
			target.GameSkillMetamorphoseCountList.Add(new TurnAndIntValue(turnAndIntValue7.Value, turnAndIntValue7.Turn, turnAndIntValue7.IsSelfTurn));
		}
		target.GameSkillDiscardCount = GameSkillDiscardCount;
	}

	private List<BattleCardBase> CloneCardList(ICollection<BattleCardBase> sourceCards, BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer, bool isActualClone)
	{
		if (isActualClone)
		{
			List<BattleCardBase> list = new List<BattleCardBase>(sourceCards.Count);
			for (int i = 0; i < sourceCards.Count; i++)
			{
				list.Add(sourceCards.ElementAt(i).VirtualClone(virtualSelfBattlePlayer, virtualOpponentBattlePlayer));
			}
			return list;
		}
		List<BattleCardBase> list2 = new List<BattleCardBase>(sourceCards.Count);
		for (int j = 0; j < sourceCards.Count; j++)
		{
			list2.Add(CardCreatorBase.CreateDummyInstance());
		}
		return list2;
	}

	private List<BattleCardBase> FindClonedIdCards(IEnumerable<BattleCardBase> clonedCardList, IEnumerable<BattleCardBase> findIdCardList)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		if (findIdCardList == null)
		{
			return list;
		}
		foreach (BattleCardBase uniqId in findIdCardList)
		{
			if (uniqId != null)
			{
				BattleCardBase battleCardBase = clonedCardList.SingleOrDefault((BattleCardBase c) => c.EquelsID(uniqId));
				if (battleCardBase != null)
				{
					list.Add(battleCardBase);
				}
			}
		}
		return list;
	}

	public bool CheckPlayableCards()
	{
		List<BattleCardBase> handCardList = HandCardList;
		for (int i = 0; i < handCardList.Count; i++)
		{
			if (handCardList[i].Movable(isCheckOnDraw: false, isSkipSelecting: true))
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckAttackableCards()
	{
		List<BattleCardBase> inPlayCardList = InPlayCards.ToList();
		int i = 0;
		while (i < inPlayCardList.Count)
		{
			if (inPlayCardList[i].Attackable)
			{
				IEnumerable<BattleCardBase> source = _opponentBattlePlayer.InPlayCards.Where((BattleCardBase c) => c.IsUnit && !c.CantBeFocusedAttack(inPlayCardList[i]));
				if (!inPlayCardList[i].IsCantAttackClass || source.Any())
				{
					IEnumerable<BattleCardBase> source2 = source.Where((BattleCardBase c) => c.SkillApplyInformation.IsGuard);
					if ((!inPlayCardList[i].SkillApplyInformation.IsSkillCantAtkUnitNotHasGuard || source2.Any() || !inPlayCardList[i].IsCantAttackClass) && (!inPlayCardList[i].SkillApplyInformation.IsSkillCantAtkUnit || !source2.Any() || inPlayCardList[i].SkillApplyInformation.IsIgnoreGuard) && (!inPlayCardList[i].SkillApplyInformation.IsSkillCantAtkUnitBaseCardId || ((!source2.Any() || !source2.All((BattleCardBase c) => inPlayCardList[i].SkillApplyInformation.CantAtkUnitBaseCardIdList.Contains(c.BaseParameter.BaseCardId)) || inPlayCardList[i].SkillApplyInformation.IsIgnoreGuard) && (!source.Any() || !source.All((BattleCardBase c) => inPlayCardList[i].SkillApplyInformation.CantAtkUnitBaseCardIdList.Contains(c.BaseParameter.BaseCardId)) || !inPlayCardList[i].IsCantAttackClass))))
					{
						return true;
					}
				}
			}
			int num = i + 1;
			i = num;
		}
		return false;
	}

	public bool CheckNotConsumeEpCard(BattleCardBase card)
	{
		foreach (BattleCardBase inPlayCard in InPlayCards)
		{
			if (inPlayCard.SkillApplyInformation.CheckNotConsumeEpCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<IReadOnlyBattleCardInfo> GetSpecificTurnDestroyCards(TurnPlayerInfo turnPlayerInfo)
	{
		bool isCheckSelf = IsPlayer == turnPlayerInfo.IsSelfPlayer;
		int turn = (isCheckSelf ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return from c in TurnDestroyCards
			where c.IsSelfTurn == isCheckSelf && c.Turn == turn
			select c.Card;
	}

	public VfxBase UpdateInPlayBattleCardIconLabel()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		for (int i = 1; i < ClassAndInPlayCardList.Count; i++)
		{
			if (!(ClassAndInPlayCardList[i].BattleCardView.BattleCardIconAnimations == null) && ClassAndInPlayCardList[i].BattleCardView.BattleCardIconAnimations.HasInductionNumberSkill())
			{
				sequentialVfxPlayer.Register(ClassAndInPlayCardList[i].BattleCardView.UpdateBattleCardIconLabelNumber(ClassAndInPlayCardList[i], ClassAndInPlayCardList[i].Skills));
			}
		}
		return sequentialVfxPlayer;
	}

	public int GetSpecificTurnWhenHealingCount(TurnPlayerInfo turnPlayerInfo, bool isTextKeyword)
	{
		bool isCheckSelf = IsPlayer == turnPlayerInfo.IsSelfPlayer;
		if (isTextKeyword && isCheckSelf == IsPlayer && !IsSelfTurn)
		{
			return 0;
		}
		int turn = (isCheckSelf ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return TurnWhenHealingCount.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == isCheckSelf && c.Turn == turn)?.Value ?? 0;
	}

	public int GetSpecificTurnSkillReturnCardCount(TurnPlayerInfo turnPlayerInfo)
	{
		bool isCheckSelf = turnPlayerInfo.IsSelfPlayer;
		int turn = ((IsPlayer == isCheckSelf) ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return GameSkillReturnCardCountList.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == isCheckSelf && c.Turn == turn)?.Value ?? 0;
	}

	public int GetSpecificTurnSkillDiscardCount(TurnPlayerInfo turnPlayerInfo)
	{
		bool isCheckSelf = turnPlayerInfo.IsSelfPlayer;
		int turn = ((IsPlayer == isCheckSelf) ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return GameSkillDiscardCountList.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == isCheckSelf && c.Turn == turn)?.Value ?? 0;
	}

	public int GetSpecificTurnEnhanceCardCount(TurnPlayerInfo turnPlayerInfo)
	{
		bool isSelfPlayer = turnPlayerInfo.IsSelfPlayer;
		int num = ((IsPlayer == isSelfPlayer) ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		num -= turnPlayerInfo.TurnOffset;
		List<IReadOnlyBattleCardInfo> list = new List<IReadOnlyBattleCardInfo>();
		for (int i = 0; i < GameEnhancePlayCards.Count; i++)
		{
			if (GameEnhancePlayCards[i].Turn == num && GameEnhancePlayCards[i].IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn)
			{
				list.Add(GameEnhancePlayCards[i].Card);
			}
		}
		return list.Count;
	}

	public int GetCurrentTurnPlayCount()
	{
		return TurnPlayCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && c.Turn == BattleMgr.CurrentTurn)?.Value ?? 0;
	}

	public void AddCurrentTrunPlayCount(int count)
	{
		TurnAndIntValue turnAndIntValue = TurnPlayCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && c.Turn == BattleMgr.CurrentTurn);
		if (turnAndIntValue != null)
		{
			turnAndIntValue.AddValue(count);
		}
		else
		{
			TurnPlayCardCountInfo.Add(new TurnAndIntValue(count, BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn));
		}
	}

	public int GetSpecificTurnPlayCount(TurnPlayerInfo turnPlayerInfo)
	{
		bool isCheckSelf = IsPlayer == turnPlayerInfo.IsSelfPlayer;
		int turn = (isCheckSelf ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return TurnPlayCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == isCheckSelf && c.Turn == turn)?.Value ?? 0;
	}

	public void AddCurrentTurnFusionCount(int count)
	{
		TurnAndIntValue turnAndIntValue = TurnFusionCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && c.Turn == BattleMgr.CurrentTurn);
		if (turnAndIntValue != null)
		{
			turnAndIntValue.AddValue(count);
		}
		else
		{
			TurnFusionCountInfo.Add(new TurnAndIntValue(count, BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn));
		}
	}

	public void AddCurrentEvolvePlayCount(int count)
	{
		TurnAndIntValue turnAndIntValue = TurnEvolveCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && c.Turn == BattleMgr.CurrentTurn);
		if (turnAndIntValue != null)
		{
			turnAndIntValue.AddValue(count);
		}
		else
		{
			TurnEvolveCardCountInfo.Add(new TurnAndIntValue(count, BattleMgr.CurrentTurn, BattleMgr.BattlePlayer.IsSelfTurn));
		}
	}

	public int GetCurrentTurnEvolveCount()
	{
		return TurnEvolveCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == BattleMgr.BattlePlayer.IsSelfTurn && c.Turn == BattleMgr.CurrentTurn)?.Value ?? 0;
	}

	public int GetSpecificTurnEvolveCount(TurnPlayerInfo turnPlayerInfo)
	{
		bool isCheckSelf = IsPlayer == turnPlayerInfo.IsSelfPlayer;
		int turn = (isCheckSelf ? BattleMgr.BattlePlayer.Turn : BattleMgr.BattleEnemy.Turn);
		turn -= turnPlayerInfo.TurnOffset;
		return TurnEvolveCardCountInfo.FirstOrDefault((TurnAndIntValue c) => c.IsSelfTurn == isCheckSelf && c.Turn == turn)?.Value ?? 0;
	}

	public int GetAttachTurnBySkillId(string id)
	{
		for (int i = 0; i < Class.Skills.Count(); i++)
		{
			if (Class.Skills.ElementAt(i).GetAttachSkill is Skill_attach_skill skill_attach_skill && skill_attach_skill.SaveTurnSkillId == id)
			{
				return skill_attach_skill.AttachedTurn;
			}
		}
		return 0;
	}

	public void CallOnTokenDraw(BattleCardBase owner, List<BattleCardBase> drawList, List<BattleCardBase> targets, bool isPlayer, bool isOpen, bool isReserved)
	{
		this.OnTokenDrawCards.Call(owner, drawList, targets, isPlayer, isOpen, isReserved);
	}

	public void CallOnCreateReservedCards(BattleCardBase owner, List<BattleCardBase> drawList, bool isPlayer)
	{
		this.OnCreateReservedCards.Call(owner, drawList, isPlayer);
	}

	public void CallOnCostChange(BattleCardBase card, List<BattleCardBase> targets, List<int> addList, List<int> setList, List<bool> isCostUpList, bool isHalf, bool isSpellCharge, bool isOpenCard)
	{
		this.OnCostChange.Call(card, targets, addList, setList, isCostUpList, isHalf, isSpellCharge, isOpenCard);
	}

	public void CallOnRemoveCostChange(List<SkillBase.BuffInfoContainer> targetList, bool isSpellCharge, bool isAdd)
	{
		this.OnRemoveCostChange.Call(targetList, isSpellCharge, isAdd);
	}

	public void CallOnPowerUp(BattleCardBase card, List<BattleCardBase> cards, int offense, int life, int multiplyOffense, int multiplyLife, int maxLife)
	{
		this.OnPowerUp.Call(card, cards, offense, life, multiplyOffense, multiplyLife, maxLife);
	}

	public void CallOnPowerDownStart()
	{
		this.OnPowerDownStart.Call();
	}

	public void CallOnPowerDown(BattleCardBase card, List<BattleCardBase> cards, int offense, int life, int maxLife, bool isSet)
	{
		this.OnPowerDown.Call(card, cards, offense, life, maxLife, isSet);
	}

	public void CallOnDeprivePowerUp(List<Skill_powerup.PowerUpModifierContainer> targetList)
	{
		this.OnDeprivePowerUp.Call(targetList);
	}

	public void CallOnDeprivePowerDown(List<SkillBase.BuffInfoContainer> targetList)
	{
		this.OnDeprivePowerDown.Call(targetList);
	}

	public void CallOnSpellCharge(BattleCardBase card, List<BattleCardBase> targets, List<int> addList)
	{
		this.OnSpellCharge.Call(card, targets, addList);
	}

	public void CallOnDrain(int heal)
	{
		this.OnDrain.Call(heal);
	}

	public void CallOnSkillDamageStart(BattleCardBase card)
	{
		this.OnSkillDamageStart.Call(card);
	}

	public void CallOnDamage(List<BattleCardBase> cards, List<BattleCardBase> effectTargets, List<BattleCardBase.DamageResult> damageList)
	{
		this.OnDamage.Call(cards, effectTargets, damageList);
	}

	public void CallOnHeal(BattleCardBase ownerCard, List<BattleCardBase> cards, List<int> healList)
	{
		this.OnHeal.Call(ownerCard, cards, healList);
	}

	public void CallOnDiscard(List<BattleCardBase> targets)
	{
		this.OnDiscard.Call(targets);
	}

	public void CallOnSkillDestroyOrBanish(BattleCardBase card, bool isBurialRite = false, bool isOpen = false)
	{
		this.OnSkillDestroyOrBanish.Call(card, isBurialRite, isOpen);
	}

	public void CallOnPlayVoiceOnDeath(BattleCardBase card)
	{
		this.OnPlayVoiceOnDeath.Call(card);
	}

	public void CallOnSkillReturn()
	{
		this.OnSkillReturn.Call();
	}

	public void CallOnAddPp(int addPpCount, BattleCardBase card)
	{
		this.OnAddPp.Call(addPpCount, IsPlayer, card);
	}

	public void CallOnAddBp(int addBpCount, BattleCardBase card)
	{
		this.OnAddBp.Call(addBpCount, IsPlayer, card);
	}

	public void CallOnEpModifier(BattleCardBase card, int epCount, bool isAdd)
	{
		this.OnEpModifier.Call(card, epCount, IsPlayer, isAdd);
	}

	public void CallOnBeforeSkillEvolve(BattleCardBase card, List<BattleCardBase> targets)
	{
		this.OnBeforeSkillEvolve.Call(card, targets);
	}

	public void CallOnEvolveMeWhenAttack(BattleCardBase card)
	{
		this.OnEvolveMeWhenAttack.Call(card);
	}

	public void CallOnAfterSkillEvolve(List<BattleCardBase> targets)
	{
		this.OnAfterSkillEvolve.Call(targets);
	}

	public void CallOnPlayCard(BattleCardBase originalCard, BattleCardBase playCard, bool isChoiceBrave)
	{
		this.OnPlayCard.Call(originalCard, playCard, isChoiceBrave);
	}

	public void CallOnWhenPlayEffect(SkillCollectionBase.WhenPlayEffectType whenPlayEffectType, BattleCardBase target, bool isInvoked)
	{
		this.OnWhenPlayEffect.Call(whenPlayEffectType, target, isInvoked);
	}

	public void CallOnChantCountChange(BattleCardBase card, List<BattleCardBase> targets, int changeCount)
	{
		this.OnChantCountChange.Call(card, targets, changeCount);
	}

	public void CallOnChangeWhiteRitualStack(BattleCardBase target, int changeCount, bool isDestroy = false)
	{
		this.OnChangeWhiteRitualStack.Call(target, changeCount, isDestroy);
	}

	public void CallOnChangeMaxAttackableCount(BattleCardBase card, List<BattleCardBase> targets, int changeCount)
	{
		this.OnChangeMaxAttackableCount.Call(card, targets, changeCount);
	}

	public void CallOnMetamorphose(BattleCardBase card, List<BattleCardBase> targets, int cardId)
	{
		this.OnMetamorphose.Call(card, targets, cardId);
	}

	public void CallOnFusionMetamorphose(int fusionMetamorphoseCardId)
	{
		this.OnFusionMetamorphose.Call(fusionMetamorphoseCardId);
	}

	public void CallOnOpenCard(BattleCardBase card)
	{
		this.OnOpenCard.Call(card);
	}

	public void CallOnUnite(BattleCardBase ownerCard, List<BattleCardBase> targets, BattleCardBase uniteCard)
	{
		this.OnUnite.Call(ownerCard, targets, uniteCard);
	}

	public void CallOnRemoveLatestOperationJsonData(NetworkBattleReceiver.ReplayOperationType type)
	{
		this.OnRemoveLatestOperationJsonData.Call(type);
	}

	public void CallOnClearDestroyedCardList(bool isPlayer)
	{
		this.OnClearDestroyedCardList.Call(isPlayer);
	}
}
