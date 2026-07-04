using System;
using Cute;
using UnityEngine;

namespace Wizard;

public class SelectRandomSkinButton : MonoBehaviour
{
	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private UIButton _button;

	private bool _isSelectStatus;

	public void Initialize(int skinId, bool isSelect, Action<int, bool> onClick, Action<GameObject> onDragStart, Action<GameObject, Vector2> onDrag)
	{
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(skinId.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaButton, isfetch: true);
		_texture.mainTexture = Toolbox.ResourcesManager.LoadObject(assetTypePath) as Texture;
		SetSelectStatus(isSelect);
		UIEventListener.Get(_button.gameObject).onClick = delegate
		{
			bool flag = !_isSelectStatus;

			SetSelectStatus(flag);
			onClick(skinId, flag);
		};
		UIEventListener.Get(_button.gameObject).onDragStart = delegate(GameObject g)
		{
			onDragStart(g);
		};
		UIEventListener.Get(_button.gameObject).onDrag = delegate(GameObject g, Vector2 d)
		{
			onDrag(g, d);
		};
	}

	public void SetSelectStatus(bool isSelect)
	{
		_isSelectStatus = isSelect;
		SetButtonGray(!isSelect);
	}

	private void SetButtonGray(bool isGray)
	{
		Color color = (isGray ? LabelDefine.TEXT_COLOR_BUTTON_DISABLE : ((Color32)Color.white));
		_texture.color = color;
		_button.hover = color;
		_button.pressed = color;
		_button.defaultColor = color;
		_button.disabledColor = color;
		_button.UpdateColor(instant: true);
	}
}
