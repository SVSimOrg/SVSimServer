using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.Phase;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

public class NetworkStandardBattleMgr : NetworkBattleManagerBase
{
	public int[] beforeRevengeCount = new int[2];

	public int[] beforeAvariceCount = new int[2];

	public int[] beforeWrathCount = new int[2];

	private bool _isEmitTurnEndFinal;

	public BattleFinishToOpponentDisConnectChecker battleFinishToOpponentDisConnectChecker { get; private set; }

	public BattleStopChecker battleStopChecker { get; private set; }

	public NetworkStandardBattleMgr(IBattleMgrContentsCreator contentsCreator)
		: this(contentsCreator, new GameMgr())
	{
	}

	// Phase-5 chunk 45: overload accepting a pre-seeded GameMgr.
	public NetworkStandardBattleMgr(IBattleMgrContentsCreator contentsCreator, GameMgr gameMgr)
		: base(contentsCreator, gameMgr)
	{
		if (!base.IsRecovery)
		{
			LocalLog.SetLastTraceLogTurn(0);
		}
		else
		{
			LocalLog.SendLastTraceLog(null);
		}
		battleFinishToOpponentDisConnectChecker = new BattleFinishToOpponentDisConnectChecker(this);
		_intervalCheckList.Add(battleFinishToOpponentDisConnectChecker);
		battleFinishToOpponentDisConnectChecker.OnDisConnectWin += delegate
		{
			OppoDisconnectVictory();
		};
		battleStopChecker = new BattleStopChecker();
		_intervalCheckList.Add(battleStopChecker);
		base.opponentRecoveryToDispChecker.OnDisp += delegate
		{
			DispOpponentRecovery(flag: true);
		};
		base.opponentRecoveryToDispChecker.OnErase += delegate
		{
			DispOpponentRecovery(flag: false);
		};
		base.disconnectToLoseChecker.OnDisconnectLose += delegate
		{
			DisconnectLose();
		};
		base.disconnectToLoseChecker.OnBeforeDisconnectLose += delegate
		{
			BeforeDisconnectLose();
		};
		base.disconnectToLoseChecker.OnDisconnectCheck += delegate
		{
			ServerSendDisconnectCheck();
		};
		base.opponentNotTurnStartToWinChecker.OnOpponentNotTurnStartToWin += delegate
		{
			OpponentNotTurnStartVictory();
		};
		base.opponentNotTurnEndToWinChecker.OnOpponentNotTurnEndToWin += delegate
		{
			OpponentNotTurnEndVictory();
		};
		base.notMulliganEndToJudgeChecker.OnNotMulliganEndJudge += delegate
		{
			NotMulliganToJudge();
		};
		base.notTurnEndToLoseChecker.OnNotTurnEndToLose += delegate
		{
			TurnEndLose();
		};
		base.notTurnStartToLoseChecker.OnNotTurnStartToLose += delegate
		{
			TurnStartLose();
		};
		base.judgeResultFailedToRetryChecker.OnRetry += delegate
		{
			FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.FailedToRetryJudgeResult);
		};
		battleStopChecker.OnBattleStop += delegate
		{
			FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.BattleStopToJudgeResult, isWin: false, isNotRetry: true);
		};
		base.NetworkSender = new NetworkBattleSender(this, RegisterActionManager, base.RegisterUnapprovedList, networkConsistency);
		receiveIntervalTrigger = new ReceiveIntervalTriggerStandard();
		SendIntervalTriggerMain = new SendIntervalTriggerStandard();
		Screen.sleepTimeout = -1;
	}

	public override void SettingOpponentAliveEvent()
	{
		base.SettingOpponentAliveEvent();
		NetworkStatus opponentNetworkStatus = this.InstanceNetworkAgent.OpponentNetworkStatus;
		opponentNetworkStatus.OnOffLine = (Action)Delegate.Combine(opponentNetworkStatus.OnOffLine, new Action(base.OppoDisconnectVictory));
		NetworkStatus opponentNetworkStatus2 = this.InstanceNetworkAgent.OpponentNetworkStatus;
		opponentNetworkStatus2.OnTimeOut = (Action)Delegate.Combine(opponentNetworkStatus2.OnTimeOut, new Action(base.OppoDisconnectVictory));
	}

	protected override void SendTurnStart()
	{
		if (!base.IsRecovery && !IsVirtualBattle)
		{
			base.NetworkSender.SendTurnStart();
		}
	}

	protected override void SendTurnEndAction()
	{
		if (!base.IsRecovery && !(this.InstanceNetworkAgent == null) && !IsVirtualBattle)
		{
			base.NetworkSender.SendTurnEndAction();
			if (!IsBattleGameFinishStatus())
			{
				RealTimeNetworkAgent realTimeNetworkAgent = this.InstanceNetworkAgent;
				realTimeNetworkAgent.OnAck = (Action<Dictionary<string, object>>)Delegate.Combine(realTimeNetworkAgent.OnAck, new Action<Dictionary<string, object>>(AckEmitTurnEndAction));
			}
		}
	}

	public override void SendTurnEnd()
	{
		if (!base.IsRecovery && !(this.InstanceNetworkAgent == null) && !IsVirtualBattle)
		{
			bool isNextTurnTimeDecrement = false;
			bool isNowTurnTimeDecrement = false;
			if (turnEndTimeController != null)
			{
				isNextTurnTimeDecrement = turnEndTimeController.IsNextTurnTimeDecrement;
				isNowTurnTimeDecrement = turnEndTimeController.IsNowTurnTimeDecrement;
			}
			base.NetworkSender.SendTurnEnd(isNextTurnTimeDecrement, isNowTurnTimeDecrement, final: false);
		}
	}

	protected override void SendChatStamp(ClassCharaPrm.EmotionType emoteType)
	{
		if (!base.IsRecovery)
		{
			NetworkBattleSender networkSender = base.NetworkSender;
			int num = (int)emoteType;
			networkSender.SendChatStamp(num.ToString());
		}
	}

	protected override void SendPlayCard(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
		if (!base.IsRecovery && !IsVirtualBattle)
		{
			base.NetworkSender.SendPlayCard(playCard, playSelectCard, base.validateSkillIndexList, sendKeyActionDataManager, base.registerSelectTypeSkillIndexList);
		}
	}

	protected override void SendAttackData(BattleCardBase attackCard, BattleCardBase targetCard)
	{
		if (!base.IsRecovery && !IsVirtualBattle)
		{
			base.NetworkSender.SendAtkData(attackCard, targetCard);
		}
	}

	protected override void SendEvolveData(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
		if (!base.IsRecovery && !IsVirtualBattle)
		{
			base.NetworkSender.SendEvolData(playCard, playSelectCard, base.validateSkillIndexList, sendKeyActionDataManager, base.registerSelectTypeSkillIndexList);
		}
	}

	protected override void SendFusionData(BattleCardBase playCard, List<BattleCardBase> playSelectCard, SendKeyActionDataManager sendKeyActionDataManager)
	{
		if (!base.IsRecovery && !IsVirtualBattle)
		{
			base.NetworkSender.SendFusionData(playCard, playSelectCard, sendKeyActionDataManager, base.registerSelectTypeSkillIndexList);
		}
	}

	public void SendRetire()
	{
		if (!base.IsRecovery)
		{
			base.NetworkSender.SendRetire();
		}
	}

	protected override void SendJudgement()
	{
		if (!base.IsRecovery && !BattlePlayer.IsSelfTurn)
		{
			base.NetworkSender.SendJudge();
		}
	}

	public override void SendEcho(int playIndex, NetworkBattleDefine.PlayActionType actionType, bool isNotActiveSeq = false, bool isTurnStart = false)
	{
		if (!base.IsRecovery)
		{
			base.NetworkSender.SendEcho(playIndex, actionType, sendKeyActionDataManager, isNotActiveSeq, isTurnStart);
		}
	}

	protected override void SetupNetworkEvent(bool isRecovery)
	{
		base.SetupNetworkEvent(isRecovery);
		BattlePlayer.OnPlayerActive += delegate
		{
			if (turnEndTimeController != null)
			{
				turnEndTimeController.StartCountDown("OnPlayerActive");
			}
		};
		BattlePlayer battlePlayer = BattlePlayer;
		battlePlayer.OnPostTurnEndComplete = (Action)Delegate.Combine(battlePlayer.OnPostTurnEndComplete, (Action)delegate
		{
			if (turnEndTimeController != null)
			{
				turnEndTimeController.EndCountDown("OnTurnEndComplete");
			}
			SendTurnEndAction();
		});
	}

	public override VfxBase ChangePhase(IPhase phase)
	{
		if (phase is NetworkMulliganPhase)
		{
			base.notMulliganEndToJudgeChecker.StartChecker();
			base.disconnectToDispChecker.OnDisp += delegate
			{
				ControlDisconnectOffTouchAndView(flag: true);
			};
			base.disconnectToDispChecker.OnErase += delegate
			{
				ControlDisconnectOffTouchAndView(flag: false);
			};
		}
		return base.ChangePhase(phase);
	}

	private void ServerSendDisconnectCheck()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.ServerSendDisconnectCheck);
	}

	public override void PlayRetire()
	{
		if (JudgeResultReceiveCode == NetworkBattleReceiver.RESULT_CODE.NotFinish && !_isSendSpecialWin)
		{
			base.IsPlayerRetire = true;
			SendRetire();
		}
	}

	protected override void DelayLoadCompleteOpponentResources()
	{
		base.DelayLoadCompleteOpponentResources();
		SetupNetworkBattlePlayersEvent();
	}

	public override VfxBase JudgeBattleResult()
	{
		if (!BattlePlayer.IsSelfTurn && JudgeCurrentFinishStatus() != NetworkBattleReceiver.RESULT_CODE.NotFinish)
		{
			battleFinishToOpponentDisConnectChecker.StartChecker("JudgeBattleResult");
		}
		return base.JudgeBattleResult();
	}

	public override void SendFinishBattleTask()
	{
		base.NetworkSender.SetEmitStopOutsideJudgeResult();
		base.SendFinishBattleTask();
	}

	protected override void AckEmitBattleFinish(Dictionary<string, object> objs)
	{
		battleFinishToOpponentDisConnectChecker.StartChecker("AckEmitBattleFinish");
		base.AckEmitBattleFinish(objs);
	}

	private void SetupNetworkBattlePlayersEvent()
	{
		SetupBattlePlayerEvent(BattlePlayer);
		SetupBattlePlayerEvent(BattleEnemy);
	}

	private void SetupBattlePlayerEvent(BattlePlayerBase battlePlayerBase)
	{
		BattlePlayerBase enemy = battlePlayerBase.Class.OpponentBattlePlayer;
		((ClassSkillApplyInformation)battlePlayerBase.Class.SkillApplyInformation).OnLifeChange += delegate(RegisterActionBase.ActionBaseParameter param, ClassSkillApplyInformation.LifeInfomation lifeInfo)
		{
			if (param != RegisterActionBase.ActionBaseParameter.set || lifeInfo.MaxLife < lifeInfo.BeforeLife)
			{
				if (param == RegisterActionBase.ActionBaseParameter.set && lifeInfo.MaxLife < lifeInfo.BeforeLife)
				{
					RegisterActionManager.Add(new RegisterPlayerParameter(param, lifeInfo.MaxLife, battlePlayerBase.Class.IsPlayer));
				}
				else
				{
					RegisterActionManager.Add(new RegisterPlayerParameter(param, lifeInfo.Life - lifeInfo.BeforeLife, battlePlayerBase.Class.IsPlayer));
				}
				if (battlePlayerBase.Class.SkillApplyInformation.ForceBerserkCount == 0)
				{
					if (!SkillConditionHalfLife.IsHalfLife(lifeInfo.BeforeLife) && SkillConditionHalfLife.IsHalfLife(lifeInfo.Life))
					{
						RegisterRevengeTrigger(battlePlayerBase, 1);
					}
					else if (SkillConditionHalfLife.IsHalfLife(lifeInfo.BeforeLife) && !SkillConditionHalfLife.IsHalfLife(lifeInfo.Life))
					{
						RegisterRevengeTrigger(battlePlayerBase, 0);
					}
				}
			}
		};
		((ClassSkillApplyInformation)battlePlayerBase.Class.SkillApplyInformation).OnPpChange += delegate(RegisterActionBase.ActionBaseParameter param, ClassSkillApplyInformation.PpModifyInformation ppInfo)
		{
			RegisterActionManager.Add(new RegisterPlayerParameter(param, ppInfo.AddPpValue, battlePlayerBase.Class.IsPlayer));
		};
		battlePlayerBase.OnChangeDeckAfterEvent += delegate(int previousCount, SkillProcessor skillProcessor, List<BattleCardBase> summonCards)
		{
			if (previousCount % 2 == 1 && battlePlayerBase.DeckCardList.Count % 2 == 0)
			{
				RegisterResonanceTrigger(battlePlayerBase, 1);
			}
			else if (previousCount % 2 == 0 && battlePlayerBase.DeckCardList.Count % 2 == 1)
			{
				RegisterResonanceTrigger(battlePlayerBase, 0);
			}
		};
		battlePlayerBase.OnDrawCards += delegate(int beforeTurnDrawCardsCount, int turnDrawCardsCount, List<BattleCardBase> drawCards, BattlePlayerBase player, bool isOpen)
		{
			if (player.Class.SkillApplyInformation.ForceAvariceCount == 0 && !SkillConditionAvarice.IsAvarice(beforeTurnDrawCardsCount) && SkillConditionAvarice.IsAvarice(turnDrawCardsCount))
			{
				RegisterAvariceTrigger(player, 1);
			}
		};
		battlePlayerBase.OnTurnEndStart += delegate
		{
			SettingTurnEndRestore(battlePlayerBase);
			RegisterMaxAtkTrigger(battlePlayerBase);
		};
		battlePlayerBase.OnTurnStartBeforeDraw += delegate
		{
			SettingTurnStartRestore(battlePlayerBase);
			RegisterBeforeTurnDamageFromUnit(battlePlayerBase.IsSelfTurn ? enemy : battlePlayerBase);
			return NullVfx.GetInstance();
		};
		BattlePlayerBase battlePlayerBase2 = battlePlayerBase;
		battlePlayerBase2.OnTurnEndSkillAfter = (Func<SkillProcessor, VfxBase>)Delegate.Combine(battlePlayerBase2.OnTurnEndSkillAfter, (Func<SkillProcessor, VfxBase>)delegate
		{
			if (battlePlayerBase.Class.SkillApplyInformation.ForceAvariceCount == 0 && SkillConditionAvarice.IsAvarice(battlePlayerBase.TurnDrawCards.Count))
			{
				RegisterAvariceTrigger(battlePlayerBase, 0);
			}
			if (enemy.Class.SkillApplyInformation.ForceAvariceCount == 0 && SkillConditionAvarice.IsAvarice(enemy.TurnDrawCards.Count))
			{
				RegisterAvariceTrigger(enemy, 0);
			}
			return NullVfx.GetInstance();
		});
	}

	private IEnumerator WaitToSendTurnEnd()
	{
		yield return new WaitForSeconds(0.5f);
		SendTurnEnd();
	}

	private void AckEmitTurnEndAction(Dictionary<string, object> objs)
	{
		RealTimeNetworkAgent realTimeNetworkAgent = this.InstanceNetworkAgent;
		realTimeNetworkAgent.OnAck = (Action<Dictionary<string, object>>)Delegate.Remove(realTimeNetworkAgent.OnAck, new Action<Dictionary<string, object>>(AckEmitTurnEndAction));
		BattleCoroutine.GetInstance().StartCoroutine(WaitToSendTurnEnd());
	}

	private void NotMulliganToJudge()
	{
		if (IsSendSwap)
		{
			OpponentNotMulliganEndVictory();
		}
		else
		{
			MulliganLose();
		}
	}

	private void MulliganLose()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.MulliganLose);
	}

	protected override void DisconnectLose()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.DisconnectLose);
	}

	private void TurnEndLose()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.TurnEndLose);
	}

	private void TurnStartLose()
	{
		FinishBattleSend(NetworkBattleSender.JUDGE_RESULT_STATUS.TurnStartLose);
	}

	public void RegisterRevengeTrigger(BattlePlayerBase player, int isRevenge)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingRevenge(isRevenge);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterResonanceTrigger(BattlePlayerBase player, int isResonance)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingResonance(isResonance);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterAvariceTrigger(BattlePlayerBase player, int isAvarice)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingAvarice(isAvarice);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterReturnCardTrigger(BattlePlayerBase player, int returnCard)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingReturnCard(returnCard);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterUseEpTrigger(BattlePlayerBase player)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		registerEnhanceTrigger.SettingUseEp();
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterMaxAtkTrigger(BattlePlayerBase player)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		int maxAtk = (from c in player.InPlayCards
			where c.IsUnit
			select c.Atk).DefaultIfEmpty().Max();
		registerEnhanceTrigger.SettingMaxAtk(maxAtk);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public void RegisterBeforeTurnDamageFromUnit(BattlePlayerBase player)
	{
		RegisterEnhanceTrigger registerEnhanceTrigger = new RegisterEnhanceTrigger(player);
		int specificTurnCausedDamageValue = player.Class.SkillApplyInformation.GetSpecificTurnCausedDamageValue(player.Class, new TurnPlayerInfo(SkillFilterCreator.ContentKeyword.me.ToStringCustom(), 0));
		registerEnhanceTrigger.SettingBeforeTurnDamageFromUnit(specificTurnCausedDamageValue);
		RegisterActionManager.Add(registerEnhanceTrigger);
	}

	public override void BattleFinishToTurnEndFinal(bool isSelfTurn)
	{
		if (isSelfTurn && !_isEmitTurnEndFinal)
		{
			_isEmitTurnEndFinal = true;
			base.NetworkSender.SendTurnEndFinish();
		}
		base.BattleFinishToTurnEndFinal(isSelfTurn);
	}

	protected override void ControlDisconnectOffTouchAndView(bool flag)
	{
		if (!battleFinishToOpponentDisConnectChecker.IsStart)
		{
			base.ControlDisconnectOffTouchAndView(flag);
		}
	}
}
