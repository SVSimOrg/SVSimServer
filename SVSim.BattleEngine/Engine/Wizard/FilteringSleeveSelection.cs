using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class FilteringSleeveSelection : FilteringImageSelection
{
	[Header("Objects")]
	[SerializeField]
	private UITexture _zoomTexture;

	[SerializeField]
	private UIPanel _zoomPanel;

	protected override FavoriteTask.Kind TaskKind => FavoriteTask.Kind.SLEEVE;

	protected override string SelectionButtonTextId => "Profile_0046";

	public override void Initialize(int itemMax, int seriesMax)
	{
		base.Initialize(itemMax, seriesMax);
		EventDelegate.Add(_selectedItemTexture.gameObject.AddMissingComponent<UIButton>().onClick, OpenZoom);
		EventDelegate.Add(_zoomTexture.gameObject.AddMissingComponent<UIButton>().onClick, CloseZoom);
	}

	private void OpenZoom()
	{

		_zoomPanel.gameObject.SetActive(value: true);
		iTween.Stop(_zoomTexture.gameObject);
		_zoomTexture.transform.localScale = Vector3.zero;
		iTween.ScaleTo(_zoomTexture.gameObject, iTween.Hash("islocal", true, "scale", Vector3.one, "time", 0.3f, "easetype", iTween.EaseType.easeOutExpo));
	}

	private void CloseZoom()
	{
		iTween.Stop(_zoomTexture.gameObject);

		_zoomPanel.gameObject.SetActive(value: false);
	}

	protected override void SetSelectedDisplay(ItemData data)
	{
		if (_selectedItemTexture != null && long.TryParse(data._key, out var result))
		{
			result = Toolbox.ResourcesManager.GetExistingSleeveId(result);
			_uiManager.getUIBase_CardManager().SetSleeveTexture(_zoomTexture, result);
			_uiManager.getUIBase_CardManager().SetSleeveTexture(_selectedItemTexture, result);
			_selectedItemTexture.enabled = true;
		}
		if (_selectedItemNameLabel != null)
		{
			_selectedItemNameLabel.SetWrapText(data._name);
		}
		UpdateToggleFavoriteButton(data.IsFavorite);
	}

	protected override void UpdateFavoriteFlag(IEnumerable<long> added, IEnumerable<long> removed)
	{
		foreach (long item in added)
		{
			Data.Master.SleeveMgr.SetFavorite(item, b: true);
		}
		foreach (long item2 in removed)
		{
			Data.Master.SleeveMgr.SetFavorite(item2, b: false);
		}
	}

	protected override IEnumerable<long> GetFavorites()
	{
		return Data.Master.SleeveMgr.GetFavorites();
	}
}
