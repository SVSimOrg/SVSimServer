using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard.UI.Dialog.ImageSelection;

namespace Wizard;

public class MyPageBGRandomSelectDialog : MonoBehaviour
{
	[SerializeField]
	private ImageSelection _imageSelection;

	[SerializeField]
	private UIButton _btnAllOff;

	[SerializeField]
	private UILabel _labelButton;

	public static void Create(List<string> currentSelectIdList, int panelDepth, Action<List<string>> onDecide)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/layoutParts/MyPage/MyPageBGRandomSelectDialog")) as GameObject;
		MyPageBGRandomSelectDialog customDialog = gameObject.GetComponent<MyPageBGRandomSelectDialog>();
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.XL);
		dialogBase.SetTitleLabel(Data.SystemText.Get("MyPage_0103"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetObj(gameObject);
		dialogBase.SetPanelDepth(panelDepth);
		dialogBase.onPushButton1 = delegate
		{
			onDecide.Call(customDialog._imageSelection.GetSelectedList());
		};
		customDialog.Initialize(panelDepth, dialogBase, currentSelectIdList);
	}

	private void Initialize(int panelDepth, DialogBase dialog, List<string> currentSelectIdList)
	{
		_btnAllOff.onClick.Clear();
		_btnAllOff.onClick.Add(new EventDelegate(delegate
		{

			OnClickAllOffButton();
		}));
		_imageSelection.Create(panelDepth, dialog);
		foreach (MyPageCustomBGMasterData myPageCustomBGMaster in Data.Master.MyPageCustomBGMasterList)
		{
			if (currentSelectIdList.Contains(myPageCustomBGMaster.Id))
			{
				AddItem(myPageCustomBGMaster.Id);
			}
		}
		foreach (MyPageCustomBGMasterData myPageCustomBGMaster2 in Data.Master.MyPageCustomBGMasterList)
		{
			if (Data.Load.data.AcquiredMyPageBGList.Contains(myPageCustomBGMaster2.Id) && !currentSelectIdList.Contains(myPageCustomBGMaster2.Id))
			{
				AddItem(myPageCustomBGMaster2.Id);
			}
		}
		_imageSelection.SelectMultiItem(currentSelectIdList);
		_imageSelection.SetDisplayPage(0);
		_imageSelection.LoadDisplayPage();
		_imageSelection.Open();
		UpdateAllButton();
	}

	private void AddItem(string id)
	{
		ResourcesManager.AssetLoadPathType type = ResourcesManager.AssetLoadPathType.MyPageBackGroundRandomSelectIcon;
		_imageSelection.AddItem(id, null, isSelectable: true, () => true, Toolbox.ResourcesManager.GetAssetTypePath(id, type), Toolbox.ResourcesManager.GetAssetTypePath(id, type, isfetch: true), isDisplaySprite: false, string.Empty, null, delegate
		{
		}, delegate
		{
			UpdateAllButton();
		}, delegate(GameObject g)
		{
			StartCoroutine(PushedAnimation(g));
		});
	}

	private IEnumerator PushedAnimation(GameObject obj)
	{
		TweenScale scl = obj.GetComponent<TweenScale>();
		if ((bool)scl)
		{
			scl.PlayForward();
			while (Input.GetMouseButton(0))
			{
				yield return null;
			}
			scl.PlayReverse();
		}
	}

	private void OnClickAllOffButton()
	{
		if (_imageSelection.GetSelectedList().Count > 0)
		{
			_imageSelection.SelectCancelAll();
		}
		else
		{
			_imageSelection.SelectAll();
		}
		UpdateAllButton();
	}

	private void UpdateAllButton()
	{
		SystemText systemText = Data.SystemText;
		if (_imageSelection.GetSelectedList().Count > 0)
		{
			_labelButton.text = systemText.Get("Card_0259");
		}
		else
		{
			_labelButton.text = systemText.Get("Card_0260");
		}
	}
}
