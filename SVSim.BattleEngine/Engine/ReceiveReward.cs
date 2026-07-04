using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;

public class ReceiveReward : MonoBehaviour
{
	private GameObject currentItem;

	private GameObject currentExpand;

	private UIButton currentExpandButton;

	private List<GameObject> _allItem = new List<GameObject>();

	public DialogBase ShowReadDialog(List<ReceivedReward> texts, GameObject itemPrefab, GameObject current, ResourceHandler resourceHandler, DialogBase dialog = null)
	{
		if (texts == null)
		{
			return null;
		}
		currentItem = null;
		currentExpand = null;
		currentExpandButton = null;
		DialogBase dia;
		if (dialog == null)
		{
			dia = createDialog();
		}
		else
		{
			dia = dialog;
		}
		UITable table = createTable(dia, texts.Count);
		Action<SceneTransition.TransitionData> gotoScene = delegate(SceneTransition.TransitionData data)
		{
			dia.SetBackViewToNotCloseDialog();
			dia.gameObject.SetActive(value: false);
			SceneTransition.ChangeScene(data, delegate
			{
				if (current != null)
				{
					current.SetActive(value: false);
				}
			});
		};
		foreach (ReceivedReward t in texts)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(itemPrefab);
			gameObject.transform.parent = table.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			NguiObjs component = gameObject.GetComponent<NguiObjs>();
			_allItem.Add(gameObject);
			UIButton expandButton = component.buttons[0];
			UIButton actionButton = component.buttons[1];
			UIButton actionButtonAlone = component.buttons[2];
			GameObject expandPart = component.objs[0];
			GameObject itemPart = component.objs[2];
			SystemText sysText = Data.SystemText;
			Action<string, SceneTransition.TransitionData, bool, Action> AddButtonAction = delegate(string textid, SceneTransition.TransitionData data, bool alone, Action action2)
			{
				UIManager.SetObjectToGrey(AddDialogItemButton(alone ? actionButtonAlone : actionButton, sysText.Get(textid), action2).gameObject, SceneTransition.IsMaintenance(data));
			};
			Action<string, SceneTransition.TransitionData, bool> action = delegate(string textid, SceneTransition.TransitionData data, bool alone)
			{
				AddButtonAction(textid, data, alone, delegate
				{
					gotoScene(data);
				});
			};
			GiftTransition giftTransition = Data.Master.GiftTransitionList.Find((GiftTransition data) => (t.reward_type == 4) ? (data._rewardType == t.reward_type && data._rewardDetailId == t.rewardUserGoodsId) : (data._rewardType == t.reward_type));
			bool flag = t.IsUsable;
			if (giftTransition != null)
			{
				bool arg = giftTransition._buttons.Count == 1;
				foreach (GiftTransition.TransitionButton button in giftTransition._buttons)
				{
					action(button._text, button._transitionData, arg);
				}
			}
			else
			{
				flag = false;
			}
			bool flag2 = t.reward_type == 4;
			component.textures[0].gameObject.SetActive(value: true);
			if (flag2)
			{
				SetTicket(t.rewardUserGoodsId, t.reward_count, component.textures[0], component.labels[0], resourceHandler);
			}
			else
			{
				component.labels[0].text = getTitle((UserGoods.Type)t.reward_type, t.rewardUserGoodsId, t.reward_count);
				SetTexture((UserGoods.Type)t.reward_type, t.rewardUserGoodsId, component.textures[0], resourceHandler);
			}
			initExpand(expandPart, expandButton, itemPart, component);
			actionButton.gameObject.SetActive(value: false);
			actionButtonAlone.gameObject.SetActive(value: false);
			component.objs[1].SetActive(flag);
			component.objs[1].GetComponent<UITable>().Reposition();
			component.labels[2].gameObject.SetActive(!flag);
			if (!flag)
			{
				if (flag2 && t.rewardUserGoodsId == 2)
				{
					component.labels[2].text = Data.SystemText.Get("Mail_0060");
				}
				else
				{
					component.labels[2].text = Data.SystemText.Get("Mission_0045");
				}
			}
		}
		table.onReposition = delegate
		{
			dia.SetScrollViewActive(b: true);
			table.onReposition = null;
		};
		table.Reposition();
		dia.SetScrollViewActive(b: true);
		return dia;
	}

	public void SetAllButtonDisable()
	{
		foreach (GameObject item in _allItem)
		{
			UIButton[] buttons = item.GetComponent<NguiObjs>().buttons;
			foreach (UIButton uIButton in buttons)
			{
				if (uIButton != null)
				{
					uIButton.isEnabled = false;
				}
			}
		}
	}

	private static DialogBase createDialog()
	{
		bool num = Data.Load.data._userTutorial.TutorialStep != 100;
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = (num ? MyPageMenu.CreateDialogForTutorial() : UIManager.GetInstance().CreateDialogClose());
		dialogBase.SetTitleLabel(systemText.Get("Mail_0021"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetScrollViewActive(b: true);
		if (num)
		{
			dialogBase.SetDialogNoClose();
			MyPageMenu.Instance.SetGuideToOkOnlyDialog(dialogBase);
		}
		return dialogBase;
	}

	private static UITable createTable(DialogBase dia, int rewardNum)
	{
		GameObject gameObject = new GameObject("table");
		dia.ScrollView.contentPivot = ((rewardNum >= 3) ? UIWidget.Pivot.Top : UIWidget.Pivot.Center);
		dia.AttachToScrollView(gameObject.transform);
		UITable uITable = gameObject.AddComponent<UITable>();
		uITable.columns = 1;
		uITable.padding = new Vector2(0f, 5f);
		uITable.pivot = UIWidget.Pivot.Center;
		uITable.cellAlignment = UIWidget.Pivot.Center;
		return uITable;
	}

	private void DoAction(Action action)
	{
		action();
	}

	private void initExpand(GameObject expandPart, UIButton expandButton, GameObject itemPart, NguiObjs obs)
	{
		expandPart.SetActive(value: false);
		expandButton.gameObject.SetActive(value: true);
		expandButton.gameObject.AddComponent<UIDragScrollView>();
		currentItem = null;
		float itemOriginalX = itemPart.transform.localPosition.x;
		expandButton.onClick.Add(new EventDelegate(delegate
		{
			if (currentItem != null)
			{
				currentItem.SetActive(value: true);
				RemoveITween(currentItem);
				iTween.MoveTo(currentItem, iTween.Hash("x", itemOriginalX, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeOutQuad));
				currentExpandButton.gameObject.SetActive(value: true);
				currentExpand.SetActive(value: false);
			}
			Action action = delegate
			{
				itemPart.SetActive(value: false);
			};

			RemoveITween(itemPart);
			iTween.MoveTo(itemPart, iTween.Hash("x", itemOriginalX + 800f, "time", 0.2f, "islocal", true, "easetype", iTween.EaseType.easeInQuad, "oncomplete", "DoAction", "oncompletetarget", base.gameObject, "oncompleteparams", action));
			expandButton.gameObject.SetActive(value: false);
			expandPart.SetActive(value: true);
			obs.tweenAlpha.delay = 0.2f;
			obs.tweenAlpha.ResetToBeginning();
			obs.tweenAlpha.PlayForward();
			currentItem = itemPart;
			currentExpand = expandPart;
			currentExpandButton = expandButton;
		}));
	}

	private void RemoveITween(GameObject obj)
	{
		iTween component = obj.GetComponent<iTween>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
	}

	private static UIButton AddDialogItemButton(UIButton orig, string buttonText, Action action)
	{
		UIButton uIButton = UnityEngine.Object.Instantiate(orig);
		uIButton.GetComponentInChildren<UILabel>().text = buttonText;
		uIButton.transform.parent = orig.transform.parent;
		uIButton.transform.localPosition = orig.transform.localPosition;
		uIButton.transform.localScale = orig.transform.localScale;
		uIButton.onClick.Add(new EventDelegate(delegate
		{

			action();
		}));
		return uIButton;
	}

	public static string getTitle(MailData mailData)
	{
		return getTitle((UserGoods.Type)mailData.reward_type, mailData.RewardUserGoodsId, mailData.reward_count);
	}

	public static string getTitle(UserGoods.Type reward_type, long userGoodsId, long count)
	{
		string unit = getUnit(reward_type, userGoodsId);
		string userGoodsName = UserGoods.getUserGoodsName(reward_type, userGoodsId);
		return string.Format(unit, userGoodsName, count);
	}

	private static string getUnit(UserGoods.Type reward_type, long userGoodsId)
	{
		SystemText systemText = Data.SystemText;
		if (reward_type == UserGoods.Type.Item && ((int)userGoodsId == 1000 || (int)userGoodsId == 1001))
		{
			return systemText.Get("Mail_0040");
		}
		switch (reward_type)
		{
		case UserGoods.Type.Item:
		case UserGoods.Type.Card:
		case UserGoods.Type.SpotCard:
		case UserGoods.Type.SpotCardOnlyLatestCardPack:
			return systemText.Get("Mail_0041");
		case UserGoods.Type.Rupy:
			return systemText.Get("Mail_0042");
		case UserGoods.Type.SpotCardPoint:
			return systemText.Get("Mail_0063");
		default:
			return systemText.Get("Mail_0040");
		}
	}

	public static void SetTicket(long userGoodsId, long count, UITexture tex, UILabel label, ResourceHandler resourceHandler)
	{
		Item item = Data.Master.ItemList.Find((Item data) => data.UserGoodsId == userGoodsId);
		if (item != null)
		{
			string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(item.thumbnail, ResourcesManager.AssetLoadPathType.Item);
			resourceHandler.Add(assetTypePath, delegate
			{
				string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(item.thumbnail, ResourcesManager.AssetLoadPathType.Item, isfetch: true);
				tex.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath2);
			});
			string unitFormat = item.unitFormat;
			label.text = string.Format(unitFormat, item.name, count);
		}
		else
		{
			tex.gameObject.SetActive(value: false);
			label.text = "";
		}
	}

	public static void SetTexture(UserGoods.Type type, UITexture tex, ResourceHandler resourceHandler)
	{
		string texName = UserGoods.GetUserGoodsImageName(type, 0L);
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Item);
		resourceHandler.Add(assetTypePath, delegate
		{
			string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Item, isfetch: true);
			tex.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath2);
		});
	}

	public static void SetTexture(UserGoods.Type type, long goodsId, UITexture texture, ResourceHandler resourceHandler)
	{
		string thumbnailName = GetThumbnailName(type, goodsId);
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(thumbnailName, ResourcesManager.AssetLoadPathType.Item);
		resourceHandler.Add(assetTypePath, delegate
		{
			string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(thumbnailName, ResourcesManager.AssetLoadPathType.Item, isfetch: true);
			texture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath2);
		});
	}

	public static string GetThumbnailName(UserGoods.Type type, long id)
	{
		if (type == UserGoods.Type.Skin)
		{
			return GetSkinThumbnailName(id);
		}
		return UserGoods.GetUserGoodsImageName(type, id);
	}

	public static string GetSkinThumbnailName(long id)
	{
		if (ExistsIndividualSkinThumbnail(id, out var imageName))
		{
			return imageName;
		}
		return UserGoods.GetUserGoodsImageName(UserGoods.Type.Skin, 0L);
	}

	public static bool ExistsIndividualSkinThumbnail(long id, out string imageName)
	{
		UserGoods userGoods = new UserGoods(UserGoods.Type.Skin, id);
		imageName = userGoods.GetUserGoodsIndividualImageName();
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(imageName, ResourcesManager.AssetLoadPathType.Item);
		return Toolbox.ResourcesManager.ExistsAssetBundleManifest(assetTypePath);
	}
}
