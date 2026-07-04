using System;
using System.Collections.Generic;
using UnityEngine;

public class NguiObjs : MonoBehaviour
{
	[SerializeField]
	public List<GameObject> objs;

	[SerializeField]
	public UITexture[] textures;

	[SerializeField]
	public UILabel[] labels;

	[SerializeField]
	public UIButton[] buttons;

	[SerializeField]
	public TweenAlpha tweenAlpha;
}
