using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class MyRotationPeriodSelectDialog : MonoBehaviour
{
	private List<MyRotationInfo> _selectData;

	public MyRotationInfo SelectInfo { get; private set; }

	public static void Create(MyRotationInfo defaultSelectInfo, CardBasePrm.ClanType clanType, Action<MyRotationInfo> onDecide)
	{
		List<string> list = new List<string>();
		int num = 0;
		int num2 = 0;
		List<MyRotationInfo> list2 = new List<MyRotationInfo>();
		foreach (MyRotationInfo myRotationInfo in Data.MyRotationAllInfo.MyRotationInfoList)
		{
			if (clanType != CardBasePrm.ClanType.NEMESIS || myRotationInfo.IsEnableNemesis)
			{
				list2.Add(myRotationInfo);
				list.Add(myRotationInfo.PackSelectText);
				if (defaultSelectInfo != null && myRotationInfo.Id == defaultSelectInfo.Id)
				{
					num = num2;
				}
				num2++;
			}
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/layoutParts/MyRotation/MyRotationPeriodSelectDialog")) as GameObject;
		MyRotationPeriodSelectDialog selectDialog = gameObject.GetComponentInChildren<MyRotationPeriodSelectDialog>();
		selectDialog._selectData = list2;
		DialogBase dialog = DrumrollDialog.Create(gameObject.GetComponent<DrumrollDialog>(), list, num, selectDialog.OnSelect);
		dialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_GrayBtn);
		dialog.SetButtonText(Data.SystemText.Get("Common_0003"), Data.SystemText.Get("MyRotation_ID_11"));
		dialog.SetTitleLabel(Data.SystemText.Get("MyRotation_ID_23"));
		dialog.onPushButton1 = delegate
		{
			onDecide(selectDialog.SelectInfo);
		};
		dialog.onPushButton2 = delegate
		{
			OnClickAbilityDetailButton(dialog);
		};
		dialog.isNotCloseWindowButton2 = true;
		if (defaultSelectInfo == null)
		{
			dialog.ClickSe_Btn1 = 0;
		}
		selectDialog.OnSelect(num);
	}

	private static void OnClickAbilityDetailButton(DialogBase periodSelectDialog)
	{
		DialogBase dialogBase = MyRotationAbilityDetailDialog.Create(Data.MyRotationAllInfo.AbilityGroup);
		periodSelectDialog.SetActive(inActive: false);
		dialogBase.OnClose = delegate
		{
			periodSelectDialog.SetActive(inActive: true);
		};
	}

	private void OnSelect(int index)
	{
		SelectInfo = _selectData[index];
	}
}
