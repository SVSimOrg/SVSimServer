using System;
using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class RecoveryOperationCollection : WatchOperationCollection
{
	public RecoveryOperationCollection(NetworkBattleManagerBase networkBattleMgr, OperateMgr operateMgr, NetworkBattleReceiver.ReceiveData receivedData, NetworkBattleData networkBattleData, bool isPlayer)
		: base(networkBattleMgr, operateMgr, receivedData, networkBattleData, isPlayer)
	{
	}

	public override void SecondMulliganOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan, Func<VfxBase> OnEndMulligan)
	{
		OperateMulligan(OnReceiveOpponentMulligan, OnReceivePlayerMulligan);
		_networkBattleMgr.BattlePlayer.IsTurnStartEffectNotFinished = true;
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(OnEndMulligan.GetAllFuncVfxResults());
	}

	public override void PlayHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null, bool isChoice = false)
	{
		// Route ALL recovery hand-plays through PlayAction (the type:31 PLAY_HAND_SELECT path).
		// PlayAction resolves targets from the receiver's target data and calls PlayActionMove,
		// which bypasses PlayMove's two-phase user-select guard (the guard that aborts on targeted
		// spells with SendEcho+return, waiting for a follow-up type:31 frame that never comes in
		// recovery/shadow mode). PlayAction is the path RecoveryOperationCollection already uses
		// for type:31; unifying type:30 here makes all spell plays resolve headless.
		PlaySkillSelectHandCardOperation(networkPlayCardAction, choiceIdList);
	}

	public override void PlaySkillSelectHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIds = null)
	{
		List<NetworkBattleReceiver.TargetData> targetDataList = (_isPlayer ? _receivedData.PlayerTargetDataList : _receivedData.OpponentTargetDataList);
		SetupNetworkPlayCardAction(networkPlayCardAction, targetDataList);
		_networkBattleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			networkPlayCardAction.PlayAction(_isPlayer, choiceIds);
		}));
	}

	public override void BattleFinishOperation()
	{
		((NetworkStandardBattleMgr)_networkBattleMgr)._recoveryController.RecoveryDataHandlerInstance.OnCompleteRecovery += base.BattleFinishOperation;
	}

	public override void TouchOperation()
	{
	}

	public override void SelectObjectOperation()
	{
	}

	public override void TurnEndReady()
	{
	}

	public override void SlideObject()
	{
	}

	protected override void PlayCancelSlide()
	{
	}

	protected override void CallCompleteEvent(ReceivePlayActionsReflectionBase networkAction)
	{
	}

	protected override void CheckStateAndCancel(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction, bool isPlayer)
	{
	}

	protected override void RegisterSequentialVfx(VfxBase operationVfx)
	{
	}

	public override void SendEcho()
	{
		if (_networkBattleMgr._specialWinVfx == null)
		{
			_networkBattleMgr.ClearRegisterCardList();
		}
	}
}
