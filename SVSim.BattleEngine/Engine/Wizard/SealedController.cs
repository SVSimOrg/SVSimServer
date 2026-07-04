using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class SealedController : UIBase
{
	private enum eStep
	{
		None,
		ClassSelect,
		Lobby	}

	[SerializeField]
	private UISprite _bgMask;

	[SerializeField]
	private SealedClassSelect _classSelectPrefab;

	[SerializeField]
	private SealedLobby _lobbyPrefab;

	private static bool _isKeepDataAndAsset;

	private static List<string> _unloadResidentAssetList;

	private SealedStepFuncs[] _stepFuncsTable;

	private eStep _nowStep;

	private SealedClassSelect _classSelect;

	private SealedLobby _lobby;

	private SealedData SealedData => Data.ArenaData.SealedData;

	private SealedStepFuncs NowStepFuncs => _stepFuncsTable[(int)_nowStep];

	public static void OnSoftwareReset()
	{
		_isKeepDataAndAsset = false;
		_unloadResidentAssetList = null;
	}

	public override void onFirstStart()
	{
		_stepFuncsTable = new SealedStepFuncs[3]
		{
			new SealedStepFuncs(InitNone, FinalNone),
			new SealedStepFuncs(InitClassSelect, FinalClassSelect),
			new SealedStepFuncs(InitLobby, FinalLobby)
		};
		UIManager.GetInstance().CreateTopBar(base.gameObject, Data.SystemText.Get("Sealed_0001"), UIManager.ViewScene.MyPage, MoneyDraw: false, new UIManager.ChangeViewSceneParam
		{
			MyPageMenuIndex = 3,
			IsCutCardMotion = true,
			OnFinishChangeView = delegate
			{
				MyPageMenu.Instance.GoToChallengeMenu();
			}
		});
		_bgMask.gameObject.SetActive(value: false);
		base.onFirstStart();
	}

	protected override void onOpen()
	{
		StartCoroutine(OpenCoroutine());
	}

	private IEnumerator OpenCoroutine()
	{
		base.onOpen();
		UIManager.GetInstance().UpdateFooterMenuTexture(UIManager.ViewScene.Sealed);
		if (!_isKeepDataAndAsset)
		{
			if (!SealedData.EntryId.HasValue)
			{
				SealedTopTask task = new SealedTopTask();
				StartCoroutine(Toolbox.NetworkManager.Connect(task));
				while (!task.IsResultSuccess)
				{
					yield return null;
				}
			}
			yield return StartCoroutine(LoadResidentAssetCoroutine());
		}
		else
		{
			_isKeepDataAndAsset = false;
		}
		ChangeStep((!SealedData.IsSelectedClass) ? eStep.ClassSelect : eStep.Lobby, delegate
		{
			UIManager.GetInstance().OnReadyViewScene(isFadein: true);
		});
	}

	protected override void onClose()
	{
		NowStepFuncs.FinalFunc();
		if (!_isKeepDataAndAsset)
		{
			RemoveDataAndAsset();
		}
		base.onClose();
	}

	public static void RemoveDataAndAsset()
	{
		_isKeepDataAndAsset = false;
		Data.ArenaData.SealedData.UnregisterAllSealedCard();
		Data.ArenaData.ClearSealedData();
		UnloadResidentAssetCoroutine();
	}

	private static IEnumerator LoadResidentAssetCoroutine()
	{
		UIManager uiMgr = UIManager.GetInstance();
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		List<string> residentAssetList = new List<string>();
		residentAssetList.Add(uiMgr.GetSceneAssetPath(UIAtlasManager.AssetBundleNames.CardFramePhantom));
		residentAssetList.AddRange(_3dCardFrameManager.GetLoadAssetList(_3dCardFrameManager.eFrameKind.Phantom));
		residentAssetList.Add(resourcesManager.GetAssetTypePath("cmn_sealed_class_1", ResourcesManager.AssetLoadPathType.Effect2D));
		yield return uiMgr.StartCoroutine(resourcesManager.LoadAssetGroupAsync(residentAssetList, delegate
		{
			_unloadResidentAssetList = residentAssetList;
			uiMgr.AddResidentAtlas(UIAtlasManager.AssetBundleNames.CardFramePhantom);
			uiMgr.getUIBase_CardManager()._3dCardFrameManager.InitFrameMaterials(_3dCardFrameManager.eFrameKind.Phantom);
		}));
	}

	private static void UnloadResidentAssetCoroutine()
	{
		if (_unloadResidentAssetList != null)
		{
			UIManager instance = UIManager.GetInstance();
			instance.RemoveResidentAtlas(UIAtlasManager.AssetBundleNames.CardFramePhantom);
			instance.getUIBase_CardManager()._3dCardFrameManager.ClearFrameMaterials(_3dCardFrameManager.eFrameKind.Phantom);
			Toolbox.ResourcesManager.RemoveAssetGroup(_unloadResidentAssetList);
			_unloadResidentAssetList = null;
		}
	}

	private void ChangeStep(eStep step, Action initFinishCallback = null)
	{
		StartCoroutine(ChangeStepCoroutine(step, initFinishCallback));
	}

	private IEnumerator ChangeStepCoroutine(eStep step, Action initFinishCallback)
	{
		NowStepFuncs.FinalFunc();
		_nowStep = step;
		yield return StartCoroutine(NowStepFuncs.InitFunc());
		initFinishCallback.Call();
	}

	private IEnumerator InitNone()
	{
		yield return null;
	}

	private void FinalNone()
	{
	}

	private IEnumerator InitClassSelect()
	{
		UIManager.GetInstance().ShowFooterMenu(isShow: false);
		_bgMask.gameObject.SetActive(value: true);
		_classSelect = NGUITools.AddChild(base.gameObject, _classSelectPrefab.gameObject).GetComponent<SealedClassSelect>();
		_classSelect.Init();
		while (!_classSelect.IsReady)
		{
			yield return null;
		}
	}

	private void FinalClassSelect()
	{
		_bgMask.gameObject.SetActive(value: false);
		_classSelect.Final();
		UnityEngine.Object.Destroy(_classSelect.gameObject);
	}

	private IEnumerator InitLobby()
	{
		UIManager.GetInstance().ShowFooterMenu(isShow: true);
		SealedData.ClearGachaCardInfo();
		SealedData.ClearGachaSupplyInfo();
		SealedGetMaintCardListTask task = new SealedGetMaintCardListTask();
		StartCoroutine(Toolbox.NetworkManager.Connect(task));
		while (!task.IsResultSuccess)
		{
			yield return null;
		}
		_lobby = NGUITools.AddChild(base.gameObject, _lobbyPrefab.gameObject).GetComponent<SealedLobby>();
		_lobby.Init();
		while (!_lobby.IsReady)
		{
			yield return null;
		}
		yield return null;
	}

	private void FinalLobby()
	{
		_lobby.Final();
		UnityEngine.Object.Destroy(_lobby.gameObject);
	}

	public static void GoToSealedCardPackOpen()
	{
		_isKeepDataAndAsset = true;
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.SealedCardPackOpen);
	}

	public static void GoToSealedDeckEdit()
	{
		_isKeepDataAndAsset = true;
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.SealedDeckEdit);
	}
}
