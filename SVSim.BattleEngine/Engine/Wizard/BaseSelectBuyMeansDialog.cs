using System;
using Cute;
using UnityEngine;

namespace Wizard;

public abstract class BaseSelectBuyMeansDialog : MonoBehaviour
{
	private static readonly Vector3 BUY_OBJECTS_CENTER_POS = new Vector3(0f, -230f);

	private static readonly Vector3 BUY_OBJECTS_LEFT_POS = new Vector3(-140f, -230f);

	private static readonly Vector3 BUY_OBJECTS_RIGHT_POS = new Vector3(140f, -230f);

	[SerializeField]
	private UILabel _labelDescription;

	[SerializeField]
	private UIButton m_buttonRupy;

	[SerializeField]
	private UIButton m_buttonCrystal;

	[SerializeField]
	private UIButton _ticketButton;

	[SerializeField]
	private GameObject _objectUnderLine;

	[SerializeField]
	private UILabel m_labelRupyHaveNum;

	[SerializeField]
	private UILabel m_labelCrystalHaveNum;

	[SerializeField]
	private UILabel _labelTicketHaveNum;

	[SerializeField]
	private UILabel m_labelRupyCostNum;

	[SerializeField]
	private UILabel m_labelCrystalCostNum;

	[SerializeField]
	private UILabel _labelTicketCostNum;

	[SerializeField]
	private UILabel m_labelRupyBtnBuy;

	[SerializeField]
	private UILabel m_labelCrystalBtnBuy;

	[SerializeField]
	private UILabel _labelTicketButtonBuy;

	[SerializeField]
	private UITexture _ticketIconTexture;

	private DialogBase m_dialog;

	protected void SetDescriptionLabel(string text)
	{
		_labelDescription.text = text;
	}

	protected string GetDescriptionText(ShopCommonSaleInfo info)
	{
		return Data.SystemText.Get("Shop_0047", info.name.Replace("\n", ""));
	}

	protected void _Init(ShopCommonSaleInfo info, DialogBase dialog, Action onPushBuyCrystalBtnCallBack, Action onPushBuyRupyBtnCallBack, Action onPushTicketButtonCallBack)
	{
		SystemText systemText = Data.SystemText;
		m_dialog = dialog;
		_labelDescription.text = GetDescriptionText(info);
		if (_ticketButton != null)
		{
			_ticketButton.gameObject.SetActive(value: false);
		}
		if (info.isFree)
		{
			m_buttonCrystal.gameObject.SetActive(value: false);
			m_buttonRupy.gameObject.SetActive(value: false);
			UILabel labelDescription = _labelDescription;
			labelDescription.text = labelDescription.text + "\n" + systemText.Get("Shop_0103");
			_objectUnderLine.SetActive(value: false);
		}
		else if (info.costCrystal.HasValue && info.costRupy.HasValue)
		{
			m_buttonCrystal.gameObject.SetActive(value: true);
			m_buttonCrystal.gameObject.transform.localPosition = BUY_OBJECTS_RIGHT_POS;
			_SetCrystalBuyObjects(info.costCrystal.Value, onPushBuyCrystalBtnCallBack);
			m_buttonRupy.gameObject.SetActive(value: true);
			m_buttonRupy.gameObject.transform.localPosition = BUY_OBJECTS_LEFT_POS;
			_SetRupyBuyObjects(info.costRupy.Value, onPushBuyRupyBtnCallBack);
			UILabel labelDescription2 = _labelDescription;
			labelDescription2.text = labelDescription2.text + "\n" + systemText.Get("Shop_0107");
		}
		else if (info.costCrystal.HasValue)
		{
			m_buttonCrystal.gameObject.SetActive(value: true);
			m_buttonCrystal.gameObject.transform.localPosition = BUY_OBJECTS_CENTER_POS;
			_SetCrystalBuyObjects(info.costCrystal.Value, onPushBuyCrystalBtnCallBack);
			m_buttonRupy.gameObject.SetActive(value: false);
		}
		else if (info.costTicket.HasValue)
		{
			m_buttonRupy.gameObject.SetActive(value: false);
			m_buttonCrystal.gameObject.SetActive(value: false);
			_ticketButton.gameObject.SetActive(value: true);
			_ticketButton.gameObject.transform.localPosition = BUY_OBJECTS_CENTER_POS;
			if (_ticketIconTexture != null && info.costTicketItemId.HasValue)
			{
				_ticketIconTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconPath(info.costTicketItemId.Value.ToString(), isFetch: true));
			}
			SetTicketButtonObjects(info, onPushTicketButtonCallBack);
		}
		else
		{
			m_buttonRupy.gameObject.SetActive(value: true);
			m_buttonRupy.gameObject.transform.localPosition = BUY_OBJECTS_CENTER_POS;
			_SetRupyBuyObjects(info.costRupy.Value, onPushBuyRupyBtnCallBack);
			m_buttonCrystal.gameObject.SetActive(value: false);
		}
	}

	private void SetTicketButtonObjects(ShopCommonSaleInfo info, Action onPushTicketButtonCallBack)
	{
		_labelTicketHaveNum.text = info.haveTicketNum.Value.ToString();
		_labelTicketCostNum.text = info.costTicket.Value.ToString();
		ShopCommonUtility.SetButtonLabelStyle(_ticketButton, _labelTicketButtonBuy);
		_ticketButton.onClick.Clear();
		_ticketButton.onClick.Add(new EventDelegate(delegate
		{

			onPushTicketButtonCallBack();
			m_dialog.CloseWithoutSelect();
		}));
	}

	private void _SetCrystalBuyObjects(int costCrystal, Action onPushButtonCrystal)
	{
		m_labelCrystalHaveNum.text = PlayerStaticData.UserCrystalCount.ToString();
		m_labelCrystalCostNum.text = costCrystal.ToString();
		ShopCommonUtility.SetButtonLabelStyle(m_buttonCrystal, m_labelCrystalBtnBuy);
		m_buttonCrystal.onClick.Clear();
		m_buttonCrystal.onClick.Add(new EventDelegate(delegate
		{

			onPushButtonCrystal();
			m_dialog.CloseWithoutSelect();
		}));
	}

	private void _SetRupyBuyObjects(int costRupy, Action onPushButtonRupy)
	{
		m_labelRupyHaveNum.text = PlayerStaticData.UserRupyCount.ToString();
		m_labelRupyCostNum.text = costRupy.ToString();
		if (PlayerStaticData.UserRupyCount >= costRupy)
		{
			m_buttonRupy.isEnabled = true;
		}
		else
		{
			m_buttonRupy.isEnabled = false;
		}
		ShopCommonUtility.SetButtonLabelStyle(m_buttonRupy, m_labelRupyBtnBuy);
		m_buttonRupy.onClick.Clear();
		m_buttonRupy.onClick.Add(new EventDelegate(delegate
		{

			onPushButtonRupy();
			m_dialog.CloseWithoutSelect();
		}));
	}
}
