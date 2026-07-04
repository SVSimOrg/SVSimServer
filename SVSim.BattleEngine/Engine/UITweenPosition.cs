using UnityEngine;

public class UITweenPosition : MonoBehaviour
{
	public delegate void FinishCallBack(UITweenPosition in_FadeObject);

	[SerializeField]
	public AnimationCurve Curve;

	public Vector2 From;

	public Vector2 To;

	private Vector2 _fromStart;

	private Vector2 _toStart;

	public float DelayTime;

	public float EndTime;

	private UIPanel _getPanel;

	private UIWidget _getWidget;

	private UIRect _getRect;

	private bool _isEnd;

	private float _timer;

	public Vector2 Value { get; set; }

	public FinishCallBack OnFinishCallBack { get; set; }

	public bool IsPlay { get; protected set; }

	public bool IsPlayFoward { get; set; }

	private void Awake()
	{
		_getPanel = base.gameObject.GetComponent<UIPanel>();
		if (_getPanel != null)
		{
			_getRect = _getPanel;
		}
		else
		{
			_getWidget = base.gameObject.GetComponent<UIWidget>();
			if (_getWidget == null && _getPanel == null)
			{
				_getWidget = base.gameObject.AddComponent<UIWidget>();
			}
			_getRect = _getWidget;
		}
		IsPlay = false;
		Curve.postWrapMode = WrapMode.Once;
		Curve.preWrapMode = WrapMode.Once;
	}

	private void Update()
	{
		if (IsPlay)
		{
			_timer += Time.deltaTime;
			if (_timer >= DelayTime)
			{
				float b = (_timer - DelayTime) / (Curve.keys[Curve.length - 1].time * EndTime);
				b = Mathf.Min(Curve.keys[Curve.length - 1].time, b);
				float x;
				float y;
				if (IsPlayFoward)
				{
					x = _fromStart.x + (To.x - _fromStart.x) * Curve.Evaluate(b);
					y = _fromStart.y + (To.y - _fromStart.y) * Curve.Evaluate(b);
				}
				else
				{
					x = From.x + (_toStart.x - From.x) * Curve.Evaluate(Curve.keys[Curve.length - 1].time - b);
					y = From.y + (_toStart.y - From.y) * Curve.Evaluate(Curve.keys[Curve.length - 1].time - b);
				}
				Value = new Vector2(x, y);
				base.gameObject.transform.localPosition = Value;
				if (b >= Curve.keys[Curve.length - 1].time)
				{
					if (IsPlayFoward)
					{
						base.gameObject.transform.localPosition = To;
					}
					else
					{
						base.gameObject.transform.localPosition = From;
					}
					IsPlay = false;
					_isEnd = true;
					_getRect.gameObject.SetActive(value: false);
					_getRect.gameObject.SetActive(value: true);
				}
			}
		}
		if (_isEnd)
		{
			if (OnFinishCallBack != null)
			{
				OnFinishCallBack(this);
			}
			_isEnd = false;
		}
	}

	public void PlayForward(bool resetFlag = false)
	{
		IsPlayFoward = true;
		if (resetFlag)
		{
			_fromStart = From;
		}
		else
		{
			_fromStart = new Vector2(base.transform.localPosition.x, base.transform.localPosition.y);
		}
		if (base.gameObject.transform.localPosition.x != To.x || base.gameObject.transform.localPosition.y != To.y || resetFlag)
		{
			IsPlay = true;
			_timer = 0f;
			Update();
		}
		else
		{
			_isEnd = true;
		}
	}
}
