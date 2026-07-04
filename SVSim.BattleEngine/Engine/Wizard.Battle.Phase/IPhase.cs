using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Phase;

public interface IPhase
{
	VfxBase Setup();

	VfxWith<IPhase> Update(float dt);

	VfxBase Teardown();

	void Pause();
}
