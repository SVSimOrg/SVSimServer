using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class SupplyLabelPlateListUI : MonoBehaviour
{
	[SerializeField]
	private SupplyLabelPlate _SupplyTemplate;

	[SerializeField]
	private UIGrid _parentGrid;

	public float SetSupplyList(List<ShopCommonRewardInfo> rewardInfoList)
	{
		_SupplyTemplate.gameObject.SetActive(value: false);
		if (rewardInfoList.Count <= 0)
		{
			return 0f;
		}
		for (int i = 0; i < rewardInfoList.Count; i++)
		{
			GameObject obj = NGUITools.AddChild(_parentGrid.gameObject, _SupplyTemplate.gameObject);
			obj.SetActive(value: true);
			obj.GetComponent<SupplyLabelPlate>().SetSupplyText(rewardInfoList[i]);
		}
		_parentGrid.Reposition();
		return (float)rewardInfoList.Count * _parentGrid.cellHeight + 30f;
	}
}
