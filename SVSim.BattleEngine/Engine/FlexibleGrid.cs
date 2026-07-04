using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class FlexibleGrid : MonoBehaviour
{
	public enum Order
	{
		Horizontal	}

	public enum PivotOption
	{
		NONE,
		CENTER
	}

	[SerializeField]
	private Vector2 _limitSize;

	[SerializeField]
	private Vector2 _padding;

	[SerializeField]
	private bool _hideInactive = true;

	[SerializeField]
	private Order _order;

	[SerializeField]
	private bool _reversel;

	[SerializeField]
	private PivotOption _horizonPivotOption;

	private bool _repositionCalled;

	[ContextMenu("Execute")]
	public void Reposition()
	{
		_repositionCalled = true;
		List<Transform> childList = GetChildList();
		if (childList.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		int i = 0;
		for (int count = childList.Count; i < count; i++)
		{
			Transform transform = childList[i];
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform, !_hideInactive);
			Vector3 localScale = transform.localScale;
			bounds.min = Vector3.Scale(bounds.min, localScale);
			bounds.max = Vector3.Scale(bounds.max, localScale);
			if (bounds.min == bounds.max)
			{
				continue;
			}
			if (_order == Order.Horizontal)
			{
				int num4 = ((!_reversel) ? 1 : (-1));
				num = Math.Max(num, bounds.size.y);
				if (num2 + bounds.size.x > _limitSize.x)
				{
					num2 = 0f;
					num3 -= num + _padding.y;
					num = 0f;
				}
				Vector3 localPosition = transform.localPosition;
				if (_horizonPivotOption == PivotOption.CENTER)
				{
					if (i == 0)
					{
						localPosition.x = num2;
					}
					else
					{
						float num5 = (float)num4 * bounds.size.x * 0.5f;
						localPosition.x = num2 + num5;
					}
				}
				else
				{
					localPosition.x = num2 + (float)num4 * (bounds.extents.x - bounds.center.x);
				}
				localPosition.y = num3;
				transform.localPosition = localPosition;
				num2 = ((_horizonPivotOption != PivotOption.CENTER) ? (num2 + (float)num4 * (bounds.size.x + _padding.x)) : ((i != 0) ? (num2 + (float)num4 * (_padding.x + bounds.size.x)) : (num2 + (float)num4 * (_padding.x + bounds.size.x * 0.5f))));
			}
			else
			{
				num = Math.Max(num, bounds.size.x);
				if (num3 + bounds.size.y > _limitSize.y)
				{
					num3 = 0f;
					num2 += num + _padding.x;
					num = 0f;
				}
				Vector3 localPosition2 = transform.localPosition;
				localPosition2.x = num2;
				localPosition2.y = num3 - bounds.extents.y - bounds.center.y;
				transform.localPosition = localPosition2;
				num3 -= bounds.size.y + _padding.y;
			}
		}
	}

	private List<Transform> GetChildList()
	{
		Transform transform = base.transform;
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!_hideInactive || ((bool)child && NGUITools.GetActive(child.gameObject)))
			{
				list.Add(child);
			}
		}
		return list;
	}

	public IEnumerator RepositionNextFrame()
	{
		yield return null;
		Reposition();
	}
}
