using System;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class TurnEndTimeController
{
	private enum TIMEOUT_TURNEND_SCENE
	{
		NONE = 0,
		TIMEOUT_START = 10,
		TIMEOUT_START_WAIT = 20,
		TURNEND_OPERATION = 30,
		TURNEND_OPERATION_WAIT = 40,
		EFFECT_STOP = 50,
		EFFECT_STOP_WAIT = 60,
		NOT_TOUCH_RELEASE = 70
	}

	private NetworkBattleManagerBase _networkBattleManager;

	private BattlePlayer _battlePlayer;

	private ITurnEndButtonUI _turnEndUI;

	private long _startTicks;

	private bool _isAlertEffect;

	private float _extendTime;

	private GameObject _alertEffect;

	private bool _isMovingTurnEndTimer;

	private TIMEOUT_TURNEND_SCENE _timeoutTurnEndScene;

	private VfxBase _canNotTouchVfx;

	private string _opponentTurnTurnEndLogMessage = "";

	private bool _isOpponentTurnTimerUpdateLog;

	private int _logAddNum;

	private bool _isTurnStartTimeCheckLog;

	protected virtual bool IsTurnTimeDecrement { get; set; }

	public bool IsNowTurnTimeDecrement { get; private set; }

	public bool IsNextTurnTimeDecrement { get; private set; }

	public TurnEndTimeController(BattleManagerBase battleMgr, BattlePlayer battlePlayer, ITurnEndButtonUI turnEnd)
	{
		LocalLog.AccumulateLastTraceLog("TurnEndTimeController");
		_networkBattleManager = battleMgr as NetworkBattleManagerBase;
		_battlePlayer = battlePlayer;
		_networkBattleManager.OperateMgr.OnTurnEnd_ButtonPush += delegate
		{
			IsTurnTimeDecrement = false;
			SetDecrementFlag(isDecrement: false);
			LocalLog.AccumulateLastTraceLog("OnTurnEnd_ButtonPush");
		};
		_turnEndUI = turnEnd;
		SetDecrementFlag(isDecrement: true);
	}

	public void StartCountDown(string log)
	{
		LocalLog.AccumulateLastTraceLog("StartCountDown " + log);
		DateTime absoluteTime = TimeUtil.GetAbsoluteTime();
		_startTicks = absoluteTime.Ticks;
		_extendTime = 0f;
		log = log + absoluteTime.Hour + ":" + absoluteTime.Minute + ":" + absoluteTime.Second + ":" + absoluteTime.Millisecond.ToString("000") + ":";
		AddTurnEndTimerLog(" StartCountDown " + log);
		_isTurnStartTimeCheckLog = false;
		_turnEndUI.SettingTimer(GetMaxSecond(), isRed: false);
		EndCountDown("startAfter");
		_isMovingTurnEndTimer = true;
	}

	public void EndCountDown(string log)
	{
		LocalLog.AccumulateLastTraceLog("EndCountDown " + log);
		AddTurnEndTimerLog(" EndCountDown " + log);
		_isMovingTurnEndTimer = false;
		if (_isAlertEffect)
		{
			_alertEffect = null;
			_battlePlayer.BattleMgr.GameMgr.GetEffectMgr().Stop(EffectMgr.EffectType.CMN_UI_TURN_5);
			_isAlertEffect = false;
		}
		if (_turnEndUI as UnityEngine.Object != null)
		{
			_turnEndUI.SettingTimer(0f, isRed: false);
		}
	}

	public void AddTurnEndTimerLog(string text)
	{
		_logAddNum++;
		if (_logAddNum <= 15)
		{
			DateTime dateTime = DateTime.Now.ToUniversalTime();
			string text2 = dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + ":" + dateTime.Millisecond.ToString("000");
			_opponentTurnTurnEndLogMessage = _opponentTurnTurnEndLogMessage + text2 + text + " \n";
		}
	}

	public void BattleEndToTraceLog()
	{
		if (_isOpponentTurnTimerUpdateLog)
		{
			LocalLog.AccumulateTraceLog("665987UpdateTimerCountDownBatlteEnd " + _opponentTurnTurnEndLogMessage);
		}
	}

	public bool IsCountdownRunning()
	{
		return _isMovingTurnEndTimer;
	}

	public void SetDecrementFlag(bool isDecrement)
	{
		IsNextTurnTimeDecrement = isDecrement;
		LocalLog.AccumulateLastTraceLog("SetDecrementFlag " + isDecrement);
	}

	public void SetExtendTime(float leftTime)
	{
		_extendTime = leftTime;
	}

	public void UpdateTimerCountDown()
	{
		if (!_isMovingTurnEndTimer || _turnEndUI == null || !_turnEndUI.GameObject.activeSelf || _timeoutTurnEndScene >= TIMEOUT_TURNEND_SCENE.TIMEOUT_START)
		{
			return;
		}
		if (!_battlePlayer.BattleMgr.GameMgr.IsWatchBattle && (_battlePlayer.IsTurnStartEffectNotFinished || !_battlePlayer.IsSelfTurn))
		{
			if (!_isOpponentTurnTimerUpdateLog)
			{
				LocalLog.AccumulateTraceLog("665987ErrorUpdateTimerCountDown ");
				_isOpponentTurnTimerUpdateLog = true;
			}
			return;
		}
		DateTime absoluteTime = TimeUtil.GetAbsoluteTime();
		long ticks = absoluteTime.Ticks - _startTicks;
		TimeSpan timeSpan = new TimeSpan(ticks);
		float num = GetMaxSecond() - (float)timeSpan.TotalMilliseconds / 1000f + _extendTime;
		if (!_isTurnStartTimeCheckLog)
		{
			AddTurnEndTimerLog(" UpdateTimerCountDown differenceSecond " + num + " IsTurnTimeDecrement" + IsTurnTimeDecrement + "_startTicks" + _startTicks + "_nowTicks" + absoluteTime.Ticks + "_extendTime" + _extendTime);
			_isTurnStartTimeCheckLog = true;
		}
		if (num <= 0f)
		{
			if (CompulsionTurnEnd())
			{
				return;
			}
		}
		else if (num < 20f)
		{
			UpdateTimerPosition(num, isShowingAlert: true);
			if (!_isAlertEffect)
			{
				_isAlertEffect = true;
				EmitHandUtility.SendTurnEndReady(_networkBattleManager, IsTurnTimeDecrement);
				if (!_networkBattleManager.IsRecovery)
				{
					_alertEffect = _battlePlayer.BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_UI_TURN_5, _turnEndUI.GetBtnPosition())
						.GetGameObjIns();
					_alertEffect.transform.localScale = ((!_battlePlayer.BattleMgr.GameMgr.IsWatchBattle) ? (Vector3.one * 20f) : (Vector3.one * 19f));
				}
			}
			else if (!_networkBattleManager.IsRecovery)
			{
				if (_alertEffect == null)
				{
					_alertEffect = _battlePlayer.BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_UI_TURN_5, _turnEndUI.GetBtnPosition())
						.GetGameObjIns();
				}
				else
				{
					float x = _alertEffect.transform.localScale.x;
					x += (GetEffectScaleByTime(num) - x) * 0.5f;
					_alertEffect.transform.localScale = Vector3.one * x;
				}
			}
		}
		else
		{
			UpdateTimerPosition(num, isShowingAlert: false);
		}
		if (_battlePlayer.IsSelfTurn && IsNextTurnTimeDecrement && (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2) || Input.touchCount >= 1))
		{
			LocalLog.AccumulateLastTraceLog("IsNextTurnTimeDecrement Off");
			SetDecrementFlag(isDecrement: false);
		}
	}

	protected virtual bool CompulsionTurnEnd()
	{
		if (_timeoutTurnEndScene >= TIMEOUT_TURNEND_SCENE.TIMEOUT_START)
		{
			return false;
		}
		if (_networkBattleManager.TouchControl._touchProcessor != null && _networkBattleManager.TouchControl.IsProcessorStart && _networkBattleManager.TouchControl.IsProcessorUpdate && _networkBattleManager.TouchControl.IsProcessorEnd && !_networkBattleManager.VfxMgr.IsEnd)
		{
			_networkBattleManager.TouchControl.IsForceEnd = true;
			return false;
		}
		_battlePlayer.IsTimeOverTurnEndProcessing = true;
		if (_networkBattleManager.JudgeCurrentFinishStatus() == NetworkBattleReceiver.RESULT_CODE.NotFinish)
		{
			VfxBase vfx = NullVfx.GetInstance();
			_battlePlayer.BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_UI_TURN_6, _turnEndUI.GetBtnPosition());
			if (!(_networkBattleManager.OperateMgr is RecoveryOperateMgr))
			{
				vfx = _networkBattleManager.OperateMgr.PlayerTurnEnd(isAuto: true);
			}
			_networkBattleManager.VfxMgr.RegisterSequentialVfx(vfx);
			_networkBattleManager.TouchControl.IsForceEnd = false;
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.TIMEOUT_START;
			EndCountDown("TimeOut");
			return true;
		}
		return false;
	}

	protected virtual void UpdateTimerPosition(float timeDifference, bool isShowingAlert)
	{
		_turnEndUI.SettingTimer(timeDifference, isShowingAlert);
	}

	public void UpdateTimeoutTurnEnd()
	{
		switch (_timeoutTurnEndScene)
		{
		case TIMEOUT_TURNEND_SCENE.TIMEOUT_START:
			_canNotTouchVfx = NullVfx.GetInstance();
			_networkBattleManager.VfxMgr.RegisterImmediateVfx(_canNotTouchVfx);
			IsTurnTimeDecrement = IsNextTurnTimeDecrement;
			AddTurnEndTimerLog("ExecTurnEnd IsTurnTimeDecrement" + IsTurnTimeDecrement);
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.TIMEOUT_START_WAIT;
			_turnEndUI.ChangeButtonView(isMyTurn: false);
			_turnEndUI.HideBtn();
			_turnEndUI._isChangeViewLock = true;
			break;
		case TIMEOUT_TURNEND_SCENE.TIMEOUT_START_WAIT:
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.TURNEND_OPERATION;
			break;
		case TIMEOUT_TURNEND_SCENE.TURNEND_OPERATION:
			if ((!_networkBattleManager.IsRecovery && !_networkBattleManager.IsCardPlayToTurnEndTimeoutStop && !_battlePlayer.PlayerBattleView.IsSelecting) || _networkBattleManager.VfxMgr.IsEnd)
			{
				_networkBattleManager.VfxMgr.RegisterSequentialVfx(_networkBattleManager.OperateMgr.TurnEndOperation(isPlayer: true));
				_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.TURNEND_OPERATION_WAIT;
				_turnEndUI._isChangeViewLock = false;
			}
			break;
		case TIMEOUT_TURNEND_SCENE.TURNEND_OPERATION_WAIT:
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.EFFECT_STOP;
			break;
		case TIMEOUT_TURNEND_SCENE.EFFECT_STOP:
			if (_networkBattleManager.VfxMgr.IsEnd)
			{
				_battlePlayer.PlayerBattleView.ForceStopShowSelect();
				EffectMgr effectMgr = _battlePlayer.BattleMgr.GameMgr.GetEffectMgr();
				effectMgr.Stop(EffectMgr.EffectType.CMN_CARD_SET_1);
				effectMgr.Stop(EffectMgr.EffectType.CMN_CARD_ACCELERATE_1);
				effectMgr.Stop(EffectMgr.EffectType.CMN_CARD_CRYSTALLIZE_1);
				effectMgr.Stop(EffectMgr.EffectType.CMN_CARD_SET_2);
				effectMgr.Stop(EffectMgr.EffectType.CMN_CARD_SET_3);
				_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.EFFECT_STOP_WAIT;
			}
			break;
		case TIMEOUT_TURNEND_SCENE.EFFECT_STOP_WAIT:
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.NOT_TOUCH_RELEASE;
			break;
		case TIMEOUT_TURNEND_SCENE.NOT_TOUCH_RELEASE:
		{
			IPlayerView playerBattleView = _battlePlayer.PlayerBattleView;
			playerBattleView.AllClear(popUpClose: false, isRemoveSideLog: false);
			playerBattleView.HandView.FocusRearrangeHandHand();
			_timeoutTurnEndScene = TIMEOUT_TURNEND_SCENE.NONE;
			break;
		}
		}
	}

	public float GetMaxSecond()
	{
		float result = 90f;
		IsNowTurnTimeDecrement = false;
		if (IsTurnTimeDecrement)
		{
			result = 20f;
			IsNowTurnTimeDecrement = true;
		}
		return result;
	}

	private float GetEffectScaleByTime(float time)
	{
		float num = 2f;
		float num2 = ((!(time > num)) ? 0f : Mathf.Min(1.3333334f, (time - num) / (20f - num) * 1.3333334f));
		return num2 * 2f + 1f;
	}
}
