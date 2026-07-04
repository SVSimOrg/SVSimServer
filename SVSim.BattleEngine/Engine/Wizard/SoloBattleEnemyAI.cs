using Wizard.Battle.View.Vfx;

namespace Wizard;

public class SoloBattleEnemyAI : EnemyAI
{
	public override bool IsStackAction => false;

	public override bool IsConnectNetwork => false;

	public SoloBattleEnemyAI(BattleManagerBase mgr) : base(mgr)
	{
		base.IsRankMatchAI = false;
	}

	public override void Retire()
	{
	}

	protected override void OnBeforeTurnEnd()
	{
		emoteQuery.OnOperation();
		VfxBase vfxBase = null;
		vfxBase = ((base.TurnCount != 1) ? GetEmote(AIEmoteCmdType.ON_ALLY_TURN_END) : GetEmote(AIEmoteCmdType.ON_FIRST_TURN));
		if (vfxBase != null)
		{
			battleMgr.VfxMgr.RegisterSequentialVfx(vfxBase);
		}
		base.IsThisTurnEmotePlayed = false;
	}

	protected override void OnFinishOprAttack()
	{
		emoteMng.OnOperationRequest();
	}

	protected override void OnFinishOprTargetSelect()
	{
		emoteMng.OnOperationRequest();
	}

	public override void Disconnect()
	{
	}

	public override void Reconnect()
	{
	}

	public override void CleanupStackedAction()
	{
	}

	protected override void CheckIsStackAction()
	{
	}

	protected override void SetupThinkingInterval()
	{
	}
}
