using System;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.RoomMatch;

public class TopBar : MonoBehaviour
{

	[SerializeField]
	public GameObject TitleObject;

	[SerializeField]
	public GameObject NameWindowObject;

	[SerializeField]
	public UIButton BuyCrystalButton;

	[SerializeField]
	public UILabel NameLabel;

	[SerializeField]
	public UIButton BackButton;

	[SerializeField]
	private UILabelGradientOverwriter _backLabelGradientOverwriter;

	[SerializeField]
	private UILabel BackButtonTitleLabel;

	[SerializeField]
	private UISprite _backButtonTitleLabelBG;

	private Vector3 _firstPositionBack;

	private Vector3 _firstPositionTitle;

	private Vector3 _firstPositionName;

	private bool _isWideMode;

	public void SetBackButtonEnable(bool enable)
	{
		// Pre-Phase-5b: also flipped GameMgr's InputMgr back-key flag. Headless has no
		// InputMgr; the button toggle below is the only observable effect.
		BackButton.enabled = enable;
		UIManager.SetObjectToGrey(BackButton.gameObject, !enable);
	}

	private void Awake()
	{
		TitleObject.SetActive(value: true);
		NameWindowObject.SetActive(value: false);
		_firstPositionBack = BackButton.transform.localPosition;
		_firstPositionTitle = TitleObject.transform.localPosition;
		_firstPositionName = NameWindowObject.transform.localPosition;
	}

	public void SetTitleLabel(string text, bool isWideMode)
	{
		_isWideMode = isWideMode;
		SetTitleLabel(text);
	}

	public void SetTitleLabel(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int length = text.Length;
		int num = 16;
		int num2 = 15;
		if (CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Jpn.ToString() || CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Cht.ToString() || CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Chs.ToString() || CustomPreference.GetTextLanguage() == Global.LANG_TYPE.Kor.ToString())
		{
			num = 9;
			num2 = 30;
		}
		int num3 = 294;
		int num4 = (_isWideMode ? 543 : 504);
		if (length > num)
		{
			int num5 = length - num;
			num3 += num5 * num2;
			if (num3 > num4)
			{
				num3 = num4;
			}
		}
		BackButtonTitleLabel.width = num3 + -30;
		_backButtonTitleLabelBG.width = num3;
		BackButtonTitleLabel.text = text;
	}

	public void SetTitleLabelWidth(int width)
	{
		BackButtonTitleLabel.width = width + -30;
		_backButtonTitleLabelBG.width = width;
	}

	public void OverwriteBackLabelColors(eColorCodeId gradientTopColorId, eColorCodeId gradientBottomColorId)
	{
		OverwriteBackLabelGradient(gradientTopColorId, gradientBottomColorId);
	}

	private void OverwriteBackLabelGradient(eColorCodeId topColorId, eColorCodeId bottomColorId)
	{
		_backLabelGradientOverwriter.enabled = true;
		_backLabelGradientOverwriter.GradientTopColorId = topColorId;
		_backLabelGradientOverwriter.GradientBottomColorId = bottomColorId;
	}
}
