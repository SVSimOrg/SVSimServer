using System.Collections;
using Cute;
using UnityEngine;

namespace Wizard;

public class PurchaseConfirm : MonoBehaviour
{

	[SerializeField]
	private UISprite _spriteConfirmItemIcon;

	[SerializeField]
	private UISprite _spriteHaveItemIcon;

	[SerializeField]
	private UITexture m_TextureConfirmTicket;

	[SerializeField]
	private UITexture m_TextureHaveTicket;

	[SerializeField]
	private UILabel m_LabelUseItemCnt;

	[SerializeField]
	private UILabel m_LabelBuyPack;

	[SerializeField]
	private UILabel m_LabelItemName;

	[SerializeField]
	private UILabel _premiumCardWarning;

	[SerializeField]
	private UILabel m_LabelBeforeItemCnt;

	[SerializeField]
	private UILabel m_LabelAfterItemCnt;

	[SerializeField]
	private UILabel m_LabelAfterItemUnit;

	[SerializeField]
	private GameObject _confirmObj;

	[SerializeField]
	private GameObject _haveObj;

	[SerializeField]
	private GameObject _rootObj;

	[SerializeField]
	private GameObject _jpnLawRoot;

	[SerializeField]
	private GameObject _saleTimeExistLayout;

	[SerializeField]
	private GameObject _saleTimeNoneLayout;

	[SerializeField]
	private UILabel _expiryTimeLabel;

	private void UpdateJpnLawObj()
	{
		_ = _rootObj == null;
	}

	private void HideJpnLawObj()
	{
		if (!(_jpnLawRoot == null))
		{
			_jpnLawRoot.SetActive(value: false);
		}
	}

	public void SetClystalConfirmDialog(int useItemNum, string purchaseText, int haveItemCnt, ShopExpirtyInfo expirtyInfo)
	{
		SetIconImage(UserGoods.Type.Crystal);
		int afterItemNum = haveItemCnt - useItemNum;
		string unit = Data.SystemText.Get("Common_0116");
		string useItemNumText = Data.SystemText.Get("Shop_0091", useItemNum.ToString());
		SetLabelText(Data.SystemText.Get("Common_0201"), useItemNumText, afterItemNum, unit, purchaseText, haveItemCnt);
		UpdateJpnLawObj();
		_expiryTimeLabel.gameObject.SetActive(expirtyInfo.IsEnableText);
		_expiryTimeLabel.text = expirtyInfo.GetText();
		if (_saleTimeExistLayout != null && _saleTimeNoneLayout != null)
		{
			_saleTimeExistLayout.SetActive(expirtyInfo.IsEnableText);
			_saleTimeNoneLayout.SetActive(!expirtyInfo.IsEnableText);
		}
		HideJpnLawObj();
	}

	public void SetRupyConfirmDialog(int useItemNum, string purchaseText, int haveItemCnt)
	{
		SetIconImage(UserGoods.Type.Rupy);
		int afterItemNum = haveItemCnt - useItemNum;
		string unit = Data.SystemText.Get("Common_0120");
		string useItemNumText = Data.SystemText.Get("Shop_0090", useItemNum.ToString());
		SetLabelText(Data.SystemText.Get("Common_0115"), useItemNumText, afterItemNum, unit, purchaseText, haveItemCnt);
		HideJpnLawObj();
	}

	public void SetLeaderSkinTicketConfirmDialog(int cost, string purchaseText, int haveItem, long itemId)
	{
		int afterItemNum = haveItem - cost;
		string unit = Data.SystemText.Get("Common_0117");
		string useItemNumText = Data.SystemText.Get("Shop_0042", cost.ToString());
		SetLabelText(Data.SystemText.Get("Common_0114"), useItemNumText, afterItemNum, unit, purchaseText, haveItem);
		SetIconImage(UserGoods.Type.Item, Item.Type.LeaderSkinTicket, Toolbox.ResourcesManager.LoadObject<Texture>(ShopCommonUtility.GetTicketIconRightDownPath(itemId.ToString(), isFetch: true)));
		HideJpnLawObj();
	}

	private void SetIconImage(UserGoods.Type type, Item.Type ticket = (Item.Type)0, Texture packIcon = null)
	{
		switch (type)
		{
		case UserGoods.Type.Crystal:
			ViewIconSprite("icon_crystal_s");
			break;
		case UserGoods.Type.Rupy:
			ViewIconSprite("icon_rupy_s");
			break;
		case UserGoods.Type.Item:
			switch (ticket)
			{
			case Item.Type.TwoPickTicket:
				ViewIconSprite("icon_2pick_s");
				break;
			case Item.Type.CardPackTicket:
			case Item.Type.LeaderSkinTicket:
				_spriteConfirmItemIcon.gameObject.SetActive(value: false);
				_spriteHaveItemIcon.gameObject.SetActive(value: false);
				m_TextureConfirmTicket.gameObject.SetActive(value: true);
				m_TextureHaveTicket.gameObject.SetActive(value: true);
				m_TextureConfirmTicket.mainTexture = packIcon;
				m_TextureHaveTicket.mainTexture = packIcon;
				break;
			case Item.Type.Orb:
				ViewIconSprite("icon_orb_s");
				break;
			case Item.Type.OrbPiece:
				ViewIconSprite("icon_orb_piece_s");
				break;
			}
			break;
		case UserGoods.Type.RedEther:
			ViewIconSprite("icon_liquid_s");
			break;
		case UserGoods.Type.SpotCardPoint:
			ViewIconSprite("icon_spotpoint_s");
			break;
		}
	}

	private void ViewIconSprite(string spriteName)
	{
		_spriteConfirmItemIcon.gameObject.SetActive(value: true);
		_spriteHaveItemIcon.gameObject.SetActive(value: true);
		m_TextureConfirmTicket.gameObject.SetActive(value: false);
		m_TextureHaveTicket.gameObject.SetActive(value: false);
		_spriteConfirmItemIcon.spriteName = spriteName;
		_spriteHaveItemIcon.spriteName = spriteName;
	}

	private void SetLabelText(string itemName, string useItemNumText, int afterItemNum, string unit, string purchaseText, int haveItemCnt)
	{
		m_LabelUseItemCnt.text = useItemNumText;
		m_LabelBuyPack.text = purchaseText;
		m_LabelItemName.text = itemName;
		m_LabelBeforeItemCnt.text = haveItemCnt.ToString();
		m_LabelAfterItemCnt.text = afterItemNum.ToString();
		m_LabelAfterItemUnit.text = unit;
	}

	public void SetWarningTextId(string warningTextId)
	{
		SystemText systemText = Data.SystemText;
		SetWarningText(systemText.Get(warningTextId));
	}

	public void SetWarningText(string warningText)
	{
		_premiumCardWarning.gameObject.SetActive(value: true);
		_premiumCardWarning.text = warningText;
		_confirmObj.transform.localPosition = new Vector3(0f, -40f, 0f);
		_haveObj.transform.localPosition = new Vector3(0f, -40f, 0f);
	}
}
