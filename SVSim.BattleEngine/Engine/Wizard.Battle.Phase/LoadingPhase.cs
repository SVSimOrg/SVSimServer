using Wizard.Battle.View.Vfx;
// TODO(engine-cleanup-pass2): 2 of 4 methods unrun in baseline
//   Type: Wizard.Battle.Phase.LoadingPhase
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard.Battle.Phase;

public class LoadingPhase : IPhase
{
	protected readonly BattleManagerBase _battleMgr;

	public LoadingPhase(BattleManagerBase battleMgr)
	{
		_battleMgr = battleMgr;
	}

	public virtual VfxBase Setup()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxWith<IPhase> Update(float dt)
	{
		return new VfxWith<IPhase>(NullVfx.GetInstance(), null);
	}

	public virtual VfxBase Teardown()
	{
		return NullVfx.GetInstance();
	}

	public virtual void Pause()
	{
	}
}
