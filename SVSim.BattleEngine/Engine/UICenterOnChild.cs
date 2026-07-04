using System;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Child")]
public class UICenterOnChild : MonoBehaviour
{
	public delegate void OnCenterCallback(GameObject centeredObject);

	public float springStrength = 8f;

	public float nextPageThreshold;

	public SpringPanel.OnFinished onFinished;

	public OnCenterCallback onCenter;

	[SerializeField]
	private bool _isGrandChildTarget;

	[SerializeField]
	private string _targetObjectName;

	private UIScrollView mScrollView;

	private GameObject mCenteredObject;

	private void OnDragFinished()
	{
		if (base.enabled)
		{
			Recenter();
		}
	}

	[ContextMenu("Execute")]
	public void Recenter()
	{
		if (mScrollView == null)
		{
			mScrollView = NGUITools.FindInParents<UIScrollView>(base.gameObject);
			if (mScrollView == null)
			{
				base.enabled = false;
				return;
			}
			if ((bool)mScrollView)
			{
				mScrollView.centerOnChild = this;
				UIScrollView uIScrollView = mScrollView;
				uIScrollView.onDragFinished = (UIScrollView.OnDragNotification)Delegate.Combine(uIScrollView.onDragFinished, new UIScrollView.OnDragNotification(OnDragFinished));
			}
			if (mScrollView.horizontalScrollBar != null)
			{
				UIProgressBar horizontalScrollBar = mScrollView.horizontalScrollBar;
				horizontalScrollBar.onDragFinished = (UIProgressBar.OnDragFinished)Delegate.Combine(horizontalScrollBar.onDragFinished, new UIProgressBar.OnDragFinished(OnDragFinished));
			}
			if (mScrollView.verticalScrollBar != null)
			{
				UIProgressBar verticalScrollBar = mScrollView.verticalScrollBar;
				verticalScrollBar.onDragFinished = (UIProgressBar.OnDragFinished)Delegate.Combine(verticalScrollBar.onDragFinished, new UIProgressBar.OnDragFinished(OnDragFinished));
			}
		}
		if (mScrollView.panel == null)
		{
			return;
		}
		Transform transform = base.transform;
		if (transform.childCount == 0)
		{
			return;
		}
		Vector3[] worldCorners = mScrollView.panel.worldCorners;
		Vector3 vector = (worldCorners[2] + worldCorners[0]) * 0.5f;
		Vector3 velocity = mScrollView.currentMomentum * mScrollView.momentumAmount;
		Vector3 vector2 = NGUIMath.SpringDampen(ref velocity, 9f, 2f);
		Vector3 vector3 = vector - vector2 * 0.01f;
		float num = float.MaxValue;
		Transform target = null;
		int index = 0;
		int num2 = 0;
		UIGrid component = GetComponent<UIGrid>();
		List<Transform> list = null;
		if (component != null)
		{
			if (_isGrandChildTarget)
			{
				list = new List<Transform>();
				foreach (Transform child3 in component.GetChildList())
				{
					int childCount = child3.childCount;
					for (int i = 0; i < childCount; i++)
					{
						Transform child = child3.GetChild(i);
						if (IsTarget(child) || IsCenteringSupporterTarget(child))
						{
							list.Add(child3.GetChild(i));
						}
					}
				}
			}
			else
			{
				list = component.GetChildList();
			}
			int j = 0;
			int count = list.Count;
			int num3 = 0;
			for (; j < count; j++)
			{
				Transform transform2 = list[j];
				if (transform2.gameObject.activeInHierarchy)
				{
					float num4 = Vector3.SqrMagnitude(transform2.position - vector3);
					if (num4 < num)
					{
						num = num4;
						ChapterObjectCenteringSupporter component2 = transform2.GetComponent<ChapterObjectCenteringSupporter>();
						target = ((component2 != null && component2._targetTransform != null) ? component2._targetTransform : transform2);
						index = j;
						num2 = num3;
					}
					num3++;
				}
			}
		}
		else
		{
			int k = 0;
			int childCount2 = transform.childCount;
			int num5 = 0;
			for (; k < childCount2; k++)
			{
				Transform child2 = transform.GetChild(k);
				if (child2.gameObject.activeInHierarchy)
				{
					float num6 = Vector3.SqrMagnitude(child2.position - vector3);
					if (num6 < num)
					{
						num = num6;
						target = child2;
						index = k;
						num2 = num5;
					}
					num5++;
				}
			}
		}
		if (nextPageThreshold > 0f && UICamera.currentTouch != null && mCenteredObject != null && mCenteredObject.transform == ((list != null) ? list[index] : transform.GetChild(index)))
		{
			Vector3 vector4 = UICamera.currentTouch.totalDelta;
			vector4 = base.transform.rotation * vector4;
			float num7 = 0f;
			num7 = mScrollView.movement switch
			{
				UIScrollView.Movement.Horizontal => vector4.x, 
				UIScrollView.Movement.Vertical => vector4.y, 
				_ => vector4.magnitude, 
			};
			if (Mathf.Abs(num7) > nextPageThreshold)
			{
				if (num7 > nextPageThreshold)
				{
					target = ((list != null) ? ((num2 <= 0) ? ((GetComponent<UIWrapContent>() == null) ? list[0] : list[list.Count - 1]) : list[num2 - 1]) : ((num2 <= 0) ? ((GetComponent<UIWrapContent>() == null) ? transform.GetChild(0) : transform.GetChild(transform.childCount - 1)) : transform.GetChild(num2 - 1)));
				}
				else if (num7 < 0f - nextPageThreshold)
				{
					target = ((list != null) ? ((num2 >= list.Count - 1) ? ((GetComponent<UIWrapContent>() == null) ? list[list.Count - 1] : list[0]) : list[num2 + 1]) : ((num2 >= transform.childCount - 1) ? ((GetComponent<UIWrapContent>() == null) ? transform.GetChild(transform.childCount - 1) : transform.GetChild(0)) : transform.GetChild(num2 + 1)));
				}
			}
		}
		CenterOn(target, vector);
	}

	private bool IsTarget(Transform target)
	{
		if (string.IsNullOrEmpty(_targetObjectName))
		{
			return true;
		}
		Component[] components = target.gameObject.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].GetType().Name == _targetObjectName)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCenteringSupporterTarget(Transform target)
	{
		Component[] components = target.gameObject.GetComponents<Component>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i].GetType() == typeof(ChapterObjectCenteringSupporter))
			{
				return true;
			}
		}
		return false;
	}

	private void CenterOn(Transform target, Vector3 panelCenter)
	{
		if (target != null && mScrollView != null && mScrollView.panel != null)
		{
			Transform cachedTransform = mScrollView.panel.cachedTransform;
			mCenteredObject = target.gameObject;
			Vector3 vector = cachedTransform.InverseTransformPoint(target.position);
			Vector3 vector2 = cachedTransform.InverseTransformPoint(panelCenter);
			Vector3 vector3 = vector - vector2;
			if (!mScrollView.canMoveHorizontally)
			{
				vector3.x = 0f;
			}
			if (!mScrollView.canMoveVertically)
			{
				vector3.y = 0f;
			}
			vector3.z = 0f;
			SpringPanel.Begin(mScrollView.panel.cachedGameObject, cachedTransform.localPosition - vector3, springStrength).onFinished = onFinished;
		}
		else
		{
			mCenteredObject = null;
		}
		if (onCenter != null)
		{
			onCenter(mCenteredObject);
		}
	}

	public void CenterOn(Transform target)
	{
		if (mScrollView != null && mScrollView.panel != null)
		{
			Vector3[] worldCorners = mScrollView.panel.worldCorners;
			Vector3 panelCenter = (worldCorners[2] + worldCorners[0]) * 0.5f;
			CenterOn(target, panelCenter);
		}
	}
}
