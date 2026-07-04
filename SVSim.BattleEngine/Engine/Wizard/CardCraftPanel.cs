using System;
using UnityEngine;

namespace Wizard;

public class CardCraftPanel : MonoBehaviour
{
	[SerializeField]
	private UILabel _redetherNumLabel;

	[SerializeField]
	private UILabel _useRedetherLabel;

	[SerializeField]
	private UILabel _getRedetherLabel;

	[SerializeField]
	private UIButton _createButton;

	[SerializeField]
	private UIButton _destructButton;

	[SerializeField]
	private UILabel _cantCreateDestructLabel;

	[SerializeField]
	private UILabel _cantCreateLabel;

	[SerializeField]
	private UILabel _cantDestructLabel;

	public void UpdateCraftPanel(CardParameter inCardParam, bool isUpdateHaveRedether = true)
	{
		SystemText systemText = Data.SystemText;
		if (isUpdateHaveRedether)
		{
			_redetherNumLabel.text = PlayerStaticData.UserRedEtherCount.ToString();
		}
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		bool flag = dataMgr.FavoriteCardList.Contains(inCardParam.CardId);
		bool flag2 = dataMgr.GetPossessionCardNum(inCardParam.CardId, isIncludingSpotCard: false) == 0 && dataMgr.SpotCardData.ExistsSpotCard(inCardParam.CardId);
		bool flag3 = inCardParam.UseRedEther > 0;
		bool flag4 = inCardParam.GetRedEther > 0 && !flag && !flag2;
		if (inCardParam.IsPreReleaseCard)
		{
			flag4 = false;
			flag3 = false;
		}
		_createButton.gameObject.SetActive(flag3);
		_cantCreateLabel.gameObject.SetActive(!flag3 && !inCardParam.IsNotCraftDestruct);
		_destructButton.gameObject.SetActive(flag4);
		_cantDestructLabel.gameObject.SetActive(!flag4 && !inCardParam.IsNotCraftDestruct);
		_cantCreateDestructLabel.gameObject.SetActive(inCardParam.IsNotCraftDestruct);
		if (inCardParam.IsNotCraftDestruct)
		{
			string text = string.Empty;
			if (inCardParam.IsBasicCard)
			{
				text = systemText.Get("Card_0076");
			}
			else if (inCardParam.IsPrizeCard)
			{
				text = systemText.Get("Card_0194");
			}
			else if (inCardParam.IsPreReleaseCard)
			{
				text = systemText.Get("Card_0234");
			}
			_cantCreateDestructLabel.text = text;
			return;
		}
		int possessionCardNum = 0; // Pre-Phase-5b: headless has no user inventory
		if (flag3)
		{
			_useRedetherLabel.text = inCardParam.UseRedEther.ToString();
			bool flag5 = (possessionCardNum < 3 || flag2) && PlayerStaticData.UserRedEtherCount >= inCardParam.UseRedEther;
			UIManager.SetObjectToGrey(_createButton.gameObject, !flag5);
		}
		else if (inCardParam.IsFoil)
		{
			_cantCreateLabel.text = systemText.Get("Card_0075");
		}
		else
		{
			_cantCreateLabel.text = systemText.Get("Card_0183");
		}
		if (flag4)
		{
			_getRedetherLabel.text = inCardParam.GetRedEther.ToString();
			UIManager.SetObjectToGrey(_destructButton.gameObject, possessionCardNum <= 0);
		}
		else if (flag)
		{
			_cantDestructLabel.text = systemText.Get("Card_0148");
		}
		else if (flag2)
		{
			_cantDestructLabel.text = systemText.Get("Card_0231");
		}
	}
}
