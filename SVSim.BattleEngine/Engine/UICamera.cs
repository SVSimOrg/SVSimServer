using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Event System (UICamera)")]
[RequireComponent(typeof(Camera))]
public class UICamera : MonoBehaviour
{
	public enum ControlScheme
	{
		Mouse,
		Touch,
		Controller
	}

	public enum ClickNotification
	{
		None,
		Always	}

	public class MouseOrTouch
	{

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 totalDelta;

		public GameObject last;

		public GameObject current;

		public GameObject pressed;

		public GameObject dragged;

		public ClickNotification clickNotification = ClickNotification.Always;

		public bool dragStarted;
	}

	public enum EventType
	{
}

	public delegate bool GetKeyStateFunc(KeyCode key);

	public delegate float GetAxisFunc(string name);

	public delegate bool GetAnyKeyFunc();

	public delegate void OnScreenResize();

	public delegate void OnCustomInput();

	public delegate void OnSchemeChange();

	public delegate void MoveDelegate(Vector2 delta);

	public delegate void VoidDelegate(GameObject go);

	public delegate void BoolDelegate(GameObject go, bool state);

	public delegate void FloatDelegate(GameObject go, float delta);

	public delegate void VectorDelegate(GameObject go, Vector2 delta);

	public delegate void ObjectDelegate(GameObject go, GameObject obj);

	public delegate void KeyCodeDelegate(GameObject go, KeyCode key);

	public struct DepthEntry
	{

		public Vector3 point;

		public GameObject go;
	}

	public class Touch
	{

		public Vector2 position;
	}

	public delegate int GetTouchCountCallback();

	public delegate Touch GetTouchCallback(int index);

	public static BetterList<UICamera> list = new BetterList<UICamera>();

	public static GetKeyStateFunc GetKeyDown = Input.GetKeyDown;

	public static GetKeyStateFunc GetKeyUp = Input.GetKeyUp;

	public static GetKeyStateFunc GetKey = Input.GetKey;

	public static OnScreenResize onScreenResize;

	public bool useMouse = true;

	public bool useTouch = true;

	public bool useKeyboard = true;

	public bool useController = true;

	public KeyCode submitKey0 = KeyCode.Return;

	public KeyCode submitKey1 = KeyCode.JoystickButton0;

	public KeyCode cancelKey0 = KeyCode.Escape;

	public KeyCode cancelKey1 = KeyCode.JoystickButton1;

	private static Vector2 mLastPos = Vector2.zero;

	public static Vector3 lastWorldPosition = Vector3.zero;

	public static UICamera current = null;

	public static Camera currentCamera = null;

	public static OnSchemeChange onSchemeChange;

	private static ControlScheme mLastScheme = ControlScheme.Mouse;

	public static int currentTouchID = -100;

	private static KeyCode mCurrentKey = KeyCode.Alpha0;

	public static MouseOrTouch currentTouch = null;

	private static bool mInputFocus = false;

	private static GameObject mGenericHandler;

	public static BoolDelegate onHover;

	public static BoolDelegate onSelect;

	public static BoolDelegate onTooltip;

	private static MouseOrTouch[] mMouse = new MouseOrTouch[3]
	{
		new MouseOrTouch(),
		new MouseOrTouch(),
		new MouseOrTouch()
	};

	public static MouseOrTouch controller = new MouseOrTouch();

	public static List<MouseOrTouch> activeTouches = new List<MouseOrTouch>();

	private static int mWidth = 0;

	private static int mHeight = 0;

	private static GameObject mTooltip = null;

	private Camera mCam;

	private static float mTooltipTime = 0f;

	private static GameObject mHover;

	private static GameObject mSelected;

	private static BetterList<DepthEntry> mHits = new BetterList<DepthEntry>();

	private static int mNotifying = 0;

	public static Vector2 lastEventPosition
	{
		get
		{
			if (currentScheme == ControlScheme.Controller)
			{
				GameObject gameObject = hoveredObject;
				if (gameObject != null)
				{
					Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(gameObject.transform);
					return NGUITools.FindCameraForLayer(gameObject.layer).WorldToScreenPoint(bounds.center);
				}
			}
			return mLastPos;
		}
		set
		{
			mLastPos = value;
		}
	}

	public static ControlScheme currentScheme
	{
		get
		{
			if (mCurrentKey == KeyCode.None)
			{
				return ControlScheme.Touch;
			}
			if (mCurrentKey >= KeyCode.JoystickButton0)
			{
				return ControlScheme.Controller;
			}
			if (current != null && mLastScheme == ControlScheme.Controller && (mCurrentKey == current.submitKey0 || mCurrentKey == current.submitKey1))
			{
				return ControlScheme.Controller;
			}
			return ControlScheme.Mouse;
		}
		set
		{
			switch (value)
			{
			case ControlScheme.Mouse:
				currentKey = KeyCode.Mouse0;
				break;
			case ControlScheme.Controller:
				currentKey = KeyCode.JoystickButton0;
				break;
			case ControlScheme.Touch:
				currentKey = KeyCode.None;
				break;
			default:
				currentKey = KeyCode.Alpha0;
				break;
			}
			mLastScheme = value;
		}
	}

	public static KeyCode currentKey
	{
		get
		{
			return mCurrentKey;
		}
		set
		{
			if (mCurrentKey == value)
			{
				return;
			}
			ControlScheme num = mLastScheme;
			mCurrentKey = value;
			mLastScheme = currentScheme;
			if (num != mLastScheme)
			{
				HideTooltip();
				if (mLastScheme == ControlScheme.Mouse)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				if (onSchemeChange != null)
				{
					onSchemeChange();
				}
			}
		}
	}

	public static Ray currentRay
	{
		get
		{
			if (!(currentCamera != null) || currentTouch == null)
			{
				return default(Ray);
			}
			return currentCamera.ScreenPointToRay(currentTouch.pos);
		}
	}

	public Camera cachedCamera
	{
		get
		{
			if (mCam == null)
			{
				mCam = GetComponent<Camera>();
			}
			return mCam;
		}
	}

	public static GameObject hoveredObject
	{
		get
		{
			if (currentTouch != null && currentTouch.dragStarted)
			{
				return currentTouch.current;
			}
			if ((bool)mHover && mHover.activeInHierarchy)
			{
				return mHover;
			}
			mHover = null;
			return null;
		}
		set
		{
			if (mHover == value)
			{
				return;
			}
			bool flag = false;
			UICamera uICamera = current;
			if (currentTouch == null)
			{
				flag = true;
				currentTouchID = -100;
				currentTouch = controller;
			}
			ShowTooltip(null);
			if ((bool)mSelected && currentScheme == ControlScheme.Controller)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
				mSelected = null;
			}
			if ((bool)mHover)
			{
				Notify(mHover, "OnHover", false);
				if (onHover != null)
				{
					onHover(mHover, state: false);
				}
			}
			mHover = value;
			currentTouch.clickNotification = ClickNotification.None;
			if ((bool)mHover)
			{
				if (mHover != controller.current && mHover.GetComponent<UIKeyNavigation>() != null)
				{
					controller.current = mHover;
				}
				if (flag)
				{
					UICamera uICamera2 = ((mHover != null) ? FindCameraForLayer(mHover.layer) : list[0]);
					if (uICamera2 != null)
					{
						current = uICamera2;
						currentCamera = uICamera2.cachedCamera;
					}
				}
				if (onHover != null)
				{
					onHover(mHover, state: true);
				}
				Notify(mHover, "OnHover", true);
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((uICamera != null) ? uICamera.cachedCamera : null);
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	public static GameObject selectedObject
	{
		get
		{
			if ((bool)mSelected && mSelected.activeInHierarchy)
			{
				return mSelected;
			}
			mSelected = null;
			return null;
		}
		set
		{
			if (mSelected == value)
			{
				hoveredObject = value;
				controller.current = value;
				return;
			}
			ShowTooltip(null);
			bool flag = false;
			UICamera uICamera = current;
			if (currentTouch == null)
			{
				flag = true;
				currentTouchID = -100;
				currentTouch = controller;
			}
			mInputFocus = false;
			if ((bool)mSelected)
			{
				Notify(mSelected, "OnSelect", false);
				if (onSelect != null)
				{
					onSelect(mSelected, state: false);
				}
			}
			mSelected = value;
			currentTouch.clickNotification = ClickNotification.None;
			if (value != null && value.GetComponent<UIKeyNavigation>() != null)
			{
				controller.current = value;
			}
			if ((bool)mSelected && flag)
			{
				UICamera uICamera2 = ((mSelected != null) ? FindCameraForLayer(mSelected.layer) : list[0]);
				if (uICamera2 != null)
				{
					current = uICamera2;
					currentCamera = uICamera2.cachedCamera;
				}
			}
			if ((bool)mSelected)
			{
				mInputFocus = mSelected.activeInHierarchy && mSelected.GetComponent<UIInput>() != null;
				if (onSelect != null)
				{
					onSelect(mSelected, state: true);
				}
				Notify(mSelected, "OnSelect", true);
			}
			if (flag)
			{
				current = uICamera;
				currentCamera = ((uICamera != null) ? uICamera.cachedCamera : null);
				currentTouch = null;
				currentTouchID = -100;
			}
		}
	}

	public static bool IsPressed(GameObject go)
	{
		for (int i = 0; i < 3; i++)
		{
			if (mMouse[i].pressed == go)
			{
				return true;
			}
		}
		int j = 0;
		for (int count = activeTouches.Count; j < count; j++)
		{
			if (activeTouches[j].pressed == go)
			{
				return true;
			}
		}
		if (controller.pressed == go)
		{
			return true;
		}
		return false;
	}

	public static BetterList<DepthEntry> GetHitsList()
	{
		return mHits;
	}

	public static bool IsHighlighted(GameObject go)
	{
		return hoveredObject == go;
	}

	public static UICamera FindCameraForLayer(int layer)
	{
		int num = 1 << layer;
		for (int i = 0; i < list.size; i++)
		{
			UICamera uICamera = list.buffer[i];
			Camera camera = uICamera.cachedCamera;
			if (camera != null && (camera.cullingMask & num) != 0)
			{
				return uICamera;
			}
		}
		return null;
	}

	public static void Notify(GameObject go, string funcName, object obj)
	{
		if (mNotifying > 10)
		{
			return;
		}
		if (currentScheme == ControlScheme.Controller && UIPopupList.isOpen && UIPopupList.current.source == go && UIPopupList.isOpen)
		{
			go = UIPopupList.current.gameObject;
		}
		if ((bool)go && go.activeInHierarchy)
		{
			mNotifying++;
			go.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			if (mGenericHandler != null && mGenericHandler != go)
			{
				mGenericHandler.SendMessage(funcName, obj, SendMessageOptions.DontRequireReceiver);
			}
			mNotifying--;
		}
	}

	private void Awake()
	{
		mWidth = Screen.width;
		mHeight = Screen.height;
		mMouse[0].pos = Input.mousePosition;
		for (int i = 1; i < 3; i++)
		{
			mMouse[i].pos = mMouse[0].pos;
			mMouse[i].lastPos = mMouse[0].pos;
		}
		mLastPos = mMouse[0].pos;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		if (commandLineArgs == null)
		{
			return;
		}
		for (int j = 0; j < commandLineArgs.Length; j++)
		{
			switch (commandLineArgs[j])
			{
			case "-noMouse":
				useMouse = false;
				break;
			case "-noTouch":
				useTouch = false;
				break;
			case "-noController":
				useController = false;
				break;
			case "-noJoystick":
				useController = false;
				break;
			case "-useMouse":
				useMouse = true;
				break;
			case "-useTouch":
				useTouch = true;
				break;
			case "-useController":
				useController = true;
				break;
			case "-useJoystick":
				useController = true;
				break;
			}
		}
	}

	public static bool ShowTooltip(GameObject go)
	{
		if (mTooltip != go)
		{
			if (mTooltip != null)
			{
				if (onTooltip != null)
				{
					onTooltip(mTooltip, state: false);
				}
				Notify(mTooltip, "OnTooltip", false);
			}
			mTooltip = go;
			mTooltipTime = 0f;
			if (mTooltip != null)
			{
				if (onTooltip != null)
				{
					onTooltip(mTooltip, state: true);
				}
				Notify(mTooltip, "OnTooltip", true);
			}
			return true;
		}
		return false;
	}

	public static bool HideTooltip()
	{
		return ShowTooltip(null);
	}
}
