using System.Collections.Generic;
using UnityEngine;
using Wizard;

public class InputMgr
{
	public enum TYPE
	{
		NONE,
		PRESS,
		DOWN,
		UP}

	public enum LAYER_TYPE
	{
		NONE = 0,
		UI = 32,
		GAME = 512,
		ALL = 65535
	}

	private enum ClickState
	{
		None,
		UpThisFrame,
		DownMoved
	}

	private enum DoubleClickState
	{
		None,
		SecondClickUp
	}

	private Vector2 m_NonePos = new Vector2(-10000f, -10000f);

	private IList<TYPE> m_NowTypeList = new List<TYPE>();

	private int m_LayerMask;

	private IList<Vector2> m_FirstPosList = new List<Vector2>();

	private IList<Vector2> m_NowPosList = new List<Vector2>();

	private IList<Vector2> m_FixedPosList = new List<Vector2>();

	private IList<Vector2> m_EndPosList = new List<Vector2>();

	private float m_DraggingSec;

	private bool m_IsFlick;

	private IList<GameObject> m_DragEfcList = new List<GameObject>();

	public BattleCamera m_BattleCamera;

	private bool _wentOverDrag;

	public static bool MouseControl;

	private DoubleClickState _doubleClick;

	private ClickState _clickState;

	public bool WentOverDrag
	{
		get
		{
			return _wentOverDrag;
		}
		set
		{
			_wentOverDrag = value;
		}
	}

	public static bool ShowDetailLeftAndRight => MouseControl;

	public InputMgr()
	{
		m_LayerMask = 0;
		for (int i = 0; i < 1; i++)
		{
			m_NowTypeList.Add(TYPE.NONE);
			m_FirstPosList.Add(m_NonePos);
			m_NowPosList.Add(m_NonePos);
			m_EndPosList.Add(m_NonePos);
			m_FixedPosList.Add(m_NonePos);
			m_DragEfcList.Add(null);
		}
		m_DraggingSec = 0f;
		m_IsFlick = false;
		Input.multiTouchEnabled = false;
		MouseControl = PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.MOUSE_CONTROL);
	}

	public void SetBattleCamera(BattleCamera battleCamera)
	{
		m_BattleCamera = battleCamera;
	}

	public bool IsNone()
	{
		if (m_NowTypeList[0] == TYPE.NONE)
		{
			return true;
		}
		return false;
	}

	public bool IsPress()
	{
		if (m_NowTypeList[0] == TYPE.PRESS)
		{
			return true;
		}
		return false;
	}

	public bool IsDown()
	{
		if (m_NowTypeList[0] == TYPE.DOWN)
		{
			return true;
		}
		return false;
	}

	public bool IsUp()
	{
		if (m_NowTypeList[0] == TYPE.UP)
		{
			return true;
		}
		return false;
	}

	public Vector2 GetFirstPos()
	{
		return m_FirstPosList[0];
	}

	public Vector2 GetPos()
	{
		return m_NowPosList[0];
	}

	public bool IsDoubleClick()
	{
		return _doubleClick == DoubleClickState.SecondClickUp;
	}

	public bool IsClick()
	{
		return _clickState == ClickState.UpThisFrame;
	}

	public bool IsDownMoved()
	{
		return _clickState == ClickState.DownMoved;
	}

	public bool IsOverDragDistanceMulligan()
	{
		return Vector2.Distance(GetPos(), GetFirstPos()) > 40f;
	}
}
