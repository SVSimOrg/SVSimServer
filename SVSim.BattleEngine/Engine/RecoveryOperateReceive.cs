public class RecoveryOperateReceive : OperateReceive
{
	private NetworkBattleSender.SELECT_SKILL_OPERATION _latestSelectSkillOperation;

	private bool _latestDataIsEvolve;

	public RecoveryOperateReceive(NetworkBattleManagerBase networkBattleMgr, RegisterActionManager registerCardList, OperateMgr operateMgr, NetworkBattleData networkBattleData)
		: base(networkBattleMgr, registerCardList, operateMgr, networkBattleData)
	{
	}

	protected override InPlayCardReflection CreateNetworkInPlayAction()
	{
		return new RecoveryNetworkInPlayAction(_battleMgr, _operateMgr);
	}

	public override void RecordSelectSkillInRecovery(NetworkBattleReceiver.ReceiveData receiveData)
	{
		_latestSelectSkillOperation = receiveData._selectSkillOperation;
		_latestDataIsEvolve = receiveData._isEvolveTargetSelect;
		ReceivePlayActionsReflectionBase playActionsReflection = GetPlayActionsReflection(receiveData._isEvolveTargetSelect);
		switch (receiveData._selectSkillOperation)
		{
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartSelect:
			playActionsReflection.RecordSelectStart(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattlePlayer, receiveData.idx));
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectCard:
			playActionsReflection.RecordSelectCard(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.GetBattlePlayer(receiveData._isPlayerCard), receiveData._selectedCardIndex), receiveData._isBurialRiteSelect);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteSelect:
			playActionsReflection.RecordCompleteSelect(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.GetBattlePlayer(receiveData._isPlayerCard), receiveData._selectedCardIndex), receiveData._isBurialRiteSelect, receiveData.IsChoiceBraveSelect);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelSelect:
			playActionsReflection.RecordCancelSelect();
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartChoiceSelect:
			playActionsReflection.RecordStartChoiceSelect(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattlePlayer, receiveData.idx));
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteChoiceSelect:
			playActionsReflection.RecordCompleteChoiceSelect(receiveData._selectedChoiceCardIdList);
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CancelChoiceSelect:
			playActionsReflection.RecordCancelChoice();
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.StartFusionSelect:
			playActionsReflection.RecordStartFusion(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattlePlayer, receiveData.idx));
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectFusionIngredient:
			playActionsReflection.RecordSelectFusion(NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.GetBattlePlayer(receiveData._isPlayerCard), receiveData._selectedCardIndex));
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.CompleteFusionSelect:
			playActionsReflection.ClearData();
			break;
		case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectChoiceCard:
			break;
		}
	}

	public override void CheckLatestReplayInfoInRecovery()
	{
		ReceivePlayActionsReflectionBase playActionsReflection = GetPlayActionsReflection(_latestDataIsEvolve);
		if (playActionsReflection.ActingCard != null)
		{
			switch (_latestSelectSkillOperation)
			{
			case NetworkBattleSender.SELECT_SKILL_OPERATION.StartSelect:
			case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectCard:
			case NetworkBattleSender.SELECT_SKILL_OPERATION.StartFusionSelect:
			case NetworkBattleSender.SELECT_SKILL_OPERATION.SelectFusionIngredient:
				playActionsReflection.RecordCancelSelect();
				break;
			case NetworkBattleSender.SELECT_SKILL_OPERATION.StartChoiceSelect:
				playActionsReflection.RecordCancelChoice();
				break;
			}
		}
	}
}
