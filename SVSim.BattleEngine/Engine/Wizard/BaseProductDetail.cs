using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public abstract class BaseProductDetail : MonoBehaviour
{
	[SerializeField]
	protected UITexture _texProductImg;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private SupplyLabelPlateListUI _supplyLabelPlateList;

	[SerializeField]
	private GameObject _parentDetailList;

	[SerializeField]
	private UILabel _labelDescription;

	[SerializeField]
	private UIGrid _gridDescriptionLines;

	[SerializeField]
	private GameObject _lineTemplate;

	protected float TailPosY;

	public void ResetPositionScrollView()
	{
		_scrollView.ResetPosition();
	}

	protected void SetProductDetail(Texture textureProductImage, List<ShopCommonRewardInfo> rewardInfoList, string descriptionText = null)
	{
		_texProductImg.mainTexture = textureProductImage;
		TailPosY = _supplyLabelPlateList.transform.localPosition.y;
		TailPosY -= _supplyLabelPlateList.SetSupplyList(rewardInfoList);
		if (descriptionText != null)
		{
			_parentDetailList.SetActive(value: true);
			_parentDetailList.transform.localPosition = Vector3.up * TailPosY;
			string text = "";
			text = Global.GetConvertWrapText(_labelDescription, descriptionText);
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					NGUITools.AddChild(_gridDescriptionLines.gameObject, _lineTemplate);
				}
			}
			_labelDescription.text = text;
			_gridDescriptionLines.Reposition();
		}
		else if (_parentDetailList != null)
		{
			_parentDetailList.SetActive(value: false);
		}
		ResetPositionScrollView();
	}
}
