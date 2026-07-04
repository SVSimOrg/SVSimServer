using UnityEngine;

public class UIScrollBarWrapContent : UIScrollBar
{

	private bool m_bDragNow;

	public UIWrapContent m_WrapContents { get; set; }

	protected override void Update()
	{
		if (!m_bDragNow || !m_WrapContents)
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			if (m_WrapContents.WrapContent())
			{
				break;
			}
		}
	}

	protected override void OnPressForeground(GameObject go, bool isPressed)
	{
		base.OnPressForeground(go, isPressed);
		m_bDragNow = isPressed;
	}

	protected override void OnPressBackground(GameObject go, bool isPressed)
	{
		base.OnPressBackground(go, isPressed);
		m_bDragNow = isPressed;
	}
}
