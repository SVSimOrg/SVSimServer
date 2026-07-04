using UnityEngine;

namespace Wizard;

public class UIGauge : MonoBehaviour
{
	[SerializeField]
	private UIProgressBar _progressBar;

	[SerializeField]
	private UISprite _barStartEdge;

	[SerializeField]
	private UISprite _barEndEdge;

	public float Value
	{
		get
		{
			return _progressBar.value;
		}
		set
		{
			UpdateBar(value);
		}
	}

	private void UpdateBar(float value)
	{
		if (value <= 0f)
		{
			_barStartEdge.gameObject.SetActive(value: false);
			_barEndEdge.gameObject.SetActive(value: false);
		}
		else if (value < 1f)
		{
			_barStartEdge.gameObject.SetActive(value: true);
			_barEndEdge.gameObject.SetActive(value: false);
		}
		else
		{
			_barStartEdge.gameObject.SetActive(value: true);
			_barEndEdge.gameObject.SetActive(value: true);
		}
		_progressBar.value = value;
	}
}
