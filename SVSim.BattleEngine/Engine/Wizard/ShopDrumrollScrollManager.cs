using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wizard;

public class ShopDrumrollScrollManager : MonoBehaviour
{
	public class DrumrollItem
	{
		public Texture LogoImage { get; private set; }

		public bool IsNew { get; private set; }

		public bool IsPrerelease { get; private set; }

		public DrumrollItem(Texture logoImage, bool isNew = false, bool isPrerelease = false)
		{
			LogoImage = logoImage;
			IsNew = isNew;
			IsPrerelease = isPrerelease;
		}
	}

	[SerializeField]
	private DrumrollScrollManager _drumrollScrollManagaer;

	[SerializeField]
	private UITexture _itemOriginal;

	public List<GameObject> ItemList { get; private set; } = new List<GameObject>();

	public IEnumerator CreateDrumrollScroll_Coroutine(List<DrumrollItem> itemList, int defaultIndex, Action<int> selectCallback, Action callBack = null)
	{
		foreach (GameObject item in ItemList)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		ItemList.Clear();
		foreach (DrumrollItem item2 in itemList)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_itemOriginal.gameObject);
			ItemList.Add(gameObject);
			gameObject.GetComponent<UITexture>().mainTexture = item2.LogoImage;
			ShopDrumrollScrollItem component = gameObject.GetComponent<ShopDrumrollScrollItem>();
			component.SetActivePrereleaseMark(item2.IsPrerelease);
			component.SetActiveNewMark(item2.IsNew);
		}
		_itemOriginal.gameObject.SetActive(value: false);
		yield return StartCoroutine(_drumrollScrollManagaer.CreateDrumrollScroll_Coroutine(ItemList, defaultIndex, selectCallback, callBack));
	}
}
