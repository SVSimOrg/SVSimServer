using UnityEngine;
using Wizard.Battle.View.Vfx;

public class BattleCamera
{
	public UICamera m_CutInCamera;

	public Camera Camera;

	public Camera _backgroundCamera;

	public BattleCamera()
	{
		Camera = null;
	}

	public Camera Get3DCamera()
	{
		return Camera;
	}

	public void Dispose()
	{
		Camera = null;
	}
}
