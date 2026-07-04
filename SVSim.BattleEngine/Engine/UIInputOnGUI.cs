using System;
using UnityEngine;

[RequireComponent(typeof(UIInput))]
public class UIInputOnGUI : MonoBehaviour
{

	[NonSerialized]
	private UIInput mInput;

	private void Awake()
	{
		mInput = GetComponent<UIInput>();
	}
}
