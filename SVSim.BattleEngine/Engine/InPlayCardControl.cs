using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard.Battle.View;

public class InPlayCardControl
{
	private readonly GameObject m_gameObject;

	private Vector3[] cPos;

	public Transform transform { get; private set; }

	public InPlayCardControl(GameObject gameObject)
	{
		m_gameObject = gameObject;
		transform = m_gameObject.transform;
		cPos = new Vector3[5];
	}

	public static Vector3 CalcPosition(int cardCount, int index, bool isPlayer)
	{
		if (isPlayer)
		{
			return new Vector3(215f * (float)index - 215f * (float)(cardCount - 1) / 2f, 0f, 0f);
		}
		return new Vector3(-215f * (float)index + 215f * (float)(cardCount - 1) / 2f, 0f, 0f);
	}
}
