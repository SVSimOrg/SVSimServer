using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Phase;
using Wizard.Battle.Player.ClassCharacter;
using Wizard.Battle.Recovery;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

public class NetworkBattleManagerBase : BattleManagerBase
{
	public class ValidateSkillData
	{
		public int CardIndex;

		public bool isPlayer;

		public int SkillIndex;

		public ValidateSkillData(int card, bool player, int skill)
		{
			CardIndex = card;
			isPlayer = player;
			SkillIndex = skill;
		}
	}

	public RegisterActionManager RegisterActionManager;

	protected SendKeyActionDataManager sendKeyActionDataManager;

	public NetworkBattleReceiver.ReceiveData _lastReceivedData;

	public VfxBase _specialWinVfx;

	protected NetworkBattleReceiver networkReceiver;

	protected NetworkConsistency networkConsistency;

	private BattleCardBase _operatePlayCard;

	private List<BattleCardBase> _operatePlaySelectCard;

	protected bool _isSendSpecialWin;

	private bool _isSpecialWin;

	public OperateReceive OperateReceive;

	protected OperateReceiveChecker operateReceiveChecker;

	public TurnEndTimeController turnEndTimeController;

	protected NetworkTouchControl networkTouchControl;

	public NetworkBattleSetupCardEvent _networkBattleSetupCardEventBase;

	protected List<NetworkBattleIntervalCheckerBase> _intervalCheckList;

	protected OperateMgr operateEvent_OperateMgr;

	protected bool isStopOperateFlag;

	public bool IsSendSwap;

	private Func<CardCreateInfo, IndexInfo, int, bool, bool, BattleCardBase> CreateBattleCardFunc;

	private bool _isJudgeResultReceive;

	private Coroutine _checkJudgeResultToDisconnectCoroutine;

	private Coroutine _resultRetryCoroutine;

	private BATTLE_RESULT_TYPE _finishEffectType;

	private int judgeResult_NotFinishNum;

	public NetworkBattleReceiver.RESULT_CODE JudgeResultReceiveCode;

	protected ReceiveIntervalTrigger receiveIntervalTrigger;

	public SendIntervalTrigger SendIntervalTriggerMain;

	public bool IsStopIntervalCheck;

	protected bool IsShowDisconnectPanel;

	protected bool IsShowOpponentDisconnectPanel;

	public RecoveryController _recoveryController;

	private bool _isNoLimitJudgeResult;

	private bool _isNodeErrorToNocontest;

	protected List<ReplaceReceivedCard.CardIdAndIndex> NotReplaceCardList;

	private Coroutine _waitToReconnectSocketCoroutine;

	private bool _isBattleEndLog;

	public List<RegisterUnapproved> RegisterUnapprovedList { get; protected set; }

	public NetworkBattleSender NetworkSender { get; protected set; }

	public BattleCardBase NowPlayCard { get; private set; }

	public bool IsCardPlayToTurnEndTimeoutStop { get; private set; }

	public NetworkBattleData networkBattleData { get; protected set; }

	public bool IsNetworkBattleEnd { get; private set; }

	public OpponentRecoveryToDispChecker opponentRecoveryToDispChecker { get; protected set; }

	public DisconnectToDispChecker disconnectToDispChecker { get; protected set; }

	public RecoveryToDispChecker recoveryToDispChecker { get; protected set; }

	public DisconnectToLoseChecker disconnectToLoseChecker { get; protected set; }

	public NotMulliganEndToJudgeChecker notMulliganEndToJudgeChecker { get; protected set; }

	public OpponentNotTurnStartToWinChecker opponentNotTurnStartToWinChecker { get; private set; }

	public OpponentNotTurnEndToWinChecker opponentNotTurnEndToWinChecker { get; private set; }

	public NotTurnEndToLoseChecker notTurnEndToLoseChecker { get; protected set; }

	public NotTurnStartToLoseChecker notTurnStartToLoseChecker { get; protected set; }

	public ReceiveTurnEndToJudgeResult receiveTurnEndToJudgeResult { get; protected set; }

	public JudgeResultFailedToRetryChecker judgeResultFailedToRetryChecker { get; private set; }

	public override bool IsStopOperate => isStopOperateFlag;

	public bool IsBeforePlayerTurn { get; private set; }

	public SlideObjectReceiveControl SlideObjectReceiveCtrl { get; private set; }

	protected List<ValidateSkillData> validateSkillIndexList { get; set; }

	protected List<int> registerSelectTypeSkillIndexList { get; set; }

	public bool IsValidateSkillIndexListEmpty
	{
		get
		{
			if (validateSkillIndexList != null)
			{
				return !validateSkillIndexList.Any();
			}
			return true;
		}
	}

	public bool IsSkillSelectTiming { get; private set; }

	private List<int> LastCheckInplayWhiteRitualStackPair { get; set; } = new List<int> { 0, 0 };

	public override void Update(float dt)
	{
		base.Update(dt);
		if (IsBattleEnd && turnEndTimeController != null)
		{
			if (!_isBattleEndLog)
			{
				turnEndTimeController.EndCountDown("BattleEnd");
				_isBattleEndLog = true;
			}
		}
		else if (turnEndTimeController != null)
		{
			turnEndTimeController.UpdateTimerCountDown();
			turnEndTimeController.UpdateTimeoutTurnEnd();
		}
	}

	public bool IsNotReplaceCardListAny(int index, bool isPlayer)
	{
		for (int i = 0; i < NotReplaceCardList.Count; i++)
		{
			if (NotReplaceCardList[i].CardIndex == index && NotReplaceCardList[i].IsPlayer == isPlayer)
			{
				return true;
			}
		}
		return false;
	}

	public void AddNotReplaceCardList(ReplaceReceivedCard.CardIdAndIndex cardInfo)
	{
		NotReplaceCardList.Add(cardInfo);
	}

	public ReplaceReceivedCard.CardIdAndIndex GetAndRemoveNotReplaceCard(int index, bool isPlayer)
	{
		ReplaceReceivedCard.CardIdAndIndex cardIdAndIndex = null;
		for (int i = 0; i < NotReplaceCardList.Count; i++)
		{
			if (NotReplaceCardList[i].CardIndex == index && NotReplaceCardList[i].IsPlayer == isPlayer)
			{
				cardIdAndIndex = NotReplaceCardList[i];
			}
		}
		NotReplaceCardList.Remove(cardIdAndIndex);
		return cardIdAndIndex;
	}

	public override int GetMaxDeckCount(bool isSelf)
	{
		return this.GameMgr.GetDataMgr().GetDeckMaxCount(isSelf);
	}

	public NetworkBattleManagerBase(IBattleMgrContentsCreator contentsCreator)
		: base(contentsCreator)
	{
		NetworkBattleManagerSetup();
	}

	// Phase-5 chunk 45: overload accepting a pre-seeded GameMgr.
	public NetworkBattleManagerBase(IBattleMgrContentsCreator contentsCreator, GameMgr gameMgr)
		: base(contentsCreator, gameMgr)
	{
		NetworkBattleManagerSetup();
	}

	protected override void FirstRecoverySetting()
	{
	}

	protected virtual void NetworkBattleManagerSetup()
	{
		IsShowDisconnectPanel = false;
		IsShowOpponentDisconnectPanel = false;
		NotReplaceCardList = new List<ReplaceReceivedCard.CardIdAndIndex>();
		validateSkillIndexList = new List<ValidateSkillData>();
		RegisterActionManager = new RegisterActionManager(this);
		registerSelectTypeSkillIndexList = new List<int>();
		sendKeyActionDataManager = new SendKeyActionDataManager();
		TouchControl = new NetworkTouchControl(this, _battleCamera, _backGround);
		networkTouchControl = TouchControl as NetworkTouchControl;
		JudgeResultReceiveCode = NetworkBattleReceiver.RESULT_CODE.NotFinish;
		// IsReplayBattle is const-false in headless (Phase-4 target): only IsRecovery can enter
		// this branch, and its selfIdxSeed always uses the RecoveryManager path.
		if (base.IsRecovery)
		{
			networkTouchControl.SetDisableTouch();
			networkBattleData = new NetworkRecoveryBattleData(this);
			networkReceiver = new NetworkReplayBattleReceiver(this);
			OperateReceive = new RecoveryOperateReceive(this, RegisterActionManager, OperateMgr, networkBattleData);
			int selfIdxSeed = _contentsCreator.RecoveryManager.IdxChangeSeed;
			CreateXorShift(selfIdxSeed, -1);
		}
		else
		{
			networkBattleData = new NetworkBattleData(this);
			networkReceiver = new NetworkBattleReceiver(this);
			OperateReceive = new OperateReceive(this, RegisterActionManager, OperateMgr, networkBattleData);
		}
		SetupCreateBattleCardFunc(base.IsRecovery);
		OperateMgr.SetTouchControl(TouchControl);
		RegisterUnapprovedList = new List<RegisterUnapproved>();
		networkConsistency = new NetworkConsistency(this);
		_networkBattleSetupCardEventBase = new NetworkBattleSetupCardEvent(this, RegisterActionManager, networkBattleData);
		operateReceiveChecker = new OperateReceiveChecker(this, networkBattleData);
		_intervalCheckList = new List<NetworkBattleIntervalCheckerBase>();
		opponentRecoveryToDispChecker = new OpponentRecoveryToDispChecker();
		disconnectToDispChecker = new DisconnectToDispChecker();
		_intervalCheckList.Add(disconnectToDispChecker);
		disconnectToLoseChecker = new DisconnectToLoseChecker();
		_intervalCheckList.Add(disconnectToLoseChecker);
		opponentNotTurnStartToWinChecker = new OpponentNotTurnStartToWinChecker(this);
		_intervalCheckList.Add(opponentNotTurnStartToWinChecker);
		opponentNotTurnEndToWinChecker = new OpponentNotTurnEndToWinChecker(this);
		_intervalCheckList.Add(opponentNotTurnEndToWinChecker);
		notMulliganEndToJudgeChecker = new NotMulliganEndToJudgeChecker();
		_intervalCheckList.Add(notMulliganEndToJudgeChecker);
		notTurnEndToLoseChecker = new NotTurnEndToLoseChecker(this);
		_intervalCheckList.Add(notTurnEndToLoseChecker);
		notTurnStartToLoseChecker = new NotTurnStartToLoseChecker();
		_intervalCheckList.Add(notTurnStartToLoseChecker);
		receiveTurnEndToJudgeResult = new ReceiveTurnEndToJudgeResult();
		_intervalCheckList.Add(receiveTurnEndToJudgeResult);
		judgeResultFailedToRetryChecker = new JudgeResultFailedToRetryChecker();
		_intervalCheckList.Add(judgeResultFailedToRetryChecker);
		receiveIntervalTrigger = new ReceiveIntervalTrigger();
		SendIntervalTriggerMain = new SendIntervalTrigger();
		SlideObjectReceiveCtrl = new SlideObjectReceiveControl(this);
	}

	protected virtual void OpponentAliveCallback()
	{
		DispOpponentDisconnect(flag: false);
	}

	protected virtual void OpponentDisconnectCallback()
	{
		DispOpponentDisconnect(flag: true);
	}

	public override void SetupActionProcessorEvent(ActionProcessor processor, bool isPlayer)
	{
		base.SetupActionProcessorEvent(processor, isPlayer);
		SetupNetworkActionProcessorEvent(processor, isPlayer);
	}

	protected virtual void SetupNetworkActionProcessorEvent(ActionProcessor processor, bool isPlayer)
	{
		processor.OnTransform += delegate(BattleCardBase card, int id, bool isChoice)
		{
			if (card.IsPlayer)
			{
				RegisterActionManager.Add(new RegisterMetamorphoseData(id, card.Index, card.IsPlayer, null, isChoice));
			}
		};
		processor.OnSpecialAccelerate += delegate(SkillBase skill)
		{
			if (skill.SkillPrm.ownerCard.IsPlayer && RegisterSkillConditionCheck.IsSkillConditionCheck(skill))
			{
				_networkBattleSetupCardEventBase.Event_SkillConditionCheck(skill, new List<BattleCardBase>());
			}
		};
		processor.OnBeforeChosenPlayCard += delegate(BattleCardBase originalCard, BattleCardBase playCard, List<int> chosenIndexs)
		{
			if (originalCard.IsPlayer || NetworkBattleGenericTool.IsAcceleratedCard(originalCard) || NetworkBattleGenericTool.IsCrystallizeCard(originalCard) || playCard.IsChoiceBraveSkillCard)
			{
				sendKeyActionDataManager.SettingKeyActionData(originalCard, playCard, chosenIndexs);
			}
		};
		processor.OnBeforeBurialRitePlayCard += delegate(BattleCardBase originalCard, IEnumerable<BattleCardBase> selectedCards, bool isEvolve)
		{
			sendKeyActionDataManager.SettingBurialRiteKeyActionData(originalCard, selectedCards, isEvolve);
		};
		processor.OnBeforeChosenEvolution += delegate(BattleCardBase originalCard, BattleCardBase evolCard, List<int> chosenIndexs)
		{
			if (originalCard.IsPlayer)
			{
				sendKeyActionDataManager.SettingKeyActionData(originalCard, evolCard, chosenIndexs, isEvol: true);
			}
		};
		processor.OnBeforeFusion += delegate(BattleCardBase originalCard, IEnumerable<BattleCardBase> selectedCards)
		{
			if (originalCard.IsPlayer)
			{
				sendKeyActionDataManager.SettingFusionKeyActionData(originalCard, selectedCards);
			}
		};
	}

	protected void SetupCreateBattleCardFunc(bool createCardWithoutGameObject)
	{
		if (createCardWithoutGameObject)
		{
			CreateBattleCardFunc = delegate(CardCreateInfo info, IndexInfo indexInfo, int repeat, bool isVirtual, bool isActualCard)
			{
				CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(info.Id);
				BattlePlayerBase battlePlayer = GetBattlePlayer(info.IsPlayer);
				return CreateBattleCard(info.Id, info.IsPlayer, null, cardParameterFromId, battlePlayer, SetupCardIndex(battlePlayer, (info.IndexInfo == null) ? indexInfo.AddIndex : info.IndexInfo.AddIndex));
			};
		}
		else
		{
			CreateBattleCardFunc = base.CreateBattleCardWithGameObject;
		}
	}

	public void SetTimeDecrementFlag(bool isDecrement)
	{
		if (turnEndTimeController != null)
		{
			turnEndTimeController.SetDecrementFlag(isDecrement);
		}
	}

	protected override int CreateBackgroundId()
	{
		if (PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SIMPLE_STAGE))
		{
			return 9;
		}
		int backGroundId = _contentsCreator.RecoveryManager.BackGroundId;
		if (backGroundId >= 0)
		{
			return backGroundId;
		}
		return this.GameMgr.GetNetworkUserInfoData().GetFieldId();
	}

	protected override OperateMgr CreateOperateMgr()
	{
		if (base.IsRecovery)
		{
			return new RecoveryOperateMgr(this, TouchControl);
		}
		return base.CreateOperateMgr();
	}

	public override IInnerOptionsBuilder CreateEnemyInnerOptionsBuilder()
	{
		return new NetworkOpponentInnerOptionsBuilder();
	}

	public override void StartOpening(int FirstAttack)
	{
		base.StartOpening(FirstAttack);
		TurnEndButtonUI component = SBattleLoad.m_TurnEndBtnUI.GetComponent<TurnEndButtonUI>();
		// IsAINetwork is const-false in headless — the guard `!false || X` is a tautology, so
		// turnEndTimeController is always re-constructed on StartOpening.
		turnEndTimeController = new TurnEndTimeController(this, BattlePlayer, component);
	}

	protected override void SetupEvent()
	{
		base.SetupEvent();
		SetupBattlePlayerRegisterEvents(BattlePlayer);
		SetupBattlePlayerRegisterEvents(BattleEnemy);
		if (base.IsRecovery)
		{
			StartRecoveryRecording();
		}
		else
		{
			SetupNetworkEvent(isRecovery: false);
		}
	}

	protected virtual void SendTurnStart()
	{
	}

	protected virtual void SendTurnEndAction()
	{
	}

	public virtual void SendTurnEnd()
	{
	}

	protected virtual void SendChatStamp(ClassCharaPrm.EmotionType emoteType)
	{
	}

	protected virtual void SendPlayCard(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
	}

	protected virtual void SendAttackData(BattleCardBase attackCard, BattleCardBase targetCard)
	{
	}

	protected virtual void SendEvolveData(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
	}

	protected virtual void SendFusionData(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
	}

	protected virtual void SendJudgement()
	{
	}

	public virtual void SendEcho(int playIndex, NetworkBattleDefine.PlayActionType actionType, bool isNotActiveSeq = false, bool isTurnStart = false)
	{
	}

	protected virtual void SetupNetworkEvent(bool isRecovery)
	{
		BattlePlayer battlePlayer = BattlePlayer;
		battlePlayer.OnTurnStartComplete = (Action)Delegate.Combine(battlePlayer.OnTurnStartComplete, (Action)delegate
		{
			notTurnStartToLoseChecker.StopChecker();
			_isNoLimitJudgeResult = false;
			SendTurnStart();
			notTurnEndToLoseChecker.StartChecker();
		});
		BattleEnemy battleEnemy = BattleEnemy;
		battleEnemy.OnTurnStartComplete = (Action)Delegate.Combine(battleEnemy.OnTurnStartComplete, (Action)delegate
		{
			notTurnStartToLoseChecker.StopChecker();
			_isNoLimitJudgeResult = false;
		});
		BattlePlayer.OnShortageDeck += delegate
		{
			RegisterActionManager.Add(new RegisterDeckOut(isSelf: true));
			return OnShortageDeck(BattlePlayer);
		};
		BattleEnemy.OnShortageDeck += delegate
		{
			RegisterActionManager.Add(new RegisterDeckOut(isSelf: false));
			return OnShortageDeck(BattleEnemy);
		};
		BattlePlayer.OnTurnEndStart += delegate
		{
			_isNoLimitJudgeResult = true;
			if (this.InstanceNetworkAgent != null)
			{
				this.InstanceNetworkAgent.ResetDisconnectLogNum();
			}
		};
		BattleEnemy.OnTurnEndStart += delegate
		{
			notTurnStartToLoseChecker.StartChecker();
			_isNoLimitJudgeResult = true;
		};
		if (isRecovery)
		{
			FirstSettingRealTimeNetworkBattle();
		}
		else
		{
			BattleCoroutine.GetInstance().StartCoroutine(WaitNetworkBattleLoading());
		}
	}

	public void SettingTurnEndRestore(BattlePlayerBase player)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingTurnEndRestore(new List<BattleCardBase>(player.HandCardList));
		if (player.EvolveWaitTurnCount <= 0)
		{
			registerEnhanceTrigger.SettingCanEvolve();
		}
		if (Data.CurrentFormat == Format.Unlimited)
		{
			registerEnhanceTrigger.SettingIsUnlimited();
		}
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void SettingTurnStartRestore(BattlePlayerBase player)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingTurnStartRestore();
		if (player.EvolveWaitTurnCount <= 0)
		{
			registerEnhanceTrigger.SettingCanEvolve();
		}
		if (Data.CurrentFormat == Format.Unlimited)
		{
			registerEnhanceTrigger.SettingIsUnlimited();
		}
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterInplayWhiteRitualStack(BattlePlayerBase player)
	{
		int num = player.InPlayCards.Where((BattleCardBase c) => c.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL) && (c.IsField || c.IsChantField)).Sum((BattleCardBase c) => c.SkillApplyInformation.WhiteRitualCount);
		if (LastCheckInplayWhiteRitualStackPair[(!player.IsPlayer) ? 1 : 0] != num)
		{
			RegisterPlayerParameter data = new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.stack, num, player.IsPlayer);
			RegisterActionManager.Add(data);
			LastCheckInplayWhiteRitualStackPair[(!player.IsPlayer) ? 1 : 0] = num;
		}
	}

	protected virtual void SetupBattlePlayerRegisterEvents(BattlePlayerBase battlePlayer)
	{
		battlePlayer.OnAddHandCardEvent += delegate(BattleCardBase card, NetworkBattleDefine.NetworkCardPlaceState fromState, bool isOpen, SkillBase skill)
		{
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Hand;
			AddRegisterMoveCard(card, fromState, to, skill, isNotCheck: false, isOpen, isFlood: false, card.Skills.Any((SkillBase s) => s.OnWhenDraw != 0));
		};
		battlePlayer.OnAfterReturnCardEvent += delegate(BattleCardBase card)
		{
			_networkBattleSetupCardEventBase.SetupCardSkillEvent(card);
		};
		battlePlayer.OnAddCemeteryEvent += delegate(BattleCardBase card, BattlePlayerBase.CEMETERY_TYPE cemeteryType, bool isOpen, SkillBase skill)
		{
			if (!card.IsClass && card.Index != -1)
			{
				bool flag = false;
				if (cemeteryType != BattlePlayerBase.CEMETERY_TYPE.NORMAL)
				{
					foreach (RegisterActionBase item in RegisterActionManager.RegisterDataList.FindAll((RegisterActionBase x) => x is RegisterToken))
					{
						RegisterChoiceAdd registerChoiceAdd = item as RegisterChoiceAdd;
						RegisterToken registerToken = item as RegisterToken;
						if (registerChoiceAdd != null && card.IsPlayer == registerChoiceAdd.IsSelf && registerChoiceAdd.IndexList.Any((int s) => s == card.Index))
						{
							registerChoiceAdd.SetToPlace(NetworkBattleDefine.NetworkCardPlaceState.Cemetery);
							flag = true;
						}
						else if (registerToken != null && registerToken.CardObj == card && registerToken.ToPlaceState != NetworkBattleDefine.NetworkCardPlaceState.Deck)
						{
							registerToken.SetToPlace(NetworkBattleDefine.NetworkCardPlaceState.Cemetery);
							flag = true;
						}
					}
				}
				if (!flag)
				{
					NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.None;
					switch (cemeteryType)
					{
					case BattlePlayerBase.CEMETERY_TYPE.NORMAL:
						networkCardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index);
						if (networkCardPlaceState == NetworkBattleDefine.NetworkCardPlaceState.Hand && card.Skills.Any((SkillBase s) => s.OnDisCardStart != 0))
						{
							isOpen = true;
						}
						break;
					case BattlePlayerBase.CEMETERY_TYPE.FIELD_RETURN_HAND_OVER:
						networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Field;
						break;
					case BattlePlayerBase.CEMETERY_TYPE.DECK_DRAW_HAND_OVER:
						networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
						break;
					}
					NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
					AddRegisterMoveCard(card, networkCardPlaceState, to, skill, isNotCheck: true, isOpen, (cemeteryType != BattlePlayerBase.CEMETERY_TYPE.NORMAL) ? true : false, isWhenDraw: false, card.SkillApplyInformation.IsGuard);
				}
			}
		};
		battlePlayer.OnSummonAfterEvent += delegate(BattleCardBase card)
		{
			RegisterInplayWhiteRitualStack(card.SelfBattlePlayer);
		};
		battlePlayer.OnLeaveAfterEvent += delegate(BattleCardBase card)
		{
			RegisterInplayWhiteRitualStack(card.SelfBattlePlayer);
		};
		battlePlayer.OnMetamorphoseAfterEvent += delegate(BattleCardBase originalCard, BattleCardBase newCard)
		{
			RegisterInplayWhiteRitualStack(newCard.SelfBattlePlayer);
		};
		battlePlayer.OnSpellPlayEvent += delegate(BattleCardBase card)
		{
			NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Cemetery;
			AddRegisterMoveCard(card, networkCardPlaceState, to, null);
		};
		battlePlayer.OnAddPlayCardEvent += delegate(BattleCardBase card, bool isGetoff, SkillBase skill)
		{
			NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = ((!isGetoff) ? NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index) : NetworkBattleDefine.NetworkCardPlaceState.Riding);
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Field;
			AddRegisterMoveCard(card, networkCardPlaceState, to, skill);
			List<RegisterMetamorphoseData> list = RegisterActionManager.RegisterDataList.FindAll((RegisterActionBase x) => x is RegisterMetamorphoseData).ConvertAll((RegisterActionBase x) => x as RegisterMetamorphoseData);
			if (list != null)
			{
				RegisterMetamorphoseData registerMetamorphoseData = list.Find((RegisterMetamorphoseData x) => x.IsChoice);
				if (registerMetamorphoseData != null)
				{
					RegisterActionManager.Remove(registerMetamorphoseData);
					RegisterActionManager.Add(registerMetamorphoseData);
				}
			}
		};
		battlePlayer.OnAddDeckEvent += delegate(BattleCardBase card, SkillBase skill)
		{
			NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index);
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Deck;
			AddRegisterMoveCard(card, cardPlaceState, to, skill);
		};
		battlePlayer.OnAddBanishEvent += delegate(BattleCardBase card, SkillBase skill, bool isOpen)
		{
			NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index);
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Banish;
			AddRegisterMoveCard(card, cardPlaceState, to, skill, isNotCheck: false, isOpen);
		};
		battlePlayer.OnGeton += delegate(BattleCardBase card, SkillBase skill)
		{
			NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index);
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Riding;
			AddRegisterMoveCard(card, cardPlaceState, to, skill);
		};
		battlePlayer.OnAddBlackHole += delegate(List<BattleCardBase> cards, SkillBase skill)
		{
			for (int i = 0; i < cards.Count; i++)
			{
				if (!(cards[i] is NullBattleCard))
				{
					NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, cards[i].Index);
					NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.BlackHole;
					AddRegisterMoveCard(cards[i], cardPlaceState, to, skill);
				}
			}
			RegisterInplayWhiteRitualStack(battlePlayer);
		};
		battlePlayer.OnAddUniteEvent += delegate(BattleCardBase card, SkillBase skill)
		{
			NetworkBattleDefine.NetworkCardPlaceState cardPlaceState = NetworkBattleGenericTool.GetCardPlaceState(battlePlayer, card.Index);
			NetworkBattleDefine.NetworkCardPlaceState to = NetworkBattleDefine.NetworkCardPlaceState.Unite;
			AddRegisterMoveCard(card, cardPlaceState, to, skill);
		};
		battlePlayer.OnChangePP += delegate(int changePpCount)
		{
			if (changePpCount != 0)
			{
				RegisterActionManager.Add(new RegisterPlayerParameter(RegisterActionBase.ActionBaseParameter.maxPP, changePpCount, battlePlayer.IsPlayer));
			}
		};
		battlePlayer.OnTurnEndFinish += delegate
		{
			if (battlePlayer.IsPlayer)
			{
				IsBeforePlayerTurn = battlePlayer.IsExtraTurn;
			}
			else
			{
				IsBeforePlayerTurn = !battlePlayer.IsExtraTurn;
			}
			return NullVfx.GetInstance();
		};
	}

	protected virtual bool isNetworkOepn()
	{
		return this.InstanceNetworkAgent.IsOpen();
	}

	private IEnumerator WaitNetworkBattleLoading()
	{
		while (this.InstanceNetworkAgent == null)
		{
			yield return null;
		}
		while (!isNetworkOepn())
		{
			yield return null;
		}
		FirstSettingRealTimeNetworkBattle();
		int randomSeed = this.GameMgr.GetNetworkUserInfoData().GetRandomSeed();
		LocalLog.AccumulateLastTraceLog("657699SetSeed " + randomSeed);
		_stableRandom = new System.Random(randomSeed);
		_stableRandomOnlySelf = new System.Random(randomSeed);
	}

	protected void FirstSettingRealTimeNetworkBattle()
	{
		this.InstanceNetworkAgent.SetNetworkBattleMgr(this);
	}

	public virtual void SettingOpponentAliveEvent()
	{
		NetworkStatus playerNetworkStatus = this.InstanceNetworkAgent.PlayerNetworkStatus;
		playerNetworkStatus.OnAlive = (Action)Delegate.Combine(playerNetworkStatus.OnAlive, new Action(OnPlayerAlive));
		NetworkStatus opponentNetworkStatus = this.InstanceNetworkAgent.OpponentNetworkStatus;
		opponentNetworkStatus.OnAlive = (Action)Delegate.Combine(opponentNetworkStatus.OnAlive, new Action(OpponentAliveCallback));
		NetworkStatus opponentNetworkStatus2 = this.InstanceNetworkAgent.OpponentNetworkStatus;
		opponentNetworkStatus2.OnDisconnect = (Action)Delegate.Combine(opponentNetworkStatus2.OnDisconnect, new Action(OpponentDisconnectCallback));
	}

	public override void SetupBattlePlayersEvent()
	{
		base.SetupBattlePlayersEvent();
		BattlePlayer.Emotion.OnPlay += (ClassCharaPrm.EmotionType emoteType) => (emoteType == ClassCharaPrm.EmotionType.LOSE || emoteType == ClassCharaPrm.EmotionType.SURRENDER_LOSE || ClassCharaPrm.IsEvolutionEmotionType(emoteType)) ? ((VfxBase)NullVfx.GetInstance()) : ((VfxBase)InstantVfx.Create(delegate
		{
			SendChatStamp(emoteType);
		}));
	}

	public override void SetUpOperateEvent(OperateMgr operateMgr)
	{
		base.SetUpOperateEvent(operateMgr);
		operateEvent_OperateMgr = operateMgr;
		SetUpNetworkOperateEvent();
	}

	private void SetUpNetworkOperateEvent()
	{
		OperateMgr operateMgr = operateEvent_OperateMgr;
		operateMgr.OnBeforeSetCard += delegate
		{
			IsCardPlayToTurnEndTimeoutStop = true;
		};
		operateMgr.OnSetCard += delegate(BattleCardBase card)
		{
			NowPlayCard = card;
		};
		if (base.IsRecovery)
		{
			return;
		}
		operateMgr.OnSetCardSuccess += delegate(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCard)
		{
			SettingPlaySelectCard(card.IsChoiceBraveSkillCard ? originalCard : card, selectedCard);
		};
		operateMgr.OnSetCardComplete += delegate
		{
			if (_operatePlayCard != null && _operatePlayCard.IsPlayer)
			{
				SendPlayCard(_operatePlayCard, _operatePlaySelectCard, sendKeyActionDataManager);
				initSelectData();
			}
			IsCardPlayToTurnEndTimeoutStop = false;
			return NullVfx.GetInstance();
		};
		operateMgr.OnEvolveSuccess += delegate(BattleCardBase originalCard, BattleCardBase card, IEnumerable<BattleCardBase> selectedCard)
		{
			SettingPlaySelectCard(card, selectedCard);
		};
		operateMgr.OnEvoleComplete += delegate
		{
			if (_operatePlayCard != null && _operatePlayCard.IsPlayer)
			{
				SendEvolveData(_operatePlayCard, _operatePlaySelectCard, sendKeyActionDataManager);
				initSelectData();
			}
			return NullVfx.GetInstance();
		};
		operateMgr.OnPlayerAttack += delegate(BattleCardBase attackCard, BattleCardBase targetCard)
		{
			SendAttackData(attackCard, targetCard);
			return NullVfx.GetInstance();
		};
		operateMgr.OnBeforeFusion += delegate(BattleCardBase card, IEnumerable<BattleCardBase> selectedCard)
		{
			SettingPlaySelectCard(card, selectedCard);
		};
		operateMgr.OnAfterFusion += delegate
		{
			if (_operatePlayCard != null && _operatePlayCard.IsPlayer)
			{
				SendFusionData(_operatePlayCard, _operatePlaySelectCard, sendKeyActionDataManager);
				initSelectData();
			}
			return NullVfx.GetInstance();
		};
	}

	private void SettingPlaySelectCard(BattleCardBase card, IEnumerable<BattleCardBase> selectedCards)
	{
		if (!card.IsPlayer)
		{
			return;
		}
		_operatePlayCard = card;
		if (selectedCards != null)
		{
			List<BattleCardBase> list = new List<BattleCardBase>();
			List<BattleCardBase> list2 = selectedCards.ToList();
			foreach (BattleCardBase item in list2)
			{
				if (NetworkBattleGenericTool.GetCardPlaceState(item.SelfBattlePlayer, item.Index) == NetworkBattleDefine.NetworkCardPlaceState.None)
				{
					list.Add(item);
				}
			}
			foreach (BattleCardBase item2 in list)
			{
				list2.Remove(item2);
			}
			_operatePlaySelectCard = list2.ToList();
		}
		else
		{
			_operatePlaySelectCard = null;
		}
	}

	private void initSelectData()
	{
		_operatePlayCard = null;
		_operatePlaySelectCard = null;
	}

	public NetworkBattleReceiver GetNetworkBattleReceiver()
	{
		return networkReceiver;
	}

	public override void DisposeBattleGameObj()
	{
		LocalLog.AccumulateLastTraceLog("DisposeBattleGameObj");
		base.DisposeBattleGameObj();
		BattleFinishToEffectClear();
		BattleFinishToStopIntervalChecker();
		if (this.InstanceNetworkAgent is { } _agentToDestroy) { UnityEngine.Object.DestroyImmediate(_agentToDestroy.gameObject); this.InstanceNetworkAgent = null; }
		StopJudgeResultCoroutine();
		StopReconnectCorutine();
		// End-of-battle GameMgr flag resets dropped in Phase-3: three of the four flags are
		// const-false in Phase-4 (IsWatchBattle / IsReplayBattle / IsNewReplayBattle) and can't
		// be assigned; IsNetworkBattle stays true for the node's whole lifetime (the node IS a
		// network battle) so resetting it here would be wrong for a subsequent battle anyway.
		SettingNetworkBattleEnd();
	}

	public void SettingNetworkBattleEnd()
	{
		IsNetworkBattleEnd = true;
	}

	protected override int GetFirstAttack(int FirstAttack)
	{
		return FirstAttack;
	}

	public override void SetupCardEvent(BattleCardBase card)
	{
		base.SetupCardEvent(card);
		_networkBattleSetupCardEventBase.SetupCardEvent(this, RegisterActionManager, card, RegisterUnapprovedList);
	}

	public virtual bool IsSkillConditionCheckSkill(int cardIdx)
	{
		if (networkBattleData.GetReceiveData() == null)
		{
			return false;
		}
		if (networkBattleData.GetReceiveData().SkillConditionCheckList.Find((CardDataModel x) => x.Index == cardIdx) == null)
		{
			return false;
		}
		return true;
	}

	public override BattleCardBase MetamorphoseCard(int metemprphoseID, bool isPlayer, int index, SkillBase skill, bool isFusion = false)
	{
		bool flag = false;
		BattleCardBase battleCardBase = null;
		// IsAdmin is const-false in headless — `!IsAdmin` is a tautology; dropped from the guard.
		if (!isPlayer && NetworkBattleGenericTool.GetCardPlaceState(BattleEnemy, index) == NetworkBattleDefine.NetworkCardPlaceState.Hand)
		{
			flag = true;
			battleCardBase = NetworkBattleGenericTool.GetIndexToCardBase(this, BattleEnemy, index);
		}
		BattleCardBase battleCardBase2 = base.MetamorphoseCard(metemprphoseID, isPlayer, index, skill);
		RegisterActionManager.Add(new RegisterMetamorphoseData(battleCardBase2.CardId, index, isPlayer, skill, isChoice: false, isFirstOnly: false, isFusion));
		if (flag && !isFusion)
		{
			battleCardBase2 = base.MetamorphoseCard(battleCardBase.BaseParameter.BaseCardId, isPlayer, index, skill);
		}
		return battleCardBase2;
	}

	public override BattleCardBase CreateBattleCardWithGameObject(CardCreateInfo info, IndexInfo infoIndex, int repeatCount = -1, bool isVirtual = false, bool isActualCard = false)
	{
		if (!isActualCard && infoIndex.AddIndex == -1 && IsNotReplaceCardListAny(GetBattlePlayer(info.IsPlayer).cardTotalNum, info.IsPlayer))
		{
			ReplaceReceivedCard.CardIdAndIndex andRemoveNotReplaceCard = GetAndRemoveNotReplaceCard(GetBattlePlayer(info.IsPlayer).cardTotalNum, info.IsPlayer);
			info.SetId(andRemoveNotReplaceCard.CardId);
			info.SetCost(andRemoveNotReplaceCard.Cost);
		}
		BattleCardBase battleCardBase = CreateBattleCardFunc(info, infoIndex, repeatCount, isVirtual, isActualCard);
		if (infoIndex.IsSkillCopy)
		{
			RegisterActionManager.Add(new RegisterCopyToken(battleCardBase, info.IsChoice, info.PlaceStatus, infoIndex.CopySkillSelectIndex, info.Skill));
		}
		else if ((info.PlaceStatus == NetworkBattleDefine.NetworkCardPlaceState.Hand || info.PlaceStatus == NetworkBattleDefine.NetworkCardPlaceState.Deck) && infoIndex.TargetIndex != -1 && !info.IsReferenceOpponenCard)
		{
			RegisterActionManager.Add(new RegisterCopyToken(battleCardBase, info.IsChoice, info.PlaceStatus, infoIndex.TargetIndex, info.Skill));
		}
		else if (info.PlaceStatus != NetworkBattleDefine.NetworkCardPlaceState.None)
		{
			RegisterActionManager.Add(new RegisterToken(battleCardBase, info.IsChoice, info.PlaceStatus, info.Skill, repeatCount));
		}
		else
		{
			LocalLog.AccumulateLastTraceLog("NonRegisterToken addIndex " + infoIndex.AddIndex);
		}
		return battleCardBase;
	}

	public override VfxBase PlaySpecialWin(BattlePlayerBase winPlayer)
	{
		RegisterActionManager.Add(new RegisterSpecialWin(winPlayer.IsPlayer));
		BattleFinishToEffectClear();
		base.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
		_isSendSpecialWin = true;
		bool playerDead = !winPlayer.IsPlayer;
		_isSpecialWin = winPlayer.IsPlayer;
		return SequentialVfxPlayer.Create(DeadClass(playerDead, FINISH_TYPE.SPECIAL_WIN));
	}

	protected override SequentialVfxPlayer OnShortageDeck(BattlePlayerBase battlePlayer)
	{
		if (battlePlayer.IsShortageDeckWin)
		{
			BattleFinishToEffectClear();
			base.VfxMgr.RegisterImmediateVfx(NullVfx.GetInstance());
			_isSendSpecialWin = true;
			_isSpecialWin = battlePlayer.IsPlayer;
		}
		return base.OnShortageDeck(battlePlayer);
	}

	public void SendJudge()
	{
		SendJudgement();
	}

	public virtual void ReceiveRetire(bool isWin)
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.ReceiveRetire);
	}

	public void ReceiveConsistencyLose()
	{
		JudgeResultReceiveCode = NetworkBattleReceiver.RESULT_CODE.NoContest;
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.ReceiveConsistencyLose);
	}

	public void ReceiveInvalidLose()
	{
		JudgeResultReceiveCode = NetworkBattleReceiver.RESULT_CODE.NoContest;
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.Invalid);
	}

	public void OppoDisconnectVictory()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.OppoDisconnectVictory);
	}

	protected void OpponentNotMulliganEndVictory()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.OppoNotMulliganVictory);
	}

	protected void OpponentNotTurnStartVictory()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.OpponentNotTurnStartVictory);
	}

	protected void OpponentNotTurnEndVictory()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.OpponentNotTurnEndVictory);
	}

	protected void DispOpponentDisconnect(bool flag)
	{
		try
		{
			if (IsBattleEnd)
			{
				return;
			}
			IClassCharacter classCharacter = ((IClassBattleCardView)BattleEnemy.Class.BattleCardView).ClassCharacter;
			if (flag)
			{
				if (!IsShowDisconnectPanel)
				{
					VfxBase vfx = classCharacter.SetWaiting(flag: true);
					base.VfxMgr.RegisterImmediateVfx(vfx);
				}
				IsShowOpponentDisconnectPanel = true;
			}
			else
			{
				VfxBase vfx2 = classCharacter.SetWaiting(flag: false);
				base.VfxMgr.RegisterImmediateVfx(vfx2);
				IsShowOpponentDisconnectPanel = false;
			}
		}
		catch
		{
		}
	}

	protected void DispOpponentRecovery(bool flag)
	{
		IClassCharacter classCharacter = ((IClassBattleCardView)BattleEnemy.Class.BattleCardView).ClassCharacter;
		if (flag)
		{
			VfxBase vfx = classCharacter.SetRecovery(flag: true);
			base.VfxMgr.RegisterImmediateVfx(vfx);
		}
		else
		{
			VfxBase vfx2 = classCharacter.SetRecovery(flag: false);
			base.VfxMgr.RegisterImmediateVfx(vfx2);
		}
	}

	protected virtual void ControlDisconnectOffTouchAndView(bool flag)
	{
		try
		{
			if (flag)
			{
				RealTimeNetworkAgent.ReconnectSocketAndLogFlagOn();
				_waitToReconnectSocketCoroutine = BattleCoroutine.GetInstance().StartCoroutine(WaitToReconnectSocket());
				if (IsShowOpponentDisconnectPanel)
				{
					VfxBase vfx = ((IClassBattleCardView)BattleEnemy.Class.BattleCardView).ClassCharacter.SetWaiting(flag: false);
					base.VfxMgr.RegisterImmediateVfx(vfx);
				}
				SelfDisconnectOffTouch();
				BattlePlayer.PlayerBattleView.ShowAlert(PanelMgr.BattleAlertType.DisconnectInfomation, isClass: false);
				IsShowDisconnectPanel = true;
			}
			else
			{
				StopReconnectCorutine();
				if (IsShowOpponentDisconnectPanel)
				{
					VfxBase vfx2 = ((IClassBattleCardView)BattleEnemy.Class.BattleCardView).ClassCharacter.SetWaiting(flag: true);
					base.VfxMgr.RegisterImmediateVfx(vfx2);
				}
				SelfDisconnectOffTouchRelease();
				BattlePlayer.PlayerBattleView.OffNotHideAndNotCreate();
				BattlePlayer.PlayerBattleView.HideAlertDialogue();
				IsShowDisconnectPanel = false;
			}
		}
		catch
		{
		}
	}

	protected virtual void FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS judgeResultStatus, bool isWin = false, bool isNotRetry = false)
	{
		LocalLog.AccumulateLastTraceLog("FinishSend " + judgeResultStatus);
		judgeResult_NotFinishNum = 0;
		SendJudgeResultToFinishBattleTask(judgeResultStatus, isNotRetry);
	}

	private void SendJudgeResultToFinishBattleTask(NetworkBattleSender.JUDGE_RESULT_STATUS judgeResultStatus, bool isNotRetry)
	{
		if (!IsNetworkBattleEnd)
		{
			NetworkSender.SendJudgeResult(judgeResultStatus);
			if (!isNotRetry)
			{
				StopJudgeResultCoroutine();
				_checkJudgeResultToDisconnectCoroutine = BattleCoroutine.GetInstance().StartCoroutine(CheckJudgeResultToDisconnect());
			}
		}
	}

	private IEnumerator WaitToReconnectSocket()
	{
		WaitForSeconds wait = new WaitForSeconds(16f);
		while (true)
		{
			yield return wait;
			RealTimeNetworkAgent.ReconnectSocketAndLogFlagOn();
		}
	}

	private void StopReconnectCorutine()
	{
		if (_waitToReconnectSocketCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(_waitToReconnectSocketCoroutine);
		}
	}

	protected IEnumerator CheckJudgeResultToDisconnect()
	{
		while (!_isJudgeResultReceive)
		{
			yield return null;
			if (IsBackTitleOnDisconnect())
			{
				JudgeErrorDialog(isError: false);
				break;
			}
		}
		_checkJudgeResultToDisconnectCoroutine = null;
	}

	public virtual void SendFinishBattleTask()
	{
		BattlePlayer.BattleView.HideTurnEndButton();
		this.InstanceNetworkAgent.FinishBattleTask();
	}

	private IEnumerator SendJudgeResultRetry()
	{
		long matchedTimer = TimeUtil.GetAbsoluteTime().Ticks;
		do
		{
			yield return null;
		}
		while (!((float)NetworkUtility.GetTimeSpanSecond(matchedTimer) >= 5f));
		SendJudgeResultToFinishBattleTask(NetworkBattleSender.JUDGE_RESULT_STATUS.RetrySend, isNotRetry: false);
	}

	public void JudgeResultReceive(NetworkBattleReceiver.RESULT_CODE result, bool isNotStopCoroutine = false)
	{
		if (_isJudgeResultReceive)
		{
			return;
		}
		JudgeResultReceiveCode = result;
		switch (result)
		{
		case NetworkBattleReceiver.RESULT_CODE.NotFinish:
			judgeResult_NotFinishNum++;
			if (!_isNoLimitJudgeResult && judgeResult_NotFinishNum >= 5)
			{
				judgeResult_NotFinishNum = 0;
				StopJudgeResultCoroutine();
				judgeResultFailedToRetryChecker.StartChecker();
			}
			else if (!judgeResultFailedToRetryChecker.IsStarted())
			{
				if (_resultRetryCoroutine != null)
				{
					BattleCoroutine.GetInstance().StopCoroutine(_resultRetryCoroutine);
					_resultRetryCoroutine = null;
				}
				_resultRetryCoroutine = BattleCoroutine.GetInstance().StartCoroutine(SendJudgeResultRetry());
			}
			break;
		case NetworkBattleReceiver.RESULT_CODE.LifeWin:
		case NetworkBattleReceiver.RESULT_CODE.DeckoutWin:
		case NetworkBattleReceiver.RESULT_CODE.RetireWin:
		case NetworkBattleReceiver.RESULT_CODE.SpecialWin:
		case NetworkBattleReceiver.RESULT_CODE.DisconnectWin:
		case NetworkBattleReceiver.RESULT_CODE.FirstcardWin:
		case NetworkBattleReceiver.RESULT_CODE.TurnendWin:
		case NetworkBattleReceiver.RESULT_CODE.TurnstartWin:
			_isJudgeResultReceive = true;
			SettingResultUI_SpecialResultTypeText(BATTLE_RESULT_TYPE.LOSE);
			break;
		case NetworkBattleReceiver.RESULT_CODE.LifeLose:
		case NetworkBattleReceiver.RESULT_CODE.DeckoutLose:
		case NetworkBattleReceiver.RESULT_CODE.RetireLose:
		case NetworkBattleReceiver.RESULT_CODE.SpecialLose:
		case NetworkBattleReceiver.RESULT_CODE.DisconnectLose:
		case NetworkBattleReceiver.RESULT_CODE.FirstcardLose:
		case NetworkBattleReceiver.RESULT_CODE.TurnendLose:
		case NetworkBattleReceiver.RESULT_CODE.TurnstartLose:
			_isJudgeResultReceive = true;
			SettingResultUI_SpecialResultTypeText(BATTLE_RESULT_TYPE.WIN);
			break;
		case NetworkBattleReceiver.RESULT_CODE.MaxTurnLose:
		{
			_isJudgeResultReceive = true;
			SettingResultUI_SpecialResultTypeText(BATTLE_RESULT_TYPE.WIN);
			BattlePlayer.Class.FlagCardAsDestroyedBySkill();
			BattleEnemy.Class.FlagCardAsDestroyedBySkill();
			VfxBase vfxBase = BattlePlayer.CardManagement(BattlePlayer.Class, null, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false);
			VfxBase vfxBase2 = BattleEnemy.CardManagement(BattleEnemy.Class, null, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false);
			base.VfxMgr.RegisterSequentialVfx(ParallelVfxPlayer.Create(vfxBase, vfxBase2));
			break;
		}
		case NetworkBattleReceiver.RESULT_CODE.NoContest:
		case NetworkBattleReceiver.RESULT_CODE.Invalid:
			_isJudgeResultReceive = true;
			SettingResultUI_SpecialResultTypeText(BATTLE_RESULT_TYPE.CONSISTENCY);
			break;
		case NetworkBattleReceiver.RESULT_CODE.Error:
			JudgeErrorDialog(isError: true);
			break;
		}
		if (_isJudgeResultReceive && !isNotStopCoroutine)
		{
			StopJudgeResultCoroutine();
			SendFinishBattleTask();
			ReceiveStop();
			Screen.sleepTimeout = -2;
		}
	}

	private void StopJudgeResultCoroutine()
	{
		if (_checkJudgeResultToDisconnectCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(_checkJudgeResultToDisconnectCoroutine);
			_checkJudgeResultToDisconnectCoroutine = null;
		}
		if (_resultRetryCoroutine != null)
		{
			BattleCoroutine.GetInstance().StopCoroutine(_resultRetryCoroutine);
			_resultRetryCoroutine = null;
		}
	}

	protected void JudgeErrorDialog(bool isError)
	{
		if (!IsNetworkBattleEnd)
		{
			UIManager instance = UIManager.GetInstance();
			instance.dialogAllClear();
			SettingNetworkBattleEnd();
			if (this.InstanceNetworkAgent is { } _agentToDestroy) { UnityEngine.Object.DestroyImmediate(_agentToDestroy.gameObject); this.InstanceNetworkAgent = null; }
			DialogBase dialogBase = instance.CreateDialogClose(isSystem: true);
			dialogBase.SetSize(DialogBase.Size.M);
			if (isError)
			{
				dialogBase.SetTitleLabel(Data.SystemText.Get("ErrorHeader_0015"));
				dialogBase.SetText(Data.SystemText.Get("Error_0015"));
			}
			else
			{
				dialogBase.SetTitleLabel(Data.SystemText.Get("ErrorHeader_0014"));
				dialogBase.SetText(Data.SystemText.Get("Error_0014"));
			}
			dialogBase.AddButton(DialogBase.ButtonType.BackToTitle);
			dialogBase.SetPanelDepth(5000);
			dialogBase.SetFadeButtonEnabled(flag: false);
			BattleFinishToStopIntervalChecker();
			instance.closeInSceneCenterLoading();
			BattlePlayer.PlayerBattleView.OffNotHideAndNotCreate();
			BattlePlayer.PlayerBattleView.HideAlertDialogue();
		}
	}

	protected void BeforeDisconnectLose()
	{
		if (this.InstanceNetworkAgent != null)
		{
			this.InstanceNetworkAgent.ReconnectSocket();
		}
	}

	protected virtual void DisconnectLose()
	{
	}

	public void BattleFinishReceiveAfterFinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS log, bool isWin = false)
	{
		FinishBattleSend(log, isWin);
	}

	public NetworkBattleReceiver.RESULT_CODE JudgeCurrentFinishStatus()
	{
		if (BattlePlayer.Class.IsDead && BattleEnemy.Class.IsDead)
		{
			if ((BattlePlayer.Class.IsDead && BattlePlayer.DeckCardList.Count == 0) || (BattleEnemy.Class.IsDead && BattleEnemy.DeckCardList.Count == 0))
			{
				if (BattlePlayer.IsSelfTurn)
				{
					return NetworkBattleReceiver.RESULT_CODE.DeckoutLose;
				}
				return NetworkBattleReceiver.RESULT_CODE.DeckoutWin;
			}
			if (BattlePlayer.Class.Life <= 0 || BattleEnemy.Class.Life <= 0)
			{
				if (BattlePlayer.IsSelfTurn)
				{
					return NetworkBattleReceiver.RESULT_CODE.LifeLose;
				}
				return NetworkBattleReceiver.RESULT_CODE.LifeWin;
			}
		}
		if (BattlePlayer.Class.Life <= 0)
		{
			return NetworkBattleReceiver.RESULT_CODE.LifeLose;
		}
		if (BattleEnemy.Class.Life <= 0)
		{
			return NetworkBattleReceiver.RESULT_CODE.LifeWin;
		}
		if (BattlePlayer.Class.IsDead && BattlePlayer.DeckCardList.Count == 0)
		{
			return NetworkBattleReceiver.RESULT_CODE.DeckoutLose;
		}
		if (BattleEnemy.Class.IsDead && BattleEnemy.DeckCardList.Count == 0)
		{
			return NetworkBattleReceiver.RESULT_CODE.DeckoutWin;
		}
		if (BattlePlayer.IsShortageDeck && BattlePlayer.IsShortageDeckWin && BattleEnemy.IsShortageDeck && BattleEnemy.IsShortageDeckWin)
		{
			if (BattlePlayer.IsSelfTurn)
			{
				return NetworkBattleReceiver.RESULT_CODE.SpecialLose;
			}
			return NetworkBattleReceiver.RESULT_CODE.SpecialWin;
		}
		if (BattlePlayer.IsShortageDeck && BattlePlayer.IsShortageDeckWin)
		{
			return NetworkBattleReceiver.RESULT_CODE.DeckoutWin;
		}
		if (BattleEnemy.IsShortageDeck && BattleEnemy.IsShortageDeckWin)
		{
			return NetworkBattleReceiver.RESULT_CODE.DeckoutLose;
		}
		if (_isSendSpecialWin)
		{
			if (_isSpecialWin)
			{
				return NetworkBattleReceiver.RESULT_CODE.SpecialWin;
			}
			return NetworkBattleReceiver.RESULT_CODE.SpecialLose;
		}
		return NetworkBattleReceiver.RESULT_CODE.NotFinish;
	}

	private bool IsBackTitleOnDisconnect()
	{
		if (disconnectToLoseChecker.IsSelfDisConnectOnTimeout())
		{
			return true;
		}
		return false;
	}

	public override VfxBase JudgeBattleResult()
	{
		if (BattlePlayer.IsSelfTurn)
		{
			if (base.IsRecovery && (JudgeCurrentFinishStatus() == NetworkBattleReceiver.RESULT_CODE.DeckoutLose || JudgeCurrentFinishStatus() == NetworkBattleReceiver.RESULT_CODE.DeckoutWin))
			{
				return NullVfx.GetInstance();
			}
			if (JudgeCurrentFinishStatus() != NetworkBattleReceiver.RESULT_CODE.NotFinish)
			{
				RealTimeNetworkAgent realTimeNetworkAgent = this.InstanceNetworkAgent;
				realTimeNetworkAgent.OnAck = (Action<Dictionary<string, object>>)Delegate.Combine(realTimeNetworkAgent.OnAck, new Action<Dictionary<string, object>>(AckEmitBattleFinish));
			}
		}
		else if (JudgeCurrentFinishStatus() != NetworkBattleReceiver.RESULT_CODE.NotFinish)
		{
			BattleFinishToTurnEndFinal(isSelfTurn: false);
		}
		if (IsBattleGameFinishStatus())
		{
			notTurnStartToLoseChecker.EndTimer();
			notTurnEndToLoseChecker.EndTimer();
		}
		return NullVfx.GetInstance();
	}

	protected virtual void AckEmitBattleFinish(Dictionary<string, object> objs)
	{
		RealTimeNetworkAgent realTimeNetworkAgent = this.InstanceNetworkAgent;
		realTimeNetworkAgent.OnAck = (Action<Dictionary<string, object>>)Delegate.Remove(realTimeNetworkAgent.OnAck, new Action<Dictionary<string, object>>(AckEmitBattleFinish));
		BattleFinishToTurnEndFinal(isSelfTurn: true);
	}

	private void AddRegisterMoveCard(BattleCardBase card, NetworkBattleDefine.NetworkCardPlaceState from, NetworkBattleDefine.NetworkCardPlaceState to, SkillBase skill, bool isNotCheck = false, bool isOpen = false, bool isFlood = false, bool isWhenDraw = false, bool hasGuard = false)
	{
		bool flag = false;
		if (isNotCheck)
		{
			flag = true;
		}
		else if (from != NetworkBattleDefine.NetworkCardPlaceState.None && from != NetworkBattleDefine.NetworkCardPlaceState.Banish && from != NetworkBattleDefine.NetworkCardPlaceState.FusionIngredient && from != NetworkBattleDefine.NetworkCardPlaceState.Unite)
		{
			flag = true;
		}
		if (flag)
		{
			RegisterActionManager.Add(new RegisterStateChangeCard(card, from, to, skill, isOpen, isFlood, isWhenDraw, hasGuard));
		}
	}

	public virtual void BattleFinishToTurnEndFinal(bool isSelfTurn)
	{
		BattleFinishToEffectClear();
		foreach (NetworkBattleIntervalCheckerBase intervalCheck in _intervalCheckList)
		{
			if (!(intervalCheck is OpponentNotTurnEndToWinChecker) && !(intervalCheck is OpponentNotTurnStartToWinChecker) && !(intervalCheck is BattleFinishToOpponentDisConnectChecker) && !(intervalCheck is DisconnectToLoseChecker))
			{
				intervalCheck.StopChecker();
			}
		}
	}

	protected void SettingResultUI_SpecialResultTypeText(BATTLE_RESULT_TYPE battleResult)
	{
		string text = "";
		NetworkBattleReceiver.RESULT_CODE judgeResultReceiveCode = JudgeResultReceiveCode;
		if (battleResult == BATTLE_RESULT_TYPE.LOSE)
		{
			if (judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.RetireWin)
			{
				text = Data.SystemText.Get("Battle_0418");
			}
			else if (judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.DisconnectWin || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.FirstcardWin || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.TurnendWin || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.TurnstartWin || (_isNodeErrorToNocontest && judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.NotFinish))
			{
				text = Data.SystemText.Get("Battle_0420");
			}
		}
		else if (judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.DisconnectLose || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.FirstcardLose || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.TurnendLose || judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.TurnstartLose || (_isNodeErrorToNocontest && judgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.NotFinish))
		{
			text = Data.SystemText.Get("Battle_0421");
		}
		else
		{
			switch (judgeResultReceiveCode)
			{
			case NetworkBattleReceiver.RESULT_CODE.MaxTurnLose:
				text = Data.SystemText.Get("Battle_0498");
				break;
			case NetworkBattleReceiver.RESULT_CODE.NoContest:
			case NetworkBattleReceiver.RESULT_CODE.Invalid:
				text = Data.SystemText.Get("Battle_0419");
				break;
			}
		}
		if (text != "")
		{
			BattleResultControl.SetSpecialResultTypeText(text);
		}
		_finishEffectType = battleResult;
	}

	public void ReceiveStop()
	{
		networkReceiver.ReceiveStop();
	}

	public void ConductReceiveData(NetworkBattleReceiver.ReceiveData receiveData, bool isPlayer = false)
	{
		bool isExtraTurn = BattlePlayer.IsExtraTurn;
		networkBattleData.SetReceiveData(receiveData);
		networkBattleData.BeforeSettingReceiveData();
		if (base.CurrentTurn <= 1 && !isPlayer && (receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.TurnStart || receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.PlayActions))
		{
			LocalLog.AccumulateLastTraceLog("Conduct " + receiveData.dataUri.ToString() + " " + Convert.ToInt32(_isJudgeResultReceive));
		}
		if (!_isJudgeResultReceive)
		{
			NetworkOperationCollectionBase networkOperationCollection = CreateNetworkOperationCollection(receiveData, isPlayer);
			OperateReceive.StartOperate(networkOperationCollection, receiveData);
		}
		receiveIntervalTrigger.ReceiveDataCheck(this, networkBattleData, isPlayer, isExtraTurn);
		networkBattleData.AfterSettingReceiveData();
	}

	protected virtual NetworkOperationCollectionBase CreateNetworkOperationCollection(NetworkBattleReceiver.ReceiveData receivedData, bool isPlayer)
	{
		if (base.IsRecovery)
		{
			return new RecoveryOperationCollection(this, OperateMgr, receivedData, networkBattleData, isPlayer);
		}
		if (!IsOperateReceiveCheck())
		{
			LocalLog.AccumulateTraceLog("ConductError");
			return new NullOperationCollection();
		}
		return new NetworkOperationCollection(this, OperateMgr, receivedData, networkBattleData, isPlayer);
	}

	protected virtual bool IsOperateReceiveCheck()
	{
		return operateReceiveChecker.IsOperateReceive();
	}

	public void ConductReceiveData_NotHaveSequence(NetworkBattleReceiver.ReceiveData receiveData, bool isPlayer)
	{
		networkBattleData.SetReceiveData(receiveData);
		NetworkOperationCollectionBase networkOperationCollection = CreateNetworkOperationCollection(receiveData, isPlayer);
		OperateReceive.StartOperate(networkOperationCollection, receiveData);
		receiveIntervalTrigger.ReceiveDataCheck(this, networkBattleData, isPlayer, isExTurn: false);
	}

	public List<BattleCardBase> GetUnapprovedCardObj(BattlePlayerBase player, int skillCardIndex, int publishedActiveCount, int movement, SkillBase skill)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<CardDataModel> unapprovedList = networkBattleData.GetReceiveData().unapprovedList;
		if (unapprovedList == null)
		{
			return list;
		}
		int num = -1;
		if (skill.ApplySelectFilter is SkillRandomSelectFilter skillRandomSelectFilter && !skill.IsOnceCallTiming)
		{
			num = skillRandomSelectFilter.CalcCount(skill.OptionValue);
		}
		foreach (CardDataModel item in unapprovedList)
		{
			if (item.Index == -99)
			{
				continue;
			}
			if (RegisterSkillConditionCheck.DoesSkillUsePrivateCount(skill) && skill is Skill_powerup)
			{
				int movementCount = RegisterSkillConditionCheck.GetMovementCount(skill);
				if (item.skillMovementNum / movementCount != movement)
				{
					continue;
				}
			}
			else if (item.skillMovementNum != movement)
			{
				continue;
			}
			if (item.skillCardIndex != skillCardIndex || item.publishedActiveSkillCount != publishedActiveCount || item.IsInvoked != skill.IsInvoked || item.IsGotUnapproved)
			{
				continue;
			}
			BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(this, player, item.Index);
			if (skill is NetworkSkill_metamorphose && skill.OnWhenAddToHand != 0 && item.ToStateList.Any((NetworkBattleDefine.NetworkCardPlaceState s) => s == NetworkBattleDefine.NetworkCardPlaceState.Hand) && (indexToCardBase == null || !indexToCardBase.IsInHand))
			{
				item.skillMovementNum++;
				continue;
			}
			if (indexToCardBase != null)
			{
				if (skill.OnWhenDrawOtherStart != 0 && item.ToStateList.Any((NetworkBattleDefine.NetworkCardPlaceState s) => s == NetworkBattleDefine.NetworkCardPlaceState.Hand) && !indexToCardBase.IsInHand)
				{
					continue;
				}
				list.Add(indexToCardBase);
				if (!skill.IsOnceCallTiming)
				{
					item.IsGotUnapproved = true;
				}
			}
			if (num != -1 && list.Count >= num)
			{
				break;
			}
		}
		return list;
	}

	public bool IsContainUnapprovedSkill(SkillBase skill, int skillCardIndex, int publishedActiveCount, int movement)
	{
		List<CardDataModel> unapprovedList = networkBattleData.GetReceiveData().unapprovedList;
		if (unapprovedList == null)
		{
			return false;
		}
		foreach (CardDataModel item in unapprovedList)
		{
			if (item.Index == -99)
			{
				continue;
			}
			if (RegisterSkillConditionCheck.DoesSkillUsePrivateCount(skill) && skill is Skill_powerup)
			{
				int movementCount = RegisterSkillConditionCheck.GetMovementCount(skill);
				if (item.skillMovementNum / movementCount != movement)
				{
					continue;
				}
			}
			else if (item.skillMovementNum != movement)
			{
				continue;
			}
			if (item.skillCardIndex == skillCardIndex && item.publishedActiveSkillCount == publishedActiveCount && item.IsInvoked == skill.IsInvoked)
			{
				return true;
			}
		}
		return false;
	}

	public List<int> GetValidateTargetSkillIndexList()
	{
		return networkBattleData.GetReceiveData().validateSkillIndexList;
	}

	public virtual bool IsReceivedSkillConditionCheck(int movement, SkillBase skill)
	{
		bool isPlayer = skill.SkillPrm.ownerCard.IsPlayer;
		int index = skill.SkillPrm.ownerCard.Index;
		int num = skill.SkillPrm.ownerCard.Skills.IndexOf(skill);
		int publishSkillCount = NetworkBattleGenericTool.GetPublishSkillCount(skill);
		int movementCount = RegisterSkillConditionCheck.GetMovementCount(skill);
		foreach (CardDataModel skillConditionCheck in networkBattleData.GetReceiveData().SkillConditionCheckList)
		{
			bool flag = false;
			if (skillConditionCheck.publishedActiveSkillCount == publishSkillCount)
			{
				flag = true;
			}
			else if (skillConditionCheck.publishedActiveSkillCount != publishSkillCount && skillConditionCheck.SkillIndex == num)
			{
				flag = true;
			}
			bool flag2 = skillConditionCheck.skillMovementNum / movementCount == movement;
			bool flag3 = skillConditionCheck.IsInvoked == skill.IsInvoked;
			if (flag && skillConditionCheck.Index == index && flag2 && flag3)
			{
				// IsWatchBattle const-false in headless — the guarded `return false` was watch-mode
				// dead code.
				return skillConditionCheck.activate == 1;
			}
		}
		return false;
	}

	public List<CardDataModel> SearchSkillConditionCheckDataList(int cardIdx, int publishSkillCount, int skillMovement, int skillConditionCount)
	{
		if (networkBattleData.GetReceiveData() == null)
		{
			return null;
		}
		List<CardDataModel> list = new List<CardDataModel>();
		foreach (CardDataModel skillConditionCheck in networkBattleData.GetReceiveData().SkillConditionCheckList)
		{
			if (skillConditionCheck.Index == cardIdx && skillConditionCheck.publishedActiveSkillCount == publishSkillCount && skillConditionCheck.skillMovementNum / skillConditionCount == skillMovement)
			{
				list.Add(skillConditionCheck);
			}
		}
		return list;
	}

	public void ClearRegisterCardList()
	{
		if (base.CurrentTurn <= 1 && !BattlePlayer.IsSelfTurn)
		{
			LocalLog.AccumulateLastTraceLog("ClearRegister" + RegisterActionManager.RegisterDataList.Count);
		}
		validateSkillIndexList.Clear();
		RegisterActionManager.Clear();
		RegisterUnapprovedList.Clear();
		registerSelectTypeSkillIndexList.Clear();
		_networkBattleSetupCardEventBase.ClearSkillMovement();
		base.TemporaryPublishedAddCount = 0;
		sendKeyActionDataManager.Clear();
	}

	public void AddValidateSkillIndexList(int validateCardIndex, bool isPlayer, int validateSkillIndex)
	{
		validateSkillIndexList.Add(new ValidateSkillData(validateCardIndex, isPlayer, validateSkillIndex));
	}

	public void AddRegisterSelectTypeSkillIndexList(int index)
	{
		registerSelectTypeSkillIndexList.Add(index);
	}

	private void SelfDisconnectOffTouch()
	{
		LocalLog.AccumulateLastTraceLog("SelfDisconnectOffTouch time " + disconnectToLoseChecker.GetDisconnectTime());
		LocalLog._isSendGungnirLog = true;
		networkTouchControl.IsDisconnect = true;
		networkTouchControl.notDragPlayCardFlag = true;
		networkTouchControl.notAttackFlag = true;
		networkTouchControl.notEmoteFlag = true;
		networkTouchControl.notEvolCardFlag = true;
		isStopOperateFlag = true;
		BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideBtn();
		// IsWatchBattle const-false — `!IsWatchBattle` is a tautology.
		MenuButtonObject.SetActive(value: false);
		BattlePlayer.PlayerBattleView.AllClear(popUpClose: true);
		BattleCardBase hitCard = networkTouchControl._hitCard;
		if (hitCard != null && hitCard.IsOnMove)
		{
			networkTouchControl.StopMovingHandCard(hitCard);
			networkTouchControl.Exit();
		}
		if (networkTouchControl._touchProcessor != null)
		{
			base.VfxMgr.RegisterImmediateVfx(networkTouchControl._touchProcessor.End().Vfx);
		}
	}

	private void SelfDisconnectOffTouchRelease()
	{
		LocalLog.AccumulateLastTraceLog("SelfDisconnectOffTouchRelease");
		networkTouchControl.IsDisconnect = false;
		networkTouchControl.notDragPlayCardFlag = false;
		networkTouchControl.notAttackFlag = false;
		networkTouchControl.notEmoteFlag = false;
		networkTouchControl.notEvolCardFlag = false;
		BattlePlayer.PlayerBattleView.ShowTurnEndButton();
		isStopOperateFlag = false;
		if (!(_phase is NetworkMulliganPhase))
		{
			MenuButtonObject.SetActive(value: true);
		}
		NetworkSender.SendChatStamp("-1");
	}

	public void BattleFinishToEffectClear()
	{
		if (turnEndTimeController != null)
		{
			turnEndTimeController.BattleEndToTraceLog();
			turnEndTimeController.EndCountDown("BattleFinishToEffectClear");
		}
	}

	public void BattleFinishToStopIntervalChecker()
	{
		foreach (NetworkBattleIntervalCheckerBase intervalCheck in _intervalCheckList)
		{
			intervalCheck.FinishChecker();
		}
	}

	private void OnPlayerAlive()
	{
		ConnectionReportTrigger.ConnectionReport(this);
	}

	public virtual void RecoveryRecordSkillTarget(IEnumerable<BattleCardBase> targetCards)
	{
		_contentsCreator.RecoveryRecordManager.RecordSkillTarget(targetCards);
	}

	public IEnumerable<BattleCardBase> RecoverySkillTarget(IEnumerable<BattleCardBase> skillTargets, int targetCount)
	{
		if (this.GameMgr.IsNetworkBattle)
		{
			return skillTargets;
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		for (int i = 0; i < targetCount; i++)
		{
			string cardName = _contentsCreator.RecoveryManager.RecoveryPopSkillTargetCardName();
			BattleCardBase item = BattleEnemy.AllCards.FirstOrDefault((BattleCardBase c) => c.GetName() == cardName);
			list.Add(item);
		}
		return list;
	}

	public override VfxBase StartBattle()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		LocalLog.SetLastTraceLogTurn(1);
		sequentialVfxPlayer.Register(ChangePhase(base.PhaseCreator.CreateMainPhase()));
		if (base.IsRecovery)
		{
			return sequentialVfxPlayer;
		}
		if (IsFirst)
		{
			sequentialVfxPlayer.Register(BattlePlayer.StartTurnControl("First"));
		}
		return sequentialVfxPlayer;
	}

	public override VfxBase ChangePhase(IPhase phase)
	{
		if (phase is NetworkMulliganPhase)
		{
			notMulliganEndToJudgeChecker.StartChecker();
		}
		return base.ChangePhase(phase);
	}

	protected bool IsBattleGameFinishStatus()
	{
		NetworkBattleReceiver.RESULT_CODE rESULT_CODE = JudgeCurrentFinishStatus();
		if (rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.LifeWin || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.LifeLose || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.DeckoutWin || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.DeckoutLose || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.SpecialWin || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.SpecialLose || rESULT_CODE == NetworkBattleReceiver.RESULT_CODE.MaxTurnLose)
		{
			return true;
		}
		return false;
	}

	public int GetExpectCount(int publishedActiveSkillCount)
	{
		CardDataModel cardDataModel = networkBattleData.GetReceiveData().SkillConditionCheckList.FirstOrDefault((CardDataModel s) => s.publishedActiveSkillCount == publishedActiveSkillCount);
		if (cardDataModel != null)
		{
			networkBattleData.GetReceiveData().SkillConditionCheckList.Remove(cardDataModel);
			if (cardDataModel.SkillCallCount != -1)
			{
				return cardDataModel.SkillCallCount;
			}
			if (cardDataModel.SkillValueCount != -1)
			{
				return cardDataModel.SkillValueCount;
			}
			if (cardDataModel.SkillValueParameter.HasValue)
			{
				return cardDataModel.SkillValueParameter.Value;
			}
			if (cardDataModel.IsHighlander)
			{
				return 1;
			}
			if (cardDataModel.activate != -1)
			{
				return cardDataModel.activate;
			}
		}
		return -1;
	}
}
