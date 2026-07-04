using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Phase;

public class OpeningPhase : IPhase
{
	protected readonly BattleManagerBase _battleMgr;

	public OpeningPhase(BattleManagerBase battleMgr)
	{
		_battleMgr = battleMgr;
	}

	public virtual VfxBase Setup()
	{
		return NullVfx.GetInstance();
	}

	public virtual VfxWith<IPhase> Update(float dt)
	{
		return new VfxWith<IPhase>(NullVfx.GetInstance(), _battleMgr.PhaseCreator.CreateMulliganPhase());
	}

	public virtual VfxBase Teardown()
	{
		return NullVfx.GetInstance();
	}

	public virtual void Pause()
	{
	}
}
