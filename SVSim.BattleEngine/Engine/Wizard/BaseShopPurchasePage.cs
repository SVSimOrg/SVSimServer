using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class BaseShopPurchasePage : UIBase
{

	[SerializeField]
	private UITexture _bgTexture;

	[SerializeField]
	protected UITexture _titleLogoTexture;

	[SerializeField]
	protected UILabel _labelSeriesDescription;

	[SerializeField]
	protected UIScrollView _scrollView;

	[SerializeField]
	protected UIWrapContent _wrapContent;

	[SerializeField]
	protected GameObject _productPlateOriginal;

	protected List<int> _cacheSeriesIdList;

	protected List<int> _cacheRefCountList;

	protected List<string> _cacheResourceList;

	protected List<string> _loadedResourceList;

	protected List<Texture> _drumrollSeriesImageList = new List<Texture>();

	protected List<Texture> _seriesTitleImageList = new List<Texture>();

	public override void onFirstStart()
	{
		_cacheSeriesIdList = new List<int>(4);
		_cacheRefCountList = new List<int>(4);
		_cacheResourceList = new List<string>();
		_loadedResourceList = new List<string>();
		SetupScrollView();
		base.IsShowFooterMenu = true;
		_bgTexture.gameObject.layer = LayerMask.NameToLayer("FrontUI");
		base.onFirstStart();
	}

	protected virtual void SetupScrollView()
	{
		for (int i = 0; i < 4; i++)
		{
			NGUITools.AddChild(_wrapContent.gameObject, _productPlateOriginal);
		}
		_productPlateOriginal.gameObject.SetActive(value: false);
		_wrapContent.onInitializeItem = OnInitializeItem;
		_wrapContent.minIndex = 0;
	}

	protected void CreateTopBar(string title, Action onFinishChangeView)
	{
		UIManager.ChangeViewSceneParam changeViewSceneParam = new UIManager.ChangeViewSceneParam();
		changeViewSceneParam.MyPageMenuIndex = 5;
		changeViewSceneParam.IsCutCardMotion = true;
		changeViewSceneParam.OnFinishChangeView = onFinishChangeView;
		UIManager.GetInstance().CreateTopBar(base.gameObject, title, UIManager.ViewScene.MyPage, MoneyDraw: true, changeViewSceneParam).gameObject.layer = LayerMask.NameToLayer("MyPage");
	}

	protected IEnumerator loadSeriesImages(ResourcesManager.AssetLoadPathType assetPathType, List<int> seriesIdList, Dictionary<int, BaseSeriesData> seriesMasterDic, Action callBack = null)
	{
		List<string> assetList = new List<string>();
		for (int i = 0; i < seriesIdList.Count; i++)
		{
			BaseSeriesData baseSeriesData = seriesMasterDic[seriesIdList[i]];
			assetList.Add(Toolbox.ResourcesManager.GetAssetTypePath(baseSeriesData.DrumrollPath, assetPathType));
			assetList.Add(Toolbox.ResourcesManager.GetAssetTypePath(baseSeriesData.TitlePath, assetPathType));
		}
		assetList.Add(Toolbox.ResourcesManager.GetAssetTypePath("cmn_shop_icon_1", ResourcesManager.AssetLoadPathType.Effect2D));
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(assetList, null));
		_loadedResourceList.AddRange(assetList);
		_drumrollSeriesImageList.Clear();
		_seriesTitleImageList.Clear();
		for (int j = 0; j < seriesIdList.Count; j++)
		{
			BaseSeriesData baseSeriesData2 = seriesMasterDic[seriesIdList[j]];
			Texture item = Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath(baseSeriesData2.DrumrollPath, assetPathType, isfetch: true)) as Texture;
			_drumrollSeriesImageList.Add(item);
			Texture item2 = Toolbox.ResourcesManager.LoadObject(Toolbox.ResourcesManager.GetAssetTypePath(baseSeriesData2.TitlePath, assetPathType, isfetch: true)) as Texture;
			_seriesTitleImageList.Add(item2);
		}
		callBack.Call();
	}

	protected override void onClose()
	{
		AllDeleteCacheSeries();
		if (_loadedResourceList.Count > 0)
		{
			Toolbox.ResourcesManager.RemoveAssetGroup(_loadedResourceList);
			_loadedResourceList.Clear();
		}
		base.onClose();
	}

	private void AllDeleteCacheSeries()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_cacheResourceList);
		_cacheSeriesIdList.Clear();
		_cacheRefCountList.Clear();
		_cacheResourceList.Clear();
	}

	protected virtual void ResetProductListScroll(int productCount)
	{
		_wrapContent.maxIndex = productCount - 1;
		_wrapContent.SortBasedOnScrollMovement();
		_scrollView.ResetPosition();
		_wrapContent.WrapContent();
	}

	protected virtual void OnInitializeItem(GameObject go, int wrapIndex, int realIndex)
	{
	}
}
