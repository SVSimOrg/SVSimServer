using System;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;

public class CardDetailBase : MonoBehaviour
{
	[Serializable]
	public class BgSizeInfo
	{
	}

	[Serializable]
	public class DetailPanelInfo
	{
		public GameObject _root;

		public UILabel _nameLabel;

		public UITexture _logImage;

		public UIScrollView _scrollView;

		public UISprite _bg;

		public int _maxLine;

		public UIPanel DiscPanelObject;

		public UILabel DiscLabel;

		public UIScrollBar DiscScrollBar;

		public int DefaultBgHeight;

		public UILabel _costLabel;

		public UILabel _classLabel;

		public UISprite _classBG;

		public UIRect RootAnchor;
	}

	[SerializeField]
	protected DetailPanelInfo _followerPanel;

	[SerializeField]
	protected DetailPanelInfo _followerEvoPanel;

	[SerializeField]
	protected DetailPanelInfo _nonFollowerPanel;

	protected void SetFollowerDetailLabel(string skillDisc, string evoSkillDisc, bool needEvolutionOrFusionButton, bool resetScrollPosition = true)
	{
		_nonFollowerPanel._root.SetActive(value: false);
		_followerPanel._root.SetActive(value: true);
		_followerEvoPanel._root.SetActive(value: true);
		int num = CheckTextLineCount(_followerPanel.DiscLabel, skillDisc);
		int num2 = CheckTextLineCount(_followerEvoPanel.DiscLabel, evoSkillDisc);
		if (num >= _followerPanel._maxLine && num2 < _followerEvoPanel._maxLine)
		{
			num = Mathf.Min(9 - num2, num);
		}
		else if (num < _followerPanel._maxLine && num2 >= _followerEvoPanel._maxLine)
		{
			num2 = Mathf.Min(9 - num, num2);
		}
		else if (num >= _followerPanel._maxLine && num2 >= _followerEvoPanel._maxLine)
		{
			num = _followerPanel._maxLine;
			num2 = _followerEvoPanel._maxLine;
		}
		SetDescLabelText(_followerPanel, skillDisc, num, needEvolutionOrFusionButton: false, resetScrollPosition);
		SetDescLabelText(_followerEvoPanel, evoSkillDisc, num2, needEvolutionOrFusionButton, resetScrollPosition);
		_followerEvoPanel.RootAnchor.UpdateAnchors();
	}

	protected void SetDescLabelText(DetailPanelInfo panel, string discText, bool needEvolutionOrFusionButton = false, bool resetScrollPosition = true, bool isClass = false)
	{
		SetDescLabelText(panel, discText, panel._maxLine, needEvolutionOrFusionButton, resetScrollPosition, isClass);
	}

	protected void SetDescLabelText(DetailPanelInfo panel, string discText, int maxLine, bool needEvolutionOrFusionButton = false, bool resetScrollPosition = true, bool isClass = false)
	{
		UILabel discLabel = panel.DiscLabel;
		UIScrollView scrollView = panel._scrollView;
		UISprite bg = panel._bg;
		discLabel.text = Global.GetConvertWrapText(discLabel, discText);
		discLabel.ProcessText();
		int textLineCount = Global.GetTextLineCount(discLabel.processedText);
		bool flag = textLineCount > maxLine;
		if (bg != null)
		{
			bg.height = panel.DefaultBgHeight + Mathf.Min(textLineCount, maxLine) * (panel.DiscLabel.fontSize + panel.DiscLabel.spacingY);
			if (needEvolutionOrFusionButton)
			{
				bg.height += 65;
			}
			if (isClass)
			{
				bg.height = 89;
			}
			bg.gameObject.SetActive(value: false);
			bg.gameObject.SetActive(value: true);
			bg.ResetAndUpdateAnchors();
		}
		panel.DiscPanelObject.bottomAnchor.absolute = (flag ? 16 : 4) + (needEvolutionOrFusionButton ? 65 : 0);
		panel.DiscPanelObject.gameObject.SetActive(value: false);
		panel.DiscPanelObject.gameObject.SetActive(value: true);
		panel.DiscPanelObject.UpdateAnchors();
		scrollView.enabled = flag;
		panel.DiscScrollBar.gameObject.SetActive(flag);
		if (resetScrollPosition)
		{
			scrollView.ResetPosition();
		}
		scrollView.UpdateScrollbars();
	}

	private int CheckTextLineCount(UILabel label, string discText)
	{
		label.text = Global.GetConvertWrapText(label, discText);
		label.ProcessText();
		return Global.GetTextLineCount(label.processedText);
	}
}
