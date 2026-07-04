using System.Collections.Generic;
using UnityEngine;

public class CardSelectListUI_Positioning : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> m_objList = new List<GameObject>();

	[SerializeField]
	private Vector3 m_offset = Vector3.zero;

	[SerializeField]
	private Vector3 m_lineDirection = Vector3.down;

	[SerializeField]
	private float m_lineWidth = 170f;

	[SerializeField]
	private Vector3 m_breakDirection = Vector3.right;

	[SerializeField]
	private float m_breakHeight = 130f;

	[SerializeField]
	private float m_breakNum = 2f;

	[SerializeField]
	private float m_speed = 0.2f;

	private float m_prog;

	public Vector3 Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
			StartAnim();
		}
	}

	public float BreakWidth
	{
		get
		{
			return m_breakHeight;
		}
		set
		{
			m_breakHeight = value;
			StartAnim();
		}
	}

	public float Speed
	{
		get
		{
			return m_speed;
		}
		set
		{
			m_speed = value;
			StartAnim();
		}
	}

	public bool IsMoving => m_prog < 0.95f;

	public void Clear()
	{
		m_objList.Clear();
	}

	public void Add(GameObject[] addList)
	{
		int num = m_objList.Count;
		if (num == 0)
		{
			m_objList.AddRange(addList);
		}
		else
		{
			int num2 = addList.Length;
			int i = 0;
			int num3 = 0;
			while (num3 < num && i < num2)
			{
				int num4 = 0;
				while (i < num2 && m_objList[num3] != addList[i])
				{
					m_objList.Insert(num3, addList[i]);
				}
				i++;
				num3 += num4;
				num += num4;
				num3++;
			}
			for (; i < num2; i++)
			{
				m_objList.Add(addList[i]);
			}
		}
		StartAnim();
	}

	public void Change(GameObject[] addList)
	{
		m_objList.Clear();
		m_objList.AddRange(addList);
		StartAnim();
	}

	public void Immediate()
	{
		float speed = m_speed;
		m_speed = 1f;
		Update_Anim();
		m_speed = speed;
	}

	public Vector3 GetPositionAtIndex(int idx)
	{
		int num = idx % (int)m_breakNum;
		int num2 = idx / (int)m_breakNum;
		float num3 = m_lineWidth * (float)num;
		float num4 = m_breakHeight * (float)num2;
		return m_offset + m_lineDirection * num3 + m_breakDirection * num4;
	}

	private void Update_Anim()
	{
		m_prog += (1f - m_prog) * m_speed;
		int count = m_objList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(m_objList[i] == null))
			{
				Vector3 localPosition = m_objList[i].transform.localPosition;
				Vector3 vector = (GetPositionAtIndex(i) - localPosition) * m_speed;
				m_objList[i].transform.localPosition = localPosition + vector;
			}
		}
	}

	private void StartAnim()
	{
		m_prog = 0f;
	}
}
