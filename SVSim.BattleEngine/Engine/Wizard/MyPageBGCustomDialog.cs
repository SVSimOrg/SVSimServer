using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard.UI.Dialog.ImageSelection;

namespace Wizard;

public class MyPageBGCustomDialog : MonoBehaviour
{

	[SerializeField]
	private ImageSelection _imageSelection;

	private List<string> _randomIdList = new List<string>();

	private static readonly string[] FIRST_SELECT_SKIN = new string[8] { "1211410310", "1212410310", "1213410310", "1214410310", "1215410310", "1216410310", "1217410310", "1218410310" };

	public static void Create(Action<MyPageDetail.BGType, string, List<string>> onDecide)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/layoutParts/MyPage/MyPageBGCustomDialog")) as GameObject;
		MyPageBGCustomDialog customDialog = gameObject.GetComponent<MyPageBGCustomDialog>();
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.XL);
		dialogBase.SetTitleLabel(Data.SystemText.Get("MyPage_0102"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		dialogBase.SetObj(gameObject);
		dialogBase.onPushButton1 = delegate
		{
			string id = customDialog._imageSelection.GetSelectedItemKey();
			if (id == null)
			{
				goto IL_0060;
			}
			string text = id;
			MyPageDetail.BGType type;
			if (!(text == "deck"))
			{
				if (!(text == "random"))
				{
					goto IL_0060;
				}
				type = MyPageDetail.BGType.RandomBG;
			}
			else
			{
				type = MyPageDetail.BGType.Deck;
			}
			goto IL_0067;
			IL_0067:
			bool flag = false;
			MyPageBGInfo bGInfo = Data.MyPage.data.BGInfo;
			if (type != bGInfo.BGType)
			{
				flag = true;
			}
			if (type == MyPageDetail.BGType.CustomBG && id != bGInfo.Id)
			{
				flag = true;
			}
			if (IsNotEqualList(bGInfo.RandomIdList, customDialog._randomIdList))
			{
				flag = true;
			}
			if (flag)
			{
				Data.MyPage.data.BGInfo.Id = ((type == MyPageDetail.BGType.CustomBG) ? id : string.Empty);
				Data.MyPage.data.BGInfo.RandomIdList = customDialog._randomIdList;
				Data.MyPage.data.BGInfo.BGType = type;
				MyPageBGInfo bGInfo2 = Data.MyPage.data.BGInfo;
				MyPageSettingUpdateTask myPageSettingUpdateTask = new MyPageSettingUpdateTask();
				myPageSettingUpdateTask.SetParameter(bGInfo2.BGType, bGInfo2.Id, bGInfo2.RandomIdList);
				UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(myPageSettingUpdateTask, delegate
				{
					onDecide.Call(type, id, customDialog._randomIdList);
				}));
			}
			return;
			IL_0060:
			type = MyPageDetail.BGType.CustomBG;
			goto IL_0067;
		};
		customDialog.Initialize();
	}

	private static bool IsNotEqualList(List<string> current, List<string> after)
	{
		if (current.Count != after.Count)
		{
			return true;
		}
		for (int i = 0; i < current.Count; i++)
		{
			if (current[i] != after[i])
			{
				return true;
			}
		}
		return false;
	}

	private void Initialize()
	{
		_randomIdList = Data.MyPage.data.BGInfo.RandomIdList;
		if (_randomIdList.Count == 0)
		{
			List<string> list = new List<string>();
			string[] fIRST_SELECT_SKIN = FIRST_SELECT_SKIN;
			foreach (string item in fIRST_SELECT_SKIN)
			{
				if (Data.Load.data.AcquiredMyPageBGList.Contains(item))
				{
					list.Add(item);
				}
			}
			_randomIdList = list;
		}
		_imageSelection.Create();
		_imageSelection.AddItem("deck", null, isSelectable: true, null, null, null, isDisplaySprite: true, string.Empty, new string[2]
		{
			Data.SystemText.Get("MyPage_0104"),
			Data.SystemText.Get("MyPage_0105")
		}, null, delegate
		{
			OnClickDeckView();
		});
		_imageSelection.AddItem("random", null, isSelectable: true, null, null, null, isDisplaySprite: true, string.Empty, new string[2]
		{
			Data.SystemText.Get("MyPage_0106"),
			Data.SystemText.Get("MyPage_0107")
		}, null, delegate
		{
			OnClickRandomSelect();
		});
		string key = "deck";
		if (Data.MyPage.data.BGInfo.BGType == MyPageDetail.BGType.RandomBG)
		{
			key = "random";
		}
		foreach (MyPageCustomBGMasterData myPageCustomBGMaster in Data.Master.MyPageCustomBGMasterList)
		{
			string id = myPageCustomBGMaster.Id;
			if (Data.Load.data.AcquiredMyPageBGList.Contains(id))
			{
				if (id == Data.MyPage.data.BGInfo.Id)
				{
					key = id;
				}
				ResourcesManager.AssetLoadPathType type = ResourcesManager.AssetLoadPathType.MyPageBackGroundIcon;
				_imageSelection.AddItem(id, null, isSelectable: true, () => true, Toolbox.ResourcesManager.GetAssetTypePath(id, type), Toolbox.ResourcesManager.GetAssetTypePath(id, type, isfetch: true), isDisplaySprite: false, string.Empty, null, delegate
				{
				});
			}
		}
		_imageSelection.SelectItemWithKey(key);
		_imageSelection.SetDisplayPage(_imageSelection.SelectItemWithKey(key));
		_imageSelection.LoadDisplayPage();
		_imageSelection.Open();
	}

	private void OnClickDeckView()
	{
	}

	private void OnClickRandomSelect()
	{
		MyPageBGRandomSelectDialog.Create(_randomIdList, 650, delegate(List<string> newSelectList)
		{
			_randomIdList = newSelectList;
		});
	}
}
