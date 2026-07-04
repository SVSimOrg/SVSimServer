public class RecoveryNetworkInPlayAction : InPlayCardReflection
{
	public RecoveryNetworkInPlayAction(BattleManagerBase battleMgr, OperateMgr operateMgr)
		: base(battleMgr, operateMgr)
	{
	}

	protected override void RegisterPairToAttackSelectControl(BattleCardBase attackCard, BattleCardBase targetCard)
	{
	}
}
