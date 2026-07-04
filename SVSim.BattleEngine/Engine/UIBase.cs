using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

public class UIBase : MonoBehaviour
{

	private bool isAssetSetting;

	private bool isOpen;

	private List<string> m_loadBackgroundList = new List<string>();

	protected bool IsShowFooterMenu { get; set; }

	public virtual bool IsGetOutOfScene(Action backupExec)
	{
		return true;
	}

	public virtual bool IsUseCommonBackground()
	{
		return false;
	}

	protected virtual void OnDestroy()
	{
		UnloadBackground();
	}

	public IEnumerator assetSetting()
	{
		if (isAssetSetting)
		{
			yield return null;
		}
		isAssetSetting = true;
		bool flag = UIManager.GetInstance().IsCurrentScene(UIManager.ViewScene.Battle);
		UIManager.GetInstance().AttachAtlas(base.gameObject, !flag);
		SetBackground();
	}

	private void SetBackground()
	{
		ResourcesManager resMgr = Toolbox.ResourcesManager;
		UIBase_Textuer[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIBase_Textuer>(includeInactive: true);
		foreach (UIBase_Textuer bgTex in componentsInChildren)
		{
			string texName = bgTex.TextuerName;
			string assetTypePath = resMgr.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Background);
			m_loadBackgroundList.Add(assetTypePath);
			resMgr.StartCoroutine_LoadAssetGroupAsync(assetTypePath, delegate
			{
				if (bgTex != null)
				{
					bgTex.GetComponent<UITexture>().mainTexture = resMgr.LoadObject<Texture>(resMgr.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Background, isfetch: true));
				}
			});
		}
	}

	private void UnloadBackground()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(m_loadBackgroundList);
		m_loadBackgroundList.Clear();
	}

	public virtual void onFirstStart()
	{
		Open();
	}

	public void Open()
	{
		if (!isOpen)
		{
			isOpen = true;
			if (IsShowFooterMenu)
			{
				UIManager.GetInstance().ShowFooterMenu(isShow: true);
			}
		}
		onOpen();
	}

	protected virtual void onOpen()
	{
		if (base.transform.localScale.x != 1f)
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	public void Close()
	{
		if (isOpen)
		{
			isOpen = false;
			if (IsShowFooterMenu)
			{
				UIManager.GetInstance().ShowFooterMenu(isShow: false);
			}
			onClose();
		}
	}

	protected virtual void onClose()
	{
		// Pre-Phase-5b: re-enable the back key on the ambient GameMgr's InputMgr.
		// Headless has no input manager; the branch was a no-op through the null-guards
		// anyway (GameMgr's InputMgr wasn't seeded), so dropping the ambient reach is safe.
	}

	public virtual void onMove()
	{
	}

	public virtual void assetBundleEnd()
	{
	}
}
