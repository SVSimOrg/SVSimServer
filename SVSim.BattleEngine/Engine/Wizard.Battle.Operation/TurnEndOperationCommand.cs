namespace Wizard.Battle.Operation;

public class TurnEndOperationCommand : IOperationCommand
{
	public TurnEndOperationCommand(bool currentTurnIsPlayer)
	{
	}

	public void Operation(BattleManagerBase battleMgr)
	{
		battleMgr.VfxMgr.RegisterSequentialVfx(battleMgr.OperateMgr.TurnEndOperation(battleMgr.BattlePlayer.IsSelfTurn));
	}

	public override string ToString()
	{
		return "Operation turn_end";
	}
}
