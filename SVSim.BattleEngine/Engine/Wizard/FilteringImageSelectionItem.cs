using UnityEngine;

namespace Wizard;

public class FilteringImageSelectionItem : MonoBehaviour
{
	[SerializeField]
	protected UITexture _texture;

	[SerializeField]
	private UISprite _selectSprite;

	[SerializeField]
	private UILabel _newLabel;

	[SerializeField]
	private UISprite _bgSprite;

	[SerializeField]
	private UILabel[] _textLabels;

	[SerializeField]
	private TweenAlpha _selectTweenAlpha;

	[SerializeField]
	private TweenPosition _newTweenPos;

	[SerializeField]
	private TweenAlpha _newTweenAlpha;

	[SerializeField]
	private UISprite _favoriteIcon;

	public FilteringImageSelection.ItemData Data { get; private set; }

	public void SetItemData(FilteringImageSelection.ItemData data)
	{
		Data = data;
		SetBgSprite(data._isDisplaySprite);
		SetTextLabels(data._texts);
	}

	private void SetBgSprite(bool isDisplaySprite)
	{
		if (!(_bgSprite == null))
		{
			_bgSprite.gameObject.SetActive(isDisplaySprite);
		}
	}

	private void SetTextLabels(string[] texts)
	{
		if (_textLabels == null || _textLabels.Length == 0)
		{
			return;
		}
		if (texts != null)
		{
			for (int i = 0; i < _textLabels.Length; i++)
			{
				UILabel uILabel = _textLabels[i];
				if (i >= texts.Length || texts[i] == null)
				{
					uILabel.gameObject.SetActive(value: false);
					continue;
				}
				uILabel.gameObject.SetActive(value: true);
				uILabel.text = texts[i];
			}
		}
		else
		{
			for (int j = 0; j < _textLabels.Length; j++)
			{
				_textLabels[j].gameObject.SetActive(value: false);
			}
		}
	}

	public virtual void SetTexture(FilteringImageSelection.ItemData data)
	{
		if (!(_texture == null))
		{
			if (data.TextureSettingCustomize != null)
			{
				data.TextureSettingCustomize(_texture, data);
			}
			else
			{
				_texture.mainTexture = data.Texture;
			}
		}
	}

	public void SetActiveSelectMark(bool isActive)
	{
		if (!(_selectSprite == null))
		{
			if (_selectSprite.gameObject.activeSelf != isActive)
			{
				_selectSprite.gameObject.SetActive(isActive);
			}
			if (isActive)
			{
				_selectTweenAlpha.PlayPingPong(isIncreaseAlpha: false);
			}
		}
	}

	public void SetActiveNewMark(bool isActive)
	{
		if (!(_newLabel == null))
		{
			if (_newLabel.gameObject.activeSelf != isActive)
			{
				_newLabel.gameObject.SetActive(isActive);
			}
			if (isActive)
			{
				_newTweenPos.ResetToBeginning();
				_newTweenPos.PlayForward();
				_newTweenAlpha.ResetToBeginning();
				_newTweenAlpha.PlayForward();
			}
		}
	}

	public void SetVisible(bool isVisible)
	{
		_texture.enabled = isVisible;
		_selectSprite.enabled = isVisible;
		_newLabel.enabled = isVisible;
		if (_bgSprite != null)
		{
			_bgSprite.enabled = isVisible;
		}
	}

	public void SetFavorite(bool isFavorite)
	{
		if (_favoriteIcon != null)
		{
			_favoriteIcon.enabled = isFavorite;
		}
	}
}
