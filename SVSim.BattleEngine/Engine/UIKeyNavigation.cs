using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Key Navigation")]
public class UIKeyNavigation : MonoBehaviour
{
	public enum Constraint
	{
		None,
		Vertical,
		Horizontal,
		Explicit
	}

	public static BetterList<UIKeyNavigation> list = new BetterList<UIKeyNavigation>();

	public Constraint constraint;

	public GameObject onUp;

	public GameObject onDown;

	public GameObject onLeft;

	public GameObject onRight;

	public GameObject onTab;

	public static int mLastFrame = 0;

	public bool isColliderEnabled
	{
		get
		{
			if (base.enabled && base.gameObject.activeInHierarchy)
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
			return false;
		}
	}

	private static bool IsActive(GameObject go)
	{
		if ((bool)go && go.activeInHierarchy)
		{
			Collider component = go.GetComponent<Collider>();
			if (component != null)
			{
				return component.enabled;
			}
			Collider2D component2 = go.GetComponent<Collider2D>();
			if (component2 != null)
			{
				return component2.enabled;
			}
			return false;
		}
		return false;
	}

	public GameObject GetLeft()
	{
		if (IsActive(onLeft))
		{
			return onLeft;
		}
		if (constraint == Constraint.Vertical || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.left, 1f, 2f);
	}

	public GameObject GetRight()
	{
		if (IsActive(onRight))
		{
			return onRight;
		}
		if (constraint == Constraint.Vertical || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.right, 1f, 2f);
	}

	public GameObject GetUp()
	{
		if (IsActive(onUp))
		{
			return onUp;
		}
		if (constraint == Constraint.Horizontal || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.up, 2f);
	}

	public GameObject GetDown()
	{
		if (IsActive(onDown))
		{
			return onDown;
		}
		if (constraint == Constraint.Horizontal || constraint == Constraint.Explicit)
		{
			return null;
		}
		return Get(Vector3.down, 2f);
	}

	public GameObject Get(Vector3 myDir, float x = 1f, float y = 1f)
	{
		Transform transform = base.transform;
		myDir = transform.TransformDirection(myDir);
		Vector3 center = GetCenter(base.gameObject);
		float num = float.MaxValue;
		GameObject result = null;
		for (int i = 0; i < list.size; i++)
		{
			UIKeyNavigation uIKeyNavigation = list[i];
			if (uIKeyNavigation == this || uIKeyNavigation.constraint == Constraint.Explicit || !uIKeyNavigation.isColliderEnabled)
			{
				continue;
			}
			UIWidget component = uIKeyNavigation.GetComponent<UIWidget>();
			if (component != null && component.alpha == 0f)
			{
				continue;
			}
			Vector3 direction = GetCenter(uIKeyNavigation.gameObject) - center;
			if (!(Vector3.Dot(myDir, direction.normalized) < 0.707f))
			{
				direction = transform.InverseTransformDirection(direction);
				direction.x *= x;
				direction.y *= y;
				float sqrMagnitude = direction.sqrMagnitude;
				if (!(sqrMagnitude > num))
				{
					result = uIKeyNavigation.gameObject;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	protected static Vector3 GetCenter(GameObject go)
	{
		UIWidget component = go.GetComponent<UIWidget>();
		UICamera uICamera = UICamera.FindCameraForLayer(go.layer);
		if (uICamera != null)
		{
			Vector3 position = go.transform.position;
			if (component != null)
			{
				Vector3[] worldCorners = component.worldCorners;
				position = (worldCorners[0] + worldCorners[2]) * 0.5f;
			}
			position = uICamera.cachedCamera.WorldToScreenPoint(position);
			position.z = 0f;
			return position;
		}
		if (component != null)
		{
			Vector3[] worldCorners2 = component.worldCorners;
			return (worldCorners2[0] + worldCorners2[2]) * 0.5f;
		}
		return go.transform.position;
	}

	public virtual void OnKey(KeyCode key)
	{
		if (UIPopupList.isOpen || mLastFrame == Time.frameCount)
		{
			return;
		}
		mLastFrame = Time.frameCount;
		if (key != KeyCode.Tab)
		{
			return;
		}
		GameObject gameObject = onTab;
		if (gameObject == null)
		{
			if (UICamera.GetKey(KeyCode.LeftShift) || UICamera.GetKey(KeyCode.RightShift))
			{
				gameObject = GetLeft();
				if (gameObject == null)
				{
					gameObject = GetUp();
				}
				if (gameObject == null)
				{
					gameObject = GetDown();
				}
				if (gameObject == null)
				{
					gameObject = GetRight();
				}
			}
			else
			{
				gameObject = GetRight();
				if (gameObject == null)
				{
					gameObject = GetDown();
				}
				if (gameObject == null)
				{
					gameObject = GetUp();
				}
				if (gameObject == null)
				{
					gameObject = GetLeft();
				}
			}
		}
		if (gameObject != null)
		{
			UICamera.currentScheme = UICamera.ControlScheme.Controller;
			UICamera.hoveredObject = gameObject;
			UIInput component = gameObject.GetComponent<UIInput>();
			if (component != null)
			{
				component.isSelected = true;
			}
		}
	}
}
