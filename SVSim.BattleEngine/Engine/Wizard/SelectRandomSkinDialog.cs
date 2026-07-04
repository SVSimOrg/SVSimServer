using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class SelectRandomSkinDialog : MonoBehaviour
{

	[SerializeField]
	private UIGrid _itemGrid;

	[SerializeField]
	private GameObject _skinButtonItemOriginal;

	[SerializeField]
	private UIButton _btnAllOff;

	[SerializeField]
	private UILabel _labelButton;

	[SerializeField]
	private UIButton _btnPrevPage;

	[SerializeField]
	private UIButton _btnNextPage;

	[SerializeField]
	private BoxCollider _flickCollider;

	[SerializeField]
	private UIPageIndicator _indicator;

	private int _currentPageIndex;

	private int _lastPageIndex = 1;

	private bool _flickStart;

	protected List<string> _loadedResourceList = new List<string>();

	private DialogBase _dialog;

	private List<int> _usableSkinIdList;

	private List<int> _selecteSkinIdList;

	private List<SelectRandomSkinButton> _skinButtonList;

	private bool IsFirstPage => _currentPageIndex <= 1;

	private bool IsLastPage => _currentPageIndex >= _lastPageIndex;

	public static DialogBase Create(List<int> usableSkinIdList, List<int> selectedSkinIdList, Action<List<int>> onClickOk)
	{
		SelectRandomSkinDialog component = (UnityEngine.Object.Instantiate(Resources.Load("UI/layoutParts/Dialog/SelectRandomSkinDialog")) as GameObject).GetComponent<SelectRandomSkinDialog>();
		component.CreateDialog(onClickOk);
		component.Initialize(usableSkinIdList, selectedSkinIdList);
		return component._dialog;
	}

	private void CreateDialog(Action<List<int>> onClickOk)
	{
		_dialog = UIManager.GetInstance().CreateDialogClose();
		_dialog.SetSize(DialogBase.Size.XL);
		_dialog.SetTitleLabel(Data.SystemText.Get("Card_0258"));
		_dialog.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		_dialog.SetButtonDelegate(delegate
		{
			_selecteSkinIdList.Sort();
			onClickOk.Call(_selecteSkinIdList);
		});
		_dialog.SetObj(base.gameObject);
	}

	private void Initialize(List<int> usableSkinIdList, List<int> selectedSkinIdList)
	{
		UIManager.GetInstance().createInSceneCenterLoading();
		StartCoroutine(LoadResources(usableSkinIdList, delegate
		{
			GenerateSkinButtons(usableSkinIdList, selectedSkinIdList);
			InitializePage(usableSkinIdList);
			ShowPage(1);
			UIManager.GetInstance().closeInSceneCenterLoading();
		}));
	}

	private IEnumerator LoadResources(List<int> usableSkinIdList, Action onFinish)
	{
		List<string> skinButtonPathList = new List<string>();
		for (int i = 0; i < usableSkinIdList.Count; i++)
		{
			string path = usableSkinIdList[i].ToString();
			string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaButton);
			skinButtonPathList.Add(assetTypePath);
		}
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(skinButtonPathList, null));
		_loadedResourceList.AddRange(skinButtonPathList);
		onFinish.Call();
	}

	private void GenerateSkinButtons(List<int> usableSkinIdList, List<int> selectedSkinIdList)
	{
		_usableSkinIdList = usableSkinIdList;
		_selecteSkinIdList = new List<int>(selectedSkinIdList);
		_skinButtonList = new List<SelectRandomSkinButton>();
		List<int> list = new List<int>();
		selectedSkinIdList.Sort();
		list.AddRange(selectedSkinIdList);
		List<int> list2 = usableSkinIdList.Except(selectedSkinIdList).ToList();
		list2.Sort();
		list.AddRange(list2);
		for (int i = 0; i < list.Count; i++)
		{
			SelectRandomSkinButton component = NGUITools.AddChild(_itemGrid.gameObject, _skinButtonItemOriginal.gameObject).GetComponent<SelectRandomSkinButton>();
			int skinId = list[i];
			component.Initialize(skinId, _selecteSkinIdList.Contains(skinId), delegate(int id, bool status)
			{
				if (status)
				{
					_selecteSkinIdList.Add(skinId);
				}
				else
				{
					_selecteSkinIdList.Remove(skinId);
				}
				UpdateAllButton();
			}, OnDragStart, OnDrag);
			_skinButtonList.Add(component);
		}
	}

	private void InitializePage(List<int> usableSkinList)
	{
		_lastPageIndex = (usableSkinList.Count - 1) / 21 + 1;
		_currentPageIndex = 1;
		_indicator.Init(_lastPageIndex);
		if (_lastPageIndex > 1)
		{
			_flickCollider.gameObject.SetActive(value: true);
			UIEventListener uIEventListener = UIEventListener.Get(_flickCollider.gameObject);
			uIEventListener.onDragStart = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDragStart, new UIEventListener.VoidDelegate(OnDragStart));
			UIEventListener uIEventListener2 = UIEventListener.Get(_flickCollider.gameObject);
			uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(OnDrag));
			_btnPrevPage.gameObject.SetActive(value: true);
			_btnNextPage.gameObject.SetActive(value: true);
			UIEventListener uIEventListener3 = UIEventListener.Get(_btnPrevPage.gameObject);
			uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
			{
				ShowPrevPage();
			});
			UIEventListener uIEventListener4 = UIEventListener.Get(_btnNextPage.gameObject);
			uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, (UIEventListener.VoidDelegate)delegate
			{
				ShowNextPage();
			});
		}
		_btnAllOff.onClick.Clear();
		_btnAllOff.onClick.Add(new EventDelegate(delegate
		{

			OnClickAllOffButton();
		}));
		UpdateAllButton();
	}

	private void UpdateAllButton()
	{
		SystemText systemText = Data.SystemText;
		if (_selecteSkinIdList.Count > 0)
		{
			_labelButton.text = systemText.Get("Card_0259");
			UIManager.SetObjectToGrey(_dialog.button1.gameObject, b: false);
		}
		else
		{
			_labelButton.text = systemText.Get("Card_0260");
			UIManager.SetObjectToGrey(_dialog.button1.gameObject, b: true);
		}
	}

	private void OnClickAllOffButton()
	{
		if (_selecteSkinIdList.Count > 0)
		{
			_selecteSkinIdList.Clear();
			for (int i = 0; i < _skinButtonList.Count; i++)
			{
				_skinButtonList[i].SetSelectStatus(isSelect: false);
			}
		}
		else
		{
			_selecteSkinIdList.AddRange(_usableSkinIdList);
			for (int j = 0; j < _skinButtonList.Count; j++)
			{
				_skinButtonList[j].SetSelectStatus(isSelect: true);
			}
		}
		UpdateAllButton();
	}

	private void ShowPage(int pageIndex)
	{
		_currentPageIndex = pageIndex;
		for (int i = 0; i < _skinButtonList.Count; i++)
		{
			_skinButtonList[i].gameObject.SetActive(value: false);
		}
		int num = (_currentPageIndex - 1) * 21;
		int num2 = Mathf.Min(num + 21, _skinButtonList.Count);
		for (int j = num; j < num2; j++)
		{
			_skinButtonList[j].gameObject.SetActive(value: true);
		}
		_itemGrid.Reposition();
		_btnPrevPage.gameObject.SetActive(!IsFirstPage);
		_btnNextPage.gameObject.SetActive(!IsLastPage);
		_indicator.UpdateIndicator(_currentPageIndex);
	}

	private void ShowPrevPage()
	{
		if (!IsFirstPage)
		{

			ShowPage(_currentPageIndex - 1);
		}
	}

	private void ShowNextPage()
	{
		if (!IsLastPage)
		{

			ShowPage(_currentPageIndex + 1);
		}
	}

	private void OnDragStart(GameObject obj)
	{
		_flickStart = true;
	}

	private void OnDrag(GameObject obj, Vector2 dir)
	{
		if (_flickStart)
		{
			if (_lastPageIndex <= 1)
			{
				_flickStart = false;
			}
			else if (dir.x >= 70f)
			{
				_flickStart = false;
				ShowPrevPage();
			}
			else if (dir.x <= -70f)
			{
				_flickStart = false;
				ShowNextPage();
			}
		}
	}
}
