using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class ShopDrumrollScrollItem : MonoBehaviour
{
	[SerializeField]
	private GameObject _objNewMark;

	[SerializeField]
	private GameObject _objPrerelease;

	private void Awake()
	{
		_ = _objNewMark != null;
	}

	public void SetActiveNewMark(bool isActive)
	{
		if (_objNewMark == null)
		{
			return;
		}
		if (_objNewMark.activeSelf != isActive)
		{
			_objNewMark.SetActive(isActive);
		}
		if (isActive)
		{
			GameObject _objNewEffect = Object.Instantiate(Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath("cmn_shop_icon_1", ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)) as GameObject);
			_objNewEffect.transform.SetParent(_objNewMark.transform);
			_objNewEffect.transform.localPosition = Vector3.zero;
			// Pre-Phase-5b: SetUIParticleShader + ChangeMaskShader dropped; headless has no EffectMgr.
			_objNewEffect.SetActive(value: true);
		}
	}

	public void SetActivePrereleaseMark(bool isActive)
	{
		if (_objPrerelease == null)
		{
			return;
		}
		if (_objPrerelease.activeSelf != isActive)
		{
			_objPrerelease.SetActive(isActive);
		}
		if (isActive)
		{
			GameObject effectObj = Object.Instantiate(Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath("cmn_shop_icon_1", ResourcesManager.AssetLoadPathType.Effect2D, isfetch: true)) as GameObject);
			effectObj.transform.SetParent(_objPrerelease.transform);
			effectObj.transform.localPosition = Vector3.zero;
			// Pre-Phase-5b: SetUIParticleShader + ChangeMaskShader dropped; headless has no EffectMgr.
			effectObj.SetActive(value: true);
		}
	}
}
