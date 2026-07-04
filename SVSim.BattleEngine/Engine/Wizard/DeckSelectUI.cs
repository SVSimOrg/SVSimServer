using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.DeckSelect.FirstDisplayPageIndexGetter;

namespace Wizard;

public class DeckSelectUI : MonoBehaviour
{
	public class InitOptions
	{
	}

	private enum ChangeMoveDirection
	{
		NONE}

	public class PageData
	{
		public Format Format { get; private set; }

		public DeckAttributeType AttributeType { get; private set; }

		public string GroupName { get; private set; }

		public List<DeckUI.DeckViewData> DeckViewList { get; private set; }

		private PageData(List<DeckUI.DeckViewData> deckViewList, Format format, DeckAttributeType attributeType, string groupName)
		{
			DeckViewList = deckViewList;
			Format = format;
			AttributeType = attributeType;
			GroupName = groupName;
		}
	}

	private class DeckTable
	{
		private List<DeckUI> _deckUIList = new List<DeckUI>();

		private Action<DeckUI> _onUpdateDeckUICustomize;

		public GameObject Obj { get; private set; }

		public DeckTable(UIGrid uiGrid, DeckUI originalDeckUI, Action<DeckUI> onClick, Action<Vector2> onDrag, Action<DeckUI> onUpdateDeckUICustomize)
		{
			_onUpdateDeckUICustomize = onUpdateDeckUICustomize;
			for (int i = 0; i < 9; i++)
			{
				DeckUI component = NGUITools.AddChild(uiGrid.gameObject, originalDeckUI.gameObject).GetComponent<DeckUI>();
				component.Initialize(onClick);
				UIEventListener uIEventListener = UIEventListener.Get(component.gameObject);
				uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, (UIEventListener.VectorDelegate)delegate(GameObject g, Vector2 v)
				{
					onDrag.Call(v);
				});
				_deckUIList.Add(component);
				component.gameObject.SetActive(value: false);
			}
			uiGrid.repositionNow = true;
			Obj = uiGrid.gameObject;
			Obj.SetActive(value: true);
		}
	}
}
