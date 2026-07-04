using System;
using System.Collections.Generic;
using Wizard.Battle.View.Vfx;

public class NullOperationCollection : NetworkOperationCollectionBase
{
	public NullOperationCollection()
		: base(null, null, null, null, isPlayer: false)
	{
	}

	public override void RetryOperation()
	{
	}

	public override void DealOperation()
	{
	}

	public override void SwapOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan)
	{
	}

	public override void SecondMulliganOperation(Func<List<int>, VfxBase> OnReceiveOpponentMulligan, Func<List<int>, VfxBase> OnReceivePlayerMulligan, Func<VfxBase> OnEndMulligan)
	{
	}

	public override void TurnStartOperation(NetworkBattleDefine.NetworkBattleURI lastReceivedUri, int lastReceivedTime)
	{
	}

	public override void TurnEndOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
	}

	public override void TurnEndFinalOperation()
	{
	}

	public override void TurnEndWithSkillActivationOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
	}

	public override void JudgeOperation()
	{
	}

	public override void PlayHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null, bool isChoice = false)
	{
	}

	public override void PlaySkillSelectHandCardOperation(PlayHandCardReflection networkPlayCardAction, List<int> choiceIdList = null)
	{
	}

	public override void InPlayActionOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
	{
	}

	public override void RetireOperation()
	{
	}

	public override void ChatStampOperation()
	{
	}

	public override void DataInconsistencyBattleEndOperation()
	{
	}

	public override void TouchOperation()
	{
	}

	public override void SelectSkillOperation(PlayHandCardReflection networkPlayCardAction, InPlayCardReflection networkInPlayAction)
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

	public override void BattleFinishOperation()
	{
	}

	public override void MaintenanceOperation()
	{
	}

	public override void JudgeResultOperation()
	{
	}
}
