using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class NGUITools
{

	private static Vector3[] mSides = new Vector3[4];

	public static Vector2 screenSize => new Vector2(Screen.width, Screen.height);

	public static string GetHierarchy(GameObject obj)
	{
		if (obj == null)
		{
			return "";
		}
		string text = obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			text = obj.name + "\\" + text;
		}
		return text;
	}

	public static T[] FindActive<T>() where T : Component
	{
		return UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
	}

	public static Camera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		Camera cachedCamera;
		for (int i = 0; i < UICamera.list.size; i++)
		{
			cachedCamera = UICamera.list.buffer[i].cachedCamera;
			if ((bool)cachedCamera && (cachedCamera.cullingMask & num) != 0)
			{
				return cachedCamera;
			}
		}
		cachedCamera = Camera.main;
		if ((bool)cachedCamera && (cachedCamera.cullingMask & num) != 0)
		{
			return cachedCamera;
		}
		Camera[] array = new Camera[Camera.allCamerasCount];
		int allCameras = Camera.GetAllCameras(array);
		for (int j = 0; j < allCameras; j++)
		{
			cachedCamera = array[j];
			if ((bool)cachedCamera && cachedCamera.enabled && (cachedCamera.cullingMask & num) != 0)
			{
				return cachedCamera;
			}
		}
		return null;
	}

	public static void UpdateWidgetCollider(GameObject go)
	{
		UpdateWidgetCollider(go, considerInactive: false);
	}

	public static void UpdateWidgetCollider(GameObject go, bool considerInactive)
	{
		if (!(go != null))
		{
			return;
		}
		BoxCollider component = go.GetComponent<BoxCollider>();
		if (component != null)
		{
			UpdateWidgetCollider(component, considerInactive);
			return;
		}
		BoxCollider2D component2 = go.GetComponent<BoxCollider2D>();
		if (component2 != null)
		{
			UpdateWidgetCollider(component2, considerInactive);
		}
	}

	public static void UpdateWidgetCollider(BoxCollider box, bool considerInactive)
	{
		if (!(box != null))
		{
			return;
		}
		GameObject gameObject = box.gameObject;
		UIWidget component = gameObject.GetComponent<UIWidget>();
		if (component != null)
		{
			Vector4 drawRegion = component.drawRegion;
			if (drawRegion.x != 0f || drawRegion.y != 0f || drawRegion.z != 1f || drawRegion.w != 1f)
			{
				Vector4 drawingDimensions = component.drawingDimensions;
				box.center = new Vector3((drawingDimensions.x + drawingDimensions.z) * 0.5f, (drawingDimensions.y + drawingDimensions.w) * 0.5f);
				box.size = new Vector3(drawingDimensions.z - drawingDimensions.x, drawingDimensions.w - drawingDimensions.y);
			}
			else
			{
				Vector3[] localCorners = component.localCorners;
				box.center = Vector3.Lerp(localCorners[0], localCorners[2], 0.5f);
				box.size = localCorners[2] - localCorners[0];
			}
		}
		else
		{
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(gameObject.transform, considerInactive);
			box.center = bounds.center;
			box.size = new Vector3(bounds.size.x, bounds.size.y, 0f);
		}
	}

	public static void UpdateWidgetCollider(BoxCollider2D box, bool considerInactive)
	{
		if (box != null)
		{
			GameObject gameObject = box.gameObject;
			UIWidget component = gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				Vector3[] localCorners = component.localCorners;
				box.offset = Vector3.Lerp(localCorners[0], localCorners[2], 0.5f);
				box.size = localCorners[2] - localCorners[0];
			}
			else
			{
				Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(gameObject.transform, considerInactive);
				box.offset = bounds.center;
				box.size = new Vector2(bounds.size.x, bounds.size.y);
			}
		}
	}

	public static string GetTypeName<T>()
	{
		string text = typeof(T).ToString();
		if (text.StartsWith("UI"))
		{
			text = text.Substring(2);
		}
		else if (text.StartsWith("UnityEngine."))
		{
			text = text.Substring(12);
		}
		return text;
	}

	public static void SetDirty(UnityEngine.Object obj)
	{
	}

	public static void CheckForPrefabStage(GameObject gameObject)
	{
	}

	public static GameObject AddChild(GameObject parent)
	{
		return AddChild(parent, undo: true);
	}

	public static GameObject AddChild(GameObject parent, bool undo)
	{
		GameObject gameObject = new GameObject();
		if (parent != null)
		{
			Transform transform = gameObject.transform;
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			gameObject.layer = parent.layer;
		}
		return gameObject;
	}

	public static GameObject AddChild(GameObject parent, GameObject prefab)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		if (gameObject != null && parent != null)
		{
			Transform transform = gameObject.transform;
			transform.parent = parent.transform;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			gameObject.layer = parent.layer;
		}
		return gameObject;
	}

	public static int CalculateNextDepth(GameObject go)
	{
		if ((bool)go)
		{
			int num = -1;
			UIWidget[] componentsInChildren = go.GetComponentsInChildren<UIWidget>();
			int i = 0;
			for (int num2 = componentsInChildren.Length; i < num2; i++)
			{
				num = Mathf.Max(num, componentsInChildren[i].depth);
			}
			return num + 1;
		}
		return 0;
	}

	public static void SetChildLayer(Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; i++)
		{
			Transform child = t.GetChild(i);
			child.gameObject.layer = layer;
			SetChildLayer(child, layer);
		}
	}

	public static T AddChild<T>(GameObject parent) where T : Component
	{
		GameObject gameObject = AddChild(parent);
		gameObject.name = GetTypeName<T>();
		return gameObject.AddComponent<T>();
	}

	public static T AddWidget<T>(GameObject go, int depth = int.MaxValue) where T : UIWidget
	{
		if (depth == int.MaxValue)
		{
			depth = CalculateNextDepth(go);
		}
		T val = AddChild<T>(go);
		val.width = 100;
		val.height = 100;
		val.depth = depth;
		return val;
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		T component = go.GetComponent<T>();
		if (component == null)
		{
			Transform parent = go.transform.parent;
			while (parent != null && component == null)
			{
				component = parent.gameObject.GetComponent<T>();
				parent = parent.parent;
			}
		}
		return component;
	}

	public static T FindInParents<T>(Transform trans) where T : Component
	{
		if (trans == null)
		{
			return null;
		}
		return trans.GetComponentInParent<T>();
	}

	public static void Destroy(UnityEngine.Object obj)
	{
		if (!obj)
		{
			return;
		}
		if (obj is Transform)
		{
			Transform transform = obj as Transform;
			GameObject gameObject = transform.gameObject;
			if (Application.isPlaying)
			{
				transform.parent = null;
				UnityEngine.Object.Destroy(gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
		else if (obj is GameObject)
		{
			GameObject gameObject2 = obj as GameObject;
			Transform transform2 = gameObject2.transform;
			if (Application.isPlaying)
			{
				transform2.parent = null;
				UnityEngine.Object.Destroy(gameObject2);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(gameObject2);
			}
		}
		else if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(obj);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	public static void DestroyChildren(this Transform t)
	{
		bool isPlaying = Application.isPlaying;
		while (t.childCount != 0)
		{
			Transform child = t.GetChild(0);
			if (isPlaying)
			{
				child.parent = null;
				UnityEngine.Object.Destroy(child.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(child.gameObject);
			}
		}
	}

	public static void DestroyImmediate(UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isEditor)
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
			else
			{
				UnityEngine.Object.Destroy(obj);
			}
		}
	}

	private static void Activate(Transform t, bool compatibilityMode)
	{
		SetActiveSelf(t.gameObject, state: true);
		if (!compatibilityMode)
		{
			return;
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			if (t.GetChild(i).gameObject.activeSelf)
			{
				return;
			}
		}
		int j = 0;
		for (int childCount2 = t.childCount; j < childCount2; j++)
		{
			Activate(t.GetChild(j), compatibilityMode: true);
		}
	}

	private static void Deactivate(Transform t)
	{
		SetActiveSelf(t.gameObject, state: false);
	}

	public static void SetActive(GameObject go, bool state)
	{
		SetActive(go, state, compatibilityMode: true);
	}

	public static void SetActive(GameObject go, bool state, bool compatibilityMode)
	{
		if ((bool)go)
		{
			if (state)
			{
				Activate(go.transform, compatibilityMode);
				CallCreatePanel(go.transform);
			}
			else
			{
				Deactivate(go.transform);
			}
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	private static void CallCreatePanel(Transform t)
	{
		UIWidget component = t.GetComponent<UIWidget>();
		if (component != null)
		{
			component.CreatePanel();
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			CallCreatePanel(t.GetChild(i));
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static bool GetActive(Behaviour mb)
	{
		if ((bool)mb && mb.enabled)
		{
			return mb.gameObject.activeInHierarchy;
		}
		return false;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static bool GetActive(GameObject go)
	{
		if ((bool)go)
		{
			return go.activeInHierarchy;
		}
		return false;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static void SetActiveSelf(GameObject go, bool state)
	{
		go.SetActive(state);
	}

	public static void SetLayer(GameObject go, int layer)
	{
		go.layer = layer;
		Transform transform = go.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			SetLayer(transform.GetChild(i).gameObject, layer);
		}
	}

	public static Vector3 Round(Vector3 v)
	{
		v.x = Mathf.Round(v.x);
		v.y = Mathf.Round(v.y);
		v.z = Mathf.Round(v.z);
		return v;
	}

	public static void MakePixelPerfect(Transform t)
	{
		UIWidget component = t.GetComponent<UIWidget>();
		if (component != null)
		{
			component.MakePixelPerfect();
		}
		if (t.GetComponent<UIAnchor>() == null && t.GetComponent<UIRoot>() == null)
		{
			t.localPosition = Round(t.localPosition);
			t.localScale = Round(t.localScale);
		}
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			MakePixelPerfect(t.GetChild(i));
		}
	}

	public static Color ApplyPMA(Color c)
	{
		if (c.a != 1f)
		{
			c.r *= c.a;
			c.g *= c.a;
			c.b *= c.a;
		}
		return c;
	}

	public static void MarkParentAsChanged(GameObject go)
	{
		UIRect[] componentsInChildren = go.GetComponentsInChildren<UIRect>();
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			componentsInChildren[i].ParentHasChanged();
		}
	}

	public static T AddMissingComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static Vector3[] GetSides(this Camera cam, Transform relativeTo)
	{
		return cam.GetSides(Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, 0.5f), relativeTo);
	}

	public static Vector3[] GetSides(this Camera cam, float depth, Transform relativeTo)
	{
		if (cam.orthographic)
		{
			float orthographicSize = cam.orthographicSize;
			float num = 0f - orthographicSize;
			float num2 = orthographicSize;
			float y = 0f - orthographicSize;
			float y2 = orthographicSize;
			Rect rect = cam.rect;
			Vector2 vector = screenSize;
			float num3 = vector.x / vector.y;
			num3 *= rect.width / rect.height;
			num *= num3;
			num2 *= num3;
			Transform transform = cam.transform;
			Quaternion rotation = transform.rotation;
			Vector3 position = transform.position;
			int num4 = Mathf.RoundToInt(vector.x);
			int num5 = Mathf.RoundToInt(vector.y);
			if ((num4 & 1) == 1)
			{
				position.x -= 1f / vector.x;
			}
			if ((num5 & 1) == 1)
			{
				position.y += 1f / vector.y;
			}
			mSides[0] = rotation * new Vector3(num, 0f, depth) + position;
			mSides[1] = rotation * new Vector3(0f, y2, depth) + position;
			mSides[2] = rotation * new Vector3(num2, 0f, depth) + position;
			mSides[3] = rotation * new Vector3(0f, y, depth) + position;
			if (num3 > 1.7777778f)
			{
				mSides[0] *= AspectCamera.SafeAreaRate;
				mSides[2] *= AspectCamera.SafeAreaRate;
			}
		}
		else
		{
			mSides[0] = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, depth));
			mSides[1] = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, depth));
			mSides[2] = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));
			mSides[3] = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, depth));
		}
		if (relativeTo != null)
		{
			for (int i = 0; i < 4; i++)
			{
				mSides[i] = relativeTo.InverseTransformPoint(mSides[i]);
			}
		}
		return mSides;
	}

	public static void Execute<T>(GameObject go, string funcName) where T : Component
	{
		T[] components = go.GetComponents<T>();
		foreach (T val in components)
		{
			MethodInfo method = val.GetType().GetMethod(funcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				method.Invoke(val, null);
			}
		}
	}

	public static void ExecuteAll<T>(GameObject root, string funcName) where T : Component
	{
		Execute<T>(root, funcName);
		Transform transform = root.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			ExecuteAll<T>(transform.GetChild(i).gameObject, funcName);
		}
	}
}
