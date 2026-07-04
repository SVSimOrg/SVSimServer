using UnityEngine;

namespace Wizard;

public class SupplyLabelPlate : MonoBehaviour
{
	[SerializeField]
	private UILabel _labelSupplyType;

	[SerializeField]
	private UILabel _labelSupplyName;

	public void SetSupplyText(ShopCommonRewardInfo rewardInfo)
	{
		string typeName = null;
		string detailName = null;
		ShopCommonUtility.GetRewardNames(rewardInfo, out typeName, out detailName);
		_labelSupplyType.text = typeName;
		_labelSupplyName.text = detailName;
		if (rewardInfo.IsAlreadyGet)
		{
			_labelSupplyName.text = Data.SystemText.Get("Shop_0237");
		}
	}
}
