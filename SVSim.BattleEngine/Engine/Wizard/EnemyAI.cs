using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public abstract class EnemyAI : IEnemyAI, IEnemyAIBattleInfoRecieveDataAccessor
{

	public static float BREAKBONUS_RATE_IN_HAND = 0.75f;

	public static float BREAKBONUS_RATE_ON_FIELD = 0.1f;

	private static List<int> _emptyPlayPtn = new List<int>();

	protected BattleManagerBase battleMgr;

	protected AIParamQuery paramQuery;

	protected AIStyleQuery styleQuery;

	protected AIEmoteMng emoteMng;

	protected AIEmoteCtrl emoteCtrl;

	protected AIEmoteQuery emoteQuery;

	public bool isUsedEvo;

	public List<PlaySkipInformation> PlaySkipInfo;

	public bool IsBreakBeforePlay;

	private int spareSpace;

	protected bool isPlagueCityTagged;

	private float _previousFieldInplayValue;

	public AILethalPlan OutputLethalPlan;

	protected float m_fieldAdvantage;

	public bool cr_isOprAtk;

	public bool cr_isOprPlay;

	public bool cr_isTerminate;

	public bool is_onSelectSkillTarget;

	public bool IsTurnEndLethal;

	public bool IsFullSimulation;

	protected int _timeOverLogicSec;

	protected Coroutine weakLogicCoroutine;

	protected int oprationQueueActCount;

	public AITokenManager tokenManager;

	private AIVirtualField _currentVirtualField;

	public AIRealActionInformation LatestAction;

	public AIVirtualField BeforeLatestActionField;

	public BattlePlayerBase ALLY;

	public BattlePlayerBase OPPONENT;

	public AISinglePlayptnRecord BestPlayPtnRecord;

	protected IEnumerator aiCoroutine;

	protected bool isForceBreak;

	protected Queue<Action> AIOperationQueue;

	protected AIOperationProcessor OperationProcessor;

	public bool IsOpponentBarbarossaDestroyed;

	public Dictionary<int, int> ReferenceIdTable;

	public Dictionary<string, List<int>> ReferenceTribeTable;

	protected bool isAIEmoteRegistered;

	public Dictionary<ulong, float> FieldHashAndValueTable;

	public Dictionary<ulong, float> FieldHashAndThreatenTable;

	private AIFunctionResultContainer _funcResultContainer;

	private AIVariableResultContainer _valResultContainer;

	private List<int> _playerAbilityIdList;

	public CardBasePrm.ClanType AISubClassType = CardBasePrm.ClanType.NONE;

	public CardBasePrm.ClanType PlayerSubClassType = CardBasePrm.ClanType.NONE;

	private System.Random stableRandom;

	protected bool _isStackAction;

	protected bool _isConnectNetwork = true;

	protected float _currentThinkingIntervalTime;

	protected bool _enabledThinkingCounter;

	protected float _elapsedThinkingTime;

	protected float _elapsedTurnTimeAfterTurnStartVfx;

	public static List<int> EmptyPlayPtn => _emptyPlayPtn;

	public EnemyAI_Play EnemyAIPlay { get; private set; }

	public EnemyAI_Attack EnemyAIAttack { get; private set; }

	public EnemyAI_Skill EnemyAISkill { get; private set; }

	public EnemyAI_WeakLogic EnemyAIWeakLogic { get; private set; }

	public EnemyAIFusion EnemyAIFusion { get; private set; }

	public BattleManagerBase BattleMgr => battleMgr;

	public bool IsThisTurnEmotePlayed { get; protected set; }

	public AIAttachPlayerBattleEventCache PlayerBattleEvent { get; private set; }

	public AIAttachOperateMgrBattleEventCache OprMgrBattleEvent { get; private set; }

	public int PlayerCharaId { get; private set; }

	public bool IsEvoPermissionOnSimu { get; private set; }

	public AIVirtualCard CurrentBattleSimEvoCard { get; set; }

	public int CurrentBattleBeforeSimEvoEvolCount { get; set; }

	public List<AIVirtualCard> AllySuicideList { get; protected set; }

	public bool IsPlagueCityTagged => isPlagueCityTagged;

	public float FieldInplayValueDifference
	{
		get
		{
			if (CurrentVirtualField != null)
			{
				return _previousFieldInplayValue - CurrentVirtualField.GetInplayTotalValue();
			}
			return 0f;
		}
	}

	public bool IsAllyFirst => ALLY.IsGameFirst;

	public bool IsBattleEnd => battleMgr.IsBattleEnd;

	public bool _isSetCardReady { get; protected set; }

	public bool IsRunWeakLogic { get; private set; }

	public AIVirtualField CurrentVirtualField => _currentVirtualField;

	public List<AIVirtualCard> AllyDeckCards { get; private set; }

	public List<AIVirtualCard> EnemyDeckCards { get; private set; }

	public List<AIVirtualCard> DiscardedCards { get; private set; }

	public AIPlayptnRecorder PlayPtnRecorder { get; private set; }

	public AISelectedTargetInfoSet PreDecidedTargets { get; private set; }

	public List<AIVirtualCard> BeforeLatestActionAllyDeckCards { get; private set; }

	public List<AIVirtualCard> BeforeLatestActionEnemyDeckCards { get; private set; }

	public BattlePlayerPair PlayerPair { get; private set; }

	public List<int> BestPlayPtn
	{
		get
		{
			if (BestPlayPtnRecord != null)
			{
				return BestPlayPtnRecord.PlayPtn;
			}
			return EmptyPlayPtn;
		}
	}

	public bool IsAIExecution => aiCoroutine != null;

	public float FieldAdvantage => m_fieldAdvantage;

	public int SpareSpace => spareSpace;

	public AIFunctionResultContainer FuncResultContainer => _funcResultContainer;

	public AIVariableResultContainer ValResultContainer => _valResultContainer;

	public AIEmoteMng EmoteMng => emoteMng;

	public AIEmoteQuery EmoteQuery => emoteQuery;

	public AIParamQuery ParamQuery => paramQuery;

	public AIStyleQuery StyleQuery => styleQuery;

	public static int EnemyAIID { get; set; } = -1;

	public AIGenerateTagOwnerTable GenerateTagOwnerTable { get; }

	public AIBattleInfoReceivedData BattleInfoReceivedData { get; }

	public bool IsInVirutalSimulation { get; set; }

	public int TurnCount => PlayerPair.Self.Turn;

	public int OpponentTurnCount => PlayerPair.Opponent.Turn;

	public bool IsRankMatchAI { get; protected set; }

	public abstract bool IsStackAction { get; }

	public abstract bool IsConnectNetwork { get; }

	public bool IsStopThinkingLogic { get; protected set; }

	protected bool IsThinkingInterval => _currentThinkingIntervalTime > 0f;

	public IAIEmoteCtrl EmoteCtrl()
	{
		return emoteCtrl;
	}

	public EnemyAI(BattleManagerBase mgr)
	{
		battleMgr = mgr;
		EnemyAIPlay = new EnemyAI_Play(this);
		EnemyAIAttack = new EnemyAI_Attack(this);
		EnemyAISkill = new EnemyAI_Skill(this);
		EnemyAIFusion = new EnemyAIFusion(this);
		EnemyAIWeakLogic = new EnemyAI_WeakLogic(this);
		PlayPtnRecorder = new AIPlayptnRecorder();
		paramQuery = new AIParamQuery(this);
		styleQuery = new AIStyleQuery(this, paramQuery);
		emoteCtrl = new AIEmoteCtrl(this);
		emoteMng = new AIEmoteMng(this);
		emoteQuery = new AIEmoteQuery(this);
		paramQuery.SetUp(styleQuery);
		stableRandom = new System.Random(emoteMng.EmoteRandomSeed);
		AIOperationQueue = new Queue<Action>();
		OperationProcessor = new AIOperationProcessor(battleMgr, this);
		IsOpponentBarbarossaDestroyed = false;
		GenerateTagOwnerTable = new AIGenerateTagOwnerTable();
		BattleInfoReceivedData = new AIBattleInfoReceivedData();
	}

	public void ExecuteEnemyAI(bool useWait)
	{
		aiCoroutine = EnemyAI_Move(useWait);
		EnemyAICoroutine.GetInstance().StartCoroutine(aiCoroutine);
	}

	public void StopEnemyAI()
	{
		EnemyAICoroutine.GetInstance().StopAllCoroutines();
		aiCoroutine = null;
		if (weakLogicCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(weakLogicCoroutine);
		}
	}

	public bool SetUpBattleState(int classId, AI_LOGIC_LV logicLv, string deckName, string styleName, string emoteName, int enemyAiID = -1)
	{
		AIDataLibrary aIDataLibrary = battleMgr.GameMgr.GetDataMgr().m_AIDataLibrary;
		AIDeckData curDeck = aIDataLibrary.SearchDeckData(deckName);
		paramQuery.SetDeck(classId, logicLv, curDeck, aIDataLibrary.GetCommonDic(), aIDataLibrary.GetAllyCommonDic());
		AIStyleData deckStyle = aIDataLibrary.SearchDeckStyle(styleName);
		styleQuery.SetDeckStyle(deckStyle);
		styleQuery.UpdateStyle();
		EnemyAIID = enemyAiID;
		if (!IsRankMatchAI && EmoteQuery != null && emoteName != "")
		{
			AIEmoteSet aIEmoteSet = aIDataLibrary.SearchEmoteSet(emoteName);
			if (aIEmoteSet != null)
			{
				EmoteQuery.SetEmoteSet(aIEmoteSet);
			}
		}
		return true;
	}

	public void InitOnGame(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		tokenManager = new AITokenManager(selfBattlePlayer, opponentBattlePlayer, this);
		ALLY = selfBattlePlayer;
		OPPONENT = opponentBattlePlayer;
		PlayerPair = new BattlePlayerPair(selfBattlePlayer, opponentBattlePlayer);
		DataMgr dataMgr = battleMgr.GameMgr.GetDataMgr();
		if (dataMgr != null)
		{
			PlayerCharaId = dataMgr.GetPlayerCharaId();
			AISubClassType = (CardBasePrm.ClanType)dataMgr.GetEnemySubClassId();
			PlayerSubClassType = (CardBasePrm.ClanType)dataMgr.GetPlayerSubClassId();
		}
		styleQuery.UpdateStyle();
		isUsedEvo = false;
		IsEvoPermissionOnSimu = true;
		PlaySkipInfo = null;
		IsBreakBeforePlay = false;
		BestPlayPtnRecord = null;
		m_fieldAdvantage = 0f;
		is_onSelectSkillTarget = false;
		IsOpponentBarbarossaDestroyed = false;
		IsThisTurnEmotePlayed = false;
		PlayerBattleEvent = new AIAttachPlayerBattleEventCache();
		OprMgrBattleEvent = new AIAttachOperateMgrBattleEventCache();
		OperateMgr operateMgr = selfBattlePlayer.BattleMgr.OperateMgr;
		AIAttachEventToBattleModuleUtility.SetupEventWhenInitGame(selfBattlePlayer, opponentBattlePlayer, operateMgr, this);
		IsTurnEndLethal = false;
		FieldHashAndValueTable = new Dictionary<ulong, float>();
		FieldHashAndThreatenTable = new Dictionary<ulong, float>();
		_funcResultContainer = new AIFunctionResultContainer();
		_valResultContainer = new AIVariableResultContainer();
		LatestAction = null;
		BeforeLatestActionField = null;
		IsInVirutalSimulation = false;
	}

	protected void UpdateCurrentVirtualField(AIParamQuery paramQuery, AIStyleQuery styleQuery, BattlePlayerPair pair, List<int> bestPlayPtn, bool isUsePreviousFieldParameter)
	{
		AIVirtualField currentVirtualField = _currentVirtualField;
		_previousFieldInplayValue = ((_currentVirtualField != null) ? _currentVirtualField.GetInplayTotalValue() : 0f);
		AIVirtualFieldBuildParameterCollction buildParameters = ((!isUsePreviousFieldParameter) ? new AIVirtualFieldBuildParameterCollction(null)
		{
			PlayedCardContainer = currentVirtualField.PlayedCardContainer
		} : new AIVirtualFieldBuildParameterCollction(currentVirtualField));
		_currentVirtualField = new AIVirtualField(this, paramQuery, styleQuery, pair, bestPlayPtn, buildParameters);
		GenerateTagOwnerTable.RegisterAllGenerateTagOwner(_currentVirtualField);
		UpdateDeckCards(pair);
		UpdateDiscardedCards(pair.Self);
		tokenManager.UpdateTokenPool(_currentVirtualField);
		_currentVirtualField.InitializeBothDefValue();
	}

	private void UpdateDeckCards(BattlePlayerPair pair)
	{
		if (pair.Self.DeckCardList == null || pair.Opponent.DeckCardList == null)
		{
			AIConsoleUtility.LogError("UpdateDeckCards error!! deck is null!!!!!");
		}
		List<Tuple<int, AIAttachedTagCollection>> list;
		if (AllyDeckCards == null)
		{
			AllyDeckCards = new List<AIVirtualCard>();
			list = null;
		}
		else
		{
			list = new List<Tuple<int, AIAttachedTagCollection>>();
			for (int i = 0; i < AllyDeckCards.Count; i++)
			{
				AIVirtualCard aIVirtualCard = AllyDeckCards[i];
				list.Add(new Tuple<int, AIAttachedTagCollection>(aIVirtualCard.CardIndex, aIVirtualCard.TagCollectionContainer.AttachedTags));
			}
			AllyDeckCards.Clear();
		}
		for (int j = 0; j < pair.Self.DeckCardList.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = new DeckVirtualCard(pair.Self.DeckCardList[j], CurrentVirtualField);
			aIVirtualCard2.InitializeTags(ParamQuery, FindInheritanceAttachedTagForDeckCard(aIVirtualCard2.CardIndex, list), null);
			AllyDeckCards.Add(aIVirtualCard2);
		}
		if (EnemyDeckCards == null)
		{
			EnemyDeckCards = new List<AIVirtualCard>();
			list = null;
		}
		else
		{
			list = new List<Tuple<int, AIAttachedTagCollection>>();
			for (int k = 0; k < EnemyDeckCards.Count; k++)
			{
				AIVirtualCard aIVirtualCard3 = EnemyDeckCards[k];
				list.Add(new Tuple<int, AIAttachedTagCollection>(aIVirtualCard3.CardIndex, aIVirtualCard3.TagCollectionContainer.AttachedTags));
			}
			EnemyDeckCards.Clear();
		}
		for (int l = 0; l < pair.Opponent.DeckCardList.Count; l++)
		{
			AIVirtualCard aIVirtualCard4 = new DeckVirtualCard(pair.Opponent.DeckCardList[l], CurrentVirtualField);
			aIVirtualCard4.InitializeTags(ParamQuery, FindInheritanceAttachedTagForDeckCard(aIVirtualCard4.CardIndex, list), null);
			EnemyDeckCards.Add(aIVirtualCard4);
		}
	}

	private AIAttachedTagCollection FindInheritanceAttachedTagForDeckCard(int deckCardIndex, List<Tuple<int, AIAttachedTagCollection>> attachedTagPool)
	{
		if (attachedTagPool == null || attachedTagPool.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < attachedTagPool.Count; i++)
		{
			Tuple<int, AIAttachedTagCollection> tuple = attachedTagPool[i];
			if (deckCardIndex == tuple.first)
			{
				return tuple.second;
			}
		}
		return null;
	}

	private void UpdateDiscardedCards(BattlePlayerBase self)
	{
		if (self.DiscardedCardList != null)
		{
			if (DiscardedCards == null)
			{
				DiscardedCards = new List<AIVirtualCard>();
			}
			else
			{
				DiscardedCards.Clear();
			}
			for (int i = 0; i < self.DiscardedCardList.Count; i++)
			{
				DiscardedCards.Add(new DiscardedVirtualCard(self.DiscardedCardList[i], _currentVirtualField));
			}
		}
	}

	public void SaveBeforeLatestActionInformation()
	{
		BeforeLatestActionField = new AIVirtualField(_currentVirtualField, isLatestAction: true);
		BeforeLatestActionAllyDeckCards = new List<AIVirtualCard>(AllyDeckCards);
		BeforeLatestActionEnemyDeckCards = new List<AIVirtualCard>(EnemyDeckCards);
	}

	public void LoadBufferedBattleState()
	{
		AISetUpData setupInfoBuf = battleMgr.GameMgr.GetDataMgr().m_AIDataLibrary.SetupInfoBuf;
		if (setupInfoBuf != null)
		{
			SetUpBattleState(setupInfoBuf.classID, setupInfoBuf.logicLv, setupInfoBuf.deckName, setupInfoBuf.styleName, setupInfoBuf.emoteName, setupInfoBuf.enemyAiID);
			if (!IsRankMatchAI && EmoteQuery != null)
			{
				EmoteQuery.SetOnOffEmote(setupInfoBuf.doesUseEmote, setupInfoBuf.useInnerEmote);
			}
			if (setupInfoBuf.specialAbilityList != null)
			{
				_playerAbilityIdList = new List<int>(setupInfoBuf.specialAbilityList);
			}
		}
	}

	private void InitOnTurnStart()
	{
		PlaySkipInfo = null;
		IsBreakBeforePlay = false;
		BestPlayPtnRecord = null;
		m_fieldAdvantage = 0f;
		EnemyAIAttack.InitOnTurnStart();
		CurrentBattleSimEvoCard = null;
		CurrentBattleBeforeSimEvoEvolCount = ALLY.CurrentEpCount;
		if (paramQuery.GetLogicLv() == AI_LOGIC_LV.MIDDLE)
		{
			int num = ((CalcFieldAdvantage() <= -8f) ? 100 : 30);
			int num2 = AIStableRandom(100);
			IsEvoPermissionOnSimu = num2 <= num;
		}
		else
		{
			IsEvoPermissionOnSimu = true;
		}
		IsInVirutalSimulation = false;
		IsTurnEndLethal = false;
		tokenManager.RegistPermanentlyToken();
		FieldHashAndValueTable.Clear();
		FieldHashAndThreatenTable.Clear();
	}

	public void Mulligan(List<BattleCardBase> dstList, BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		UpdateCurrentVirtualField(paramQuery, styleQuery, new BattlePlayerPair(selfBattlePlayer, opponentBattlePlayer), EmptyPlayPtn, isUsePreviousFieldParameter: true);
		_currentVirtualField.InitializeGameStartInnerTags();
		int count = CurrentVirtualField.AllyHandCards.Count;
		int num = 0;
		int[] array = new int[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = 0;
		}
		for (int j = 0; j < count - 1; j++)
		{
			AIVirtualCard aIVirtualCard = CurrentVirtualField.AllyHandCards[j];
			for (int k = j + 1; k < count; k++)
			{
				if (CurrentVirtualField.AllyHandCards[k].BaseId == aIVirtualCard.BaseId)
				{
					array[j] = 2;
					break;
				}
			}
		}
		for (int l = 0; l < count; l++)
		{
			if (array[l] == 0)
			{
				AIVirtualCard card = CurrentVirtualField.AllyHandCards[l];
				if (paramQuery.IsEnabledTag(card, CurrentVirtualField, AIPlayTagType.MulliganKeep, EmptyPlayPtn, null))
				{
					array[l] = 1;
					num++;
				}
				if (paramQuery.IsEnabledTag(card, CurrentVirtualField, AIPlayTagType.MulliganChange, EmptyPlayPtn, null))
				{
					array[l] = 2;
				}
			}
		}
		if (num == count)
		{
			return;
		}
		bool flag = false;
		for (int m = 0; m < count; m++)
		{
			AIVirtualCard aIVirtualCard2 = CurrentVirtualField.AllyHandCards[m];
			if (array[m] == 0 && aIVirtualCard2.Cost == 2 && isKeepableCard(aIVirtualCard2))
			{
				array[m] = 1;
				num++;
				flag = true;
				break;
			}
		}
		if (num == count)
		{
			return;
		}
		bool flag2 = false;
		bool flag3 = false;
		if (num > 0 && flag)
		{
			for (int n = 0; n < count; n++)
			{
				AIVirtualCard aIVirtualCard3 = CurrentVirtualField.AllyHandCards[n];
				if ((aIVirtualCard3.Cost == 1 || aIVirtualCard3.Cost == 3) && array[n] == 0 && isKeepableCard(aIVirtualCard3))
				{
					array[n] = 1;
					num++;
					if (aIVirtualCard3.Cost == 1)
					{
						flag2 = true;
					}
					else if (aIVirtualCard3.Cost == 3)
					{
						flag3 = true;
					}
					break;
				}
			}
		}
		if (num == count)
		{
			return;
		}
		if (num > 0 && flag && (flag2 || flag3))
		{
			for (int num2 = 0; num2 < count; num2++)
			{
				AIVirtualCard aIVirtualCard4 = CurrentVirtualField.AllyHandCards[num2];
				if (array[num2] == 0)
				{
					if (!isKeepableCard(aIVirtualCard4))
					{
						break;
					}
					if ((flag2 && aIVirtualCard4.Cost == 3) || (flag3 && aIVirtualCard4.Cost == 4))
					{
						array[num2] = 1;
						num++;
						break;
					}
				}
			}
		}
		if (num == count)
		{
			return;
		}
		for (int num3 = 0; num3 < count; num3++)
		{
			if (array[num3] == 2 || array[num3] == 0)
			{
				dstList.Add(CurrentVirtualField.AllyHandCards[num3].BaseCard);
			}
		}
		bool isKeepableCard(AIVirtualCard aIVirtualCard5)
		{
			if (!aIVirtualCard5.IsUnit)
			{
				if (IsBishop())
				{
					return aIVirtualCard5.IsAmulet;
				}
				return false;
			}
			return true;
		}
	}

	public void ChangeWeakLogic()
	{
		if (battleMgr is SingleBattleMgr singleBattleMgr)
		{
			singleBattleMgr.RecordChangeAI("weak", oprationQueueActCount);
			oprationQueueActCount = 0;
		}
		IsRunWeakLogic = true;
	}

	public void ChangeNormalLogic()
	{
		IsRunWeakLogic = false;
		_timeOverLogicSec = 0;
	}

	protected virtual IEnumerator WeakLogicTimerCoroutine()
	{
		_timeOverLogicSec = 0;
		while (!IsBattleEnd)
		{
			if (!IsRunWeakLogic)
			{
				_timeOverLogicSec++;
				if (_timeOverLogicSec > 7 && battleMgr.VfxMgr.IsEnd)
				{
					ChangeWeakLogic();
				}
			}
			yield return new WaitForSecondsRealtime(1f);
		}
	}

	public bool IsLethal(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation is AIVirtualTargetSelectAction { forceLethalMode: not false })
		{
			return true;
		}
		AIVariableResultContainer valResultContainer = field.AI.ValResultContainer;
		ulong hash = AIFunctionResultHashCalculator.GetHash(tagOwner, field, playPtn, situation, 0uL);
		if (valResultContainer.GetContainsResultValue(AIScriptTokenVariableType.IS_LETHAL, hash, out var getResult))
		{
			return getResult == 1f;
		}
		AISinglePlayptnRecord playPtnRecord = PlayPtnRecorder.FindMatchedPlayPtnRecord(playPtn, field);
		bool flag = AIPlayOutChecker.CalculatePlayOutDamageProspected(paramQuery, field, playPtnRecord, this) >= field.EnemyClass.Life;
		valResultContainer.CheckDuplicateAndAddRecord(AIScriptTokenVariableType.IS_LETHAL, hash, flag ? 1f : 0f, $"IsLethal(): Already hashed target and not equal value. CardName:[{tagOwner.CardName}] hash:[{hash}]");
		return flag;
	}

	private IEnumerator EnemyAI_Move(bool useWait)
	{
		is_onSelectSkillTarget = false;
		IsRunWeakLogic = false;
		IsStopThinkingLogic = false;
		if (weakLogicCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(weakLogicCoroutine);
			weakLogicCoroutine = null;
		}
		if (!BattleMgr.IsRecovery)
		{
			weakLogicCoroutine = BattleCoroutine.GetInstance().StartCoroutine(WeakLogicTimerCoroutine());
		}
		InitOnTurnStart();
		WaitForSeconds shortWait = new WaitForSeconds(0.2f);
		WaitForSeconds longWait = new WaitForSeconds(0.5f);
		isForceBreak = false;
		oprationQueueActCount = 0;
		while (true)
		{
			if (IsRankMatchAI && IsStopThinkingLogic)
			{
				continue;
			}
			yield return useWait ? shortWait : null;
			if (AIOperationQueue.Count > 0)
			{
				if (!battleMgr.VfxMgr.IsEnd || (IsRankMatchAI && IsThinkingInterval))
				{
					continue;
				}
				if (IsRankMatchAI)
				{
					if (_isConnectNetwork)
					{
						AIOperationQueue.Dequeue().Call();
						oprationQueueActCount++;
						_isStackAction = false;
					}
					else
					{
						_isStackAction = true;
					}
				}
				else
				{
					AIOperationQueue.Dequeue().Call();
					oprationQueueActCount++;
				}
				continue;
			}
			if (isForceBreak)
			{
				break;
			}
			if (battleMgr.IsBattleEnd || is_onSelectSkillTarget)
			{
				continue;
			}
			BestPlayPtnRecord = null;
			ChangeNormalLogic();
			EnemyAIPlay.InitOnIterationStart();
			EnemyAIAttack.InitOnIterationStart();
			EnemyAIFusion.InitOnIterationStart();
			PlaySkipInfo = null;
			IsBreakBeforePlay = false;
			AllySuicideList = new List<AIVirtualCard>();
			spareSpace = 5;
			IsFullSimulation = false;
			PreDecidedTargets = null;
			LatestAction = null;
			BeforeLatestActionField = null;
			_funcResultContainer.Clear();
			_valResultContainer.Clear();
			if (IsRankMatchAI)
			{
				_enabledThinkingCounter = true;
				_elapsedThinkingTime = 0f;
			}
			AllySuicideList = GetSuicideAllyFollowerSequence(CurrentVirtualField, paramQuery, styleQuery);
			PlayPtnRecorder = new AIPlayptnRecorder();
			PlayPtnRecorder.CreateValidPlayPtnList(_currentVirtualField);
			BestPlayPtnRecord = PlayPtnRecorder.GetEmptyPlayPtnRecord();
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(HandPlayEstimationCoroutine());
			m_fieldAdvantage = CalcFieldAdvantage();
			isPlagueCityTagged = CurrentVirtualField.IsPlagueCity();
			VfxBase emote = GetEmote(AIEmoteCmdType.ON_ITERATION_START);
			cr_isTerminate = false;
			IsTurnEndLethal = false;
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(EnemyAIAttack.Cr_BattleAI_ImmediateAttack());
			if (cr_isTerminate)
			{
				if (!IsTurnEndLethal)
				{
					continue;
				}
				break;
			}
			cr_isOprPlay = false;
			IsTurnEndLethal = false;
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(EnemyAIPlay.BattleAI_HandPlay());
			AIGetOnSimulationUtility.RegisterGetOnTokenInPlayPtn(this);
			if (cr_isOprPlay)
			{
				if (!IsTurnEndLethal)
				{
					continue;
				}
				break;
			}
			if (paramQuery.GetLogicLv() == AI_LOGIC_LV.STRONG || paramQuery.GetLogicLv() == AI_LOGIC_LV.MIDDLE)
			{
				cr_isOprAtk = false;
				yield return EnemyAICoroutine.GetInstance().StartCoroutine(EnemyAIAttack.BattleAI_UseHighSimu());
				if (cr_isOprAtk)
				{
					continue;
				}
			}
			else if (paramQuery.GetLogicLv() == AI_LOGIC_LV.WEAK && (EnemyAIWeakLogic.BattleAI_AttackWeak() || EnemyAIWeakLogic.BattleAI_EvoWeak()))
			{
				continue;
			}
			if ((PlaySkipInfo != null || IsBreakBeforePlay) && ((BestPlayPtn != null && BestPlayPtn.Count > 0) || EnemyAIPlay.IsFusion))
			{
				yield return EnemyAICoroutine.GetInstance().StartCoroutine(HandCardAction());
				if (cr_isOprPlay)
				{
					if (IsTurnEndLethal)
					{
						break;
					}
					continue;
				}
			}
			if ((paramQuery.GetLogicLv() != AI_LOGIC_LV.WEAK || !EnemyAIAttack.BattleAI_TurnEndAttack()) && AIOperationQueue.Count <= 0)
			{
				break;
			}
		}
		yield return useWait ? longWait : null;
		while (!battleMgr.VfxMgr.IsEnd)
		{
			yield return null;
		}
		if (IsRankMatchAI && !battleMgr.IsRecovery)
		{
			float delayTurnEndTime = StyleQuery.GetDelayTurnEndTime(CurrentVirtualField.AllyClass, BestPlayPtn);
			while (_elapsedTurnTimeAfterTurnStartVfx < delayTurnEndTime)
			{
				yield return null;
			}
		}
		emoteQuery.OnOperation();
		VfxBase vfxBase = ((TurnCount != 1) ? GetEmote(AIEmoteCmdType.ON_ALLY_TURN_END) : GetEmote(AIEmoteCmdType.ON_FIRST_TURN));
		if (vfxBase != null)
		{
			battleMgr.VfxMgr.RegisterSequentialVfx(vfxBase);
		}
		IsThisTurnEmotePlayed = false;
		battleMgr.VfxMgr.RegisterSequentialVfx(battleMgr.OperateMgr.TurnEndOperation(ALLY.IsPlayer));
		if (weakLogicCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(weakLogicCoroutine);
			weakLogicCoroutine = null;
		}
	}

	public IEnumerator HandCardAction()
	{
		if (EnemyAIPlay.PriorityCard == null)
		{
			AIConsoleUtility.LogError("HandCardAction error!! PriorityCard is null!!!!!");
			cr_isOprPlay = false;
			yield break;
		}
		if (EnemyAIPlay.IsFusion && EnemyAIPlay.FusionPattern != null)
		{
			EnemyAIFusion.SetFusionSituation(EnemyAIPlay.FusionPattern);
			if (EnemyAIFusion.FusionAI())
			{
				cr_isOprPlay = true;
			}
			yield break;
		}
		AIVirtualCard aIVirtualCard = EnemyAIPlay.PriorityCard.FindRealActor(BestPlayPtnRecord);
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard, EnemyAIPlay.PriorityCard, AIOperationType.PLAY);
		int playSpaceRequired = CurrentVirtualField.GetPlaySpaceRequired(aIVirtualCard, BestPlayPtn, situation);
		int num = 6 - CurrentVirtualField.CardListSet.AllyClassAndInplayCards.Count;
		bool flag = true;
		if (playSpaceRequired > num)
		{
			flag = false;
		}
		else if (EnemyAIPlay.BestPlayPtnWithToken != null)
		{
			bool num2 = StyleQuery.IsPlayBreak(EnemyAIPlay.PriorityCard, BestPlayPtn, null);
			int count = EnemyAIPlay.BestPlayPtnWithToken.TokenPtn[0].TokenInfoPairList.Count;
			if (num2)
			{
				int allyLastwordTokenCount = EnemyAIPlay.PriorityCard.GetAllyLastwordTokenCount(BestPlayPtn);
				count += allyLastwordTokenCount;
			}
			else
			{
				count += playSpaceRequired;
			}
			if (count > num)
			{
				flag = false;
			}
		}
		if (EnemyAIPlay.InstantAttackActionInfo != null && !flag)
		{
			int item = CurrentVirtualField.AllyHandCards.IndexOf(EnemyAIPlay.PriorityCard);
			int index = EnemyAIPlay.BestPlayPtnWithToken.PlayPtn.IndexOf(item);
			TokenPlayPattern tokenPlayPattern = EnemyAIPlay.BestPlayPtnWithToken.TokenPtn[index];
			if (CurrentVirtualField.GetPlaySpaceRequired(EnemyAIPlay.PriorityCard, BestPlayPtn, situation, needsTokenCount: false) + tokenPlayPattern.TokenInfoPairList.Count > 6 - CurrentVirtualField.CardListSet.AllyClassAndInplayCards.Count)
			{
				AIVirtualCard aIVirtualCard2 = null;
				if (EnemyAIPlay.InstantAttackActionInfo is AIVirtualAttackInfo aIVirtualAttackInfo)
				{
					aIVirtualCard2 = aIVirtualAttackInfo.AttackTarget;
				}
				if (aIVirtualCard2 != null)
				{
					OprAttack(EnemyAIPlay.InstantAttackActionInfo);
					cr_isOprPlay = true;
				}
				else
				{
					AIConsoleUtility.LogError("InstantAttack error!! Cannot find attack target!!!!!");
				}
				yield break;
			}
		}
		if (!EnemyAIPlay.PriorityCard.BaseCard.Movable())
		{
			cr_isOprPlay = false;
			yield break;
		}
		if (EnemyAIPlay.PriorityCardPlayInfo != null && EnemyAIPlay.PriorityCardPlayInfo.HasPreDecidedSelectTargets)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			AISelectedTargetInfoSet preDecidedSelectTargets = EnemyAIPlay.PriorityCardPlayInfo.PreDecidedSelectTargets;
			for (int i = 0; i < AISelectedTargetInfoSet.LENGTH; i++)
			{
				AISelectedTargetInfo aISelectedTargetInfo = preDecidedSelectTargets.Get(i);
				if (aISelectedTargetInfo != null && aISelectedTargetInfo.HasTarget)
				{
					for (int j = 0; j < aISelectedTargetInfo.Targets.Count; j++)
					{
						list.Add(aISelectedTargetInfo.Targets[j].BaseCard);
					}
				}
			}
			EnemyAISkill.SetPreDecidedTarget(list);
			SetPreDecidedTargets(preDecidedSelectTargets);
		}
		OprPlay(EnemyAIPlay.PriorityCard, BestPlayPtnRecord);
		cr_isOprPlay = true;
	}

	public void SelectSkillTarget(AIVirtualCard actCard, AIOperationType operationType, AISinglePlayptnRecord playptnRecord)
	{
		is_onSelectSkillTarget = true;
		AISinglePlayptnRecord aISinglePlayptnRecord = playptnRecord;
		List<int> list = ((playptnRecord != null) ? playptnRecord.PlayPtn : EmptyPlayPtn);
		AIVirtualCard aIVirtualCard = _currentVirtualField.SearchVirtualCard(actCard);
		AIVirtualCard aIVirtualCard2 = ((operationType != AIOperationType.PLAY || list == null || list.Count <= 0) ? aIVirtualCard : aIVirtualCard.FindRealActor(aISinglePlayptnRecord));
		List<int> list2 = list;
		if (aIVirtualCard2.HasDestroyPlayPtnTag(operationType))
		{
			list2 = ((operationType != AIOperationType.PLAY) ? EmptyPlayPtn : new List<int> { _currentVirtualField.AllyHandCards.IndexOf(actCard) });
			aISinglePlayptnRecord = PlayPtnRecorder.FindMatchedPlayPtnRecord(list2, _currentVirtualField);
		}
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard, operationType, (AISelectedTargetInfoSet)null);
		if (operationType == AIOperationType.PLAY && PreDecidedTargets != null)
		{
			aIVirtualTargetSelectAction.SelectedTargets = PreDecidedTargets;
		}
		List<AIVirtualTargetSelectInfo> list3 = aIVirtualCard2.CreateAIVirtualSelectInfo(_currentVirtualField, aIVirtualTargetSelectAction);
		if (aIVirtualTargetSelectAction.GetChoiceTarget() != null)
		{
			SetPreDecidedTargets(aIVirtualTargetSelectAction.SelectedTargets);
		}
		AIVirtualCard aIVirtualCard3 = null;
		if (list2 != null)
		{
			switch (operationType)
			{
			case AIOperationType.EVOLVE:
				if (list2.Count > 0)
				{
					aIVirtualCard3 = _currentVirtualField.AllyHandCards[list2[0]];
				}
				break;
			case AIOperationType.PLAY:
				if (list2.Count > 1)
				{
					aIVirtualCard3 = _currentVirtualField.AllyHandCards[list2[1]];
				}
				break;
			}
		}
		PlayedCardInfo nextPlayCardInfo = null;
		if (aIVirtualCard3 != null && aISinglePlayptnRecord != null)
		{
			nextPlayCardInfo = aISinglePlayptnRecord.FindPlayedCardInfo(aIVirtualCard3);
		}
		AIVirtualTargetSelectSimulationInfo simulationInfo = new AIVirtualTargetSelectSimulationInfo
		{
			OriginalActor = aIVirtualCard,
			RealActor = aIVirtualCard2,
			SelectInfoList = list3,
			OperationType = operationType,
			NextPlayCardInfo = nextPlayCardInfo,
			IsFirstAction = true
		};
		if (list3 != null && list3.Count > 0)
		{
			EnemyAICoroutine.GetInstance().StartCoroutine(AIVirtualTargetSelectSimulator.ExecuteTargetSelect(simulationInfo, _currentVirtualField, aISinglePlayptnRecord));
		}
		else
		{
			EnemyAICoroutine.GetInstance().StartCoroutine(EnemyAISkill._Cr_SelectSkillTarget(actCard, operationType, aISinglePlayptnRecord));
		}
		PreDecidedTargets = null;
	}

	protected IEnumerator HandPlayEstimationCoroutine()
	{
		int handCardCount = ALLY.HandCardList.Count;
		BattlePlayerPair sourcePair = new BattlePlayerPair(ALLY, OPPONENT);
		int index = 0;
		while (index < handCardCount)
		{
			CurrentVirtualField.AllyHandCards[index].CreateHandPlayEstimator(paramQuery, index, sourcePair, this);
			yield return null;
			int num = index + 1;
			index = num;
		}
	}

	public static List<AIVirtualCard> GetSuicideAllyFollowerSequence(AIVirtualField field, AIParamQuery ParamQuery, AIStyleQuery styleQuery)
	{
		AIVirtualField aIVirtualField = new AIVirtualField(field);
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < aIVirtualField.AllyInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = aIVirtualField.AllyInplayCards[i];
			if (!aIVirtualCard.IsUnit || !aIVirtualCard.IsAttackable(EmptyPlayPtn))
			{
				continue;
			}
			if (list.Count == 0)
			{
				list.Add(aIVirtualCard);
				continue;
			}
			int num = 0;
			for (num = 0; num < list.Count; num++)
			{
				AIVirtualCard aIVirtualCard2 = list[num];
				if (aIVirtualCard.Value < aIVirtualCard2.Value)
				{
					break;
				}
			}
			list.Insert(num, aIVirtualCard);
		}
		if (list == null || list.Count <= 0)
		{
			return new List<AIVirtualCard>();
		}
		List<AIVirtualActionInfo> list2 = new List<AIVirtualActionInfo>();
		for (int j = 0; j < list.Count; j++)
		{
			AIVirtualCard sourceCard = list[j];
			list2.Add(new AIVirtualAttackInfo(sourceCard, isAttackFollower: true));
		}
		List<AIVirtualCard> list3 = new List<AIVirtualCard>();
		for (int k = 0; k < aIVirtualField.EnemyInplayCards.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = aIVirtualField.EnemyInplayCards[k];
			if (!aIVirtualCard3.IsUnit)
			{
				continue;
			}
			if (list3.Count == 0)
			{
				list3.Add(aIVirtualCard3);
				continue;
			}
			int num2 = 0;
			for (num2 = 0; num2 < list3.Count; num2++)
			{
				AIVirtualCard aIVirtualCard4 = list3[num2];
				if ((aIVirtualCard3.IsGuard || !aIVirtualCard4.IsGuard) && ((aIVirtualCard3.IsGuard && !aIVirtualCard4.IsGuard) || aIVirtualCard3.IsKiller || aIVirtualCard3.Attack > aIVirtualCard4.Attack))
				{
					break;
				}
			}
			list3.Insert(num2, aIVirtualCard3);
		}
		BattleSequencer.ExecSimulation(aIVirtualField, list2, list3, new SimulationSetting(isHandRemovalValid: false, useLeaderAttackPreCheck: false, noSkipAttack: true, checkAct: false));
		list.RemoveAll((AIVirtualCard c) => !c.IsDead);
		List<AIVirtualCard> list4 = new List<AIVirtualCard>();
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			for (int num4 = 0; num4 < field.AllyInplayCards.Count; num4++)
			{
				if (field.AllyInplayCards[num4].CardIndex == list[num3].CardIndex && field.AllyInplayCards[num4].BaseId == list[num3].BaseId)
				{
					list4.Add(field.AllyInplayCards[num4]);
					break;
				}
			}
		}
		return list4;
	}

	public float GetEvoPenalty()
	{
		float num = 4f;
		if (!isUsedEvo && TurnCount >= 6)
		{
			num /= 2f;
		}
		return num;
	}

	public static int GetBaseId(int id)
	{
		return CardMaster.GetInstanceForBattle().GetCardParameterFromId(id).BaseCardId;
	}

	public int AIStableRandom()
	{
		return stableRandom.Next();
	}

	public int AIStableRandom(int val)
	{
		return (int)Math.Floor((double)val * stableRandom.NextDouble());
	}

	public float CalcFieldAdvantage()
	{
		float num = 0f;
		for (int i = 0; i < CurrentVirtualField.AllyInplayCards.Count; i++)
		{
			float num2 = CurrentVirtualField.AllyInplayCards[i].EvaluateValueOnField(EmptyPlayPtn, null, useStyle: true);
			if (num2 > 0f)
			{
				num += num2;
			}
		}
		float num3 = 0f;
		for (int j = 0; j < CurrentVirtualField.EnemyInplayCards.Count; j++)
		{
			float num4 = CurrentVirtualField.EnemyInplayCards[j].EvaluateValueOnField(EmptyPlayPtn, null, useStyle: true);
			if (num4 > 0f)
			{
				num3 += num4;
			}
		}
		return num - num3;
	}

	public int GetAllySpaceNum()
	{
		return 5 - CurrentVirtualField.AllyInplayCards.Count;
	}

	public void OprEvolution(AIVirtualCard virtualCard)
	{
		AIOperationQueue.Enqueue(delegate
		{
			SetupThinkingInterval();
			isUsedEvo = true;
			emoteQuery.OnOperation();
			SelectSkillTarget(virtualCard, AIOperationType.EVOLVE, BestPlayPtnRecord);
			emoteMng.OnOperationRequest();
		});
	}

	public void OprPlay(AIVirtualCard card, AISinglePlayptnRecord playptnRecord)
	{
		AIVirtualCard currentFieldCard = _currentVirtualField.SearchVirtualCard(card);
		AIOperationQueue.Enqueue(delegate
		{
			SetupThinkingInterval();
			emoteMng.EvalAllyOnCardPlay(currentFieldCard, playptnRecord);
			emoteQuery.OnOperation();
			_isSetCardReady = false;
			SelectSkillTarget(currentFieldCard, AIOperationType.PLAY, playptnRecord);
			_isSetCardReady = true;
			emoteMng.OnOperationRequest();
		});
	}

	public void OprAttack(AISituationInfo situation)
	{
		AIVirtualAttackInfo attackSituation = situation as AIVirtualAttackInfo;
		if (attackSituation == null || attackSituation.Actor == null || attackSituation.AttackTarget == null)
		{
			AIConsoleUtility.LogError("OprAttack error!!! situation is invalid");
			return;
		}
		AIOperationQueue.Enqueue(delegate
		{
			AIRealBattleCardSearcher.SearchAttackPairFromSituation(PlayerPair, attackSituation, out var attacker, out var attackTarget);
			if (attackTarget != null && attackTarget != null)
			{
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				VfxBase emote = GetEmote(AIEmoteCmdType.ON_ALLY_ATTACK, situation);
				sequentialVfxPlayer.Register(emote);
				if (AttackSelectControl.CanCardAttackTarget(attacker, attackTarget, attackTarget.SelfBattlePlayer.InPlayCards))
				{
					VfxBase vfx = battleMgr.OperateMgr.Attack(attacker, attackTarget, ALLY.IsPlayer);
					sequentialVfxPlayer.Register(vfx);
					battleMgr.VfxMgr.RegisterSequentialVfx(sequentialVfxPlayer);
				}
				else
				{
					if (attackTarget != null)
					{
						_ = attacker.BaseParameter.CardName;
					}
					_ = attackTarget?.BaseParameter.CardName;
					isForceBreak = true;
				}
			}
			else
			{
				isForceBreak = true;
			}
			emoteMng.OnOperationRequest();
			if (IsRankMatchAI)
			{
				CheckIsStackAction();
			}
		});
	}

	public void OprTargetSelect(AIVirtualCard virtualActor, AISelectedTargetInfoSet targetInfoSet, AIOperationType operationType)
	{
		AIOperationQueue.Enqueue(delegate
		{
			if (!battleMgr.IsBattleEnd)
			{
				AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(virtualActor, virtualActor, operationType, targetInfoSet);
				VfxBase vfx = null;
				switch (operationType)
				{
				case AIOperationType.EVOLVE:
					vfx = OperationProcessor.AIEvolutionCard(situation);
					break;
				case AIOperationType.PLAY:
					vfx = OperationProcessor.AIPlayCard(situation);
					break;
				case AIOperationType.FUSION:
					vfx = OperationProcessor.AIFusionCard(situation);
					break;
				}
				is_onSelectSkillTarget = false;
				battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
				emoteMng.OnOperationRequest();
				if (IsRankMatchAI)
				{
					SetupThinkingInterval();
					CheckIsStackAction();
				}
			}
		});
		EnemyAISkill.ClearPreDecidedTarget();
	}

	public bool OprLethalAction(AISituationInfo action)
	{
		AIVirtualCard originalCard = action.OriginalCard;
		AISelectedTargetInfoSet selectedTargets = action.SelectedTargets;
		AIOperationType actionType = action.ActionType;
		switch (actionType)
		{
		case AIOperationType.EVOLVE:
		case AIOperationType.PLAY:
			OprTargetSelect(originalCard, selectedTargets, actionType);
			return true;
		case AIOperationType.ATTACK:
			OprAttack(action);
			return true;
		default:
			return false;
		}
	}

	public static bool IsSameValue(float value1, float value2)
	{
		return (int)Math.Round(value1 * 1000f) == (int)Math.Round(value2 * 1000f);
	}

	public static bool IsLargerThan(float value1, float value2)
	{
		return (int)Math.Round(value1 * 1000f) > (int)Math.Round(value2 * 1000f);
	}

	public virtual VfxBase GetEmote(AIEmoteCmdType cmdType, AISituationInfo situation = null, ClassCharaPrm.EmotionType receivedEmoteType = ClassCharaPrm.EmotionType.NULL, int emoteInput = -1)
	{
		if (IsBattleEnd)
		{
			return NullVfx.GetInstance();
		}
		if (isAIEmoteRegistered && cmdType != AIEmoteCmdType.ON_OPPONENT_TURN_START && cmdType != AIEmoteCmdType.ON_ALLY_TURN_START)
		{
			AIConsoleUtility.Log("<color=green>既にEmoteを登録中です！</color>");
			return NullVfx.GetInstance();
		}
		isAIEmoteRegistered = true;
		float num = 0f;
		float num2 = 0f;
		AIEmoteCmd aIEmoteCmd = null;
		VfxBase vfxBase = NullVfx.GetInstance();
		switch (cmdType)
		{
		case AIEmoteCmdType.ON_RECEIVE:
			aIEmoteCmd = emoteCtrl.OnOpponentEmotion(receivedEmoteType);
			if (aIEmoteCmd != null)
			{
				vfxBase = ALLY.Emotion.PlayEmotion(aIEmoteCmd.emoteType, 1.5f);
				num = 2f;
			}
			break;
		case AIEmoteCmdType.ON_FIRST_TURN:
			vfxBase = ALLY.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.PROVOCATION, 1.5f);
			break;
		case AIEmoteCmdType.ON_ITERATION_START:
			aIEmoteCmd = emoteCtrl.OnIterationStart();
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_ALLY_TURN_START:
			aIEmoteCmd = emoteCtrl.OnAllyTurnStart(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_OPPONENT_TURN_START:
			aIEmoteCmd = emoteCtrl.OnOpponentTurnStart(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_ALLY_TURN_END:
			aIEmoteCmd = emoteCtrl.OnAllyTurnEnd();
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_OPPONENT_TURN_END:
			aIEmoteCmd = emoteCtrl.OnOpponentTurnEnd();
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_ALLY_ATTACK:
			aIEmoteCmd = emoteCtrl.OnAllyAttack(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_OPPONENT_ATTACK:
			aIEmoteCmd = emoteCtrl.OnOpponentAttackExecuted();
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_ALLY_EVOLUTION:
			aIEmoteCmd = emoteCtrl.OnAllyEvolution(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_CARD_DESTROY:
			aIEmoteCmd = emoteCtrl.OnCardDestroy(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_CARD_PLAY_ALLY:
			aIEmoteCmd = emoteCtrl.OnCardPlay(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_CARD_PLAY_OPPONENT:
			aIEmoteCmd = emoteCtrl.OnOpponentPlayCardExecuted(situation);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
			}
			break;
		case AIEmoteCmdType.ON_LEADER_DAMAGED:
			aIEmoteCmd = emoteCtrl.OnLeaderDamaged(_currentVirtualField);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
				num = 2f;
			}
			break;
		case AIEmoteCmdType.ON_PLAYER_LEADER_DAMAGED:
			aIEmoteCmd = emoteCtrl.OnPlayerLeaderDamaged(_currentVirtualField);
			if (aIEmoteCmd != null)
			{
				vfxBase = PlayEmotionDefault(aIEmoteCmd);
				num = 2f;
			}
			break;
		case AIEmoteCmdType.ON_DEBUG_SEARCH_ID:
			aIEmoteCmd = emoteQuery.SearchEmoteByID(emoteInput);
			if (aIEmoteCmd != null)
			{
				isAIEmoteRegistered = false;
				return PlayEmotionDefault(aIEmoteCmd, isForcePlay: true);
			}
			break;
		case AIEmoteCmdType.ON_DEBUG_SEARCH_CATEGORY:
		{
			IEnumerable<AIEmoteCmd> enumerable = emoteQuery.SearchEmoteByCategory(emoteInput);
			if (enumerable == null)
			{
				break;
			}
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			foreach (AIEmoteCmd item in enumerable)
			{
				sequentialVfxPlayer.Register(PlayEmotionDefault(item, isForcePlay: true));
				sequentialVfxPlayer.Register(WaitVfx.Create(2f));
			}
			isAIEmoteRegistered = false;
			return NullVfx.GetInstance();
		}
		}
		if (vfxBase == NullVfx.GetInstance())
		{
			isAIEmoteRegistered = false;
			return NullVfx.GetInstance();
		}
		IsThisTurnEmotePlayed = true;
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		if (num > 0f)
		{
			sequentialVfxPlayer2.Register(WaitVfx.Create(num));
		}
		sequentialVfxPlayer2.Register(vfxBase);
		if (num2 > 0f)
		{
			sequentialVfxPlayer2.Register(WaitVfx.Create(num2));
		}
		sequentialVfxPlayer2.Register(InstantVfx.Create(delegate
		{
			isAIEmoteRegistered = false;
		}));
		return sequentialVfxPlayer2;
	}

	private VfxBase PlayEmotionDefault(AIEmoteCmd emoteCmd, bool isForcePlay = false)
	{
		if (!isForcePlay)
		{
			if (emoteQuery.GetCategoryInterval(emoteCmd.CategoryKey) > 0)
			{
				return NullVfx.GetInstance();
			}
			if (AIEmoteUtility.IsSystemEmoteKey(emoteCmd.CategoryKey) && ALLY.Turn < 4)
			{
				return NullVfx.GetInstance();
			}
			if (!ALLY.IsSelfTurn)
			{
				emoteMng.OnOpponentTurnEmote();
			}
			if (AIEmoteUtility.IsSystemEmoteKey(emoteCmd.CategoryKey))
			{
				emoteQuery.SetInterval(emoteCmd.CategoryKey, AIStableRandom() % 2 + 1);
			}
		}
		emoteMng.AddPlayedCountOnEmotePlaying(emoteCmd.CategoryKey);
		if (emoteCmd.isAI)
		{
			return ALLY.Emotion.PlayEmotion((ClassCharaPrm.MotionType)emoteCmd.motionID, (ClassCharaPrm.FaceType)emoteCmd.faceID, emoteCmd.voiceID, emoteCmd.textID);
		}
		return OPPONENT.Emotion.PlayEmotion((ClassCharaPrm.MotionType)emoteCmd.motionID, (ClassCharaPrm.FaceType)emoteCmd.faceID, emoteCmd.voiceID, emoteCmd.textID);
	}

	public bool IsBishop()
	{
		return paramQuery.GetClassID() == 7;
	}

	public bool IsNecromancer()
	{
		return paramQuery.GetClassID() == 5;
	}

	public bool IsRoyal()
	{
		return paramQuery.GetClassID() == 2;
	}

	public bool IsAbleEvo()
	{
		if (ALLY.EvolveWaitTurnCount <= 0)
		{
			return ALLY.NowTurnEvol;
		}
		return false;
	}

	public bool IsAllyCard(BattleCardBase card)
	{
		return PlayerPair.Self.IsPlayer == card.IsPlayer;
	}

	public bool IsAllyCard(AIVirtualCard card)
	{
		return PlayerPair.Self.IsPlayer == card.IsPlayer;
	}

	public void SetPreDecidedTargets(AISelectedTargetInfoSet set)
	{
		PreDecidedTargets = set;
	}

	public bool HasPlayerAbilityId(int id)
	{
		if (_playerAbilityIdList == null || _playerAbilityIdList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < _playerAbilityIdList.Count; i++)
		{
			if (_playerAbilityIdList[i] == id)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateAICurrentVirtualField(bool isUsePreviousFieldParameter = true)
	{
		UpdateCurrentVirtualField(paramQuery, styleQuery, PlayerPair, EmptyPlayPtn, isUsePreviousFieldParameter);
	}

	public abstract void Retire();

	public abstract void Disconnect();

	public abstract void Reconnect();

	public abstract void CleanupStackedAction();

	protected abstract void OnFinishOprAttack();

	protected abstract void OnFinishOprTargetSelect();

	protected abstract void OnBeforeTurnEnd();

	public void TurnEnd()
	{
		OnBeforeTurnEnd();
		BattleCoroutine instance = BattleCoroutine.GetInstance();
		if (weakLogicCoroutine != null)
		{
			instance.StopCoroutine(weakLogicCoroutine);
			weakLogicCoroutine = null;
		}
		EnemyAICoroutine.GetInstance().StopAllCoroutines();
		EnemyAIUtil.TurnEnd(BattleMgr, PlayerPair.Self.IsPlayer);
	}

	protected abstract void CheckIsStackAction();

	protected abstract void SetupThinkingInterval();

	public int CalcHandNextTurnDamage(AIVirtualField field)
	{
		int num = 0;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		List<AIVirtualCard> list2 = new List<AIVirtualCard>();
		foreach (AIVirtualCard allyHandCard in field.AllyHandCards)
		{
			if (allyHandCard.TagCollectionContainer.HasTag(AIPlayTagType.PlayoutNextTurn))
			{
				list.Add(allyHandCard);
			}
		}
		if (0 < list.Count)
		{
			int num2 = Mathf.Min(10, ALLY.PpTotal + 1);
			int num3 = (int)Mathf.Pow(2f, list.Count);
			for (int i = 0; i < num3; i++)
			{
				int num4 = 0;
				int totalCost = 0;
				ConvertPlayoutNextTurnHandPtnList(list, list2, i, out totalCost);
				if (num2 < totalCost)
				{
					continue;
				}
				for (int j = 0; j < list2.Count; j++)
				{
					if (!list2[j].TagCollectionContainer.HasTag(AIPlayTagType.PlayoutNextTurn))
					{
						continue;
					}
					PlayoutNextTurnTagCollection playoutNextTurnTags = list2[j].TagCollectionContainer.PlayoutNextTurnTags;
					for (int k = 0; k < playoutNextTurnTags.TagList.Count; k++)
					{
						if (playoutNextTurnTags.TagList[k].CheckCondition(list[j], EmptyPlayPtn, field, null))
						{
							num4 += (int)playoutNextTurnTags.TagList[k].EvalArg(list2[j], null, field, null);
						}
					}
				}
				if (num < num4)
				{
					num = num4;
				}
			}
		}
		return num;
	}

	private void ConvertPlayoutNextTurnHandPtnList(List<AIVirtualCard> srcList, List<AIVirtualCard> dstList, int index, out int totalCost)
	{
		totalCost = 0;
		int count = srcList.Count;
		int num = index;
		dstList.Clear();
		for (int i = 0; i < count; i++)
		{
			AIVirtualCard aIVirtualCard = srcList[i];
			int num2 = (int)Mathf.Pow(2f, count - i - 1);
			if (0 < num / num2)
			{
				num -= num2;
				dstList.Add(aIVirtualCard);
				totalCost += aIVirtualCard.Cost;
			}
		}
	}
}
