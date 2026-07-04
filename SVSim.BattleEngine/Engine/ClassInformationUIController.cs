using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class ClassInformationUIController
{
	private List<IClassInfomationUI> _classInformationUIList;

	public ClassInformationUIController(List<IClassInfomationUI> classInformationUIList)
	{
		_classInformationUIList = classInformationUIList;
	}

	public void SetUpEvent(BattlePlayerBase player)
	{
		for (int i = 0; i < _classInformationUIList.Count; i++)
		{
			if (_classInformationUIList[i] != null)
			{
				_classInformationUIList[i].SetUpEvent(player);
			}
		}
	}

	public void ShowInfomation(bool playEffect = true)
	{
		for (int i = 0; i < _classInformationUIList.Count; i++)
		{
			if (_classInformationUIList[i] != null)
			{
				_classInformationUIList[i].ShowInfomation(playEffect);
			}
		}
	}

	public void HideInfomation()
	{
		for (int i = 0; i < _classInformationUIList.Count; i++)
		{
			if (_classInformationUIList[i] != null)
			{
				_classInformationUIList[i].HideInfomation();
			}
		}
	}

	public void SetIsSelect(bool isSelect)
	{
		for (int i = 0; i < _classInformationUIList.Count; i++)
		{
			if (_classInformationUIList[i] != null)
			{
				_classInformationUIList[i].SetIsSelect(isSelect);
			}
		}
	}

	public void UpdateInfomation()
	{
	}
}
