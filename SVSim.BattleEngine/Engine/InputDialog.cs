using System;
using Cute;
using UnityEngine;

public class InputDialog : MonoBehaviour
{
	[SerializeField]
	private UIToggle _toggle;

	private bool _enableToggleSound = true;

	private bool _isFirstOnChange = true;

	public Action OnChangeToggleEvent;

	public static DialogBase Create(int charalimitinput, int charalimitfix, UIInput.KeyboardType keyboard = UIInput.KeyboardType.Default)
	{
		NguiObjs textInputDialogPrefab = UIManager.GetInstance().TextInputDialogPrefab;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		GameObject gameObject = NGUITools.AddChild(dialogBase.gameObject, textInputDialogPrefab.gameObject);
		dialogBase.InputAreaObjs = gameObject.GetComponent<NguiObjs>();
		dialogBase.InputAreaObjs.transform.localPosition = textInputDialogPrefab.transform.localPosition;
		dialogBase.InputDialog = gameObject.GetComponent<InputDialog>();
		UIInputWizard[] componentsInChildren = dialogBase.InputAreaObjs.GetComponentsInChildren<UIInputWizard>(includeInactive: true);
		if (componentsInChildren != null)
		{
			EventDelegate item = new EventDelegate(delegate
			{
				TextInputLimitCheck(charalimitfix);
			});
			foreach (UIInputWizard obj in componentsInChildren)
			{
				obj.keyboardType = keyboard;
				obj.characterLimit = charalimitinput;
				obj.onSubmit.Add(item);
				obj.onDeselect.Add(item);
			}
		}
		return dialogBase;
	}

	public static void TextInputLimitCheck(int charalimitfix)
	{
		if (charalimitfix > 0 && !(null == UIInput.current) && charalimitfix < UIInput.current.value.Length)
		{
			UIInput.current.value = UIInput.current.value.Substring(0, charalimitfix);
			UIInput.current.label.text = UIInput.current.value;
		}
	}

	private void Awake()
	{
		_toggle.onChange.Add(new EventDelegate(delegate
		{
			OnClickToggle();
		}));
		_toggle.gameObject.SetActive(value: false);
	}

	private void OnClickToggle()
	{
		if (_enableToggleSound && !_isFirstOnChange)
		{

		}
		_isFirstOnChange = false;
		OnChangeToggleEvent.Call();
	}
}
