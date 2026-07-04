using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

public class ClassSkinPurchasePage : BaseShopPurchasePage
{
	private enum eBuyType
	{
		single,
		multi,
		rewardOnly
	}

	[SerializeField]
	private UITable _uiTableProducts;

	[SerializeField]
	private GameObject _PrefabDialogSelectBuyMeans;

	[SerializeField]
	private PurchaseConfirm _PrefabDialogPurchaseConfirm;

	// ClassSkinDetailWindow removed (DEAD-COLD engine cleanup Task 13)
	// private ClassSkinDetailWindow _PrefabDialogSkinDetail;

	[SerializeField]
	private ShopDrumrollScrollManager _drumrollManager;

	private SkinSeriesPurchaseInfo _selectSeriesInfo;

	private string _purchaseProductName;

	private SkinProductInfo _purchaseProductInfo;

	private ClassSkinPlate _selectPlate;

	private GameObject _MultiPlateObj;

	private DialogBase _tempCloseDialog;

	private DialogBase _dialogPurchaseConfirm;

	private DialogBase _dialogSelectBuyMeans;

	private DialogBase _dialogCrystalShortage;

	private bool _isBuyConnect;

	private List<GameObject> _productPlateObjList = new List<GameObject>();

	private readonly List<string> _loadedVoiceList = new List<string>();

	public override void onFirstStart()
	{
		CreateTopBar(Data.SystemText.Get("Shop_0104"), delegate
		{
			MyPageMenu.Instance.GoToShopSupply();
		});
		base.onFirstStart();
	}

	protected override void SetupScrollView()
	{
		_productPlateOriginal.gameObject.SetActive(value: false);
	}

	protected override void ResetProductListScroll(int productCount)
	{
		if (productCount > _productPlateObjList.Count)
		{
			int num = productCount - _productPlateObjList.Count;
			for (int i = 0; i < num; i++)
			{
				GameObject item = NGUITools.AddChild(_uiTableProducts.gameObject, _productPlateOriginal);
				_productPlateObjList.Add(item);
			}
		}
		for (int j = 0; j < _productPlateObjList.Count; j++)
		{
			UpdateScrollItem(_productPlateObjList[j], j);
		}
		_uiTableProducts.Reposition();
		_scrollView.ResetPosition();
	}

	protected override void onOpen()
	{
		base.onOpen();
		_loadedResourceList = new List<string>();
		StartGetClassSkinInfo(OnClassSkinInfoRequestFinished);
	}

	protected override void onClose()
	{
		if (_loadedVoiceList.Count > 0)
		{

			Toolbox.ResourcesManager.RemoveAssetGroup(_loadedVoiceList);
			_loadedVoiceList.Clear();
		}
		base.onClose();
	}

	private void StartGetClassSkinInfo(Action<NetworkTask.ResultCode> callbackOnSuccess)
	{
		SkinPurchaseInfoTask skinPurchaseInfoTask = new SkinPurchaseInfoTask();
		skinPurchaseInfoTask.SetParameter();
		StartCoroutine(Toolbox.NetworkManager.Connect(skinPurchaseInfoTask, callbackOnSuccess));
	}

	private void StartBuySkin(eBuyType buyType, ShopCommonUtility.SalesType costType, int id, long? itemId)
	{
		if (!_isBuyConnect)
		{
			_isBuyConnect = true;
			UIManager.GetInstance().createInSceneCenterLoading();
			switch (buyType)
			{
			case eBuyType.single:
			{
				SkinBuySingleTask skinBuySingleTask = new SkinBuySingleTask();
				skinBuySingleTask.SetParameter(id, costType, itemId);
				StartCoroutine(Toolbox.NetworkManager.Connect(skinBuySingleTask, onSuccessPurchaseSingle, _OnFailurePurchase, _OnResultCodeError));
				break;
			}
			case eBuyType.multi:
			{
				SkinBuyMultiTask skinBuyMultiTask = new SkinBuyMultiTask();
				skinBuyMultiTask.SetParameter(id, costType, itemId);
				StartCoroutine(Toolbox.NetworkManager.Connect(skinBuyMultiTask, onSuccessPurchaseMulti, _OnFailurePurchase, _OnResultCodeError));
				break;
			}
			case eBuyType.rewardOnly:
			{
				SkinBuyMultiRewardTask skinBuyMultiRewardTask = new SkinBuyMultiRewardTask();
				skinBuyMultiRewardTask.SetParameter(id);
				StartCoroutine(Toolbox.NetworkManager.Connect(skinBuyMultiRewardTask, onSuccessPurchaseMulti, _OnFailurePurchase, _OnResultCodeError));
				break;
			}
			}
		}
	}

	public static void SetFirstDisplaySeries(int seriesId)
	{
		PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_SKIN_SERIES_ID, seriesId);
	}

	private int GetViewSeriesId()
	{
		int series_id = Data.SkinPurchaseInfo.seriesList[0].series_id;
		int value = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LATEST_SKIN_SERIES_ID);
		if (series_id != value)
		{
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LATEST_SKIN_SERIES_ID, series_id);
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_PURCHASE_SKIN_SERIES_ID, series_id);
		}
		int value2 = PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_SKIN_SERIES_ID);
		if (value2 > -1)
		{
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.SCENE_TRANSITION_VIEW_SKIN_SERIES_ID, -1);
			return value2;
		}
		return PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_PURCHASE_SKIN_SERIES_ID);
	}

	private void OnClassSkinInfoRequestFinished(NetworkTask.ResultCode error)
	{
		List<int> seriesIdList = GetSeriesIdList();
		Dictionary<int, BaseSeriesData> seriesDataDictionary = GetSeriesDataDictionary();
		StartCoroutine(loadSeriesImages(ResourcesManager.AssetLoadPathType.ShopClassSkin, seriesIdList, seriesDataDictionary, delegate
		{
			if (_drumrollSeriesImageList.Count <= 0)
			{
				UIManager.GetInstance().OnReadyViewScene(isFadein: true);
			}
			else
			{
				int viewSeriesId = GetViewSeriesId();
				int seriesIndex = Data.SkinPurchaseInfo.seriesList.FindIndex((SkinSeriesPurchaseInfo data) => data.series_id == viewSeriesId);
				if (seriesIndex < 0)
				{
					seriesIndex = 0;
				}
				List<SkinSeriesPurchaseInfo> seriesList = Data.SkinPurchaseInfo.seriesList;
				List<ShopDrumrollScrollManager.DrumrollItem> itemList = _drumrollSeriesImageList.Select((Texture tex, int index) => new ShopDrumrollScrollManager.DrumrollItem(tex, seriesList[index].IsNew)).ToList();
				StartCoroutine(_drumrollManager.CreateDrumrollScroll_Coroutine(itemList, seriesIndex, onSelectSeries, delegate
				{
					onSelectSeries(seriesIndex, delegate
					{
						UIManager.GetInstance().OnReadyViewScene(isFadein: true);
					});
				}));
			}
		}));
	}

	private List<int> GetSeriesIdList()
	{
		return Data.SkinPurchaseInfo.seriesList.ConvertAll((SkinSeriesPurchaseInfo info) => info.series_id);
	}

	private Dictionary<int, BaseSeriesData> GetSeriesDataDictionary()
	{
		Dictionary<int, BaseSeriesData> dictionary = new Dictionary<int, BaseSeriesData>();
		foreach (KeyValuePair<int, LeaderSkinSeries> item in Data.Master.LeaderSkinSeriesIdDic)
		{
			dictionary.Add(item.Key, item.Value);
		}
		return dictionary;
	}

	private void onSelectSeries(int seriesIndex)
	{
		onSelectSeries(seriesIndex, null);
	}

	private void onSelectSeries(int seriesIndex, Action onFinish)
	{
		SkinSeriesPurchaseInfo seriesInfo = Data.SkinPurchaseInfo.seriesList[seriesIndex];
		_selectSeriesInfo = seriesInfo;
		_titleLogoTexture.mainTexture = _seriesTitleImageList[seriesIndex];
		_labelSeriesDescription.text = seriesInfo.description;
		int num = _cacheSeriesIdList.IndexOf(seriesInfo.series_id);
		if (num != -1)
		{
			_cacheRefCountList[num]++;
			ResetProductListScroll(seriesInfo.GetProductCount());
			onFinish.Call();
			return;
		}
		if (_cacheSeriesIdList.Count >= 4)
		{
			int num2 = _cacheRefCountList[0];
			int cacheIndex = 0;
			for (int i = 1; i < _cacheRefCountList.Count; i++)
			{
				if (num2 > _cacheRefCountList[i])
				{
					num2 = _cacheRefCountList[i];
					cacheIndex = i;
				}
			}
			DeleteCacheSeriesByCashIndex(cacheIndex);
		}
		StartCoroutine(loadClassSkins(delegate
		{
			if (_selectSeriesInfo == seriesInfo)
			{
				ResetProductListScroll(seriesInfo.GetProductCount());
			}
			_cacheSeriesIdList.Add(seriesInfo.series_id);
			_cacheRefCountList.Add(1);
			onFinish.Call();
		}));
	}

	private void DeleteCacheSeriesByCashIndex(int cacheIndex)
	{
		List<string> listResource = new List<string>();
		SkinSeriesPurchaseInfo skinSeriesPurchaseInfo = Data.SkinPurchaseInfo.seriesList.Find((SkinSeriesPurchaseInfo m) => m.series_id == _cacheSeriesIdList[cacheIndex]);
		if (skinSeriesPurchaseInfo != null)
		{
			if (skinSeriesPurchaseInfo.SetSalesStatus != SkinSeriesPurchaseInfo.eSetSalesStatus.None)
			{
				listResource.Add(Toolbox.ResourcesManager.GetAssetTypePath(skinSeriesPurchaseInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ShopClassSkin));
			}
			skinSeriesPurchaseInfo.productList.ForEach(delegate(SkinProductInfo s)
			{
				listResource.Add(Toolbox.ResourcesManager.GetAssetTypePath(s.saleInfo.path, ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail));
			});
		}
		Toolbox.ResourcesManager.RemoveAssetGroup(listResource);
		_cacheSeriesIdList.RemoveAt(cacheIndex);
		_cacheRefCountList.RemoveAt(cacheIndex);
		for (int num = 0; num < listResource.Count; num++)
		{
			_cacheResourceList.Remove(listResource[num]);
		}
	}

	private IEnumerator loadClassSkins(Action callBack = null)
	{
		UIManager.GetInstance().createInSceneCenterLoading();
		List<string> listResource = new List<string>();
		if (_selectSeriesInfo.SetSalesStatus != SkinSeriesPurchaseInfo.eSetSalesStatus.None)
		{
			listResource.Add(Toolbox.ResourcesManager.GetAssetTypePath(_selectSeriesInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ShopClassSkin));
		}
		foreach (SkinProductInfo product in _selectSeriesInfo.productList)
		{
			if (product.IsEnableBuyTicket)
			{
				listResource.Add(ShopCommonUtility.GetTicketIconPath(product.saleInfo.costTicketItemId.Value.ToString(), isFetch: false));
				listResource.Add(ShopCommonUtility.GetTicketIconRightDownPath(product.saleInfo.costTicketItemId.Value.ToString(), isFetch: false));
			}
		}
		_selectSeriesInfo.productList.ForEach(delegate(SkinProductInfo s)
		{
			listResource.Add(Toolbox.ResourcesManager.GetAssetTypePath(s.saleInfo.path, ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail));
		});
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(listResource, null));
		UIManager.GetInstance().closeInSceneCenterLoading();
		for (int num = 0; num < listResource.Count; num++)
		{
			_cacheResourceList.Add(listResource[num]);
		}
		callBack.Call();
	}

	private void UpdateScrollItem(GameObject go, int index)
	{
		if (_selectSeriesInfo == null)
		{
			return;
		}
		if (index >= _selectSeriesInfo.GetProductCount() || index < 0)
		{
			go.SetActive(value: false);
			return;
		}
		go.SetActive(value: true);
		ClassSkinPlate plate = go.GetComponent<ClassSkinPlate>();
		EventDelegate eventDelegate = new EventDelegate(this, "onPushBuyButton");
		eventDelegate.parameters[0].value = plate;
		plate.SetBuyButtonToGrey(isGrey: false);
		if (_selectSeriesInfo.SetSalesStatus != SkinSeriesPurchaseInfo.eSetSalesStatus.None)
		{
			if (index == 0)
			{
				_MultiPlateObj = go;
				eventDelegate.parameters[1].value = true;
				plate.SetMultiData(_selectSeriesInfo, eventDelegate, delegate
				{
					_onTapClassSkinImage(plate, isMulti: true);
				});
				if (_selectSeriesInfo.SetSalesStatus == SkinSeriesPurchaseInfo.eSetSalesStatus.Disable)
				{
					plate.SetBuyButtonToGrey(isGrey: true);
				}
			}
			else
			{
				if (_MultiPlateObj == go)
				{
					_MultiPlateObj = null;
				}
				eventDelegate.parameters[1].value = false;
				plate.SetData(_selectSeriesInfo.productList[index - 1], eventDelegate, delegate
				{
					_onTapClassSkinImage(plate, isMulti: false);
				});
			}
		}
		else
		{
			_MultiPlateObj = null;
			eventDelegate.parameters[1].value = false;
			plate.SetData(_selectSeriesInfo.productList[index], eventDelegate, delegate
			{
				_onTapClassSkinImage(plate, isMulti: false);
			});
		}
	}

	private void _onTapClassSkinImage(ClassSkinPlate plate, bool isMulti)
	{
		// ClassSkinDetailWindow removed (DEAD-COLD engine cleanup Task 13)
	}

	private void onPushBuyButton(ClassSkinPlate plate, bool isMulti)
	{

		_selectPlate = plate;
		if (isMulti)
		{
			createClassSkinSelectBuyMeansMultiDialog(plate.SeriesInfo);
		}
		else
		{
			createClassSkinSelectBuyMeansDialog(plate.ProductInfo);
		}
	}

	private void createClassSkinSelectBuyMeansMultiDialog(SkinSeriesPurchaseInfo sInfo)
	{
		if (_dialogSelectBuyMeans != null)
		{
			return;
		}
		if (sInfo.is_completed)
		{
			_ = sInfo._rewardStatus;
			_ = 2;
		}
		_dialogSelectBuyMeans = _createBaseDialogForSelectBuyMeans(sInfo.saleInfo);
		ClassSkinSelectBuyMeansDialog component = UnityEngine.Object.Instantiate(_PrefabDialogSelectBuyMeans).GetComponent<ClassSkinSelectBuyMeansDialog>();
		_dialogSelectBuyMeans.SetObj(component.gameObject);
		Action onPushBuyCrystalBtnCallBack = null;
		Action onPushBuyRupyBtnCallBack = null;
		Action onPushBuyTicketButtonCallBack = null;
		if (sInfo.saleInfo.isFree)
		{
			_dialogSelectBuyMeans.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			_dialogSelectBuyMeans.SetButtonText(Data.SystemText.Get("Shop_0082"));
			_dialogSelectBuyMeans.onPushButton1 = delegate
			{
				_purchaseProductName = sInfo.saleInfo.name;
				UIManager.GetInstance().createInSceneCenterLoading();
				if (sInfo.is_completed && sInfo._rewardStatus == SkinSeriesPurchaseInfo.RewardStatus.not_got)
				{
					StartBuySkin(eBuyType.rewardOnly, ShopCommonUtility.SalesType.free, sInfo.series_id, null);
				}
				else
				{
					StartBuySkin(eBuyType.multi, ShopCommonUtility.SalesType.free, sInfo.series_id, null);
				}
			};
		}
		else
		{
			_dialogSelectBuyMeans.SetButtonLayout(DialogBase.ButtonLayout.NONE);
			onPushBuyCrystalBtnCallBack = delegate
			{
				_createPurchaseConfirmDialog(sInfo.saleInfo, ShopCommonUtility.SalesType.crystal, delegate
				{
					StartBuySkin(eBuyType.multi, ShopCommonUtility.SalesType.crystal, sInfo.series_id, null);
				});
			};
			onPushBuyRupyBtnCallBack = delegate
			{
				_createPurchaseConfirmDialog(sInfo.saleInfo, ShopCommonUtility.SalesType.rupy, delegate
				{
					StartBuySkin(eBuyType.multi, ShopCommonUtility.SalesType.rupy, sInfo.series_id, null);
				});
			};
			onPushBuyTicketButtonCallBack = delegate
			{
				_createPurchaseConfirmDialog(sInfo.saleInfo, ShopCommonUtility.SalesType.ticket, delegate
				{
					StartBuySkin(eBuyType.multi, ShopCommonUtility.SalesType.ticket, sInfo.series_id, sInfo.saleInfo.costTicketItemId);
				});
			};
		}
		component.Init(sInfo, _dialogSelectBuyMeans, onPushBuyCrystalBtnCallBack, onPushBuyRupyBtnCallBack, onPushBuyTicketButtonCallBack);
	}

	private void createClassSkinSelectBuyMeansDialog(SkinProductInfo pInfo)
	{
		if (_dialogSelectBuyMeans != null)
		{
			return;
		}
		_ = pInfo.is_purchased;
		_dialogSelectBuyMeans = _createBaseDialogForSelectBuyMeans(pInfo.saleInfo);
		ClassSkinSelectBuyMeansDialog component = UnityEngine.Object.Instantiate(_PrefabDialogSelectBuyMeans).GetComponent<ClassSkinSelectBuyMeansDialog>();
		_dialogSelectBuyMeans.SetObj(component.gameObject);
		Action onPushBuyCrystalBtnCallBack = null;
		Action onPushBuyRupyBtnCallBack = null;
		Action onPushBuyTicketButtonCallBack = null;
		if (pInfo.saleInfo.isFree)
		{
			_dialogSelectBuyMeans.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			_dialogSelectBuyMeans.SetButtonText(Data.SystemText.Get("Shop_0082"));
			_dialogSelectBuyMeans.onPushButton1 = delegate
			{
				_purchaseProductName = pInfo.saleInfo.name;
				_purchaseProductInfo = pInfo;
				UIManager.GetInstance().createInSceneCenterLoading();
				StartBuySkin(eBuyType.single, ShopCommonUtility.SalesType.free, pInfo.product_id, null);
			};
		}
		else
		{
			_dialogSelectBuyMeans.SetButtonLayout(DialogBase.ButtonLayout.NONE);
			onPushBuyCrystalBtnCallBack = delegate
			{
				_createPurchaseConfirmDialog(pInfo.saleInfo, ShopCommonUtility.SalesType.crystal, delegate
				{
					_purchaseProductInfo = pInfo;
					StartBuySkin(eBuyType.single, ShopCommonUtility.SalesType.crystal, pInfo.product_id, null);
				});
			};
			onPushBuyRupyBtnCallBack = delegate
			{
				_createPurchaseConfirmDialog(pInfo.saleInfo, ShopCommonUtility.SalesType.rupy, delegate
				{
					_purchaseProductInfo = pInfo;
					StartBuySkin(eBuyType.single, ShopCommonUtility.SalesType.rupy, pInfo.product_id, null);
				});
			};
			onPushBuyTicketButtonCallBack = delegate
			{
				_createPurchaseConfirmDialog(pInfo.saleInfo, ShopCommonUtility.SalesType.ticket, delegate
				{
					_purchaseProductInfo = pInfo;
					StartBuySkin(eBuyType.single, ShopCommonUtility.SalesType.ticket, pInfo.product_id, pInfo.saleInfo.costTicketItemId);
				});
			};
		}
		component.Init(pInfo, _dialogSelectBuyMeans, onPushBuyCrystalBtnCallBack, onPushBuyRupyBtnCallBack, onPushBuyTicketButtonCallBack);
	}

	private DialogBase _createBaseDialogForSelectBuyMeans(ShopCommonSaleInfo info)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetTitleLabel(Data.SystemText.Get("Dia_BuySkin_002_Title"));
		dialogBase.SetReturnMsg(null, "");
		return dialogBase;
	}

	private void _createPurchaseConfirmDialog(ShopCommonSaleInfo info, ShopCommonUtility.SalesType costType, Action buyApiFunc)
	{
		if (!(_dialogPurchaseConfirm != null) && ShopCommonUtility.IsHaveEnoughCost(info, costType, delegate
		{
			if (_dialogCrystalShortage == null)
			{
				_dialogCrystalShortage = ShopCommonUtility.CreateCrystalShortagePopup();
			}
		}))
		{
			_dialogPurchaseConfirm = ShopCommonUtility.CreatePurchaseConfirmPopup(info, costType, _PrefabDialogPurchaseConfirm, delegate
			{
				_purchaseProductName = info.name;
				buyApiFunc();
			});
			_dialogPurchaseConfirm.SetTitleLabel(Data.SystemText.Get("Dia_BuySkin_001_Title"));
		}
	}

	private void _OnFailurePurchase(NetworkTask.ResultCode code)
	{
		_isBuyConnect = false;
		UIManager.GetInstance().closeInSceneCenterLoading();
	}

	private void _OnResultCodeError(int code)
	{
		if (code != 110)
		{
			_isBuyConnect = false;
			UIManager.GetInstance().closeInSceneCenterLoading();
			_ReloadSkinInfo();
		}
	}

	private void onSuccessPurchaseSingle(NetworkTask.ResultCode error)
	{
		_isBuyConnect = false;
		PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_PURCHASE_SKIN_SERIES_ID, _selectSeriesInfo.series_id);
		if (_purchaseProductName != null)
		{
			DialogBase diaChange = UIManager.GetInstance().CreateDialogClose();
			int skinId = _purchaseProductInfo.leader_skin_id;
			// Pre-Phase-5b: pulled ClassCharacterMasterData via GetCharaPrmBySkinId + GetClanNameByKey.
			// Headless has no chara master; leave the purchase-success dialog text as the base
			// "purchased X" substitution without the class/chara suffix.
			ClassCharacterMasterData charaData = null;
			string text = Data.SystemText.Get("Shop_0022", _purchaseProductName.Replace("\n", ""));
			diaChange.SetText(text);
			diaChange.SetTitleLabel(Data.SystemText.Get("Dia_BuySkin_004_Title"));
			diaChange.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
			diaChange.SetButtonText(Data.SystemText.Get("Dia_BuySkin_001_Button"));
			diaChange.onPushButton1 = delegate
			{
				_tempCloseDialog = diaChange;
				LeaderSkinUpdateTask leaderSkinUpdateTask = new LeaderSkinUpdateTask();
				leaderSkinUpdateTask.SetParameter(charaData.class_id, skinId);
				StartCoroutine(Toolbox.NetworkManager.Connect(leaderSkinUpdateTask, onSuccessChangeSkin));
			};
		}
		_ReloadSkinInfo();
	}

	private void onSuccessChangeSkin(NetworkTask.ResultCode error)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetText(Data.SystemText.Get("Shop_0109"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		_purchaseProductInfo = null;
		if (_tempCloseDialog != null)
		{
			_tempCloseDialog.Close();
		}
	}

	private void onSuccessPurchaseMulti(NetworkTask.ResultCode error)
	{
		_isBuyConnect = false;
		PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.LAST_PURCHASE_SKIN_SERIES_ID, _selectSeriesInfo.series_id);
		if (_purchaseProductName != null)
		{
			ShopCommonUtility.CreatePurchaseSuccess(_purchaseProductName.Replace("\n", ""));
			_purchaseProductName = null;
		}
		_ReloadSkinInfo();
	}

	private void _ReloadSkinInfo()
	{
		MyPageMenu.Instance.UpdateCrystalCount();
		MyPageMenu.Instance.UpdateRupyCount();
		List<SkinSeriesPurchaseInfo> oldSeriesList = new List<SkinSeriesPurchaseInfo>(Data.SkinPurchaseInfo.seriesList);
		StartGetClassSkinInfo(delegate
		{
			bool flag = false;
			List<SkinSeriesPurchaseInfo> seriesList = Data.SkinPurchaseInfo.seriesList;
			for (int i = 0; i < seriesList.Count; i++)
			{
				if (oldSeriesList[i].series_id != seriesList[i].series_id)
				{
					flag = true;
				}
				if (seriesList[i].series_id == _selectSeriesInfo.series_id)
				{
					if (_selectPlate.ProductInfo == null)
					{
						onSelectSeries(i);
					}
					else if (_selectPlate.ProductInfo.product_id == _purchaseProductInfo.product_id)
					{
						_selectSeriesInfo = seriesList[i];
						int num = ((_selectSeriesInfo.SetSalesStatus != SkinSeriesPurchaseInfo.eSetSalesStatus.None) ? 1 : 0);
						for (int j = 0; j < _selectSeriesInfo.productList.Count; j++)
						{
							UpdateScrollItem(_productPlateObjList[j + num], j + num);
						}
						if (_MultiPlateObj != null)
						{
							UpdateScrollItem(_MultiPlateObj, 0);
						}
					}
					else
					{
						onSelectSeries(i);
					}
				}
			}
			if (oldSeriesList.Count != seriesList.Count)
			{
				flag = true;
			}
			_purchaseProductName = null;
			_purchaseProductInfo = null;
			_selectPlate = null;
			if (flag)
			{
				List<int> seriesIdList = GetSeriesIdList();
				Dictionary<int, BaseSeriesData> seriesDataDictionary = GetSeriesDataDictionary();
				StartCoroutine(loadSeriesImages(ResourcesManager.AssetLoadPathType.ShopClassSkin, seriesIdList, seriesDataDictionary, delegate
				{
					List<SkinSeriesPurchaseInfo> seriesList2 = Data.SkinPurchaseInfo.seriesList;
					List<ShopDrumrollScrollManager.DrumrollItem> itemList = _drumrollSeriesImageList.Select((Texture tex, int index) => new ShopDrumrollScrollManager.DrumrollItem(tex, seriesList2[index].IsNew)).ToList();
					StartCoroutine(_drumrollManager.CreateDrumrollScroll_Coroutine(itemList, 0, onSelectSeries, delegate
					{
						onSelectSeries(0);
						UIManager.GetInstance().closeInSceneCenterLoading();
					}));
				}));
			}
			else
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
			}
		});
	}
}
