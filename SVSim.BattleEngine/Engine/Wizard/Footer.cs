using System;
using UnityEngine;
using Wizard.RoomMatch;

namespace Wizard;

public class Footer : UIBase
{
	[SerializeField]
	private UIButton[] underButtons;

	[SerializeField]
	private UISprite[] underButtonSprites;

	[SerializeField]
	private UILabel[] underButtonLabels;

	[SerializeField]
	private UILabelGradientOverwriter[] _labelGradientOverwriters;

	[SerializeField]
	private UILabelEffectOverwriter[] _labelEffectOverwriters;

	[SerializeField]
	private GameObject InviteIcon;

	[SerializeField]
	private GameObject ArenaIcon;

	[SerializeField]
	private GameObject _soloPlayIcon;

	[SerializeField]
	private GameObject _shopIcon;

	[SerializeField]
	private GameObject OtherIcon;

	private string[] menuIdleTexTbl = new string[7] { "btn_main_home_off", "btn_main_story_off", "btn_main_battle_off", "btn_main_arena_off", "btn_main_card_off", "btn_main_shop_off", "btn_main_other_off" };

	private string[] menuPushTexTbl = new string[7] { "btn_main_home_on", "btn_main_story_on", "btn_main_battle_on", "btn_main_arena_on", "btn_main_card_on", "btn_main_shop_on", "btn_main_other_on" };

	public UIButton[] _underButtons => underButtons;

	public int CurrentIndex { get; private set; }

	private void Awake()
	{
		UpdateCurrentIndex(0);
		InviteIconDisp(inDisp: false);
		UpdateArenaBadgeIcon();
		SoloPlayIconDisp(inDisp: false);
		SetShopIconVisible(visible: false);
		underButtonLabels[0].text = Data.SystemText.Get("MyPage_0001");
		underButtonLabels[1].text = Data.SystemText.Get("MyPage_0002");
		underButtonLabels[2].text = Data.SystemText.Get("MyPage_0003");
		underButtonLabels[3].text = Data.SystemText.Get("MyPage_0004");
		underButtonLabels[4].text = Data.SystemText.Get("MyPage_0005");
		underButtonLabels[5].text = Data.SystemText.Get("MyPage_0006");
		underButtonLabels[6].text = Data.SystemText.Get("MyPage_0007");
		for (int i = 0; i < underButtons.Length; i++)
		{
			UIButton buttonUnder = underButtons[i];
			buttonUnder.gameObject.layer = 27;
			buttonUnder.onClick.Clear();
			buttonUnder.onClick.Add(new EventDelegate(delegate
			{
				UIManager uiMgr = UIManager.GetInstance();
				int selectedMenu = int.Parse(buttonUnder.name);
				if (uiMgr.IsTouchable)
				{
					if (MyPageMenu.Instance != null)
					{
						if (!MyPageMenu.Instance.IsVisible)
						{
							Action action = delegate
							{
								UIManager.ChangeViewSceneParam param2 = new UIManager.ChangeViewSceneParam
								{
									MyPageMenuIndex = selectedMenu,
									IsCutCardMotion = false
								};
								if (UIManager.GetInstance().IsCurrentScene(UIManager.ViewScene.Room))
								{
									(UIManager.GetInstance().GetUIBase(UIManager.ViewScene.Room) as RoomRoot).CreateChangeSceneDialog(UIManager.ViewScene.MyPage, param2, delegate
									{
										UpdateCurrentIndex(selectedMenu);
									});
								}
								else
								{
									buttonSoundMenu();
									uiMgr.ChangeViewScene(UIManager.ViewScene.MyPage, param2);
									UpdateCurrentIndex(selectedMenu);
								}
							};
							if (uiMgr.GetUiBaseOfCurrentScene().IsGetOutOfScene(action))
							{
								action();
							}
						}
						else
						{
							buttonSoundMenu();
							if (CanPushCurrentButton(selectedMenu))
							{
								MyPageMenu.Instance.ChangeMenu(selectedMenu);
							}
						}
					}
					else
					{
						UIManager.ChangeViewSceneParam param = new UIManager.ChangeViewSceneParam
						{
							MyPageMenuIndex = selectedMenu,
							IsCutCardMotion = false
						};
						if (UIManager.GetInstance().IsCurrentScene(UIManager.ViewScene.Room))
						{
							(UIManager.GetInstance().GetUIBase(UIManager.ViewScene.Room) as RoomRoot).CreateChangeSceneDialog(UIManager.ViewScene.MyPage, param, delegate
							{
								UpdateCurrentIndex(selectedMenu);
							});
						}
						else
						{
							buttonSoundMenu();
							uiMgr.ChangeViewScene(UIManager.ViewScene.MyPage, param);
							UpdateCurrentIndex(selectedMenu);
						}
					}
				}
			}));
		}
		UpdateArenaBadgeIcon();
		UpdateInviteIcon();
		UpdateOtherBadgeIcon();
		UpdateSoloPlayBadgeIcon();
		UpdateShopBadgeIcon();
	}

	protected bool CanPushCurrentButton(int selectedMenuIndex)
	{
		if (CurrentIndex != selectedMenuIndex)
		{
			return true;
		}
		if (UIManager.GetInstance().GetCurrentScene() == UIManager.ViewScene.MyPage && MyPageMenu.Instance.IsEnableFooterCurrentMenu)
		{
			return true;
		}
		return false;
	}

	private void buttonSoundMenu()
	{

	}

	public void UpdateCurrentIndex(int pushMenuIdx)
	{
		CurrentIndex = pushMenuIdx;
		int num = menuIdleTexTbl.Length;
		for (int i = 0; i < num; i++)
		{
			underButtonSprites[i].spriteName = menuIdleTexTbl[i];
		}
		underButtonSprites[pushMenuIdx].spriteName = menuPushTexTbl[pushMenuIdx];
	}

	public void SetButtonEnableColorChange(int btnIndex, bool btnIsEnable = true)
	{
		underButtons[btnIndex].isEnabled = btnIsEnable;
		if (btnIsEnable)
		{
			underButtonLabels[btnIndex].color = LabelDefine.TEXT_COLOR_BUTTON_ENABLE;
			underButtonSprites[btnIndex].color = Color.white;
			underButtonLabels[btnIndex].effectColor = LabelDefine.OUTLINE_COLOR_FOOTER_DEFAULT;
			return;
		}
		underButtonLabels[btnIndex].color = LabelDefine.TEXT_COLOR_BUTTON_DISABLE;
		underButtonSprites[btnIndex].color = LabelDefine.TEXT_COLOR_BUTTON_DISABLE;
		if (Array.IndexOf(menuIdleTexTbl, underButtonSprites[btnIndex].spriteName) >= 0)
		{
			underButtonLabels[btnIndex].effectColor = LabelDefine.OUTLINE_COLOR_FOOTER_DISABLE_DEFAULT;
		}
		else
		{
			underButtonLabels[btnIndex].effectColor = LabelDefine.OUTLINE_COLOR_FOOTER_DISABLE_PUSH;
		}
	}

	public void SetAllButtonEnableColorChange(bool isEnable)
	{
		for (int i = 0; i < underButtons.Length; i++)
		{
			SetButtonEnableColorChange(i, isEnable);
		}
	}

	public void InviteIconDisp(bool inDisp)
	{
		InviteIcon.SetActive(inDisp);
	}

	public void SoloPlayIconDisp(bool inDisp)
	{
		_soloPlayIcon.SetActive(inDisp);
	}

	private void OtherIconDisp(bool inDisp)
	{
		OtherIcon.SetActive(inDisp);
	}

	private void SetShopIconVisible(bool visible)
	{
		_shopIcon.SetActive(visible);
	}

	public void UpdateInviteIcon()
	{
		InviteIconDisp(Data.Load.data._receiveInviteCount > 0);
	}

	public void UpdateArenaBadgeIcon()
	{
		bool active = false;
		if (Data.Load.data._userTutorial.TutorialStep != 100)
		{
			ArenaIcon.SetActive(value: false);
			return;
		}
		if (Data.ArenaData != null && Data.ArenaData.ColosseumData.IsFreeEntry)
		{
			active = true;
		}
		if (Data.MyPageNotifications.data.IsCompetitionBadge)
		{
			active = true;
		}
		if (Data.MyPageNotifications.data.IsInviteGathering)
		{
			active = true;
		}
		if (Data.MyPageNotifications.data.GatheringMyPageInfo.IsMatchingNotification)
		{
			active = true;
		}
		ArenaIcon.SetActive(active);
	}

	public void UpdateSoloPlayBadgeIcon()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (Data.Load.data._userTutorial.TutorialStep == 100)
		{
			flag = Data.MyPageNotifications.data.QuestOpenInfo.IsDisplayBadge;
			flag2 = Data.MyPageNotifications.data.StoryNotification.IsDisplayBadge;
			flag3 = Data.MyPageNotifications.data.IsPracticePuzzleBadgeEnable;
		}
		SoloPlayIconDisp(flag || flag2 || flag3);
	}

	public void UpdateQuestBadgeIcon(bool isDisp)
	{
		bool flag = isDisp;
		bool flag2 = false;
		bool flag3 = false;
		if (Data.Load.data._userTutorial.TutorialStep == 100)
		{
			flag2 = Data.MyPageNotifications.data.StoryNotification.IsDisplayBadge;
			flag3 = Data.MyPageNotifications.data.IsPracticePuzzleBadgeEnable;
		}
		SoloPlayIconDisp(flag || flag2 || flag3);
	}

	public void UpdateOtherBadgeIcon()
	{
		OtherIconDisp(Data.MyPageNotifications.data.ReceiveFriendApplyCount > 0);
	}

	public void UpdateShopBadgeIcon()
	{
		if (Data.Load.data._userTutorial.TutorialStep != 100)
		{
			SetShopIconVisible(visible: false);
		}
		else if (Data.MyPageNotifications.data.ShopNotification.AppealCardPack == null)
		{
			SetShopIconVisible(visible: false);
		}
		else
		{
			SetShopIconVisible(Data.MyPageNotifications.data.ShopNotification.AppealCardPack.NeedsFooterBadgeIcon);
		}
	}

	public void OverwriteLabelColors(eColorCodeId gradientTopColorId, eColorCodeId gradientBottomColorId, eColorCodeId effectColorId)
	{
		OverwriteLabelGradients(gradientTopColorId, gradientBottomColorId);
		OverwriteLabelEffects(effectColorId);
	}

	public void CancelOverwriteLabelColors()
	{
		CancelOverwriteLabelGradients();
		CancelOverwriteLabelEffects();
	}

	private void OverwriteLabelGradients(eColorCodeId topColorId, eColorCodeId bottomColorId)
	{
		UILabelGradientOverwriter[] labelGradientOverwriters = _labelGradientOverwriters;
		foreach (UILabelGradientOverwriter obj in labelGradientOverwriters)
		{
			obj.enabled = true;
			obj.GradientTopColorId = topColorId;
			obj.GradientBottomColorId = bottomColorId;
		}
	}

	private void CancelOverwriteLabelGradients()
	{
		UILabelGradientOverwriter[] labelGradientOverwriters = _labelGradientOverwriters;
		for (int i = 0; i < labelGradientOverwriters.Length; i++)
		{
			labelGradientOverwriters[i].enabled = false;
		}
	}

	private void OverwriteLabelEffects(eColorCodeId colorId)
	{
		UILabelEffectOverwriter[] labelEffectOverwriters = _labelEffectOverwriters;
		foreach (UILabelEffectOverwriter obj in labelEffectOverwriters)
		{
			obj.enabled = true;
			obj.EffectColorId = colorId;
		}
	}

	private void CancelOverwriteLabelEffects()
	{
		UILabelEffectOverwriter[] labelEffectOverwriters = _labelEffectOverwriters;
		for (int i = 0; i < labelEffectOverwriters.Length; i++)
		{
			labelEffectOverwriters[i].enabled = false;
		}
	}
}
