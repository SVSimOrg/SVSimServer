using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Popup List")]
public class UIPopupList : UIWidgetContainer
{
	public enum Position
	{
}

	public enum OpenOn
	{
	}

	public delegate void LegacyEvent(string val);

	public static UIPopupList current;

	private static GameObject mChild;

	private static float mFadeOutComplete;

	public Position position;

	public List<string> items = new List<string>();

	public List<object> itemData = new List<object>();

	public bool isAnimated = true;

	public List<EventDelegate> onChange = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	protected string mSelectedItem;

	[HideInInspector]
	[SerializeField]
	protected UISprite mBackground;

	[HideInInspector]
	[SerializeField]
	protected UISprite mHighlight;

	[HideInInspector]
	[SerializeField]
	protected List<UILabel> mLabelList = new List<UILabel>();

	[NonSerialized]
	protected GameObject mSelection;

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string functionName = "OnSelectionChange";

	private LegacyEvent mLegacyEvent;

	[NonSerialized]
	protected bool mExecuting;

	public GameObject source;

	public static bool isOpen
	{
		get
		{
			if (current != null)
			{
				if (!(mChild != null))
				{
					return mFadeOutComplete > Time.unscaledTime;
				}
				return true;
			}
			return false;
		}
	}

	public virtual string value
	{
		get
		{
			return mSelectedItem;
		}
		set
		{
			mSelectedItem = value;
			if (mSelectedItem != null && mSelectedItem != null)
			{
				TriggerCallbacks();
			}
		}
	}

	public virtual object data
	{
		get
		{
			int num = items.IndexOf(mSelectedItem);
			if (num <= -1 || num >= itemData.Count)
			{
				return null;
			}
			return itemData[num];
		}
	}

	protected void TriggerCallbacks()
	{
		if (!mExecuting)
		{
			mExecuting = true;
			UIPopupList uIPopupList = current;
			current = this;
			if (mLegacyEvent != null)
			{
				mLegacyEvent(mSelectedItem);
			}
			if (EventDelegate.IsValid(onChange))
			{
				EventDelegate.Execute(onChange);
			}
			else if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
			{
				eventReceiver.SendMessage(functionName, mSelectedItem, SendMessageOptions.DontRequireReceiver);
			}
			current = uIPopupList;
			mExecuting = false;
		}
	}

	protected virtual void OnSelect(bool isSelected)
	{
		if (!isSelected)
		{
			CloseSelf();
		}
	}

	public static void Close()
	{
		if (current != null)
		{
			current.CloseSelf();
			current = null;
		}
	}

	public virtual void CloseSelf()
	{
		if (!(mChild != null) || !(current == this))
		{
			return;
		}
		StopCoroutine("CloseIfUnselected");
		mSelection = null;
		mLabelList.Clear();
		if (isAnimated)
		{
			UIWidget[] componentsInChildren = mChild.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				UIWidget obj = componentsInChildren[i];
				Color color = obj.color;
				color.a = 0f;
				TweenColor.Begin(obj.gameObject, 0.15f, color).method = UITweener.Method.EaseOut;
			}
			Collider[] componentsInChildren2 = mChild.GetComponentsInChildren<Collider>();
			int j = 0;
			for (int num2 = componentsInChildren2.Length; j < num2; j++)
			{
				componentsInChildren2[j].enabled = false;
			}
			UnityEngine.Object.Destroy(mChild, 0.15f);
			mFadeOutComplete = Time.unscaledTime + Mathf.Max(0.1f, 0.15f);
		}
		else
		{
			UnityEngine.Object.Destroy(mChild);
			mFadeOutComplete = Time.unscaledTime + 0.1f;
		}
		mBackground = null;
		mHighlight = null;
		mChild = null;
		current = null;
	}

	private IEnumerator CloseIfUnselected()
	{
		do
		{
			yield return null;
		}
		while (!(UICamera.selectedObject != mSelection));
		CloseSelf();
	}
}
