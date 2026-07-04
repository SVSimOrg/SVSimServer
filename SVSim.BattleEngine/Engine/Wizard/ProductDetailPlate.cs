using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class ProductDetailPlate : MonoBehaviour
{
	[SerializeField]
	private UILabel _labelProductName;

	[SerializeField]
	private UISprite _spriteClassIcon;

	[SerializeField]
	private SupplyLabelPlateListUI _supplyLabelPlateList;

	public float SetProductData(string name, ClassCharacterMasterData charaData, List<ShopCommonRewardInfo> rewardInfoList)
	{
		_labelProductName.text = name;
		ClassCharaPrm.SetClassLabelSetting(_labelProductName, charaData.ClassColorId);
		_spriteClassIcon.spriteName = ClassCharaPrm.GetIconSpriteName(charaData.clan);
		return _supplyLabelPlateList.SetSupplyList(rewardInfoList) + 30f;
	}
}
