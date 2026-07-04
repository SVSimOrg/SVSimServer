using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class MyRotationAbilityDetailDialog : MonoBehaviour
{
	[SerializeField]
	private FlexibleGrid _grid;

	[SerializeField]
	private MyRotationAbilityDetailDialogItem _itemOriginal;

	[SerializeField]
	private GameObject _lineOriginal;

	public static DialogBase Create(List<MyRotationAbilityGroup> abilityGroupList)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("MyRotation_ID_11"));
		GameObject gameObject = Object.Instantiate(Resources.Load("UI/layoutParts/MyRotation/MyRotationAbilityDetailDialog")) as GameObject;
		dialogBase.SetObj(gameObject);
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.CloseBtn);
		gameObject.GetComponent<MyRotationAbilityDetailDialog>().Initialize(abilityGroupList);
		return dialogBase;
	}

	private void Initialize(List<MyRotationAbilityGroup> abilityGroupList)
	{
		_itemOriginal.gameObject.SetActive(value: false);
		_lineOriginal.SetActive(value: false);
		bool flag = true;
		foreach (MyRotationAbilityGroup abilityGroup in abilityGroupList)
		{
			if (abilityGroup.AbilityList.Count != 0)
			{
				if (!flag)
				{
					NGUITools.AddChild(_grid.gameObject, _lineOriginal).SetActive(value: true);
				}
				GameObject obj = NGUITools.AddChild(_grid.gameObject, _itemOriginal.gameObject);
				obj.SetActive(value: true);
				obj.GetComponent<MyRotationAbilityDetailDialogItem>().Initialize(abilityGroup);
				flag = false;
			}
		}
		_grid.Reposition();
	}
}
