using UnityEngine;

namespace Wizard.Dialog.Setting;

public class ItemToggle : Item
{

	[SerializeField]
	private UILabel m_titleLabel;

	[SerializeField]
	private UILabel _subTextLabel;

	[SerializeField]
	private UILabel m_stateLabel;

	[SerializeField]
	private UIToggle m_toggle;

	[SerializeField]
	private GameObject m_separatorLineObj;

	[SerializeField]
	private BoxCollider _collider;

	private string m_OnText;

	private string m_OffText;

	private bool m_isPlaySe;

	private void Awake()
	{
		m_titleLabel.text = string.Empty;
		if (m_stateLabel != null)
		{
			m_stateLabel.text = string.Empty;
		}
		if (m_separatorLineObj != null)
		{
			m_separatorLineObj.SetActive(value: false);
		}
		_collider = m_toggle.GetComponent<BoxCollider>();
		if (_subTextLabel != null)
		{
			_subTextLabel.gameObject.SetActive(value: false);
			_subTextLabel.text = string.Empty;
		}
		AddChangeCallback(delegate
		{
			UpdateStateLabel();
			if (m_isPlaySe)
			{

			}
			else
			{
				m_isPlaySe = true;
			}
		});
	}

	public void SetValue(bool value, bool isDisablePlayse = true)
	{
		m_toggle.value = value;
		if (isDisablePlayse)
		{
			m_isPlaySe = false;
		}
	}

	public bool GetValue()
	{
		return m_toggle.value;
	}

	public void SetTitleLabel(string title)
	{
		m_titleLabel.text = title;
	}

	private void UpdateStateLabel()
	{
		if (!string.IsNullOrEmpty(m_OnText) && !string.IsNullOrEmpty(m_OffText))
		{
			m_stateLabel.text = (m_toggle.value ? m_OnText : m_OffText);
		}
	}

	public void SetValidator(UIToggle.Validate validator)
	{
		m_toggle.validator = validator;
	}

	public override void AddChangeCallback(EventDelegate.Callback callback)
	{
		EventDelegate.Add(m_toggle.onChange, callback);
	}

	public override void SetActive_SeparatorLine(bool isActive)
	{
		m_separatorLineObj.SetActive(isActive);
	}
}
