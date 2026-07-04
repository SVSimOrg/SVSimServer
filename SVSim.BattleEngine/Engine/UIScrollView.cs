using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Interaction/Scroll View")]
public class UIScrollView : MonoBehaviour
{
	public enum Movement
	{
		Horizontal,
		Vertical,
		Unrestricted,
		Custom
	}

	public enum DragEffect
	{
		None,
		MomentumAndSpring
	}

	public enum ShowCondition
	{
}

	public delegate void OnDragNotification();

	public Movement movement;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public bool restrictWithinPanel = true;

	public bool disableDragIfFits;

	public bool smoothDragStart = true;

	public float momentumAmount = 35f;

	public UIProgressBar horizontalScrollBar;

	public UIProgressBar verticalScrollBar;

	public Vector2 customMovement = new Vector2(1f, 0f);

	public UIWidget.Pivot contentPivot;

	public OnDragNotification onDragStarted;

	public OnDragNotification onDragFinished;

	public OnDragNotification onStoppedMoving;

	[HideInInspector]
	[SerializeField]
	private Vector3 scale = new Vector3(1f, 0f, 0f);

	[SerializeField]
	[HideInInspector]
	private Vector2 relativePositionOnReset = Vector2.zero;

	public bool considerSoftnessInShouldMove;

	protected Transform mTrans;

	protected UIPanel mPanel;

	protected Plane mPlane;

	protected Vector3 mLastPos;

	protected bool mPressed;

	protected Vector3 mMomentum = Vector3.zero;

	protected float mScroll;

	protected Bounds mBounds;

	protected bool mCalculatedBounds;

	protected bool mShouldMove;

	protected bool mIgnoreCallbacks;

	protected int mDragID = -10;

	protected Vector2 mDragStartOffset = Vector2.zero;

	protected bool mDragStarted;

	[HideInInspector]
	public UICenterOnChild centerOnChild;

	public UIPanel panel => mPanel;

	public virtual Bounds bounds
	{
		get
		{
			if (!mCalculatedBounds)
			{
				mCalculatedBounds = true;
				mTrans = base.transform;
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
			}
			return mBounds;
		}
	}

	public bool canMoveHorizontally
	{
		get
		{
			if (movement != Movement.Horizontal && movement != Movement.Unrestricted)
			{
				if (movement == Movement.Custom)
				{
					return customMovement.x != 0f;
				}
				return false;
			}
			return true;
		}
	}

	public bool canMoveVertically
	{
		get
		{
			if (movement != Movement.Vertical && movement != Movement.Unrestricted)
			{
				if (movement == Movement.Custom)
				{
					return customMovement.y != 0f;
				}
				return false;
			}
			return true;
		}
	}

	protected virtual bool shouldMove
	{
		get
		{
			if (!disableDragIfFits)
			{
				return true;
			}
			if (mPanel == null)
			{
				mPanel = GetComponent<UIPanel>();
			}
			Vector4 finalClipRegion = mPanel.finalClipRegion;
			Bounds bounds = this.bounds;
			float num = ((finalClipRegion.z == 0f) ? ((float)Screen.width) : (finalClipRegion.z * 0.5f));
			float num2 = ((finalClipRegion.w == 0f) ? ((float)Screen.height) : (finalClipRegion.w * 0.5f));
			if (considerSoftnessInShouldMove)
			{
				num -= mPanel.clipSoftness.x;
				num2 -= mPanel.clipSoftness.y;
			}
			if (canMoveHorizontally)
			{
				if (bounds.min.x < finalClipRegion.x - num)
				{
					return true;
				}
				if (bounds.max.x > finalClipRegion.x + num)
				{
					return true;
				}
			}
			if (canMoveVertically)
			{
				if (bounds.min.y < finalClipRegion.y - num2)
				{
					return true;
				}
				if (bounds.max.y > finalClipRegion.y + num2)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Vector3 currentMomentum
	{
		get
		{
			return mMomentum;
		}
		set
		{
			mMomentum = value;
			mShouldMove = !disableDragIfFits;
		}
	}

	private void Awake()
	{
		mTrans = base.transform;
		mPanel = GetComponent<UIPanel>();
		if (mPanel.clipping == UIDrawCall.Clipping.None)
		{
			mPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;
		}
		if (movement != Movement.Custom && scale.sqrMagnitude > 0.001f)
		{
			if (scale.x == 1f && scale.y == 0f)
			{
				movement = Movement.Horizontal;
			}
			else if (scale.x == 0f && scale.y == 1f)
			{
				movement = Movement.Vertical;
			}
			else if (scale.x == 1f && scale.y == 1f)
			{
				movement = Movement.Unrestricted;
			}
			else
			{
				movement = Movement.Custom;
				customMovement.x = scale.x;
				customMovement.y = scale.y;
			}
			scale = Vector3.zero;
		}
		if (contentPivot == UIWidget.Pivot.TopLeft && relativePositionOnReset != Vector2.zero)
		{
			contentPivot = NGUIMath.GetPivot(new Vector2(relativePositionOnReset.x, 1f - relativePositionOnReset.y));
			relativePositionOnReset = Vector2.zero;
		}
	}

	public bool RestrictWithinBounds(bool instant)
	{
		return RestrictWithinBounds(instant, horizontal: true, vertical: true);
	}

	public bool RestrictWithinBounds(bool instant, bool horizontal, bool vertical)
	{
		if (mPanel == null)
		{
			return false;
		}
		Bounds bounds = this.bounds;
		Vector3 vector = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);
		if (!horizontal)
		{
			vector.x = 0f;
		}
		if (!vertical)
		{
			vector.y = 0f;
		}
		if (vector.sqrMagnitude > 0.1f)
		{
			if (!instant && dragEffect == DragEffect.MomentumAndSpring)
			{
				Vector3 pos = mTrans.localPosition + vector;
				pos.x = Mathf.Round(pos.x);
				pos.y = Mathf.Round(pos.y);
				SpringPanel.Begin(mPanel.gameObject, pos, 13f).strength = 8f;
			}
			else
			{
				MoveRelative(vector);
				if (Mathf.Abs(vector.x) > 0.01f)
				{
					mMomentum.x = 0f;
				}
				if (Mathf.Abs(vector.y) > 0.01f)
				{
					mMomentum.y = 0f;
				}
				if (Mathf.Abs(vector.z) > 0.01f)
				{
					mMomentum.z = 0f;
				}
				mScroll = 0f;
			}
			return true;
		}
		return false;
	}

	public void DisableSpring()
	{
		SpringPanel component = GetComponent<SpringPanel>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public void UpdateScrollbars()
	{
		UpdateScrollbars(recalculateBounds: true);
	}

	public virtual void UpdateScrollbars(bool recalculateBounds)
	{
		if (mPanel == null)
		{
			return;
		}
		if (horizontalScrollBar != null || verticalScrollBar != null)
		{
			if (recalculateBounds)
			{
				mCalculatedBounds = false;
				mShouldMove = shouldMove;
			}
			Bounds bounds = this.bounds;
			Vector2 vector = bounds.min;
			Vector2 vector2 = bounds.max;
			if (horizontalScrollBar != null && vector2.x > vector.x)
			{
				Vector4 finalClipRegion = mPanel.finalClipRegion;
				int num = Mathf.RoundToInt(finalClipRegion.z);
				if ((num & 1) != 0)
				{
					num--;
				}
				float f = (float)num * 0.5f;
				f = Mathf.Round(f);
				if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
				{
					f -= mPanel.clipSoftness.x;
				}
				float contentSize = vector2.x - vector.x;
				float viewSize = f * 2f;
				float x = vector.x;
				float x2 = vector2.x;
				float num2 = finalClipRegion.x - f;
				float num3 = finalClipRegion.x + f;
				x = num2 - x;
				x2 -= num3;
				UpdateScrollbars(horizontalScrollBar, x, x2, contentSize, viewSize, inverted: false);
			}
			if (verticalScrollBar != null && vector2.y > vector.y)
			{
				Vector4 finalClipRegion2 = mPanel.finalClipRegion;
				int num4 = Mathf.RoundToInt(finalClipRegion2.w);
				if ((num4 & 1) != 0)
				{
					num4--;
				}
				float f2 = (float)num4 * 0.5f;
				f2 = Mathf.Round(f2);
				if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
				{
					f2 -= mPanel.clipSoftness.y;
				}
				float contentSize2 = vector2.y - vector.y;
				float viewSize2 = f2 * 2f;
				float y = vector.y;
				float y2 = vector2.y;
				float num5 = finalClipRegion2.y - f2;
				float num6 = finalClipRegion2.y + f2;
				y = num5 - y;
				y2 -= num6;
				UpdateScrollbars(verticalScrollBar, y, y2, contentSize2, viewSize2, inverted: true);
			}
		}
		else if (recalculateBounds)
		{
			mCalculatedBounds = false;
		}
	}

	protected void UpdateScrollbars(UIProgressBar slider, float contentMin, float contentMax, float contentSize, float viewSize, bool inverted)
	{
		if (slider == null)
		{
			return;
		}
		mIgnoreCallbacks = true;
		float num;
		if (viewSize < contentSize)
		{
			contentMin = Mathf.Clamp01(contentMin / contentSize);
			contentMax = Mathf.Clamp01(contentMax / contentSize);
			num = contentMin + contentMax;
			slider.value = ((!inverted) ? ((num > 0.001f) ? (contentMin / num) : 1f) : ((num > 0.001f) ? (1f - contentMin / num) : 0f));
		}
		else
		{
			contentMin = Mathf.Clamp01((0f - contentMin) / contentSize);
			contentMax = Mathf.Clamp01((0f - contentMax) / contentSize);
			num = contentMin + contentMax;
			slider.value = ((!inverted) ? ((num > 0.001f) ? (contentMin / num) : 1f) : ((num > 0.001f) ? (1f - contentMin / num) : 0f));
			if (contentSize > 0f)
			{
				contentMin = Mathf.Clamp01(contentMin / contentSize);
				contentMax = Mathf.Clamp01(contentMax / contentSize);
				num = contentMin + contentMax;
			}
		}
		UIScrollBar uIScrollBar = slider as UIScrollBar;
		if (uIScrollBar != null)
		{
			uIScrollBar.barSize = 1f - num;
		}
		mIgnoreCallbacks = false;
	}

	public virtual void SetDragAmount(float x, float y, bool updateScrollbars)
	{
		if (mPanel == null)
		{
			mPanel = GetComponent<UIPanel>();
		}
		DisableSpring();
		Bounds bounds = this.bounds;
		if (bounds.min.x == bounds.max.x || bounds.min.y == bounds.max.y)
		{
			return;
		}
		Vector4 finalClipRegion = mPanel.finalClipRegion;
		float num = finalClipRegion.z * 0.5f;
		float num2 = finalClipRegion.w * 0.5f;
		float num3 = bounds.min.x + num;
		float num4 = bounds.max.x - num;
		float num5 = bounds.min.y + num2;
		float num6 = bounds.max.y - num2;
		if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			num3 -= mPanel.clipSoftness.x;
			num4 += mPanel.clipSoftness.x;
			num5 -= mPanel.clipSoftness.y;
			num6 += mPanel.clipSoftness.y;
		}
		float num7 = Mathf.Lerp(num3, num4, x);
		float num8 = Mathf.Lerp(num6, num5, y);
		if (!updateScrollbars)
		{
			Vector3 localPosition = mTrans.localPosition;
			if (canMoveHorizontally)
			{
				localPosition.x += finalClipRegion.x - num7;
			}
			if (canMoveVertically)
			{
				localPosition.y += finalClipRegion.y - num8;
			}
			mTrans.localPosition = localPosition;
		}
		if (canMoveHorizontally)
		{
			finalClipRegion.x = num7;
		}
		if (canMoveVertically)
		{
			finalClipRegion.y = num8;
		}
		Vector4 baseClipRegion = mPanel.baseClipRegion;
		mPanel.clipOffset = new Vector2(finalClipRegion.x - baseClipRegion.x, finalClipRegion.y - baseClipRegion.y);
		if (updateScrollbars)
		{
			UpdateScrollbars(mDragID == -10);
		}
	}

	[ContextMenu("Reset Clipping Position")]
	public void ResetPosition()
	{
		if (NGUITools.GetActive(this))
		{
			mScroll = 0f;
			mCalculatedBounds = false;
			Vector2 pivotOffset = NGUIMath.GetPivotOffset(contentPivot);
			SetDragAmount(pivotOffset.x, 1f - pivotOffset.y, updateScrollbars: false);
			SetDragAmount(pivotOffset.x, 1f - pivotOffset.y, updateScrollbars: true);
		}
	}

	public virtual void MoveRelative(Vector3 relative)
	{
		mTrans.localPosition += relative;
		Vector2 clipOffset = mPanel.clipOffset;
		clipOffset.x -= relative.x;
		clipOffset.y -= relative.y;
		mPanel.clipOffset = clipOffset;
		UpdateScrollbars(recalculateBounds: false);
	}

	public void Press(bool pressed)
	{
		if (UICamera.currentScheme == UICamera.ControlScheme.Controller)
		{
			return;
		}
		if (smoothDragStart && pressed)
		{
			mDragStarted = false;
			mDragStartOffset = Vector2.zero;
		}
		if (!base.enabled || !NGUITools.GetActive(base.gameObject))
		{
			return;
		}
		if (!pressed && mDragID == UICamera.currentTouchID)
		{
			mDragID = -10;
		}
		mCalculatedBounds = false;
		mShouldMove = shouldMove;
		if (!mShouldMove)
		{
			return;
		}
		mPressed = pressed;
		if (pressed)
		{
			mMomentum = Vector3.zero;
			mScroll = 0f;
			DisableSpring();
			mLastPos = UICamera.lastWorldPosition;
			mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);
			Vector2 clipOffset = mPanel.clipOffset;
			clipOffset.x = Mathf.Round(clipOffset.x);
			clipOffset.y = Mathf.Round(clipOffset.y);
			mPanel.clipOffset = clipOffset;
			Vector3 localPosition = mTrans.localPosition;
			localPosition.x = Mathf.Round(localPosition.x);
			localPosition.y = Mathf.Round(localPosition.y);
			mTrans.localPosition = localPosition;
			if (!smoothDragStart)
			{
				mDragStarted = true;
				mDragStartOffset = Vector2.zero;
				if (onDragStarted != null)
				{
					onDragStarted();
				}
			}
		}
		else if ((bool)centerOnChild)
		{
			centerOnChild.Recenter();
		}
		else
		{
			if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None)
			{
				RestrictWithinBounds(dragEffect == DragEffect.None, canMoveHorizontally, canMoveVertically);
			}
			if (mDragStarted && onDragFinished != null)
			{
				onDragFinished();
			}
			if (!mShouldMove && onStoppedMoving != null)
			{
				onStoppedMoving();
			}
		}
	}
}
