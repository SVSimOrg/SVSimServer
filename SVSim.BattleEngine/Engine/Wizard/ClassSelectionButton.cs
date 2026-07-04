using Cute;
using UnityEngine;

namespace Wizard;

public class ClassSelectionButton : MonoBehaviour
{
	public delegate void OnClickClassButton(ClassSelectionButton classSelectionButton);

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private UIButton _button;

	[SerializeField]
	private UILabel _storyClearLabel;

	[SerializeField]
	private UILabel _usedLabel;

	[SerializeField]
	private GameObject _notificationIcon;

	public ClassCharacterMasterData ClassCharacterMasterData { get; private set; }

	public void Init(ClassCharacterMasterData classCharacterMasterData, Texture texture, OnClickClassButton onClick, bool isShowStoryClearLabel, bool isShowUsedLabel, bool showNotificationIcon)
	{
		ClassCharacterMasterData = classCharacterMasterData;
		_texture.mainTexture = texture;
		UIEventListener.Get(_button.gameObject).onClick = delegate
		{
			onClick(this);
		};
		_storyClearLabel.gameObject.SetActive(isShowStoryClearLabel);
		_usedLabel.gameObject.SetActive(isShowUsedLabel);
		UIManager.SetObjectToGrey(_texture.gameObject, isShowUsedLabel);
		_notificationIcon.SetActive(showNotificationIcon);
	}

	public void InitEmpty()
	{
		_texture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath("empty", ResourcesManager.AssetLoadPathType.ClassCharaButton, isfetch: true));
		_button.isEnabled = false;
		_storyClearLabel.gameObject.SetActive(value: false);
		_usedLabel.gameObject.SetActive(value: false);
		UIManager.SetObjectToGrey(_texture.gameObject, b: false);
	}
}
