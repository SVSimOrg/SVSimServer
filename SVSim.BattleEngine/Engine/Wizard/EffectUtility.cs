using Cute;
using UnityEngine;

namespace Wizard;

public static class EffectUtility
{
	public static GameObject CreateEffect2D(Effect2dCreateParam param)
	{
		GameObject prefab = Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath(param.EffectName, ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)) as GameObject;
		GameObject gameObject = NGUITools.AddChild(param.Parent, prefab);
		gameObject.transform.localScale /= UIManager.GetInstance().UIManagerRoot.transform.localScale.x;
		if (param.ColorCode.HasValue)
		{
			MotionUtils.ChangeParticleSystemColor(gameObject, ColorCode.Get(param.ColorCode.Value));
		}
		/* Pre-Phase-5b: SetUIParticleShader dropped */
		gameObject.SetActive(param.InitActive);
		return gameObject;
	}
}
