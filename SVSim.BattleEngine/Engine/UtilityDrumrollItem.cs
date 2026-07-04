using UnityEngine;
using Wizard;

[RequireComponent(typeof(UIDragScrollView), typeof(UICenterOnClick), typeof(UIEventListener))]
[RequireComponent(typeof(BoxCollider))]
public class UtilityDrumrollItem : MonoBehaviour
{
	[SerializeField]
	private UILabel _childLabel;

	[SerializeField]
	private UtilityDrumrollItemCustomize _customizeItem;

	private UIPanel _scrollView;

	private Vector2 _scrollSize = Vector2.zero;

	public int _index { get; private set; }

	public UtilityDrumrollItemCustomize CustomizeItem => _customizeItem;

	public void Init(UIPanel scroll, int index, string text)
	{
		_index = index;
		_childLabel.text = text;
		_scrollView = scroll;
		_scrollSize = _scrollView.GetViewSize();
	}
}
