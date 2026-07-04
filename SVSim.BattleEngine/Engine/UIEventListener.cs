using UnityEngine;

[AddComponentMenu("NGUI/Internal/Event Listener")]
public class UIEventListener : MonoBehaviour
{
	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public VoidDelegate onClick;

	public VoidDelegate onDoubleClick;

	public BoolDelegate onHover;

	public BoolDelegate onPress;

	public BoolDelegate onSelect;

	public FloatDelegate onScroll;

	public VoidDelegate onDragStart;

	public VectorDelegate onDrag;

	public VoidDelegate onDragOver;

	public VoidDelegate onDragEnd;

	public BoolDelegate onTooltip;

	public BoolDelegate onPressRight;

	private bool isColliderEnabled
	{
		get
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = GetComponent<Collider2D>();
			if (component2 != null)
			{
				return component2.enabled;
			}
			return false;
		}
	}

	private void OnHover(bool isOver)
	{
		if (isColliderEnabled && onHover != null)
		{
			onHover(base.gameObject, isOver);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (isColliderEnabled && onPress != null)
		{
			onPress(base.gameObject, isPressed);
		}
	}

	private void OnSelect(bool selected)
	{
		if (isColliderEnabled && onSelect != null)
		{
			onSelect(base.gameObject, selected);
		}
	}

	private void OnTooltip(bool show)
	{
		if (isColliderEnabled && onTooltip != null)
		{
			onTooltip(base.gameObject, show);
		}
	}

	public static UIEventListener Get(GameObject go)
	{
		UIEventListener uIEventListener = go.GetComponent<UIEventListener>();
		if (uIEventListener == null)
		{
			uIEventListener = go.AddComponent<UIEventListener>();
		}
		return uIEventListener;
	}
}
