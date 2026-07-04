using UnityEngine;
using Wizard.Battle.View.Vfx;

namespace Wizard.Battle.Touch;

public interface ITouchProcessor
{
	VfxBase Start();

	VfxBase Update(float dt, Camera camera);

	VfxWith<ITouchProcessor> End();

	bool CheckIsEnd();
}
