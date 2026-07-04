using Cute;
using UnityEngine;
using Wizard.Scripts.Network.Data.TaskData.SkinPurchase;

namespace Wizard;

public class SkinProductDetail : BaseProductDetail
{

	[SerializeField]
	private GameObject _detailObjsParent;

	[SerializeField]
	private ProductDetailPlate _productTemplate;

	public void SetMultiProductDetail(SkinSeriesPurchaseInfo seriesInfo)
	{
		Texture textureProductImage = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(seriesInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ShopClassSkin, isfetch: true));
		SetProductDetail(textureProductImage, seriesInfo.rewardInfoList);
		if (Data.Master.LeaderSkinSeriesIdDic[seriesInfo.series_id].IsLargeImage)
		{
			_texProductImg.width = 370;
			_texProductImg.height = 213;
		}
		else
		{
			_texProductImg.width = 249;
			_texProductImg.height = 199;
		}
		_SetMultiSupplyList(seriesInfo);
		ResetPositionScrollView();
	}

	public void SetSingleProductDetail(SkinProductInfo productInfo, string descriptionText = null)
	{
		_productTemplate.gameObject.SetActive(value: false);
		Texture textureProductImage = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath(productInfo.saleInfo.path, ResourcesManager.AssetLoadPathType.ClassCharaSkinThumbnail, isfetch: true));
		SetProductDetail(textureProductImage, productInfo.rewardInfoList, descriptionText);
	}

	private void _SetMultiSupplyList(SkinSeriesPurchaseInfo info)
	{
		bool flag = info.is_completed && info._rewardStatus != SkinSeriesPurchaseInfo.RewardStatus.not_got;
		for (int i = 0; i < info.productList.Count; i++)
		{
			SkinProductInfo skinProductInfo = info.productList[i];
			if (!skinProductInfo.is_purchased || flag)
			{
				GameObject obj = NGUITools.AddChild(_detailObjsParent.gameObject, _productTemplate.gameObject);
				obj.transform.localPosition = Vector3.up * TailPosY;
				ProductDetailPlate component = obj.GetComponent<ProductDetailPlate>();
				ClassCharacterMasterData charaPrmBySkinId = null; // Pre-Phase-5b: no chara master headless
				TailPosY -= component.SetProductData(skinProductInfo.saleInfo.name, charaPrmBySkinId, skinProductInfo.rewardInfoList);
			}
		}
		_productTemplate.gameObject.SetActive(value: false);
	}
}
