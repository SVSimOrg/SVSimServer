using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Wrap Content")]
public class UIWrapContent : MonoBehaviour
{
	public delegate void OnInitializeItem(GameObject go, int wrapIndex, int realIndex);

	public int itemSize = 100;

	public bool cullContent = true;

	public int minIndex;

	public int maxIndex;

	public OnInitializeItem onInitializeItem;

	protected Transform mTrans;

	protected UIPanel mPanel;

	protected UIScrollView mScroll;

	protected bool mHorizontal;

	protected bool mFirstTime = true;

	protected List<Transform> mChildren = new List<Transform>();

	public bool EnableNoLimit { get; set; } = true;

	[ContextMenu("Sort Based on Scroll Movement")]
	public virtual void SortBasedOnScrollMovement()
	{
		if (CacheScrollView())
		{
			mChildren.Clear();
			for (int i = 0; i < mTrans.childCount; i++)
			{
				mChildren.Add(mTrans.GetChild(i));
			}
			if (mHorizontal)
			{
				mChildren.Sort(UIGrid.SortHorizontal);
			}
			else
			{
				mChildren.Sort(UIGrid.SortVertical);
			}
			ResetChildPositions();
		}
	}

	protected bool CacheScrollView()
	{
		mTrans = base.transform;
		mPanel = NGUITools.FindInParents<UIPanel>(base.gameObject);
		mScroll = mPanel.GetComponent<UIScrollView>();
		if (mScroll == null)
		{
			return false;
		}
		if (mScroll.movement == UIScrollView.Movement.Horizontal)
		{
			mHorizontal = true;
		}
		else
		{
			if (mScroll.movement != UIScrollView.Movement.Vertical)
			{
				return false;
			}
			mHorizontal = false;
		}
		return true;
	}

	protected virtual void ResetChildPositions()
	{
		int i = 0;
		for (int count = mChildren.Count; i < count; i++)
		{
			Transform transform = mChildren[i];
			transform.localPosition = (mHorizontal ? new Vector3(i * itemSize, 0f, 0f) : new Vector3(0f, -i * itemSize, 0f));
			UpdateItem(transform, i);
		}
	}

	public virtual bool WrapContent()
	{
		bool result = false;
		float num = (float)(itemSize * mChildren.Count) * 0.5f;
		Vector3[] worldCorners = mPanel.worldCorners;
		for (int i = 0; i < 4; i++)
		{
			Vector3 position = worldCorners[i];
			position = mTrans.InverseTransformPoint(position);
			worldCorners[i] = position;
		}
		Vector3 vector = Vector3.Lerp(worldCorners[0], worldCorners[2], 0.5f);
		bool flag = true;
		float num2 = num * 2f;
		if (mHorizontal)
		{
			float num3 = worldCorners[0].x - (float)itemSize;
			float num4 = worldCorners[2].x + (float)itemSize;
			float num5 = (worldCorners[2].x - worldCorners[0].x) * 0.5f - mPanel.clipSoftness.x;
			float num6 = num - num5;
			bool flag2 = 0f < num6 && num6 < (float)itemSize * 0.5f;
			int j = 0;
			for (int count = mChildren.Count; j < count; j++)
			{
				Transform transform = mChildren[j];
				float num7 = transform.localPosition.x - vector.x;
				if (num7 < 0f - num)
				{
					Vector3 localPosition = transform.localPosition;
					localPosition.x += num2;
					num7 = localPosition.x - vector.x;
					int num8 = Mathf.RoundToInt(localPosition.x / (float)itemSize);
					if ((EnableNoLimit && minIndex == maxIndex) || (minIndex <= num8 && num8 <= maxIndex))
					{
						transform.localPosition = localPosition;
						UpdateItem(transform, j);
					}
					else
					{
						flag = false;
					}
				}
				else if (num7 > num)
				{
					Vector3 localPosition2 = transform.localPosition;
					localPosition2.x -= num2;
					num7 = localPosition2.x - vector.x;
					int num9 = Mathf.RoundToInt(localPosition2.x / (float)itemSize);
					if ((EnableNoLimit && minIndex == maxIndex) || (minIndex <= num9 && num9 <= maxIndex))
					{
						transform.localPosition = localPosition2;
						UpdateItem(transform, j);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (mFirstTime)
					{
						UpdateItem(transform, j);
					}
					if (flag2)
					{
						float num10 = (float)itemSize * 0.5f - num6;
						if (num7 < 0f - (num - num10) || num7 > num - num10)
						{
							flag = false;
						}
					}
					result = true;
				}
				if (cullContent)
				{
					num7 += mPanel.clipOffset.x - mTrans.localPosition.x;
					if (!UICamera.IsPressed(transform.gameObject))
					{
						NGUITools.SetActive(transform.gameObject, num7 > num3 && num7 < num4, compatibilityMode: false);
					}
				}
			}
		}
		else
		{
			float num11 = worldCorners[0].y - (float)itemSize;
			float num12 = worldCorners[2].y + (float)itemSize;
			float num13 = (worldCorners[2].y - worldCorners[0].y) * 0.5f - mPanel.clipSoftness.y;
			float num14 = num - num13;
			bool flag3 = 0f < num14 && num14 < (float)itemSize * 0.5f;
			int k = 0;
			for (int count2 = mChildren.Count; k < count2; k++)
			{
				Transform transform2 = mChildren[k];
				float num15 = transform2.localPosition.y - vector.y;
				if (num15 < 0f - num)
				{
					Vector3 localPosition3 = transform2.localPosition;
					localPosition3.y += num2;
					num15 = localPosition3.y - vector.y;
					int num16 = Mathf.RoundToInt(localPosition3.y / (float)itemSize);
					if ((EnableNoLimit && minIndex == maxIndex) || (minIndex <= num16 && num16 <= maxIndex))
					{
						transform2.localPosition = localPosition3;
						UpdateItem(transform2, k);
					}
					else
					{
						flag = false;
					}
				}
				else if (num15 > num)
				{
					Vector3 localPosition4 = transform2.localPosition;
					localPosition4.y -= num2;
					num15 = localPosition4.y - vector.y;
					int num17 = Mathf.RoundToInt(localPosition4.y / (float)itemSize);
					if ((EnableNoLimit && minIndex == maxIndex) || (minIndex <= num17 && num17 <= maxIndex))
					{
						transform2.localPosition = localPosition4;
						UpdateItem(transform2, k);
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (mFirstTime)
					{
						UpdateItem(transform2, k);
					}
					if (flag3)
					{
						float num18 = (float)itemSize * 0.5f - num14;
						if (num15 < 0f - (num - num18) || num15 > num - num18)
						{
							flag = false;
						}
					}
					result = true;
				}
				if (cullContent)
				{
					num15 += mPanel.clipOffset.y - mTrans.localPosition.y;
					if (!UICamera.IsPressed(transform2.gameObject))
					{
						NGUITools.SetActive(transform2.gameObject, num15 > num11 && num15 < num12, compatibilityMode: false);
					}
				}
			}
		}
		mScroll.restrictWithinPanel = !flag;
		return result;
	}

	protected virtual void UpdateItem(Transform item, int index)
	{
		if (onInitializeItem != null)
		{
			int realIndex = ((mScroll.movement == UIScrollView.Movement.Vertical) ? Mathf.RoundToInt(item.localPosition.y / (float)itemSize) : Mathf.RoundToInt(item.localPosition.x / (float)itemSize));
			onInitializeItem(item.gameObject, index, realIndex);
		}
	}
}
